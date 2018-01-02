﻿namespace ZeroMQ.lib
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;

	public static partial class Platform
	{
		public static class Posix
		{
			private const CallingConvention CCCdecl = CallingConvention.Cdecl;

			private const string LibraryName = "libdl";

			public static readonly string[] LibraryPaths = new string[] {
				"{Path}/{LibraryName}.so",
				"{Path}/{LibraryName}.so.*",
				"{DllPath}/{Arch}/{LibraryName}.so",
				"{DllPath}/{Arch}/{LibraryName}.so.*",
				"{AppBase}/{Arch}/{LibraryName}.so",
				"{AppBase}/{Arch}/{LibraryName}.so.*",
			};

			private const int RTLD_LAZY = 0x0001;
			private const int RTLD_NOW = 0x0002;
			private const int RTLD_GLOBAL = 0x0100;
			private const int RTLD_LOCAL = 0x0000;

			[DllImport(LibraryName, CallingConvention = CCCdecl)]
			private static extern SafeLibraryHandle dlopen(IntPtr filename, int flags);

			[DllImport(LibraryName, CallingConvention = CCCdecl)]
			private static extern int dlclose(IntPtr handle);

			[DllImport(LibraryName, CallingConvention = CCCdecl)]
			private static extern IntPtr dlerror();

			[DllImport(LibraryName, CallingConvention = CCCdecl)]
			private static extern IntPtr dlsym(SafeLibraryHandle handle, IntPtr symbol);

			/* [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
			internal static extern int syscall_chmod (IntPtr path, uint mode); */

			/* private static void syscall_chmod_execute(string libraryPath) 
			{
				IntPtr pathPtr = Marshal.StringToHGlobalAnsi(libraryPath);
				if (0 != syscall_chmod(pathPtr, (uint)( FilePermissions.ALLPERMS ))) {
					// error
				}
				Marshal.FreeHGlobal(pathPtr);
			} */

			public static UnmanagedLibrary LoadUnmanagedLibrary(string libraryName)
			{
				if (string.IsNullOrWhiteSpace(libraryName))
				{
					throw new ArgumentException("A valid library name is expected.", "libraryName");
				}

				// Now look: This method should ExpandPaths on LibraryPaths.
				// That being said, it should just enumerate
				// Path, AppBase, Arch, LibraryName, Extension

				// Secondly, this method should try each /lib/x86_64-linux-gnu/libload.so.2 to load,
				// Third, this method should try EmbeddedResources,
				// Finally, this method fails, telling the user all libraryPaths searched.

				var libraryPaths = new List<string>(Platform.LibraryPaths);

				Platform.ExpandPaths(libraryPaths, "{Path}", EnumerateLibLdConf("/etc/ld.so.conf"));

				var PATHs = new List<string>();
				PATHs.Add(EnsureNotEndingSlash(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
				PATHs.AddRange(EnumerateLibLdPATH());
				Platform.ExpandPaths(libraryPaths, "{DllPath}", PATHs.ToArray());

				Platform.ExpandPaths(libraryPaths, "{AppBase}", EnsureNotEndingSlash(
						AppDomain.CurrentDomain.BaseDirectory));

				Platform.ExpandPaths(libraryPaths, "{LibraryName}", libraryName);

				// Platform.ExpandPaths(libraryPaths, "{Ext}", Platform.LibraryFileExtension);

				string architecture;
				string[] architecturePaths = null;
				if (Platform.Architecture == ImageFileMachine.I386 && Environment.Is64BitProcess) 
				{
					architecture = "amd64";
				}
				else {
					architecture = Enum.GetName(typeof(ImageFileMachine), Platform.Architecture).ToLower();
				}
				if (architecture == "i386") architecturePaths = new string[] { "i386", "x86" };
				if (architecture == "amd64") architecturePaths = new string[] { "amd64", "x64" };
				if (architecturePaths == null) architecturePaths = new string[] { architecture };
				Platform.ExpandPaths(libraryPaths, "{Arch}", architecturePaths);

				// Now TRY the enumerated Directories for libFile.so.*

				string traceLabel = string.Format("UnmanagedLibrary[{0}]", libraryName);

				foreach (string libraryPath in libraryPaths)
				{

				    IEnumerable<string> files;
				    if (libraryPath.Contains("/"))
				    {

				        string folder = null;
				        string filesPattern = libraryPath;
				        int filesPatternI;
				        if (-1 < (filesPatternI = filesPattern.LastIndexOf('/')))
				        {
				            folder = filesPattern.Substring(0, filesPatternI + 1);
				            filesPattern = filesPattern.Substring(filesPatternI + 1);
				        }

				        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) continue;

				        files = Directory.EnumerateFiles(folder, filesPattern, SearchOption.TopDirectoryOnly).ToArray();
				    }
				    else
				    {
				        files = Enumerable.Repeat(libraryPath, 1);
				    }

				    foreach (string file in files)
					{
						// Finally, I am really loading this file
						SafeLibraryHandle handle = OpenHandle(file);

						if (!handle.IsNullOrInvalid())
						{
							Trace.TraceInformation(string.Format("{0} Loaded binary \"{1}\"", 
								traceLabel, file));

							return new UnmanagedLibrary(libraryName, handle);
						}
						else
						{
							Exception nativeEx = GetLastLibraryError();
							Trace.TraceInformation(string.Format("{0} Custom binary \"{1}\" not loaded: {2}", 
								traceLabel, file, nativeEx.Message));
						}
					}					
				}

				// Search ManifestResources for fileName.arch.ext
				// TODO: Enumerate ManifestResources for ZeroMQ{Arch}{LibraryName}{Ext}.so.*
				string resourceName = string.Format("ZeroMQ.{0}.{1}{2}", libraryName, architecture, ".so");
				string tempPath = Path.Combine(Path.GetTempPath(), resourceName);

				if (ExtractManifestResource(resourceName, tempPath))
				{
					// TODO: need syscall_chmod_execute(path); ?
					SafeLibraryHandle handle = OpenHandle(tempPath);

					if (!handle.IsNullOrInvalid())
					{
						Trace.TraceInformation(string.Format("{0} Loaded binary from EmbeddedResource \"{1}\" from \"{2}\".", 
							traceLabel, resourceName, tempPath));
						
						return new UnmanagedLibrary(libraryName, handle);
					}					
					else
					{
						Trace.TraceWarning(string.Format("{0} Unable to run the extracted EmbeddedResource \"{1}\" from \"{2}\".",
							traceLabel, resourceName, tempPath));
					}
				}
				else
				{
					Trace.TraceWarning(string.Format("{0} Unable to extract the EmbeddedResource \"{1}\" to \"{2}\".",
						traceLabel, resourceName, tempPath));
				}


				var fnf404 = new StringBuilder();
				fnf404.Append(traceLabel);
				fnf404.Append(" Unable to load binary \"");
				fnf404.Append(libraryName);
				fnf404.AppendLine("\" from folders");
				foreach (string path in libraryPaths)
				{
					fnf404.Append("\t");
					fnf404.AppendLine(path);
				}
				fnf404.Append(" Also unable to load binary from EmbeddedResource \"");
				fnf404.Append(resourceName);
				fnf404.Append("\", from temporary path \"");
				fnf404.Append(tempPath);
				fnf404.Append("\". See Trace output for more information.");

				throw new FileNotFoundException(fnf404.ToString());
			}

			public static string[] EnumerateLibLdConf(string fileName)
			{
				if (!File.Exists(fileName)) return null;

				var libLoadConf = new List<string>();
				using (var fileReader = new StreamReader(fileName))
				{
					while (!fileReader.EndOfStream)
					{
						string line = fileReader.ReadLine().TrimStart(new char[] { ' ' });

						// Comments
						if (line.StartsWith("#", StringComparison.OrdinalIgnoreCase)) continue;
						int commentI;
						if (-1 < (commentI = line.IndexOf("#")))
						{
							// remove Comments
							line = line.Substring(0, commentI);
						}

						if (string.IsNullOrWhiteSpace(line.Trim())) continue;

						// Include /etc/ld.so.conf.d/*.conf, say enumerate files
						if (line.StartsWith("include ", StringComparison.OrdinalIgnoreCase))
						{
							string folder = null;
							string filesPattern = line.Substring("include ".Length);
							int filesPatternI;
							if (-1 == (filesPatternI = filesPattern.IndexOf('*')))
							{
								filesPatternI = filesPattern.Length;
							}
							if (-1 < (filesPatternI = filesPattern.LastIndexOf('/', filesPatternI)))
							{
								folder = filesPattern.Substring(0, filesPatternI + 1);
								filesPattern = filesPattern.Substring(filesPatternI + 1);
							}

							if (folder == null || !Directory.Exists(folder)) continue;

							string[] files = Directory.EnumerateFiles(folder, filesPattern, SearchOption.TopDirectoryOnly).ToArray();

							foreach (string file in files)
							{
								string[] _libLoadConf = EnumerateLibLdConf(file);
								if (_libLoadConf != null) libLoadConf.AddRange(_libLoadConf);
							}

							continue;
						}

						// Folder
						string path = EnsureNotEndingSlash(line);
						if (path != null && Directory.Exists(path)) libLoadConf.Add(path);
					}
				}

				return libLoadConf.ToArray();
			}

            private static IEnumerable<string> EnumerateLibLdPATH()
            {
                // TODO: does it really make sense to manually enumerate these paths? dlopen 
                // will search them by default if the library name is given without any path fragments
                string[] variables;
                switch (Name)
                {
                    case PlatformName.MacOSX:
                        variables = new[] { "DYLD_LIBRARY_PATH", "DYLD_FALLBACK_LIBRARY_PATH" };
                        break;
                    case PlatformName.Posix:
                        variables = new[] { "LD_LIBRARY_PATH" };
                        break;
                    default:
                        variables = new string[] { };
                        break;
                }
                var inpaths = variables.Select(Environment.GetEnvironmentVariable).Where(x => !string.IsNullOrEmpty(x));
                foreach (var inpath in inpaths)
                {
                    foreach (var filename in EnumeratePath(inpath))
                    {
                        yield return filename;
                    }
                }
            }

            private static IEnumerable<string> EnumeratePath(string inpath)
            {
                string[] paths = inpath.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string path in paths)
                {
                    string _path = EnsureNotEndingSlash(path);

                    if (_path != null && Directory.Exists(_path)) yield return _path;
                }
            }

            private static string EnsureNotEndingSlash(string path)
			{
				if (path == null) return null;
				if (path.EndsWith("/")) return path.Substring(0, path.Length - 1);
				return path;
			}

			public static SafeLibraryHandle OpenHandle(string fileName)
			{
				IntPtr fileNamePtr = Marshal.StringToHGlobalAnsi(fileName);
				SafeLibraryHandle libHandle = dlopen(fileNamePtr, RTLD_LAZY | RTLD_GLOBAL);
				Marshal.FreeHGlobal(fileNamePtr);
				return libHandle;
			}

			public static IntPtr LoadProcedure(SafeLibraryHandle libHandle, string functionName)
			{
				IntPtr functionNamePtr = Marshal.StringToHGlobalAnsi(functionName);
				IntPtr procHandle = dlsym(libHandle, functionNamePtr);
				Marshal.FreeHGlobal(functionNamePtr);
				return procHandle;
			}

			public static bool ReleaseHandle(IntPtr handle)
			{
				return dlclose(handle) == 0;
			}

			public static Exception GetLastLibraryError()
			{
				IntPtr text = dlerror();
				string strg = null;
				if (text != IntPtr.Zero)
				{
					strg = Marshal.PtrToStringAnsi(text);
				}
				return new DllNotFoundException(strg);
			}
		}
	}
}

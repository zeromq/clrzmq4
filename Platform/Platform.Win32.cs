using System.Text;

namespace ZeroMQ.lib
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.ConstrainedExecution;
	using System.Runtime.InteropServices;

	public static partial class Platform
	{
		public static class Win32
		{
			private const string LibraryName = "kernel32";

			// public const string LibraryFileExtension = ".dll";

			public static readonly string[] LibraryPaths = new string[] {
                @"{System32}\{LibraryName}.dll",
                @"{DllPath}\{LibraryName}.dll",
				@"{AppBase}\{Arch}\{LibraryName}.dll",
            };

			[DllImport(LibraryName, CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
			private static extern SafeLibraryHandle LoadLibrary(string fileName);

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport(LibraryName, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			private static extern bool FreeLibrary(IntPtr moduleHandle);

			[DllImport(LibraryName)]
			private static extern IntPtr GetProcAddress(SafeLibraryHandle moduleHandle, string procname);

			public static UnmanagedLibrary LoadUnmanagedLibrary(string libraryName)
			{
				if (string.IsNullOrWhiteSpace(libraryName))
				{
					throw new ArgumentException("A valid library name is expected.", "libraryName");
				}

				// Now look: This method should ExpandPaths on LibraryPaths.
				// That being said, it should just enumerate
				// Path, AppBase, Arch, Compiler, LibraryName, Extension

				// Secondly, this method should try each /lib/x86_64-linux-gnu/libload.so.2 to load,
				// Third, this method should try EmbeddedResources,
				// Finally, this method fails, telling the user all libraryPaths searched.

				var libraryPaths = new List<string>(Platform.LibraryPaths);

				Platform.ExpandPaths(libraryPaths, "{System32}", Environment.SystemDirectory);
	
				var PATHs = new List<string>();
				PATHs.Add(EnsureNotEndingBackSlash(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
				PATHs.AddRange(EnumeratePATH());
				Platform.ExpandPaths(libraryPaths, "{DllPath}", PATHs);

				Platform.ExpandPaths(libraryPaths, "{AppBase}", EnsureNotEndingBackSlash(
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
					string folder = null;
					string filesPattern = libraryPath;
					int filesPatternI;
					if (-1 < (filesPatternI = filesPattern.LastIndexOf('\\')))
					{
						folder = filesPattern.Substring(0, filesPatternI + 1);
						filesPattern = filesPattern.Substring(filesPatternI + 1);
					}

					if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) continue;

					string[] files = Directory.EnumerateFiles(folder, filesPattern, SearchOption.TopDirectoryOnly).ToArray();

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

                        handle.Close();

                        Exception nativeEx = GetLastLibraryError();
						Trace.TraceInformation(string.Format("{0} Custom binary \"{1}\" not loaded: {2}", 
							traceLabel, file, nativeEx.Message));
					}
				}

				// Search ManifestResources for fileName.arch.ext
				string resourceName = string.Format("ZeroMQ.{0}.{1}{2}", libraryName, architecture, ".dll");
				string tempPath = Path.Combine(Path.GetTempPath(), resourceName);

				if (ExtractManifestResource(resourceName, tempPath))
				{
					SafeLibraryHandle handle = OpenHandle(tempPath);

					if (!handle.IsNullOrInvalid())
					{
						Trace.TraceInformation(string.Format("{0} Loaded binary from EmbeddedResource \"{1}\" from \"{2}\".", 
							traceLabel, resourceName, tempPath));
						
						return new UnmanagedLibrary(libraryName, handle);
					}

                    handle.Close();

                    Trace.TraceWarning(string.Format("{0} Unable to run the extracted EmbeddedResource \"{1}\" from \"{2}\".",
						traceLabel, resourceName, tempPath));
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

			public static string[] EnumeratePATH()
			{
				string PATH = System.Environment.GetEnvironmentVariable("PATH");
				if (PATH == null) return new string[] { };

				string[] paths = PATH.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

				var pathList = new List<string>();
				foreach (string path in paths)
				{
					string _path =
						EnsureNotDoubleQuoted(
							EnsureNotEndingBackSlash(path));
					
					if (_path != null && Directory.Exists(_path)) pathList.Add(_path);
				}
				return pathList.ToArray();
			}

			private static string EnsureNotDoubleQuoted(string path)
			{
				if (path == null) return null;
				return path.Trim(new char[] { '"' });
			}

			private static string EnsureNotEndingBackSlash(string path)
			{
				if (path == null) return null;
				return path.TrimEnd(new char[] { '\\' });
			}

			public static SafeLibraryHandle OpenHandle(string filename)
			{
				return LoadLibrary(filename);
			}

			public static IntPtr LoadProcedure(SafeLibraryHandle handle, string functionName)
			{
				return GetProcAddress(handle, functionName);
			}

			public static bool ReleaseHandle(IntPtr handle)
			{
				return FreeLibrary(handle);
			}

			public static Exception GetLastLibraryError()
			{
				return new System.ComponentModel.Win32Exception();
			}
		}
	}
}

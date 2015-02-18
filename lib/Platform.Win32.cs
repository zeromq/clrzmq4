namespace ZeroMQ.lib
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Runtime.ConstrainedExecution;
	using System.Runtime.InteropServices;

	public static partial class Platform
	{
		public static class Win32
		{
			public const string LibraryFileExtension = ".dll";

			private const string KernelLib = "kernel32";

			public static UnmanagedLibrary LoadUnmanagedLibrary(string libraryName)
			{
				if (string.IsNullOrEmpty(libraryName))
				{
					throw new ArgumentException("A valid library name is expected.", "libraryName");
				}

				string fileName = string.Concat(libraryName, Platform.LibraryFileExtension);
				string arch = Enum.GetName(typeof(ImageFileMachine), Platform.Architecture).ToLower();
				if (Platform.Architecture == ImageFileMachine.I386 && Environment.Is64BitProcess)
				{
					// In mono on linux, even the 32bit mono uses the 64bit library
					// TODO: load the 32bit binary on windows in the 32bit runtime?
					arch = Enum.GetName(typeof(ImageFileMachine), ImageFileMachine.AMD64).ToLower();
				}

				string path;
				SafeLibraryHandle handle;
				string traceLabel = string.Format("UnmanagedLibrary[{0}]", libraryName);

				// This is Platform.Windows, LoadLibrary will prepare future DllImport

				// Search ~[/bin]/arch/fileName.ext
				path = AppDomain.CurrentDomain.BaseDirectory;
				if (null != AppDomain.CurrentDomain.RelativeSearchPath)
				{
					path = Path.Combine(path, AppDomain.CurrentDomain.RelativeSearchPath);
				}
				path = Path.Combine(Path.Combine(path, arch), fileName);

				if (File.Exists(path))
				{
					handle = OpenHandle(path);
					if (!handle.IsNullOrInvalid())
					{
						Trace.TraceInformation(string.Format("{0} Loaded \"{1}\".", traceLabel, path));
						return new UnmanagedLibrary(libraryName, handle);
					}
					else
					{
						Exception nativeEx = GetLastLibraryError();
						Trace.TraceInformation(string.Format("{0} Custom binary \"{1}\" not loaded: {2}", traceLabel, path, nativeEx.Message));
					}
				}

				// Search %SYSTEMDEFAULT% + fileName.ext
				path = fileName;
				handle = OpenHandle(path);

				if (!handle.IsNullOrInvalid())
				{
					Trace.TraceInformation(string.Format("{0} Loaded \"{1}\" from system default paths.", traceLabel, path));
					return new UnmanagedLibrary(libraryName, handle);
				}

				// Search ManifestResources/fileName.arch.ext
				path = Path.Combine(Path.GetTempPath(), fileName);
				string resourceName = string.Format(string.Format("ZeroMQ.{0}.{1}{2}", libraryName, arch, LibraryFileExtension));

				if (ExtractManifestResource(resourceName, path))
				{
					handle = OpenHandle(path);
					if (!handle.IsNullOrInvalid())
					{
						Trace.TraceInformation(string.Format("{0} Loaded \"{1}\" from extracted resource \"{2}\".", traceLabel, path, resourceName));
						return new UnmanagedLibrary(libraryName, handle);
					}
				}
				else
				{
					Trace.TraceWarning(
						string.Format("{0} Unable to extract native library resource \"{1}\" to \"{2}\".",
							  traceLabel, resourceName, path));
				}

				throw new FileNotFoundException(
					string.Format(
						"{0} Unable to load library \"{1}\" from \"{2}\". Inspect Trace output for details.",
						traceLabel,
						libraryName,
						path
					),
					path,
					GetLastLibraryError()
				);
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
				return Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}

			[DllImport(KernelLib, CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
			private static extern SafeLibraryHandle LoadLibrary(string fileName);

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport(KernelLib, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			private static extern bool FreeLibrary(IntPtr moduleHandle);

			[DllImport(KernelLib)]
			private static extern IntPtr GetProcAddress(SafeLibraryHandle moduleHandle, string procname);
		}
	}
}
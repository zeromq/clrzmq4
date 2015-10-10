namespace ZeroMQ.lib
{
	using System;
	using System.Configuration;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;

	public static partial class Platform
	{
		public static class Posix
		{
			public const string LibraryFileExtension = ".so";

			private const string KernelLib = "libdl";

			private const int RTLD_LAZY = 0x0001;
			private const int RTLD_NOW = 0x0002;
			private const int RTLD_GLOBAL = 0x0100;
			private const int RTLD_LOCAL = 0x0000;

			[DllImport("__Internal")]
			private static extern void mono_dllmap_insert(IntPtr assembly, IntPtr dll, IntPtr func, IntPtr tdll, IntPtr tfunc);

			/* [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
			internal static extern int syscall_chmod (IntPtr path, uint mode); */

			public static UnmanagedLibrary LoadUnmanagedLibrary(string libraryName)
			{
				if (string.IsNullOrWhiteSpace(libraryName))
				{
					throw new ArgumentException("A valid library name is expected.", "libraryName");
				}

				string fileName = string.Concat(libraryName, Platform.LibraryFileExtension);
				string arch = Enum.GetName(typeof(ImageFileMachine), Platform.Architecture).ToLower();
				if (Platform.Architecture == ImageFileMachine.I386 && Environment.Is64BitProcess)
				{
					// In mono on linux, 32bit mono takes only the 64bit library
					// TODO: load the 32bit binary on windows in the 32bit runtime?
					arch = Enum.GetName(typeof(ImageFileMachine), ImageFileMachine.AMD64).ToLower();
				}

				// This is Platform.Posix. In mono, just dlopen'ing the library doesn't work.
				// Using DllImport("__Internal", EntryPoint = "mono_dllmap_insert") to get mono on the path.

				string path;
				SafeLibraryHandle handle;
				string traceLabel = string.Format("UnmanagedLibrary[{0}]", libraryName);

				// Search for ~[/bin]/arch/fileName.ext
				path = AppDomain.CurrentDomain.BaseDirectory;
				if (null != AppDomain.CurrentDomain.RelativeSearchPath)
				{
					path = Path.Combine(path, AppDomain.CurrentDomain.RelativeSearchPath);
				}
				path = Path.Combine(Path.Combine(path, arch), fileName);

				if (File.Exists(path))
				{
					// syscall_chmod_execute(path);
					handle = OpenHandle(path);
					if (!handle.IsNullOrInvalid())
					{
						Trace.TraceInformation(string.Format("{0} Loaded \"{1}\".", traceLabel, path));
						MonoDllMapInsert(libraryName, path);
						return new UnmanagedLibrary(libraryName, handle);
					}
					else
					{
						Exception nativeEx = GetLastLibraryError();
						Trace.TraceInformation(string.Format("{0} Custom binary \"{1}\" not loaded: {2}", traceLabel, path, nativeEx.Message));
					}
				}

				// Search System's Default Paths for fileName.ext
				path = fileName;
				handle = OpenHandle(path);
				if (!handle.IsNullOrInvalid())
				{
					Trace.TraceInformation(string.Format("{0} Loaded \"{1}\" from system default paths.", traceLabel, path));
					MonoDllMapInsert(libraryName, path);
					return new UnmanagedLibrary(libraryName, handle);
				}

				// Search ManifestResources for fileName.arch.ext
				path = Path.Combine(Path.GetTempPath(), fileName);
				string resourceName = string.Format(string.Format("ZeroMQ.{0}.{1}{2}", libraryName, arch, LibraryFileExtension));

				if (ExtractManifestResource(resourceName, path))
				{
					// TODO: need syscall_chmod_execute(path); ?
					handle = OpenHandle(path);
					if (!handle.IsNullOrInvalid())
					{
						Trace.TraceInformation(string.Format("{0} Loaded \"{1}\" from extracted resource \"{2}\".", traceLabel, path, resourceName));
						MonoDllMapInsert(libraryName, path);
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

			private static void MonoDllMapInsert(string libraryName, string libraryPath)
			{
				IntPtr libraryNamePtr = Marshal.StringToHGlobalAnsi(libraryName);
				IntPtr pathPtr = Marshal.StringToHGlobalAnsi(libraryPath);
				mono_dllmap_insert(IntPtr.Zero, libraryNamePtr, IntPtr.Zero, pathPtr, IntPtr.Zero);
				Marshal.FreeHGlobal(libraryNamePtr);
				Marshal.FreeHGlobal(pathPtr);
			}

			/* private static void syscall_chmod_execute(string libraryPath) 
			{
				IntPtr pathPtr = Marshal.StringToHGlobalAnsi(libraryPath);
				if (0 != syscall_chmod(pathPtr, (uint)( FilePermissions.ALLPERMS ))) {
					// error
				}
				Marshal.FreeHGlobal(pathPtr);
			} */

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

			[DllImport(KernelLib, CallingConvention = CallingConvention.Cdecl)]
			private static extern SafeLibraryHandle dlopen(IntPtr filename, int flags);

			[DllImport(KernelLib, CallingConvention = CallingConvention.Cdecl)]
			private static extern int dlclose(IntPtr handle);

			[DllImport(KernelLib, CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr dlerror();

			[DllImport(KernelLib, CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr dlsym(SafeLibraryHandle handle, IntPtr symbol);

		}
	}
}
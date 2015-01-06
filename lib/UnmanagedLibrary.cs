namespace ZeroMQ.lib
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Security.Permissions;

	/// <summary>
	/// Safe handle for unmanaged libraries. See http://msdn.microsoft.com/msdnmag/issues/05/10/Reliability/ for more about safe handles.
	/// </summary>
	[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
	public sealed class SafeLibraryHandle
		: Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
	{
		private SafeLibraryHandle()
			: base(true)
		{ }

		protected override bool ReleaseHandle()
		{
			return Platform.ReleaseHandle(handle);
		}
	}

	internal static class SafeLibraryHandles
	{
		public static bool IsNullOrInvalid(this SafeLibraryHandle handle)
		{
			return handle == null || handle.IsInvalid;
		}
	}

	/// <summary>
	/// Utility class to wrap an unmanaged shared lib and be responsible for freeing it.
	/// </summary>
	/// <remarks>
	/// This is a managed wrapper over the native LoadLibrary, GetProcAddress, and FreeLibrary calls on Windows
	/// and dlopen, dlsym, and dlclose on Posix environments.
	/// </remarks>
	public sealed class UnmanagedLibrary : IDisposable
	{
		private readonly string TraceLabel;

		private readonly SafeLibraryHandle _handle;

		internal UnmanagedLibrary(string libraryName, SafeLibraryHandle libraryHandle)
		{
			if (string.IsNullOrWhiteSpace(libraryName))
			{
				throw new ArgumentException("A valid library name is expected.", "libraryName");
			}
			if (libraryHandle.IsNullOrInvalid())
			{
				throw new ArgumentNullException("libraryHandle");
			}

			TraceLabel = string.Format("UnmanagedLibrary[{0}]", libraryName);

			_handle = libraryHandle;
		}

		/// <summary>
		/// Dynamically look up a function in the dll via kernel32!GetProcAddress or libdl!dlsym.
		/// </summary>
		/// <typeparam name="TDelegate">Delegate type to load</typeparam>
		/// <param name="functionName">Raw name of the function in the export table.</param>
		/// <returns>A delegate to the unmanaged function.</returns>
		/// <exception cref="MissingMethodException">Thrown if the given function name is not found in the library.</exception>
		/// <remarks>
		/// GetProcAddress results are valid as long as the dll is not yet unloaded. This
		/// is very very dangerous to use since you need to ensure that the dll is not unloaded
		/// until after you're done with any objects implemented by the dll. For example, if you
		/// get a delegate that then gets an IUnknown implemented by this dll,
		/// you can not dispose this library until that IUnknown is collected. Else, you may free
		/// the library and then the CLR may call release on that IUnknown and it will crash.
		/// </remarks>
		public TDelegate GetUnmanagedFunction<TDelegate>(string functionName) where TDelegate : class
		{
			IntPtr p = Platform.LoadProcedure(_handle, functionName);

			if (p == IntPtr.Zero)
			{
				throw new MissingMethodException("Unable to find function '" + functionName + "' in dynamically loaded library.");
			}

			// Ideally, we'd just make the constraint on TDelegate be
			// System.Delegate, but compiler error CS0702 (constrained can't be System.Delegate)
			// prevents that. So we make the constraint system.object and do the cast from object-->TDelegate.
			return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(p, typeof(TDelegate));
		}

		public void Dispose()
		{
			if (_handle != null && !_handle.IsClosed)
			{
				_handle.Close();
			}
		}

	}
}
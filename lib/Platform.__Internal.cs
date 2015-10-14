using System;
using System.Reflection;

namespace ZeroMQ.lib
{
	public static partial class Platform
	{
		public static class __Internal
		{
			public static UnmanagedLibrary LoadUnmanagedLibrary(string libraryName)
			{

				/* TODO: Move the public static readonly _delegate libversion = sqlite3_libversion
				 * TODO: into public static readonly _delegate libversion = sqlite3_libversion_internal

		// (1) Declare privately the extern entry point
		[DllImport(LibraryName, EntryPoint = "sqlite3_libversion", CallingConvention = CCCdecl)]
		private static extern IntPtr sqlite3_libversion();

		// (2) Describe the extern function using a delegate
		public delegate IntPtr sqlite3_libversion_delegate ();

		// (3) Save and return the managed delegate to the unmanaged function
		//     This static readonly field definition allows to be 
		//     initialized and possibly redirected by the static constructor.
		//     By default this is set to the extern function declaration.
		public static readonly sqlite3_libversion_delegate libversion = sqlite3_libversion;
				 * 
				 */

				return null;
			}

			public static SafeLibraryHandle OpenHandle(string fileName)
			{
				throw new NotSupportedException();
			}

			public static IntPtr LoadProcedure(SafeLibraryHandle libHandle, string functionName)
			{
				throw new NotSupportedException();
			}

			public static bool ReleaseHandle(IntPtr handle)
			{
				throw new NotSupportedException();
			}

			public static Exception GetLastLibraryError()
			{
				return new NotSupportedException();
			}
		}
	}
}
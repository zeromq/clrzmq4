using System;
using System.Reflection;

/* Example: sqlite3.cs

using System;
using System.Runtime.InteropServices;

namespace lib
{
	public static unsafe class sqlite3
	{
		private const string __Internal = "__Internal";

		private const CallingConvention CCCdecl = CallingConvention.Cdecl;

		// Use a const for the library name
		private const string LibraryName = "sqlite3";

		// Hold a handle to the static instance
		private static readonly UnmanagedLibrary NativeLib;

		// The static constructor prepares static readonly fields
		static sqlite3()
		{
			// (0) Initialize Library handle
			NativeLib = Platform.LoadUnmanagedLibrary(LibraryName);

			// (1) Initialize Platform information 
			Platform.SetupImplementation(typeof(sqlite3));
				
			// Set once LibVersion to libversion()
			LibVersion = Marshal.PtrToStringAnsi(libversion());
		}

		// (2) Declare privately the extern entry point
		[DllImport(LibraryName, EntryPoint = "sqlite3_libversion", CallingConvention = CCCdecl)]
		private static extern IntPtr sqlite3_libversion();
		[DllImport(__Internal, EntryPoint = "sqlite3_libversion", CallingConvention = CCCdecl)]
		private static extern IntPtr sqlite3_libversion__Internal();

		// (3) Describe the extern function using a delegate
		public delegate IntPtr sqlite3_libversion_delegate ();

		// (4) Save and return the managed delegate to the unmanaged function
		//     This static readonly field definition allows to be 
		//     initialized and possibly redirected by the static constructor.
		//
		//     By default this is set to the extern function declaration,
		//     it may be set to the __Internal extern function declaration.
		public static readonly sqlite3_libversion_delegate libversion = sqlite3_libversion;

		// Static LibVersion
		public static readonly Version LibVersion;

	}
}

*/

namespace ZeroMQ.lib
{
	public static partial class Platform
	{
		public static class __Internal
		{
			public static UnmanagedLibrary LoadUnmanagedLibrary(string libraryName)
			{
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
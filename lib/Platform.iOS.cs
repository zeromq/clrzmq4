namespace ZeroMQ.lib
{
	using System;
	
	public static partial class Platform
	{
		/// <summary>
		/// For iPhone, the libzmq and libsodium needs to be added as native libraries to the Xamarin project.
		/// In addition the name of the both libraries in lib.cs must be set to "__Internal"
		/// </summary>
		public static class iOS
		{
			public const string LibraryName = "__Internal";

			public static bool IsMonoTouch
			{
				get { return Type.GetType("MonoTouch.ObjCRuntime.Class") != null; }
			}
			
			public static UnmanagedLibrary LoadUnmanagedLibrary(string libraryName)
			{
				if (libraryName == "libzmq")
				{
					Platform.AssignImplementations(typeof (ZeroMQ.lib.zmq), "iOS");
				}
			
				return null;
			}
			
			public static SafeLibraryHandle OpenHandle(string fileName)
			{
				return null;
			}
			
			public static IntPtr LoadProcedure(SafeLibraryHandle libHandle, string functionName)
			{
				return IntPtr.Zero;
			}
			
			public static bool ReleaseHandle(IntPtr handle)
			{
				return true;
			}
			
			public static Exception GetLastLibraryError()
			{
				return new DllNotFoundException();
			}
		}
	}
}
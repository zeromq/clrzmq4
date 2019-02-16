namespace ZeroMQ.lib
{

	public static partial class Platform
	{
		public static class MacOSX
		{

			// public const string LibraryFileExtension = ".dylib";

			public static readonly string[] LibraryPaths = new string[] {
                "/lib/lib{LibraryName}*.dylib",
                "/lib/lib{LibraryName}*.dylib.*",
                "/usr/lib/lib{LibraryName}*.dylib",
                "/usr/lib/lib{LibraryName}*.dylib.*",
                "/usr/local/lib/lib{LibraryName}*.dylib",
                "/usr/local/lib/lib{LibraryName}*.dylib.*",
                "{DllPath}/lib{LibraryName}*.dylib",
                "{DllPath}/lib{LibraryName}*.dylib.*",
                "{Path}/lib{LibraryName}*.dylib",
                "{Path}/lib{LibraryName}*.dylib.*",
                "{AppBase}/{Arch}/lib{LibraryName}*.dylib",
                "{AppBase}/{Arch}/lib{LibraryName}*.dylib.*",
            };

		}
	}
}

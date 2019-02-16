namespace ZeroMQ.lib
{

	public static partial class Platform
	{
		public static class MacOSX
		{

			// public const string LibraryFileExtension = ".dylib";

			public static readonly string[] LibraryPaths = new string[] {
                "/lib/{LibraryName}*.dylib",
                "/lib/{LibraryName}*.dylib.*",
                "/usr/lib/{LibraryName}*.dylib",
                "/usr/lib/{LibraryName}*.dylib.*",
                "/usr/local/lib/{LibraryName}*.dylib",
                "/usr/local/lib/{LibraryName}*.dylib.*",
                "{DllPath}/{LibraryName}*.dylib",
                "{DllPath}/{LibraryName}*.dylib.*",
                "{Path}/{LibraryName}*.dylib",
                "{Path}/{LibraryName}*.dylib.*",
				"{AppBase}/{Arch}/{LibraryName}*.dylib",
				"{AppBase}/{Arch}/{LibraryName}*.dylib.*",
            };

		}
	}
}

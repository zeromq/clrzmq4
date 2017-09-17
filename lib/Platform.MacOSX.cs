namespace ZeroMQ.lib
{

	public static partial class Platform
	{
		public static class MacOSX
		{

			// public const string LibraryFileExtension = ".dylib";

			public static readonly string[] LibraryPaths = new string[] {
				"{AppBase}/{Arch}/{Compiler}/{LibraryName}.dylib",
				"{AppBase}/{Arch}/{Compiler}/{LibraryName}.dylib.*",
				"{AppBase}/{Arch}/{LibraryName}.dylib",
				"{AppBase}/{Arch}/{LibraryName}.dylib.*",
				"{Path}/{LibraryName}.dylib",
				"{Path}/{LibraryName}.dylib.*",
                "{DllPath}/{LibraryName}.dylib",
                "{DllPath}/{LibraryName}.dylib.*",
            };

		}
	}
}
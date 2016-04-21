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
				"{AppBase}/{Arch}/{Compiler}/{LibraryName}.a",
				"{AppBase}/{Arch}/{Compiler}/{LibraryName}.a.*",
				"{AppBase}/{Arch}/{Compiler}/{LibraryName}.so",
				"{AppBase}/{Arch}/{Compiler}/{LibraryName}.so.*",
				"{AppBase}/{Arch}/{LibraryName}.dylib",
				"{AppBase}/{Arch}/{LibraryName}.dylib.*",
				"{AppBase}/{Arch}/{LibraryName}.a",
				"{AppBase}/{Arch}/{LibraryName}.a.*",
				"{AppBase}/{Arch}/{LibraryName}.so",
				"{AppBase}/{Arch}/{LibraryName}.so.*",
				"{Path}/{LibraryName}.dylib",
				"{Path}/{LibraryName}.dylib.*",
				"{Path}/{LibraryName}.a",
				"{Path}/{LibraryName}.a.*",
				"{Path}/{LibraryName}.so",
				"{Path}/{LibraryName}.so.*",
			};

		}
	}
}
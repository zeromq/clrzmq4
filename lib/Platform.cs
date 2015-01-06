namespace ZeroMQ.lib
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;

	/* Common CLR type System.Runtime.InteropServices.ImageFileMachine *
	public enum ImageFileMachine
	{
		i386    = 0x014C,
		IA64    = 0x0200,
		AMD64   = 0x8664,
		ARM     = 0x01C4    // new in .NET 4.5
		
	} /**/

	public enum PlatformName : int
	{
		CLI = 0,
		Unix,
		Windows,
		MacOSX,
	}

	public enum PlatformKind : int
	{
		CLI = 0,
		Posix,
		Win32,
	}

	public enum PlatformCompiler : int
	{
		CLI = 0,
		VisualC,
		GCC
	}

	public static partial class Platform
	{

		public static readonly PlatformKind Kind;

		public static readonly PlatformName Name;

		public static readonly ImageFileMachine Architecture;

		public static readonly int OSVersion;

		public static readonly PlatformCompiler Compiler;

		public static readonly int CVersion;

		static Platform()
		{

			PortableExecutableKinds peKinds;
			typeof(object).Module.GetPEKind(out peKinds, out Architecture);

			Version osVersion;
			switch (Environment.OSVersion.Platform)
			{

				case PlatformID.Win32Windows: // Win9x supported?
				case PlatformID.Win32S: // Win16 NTVDM on Win x86?
				case PlatformID.Win32NT: // Windows NT
					Kind = PlatformKind.Win32;
					Name = PlatformName.Windows;

					/* osVersion = Environment.OSVersion.Version;
					if (osVersion.Major <= 4) {
						// WinNT 4
					} else if (osVersion.Major <= 5) {
						// Win2000, WinXP
					} else if (osVersion.Major <= 6) {
						// WinVista, Win7, Win8.x
						if (osVersion.Major == 0) {
						}
					} else {
						// info: technet .. msdn .. microsoft research

					} */
					break;

				case PlatformID.WinCE:
					// case PlatformID.Xbox:
					Kind = PlatformKind.Win32;
					Name = PlatformName.Windows;
					break;

				case PlatformID.Unix:
					Kind = PlatformKind.Posix;
					// TODO: older MS.NET frameworks say Unix for MacOSX ?
					Name = PlatformName.Unix;
					break;

				case PlatformID.MacOSX:
					Kind = PlatformKind.Posix;
					Name = PlatformName.MacOSX;
					break;

				default:
					if ((int)Environment.OSVersion.Platform == 128)
					{
						// Mono formerly used 128 for MacOSX
						Kind = PlatformKind.Posix;
						Name = PlatformName.MacOSX;
					}

					break;
			}

			// TODO: Detect and distinguish available Compilers and Runtimes

			/* switch (Kind) {

			case PlatformKind.Windows:
				LibraryFileNameFormat = Platform.Windows.LibraryFileNameFormat;
				OpenPtr = Platform.Windows.OpenPtr;
				LoadProcedure = Platform.Windows.LoadProcedure;
				ReleasePtr = Platform.Windows.ReleasePtr;
				GetLastLibraryError = Platform.Windows.GetLastLibraryError;
				break;

			case PlatformKind.Posix:
				LibraryFileNameFormat = Platform.Posix.LibraryFileNameFormat;
				OpenPtr = Platform.Posix.OpenPtr;
				LoadProcedure = Platform.Posix.LoadProcedure;
				ReleasePtr = Platform.Posix.ReleasePtr;
				GetLastLibraryError = Platform.Posix.GetLastLibraryError;
				break;

			case PlatformKind.Unknown:
			default:
				throw new PlatformNotSupportedException ();
			} */
			SetupPlatformImplementation(typeof(Platform));
		}

		public static void SetupPlatformImplementation(Type platformDependentType)
		{

			/* A typical class should look like
			 * 
			 * class MyType {
			 * 
			 * 		// A delegate to describe a method
			 * 		delegate void APlatformDependentMethodDelegate();
			 * 
			 * 		// A field to hold a method by delegate definition
			 * 		static APlatformDependentMethodDelegate APlatformDependentMethod;
			 * 
			 * 		static readonly int APlatformDependentField;
			 * 
			 * 		static MyType() {
			 * 			SetupPlatformImplementation(typeof(MyType));
			 * 		}
			 * 
			 * 		static class Posix {
			 * 
			 * 			const int APlatformDependentField = 5;
			 * 
			 * 			void APlatformDependentMethod() {
			 * 				// The actual Posix implementation of APlatformDependentMethodDelegate
			 * 			}
			 * 
			 * 		}
			 * 
			 * }
			 * 
			 */

			// BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			// MemberInfo[] members = platformDependentType.GetMembers(bindings);

			// Baseline by PlatformKind
			AssignImplementations(platformDependentType, Enum.GetName(typeof(PlatformKind), Platform.Kind));

			// Overwrite by PlatformName
			AssignImplementations(platformDependentType, Enum.GetName(typeof(PlatformName), Platform.Name));
		}

		private static void AssignImplementations(Type platformDependentType, string implementationName)
		{
			BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

			// TODO: instance members

			Type platformNameImpl = platformDependentType.GetNestedType(implementationName, bindings);
			if (platformNameImpl == null)
			{
				// TODO: else fail?
				return;
			}

			MemberInfo[] platformMembers = platformNameImpl.GetMembers(bindings);
			foreach (MemberInfo platformMember in platformMembers)
			{

				// TODO: overloaded members, GetBySignature?
				FieldInfo member = platformDependentType.GetField(platformMember.Name, bindings);
				if (member == null)
				{
					// TODO: else fail?
					continue;
				}

				if (platformMember.MemberType == MemberTypes.Method)
				{
					// if (typeof(Delegate).IsAssignableFrom(member.FieldType)) {
					var delegat = Delegate.CreateDelegate(member.FieldType, (MethodInfo)platformMember);
					member.SetValue(null /* static */, delegat);
					continue;

				}
				if (platformMember.MemberType == MemberTypes.Field)
				{
					// if (member.FieldType.IsAssignableFrom(platformMember.FieldType)) {
					member.SetValue(null /* static */, ((FieldInfo)platformMember).GetValue(null /* static */));
					continue;

				}
				// TODO: else fail?
			}
		}

		private static bool ExtractManifestResource(string resourceName, string outputPath)
		{
			if (File.Exists(outputPath))
			{
				// This is necessary to prevent access conflicts if multiple processes are run from the
				// same location. The naming scheme implemented in UnmanagedLibrary should ensure that
				// the correct version is always used.
				return true;
			}

			Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

			if (resourceStream == null)
			{
				// No manifest resources were compiled into the current assembly. This is likely a 'manual
				// deployment' situation, so do not throw an exception at this point and allow all deployment
				// paths to be searched.
				return false;
			}

			try
			{
				using (FileStream fileStream = File.Create(outputPath))
				{
					resourceStream.CopyTo(fileStream);
				}
			}
			catch (UnauthorizedAccessException)
			{
				// Caller does not have write permission for the current file
				return false;
			}

			return true;
		}

		public static readonly string LibraryFileExtension;

		public delegate UnmanagedLibrary LoadUnmanagedLibraryDelegate(string libraryName);
		public static readonly LoadUnmanagedLibraryDelegate LoadUnmanagedLibrary;

		public delegate SafeLibraryHandle OpenHandleDelegate(string filename);
		public static readonly OpenHandleDelegate OpenHandle;

		public delegate IntPtr LoadProcedureDelegate(SafeLibraryHandle handle, string functionName);
		public static readonly LoadProcedureDelegate LoadProcedure;

		public delegate bool ReleaseHandleDelegate(IntPtr handle);
		public static readonly ReleaseHandleDelegate ReleaseHandle;

		public delegate Exception GetLastLibraryErrorDelegate();
		public static readonly GetLastLibraryErrorDelegate GetLastLibraryError;

	}
}
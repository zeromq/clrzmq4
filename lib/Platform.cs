namespace ZeroMQ.lib
{
	using System;
	using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
	using System.Linq;
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

	public enum PlatformKind : int
	{
		NetCore = 0,
		Posix,
		Win32,
	}

	public enum PlatformName : int
	{
		Internal = 0,
		Posix,
		Windows,
		MacOSX,
	}

	public static partial class Platform
	{
		// public static readonly string LibraryName;

		// public static readonly string LibraryFileExtension;

		public static readonly string[] LibraryPaths;

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

		public static bool Is__Internal { get { return Platform.Name == PlatformName.Internal; } }
    
		public static readonly PlatformKind Kind;

		public static readonly PlatformName Name;

		public static readonly ImageFileMachine Architecture;

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
					// note: current Mono versions still indicate Unix for Mac OS X
					Kind = PlatformKind.Posix;
					Name = PlatformName.Posix;
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

			IsMono = Type.GetType("Mono.Runtime") != null;

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			IsMonoTouch = assemblies.Any(a => a.GetName().Name.Equals("MonoTouch", StringComparison.InvariantCultureIgnoreCase));
			IsMonoMac = assemblies.Any(a => a.GetName().Name.Equals("MonoMac", StringComparison.InvariantCultureIgnoreCase));
			IsXamarinIOS = assemblies.Any(a => a.GetName().Name.Equals("Xamarin.iOS", StringComparison.InvariantCultureIgnoreCase));
			IsXamarinAndroid = assemblies.Any(a => a.GetName().Name.Equals("Xamarin.Android", StringComparison.InvariantCultureIgnoreCase));

			if (IsMonoMac)
			{
				Kind = PlatformKind.Posix;
				Name = PlatformName.MacOSX;
			}

			if (Name == PlatformName.Posix && File.Exists("/System/Library/CoreServices/SystemVersion.plist")) 
			{
				Name = PlatformName.MacOSX;
			}

			if (IsXamarinIOS || IsMonoTouch)
			{
				// Kind = PlatformKind.__Internal;
				// Name = PlatformName.__Internal;

				Name = PlatformName.Internal;
				// Is__Internal = true;
			}

			// SetupImplementation(typeof(Platform));
		}

		public static bool IsMono { get; private set; }

		public static bool IsMonoMac { get; private set; }

		public static bool IsMonoTouch { get; private set; }

		public static bool IsXamarinIOS { get; private set; }

		public static bool IsXamarinAndroid { get; private set; }


		public static void SetupImplementation(Type platformDependant)
		{
			// Baseline by PlatformKind
			string platformKind = Enum.GetName(typeof(PlatformKind), Platform.Kind);
			AssignImplementations(platformDependant, platformKind);

			// Overwrite by PlatformName
			if (Platform.Kind != PlatformKind.NetCore) {
				string platformName = Enum.GetName (typeof(PlatformName), Platform.Name);
				if (platformName != platformKind) {
					AssignImplementations (platformDependant, platformName);
				}
			}
			else if (Is__Internal) 
			{
				AssignImplementations(platformDependant, "__Internal");
			}
		}

		private const BindingFlags bindings = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static void AssignImplementations(Type platformDependant, string implementationName)
		{
			Type platformImplementation = platformDependant.GetNestedType(implementationName, bindings);
			if (platformImplementation == null) return;

			FieldInfo[] fields = platformDependant.GetFields(bindings);
			foreach (FieldInfo field in fields)
			{
				Type fieldType = field.FieldType;
				string fieldName = fieldType.Name;

				if (fieldName.EndsWith("Delegate"))
				{
					// YOU now have
					// public static readonly UnmanagedLibrary LoadUnmanagedLibraryDelegate;

					// YOU need
					// public static readonly LoadUnmanagedLibraryDelegate LoadUnmanagedLibrary 
					//     = Platform.__Internal.LoadUnmanagedLibrary;

					fieldName = fieldName.Substring(0, fieldName.Length - "Delegate".Length);

					MethodInfo methodInfo = platformImplementation.GetMethod(fieldName, bindings);
					if (methodInfo != null)
					{
						var delegat = Delegate.CreateDelegate (fieldType, methodInfo);
						field.SetValue (null, delegat);
					}
				}
				else
				{
					FieldInfo fieldInfo = platformImplementation.GetField(field.Name, bindings);
					if (fieldInfo != null)
					{
						object value = fieldInfo.GetValue (null);
						field.SetValue (null, value);
					}
				}

			}
		}

	}
}
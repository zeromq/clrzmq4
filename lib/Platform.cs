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
		__Internal = 0,
		Posix,
		Win32,
	}

	public enum PlatformName : int
	{
		__Internal = 0,
		Posix,
		Windows,
		MacOSX,
	}

	public static partial class Platform
	{
		public static readonly string[] Compilers = new string[] {
			"msvc2008",
			"msvc2010",
			"msvc2012",
			"msvc2013",
			"msvc2015",
			"gcc3",
			"gcc4",
			"gcc5",
			"mingw32",
		};

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

		public static readonly bool Is__Internal;

		public static readonly PlatformKind Kind;

		public static readonly PlatformName Name;

		public static readonly ImageFileMachine Architecture;

		public static readonly string Compiler;

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

				Is__Internal = true;
			}

			SetupImplementation(typeof(Platform));
		}

		public static bool IsMono { get; private set; }

		public static bool IsMonoMac { get; private set; }

		public static bool IsMonoTouch { get; private set; }

		public static bool IsXamarinIOS { get; private set; }

		public static bool IsXamarinAndroid { get; private set; }

		public static void ExpandPaths(IList<string> stream,
			string extension, string path)
		{
			ExpandPaths(stream, extension, path != null ? new string[] { path } : null);
		}

		public static void ExpandPaths(IList<string> stream,
			string extension, IEnumerable<string> paths) 
		{
			int pathsC = paths == null ? 0 : paths.Count();

			foreach (string libraryPath in stream.ToArray())
			{
				if (-1 == libraryPath.IndexOf(extension)) continue;

				int libraryPathI = stream.IndexOf(libraryPath);
				stream.RemoveAt(libraryPathI);

				if (pathsC == 0)
				{
					// just continue, don't Insert them again
					continue;
				}

				if (pathsC == 1)
				{
					stream.Insert(libraryPathI, libraryPath.Replace(extension, paths.ElementAt(0)));
					continue;
				}

				foreach (string realLibraryPath in paths)
				{
					stream.Insert(libraryPathI, libraryPath.Replace(extension, realLibraryPath));
					++libraryPathI;
				}

			}
		}

		public static void SetupImplementation(Type platformDependant)
		{
			// Baseline by PlatformKind
			string platformKind = Enum.GetName(typeof(PlatformKind), Platform.Kind);
			AssignImplementations(platformDependant, platformKind);

			// Overwrite by PlatformName
			string platformName = Enum.GetName(typeof(PlatformName), Platform.Name);
			if (platformName != platformKind)
			{
				AssignImplementations(platformDependant, platformName);
			}

			if (Is__Internal) 
			{
				AssignImplementations(platformDependant, "__Internal");
			}
		}

		private const BindingFlags bindings = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static void AssignImplementations(Type platformDependant, string implementationName)
		{
			Type platformImplementation = platformDependant.GetNestedType(implementationName, bindings);
			// if (platformImplementation == null) return;

			FieldInfo[] fields = platformDependant.GetFields(bindings);
			foreach (FieldInfo field in fields)
			{
				Type fieldType = field.FieldType;
				string delegateName = fieldType.Name;
				MethodInfo methodInfo__internal = null;
				FieldInfo fieldInfo__internal = null;

				// TODO: This is mapping sodium.crypto_box to sodium.crypto_box__Internal. Should we also map them to sodium.__Internal.crypto_box?
				if (implementationName == "__Internal")
				{
					if (delegateName.EndsWith("_delegate"))
					{
						// YOU now have
						// public static readonly crypto_box_delegate box = crypto_box;

						// YOU need
						// public static readonly crypto_box_delegate box = crypto_box__Internal;

						delegateName = delegateName.Substring(0, delegateName.Length - "_delegate".Length);
						if (delegateName.Length > 0)
						{
							methodInfo__internal = platformDependant.GetMethod(delegateName + "__Internal", bindings);
						}
					}
				}
				if (methodInfo__internal == null && platformImplementation != null)
				{
					if (delegateName.EndsWith("Delegate"))
					{
						// YOU now have
						// public static readonly UnmanagedLibrary LoadUnmanagedLibraryDelegate;

						// YOU need
						// public static readonly LoadUnmanagedLibraryDelegate LoadUnmanagedLibrary 
						//     = Platform.__Internal.LoadUnmanagedLibrary;

						delegateName = delegateName.Substring(0, delegateName.Length - "Delegate".Length);

						methodInfo__internal = platformImplementation.GetMethod(delegateName, bindings);
					}
					else
					{
						methodInfo__internal = platformImplementation.GetMethod(field.Name, bindings);
					}

					if (methodInfo__internal == null)
					{
						fieldInfo__internal = platformImplementation.GetField(field.Name, bindings);
					}
				}

				if (methodInfo__internal != null)
				{
					var delegat = Delegate.CreateDelegate(fieldType, methodInfo__internal);
					field.SetValue(null, delegat);
				}
				else if (fieldInfo__internal != null)
				{
					object value = fieldInfo__internal.GetValue(null);
					field.SetValue(null, value);
				}
				// else { field.SetValue(null, null); }
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

	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZeroMQ
{
	using lib;

	public class ZError : ZSymbol
	{
		static ZError()
		{
			var one = ZSymbol.None;
		}

		public static class Code
		{
			static Code() 
			{
				Platform.SetupPlatformImplementation(typeof(Code));
			}

			private const int HAUSNUMERO = 156384712;

			public static readonly int
				EPERM = 1,
				ENOENT = 2,
				ESRCH = 3,
				EINTR = 4,
				EIO = 5,
				ENXIO = 6,
				E2BIG = 7,
				ENOEXEC = 8,
				EBADF = 9,
				ECHILD = 10,
				EAGAIN = 11,
				ENOMEM = 12,
				EACCES = 13,
				EFAULT = 14,
				ENOTBLK = 15,
				EBUSY = 16,
				EEXIST = 17,
				EXDEV = 18,
				ENODEV = 19,
				ENOTDIR = 20,
				EISDIR = 21,
				EINVAL = 22,
				ENFILE = 23,
				EMFILE = 24,
				ENOTTY = 25,
				ETXTBSY = 26,
				EFBIG = 27,
				ENOSPC = 28,
				ESPIPE = 29,
				EROFS = 30,
				EMLINK = 31,
				EPIPE = 32,
				EDOM = 33,
				ERANGE = 34, // 34

				ENOTSUP = HAUSNUMERO + 1,
				EPROTONOSUPPORT = HAUSNUMERO + 2,
				ENOBUFS = HAUSNUMERO + 3,
				ENETDOWN = HAUSNUMERO + 4,
				EADDRINUSE = HAUSNUMERO + 5,
				EADDRNOTAVAIL = HAUSNUMERO + 6,
				ECONNREFUSED = HAUSNUMERO + 7,
				EINPROGRESS = HAUSNUMERO + 8,
				ENOTSOCK = HAUSNUMERO + 9,
				EMSGSIZE = HAUSNUMERO + 10,
							// as of here are differences to nanomsg
				EAFNOSUPPORT = HAUSNUMERO + 11,
				ENETUNREACH = HAUSNUMERO + 12,
				ECONNABORTED = HAUSNUMERO + 13,
				ECONNRESET = HAUSNUMERO + 14,
				ENOTCONN = HAUSNUMERO + 15,
				ETIMEDOUT = HAUSNUMERO + 16,
				EHOSTUNREACH = HAUSNUMERO + 17,
				ENETRESET = HAUSNUMERO + 18,

				/*  Native ZeroMQ error codes. */
				EFSM = HAUSNUMERO + 51,
				ENOCOMPATPROTO = HAUSNUMERO + 52,
				ETERM = HAUSNUMERO + 53,
				EMTHREAD // = HAUSNUMERO + 54
			;

			public static class Posix
			{
				// source: http://www.virtsync.com/c-error-codes-include-errno

				public static readonly int
					// ENOTSUP = HAUSNUMERO + 1,
					EPROTONOSUPPORT = 93,
					ENOBUFS = 105,
					ENETDOWN = 100,
					EADDRINUSE = 98,
					EADDRNOTAVAIL = 99,
					ECONNREFUSED = 111,
					EINPROGRESS = 115,
					ENOTSOCK = 88,
					EMSGSIZE = 90,
					EAFNOSUPPORT = 97,
					ENETUNREACH = 101,
					ECONNABORTED = 103,
					ECONNRESET = 104,
					ENOTCONN = 107,
					ETIMEDOUT = 110,
					EHOSTUNREACH = 113,
					ENETRESET = 102
				;
			}
		}

		public static ZError GetLastErr()
		{
			int errno = zmq.errno();

			return FromErrno(errno, true);
		}

		public static ZError FromErrno(int num)
		{
			return FromErrno(num, true);
		}

		public static ZError FromErrno(int num, bool resolveText)
		{
			ZError symbol = Find("E", num).OfType<ZError>().FirstOrDefault();
			if (symbol != null) return symbol;

			// Unexpected error
			string txt = null;
			if (resolveText)
			{
				IntPtr errorString = zmq.strerror(num);
				if (errorString != IntPtr.Zero)
				{
					txt = Marshal.PtrToStringAnsi(errorString);
				}
			}
			return new ZError(num, null, txt);
		}

		public ZError(int errno)
			: base(errno)
		{ }

		public ZError(int errno, string errname, string errtext)
			: base(errno, errname, errtext)
		{ }

		public static new ZError None
		{
			get
			{
				return default(ZError); // null
			}
		}

		public static readonly ZError
			// DEFAULT = new ZmqError(0),
			EPERM,
			ENOENT,
			ESRCH,
			EINTR,
			EIO,
			ENXIO,
			E2BIG,
			ENOEXEC,
			EBADF,
			ECHILD,
			EAGAIN,
			ENOMEM,
			EACCES,
			EFAULT,
			ENOTBLK,
			EBUSY,
			EEXIST,
			EXDEV,
			ENODEV,
			ENOTDIR,
			EISDIR,
			EINVAL,
			ENFILE,
			EMFILE,
			ENOTTY,
			ETXTBSY,
			EFBIG,
			ENOSPC,
			ESPIPE,
			EROFS,
			EMLINK,
			EPIPE,
			EDOM,
			ERANGE, // 34

			ENOTSUP,
			EPROTONOSUPPORT,
			ENOBUFS,
			ENETDOWN,
			EADDRINUSE,
			EADDRNOTAVAIL,
			ECONNREFUSED,
			EINPROGRESS,
			ENOTSOCK,
			EMSGSIZE,
			// as of here are differences to nanomsg
			EAFNOSUPPORT,
			ENETUNREACH,
			ECONNABORTED,
			ECONNRESET,
			ENOTCONN,
			ETIMEDOUT,
			EHOSTUNREACH,
			ENETRESET,

			/*  Native ZeroMQ error codes. */
			EFSM,
			ENOCOMPATPROTO,
			ETERM,
			EMTHREAD // = HAUSNUMERO + 54
		;
	}
}
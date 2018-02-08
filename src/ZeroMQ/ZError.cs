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

        internal static class Code
		{
			static Code() 
			{
				Platform.SetupImplementation(typeof(Code));
			}

			private const int HAUSNUMERO = 156384712;

            // TODO: find a way to make this independent of the Windows SDK version that libzmq was built against
            // TODO: are all of these actually used by libzmq?
            // these values are the Windows error codes as defined by the Windows 10 SDK when _CRT_NO_POSIX_ERROR_CODES is not defined
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

				ENOTSUP = 129,
				EPROTONOSUPPORT = 135,
				ENOBUFS = 119,
				ENETDOWN = 116,
				EADDRINUSE = 100,
				EADDRNOTAVAIL = 101,
				ECONNREFUSED = 107,
				EINPROGRESS = 112,
				ENOTSOCK = 128,
				EMSGSIZE = 115,
							// as of here are differences to nanomsg
				EAFNOSUPPORT = 102,
				ENETUNREACH = 118,
				ECONNABORTED = 106,
				ECONNRESET = 108,
				ENOTCONN = 126,
				ETIMEDOUT = 138,
				EHOSTUNREACH = 110,
				ENETRESET = 117,

				/*  Native ZeroMQ error codes. */
				EFSM = HAUSNUMERO + 51,
				ENOCOMPATPROTO = HAUSNUMERO + 52,
				ETERM = HAUSNUMERO + 53,
				EMTHREAD // = HAUSNUMERO + 54
			;

			internal static class Posix
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

            internal static class MacOSX
            {
                public static readonly int 
                    EAGAIN = 35,
					EINPROGRESS = 36,
                    ENOTSOCK = 38,
                    EMSGSIZE = 40,
                    EPROTONOSUPPORT = 43,
                    EAFNOSUPPORT = 47,
                    EADDRINUSE = 48,
                    EADDRNOTAVAIL = 49,
                    ENETDOWN = 50,
                    ENETUNREACH = 51,
                    ENETRESET = 52,
                    ECONNABORTED = 53,
                    ECONNRESET = 54,
                    ENOBUFS = 55,
                    ENOTCONN = 57,
                    ETIMEDOUT = 60,
                    EHOSTUNREACH = 65
                    ;
            }
		}

		public static ZError GetLastErr()
		{
			int errno = zmq.errno();

			return FromErrno(errno);
		}

		public static ZError FromErrno(int num)
		{
            // TODO: this can be made more efficient
			ZError symbol = Find("E", num).OfType<ZError>().FirstOrDefault();
			if (symbol != null) return symbol;

            // unexpected error
			return new ZError(num);
		}

		internal ZError(int errno)
			: base(errno)
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
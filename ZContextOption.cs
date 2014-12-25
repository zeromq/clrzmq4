using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZeroMQ
{
    public class ZContextOption : ZSymbol
	{
        static ZContextOption()
        {
            var one = ZSymbol.None;
        }

        public static class Code
        {
            public static readonly int
                IO_THREADS = 1,
                MAX_SOCKETS = 2,
                IPV6 = 42;
        }

		public ZContextOption(int errno)
		: base(errno) 
		{ }

		public ZContextOption(int errno, string errname, string errtext)
		: base(errno, errname, errtext) 
		{ }

		public static readonly ZContextOption
				IO_THREADS,
				MAX_SOCKETS,
				IPV6 // in zmq.h ZMQ_IPV6 is in the socket options section
			;
	}
}

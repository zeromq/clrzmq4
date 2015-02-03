using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZeroMQ
{
	public enum ZContextOption : int
	{
		IO_THREADS,
		MAX_SOCKETS,
		IPV6 // in zmq.h ZMQ_IPV6 is in the socket options section
	}
}
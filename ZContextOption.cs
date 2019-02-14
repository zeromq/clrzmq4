using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZeroMQ
{
	public enum ZContextOption : int
	{
		IO_THREADS = 1,
		MAX_SOCKETS = 2,
		SOCKET_LIMIT = 3,
		THREAD_PRIORITY = 3,
		THREAD_SCHED_POLICY = 4,
		IPV6 = 42	// in zmq.h ZMQ_IPV6 is in the socket options section
	}
}
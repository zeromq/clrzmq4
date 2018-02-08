namespace ZeroMQ.lib
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct zmq_pollitem_windows_t // : zmq_pollitem_i
	{
		private IntPtr socketPtr;
		private IntPtr fileDescriptor; // Windows is an size_t
		private short events;
		private short readyEvents;

		public zmq_pollitem_windows_t(IntPtr socket, ZPoll pollEvents)
		{
			if (socket == IntPtr.Zero)
			{
				throw new ArgumentException("Expected a valid socket handle.", "socket");
			}

			socketPtr = socket;
			fileDescriptor = IntPtr.Zero;
			events = (short)pollEvents;
			readyEvents = 0;
		}

		public IntPtr SocketPtr
		{
			get { return socketPtr; }
			set { socketPtr = value; }
		}

		public IntPtr FileDescriptor
		{
			get { return fileDescriptor; }
			set { fileDescriptor = value; }
		}

		public short Events
		{
			get { return events; }
			set { events = value; }
		}

		public short ReadyEvents
		{
			get { return readyEvents; }
			set { readyEvents = value; }
		}
	}
}
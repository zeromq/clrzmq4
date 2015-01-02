namespace ZeroMQ {
	using System;

    public class ZPollItem
    {
        public delegate bool ReceiveDelegate(ZSocket socket, out ZMessage message, out ZError error);

        public delegate bool SendDelegate(ZSocket socket, ZMessage message, out ZError error);

		public ZSocket Socket {
			get;
			protected set;
		}

		public ZPoll Events;

		public ZPoll ReadyEvents;

		public ReceiveDelegate ReceiveMessage;

		public SendDelegate SendMessage;

		public ZPollItem (ZSocket socket)
		: this (socket, ZPoll.None)
		{ }

		public ZPollItem (ZSocket socket, ZPoll events)
		{
			if (socket == null) {
				throw new ArgumentNullException("socket");
			}

			Socket = socket;
			Events = events;
		}
		
	}
}


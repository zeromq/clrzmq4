namespace ZeroMQ {
	using System;
	using System.Collections.Generic;
	using lib;

	public delegate bool ZmqPollReceiveDelegate(ZSocket socket, out ZMessage message, out ZError error);

	public delegate bool ZmqPollSendDelegate(ZSocket socket, ZMessage message, out ZError error);

	public partial class ZPollItem {

		public ZSocket Socket {
			get;
			protected set;
		}

		public ZPoll Events;

		public ZPoll ReadyEvents;

		public ZmqPollReceiveDelegate ReceiveMessage;

		public ZmqPollSendDelegate SendMessage;

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


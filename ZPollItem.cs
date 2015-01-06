namespace ZeroMQ
{
	using System;

	public class ZPollItem
	{
		public delegate bool ReceiveDelegate(ZSocket socket, out ZMessage message, out ZError error);

		public delegate bool SendDelegate(ZSocket socket, ZMessage message, out ZError error);

		public ZSocket Socket
		{
			get;
			protected set;
		}

		public ZPoll Events;

		public ZPoll ReadyEvents;

		public ReceiveDelegate ReceiveMessage;

		public SendDelegate SendMessage;

		protected ZPollItem(ZSocket socket, ZPoll events)
		{
			if (socket == null)
			{
				throw new ArgumentNullException("socket");
			}

			Socket = socket;
			Events = events;
		}

		public static ZPollItem Create(ZSocket socket, ReceiveDelegate receiveMessage)
		{
			return Create(socket, receiveMessage, null);
		}

		public static ZPollItem Create(ZSocket socket, ReceiveDelegate receiveMessage, SendDelegate sendMessage)
		{
			var pollItem = new ZPollItem(socket, (receiveMessage != null ? ZPoll.In : ZPoll.None) | (sendMessage != null ? ZPoll.Out : ZPoll.None));
			pollItem.ReceiveMessage = receiveMessage;
			pollItem.SendMessage = sendMessage;
			return pollItem;
		}

		public static ZPollItem CreateSender(ZSocket socket, SendDelegate sendMessage)
		{
			return Create(socket, null, sendMessage);
		}
	}
}
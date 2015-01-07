namespace ZeroMQ
{
	using System;
	using System.Threading;

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

		public static ZPollItem CreateSender(ZSocket socket, SendDelegate sendMessage)
		{
			return Create(socket, null, sendMessage);
		}

		public static ZPollItem Create(ZSocket socket, ReceiveDelegate receiveMessage, SendDelegate sendMessage)
		{
			var pollItem = new ZPollItem(socket, (receiveMessage != null ? ZPoll.In : ZPoll.None) | (sendMessage != null ? ZPoll.Out : ZPoll.None));
			pollItem.ReceiveMessage = receiveMessage;
			pollItem.SendMessage = sendMessage;
			return pollItem;
		}

		public static ZPollItem CreateReceiver(ZSocket socket)
		{
			return Create(socket, (ZSocket _socket, out ZMessage message, out ZError error) =>
			{
				while (null == (message = _socket.ReceiveMessage(ZSocketFlags.DontWait, out error)))
				{
					if (error == ZError.EAGAIN)
					{
						error = ZError.None;
						Thread.Sleep(1);

						continue;
					}
					return false;
				}
				return true;
			}, null);
		}

		public static ZPollItem CreateSender(ZSocket socket)
		{
			return Create(socket, null, (ZSocket _socket, ZMessage message, out ZError error) =>
			{
				while (!_socket.SendMessage(message, ZSocketFlags.DontWait, out error))
				{
					if (error == ZError.EAGAIN)
					{
						error = ZError.None;
						Thread.Sleep(1);

						continue;
					}
					return false;
				}
				return true;
			});
		}

		public static ZPollItem CreateReceiverSender(ZSocket socket, ReceiveDelegate receiveMessage, SendDelegate sendMessage)
		{
			return Create(socket, (ZSocket _socket, out ZMessage message, out ZError error) =>
			{
				while (null == (message = _socket.ReceiveMessage(ZSocketFlags.DontWait, out error)))
				{
					if (error == ZError.EAGAIN)
					{
						error = ZError.None;
						Thread.Sleep(1);

						continue;
					}
					return false;
				}
				return true;

			}, (ZSocket _socket, ZMessage message, out ZError error) =>
			{
				while (!_socket.SendMessage(message, ZSocketFlags.DontWait, out error))
				{
					if (error == ZError.EAGAIN)
					{
						error = ZError.None;
						Thread.Sleep(1);

						continue;
					}
					return false;
				}
				return true;
			});
		}
	}
}
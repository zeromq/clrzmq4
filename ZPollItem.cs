namespace ZeroMQ
{
	using System;
	using System.Threading;

	public class ZPollItem
	{
		// public ZSocket Socket { get; protected set; }

		public ZPoll Events;

		public ZPoll ReadyEvents;

		public delegate bool ReceiveDelegate(ZSocket socket, out ZMessage message, out ZError error);

		public ReceiveDelegate ReceiveMessage;

		public static bool DefaultReceiveMessage(ZSocket socket, out ZMessage message, out ZError error)
		{
			message = null;
			return socket.ReceiveMessage(ref message, out error);
		}

		public delegate bool SendDelegate(ZSocket socket, ZMessage message, out ZError error);

		public SendDelegate SendMessage;

		public static bool DefaultSendMessage(ZSocket socket, ZMessage message, out ZError error)
		{
			return socket.Send(message, out error);
		}

		protected ZPollItem(/* ZSocket socket, */ ZPoll events)
		{
			/* if (socket == null)
			{
				throw new ArgumentNullException("socket");
			}

			Socket = socket; */
			Events = events;
		}

		public static ZPollItem Create(/* ZSocket socket, */ ReceiveDelegate receiveMessage)
		{
			return Create(/* socket, */ receiveMessage, null);
		}

		public static ZPollItem CreateSender(/* ZSocket socket, */ SendDelegate sendMessage)
		{
			return Create(/* socket, */ null, sendMessage);
		}

		public static ZPollItem Create(/* ZSocket socket, */ ReceiveDelegate receiveMessage, SendDelegate sendMessage)
		{
			var pollItem = new ZPollItem(/* socket, */ (receiveMessage != null ? ZPoll.In : ZPoll.None) | (sendMessage != null ? ZPoll.Out : ZPoll.None));
			pollItem.ReceiveMessage = receiveMessage;
			pollItem.SendMessage = sendMessage;
			return pollItem;
		}

		public static ZPollItem CreateReceiver(/* ZSocket socket, */ )
		{
			return Create(/* socket, */ DefaultReceiveMessage, null);
		}

		public static ZPollItem CreateSender(/* ZSocket socket, */ )
		{
			return Create(/* socket, */ null, DefaultSendMessage);
		}

		public static ZPollItem CreateReceiverSender(/* ZSocket socket, */ )
		{
			return Create(/* socket, */ DefaultReceiveMessage, DefaultSendMessage);
		}
	}
}
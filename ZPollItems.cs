using System;

namespace ZeroMQ
{
	using lib;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static partial class ZPollItems
	{

		// unsafe native delegate
		internal delegate bool PollManyDelegate(IEnumerable<ZPollItem> items, ZPoll pollFirst, out ZError error, TimeSpan? timeoutMs);
		internal static readonly PollManyDelegate PollMany;

		// unsafe native delegate
		internal delegate bool PollSingleDelegate(ZPollItem item, ZPoll pollFirst, out ZError error, TimeSpan? timeout);
		internal static readonly PollSingleDelegate PollSingle;

		static ZPollItems()
		{
			// Initialize static Fields
			Platform.SetupPlatformImplementation(typeof(ZPollItems));
		}

		public static bool PollIn(this ZPollItem item, out ZMessage incoming, out ZError error, TimeSpan? timeout = null)
		{
			incoming = null;
			return Poll(item, ZPoll.In, ref incoming, out error, timeout);
		}

		public static bool PollOut(this ZPollItem item, ZMessage outgoing, out ZError error, TimeSpan? timeout = null)
		{
			if (outgoing == null)
			{
				throw new ArgumentNullException("outgoing");
			}
			return Poll(item, ZPoll.Out, ref outgoing, out error, timeout);
		}

		public static bool Poll(this ZPollItem item, ZPoll pollEvents, ref ZMessage message, out ZError error, TimeSpan? timeout = null)
		{
			if (PollSingle(item, pollEvents, out error, timeout))
			{

				if (PollSingleResult(item, pollEvents, ref message))
				{

					return true;
				}
				error = ZError.EAGAIN;
			}
			return false;
		}

		internal static bool PollSingleResult(ZPollItem item, ZPoll pollEvents, ref ZMessage message)
		{
			bool shouldReceive = item.ReceiveMessage != null && ((pollEvents & ZPoll.In) == ZPoll.In);
			bool shouldSend = item.SendMessage != null && ((pollEvents & ZPoll.Out) == ZPoll.Out);

			if (pollEvents == ZPoll.In)
			{

				if (!shouldReceive)
				{
					throw new InvalidOperationException("No ReceiveMessage delegate set for Poll.In");
				}

				if (OnReceiveMessage(item, out message))
				{

					if (!shouldSend)
					{
						return true;
					}

					if (OnSendMessage(item, message))
					{
						return true;
					}
				}
			}
			else if (pollEvents == ZPoll.Out)
			{

				if (!shouldSend)
				{
					throw new InvalidOperationException("No SendMessage delegate set for Poll.Out");
				}

				if (OnSendMessage(item, message))
				{

					if (!shouldReceive)
					{
						return true;
					}

					if (OnReceiveMessage(item, out message))
					{
						return true;
					}
				}
			}
			return false;
		}

		internal static bool OnReceiveMessage(ZPollItem item, out ZMessage message)
		{
			message = null;

			if (((ZPoll)item.ReadyEvents & ZPoll.In) == ZPoll.In)
			{

				ZError recvWorkerE;
				if (item.ReceiveMessage == null)
				{
					// throw?
				}
				else if (item.ReceiveMessage(item.Socket, out message, out recvWorkerE))
				{
					// what to do?

					return true;
				}
			}
			return false;
		}

		internal static bool OnSendMessage(ZPollItem item, ZMessage message)
		{
			if (((ZPoll)item.ReadyEvents & ZPoll.Out) == ZPoll.Out)
			{

				ZError sendWorkerE;
				if (item.SendMessage == null)
				{
					// throw?
				}
				else if (item.SendMessage(item.Socket, message, out sendWorkerE))
				{
					// what to do?

					return true;
				}
			}
			return false;
		}

		public static bool PollIn(this IEnumerable<ZPollItem> items, out ZMessage[] incoming, out ZError error, TimeSpan? timeout = null)
		{
			incoming = null;
			return Poll(items, ZPoll.In, ref incoming, out error, timeout);
		}

		public static bool PollOut(this IEnumerable<ZPollItem> items, ZMessage[] outgoing, out ZError error, TimeSpan? timeout = null)
		{
			if (outgoing == null)
			{
				throw new ArgumentNullException("outgoing");
			}
			return Poll(items, ZPoll.Out, ref outgoing, out error, timeout);
		}

		public static bool Poll(this IEnumerable<ZPollItem> items, ZPoll pollEvents, ref ZMessage[] messages, out ZError error, TimeSpan? timeout = null)
		{
			if (PollMany(items, pollEvents, out error, timeout))
			{

				if (PollManyResult(items, pollEvents, ref messages))
				{

					return true;
				}
			}

			error = ZError.EAGAIN;
			return false;
		}

		internal static bool PollManyResult(IEnumerable<ZPollItem> items, ZPoll pollEvents, ref ZMessage[] messages)
		{
			int count = items.Count();
			int readyCount = 0;

			bool send = messages != null && ((pollEvents & ZPoll.Out) == ZPoll.Out);
			bool receive = ((pollEvents & ZPoll.In) == ZPoll.In);

			ZMessage[] incoming = null;
			if (receive)
			{
				incoming = new ZMessage[count];
			}

			for (int i = 0; i < count; ++i)
			{
				ZPollItem item = items.ElementAt(i);
				ZMessage message = send ? messages[i] : null;

				if (ZPollItems.PollSingleResult(item, pollEvents, ref message))
				{
					++readyCount;
				}
				if (receive)
				{
					incoming[i] = message;
				}
			}

			if (receive)
			{
				messages = incoming;
			}
			return readyCount > 0;
		}

	}
}
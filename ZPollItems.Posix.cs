namespace ZeroMQ
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;

	using lib;

	public static partial class ZPollItems // : IDisposable, IList<ZmqPollItem>
	{
		public static class Posix
		{
			unsafe internal static bool PollMany(
				IEnumerable<ZSocket> sockets, 
				IEnumerable<ZPollItem> items, ZPoll pollEvents, 
				out ZError error, TimeSpan? timeout = null)
			{
				error = default(ZError);
				bool result = false;
				int count = items.Count();
				int timeoutMs = !timeout.HasValue ? -1 : (int)timeout.Value.TotalMilliseconds;

				zmq_pollitem_posix_t* natives = stackalloc zmq_pollitem_posix_t[count];
				// fixed (zmq_pollitem_posix_t* natives = managedArray) {

				for (int i = 0; i < count; ++i)
				{
					ZSocket socket = sockets.ElementAt(i);
					ZPollItem item = items.ElementAt(i);
					zmq_pollitem_posix_t* native = natives + i;

					native->SocketPtr = socket.SocketPtr;
					native->Events = (short)(item.Events & pollEvents);
					native->ReadyEvents = (short)ZPoll.None;
				}

				while (!(result = (-1 != zmq.poll(natives, count, timeoutMs))))
				{
					error = ZError.GetLastErr();

					if (error == ZError.EINTR)
					{
						error = default(ZError);
						continue;
					}
					break;
				}

				for (int i = 0; i < count; ++i)
				{
					ZPollItem item = items.ElementAt(i);
					zmq_pollitem_posix_t* native = natives + i;

					item.ReadyEvents = (ZPoll)native->ReadyEvents;
				}
				// }

				return result;
			}

			unsafe internal static bool PollSingle(
				ZSocket socket,
				ZPollItem item, ZPoll pollEvents,
				out ZError error, TimeSpan? timeout = null)
			{
				error = default(ZError);
				bool result = false;
				int timeoutMs = !timeout.HasValue ? -1 : (int)timeout.Value.TotalMilliseconds;

				zmq_pollitem_posix_t* native = stackalloc zmq_pollitem_posix_t[1];
				// fixed (zmq_pollitem_posix_t* native = managedArray) {

				native->SocketPtr = socket.SocketPtr;
				native->Events = (short)(item.Events & pollEvents);
				native->ReadyEvents = (short)ZPoll.None;

				while (!(result = (-1 != zmq.poll(native, 1, timeoutMs))))
				{
					error = ZError.GetLastErr();

					if (error == ZError.EINTR)
					{
						error = default(ZError);
						continue;
					}
					break;
				}

				item.ReadyEvents = (ZPoll)native->ReadyEvents;
				//}

				return result;
			}
		}
	}
}
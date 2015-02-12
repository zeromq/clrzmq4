using System;
using System.Runtime.InteropServices;

using ZeroMQ.lib;

namespace ZeroMQ.Monitoring
{

	/// <summary>
	/// Defines extension methods related to monitoring for <see cref="ZSocket"/> instances.
	/// </summary>
	public static class ZMonitors
	{
		/// <summary>
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes all events for
		/// the specified socket over the inproc transport at the given endpoint.
		/// </summary>
		public static bool Monitor(this ZSocket socket, string endpoint)
		{
			ZError error;
			if (!Monitor(socket, endpoint, ZMonitorEvents.AllEvents, out error))
			{
				throw new ZException(error);
			}
			return true;
		}

		/// <summary>
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes all events for
		/// the specified socket over the inproc transport at the given endpoint.
		/// </summary>
		public static bool Monitor(this ZSocket socket, string endpoint, out ZError error)
		{
			return Monitor(socket, endpoint, ZMonitorEvents.AllEvents, out error);
		}

		/// <summary>
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes all events for
		/// the specified socket over the inproc transport at the given endpoint.
		/// </summary>
		public static bool Monitor(this ZSocket socket, string endpoint, ZMonitorEvents eventsToMonitor)
		{
			ZError error;
			if (!Monitor(socket, endpoint, eventsToMonitor, out error))
			{
				throw new ZException(error);
			}
			return true;
		}

		/// <summary>
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes all events for
		/// the specified socket over the inproc transport at the given endpoint.
		/// </summary>
		public static bool Monitor(this ZSocket socket, string endpoint, ZMonitorEvents eventsToMonitor, out ZError error)
		{
			if (socket == null)
			{
				throw new ArgumentNullException("socket");
			}

			if (endpoint == null)
			{
				throw new ArgumentNullException("endpoint");
			}

			if (endpoint == string.Empty)
			{
				throw new ArgumentException("Unable to publish socket events to an empty endpoint.", "endpoint");
			}

			error = ZError.None;

			using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
			{
				while (-1 == zmq.socket_monitor(socket.SocketPtr, endpointPtr, (Int32)eventsToMonitor))
				{
					error = ZError.GetLastErr();

					if (error == ZError.EINTR)
					{
						error = default(ZError);
						continue;
					}

					return false;
				}
			}
			return true;
		}
	}
}
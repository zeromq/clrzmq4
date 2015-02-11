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
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes all state changes (events) for
		/// the specified socket over the inproc transport at the given endpoint.
		/// </summary>
		/// <remarks>
		/// It is recommended to connect via a <see cref="ZSocketType.PAIR"/> socket in another thread
		/// to handle incoming monitoring events. The <see cref="ZmqMonitor"/> class provides an event-driven
		/// abstraction over event processing.
		/// </remarks>
		/// <param name="socket">The <see cref="ZSocket"/> instance to monitor for state changes.</param>
		/// <param name="endpoint">The inproc endpoint on which state changes will be published.</param>
		/// <exception cref="ArgumentNullException"><paramref name="socket"/> or <see cref="endpoint"/> is null.</exception>
		/// <exception cref="ArgumentException"><see cref="endpoint"/> is an empty string.</exception>
		/// <exception cref="ZException">An error occurred initiating socket monitoring.</exception>
		public static bool Monitor(this ZSocket socket, string endpoint, out ZError error)
		{
			return Monitor(socket, endpoint, ZMonitorEvents.AllEvents, out error);
		}

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
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes the specified state changes (events) for
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
						continue;
					}

					return false;
				}
			}
			return true;
		}
	}
}
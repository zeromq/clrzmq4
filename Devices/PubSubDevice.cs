namespace ZeroMQ.Devices
{
	using System;

    /// <summary>
    /// Collects messages from a set of publishers and forwards these to a set of subscribers.
    /// </summary>
    /// <remarks>
    /// Generally used to bridge networks. E.g. read on TCP unicast and forward on multicast.
    /// This device is part of the publish-subscribe pattern. The frontend speaks to publishers
    /// and the backend speaks to subscribers.
    /// </remarks>
    public class PubSubDevice : ZDevice
    {
        /// <summary>
        /// The frontend <see cref="ZSocketType"/> for a forwarder device.
        /// </summary>
		public static readonly ZSocketType FrontendType = ZSocketType.XSUB;

        /// <summary>
        /// The backend <see cref="ZSocketType"/> for a forwarder device.
        /// </summary>
		public static readonly ZSocketType BackendType = ZSocketType.XPUB;
		/// <summary>
		/// Initializes a new instance of the <see cref="ForwarderDevice"/> class.
		/// </summary>
		/// <param name="context">The <see cref="ZContext"/> to use when creating the sockets.</param>
		/// <param name="frontendBindAddr">The endpoint used to bind the frontend socket.</param>
		/// <param name="backendBindAddr">The endpoint used to bind the backend socket.</param>
		/// <param name="mode">The <see cref="DeviceMode"/> for the current device.</param>
		public PubSubDevice(ZContext context, string frontendBindAddr, string backendBindAddr)
			: base(context, FrontendType, BackendType)
		{
			FrontendSetup.SubscribeAll();
			FrontendSetup.Bind(frontendBindAddr);
			BackendSetup.Bind(backendBindAddr);
		}

        /// <summary>
        /// Forwards requests from the frontend socket to the backend socket.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected override bool FrontendHandler(ZSocket socket, out ZMessage message, out ZError error)
        {
			return FrontendSocket.Forward(BackendSocket, out message, out error);
        }

        /// <summary>
        /// Not implemented for the <see cref="ForwarderDevice"/>.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected override bool BackendHandler(ZSocket args, out ZMessage message, out ZError error)
        {
			throw new NotSupportedException ();
        }
    }
}

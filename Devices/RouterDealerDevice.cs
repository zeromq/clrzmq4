namespace ZeroMQ.Devices
{
    /// <summary>
    /// A shared queue that collects requests from a set of clients and distributes
    /// these fairly among a set of services.
    /// </summary>
    /// <remarks>
    /// Requests are fair-queued from frontend connections and load-balanced between
    /// backend connections. Replies automatically return to the client that made the
    /// original request. This device is part of the request-reply pattern. The frontend
    /// speaks to clients and the backend speaks to services.
    /// </remarks>
    public class RouterDealerDevice : ZDevice
    {
        /// <summary>
        /// The frontend <see cref="ZSocketType"/> for a queue device.
        /// </summary>
		public static readonly ZSocketType FrontendType = ZSocketType.ROUTER;

        /// <summary>
        /// The backend <see cref="ZSocketType"/> for a queue device.
        /// </summary>
		public static readonly ZSocketType BackendType = ZSocketType.DEALER;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueDevice"/> class that will run in a
        /// self-managed thread.
        /// </summary>
        /// <param name="context">The <see cref="ZContext"/> to use when creating the sockets.</param>
        /// <param name="frontendBindAddr">The endpoint used to bind the frontend socket.</param>
        /// <param name="backendBindAddr">The endpoint used to bind the backend socket.</param>
		public RouterDealerDevice(ZContext context, string frontendBindAddr, string backendBindAddr)
            : base(context, FrontendType, BackendType)
        {
            FrontendSetup.Bind(frontendBindAddr);
            BackendSetup.Bind(backendBindAddr);
        }

        /// <summary>
        /// Forwards requests from the frontend socket to the backend socket.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected override bool FrontendHandler(ZSocket args, out ZMessage message, out ZError error)
        {
			return FrontendSocket.Forward(BackendSocket, out message, out error);
        }

        /// <summary>
        /// Forwards replies from the backend socket to the frontend socket.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected override bool BackendHandler(ZSocket args, out ZMessage message, out ZError error)
        {
			return BackendSocket.Forward(FrontendSocket, out message, out error);
        }
    }
}

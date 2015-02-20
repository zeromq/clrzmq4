namespace ZeroMQ
{
	/// <summary>
	/// A Device on Routers and Dealers
	/// </summary>
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
		/// Initializes a new instance of the <see cref="RouterDealerDevice"/> class that will run in a
		/// self-managed thread.
		/// </summary>
		public RouterDealerDevice(ZContext context, string frontendBindAddr, string backendBindAddr)
			: base(context, FrontendType, BackendType)
		{
			FrontendSetup.Bind(frontendBindAddr);
			BackendSetup.Bind(backendBindAddr);
		}

		/// <summary>
		/// Forwards requests from the frontend socket to the backend socket.
		/// </summary>
		protected override bool FrontendHandler(ZSocket args, out ZMessage message, out ZError error)
		{
			return FrontendSocket.Forward(BackendSocket, out message, out error);
		}

		/// <summary>
		/// Forwards replies from the backend socket to the frontend socket.
		/// </summary>
		protected override bool BackendHandler(ZSocket args, out ZMessage message, out ZError error)
		{
			return BackendSocket.Forward(FrontendSocket, out message, out error);
		}
	}
}
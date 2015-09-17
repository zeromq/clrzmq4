namespace ZeroMQ.Devices
{
	using System;

	/// <summary>
	/// Device for a Publisher and Subscribers
	/// </summary>
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
		/// Initializes a new instance of the <see cref="PubSubDevice"/> class.
		/// </summary>
		public PubSubDevice() : this(ZContext.Current) { }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PubSubDevice"/> class.
		/// </summary>
		public PubSubDevice(ZContext context)
			: base(context, FrontendType, BackendType)
		{
			BackendSetup.SubscribeAll();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PubSubDevice"/> class.
		/// </summary>
		public PubSubDevice(string frontendBindAddr, string backendBindAddr)
			: this(ZContext.Current, frontendBindAddr, backendBindAddr)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="PubSubDevice"/> class.
		/// </summary>
		public PubSubDevice(ZContext context, string frontendBindAddr, string backendBindAddr)
			: base(context, FrontendType, BackendType)
		{
			FrontendSetup.Bind(frontendBindAddr);
			BackendSetup.Bind(backendBindAddr);
			BackendSetup.SubscribeAll();
		}

		/// <summary>
		/// Forwards requests from the frontend socket to the backend socket.
		/// </summary>
		protected override bool FrontendHandler(ZSocket socket, out ZMessage message, out ZError error)
		{
			return FrontendSocket.Forward(BackendSocket, out message, out error);
		}

		/// <summary>
		/// PubSub Forwards the Subscription messages
		/// </summary>
		protected override bool BackendHandler(ZSocket args, out ZMessage message, out ZError error)
		{
			return BackendSocket.Forward(FrontendSocket, out message, out error);
		}
	}
}
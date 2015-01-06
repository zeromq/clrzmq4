namespace ZeroMQ.Devices
{
	using System;

	/// <summary>
	/// Collects tasks from a set of pushers and forwards these to a set of pullers.
	/// </summary>
	/// <remarks>
	/// Generally used to bridge networks. Messages are fair-queued from pushers and
	/// load-balanced to pullers. This device is part of the pipeline pattern. The
	/// frontend speaks to pushers and the backend speaks to pullers.
	/// </remarks>
	public class PushPullDevice : ZDevice
	{
		/// <summary>
		/// The frontend <see cref="ZSocketType"/> for a streamer device.
		/// </summary>
		public static readonly ZSocketType FrontendType = ZSocketType.PULL;

		/// <summary>
		/// The backend <see cref="ZSocketType"/> for a streamer device.
		/// </summary>
		public static readonly ZSocketType BackendType = ZSocketType.PUSH;

		/// <summary>
		/// Initializes a new instance of the <see cref="StreamerDevice"/> class.
		/// </summary>
		/// <param name="context">The <see cref="ZContext"/> to use when creating the sockets.</param>
		/// <param name="frontendBindAddr">The endpoint used to bind the frontend socket.</param>
		/// <param name="backendBindAddr">The endpoint used to bind the backend socket.</param>
		/// <param name="mode">The <see cref="DeviceMode"/> for the current device.</param>
		public PushPullDevice(ZContext context, string frontendBindAddr, string backendBindAddr)
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
		/// Not implemented for the <see cref="StreamerDevice"/>.
		/// </summary>
		/// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected override bool BackendHandler(ZSocket args, out ZMessage message, out ZError error)
		{
			throw new NotSupportedException();
		}
	}
}
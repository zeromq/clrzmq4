namespace ZeroMQ.Monitoring
{
	using System;

	/// <summary>
	/// Socket transport events (for TCP and IPC sockets) that can be monitored.
	/// </summary>
	[Flags]
	public enum ZMonitorEvents
	{
		/// <summary>
		/// Triggered when a connection has been established to a remote peer.
		/// </summary>
		Connected = 1,

		/// <summary>
		/// Triggered when an immediate connection attempt is delayed and it's completion is being polled for.
		/// </summary>
		ConnectDelayed = 2,

		/// <summary>
		/// Triggered when a connection attempt is being handled by reconnect timer. The reconnect interval is recomputed for each attempt.
		/// </summary>
		ConnectRetried = 4,

		/// <summary>
		/// Triggered when a socket is successfully bound to a an interface.
		/// </summary>
		Listening = 8,

		/// <summary>
		/// Triggered when a socket could not bind to a given interface.
		/// </summary>
		BindFailed = 16,

		/// <summary>
		/// Triggered when a connection from a remote peer has been established with a socket's listen address.
		/// </summary>
		Accepted = 32,

		/// <summary>
		/// Triggered when a connection attempt to a socket's bound address fails.
		/// </summary>
		AcceptFailed = 64,

		/// <summary>
		/// Triggered when a connection's underlying descriptor has been closed.
		/// </summary>
		Closed = 128,

		/// <summary>
		/// Triggered when a descriptor could not be released back to the OS.
		/// </summary>
		CloseFailed = 256,

		/// <summary>
		/// Triggered when the stream engine (tcp and ipc specific) detects a corrupted / broken session.
		/// </summary>
		Disconnected = 512,

		/// <summary>
		/// Monitoring on this socket ended.
		/// </summary>
		Stopped = 1024,

		/// <summary>
		/// Any <see cref="ZMonitorEvents"/> event, maybe readable from EventValue.
		/// </summary>
		AllEvents = 0xFFFF
	}
}
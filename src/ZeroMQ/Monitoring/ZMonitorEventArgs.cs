namespace ZeroMQ.Monitoring
{
	using System;

	/// <summary>
	/// A base class for the all ZmqMonitor events.
	/// </summary>
	public class ZMonitorEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ZMonitorEventArgs"/> class.
		/// </summary>
		/// <param name="monitor">The <see cref="ZMonitor"/> that triggered the event.</param>
		/// <param name="address">The peer address.</param>
		public ZMonitorEventArgs(ZMonitor monitor, ZMonitorEventData ed)
		{
			this.Monitor = monitor;
			this.Event = ed;
		}

		public ZMonitorEventData Event { get; private set; }

		/// <summary>
		/// Gets the monitor that triggered the event.
		/// </summary>
		public ZMonitor Monitor { get; private set; }
	}
}
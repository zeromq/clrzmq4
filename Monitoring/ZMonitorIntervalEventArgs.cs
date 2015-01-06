namespace ZeroMQ.Monitoring
{
	/// <summary>
	/// Provides data for <see cref="ZMonitor.ConnectRetried"/> event.
	/// </summary>
	public class ZMonitorIntervalEventArgs : ZMonitorEventArgs
	{
		internal ZMonitorIntervalEventArgs(ZMonitor monitor, ZMonitorEventData data)
			: base(monitor, data)
		{
			this.Interval = data.EventValue;
		}

		/// <summary>
		/// Gets the computed reconnect interval.
		/// </summary>
		public int Interval { get; private set; }
	}
}
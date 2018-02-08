namespace ZeroMQ.Monitoring
{
	using System;

	public struct ZMonitorEventData
	{
		public ZMonitorEvents Event;
		public Int32 EventValue;
		public String Address;
	}
}
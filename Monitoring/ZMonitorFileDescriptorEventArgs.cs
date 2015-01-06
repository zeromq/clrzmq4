namespace ZeroMQ.Monitoring
{
	using System;
	using lib;

	/// <summary>
	/// Provides data for <see cref="ZMonitor.Connected"/>, <see cref="ZMonitor.Listening"/>, <see cref="ZMonitor.Accepted"/>, <see cref="ZMonitor.Closed"/> and <see cref="ZMonitor.Disconnected"/> events.
	/// </summary>
	public class ZMonitorFileDescriptorEventArgs : ZMonitorEventArgs
	{
		internal ZMonitorFileDescriptorEventArgs(ZMonitor monitor, ZMonitorEventData data)
			: base(monitor, data)
		{
			if (Platform.Kind == PlatformKind.Posix)
			{
				this.FileDescriptor_Posix = data.EventValue;
			}
			else if (Platform.Kind == PlatformKind.Win32)
			{
				this.FileDescriptor_Windows = new IntPtr(data.EventValue);
			}
			else
			{
				throw new PlatformNotSupportedException();
			}
		}

		/// <summary>
		/// Gets the monitor descriptor (Posix)
		/// </summary>
		public int FileDescriptor_Posix { get; private set; }

		/// <summary>
		/// Gets the monitor descriptor (Windows)
		/// </summary>
		public IntPtr FileDescriptor_Windows { get; private set; }
	}
}
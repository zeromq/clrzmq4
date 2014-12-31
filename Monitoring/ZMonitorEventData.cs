namespace ZeroMQ.Monitoring
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ZMonitorEventData
    {
        public ZMonitorEvents Event;

        public Int32 EventValue;

        public String Address;
    }
}
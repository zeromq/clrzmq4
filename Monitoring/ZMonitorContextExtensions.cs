namespace ZeroMQ.Monitoring
{
    using System;

    /// <summary>
    /// Defines extension methods related to monitoring for <see cref="ZmqContext"/> instances.
    /// </summary>
    public static class ZMonitorContextExtensions
    {
        /// <summary>
        /// Creates a <see cref="ZMonitor"/> (a <see cref="ZmqSocketType.PAIR"/> socket) and connects
        /// it to the specified inproc monitoring endpoint.
        /// </summary>
        public static ZMonitor CreateMonitorSocket(this ZContext context, string endpoint, out ZError error)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentException("Unable to monitor to a null or empty endpoint.", "endpoint");
            }

            return ZMonitor.CreateMonitor(context, endpoint, out error);
        }
    }
}

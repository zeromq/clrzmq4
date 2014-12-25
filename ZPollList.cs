namespace ZeroMQ
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using lib;

    /// <summary>
    /// Multiplexes input/output events in a level-triggered fashion over a set of sockets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sockets will be polled according to their capabilities. For example, sockets that are
    /// receive-only (e.g., PULL and SUB sockets) will only poll for Input events. Sockets that
    /// can both send and receive (e.g., REP, REQ, etc.) will poll for both Input and Output events.
    /// </para>
    /// <para>
    /// To actually send or receive data, the socket's <see cref="ZSocket.ReceiveReady"/> and/or
    /// <see cref="ZSocket.SendReady"/> event handlers must be attached to. If attached, these will
    /// be invoked when data is ready to be received or sent.
    /// </para>
    /// </remarks>
    public static class ZPollList // : IDisposable, IList<ZmqPollItem>
    {

		// private List<ZmqPollItem> pollList;

		/*/ <summary>
		/// Initializes a new instance of the <see cref="Poller"/> class.
		/// </summary>
		public ZmqPoller()
			: this (Enumerable.Empty<ZmqPollItem>())
		{ }

		public ZmqPoller(ZmqPollItem socketToPoll)
			: this()
		{
			pollList.Add(socketToPoll);
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Poller"/> class with a collection of sockets to poll over.
        /// </summary>
        /// <param name="socketsToPoll">The collection of <see cref="ZmqSocket"/>s to poll.</param>
        public ZmqPoller(IEnumerable<ZmqPollItem> socketsToPoll)
        {
			// Pulse = new AutoResetEvent(false);
			pollList = new List<ZmqPollItem> (socketsToPoll);
        }

        /// <summary>
        /// Gets an <see cref="AutoResetEvent"/> that is pulsed after every Poll call.
        /// </summary>
        // public AutoResetEvent Pulse { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the object is being disposed, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Pulse.Dispose();
            }
        } */

    }
}

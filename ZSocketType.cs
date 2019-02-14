using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZeroMQ
{
	public enum ZSocketType : int
	{
		None = -1,

		/// <summary>
		/// Exclusive Pair
		/// </summary>
		PAIR,

		/// <summary>
		/// Publish
		/// </summary>
		PUB,

		/// <summary>
		/// Subscribe
		/// </summary>
		SUB,

		/// <summary>
		/// Request
		/// </summary>
		REQ,

		/// <summary>
		/// Reply / Response
		/// </summary>
		REP,

		/// <summary>
		/// Dealer
		/// </summary>
		DEALER,

		/// <summary>
		/// Router
		/// </summary>
		/// <remarks>
		/// When receiving messages a <see cref="ROUTER"/> socket shall prepend a message
		/// part containing the identity of the originating peer to the message before
		/// passing it to the application. When sending messages a ZMQ_ROUTER socket shall remove
		/// the first part of the message and use it to determine the identity of the peer the message
		/// shall be routed to. If the peer does not exist anymore the message shall be silently discarded.
		/// </remarks>
		ROUTER,

		/// <summary>
		/// Pull
		/// </summary>
		PULL,

		/// <summary>
		/// Push
		/// </summary>
		PUSH,

		/// <summary>
		/// XPublisher
		/// </summary>
		/// <remarks>
		/// Subscription message is a byte '1' (for subscriptions) or byte '0' (for unsubscriptions) followed by the subscription body.
		/// </remarks>
		XPUB,

		/// <summary>
		/// XSubscriber
		/// </summary>
		/// <remarks>
		/// Subscription message is a byte '1' (for subscriptions) or byte '0' (for unsubscriptions) followed by the subscription body.
		/// </remarks>
		XSUB,

		/// <summary>
		/// Stream
		/// </summary>
		/// <remarks>
		/// </remarks>
		STREAM
	}
}
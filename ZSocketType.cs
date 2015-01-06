using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZeroMQ
{
	public class ZSocketType : ZSymbol
	{
		static ZSocketType()
		{
			var one = ZSymbol.None;
		}

		public static class Code
		{
			public const int
				PAIR = 0,
				PUB = 1,
				SUB = 2,
				REQ = 3,
				REP = 4,
				DEALER = 5,
				ROUTER = 6,
				PULL = 7,
				PUSH = 8,
				XPUB = 9,
				XSUB = 10,
				STREAM = 11;
		}

		public ZSocketType(int errno)
			: base(errno)
		{ }

		public ZSocketType(int errno, string errname, string errtext)
			: base(errno, errname, errtext)
		{ }

		public new static readonly ZSocketType None = default(ZSocketType);

		public static readonly ZSocketType

			/// <summary>
			/// Can only be connected to a single peer at any one time.
			/// Part of the Exclusive Pair pattern.
			/// </summary>
			PAIR,

			/// <summary>
			/// Used by a publisher to distribute messages in a fan out fashion to all connected peers.
			/// Part of the Publish-Subscribe pattern.
			/// </summary>
			PUB,

			/// <summary>
			/// Used by a subscriber to subscribe to data distributed by a publisher.
			/// Part of the Publish-Subscribe pattern.
			/// </summary>
			SUB,

			/// <summary>
			/// Used by a client to send requests to and receive replies from a service.
			/// Part of the Request-Reply pattern.
			/// </summary>
			REQ,

			/// <summary>
			/// Used by a service to receive requests from and send replies to a client.
			/// Part of the Request-Reply pattern.
			/// </summary>
			REP,

			/// <summary>
			/// Used for extending request/reply sockets. Each message sent is round-robined
			/// among all connected peers, and each message received is fair-queued from all connected peers.
			/// </summary>
			DEALER,

			/// <summary>
			/// Used for extending request/reply sockets. Messages received are fair-queued
			/// from among all connected peers.
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
			/// Used by a pipeline node to receive messages from upstream pipeline nodes.
			/// Part of the Pipeline pattern.
			/// </summary>
			PULL,

			/// <summary>
			/// Used by a pipeline node to send messages to downstream pipeline nodes.
			/// Part of the Pipeline pattern.
			/// </summary>
			PUSH,

			/// <summary>
			/// Same as <see cref="PUB"/> except subscriptions can be received from peers as incoming messages.
			/// Part of the Publish-Subscribe pattern.
			/// </summary>
			/// <remarks>
			/// Subscription message is a byte '1' (for subscriptions) or byte '0' (for unsubscriptions) followed by the subscription body.
			/// </remarks>
			XPUB,

			/// <summary>
			/// Same as <see cref="SUB"/> except subscription messages can be sent to the publisher.
			/// Part of the Publish-Subscribe pattern.
			/// </summary>
			/// <remarks>
			/// Subscription message is a byte '1' (for subscriptions) or byte '0' (for unsubscriptions) followed by the subscription body.
			/// </remarks>
			XSUB,

			/// <summary>
			/// STREAM socket type provides a native "ROUTER-like" TCP socket type.
			/// </summary>
			/// <remarks>
			/// </remarks>
			STREAM
		;
	}
}
namespace ZeroMQ.Devices
{
	using lib;
	// using lib.sys;

	using System;
	using System.Net;
	using System.Threading;

	/// <summary>
	/// The Stream to Dealer is a Device for reading 
	/// and sending REPlies to TCP
	/// </summary>
	public class StreamDealerDevice : ZDevice
	{
		/// <summary>
		/// The frontend <see cref="ZSocketType"/> for a queue device.
		/// </summary>
		public static readonly ZSocketType FrontendType = ZSocketType.STREAM;

		/// <summary>
		/// The backend <see cref="ZSocketType"/> for a queue device.
		/// </summary>
		public static readonly ZSocketType BackendType = ZSocketType.DEALER;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamDealerDevice"/> class.
		/// </summary>
		public StreamDealerDevice() : this(ZContext.Current) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamDealerDevice"/> class.
		/// </summary>
		public StreamDealerDevice(ZContext context)
			: base(context, FrontendType, BackendType)
		{ }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamDealerDevice"/> class.
		/// </summary>
		public StreamDealerDevice(string frontendBindAddr, string backendBindAddr)
			: this(ZContext.Current, frontendBindAddr, backendBindAddr)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="StreamDealerDevice"/> class.
		/// </summary>
		public StreamDealerDevice(ZContext context, string frontendBindAddr, string backendBindAddr)
			: base(context, FrontendType, BackendType)
		{
			FrontendSetup.Bind(frontendBindAddr);
			BackendSetup.Bind(backendBindAddr);
		}

		/// <summary>
		/// Forwards requests from the frontend socket to the backend socket.
		/// </summary>
		protected override bool FrontendHandler(ZSocket sock, out ZMessage message, out ZError error)
		{
			error = default(ZError);
			message = null;

			// receiving scope
			// STREAM: get 2 frames, identity and body
			ZMessage incoming = null;
			// IPAddress address = null;
			string address;
			if (!ReceiveMsg(sock, ref incoming, out address, out error))
			{
				return false;
			}

			// sending scope
			// DEALER: forward
			using (incoming)
			{
				if (incoming[1].Length == 0)
				{
					return true; // Ignore the Empty one
				}

				// Prepend empty delimiter between Identity frame and Data frame
				incoming.Insert(1, new ZFrame());

				// Prepend Peer-Address
				incoming.Insert(2, new ZFrame(address));

				if (!BackendSocket.Send(incoming, /* ZSocketFlags.DontWait, */ out error))
				{
					return false;
				}
				incoming.Dismiss();
			}

			return true;
		}

		static bool ReceiveMsg(ZSocket sock, ref ZMessage message, out string address, out ZError error)
		{
			error = ZError.None;
			// address = IPAddress.None;
			address = string.Empty;

			// STREAM: read frames: identity, body

			// read the ip4 address from (ZFrame)frame.GetOption("Peer-Address")

			int receiveCount = 2;
			do
			{
				var frame = ZFrame.CreateEmpty();

				while (-1 == zmq.msg_recv(frame.Ptr, sock.SocketPtr, (int)(/* ZSocketFlags.DontWait | */ ZSocketFlags.More)))
				{
					error = ZError.GetLastErr();

					if (error == ZError.EINTR) 
					{
						error = default(ZError);
						continue;
					}

					frame.Dispose();
					return false;
				}

				if (message == null)
				{
					message = new ZMessage();
				}
				message.Add(frame);

				if (receiveCount == 2)
				{
					if (default(string) == (address = frame.GetOption("Peer-Address", out error)))
					{
						// just ignore
						error = default(ZError);
						address = string.Empty;
					}
				}

			} while (--receiveCount > 0);

			return true;
		}


		/// <summary>
		/// Forwards replies from the backend socket to the frontend socket.
		/// </summary>
		protected override bool BackendHandler(ZSocket sock, out ZMessage message, out ZError error)
		{
			error = default(ZError);
			message = null;

			// receiving scope
			// DEALER: normal movemsg
			ZMessage incoming = null;
			if (!sock.ReceiveMessage(ref incoming, /* ZSocketFlags.DontWait */ ZSocketFlags.None, out error))
			{
				return false;
			}

			using (incoming)
			{
				// STREAM: write frames: identity, body, identity, empty
				// Read identity
				int ic = (int)incoming[0].Length;
				var identityBytes = new byte[ic];
				incoming[0].Read(identityBytes, 0, ic); 

				// Remove DEALER's delimiter
				incoming.RemoveAt(1);

				// Append Identity frame
				var identity0 = new ZFrame(identityBytes);
				incoming.Add(identity0);

				// Append STREAM's empty delimiter frame
				incoming.Add(new ZFrame());

				if (!SendMsg(FrontendSocket, incoming, out error))
				{
					return false;
				}
			}

			return true;
		}

		static bool SendMsg(ZSocket sock, ZMessage msg, out ZError error)
		{
			error = ZError.None;

			foreach (ZFrame frame in msg)
			{
				while (-1 == zmq.msg_send(frame.Ptr, sock.SocketPtr, (int)(/* ZSocketFlags.DontWait | */ ZSocketFlags.More)))
				{
					error = ZError.GetLastErr();

					if (error == ZError.EINTR)
					{
						error = default(ZError);
						continue;
					}
					/* if (error == ZError.EAGAIN)
					{
						error = default(ZError);
						Thread.Sleep(1);

						continue;
					} */

					return false;
				}
			}

			msg.Dismiss();
			return true;
		}
	}
}
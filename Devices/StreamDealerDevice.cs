namespace ZeroMQ.Devices
{
	using lib;

	using System;
	using System.Threading;

	/// <summary>
	/// </summary>
	/// <remarks>
	/// </remarks>
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
		/// Initializes a new instance of the <see cref="QueueDevice"/> class.
		/// </summary>
		/// <param name="context">The <see cref="ZContext"/> to use when creating the sockets.</param>
		/// <param name="frontendBindAddr">The endpoint used to bind the frontend socket.</param>
		/// <param name="backendBindAddr">The endpoint used to bind the backend socket.</param>
		/// <param name="mode">The <see cref="DeviceMode"/> for the current device.</param>
		public StreamDealerDevice(ZContext context, string frontendBindAddr, string backendBindAddr)
			: base(context, FrontendType, BackendType)
		{
			FrontendSetup.Bind(frontendBindAddr);
			BackendSetup.Bind(backendBindAddr);
		}

		/// <summary>
		/// Forwards requests from the frontend socket to the backend socket.
		/// </summary>
		/// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected override bool FrontendHandler(ZSocket sock, out ZMessage message, out ZError error)
		{
			error = default(ZError);
			message = null;

			// receiving scope
			// STREAM: get 2 frames, identity and body
			ZMessage incoming = null;
			if (!ReceiveMsg(sock, 2, ref incoming, out error))
			{
				return false;
			}

			// string ip = incoming[0].GetOption("RemoteAddress");

			// always more = ReceiveMore;

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

				// Prepend Z_LAST_ENDPOINT
				// incoming.Insert(2, new ZFrame(incoming[0].GetOption("Peer-Address")));

				while (!BackendSocket.Send(incoming, ZSocketFlags.DontWait, out error))
				{
					return false;
				}

				incoming.Dismiss();
			}

			return true;
		}

		static bool ReceiveMsg(ZSocket sock, int receiveCount, ref ZMessage message, out ZError error)
		{
			error = ZError.None;

			do
			{
				var frame = ZFrame.CreateEmpty();

				while (-1 == zmq.msg_recv(frame.Ptr, sock.SocketPtr, (int)(ZSocketFlags.DontWait | ZSocketFlags.More)))
				{
					error = ZError.GetLastErr();

					if (error == ZError.EINTR)
						continue;

					frame.Dispose();

					return false;
				}

				if (message == null)
				{
					message = new ZMessage();
				}
				message.Add(frame);

			} while (--receiveCount > 0);

			return true;
		}


		/// <summary>
		/// Forwards replies from the backend socket to the frontend socket.
		/// </summary>
		/// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected override bool BackendHandler(ZSocket sock, out ZMessage message, out ZError error)
		{
			error = default(ZError);
			message = null;

			// receiving scope
			// DEALER: normal movemsg
			ZMessage incoming = null;
			if (!sock.ReceiveMessage(ZSocketFlags.DontWait, ref incoming, out error))
			{
				return false;
			}

			// STREAM: write frames: identity, body, identity, empty
			// Read identity
			int ic = (int)incoming[0].Length;
			var identityBytes = new byte[ic];
			incoming[0].Read(identityBytes, 0, ic); 

			// Remove DEALER's delimiter
			using (ZFrame delim = incoming[1])
			{
				incoming.RemoveAt(1);
			}

			// Append Identity frame
			var identity0 = new ZFrame(identityBytes);
			incoming.Add(identity0);

			// Append STREAM's empty delimiter frame
			incoming.Add(new ZFrame());

			if (!SendMsg(FrontendSocket, incoming, out error))
			{
				return false;
			}

			return true;
		}

		static bool SendMsg(ZSocket sock, ZMessage msg, out ZError error)
		{
			error = ZError.None;

			foreach (ZFrame frame in msg)
			{
				while (-1 == zmq.msg_send(frame.Ptr, sock.SocketPtr, (int)(ZSocketFlags.DontWait | ZSocketFlags.More)))
				{
					error = ZError.GetLastErr();

					if (error == ZError.EINTR)
					{
						continue;
					}
					/* if (error == ZError.EAGAIN)
					{
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
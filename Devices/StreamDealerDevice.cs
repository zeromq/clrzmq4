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
			bool result = false;
			// bool more = true;

			do { // breakable, don't continue

				// receiving scope
				// STREAM: get 2 frames, identity and body
				ZMessage incoming;
				while (!(result = (null != (incoming = ReceiveMsg(sock, 2, ZSocketFlags.DontWait, out error ))))) {

					if (error == ZError.EAGAIN) {
						error = default(ZError);

						Thread.Yield();
						// Thread.Sleep(0);
						continue;
					}
					if (error == ZError.ETERM) {
						break;
					}
					throw new ZException (error);
				}
				if (error == ZError.ETERM) {
					break;
				}
				if (!result) {
					break;
				}

				// will have to receive more?
				// always more = ReceiveMore;

				// sending scope
				// DEALER: forward
				using (incoming) { 

					// Prepend empty delimiter between Identity frame and Data frame
					incoming.Insert(1, ZFrame.Create(0));
					
					// Prepend Z_LAST_ENDPOINT
					incoming.Insert(2, ZFrame.CreateFromString(sock.LastEndpoint));

					// Prepend empty delimiter
					// incoming.Insert(0, ZFrame.Create(0));

					/* Prepend my Identity frame
					byte[] myIdentity = sock.Identity;
					var identity0 = ZFrame.Create(myIdentity.Length);
					identity0.Write(myIdentity, 0, myIdentity.Length);
					incoming.Insert(0, identity0); */
					
					/* Prepend empty delimiter
					incoming.Insert(0, ZFrame.Create(0)); /**/

					// Read Identity
					/* int identityLength = (int)incoming[0].Length;
					var identity = new byte[identityLength];
					incoming [0].Read(identity, 0, identityLength); */

					while (!(result = BackendSocket.SendMessage( incoming, ZSocketFlags.DontWait, out error ))) {

						if (error == ZError.EAGAIN) {
							error = default(ZError);
							// Thread.Yield();
							Thread.Sleep(0);

							continue;
						}
						if (error == ZError.ETERM) {
							break;
						}
						throw new ZException (error);
					}
					if (result) {
						incoming.Dismiss();
					}
					/* else {
						// error = sndErr;
						break;
					} */
				}

			} while (false); // (result && more);

			return result;
        }
		
		static ZMessage ReceiveMsg(ZSocket sock, int receiveCount, ZSocketFlags flags, out ZError error)
		{
			bool result = false;
			error = ZError.None;
			flags |= ZSocketFlags.More; // always more on STREAM sock

			var message = new ZMessage ();
			do {
				var frame = ZFrame.CreateEmpty();

				while (!(result = (-1 != zmq.msg_recv(frame.Ptr, sock.SocketPtr, (int)flags)))) {
					error = ZError.GetLastErr();

					if (error == ZError.EINTR) {
						// if (--retry > -1)
						error = default(ZError);
						continue;
					}
					if (error == ZError.EAGAIN) {
						error = default(ZError);
						// Thread.Yield();
						Thread.Sleep(0);

						continue;
					}

					frame.Dispose();

					if (error == ZError.ETERM) {
						break;
					}

					throw new ZException (error);
				}
				if (result) {
					message.Add(frame);

				}

			} while (result && --receiveCount > 0);

			return message;
		}


        /// <summary>
        /// Forwards replies from the backend socket to the frontend socket.
        /// </summary>
        /// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected override bool BackendHandler(ZSocket sock, out ZMessage message, out ZError error)
		{
			error = default(ZError);
			message = null;
			bool result = false;
			// bool more = true;

			do { // breakable, don't continue

				// receiving scope
				// DEALER: normal movemsg
				ZMessage incoming;
				{ 
					while (!(result = (null != (incoming = sock.ReceiveMessage( ZSocketFlags.More | ZSocketFlags.DontWait, out error ))))) {

						if (error == ZError.EAGAIN) {
							error = default(ZError);
							// Thread.Yield();
							Thread.Sleep(0);

							continue;
						}
						if (error == ZError.ETERM) {
							break;
						}
						throw new ZException (error);
					}
					if (error == ZError.ETERM) {
						break;
					}
					if (!result) {
						// error = rcvErr;
						break;
					}
				}

				// sending scope
				// STREAM: write frames: identity, body, identity, empty
				{

					// Read identity
					int ic = (int)incoming [0].Length;
					var identityBytes = new byte[ic];
					incoming [0].Read(identityBytes, 0, ic);

					// Remove DEALER's delimiter
					using (ZFrame delim = incoming[1]) {
						incoming.RemoveAt(1);
					}

					// Append Identity frame
					var identity0 = ZFrame.Create(identityBytes.Length);
					identity0.Write(identityBytes, 0, identityBytes.Length);
					incoming.Add(identity0);

					// Append STREAM's empty delimiter frame
					incoming.Add(ZFrame.Create(0));

					while (!(result = SendMsg(FrontendSocket, incoming, ZSocketFlags.DontWait, out error ))) {

						if (error == ZError.EAGAIN) {
							error = default(ZError);
							// Thread.Yield();
							Thread.Sleep(0);

							continue;
						}
						if (error == ZError.ETERM) {
							break;
						}
						throw new ZException (error);
					}
					if (error == ZError.ETERM) {
						break;
					}
					if (!result) {
						// error = sndErr;
						break;
					}
				}

			} while (false); //  (result && more);

			return result;
		}

		static bool SendMsg(ZSocket sock, ZMessage msg, ZSocketFlags flags, out ZError error) 
		{
			error = ZError.None;
			bool result = false;
			flags |= ZSocketFlags.More; // always more on STREAM socket

			using (msg) {
				foreach (ZFrame frame in msg) {
					// int retry = 4;
					while (!(result = (-1 != zmq.msg_send(frame.Ptr, sock.SocketPtr, (int)flags)))) {
						error = ZError.GetLastErr();

						if (error == ZError.EINTR) {
							// if (--retry > -1)
							error = ZError.None;
							continue;
						}
						if (error == ZError.EAGAIN) {
							error = ZError.None;
                            // Thread.Yield();
                            Thread.Sleep(0);

							continue;
						}
						if (error == ZError.ETERM) {
							break;
						} 

						throw new ZException (error);
					}
					if (!result) {
						throw new InvalidOperationException ();
					}
					// Tell IDisposable to not unallocate Z_msg
					// frame.Dismiss();
				}

				msg.Dismiss();
			}

			return result;
		}
    }
}

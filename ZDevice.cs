namespace ZeroMQ
{
	using System;
	using System.Threading;
	using System.Collections.Generic;

	/// <summary>
	/// Forwards messages received by a front-end socket to a back-end socket, from which
	/// they are then sent.
	/// </summary>
	/// <remarks>
	/// The base implementation of <see cref="ZDevice"/> is <b>not</b> threadsafe. Do not construct
	/// a device with sockets that were created in separate threads or separate contexts.
	/// </remarks>
	public abstract class ZDevice : ZThread
	{
		/// <summary>
		/// The polling interval in milliseconds.
		/// </summary>
		protected readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(500);

		/// <summary>
		/// The ZContext reference, to not become finalized
		/// </summary>
		protected readonly ZContext Context;

		/// <summary>
		/// The frontend socket that will normally pass messages to <see cref="BackendSocket"/>.
		/// </summary>
		public ZSocket FrontendSocket;

		/// <summary>
		/// The backend socket that will normally receive messages from (and possibly send replies to) <see cref="FrontendSocket"/>.
		/// </summary>
		public ZSocket BackendSocket;

		/// <summary>
		/// You are using ZContext.Current!
		/// </summary>
		protected ZDevice()
			: this (ZContext.Current)
		{ }

		protected ZDevice(ZContext context)
			: base()
		{
			Context = context;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZDevice"/> class.
		/// You are using ZContext.Current!
		/// </summary>
		/// <param name="frontendSocket">
		/// A <see cref="ZSocket"/> that will pass incoming messages to <paramref name="backendSocket"/>.
		/// </param>
		/// <param name="backendSocket">
		/// A <see cref="ZSocket"/> that will receive messages from (and optionally send replies to) <paramref name="frontendSocket"/>.
		/// </param>
		/// <param name="mode">The <see cref="DeviceMode"/> for the current device.</param>
		protected ZDevice(ZSocketType frontendType, ZSocketType backendType)
			: this (ZContext.Current, frontendType, backendType)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ZDevice"/> class.
		/// </summary>
		/// <param name="frontendSocket">
		/// A <see cref="ZSocket"/> that will pass incoming messages to <paramref name="backendSocket"/>.
		/// </param>
		/// <param name="backendSocket">
		/// A <see cref="ZSocket"/> that will receive messages from (and optionally send replies to) <paramref name="frontendSocket"/>.
		/// </param>
		/// <param name="mode">The <see cref="DeviceMode"/> for the current device.</param>
		protected ZDevice(ZContext context, ZSocketType frontendType, ZSocketType backendType)
			: base()
		{
			Context = context;

			ZError error;
			if (!Initialize(frontendType, backendType, out error))
			{
				throw new ZException(error);
			}
		}

		protected virtual bool Initialize(ZSocketType frontendType, ZSocketType backendType, out ZError error)
		{
			error = default(ZError);

			/* if (frontendType == ZSocketType.None && backendType == ZSocketType.None)
			{
				throw new InvalidOperationException();
			} /**/

			if (frontendType != ZSocketType.None)
			{
				if (null == (FrontendSocket = ZSocket.Create(Context, frontendType, out error)))
				{
					return false;
				}
				FrontendSetup = new ZSocketSetup(FrontendSocket);
			}

			if (backendType != ZSocketType.None)
			{
				if (null == (BackendSocket = ZSocket.Create(Context, backendType, out error)))
				{
					return false;
				}
				BackendSetup = new ZSocketSetup(BackendSocket);
			}

			return true;
		}

		/// <summary>
		/// Gets a <see cref="ZSocketSetup"/> for configuring the frontend socket.
		/// </summary>
		public ZSocketSetup BackendSetup { get; protected set; }

		/// <summary>
		/// Gets a <see cref="ZSocketSetup"/> for configuring the backend socket.
		/// </summary>
		public ZSocketSetup FrontendSetup { get; protected set; }

		/*/ <summary>
		/// Gets a <see cref="ManualResetEvent"/> that can be used to block while the device is running.
		/// </summary>
		public ManualResetEvent DoneEvent { get; private set; } /**/

		/*/ <summary>
		/// Gets an <see cref="AutoResetEvent"/> that is pulsed after every Poll call.
		/// </summary>
		public AutoResetEvent PollerPulse
		{
				get { return _poller.Pulse; }
		}*/

		/// <summary>
		/// Initializes the frontend and backend sockets. Called automatically when starting the device.
		/// If called multiple times, will only execute once.
		/// </summary>
		public virtual void Initialize()
		{
			EnsureNotDisposed();

			if (FrontendSetup != null) FrontendSetup.Configure();
			if (BackendSetup != null) BackendSetup.Configure();
		}

		/// <summary>
		/// Start the device in the current thread. Should be used by implementations of the <see cref="DeviceRunner.Start"/> method.
		/// </summary>
		/// <remarks>
		/// Initializes the sockets prior to starting the device with <see cref="Initialize"/>.
		/// </remarks>
		protected override void Run()
		{
			EnsureNotDisposed();

			Initialize();

			ZSocket[] sockets;
			ZPollItem[] polls;
			if (FrontendSocket != null && BackendSocket != null)
			{
				sockets = new ZSocket[] {
					FrontendSocket,
					BackendSocket
				};
				polls = new ZPollItem[] {
					ZPollItem.Create(FrontendHandler),
					ZPollItem.Create(BackendHandler)
				};
			}
			else if (FrontendSocket != null)
			{
				sockets = new ZSocket[] {
					FrontendSocket
				}; 
				polls = new ZPollItem[] {
					ZPollItem.Create(FrontendHandler)
				};
			}
			else
			{
				sockets = new ZSocket[] {
					BackendSocket
				};
				polls = new ZPollItem[] {
					ZPollItem.Create(BackendHandler)
				};
			}

			/* ZPollItem[] polls;
			{
				var pollItems = new List<ZPollItem>();
				switch (FrontendSocket.SocketType)
				{
					case ZSocketType.Code.ROUTER:
					case ZSocketType.Code.XSUB:
					case ZSocketType.Code.PUSH:
						// case ZSocketType.Code.STREAM:
						pollItems.Add(new ZPollItem(FrontendSocket, ZPoll.In)
						{
								ReceiveMessage = FrontendHandler
						});

						break;
				}
				switch (BackendSocket.SocketType)
				{
					case ZSocketType.Code.DEALER:
						// case ZSocketType.Code.STREAM:
						pollItems.Add(new ZPollItem(BackendSocket, ZPoll.In)
						{
								ReceiveMessage = BackendHandler
						});

						break;
				}
				polls = pollItems.ToArray();
			} */

			// Because of using ZmqSocket.Forward, this field will always be null
			ZMessage[] lastMessageFrames = null;

			if (FrontendSetup != null) FrontendSetup.BindConnect();
			if (BackendSetup != null) BackendSetup.BindConnect();

			bool isValid = false;
			var error = default(ZError);
			try
			{
				while (!Cancellor.IsCancellationRequested)
				{

					if (!(isValid = sockets.Poll(polls, ZPoll.In, ref lastMessageFrames, out error, PollingInterval)))
					{

						if (error == ZError.EAGAIN)
						{
							Thread.Sleep(1);
							continue;
						}
						if (error == ZError.ETERM)
						{
							break;
						}

						// EFAULT
						throw new ZException(error);
					}
				}
			}
			catch (ZException)
			{
				// Swallow any exceptions thrown while stopping
				if (!Cancellor.IsCancellationRequested)
				{
					throw;
				}
			}

			if (FrontendSetup != null) FrontendSetup.UnbindDisconnect();
			if (BackendSetup != null) BackendSetup.UnbindDisconnect();

			if (error == ZError.ETERM)
			{
				Close();
			}
		}

		/// <summary>
		/// Invoked when a message has been received by the frontend socket.
		/// </summary>
		/// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected abstract bool FrontendHandler(ZSocket socket, out ZMessage message, out ZError error);

		/// <summary>
		/// Invoked when a message has been received by the backend socket.
		/// </summary>
		/// <param name="args">A <see cref="SocketEventArgs"/> object containing the poll event args.</param>
		protected abstract bool BackendHandler(ZSocket args, out ZMessage message, out ZError error);

		/// <summary>
		/// Stops the device and releases the underlying sockets. Optionally disposes of managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (FrontendSocket != null) FrontendSocket.Dispose();
				if (BackendSocket != null) BackendSocket.Dispose();
			}

			base.Dispose(disposing);
		}

	}
}
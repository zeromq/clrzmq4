namespace ZeroMQ.Devices
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
	public abstract class ZDevice : IZDevice
	{
		/// <summary>
		/// The polling interval in milliseconds.
		/// </summary>
		protected readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(150);

		/// <summary>
		/// The ZmqContext reference, to not become finalized
		/// </summary>
		protected readonly ZContext Context;

		/// <summary>
		/// The frontend socket that will normally pass messages to <see cref="BackendSocket"/>.
		/// </summary>
		protected readonly ZSocket FrontendSocket;

		/// <summary>
		/// The backend socket that will normally receive messages from (and possibly send replies to) <see cref="FrontendSocket"/>.
		/// </summary>
		protected readonly ZSocket BackendSocket;

		private readonly DeviceRunner _runner;

		private volatile bool _isRunning;

		private bool _disposed;

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
		{
			Context = context;

			ZError frontendSockErr, backendSockErr;
			FrontendSocket = ZSocket.Create(context, frontendType, out frontendSockErr);
			BackendSocket = ZSocket.Create(context, backendType, out backendSockErr);

			FrontendSetup = new DeviceSocketSetup(FrontendSocket);
			BackendSetup = new DeviceSocketSetup(BackendSocket);

			DoneEvent = new ManualResetEvent(false);

			_runner = new ThreadedDeviceRunner(this);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="ZDevice"/> class.
		/// </summary>
		~ZDevice()
		{
			Dispose(false);
		}

		/// <summary>
		/// Gets a value indicating whether the device loop is running.
		/// </summary>
		public bool IsRunning
		{
			get { return _isRunning; }
			private set { _isRunning = value; }
		}

		/// <summary>
		/// Gets a <see cref="DeviceSocketSetup"/> for configuring the frontend socket.
		/// </summary>
		public DeviceSocketSetup BackendSetup { get; private set; }

		/// <summary>
		/// Gets a <see cref="DeviceSocketSetup"/> for configuring the backend socket.
		/// </summary>
		public DeviceSocketSetup FrontendSetup { get; private set; }

		/// <summary>
		/// Gets a <see cref="ManualResetEvent"/> that can be used to block while the device is running.
		/// </summary>
		public ManualResetEvent DoneEvent { get; private set; }

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
		public void Initialize()
		{
			FrontendSetup.Configure();
			BackendSetup.Configure();
		}

		/// <summary>
		/// Start the device in the current thread.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The <see cref="ZDevice"/> has already been disposed.</exception>
		public virtual void Start()
		{
			_runner.Start();
		}

		/// <summary>
		/// Blocks the calling thread until the device terminates.
		/// </summary>
		public virtual void Join()
		{
			_runner.Join();
		}

		/// <summary>
		/// Blocks the calling thread until the device terminates or the specified time elapses.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> set to the amount of time to wait for the device to terminate.
		/// </param>
		/// <returns>
		/// true if the device terminated; false if the device has not terminated after
		/// the amount of time specified by <paramref name="timeout"/> has elapsed.
		/// </returns>
		public virtual bool Join(TimeSpan timeout)
		{
			return _runner.Join(timeout);
		}

		/// <summary>
		/// Stop the device in such a way that it can be restarted.
		/// </summary>
		public virtual void Stop()
		{
			IsRunning = false;
		}

		/// <summary>
		/// Stop the device and safely terminate the underlying sockets.
		/// </summary>
		public virtual void Close()
		{
			if (IsRunning)
			{
				Stop();
				Join(PollingInterval);
			}

			FrontendSocket.Close();
			BackendSocket.Close();
		}

		/// <summary>
		/// Releases all resources used by the current instance, including the frontend and backend sockets.
		/// </summary>
		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Start the device in the current thread. Should be used by implementations of the <see cref="DeviceRunner.Start"/> method.
		/// </summary>
		/// <remarks>
		/// Initializes the sockets prior to starting the device with <see cref="Initialize"/>.
		/// </remarks>
		protected internal void Run()
		{
			EnsureNotDisposed();

			DoneEvent.Reset();
			IsRunning = true;

			Initialize();

			var polls = new ZPollItem[] {
								ZPollItem.Create(FrontendSocket, FrontendHandler),
								ZPollItem.Create(BackendSocket, BackendHandler)
						};

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

			FrontendSetup.BindConnect();
			BackendSetup.BindConnect();

			bool isValid = false;
			var error = default(ZError);
			try
			{
				while (IsRunning)
				{

					if (!(isValid = ZPollItems.TryPoll(polls, ZPoll.In, ref lastMessageFrames, out error, PollingInterval)))
					{

						if (error == ZError.EAGAIN)
						{
							error = default(ZError);
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
				if (IsRunning)
				{
					throw;
				}
			}

			FrontendSetup.UnbindDisconnect();
			BackendSetup.UnbindDisconnect();
			IsRunning = false;

			if (error == ZError.ETERM)
			{
				Close();
			}

			DoneEvent.Set();
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
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (IsRunning)
			{
				Stop();
				Join(PollingInterval);
			}

			if (disposing)
			{
				FrontendSocket.Dispose();
				BackendSocket.Dispose();
			}

			_disposed = true;
		}

		private void EnsureNotDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}
	}
}
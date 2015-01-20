using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using ZeroMQ.lib;
using ZeroMQ.Monitoring;

namespace ZeroMQ
{
	/// <summary>
	/// Sends and receives messages across various transports to potentially multiple endpoints
	/// using the ZMQ protocol.
	/// </summary>
	public class ZSocket : IDisposable
	{
		public static ZSocket Create(ZContext context, ZSocketType socketType)
		{
			ZError error;
			ZSocket socket;
			if (null == (socket = Create(context, socketType, out error)))
			{
				throw new ZException(error);
			}
			return socket;
		}

		/// <summary>
		/// Create a socket with the current context and the specified socket type.
		/// </summary>
		/// <param name="socketType">A <see cref="ZSocketType"/> value for the socket.</param>
		/// <returns>A <see cref="ZSocket"/> instance with the current context and the specified socket type.</returns>
		public static ZSocket Create(ZContext context, ZSocketType socketType, out ZError error)
		{
			error = ZError.None;

			IntPtr socketPtr;
			if (IntPtr.Zero == (socketPtr = zmq.socket(context.ContextPtr, socketType.Number)))
			{
				error = ZError.GetLastErr();
				return default(ZSocket);
			}

			return new ZSocket(context, socketPtr, socketType);
		}

		private ZContext _context;

		private IntPtr _socketPtr;

		private ZSocketType _socketType;

		protected ZSocket(ZContext context, IntPtr socketPtr, ZSocketType socketType)
		{
			_context = context;
			_socketPtr = socketPtr;
			_socketType = socketType;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="ZSocket"/> class.
		/// </summary>
		~ZSocket()
		{
			Dispose(false);
		}

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="ZSocket"/>, and optionally disposes of the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				Close();
			}

			_disposed = true;
		}

		/// <summary>
		/// Close the current socket.
		/// </summary>
		/// <remarks>
		/// Any outstanding messages physically received from the network but not yet received by the application
		/// with Receive shall be discarded. The behaviour for discarding messages sent by the application
		/// with Send but not yet physically transferred to the network depends on the value of
		/// the <see cref="Linger"/> socket option.
		/// </remarks>
		/// <exception cref="ZmqSocketException">The underlying socket object is not valid.</exception>
		public bool Close()
		{
			ZError error;
			return Close(out error);
		}

		public bool Close(out ZError error)
		{
			EnsureNotDisposed();

			error = ZError.None;
			if (_socketPtr == IntPtr.Zero) return true;

			if (-1 == zmq.close(_socketPtr))
			{
				error = ZError.GetLastErr();
				return false;
			}
			_socketPtr = IntPtr.Zero;
			return true;
		}

		public ZContext Context
		{
			get { return _context; }
		}

		public IntPtr SocketPtr
		{
			get { return _socketPtr; }
		}

		/// <summary>
		/// Gets the <see cref="ZeroMQ.ZSocketType"/> value for the current socket.
		/// </summary>
		public ZSocketType SocketType { get { return _socketType; } }

		public void Bind(string endpoint)
		{
			ZError error;
			if (!Bind(endpoint, out error))
			{
				throw new ZException(error);
			}
		}

		/// <summary>
		/// Create an endpoint for accepting connections and bind it to the current socket.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="endpoint"/> is null.</exception>
		/// <exception cref="ZmqSocketException">An error occurred binding the socket to an endpoint.</exception>
		/// <exception cref="System.ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public bool Bind(string endpoint, out ZError error)
		{
			EnsureNotDisposed();

			error = default(ZError);

			if (string.IsNullOrWhiteSpace(endpoint))
			{
				throw new ArgumentException("IsNullOrWhiteSpace", "endpoint");
			}

			using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
			{
				if (-1 == zmq.bind(_socketPtr, endpointPtr))
				{
					error = ZError.GetLastErr();
					return false;
				}
			}
			return true;
		}

		public void Unbind(string endpoint)
		{
			ZError error;
			if (!Unbind(endpoint, out error))
			{
				throw new ZException(error);
			}
		}

		/// <summary>
		/// Stop accepting connections for a previously bound endpoint on the current socket.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="endpoint"/> is null.</exception>
		/// <exception cref="ZmqSocketException">An error occurred unbinding the socket to an endpoint.</exception>
		/// <exception cref="System.ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public bool Unbind(string endpoint, out ZError error)
		{
			EnsureNotDisposed();

			error = default(ZError);

			if (string.IsNullOrWhiteSpace(endpoint))
			{
				throw new ArgumentException("IsNullOrWhiteSpace", "endpoint");
			}

			using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
			{
				if (-1 == zmq.unbind(_socketPtr, endpointPtr))
				{
					error = ZError.GetLastErr();
					return false;
				}
			}
			return true;
		}

		public void Connect(string endpoint)
		{
			ZError error;
			if (!Connect(endpoint, out error))
			{
				throw new ZException(error);
			}
		}

		/// <summary>
		/// Connect the current socket to the specified endpoint.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="endpoint"/> is null.</exception>
		/// <exception cref="ZmqSocketException">An error occurred connecting the socket to a remote endpoint.</exception>
		/// <exception cref="System.ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public bool Connect(string endpoint, out ZError error)
		{
			EnsureNotDisposed();

			error = default(ZError);

			if (string.IsNullOrWhiteSpace(endpoint))
			{
				throw new ArgumentException("IsNullOrWhiteSpace", "endpoint");
			}

			using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
			{
				if (-1 == zmq.connect(_socketPtr, endpointPtr))
				{
					error = ZError.GetLastErr();
					return false;
				}
			}
			return true;
		}

		public void Disconnect(string endpoint)
		{
			ZError error;
			if (!Disconnect(endpoint, out error))
			{
				throw new ZException(error);
			}
		}

		/// <summary>
		/// Disconnect the current socket from a previously connected endpoint.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="endpoint"/> is null.</exception>
		/// <exception cref="ZmqSocketException">An error occurred disconnecting the socket from a remote endpoint.</exception>
		/// <exception cref="System.ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public bool Disconnect(string endpoint, out ZError error)
		{
			EnsureNotDisposed();

			error = default(ZError);

			if (string.IsNullOrWhiteSpace(endpoint))
			{
				throw new ArgumentException("IsNullOrWhiteSpace", "endpoint");
			}

			using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
			{
				if (-1 == zmq.disconnect(_socketPtr, endpointPtr))
				{
					error = ZError.GetLastErr();
					return false;
				}
			}
			return true;
		}

		public void ReceiveBytes(byte[] buffer, int offset, int count)
		{
			ZError error;
			if (!ReceiveBytes(buffer, offset, count, ZSocketFlags.None, out error))
			{
				throw new ZException(error);
			}
		}

		public bool ReceiveBytes(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			error = ZError.None;

			// int zmq_recv(void* socket, void* buf, size_t len, int flags);

			var pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IntPtr pinPtr = pin.AddrOfPinnedObject() + offset;

			if (-1 == zmq.recv(this.SocketPtr, pinPtr, count, (int)flags))
			{
				error = ZError.GetLastErr();

				pin.Free();
				return false;
			}

			pin.Free();
			return true;
		}

		public void Send(byte[] buffer, int offset, int count)
		{
			ZError error;
			if (!Send(buffer, offset, count, ZSocketFlags.None, out error))
			{
				throw new ZException(error);
			}
		}

		public bool Send(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			error = ZError.None;
			
			// int zmq_send (void *socket, void *buf, size_t len, int flags);

			var pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IntPtr pinPtr = pin.AddrOfPinnedObject() + offset;

			if (-1 == zmq.send(SocketPtr, pinPtr, count, (int)flags))
			{
				error = ZError.GetLastErr();

				pin.Free();
				return false;
			}

			pin.Free();
			return true;
		}

		public ZMessage ReceiveMessage()
		{
			return ReceiveMessage(ZSocketFlags.None);
		}

		public ZMessage ReceiveMessage(out ZError error)
		{
			return ReceiveMessage(ZSocketFlags.None, out error);
		}

		public ZMessage ReceiveMessage(ZSocketFlags flags)
		{
			ZError error;
			ZMessage message = ReceiveMessage(flags, out error);
			if (error != ZError.None)
			{
				throw new ZException(error);
			}
			return message;
		}

		public ZMessage ReceiveMessage(ZSocketFlags flags, out ZError error)
		{
			var message = new ZMessage();
			if (!ReceiveMessage(flags, ref message, out error))
			{
				return default(ZMessage);
			}

			return message;
		}

		public bool ReceiveMessage(ZSocketFlags flags, ref ZMessage message, out ZError error)
		{
			EnsureNotDisposed();

			int count = int.MaxValue;
			List<ZFrame> frames;
			bool good = true;

			while (!ReceiveFrames(ref count, out frames, flags, out error))
			{
				if (error == ZError.EAGAIN && ((flags & ZSocketFlags.DontWait) == ZSocketFlags.DontWait))
				{
					good = false;
					break;
				}

				return false;
			}

			if (message == null)
			{
				message = new ZMessage(frames);
			}
			else
			{
				message.AddRange(frames);
			}
				
			return good;
		}

		public ZFrame ReceiveFrame()
		{
			ZError error;
			ZFrame frame = ReceiveFrame(out error);
			if (error != ZError.None)
			{
				throw new ZException(error);
			}
			return frame;
		}

		public ZFrame ReceiveFrame(out ZError error)
		{
			return ReceiveFrame(ZSocketFlags.None, out error);
		}

		public ZFrame ReceiveFrame(ZSocketFlags flags, out ZError error)
		{
			IEnumerable<ZFrame> frames = ReceiveFrames(1, flags &= ZSocketFlags.More, out error);
			if (frames != null)
			{
				foreach (ZFrame frame in frames)
				{
					return frame;
				}
			}
			return null;
		}

		public IEnumerable<ZFrame> ReceiveFrames(int framesToReceive)
		{
			return ReceiveFrames(framesToReceive, ZSocketFlags.None);
		}

		public IEnumerable<ZFrame> ReceiveFrames(int framesToReceive, ZSocketFlags flags)
		{
			ZError error;
			var frames = ReceiveFrames(framesToReceive, flags, out error);
			if (error != ZError.None)
			{
				throw new ZException(error);
			}
			return frames;
		}

		public IEnumerable<ZFrame> ReceiveFrames(int framesToReceive, out ZError error)
		{
			return ReceiveFrames(framesToReceive, ZSocketFlags.None, out error);
		}

		public IEnumerable<ZFrame> ReceiveFrames(int framesToReceive, ZSocketFlags flags, out ZError error)
		{
			List<ZFrame> frames;
			
			if (!ReceiveFrames(ref framesToReceive, out frames, flags, out error)) {

				if (error == ZError.EAGAIN && ((flags & ZSocketFlags.DontWait) == ZSocketFlags.DontWait))
				{
					return frames;
				}

				return null;
			}

			return frames;
		}

		public bool ReceiveFrames(ref int framesToReceive, out List<ZFrame> frames, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			error = default(ZError);
			frames = new List<ZFrame>();

			do {

				var frame = ZFrame.CreateEmpty();

				while (-1 == zmq.msg_recv(frame.Ptr, _socketPtr, (int)flags))
				{
					error = ZError.GetLastErr();

					if (error == ZError.EINTR)
					{
						error = default(ZError);
						continue;
					}
					if (error == ZError.EAGAIN && ((flags & ZSocketFlags.DontWait) == ZSocketFlags.DontWait))
					{
						return false;
					}

					frame.Dispose();
					return false;
				}

				frames.Add(frame);

				--framesToReceive;

			} while (framesToReceive > 0 && (framesToReceive > 1 ? this.ReceiveMore : false));

			return true;
		}

		public virtual void Send(ZMessage msg)
		{
			Send(msg, ZSocketFlags.None);
		}

		public virtual bool Send(ZMessage msg, out ZError error)
		{
			return Send(msg, ZSocketFlags.None, out error);
		}

		public virtual void Send(ZMessage msg, ZSocketFlags flags)
		{
			ZError error;
			if (!Send(msg, flags, out error))
			{
				throw new ZException(error);
			}
		}

		public virtual bool Send(ZMessage msg, ZSocketFlags flags, out ZError error)
		{
			return Send((IEnumerable<ZFrame>)msg, flags, out error);
		}

		public virtual void Send(IEnumerable<ZFrame> frames)
		{
			Send(frames, ZSocketFlags.None);
		}

		public virtual bool Send(IEnumerable<ZFrame> frames, out ZError error)
		{
			return Send(frames, ZSocketFlags.None, out error);
		}

		public virtual void Send(IEnumerable<ZFrame> frames, ZSocketFlags flags)
		{
			ZError error;
			int sent = 0;
			if (!Send(frames, ref sent, flags, out error))
			{
				throw new ZException(error);
			}
		}

		public virtual bool Send(IEnumerable<ZFrame> frames, ZSocketFlags flags, out ZError error)
		{
			int sent = 0;
			if (!Send(frames, ref sent, flags, out error))
			{
				return false;
			}
			return true;
		}

		public virtual bool Send(IEnumerable<ZFrame> frames, ref int sent, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			error = ZError.None;

			bool more = (flags & ZSocketFlags.More) == ZSocketFlags.More;
			flags |= ZSocketFlags.More;

			for (int i = 0, l = frames.Count(); i < l; ++i)
			{
				if (i == l - 1 && !more)
				{
					flags &= ~ZSocketFlags.More;
				}
				if (!Send(frames.ElementAt(i), flags, out error))
				{
					return false;
				}
				++sent;
			}

			return true;
		}

		public virtual void Send(ZFrame frame)
		{
			Send(frame, ZSocketFlags.None);
		}

		public virtual bool Send(ZFrame msg, out ZError error)
		{
			return Send(msg, ZSocketFlags.None, out error);
		}

		public virtual void SendMore(ZFrame frame)
		{
			Send(frame, ZSocketFlags.More);
		}

		public virtual bool SendMore(ZFrame msg, out ZError error)
		{
			return Send(msg, ZSocketFlags.More, out error);
		}

		public virtual void SendMore(ZFrame frame, ZSocketFlags flags)
		{
			Send(frame, flags | ZSocketFlags.More);
		}

		public virtual bool SendMore(ZFrame msg, ZSocketFlags flags, out ZError error)
		{
			return Send(msg, flags | ZSocketFlags.More, out error);
		}

		public virtual void Send(ZFrame frame, ZSocketFlags flags)
		{
			ZError error;
			if (!Send(frame, flags, out error))
			{
				throw new ZException(error);
			}
		}

		public virtual bool Send(ZFrame frame, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			error = default(ZError);

			while (-1 == zmq.msg_send(frame.Ptr, _socketPtr, (int)flags))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = default(ZError);
					continue;
				}
				if (error == ZError.EAGAIN)
				{
					if ((flags & ZSocketFlags.DontWait) == ZSocketFlags.DontWait)
					{
						return false;
					}

					error = default(ZError);
					Thread.Sleep(1);

					continue;
				}

				return false;
			}

			// Tell IDisposable to not unallocate zmq_msg
			frame.Dismiss();

			return true;
		}

		public bool Forward(ZSocket destination, out ZMessage message, out ZError error)
		{
			EnsureNotDisposed();

			error = default(ZError);
			message = null; // message is always null

			bool more = false;

			using (var msg = ZFrame.CreateEmpty())
			{
				do
				{
					while (-1 == zmq.msg_recv(msg.Ptr, this.SocketPtr, (int)ZSocketFlags.DontWait))
					{
						error = ZError.GetLastErr();

						if (error == ZError.EINTR)
						{
							error = null;
							continue;
						}

						return false;
					}

					// will have to receive more?
					more = ReceiveMore;

					// sending scope
					while (-1 != zmq.msg_send(msg.Ptr, destination.SocketPtr, more ? (int)(ZSocketFlags.More | ZSocketFlags.DontWait) : (int)ZSocketFlags.DontWait))
					{
						error = ZError.GetLastErr();

						if (error == ZError.EINTR)
						{
							error = null;
							continue;
						}

						return false;
					}
					
					// msg.Dismiss

				} while (more);

			} // using (msg) -> Dispose

			return true;
		}

		private bool GetOption(ZSocketOption option, IntPtr optionValue, ref int optionLength)
		{
			EnsureNotDisposed();

			using (var optionLengthP = DispoIntPtr.Alloc(IntPtr.Size))
			{
				if (IntPtr.Size == 4)
					Marshal.WriteInt32(optionLengthP.Ptr, optionLength);
				else if (IntPtr.Size == 8)
					Marshal.WriteInt64(optionLengthP.Ptr, (long)optionLength);
				else
					throw new PlatformNotSupportedException();

				ZError error;
				while (-1 == zmq.getsockopt(this._socketPtr, (int)option, optionValue, optionLengthP.Ptr))
				{
					error = ZError.GetLastErr();

					if (error == ZError.EINTR)
					{
						error = ZError.None;
						continue;
					}

					throw new ZException(error);
				}

				if (IntPtr.Size == 4)
					optionLength = Marshal.ReadInt32(optionLengthP.Ptr);
				else if (IntPtr.Size == 8)
					optionLength = (int)Marshal.ReadInt64(optionLengthP.Ptr);
				else
					throw new PlatformNotSupportedException();
			}

			return true;
		}

		// From options.hpp: unsigned char identity [256];
		private const int MaxBinaryOptionSize = 256;

		public bool GetOption(ZSocketOption option, out byte[] value)
		{
			value = null;

			int optionLength = MaxBinaryOptionSize;
			using (var optionValue = DispoIntPtr.Alloc(optionLength))
			{
				if (GetOption(option, optionValue, ref optionLength))
				{
					value = new byte[optionLength];
					Marshal.Copy(optionValue, value, 0, optionLength);
					return true;
				}
				return false;
			}
		}

		public byte[] GetOptionBytes(ZSocketOption option)
		{
			byte[] result;
			if (GetOption(option, out result))
			{
				return result;
			}
			return null;
		}

		public bool GetOption(ZSocketOption option, out string value)
		{
			value = null;

			int optionLength = MaxBinaryOptionSize;
			using (var optionValue = DispoIntPtr.Alloc(optionLength))
			{
				if (GetOption(option, optionValue, ref optionLength))
				{
					value = Marshal.PtrToStringAnsi(optionValue, optionLength);
					return true;
				}
				return false;
			}
		}

		public string GetOptionString(ZSocketOption option)
		{
			string result;
			if (GetOption(option, out result))
			{
				return result;
			}
			return null;
		}

		public bool GetOption(ZSocketOption option, out Int32 value)
		{
			value = default(Int32);

			int optionLength = Marshal.SizeOf(typeof(Int32));
			using (var optionValue = DispoIntPtr.Alloc(optionLength))
			{
				if (GetOption(option, optionValue.Ptr, ref optionLength)) {
					value = Marshal.ReadInt32(optionValue.Ptr);
					return true;
				}
				return false;
			}
		}

		public Int32 GetOptionInt32(ZSocketOption option)
		{
			Int32 result;
			if (GetOption(option, out result))
			{
				return result;
			}
			return default(Int32);
		}

		public bool GetOption(ZSocketOption option, out UInt32 value)
		{
			Int32 resultValue;
			bool result = GetOption(option, out resultValue);
			value = (UInt32)resultValue;
			return result;
		}

		public UInt32 GetOptionUInt32(ZSocketOption option)
		{
			UInt32 result;
			if (GetOption(option, out result))
			{
				return result;
			}
			return default(UInt32);
		}

		public bool GetOption(ZSocketOption option, out Int64 value)
		{
			value = default(Int64);

			int optionLength = Marshal.SizeOf(typeof(Int64));
			using (var optionValue = DispoIntPtr.Alloc(optionLength))
			{
				if (GetOption(option, optionValue.Ptr, ref optionLength))
				{
					value = Marshal.ReadInt64(optionValue);
					return true;
				}
				return false;
			}
		}

		public Int64 GetOptionInt64(ZSocketOption option)
		{
			Int64 result;
			if (GetOption(option, out result))
			{
				return result;
			}
			return default(Int64);
		}

		public bool GetOption(ZSocketOption option, out UInt64 value)
		{
			Int64 resultValue;
			bool result = GetOption(option, out resultValue);
			value = (UInt64)resultValue;
			return result;
		}

		public UInt64 GetOptionUInt64(ZSocketOption option)
		{
			UInt64 result;
			if (GetOption(option, out result))
			{
				return result;
			}
			return default(UInt64);
		}


		private bool SetOption(ZSocketOption option, IntPtr optionValue, int optionLength)
		{
			EnsureNotDisposed();

			ZError error;
			while (-1 == zmq.setsockopt(this._socketPtr, (int)option, optionValue, optionLength))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = ZError.None;
					continue;
				}

				return false;
			}
			return true;
		}

		public bool SetOptionNull(ZSocketOption option)
		{
			return SetOption(option, IntPtr.Zero, 0);
		}

		public bool SetOption(ZSocketOption option, byte[] value)
		{
			if (value == null)
			{
				return SetOptionNull(option);
			}

			int optionLength = /* Marshal.SizeOf(typeof(byte)) * */ value.Length;
			using (var optionValue = DispoIntPtr.Alloc(optionLength))
			{
				Marshal.Copy(value, 0, optionValue.Ptr, optionLength);

				return SetOption(option, optionValue.Ptr, optionLength);
			}
		}

		public bool SetOption(ZSocketOption option, string value)
		{
			if (value == null)
			{
				return SetOptionNull(option);
			}

			int optionLength;
			using (var optionValue = DispoIntPtr.AllocString(value, out optionLength))
			{
				return SetOption(option, optionValue, optionLength);
			}
		}

		public bool SetOption(ZSocketOption option, Int32 value)
		{
			int optionLength = Marshal.SizeOf(typeof(Int32));
			using (var optionValue = DispoIntPtr.Alloc(optionLength))
			{
				Marshal.WriteInt32(optionValue, value);

				return SetOption(option, optionValue.Ptr, optionLength);
			}
		}

		public bool SetOption(ZSocketOption option, UInt32 value)
		{
			return SetOption(option, (Int32)value);
		}

		public bool SetOption(ZSocketOption option, Int64 value)
		{

			bool result = false;

			int optionLength = Marshal.SizeOf(typeof(Int64));
			using (var optionValue = DispoIntPtr.Alloc(optionLength))
			{
				Marshal.WriteInt64(optionValue, value);

				result = SetOption(option, optionValue.Ptr, optionLength);
			}

			return result;
		}

		public bool SetOption(ZSocketOption option, UInt64 value)
		{
			return SetOption(option, (Int64)value);
		}

		/// <summary>
		/// Subscribe to all messages.
		/// </summary>
		/// <remarks>
		/// Only applies to <see cref="ZeroMQ.ZSocketType.SUB"/> and <see cref="ZeroMQ.ZSocketType.XSUB"/> sockets.
		/// </remarks>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		/// <exception cref="NotSupportedException">The current socket type does not support subscriptions.</exception>
		public void SubscribeAll()
		{
			Subscribe(new byte[0]);
		}

		/// <summary>
		/// Subscribe to messages that begin with a specified prefix.
		/// </summary>
		/// <remarks>
		/// Only applies to <see cref="ZeroMQ.ZSocketType.SUB"/> and <see cref="ZeroMQ.ZSocketType.XSUB"/> sockets.
		/// </remarks>
		/// <param name="prefix">Prefix for subscribed messages.</param>
		/// <exception cref="ArgumentNullException"><paramref name="prefix"/> is null.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		/// <exception cref="NotSupportedException">The current socket type does not support subscriptions.</exception>
		public virtual void Subscribe(byte[] prefix)
		{
			SetOption(ZSocketOption.SUBSCRIBE, prefix);
		}

		/// <summary>
		/// Unsubscribe from all messages.
		/// </summary>
		/// <remarks>
		/// Only applies to <see cref="ZeroMQ.ZSocketType.SUB"/> and <see cref="ZeroMQ.ZSocketType.XSUB"/> sockets.
		/// </remarks>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		/// <exception cref="NotSupportedException">The current socket type does not support subscriptions.</exception>
		public void UnsubscribeAll()
		{
			Unsubscribe(new byte[0]);
		}

		/// <summary>
		/// Unsubscribe from messages that begin with a specified prefix.
		/// </summary>
		/// <remarks>
		/// Only applies to <see cref="ZeroMQ.ZSocketType.SUB"/> and <see cref="ZeroMQ.ZSocketType.XSUB"/> sockets.
		/// </remarks>
		/// <param name="prefix">Prefix for subscribed messages.</param>
		/// <exception cref="ArgumentNullException"><paramref name="prefix"/> is null.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		/// <exception cref="NotSupportedException">The current socket type does not support subscriptions.</exception>
		public virtual void Unsubscribe(byte[] prefix)
		{
			SetOption(ZSocketOption.UNSUBSCRIBE, prefix);
		}

		/// <summary>
		/// Gets a value indicating whether the multi-part message currently being read has more message parts to follow.
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public bool ReceiveMore
		{
			get { return GetOptionInt32(ZSocketOption.RCVMORE) == 1; }
		}

		public string LastEndpoint
		{
			get { return GetOptionString(ZSocketOption.LAST_ENDPOINT); }
		}

		/// <summary>
		/// Gets or sets the I/O thread affinity for newly created connections on this socket.
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public ulong Affinity
		{
			get { return GetOptionUInt64(ZSocketOption.AFFINITY); }
			set { SetOption(ZSocketOption.AFFINITY, value); }
		}

		/// <summary>
		/// Gets or sets the maximum length of the queue of outstanding peer connections. (Default = 100 connections).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int Backlog
		{
			get { return GetOptionInt32(ZSocketOption.BACKLOG); }
			set { SetOption(ZSocketOption.BACKLOG, value); }
		}

		public byte[] ConnectRID
		{
			get { return GetOptionBytes(ZSocketOption.CONNECT_RID); }
			set { SetOption(ZSocketOption.CONNECT_RID, value); }
		}

		public bool Conflate
		{
			get { return GetOptionInt32(ZSocketOption.CONFLATE) == 1; }
			set { SetOption(ZSocketOption.CONFLATE, value ? 1 : 0); }
		}

		public byte[] CurvePublicKey
		{
			get { return GetOptionBytes(ZSocketOption.CURVE_PUBLICKEY); }
			set { SetOption(ZSocketOption.CURVE_PUBLICKEY, value); }
		}

		public byte[] CurveSecretKey
		{
			get { return GetOptionBytes(ZSocketOption.CURVE_SECRETKEY); }
			set { SetOption(ZSocketOption.CURVE_SECRETKEY, value); }
		}

		public bool CurveServer
		{
			get { return GetOptionInt32(ZSocketOption.CURVE_SERVER) == 1; }
			set { SetOption(ZSocketOption.CURVE_SERVER, value ? 1 : 0); }
		}

		public byte[] CurveServerKey
		{
			get { return GetOptionBytes(ZSocketOption.CURVE_SERVERKEY); }
			set { SetOption(ZSocketOption.CURVE_SERVERKEY, value); }
		}

		public bool GSSAPIPlainText
		{
			get { return GetOptionInt32(ZSocketOption.GSSAPI_PLAINTEXT) == 1; }
			set { SetOption(ZSocketOption.GSSAPI_PLAINTEXT, value ? 1 : 0); }
		}

		public string GSSAPIPrincipal
		{
			get { return GetOptionString(ZSocketOption.GSSAPI_PRINCIPAL); }
			set { SetOption(ZSocketOption.GSSAPI_PRINCIPAL, value); }
		}

		public bool GSSAPIServer
		{
			get { return GetOptionInt32(ZSocketOption.GSSAPI_SERVER) == 1; }
			set { SetOption(ZSocketOption.GSSAPI_SERVER, value ? 1 : 0); }
		}

		public string GSSAPIServicePrincipal
		{
			get { return GetOptionString(ZSocketOption.GSSAPI_SERVICE_PRINCIPAL); }
			set { SetOption(ZSocketOption.GSSAPI_SERVICE_PRINCIPAL, value); }
		}

		public int HandshakeInterval
		{
			get { return GetOptionInt32(ZSocketOption.HANDSHAKE_IVL); }
			set { SetOption(ZSocketOption.HANDSHAKE_IVL, value); }
		}

		/// <summary>
		/// Gets or sets the identity of the current socket.
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public byte[] Identity
		{
			get { return GetOptionBytes(ZSocketOption.IDENTITY); }
			set { SetOption(ZSocketOption.IDENTITY, value); }
		}

		public bool Immediate
		{
			get { return GetOptionInt32(ZSocketOption.IMMEDIATE) == 1; }
			set { SetOption(ZSocketOption.IMMEDIATE, value ? 1 : 0); }
		}

		public bool IPv6
		{
			get { return GetOptionInt32(ZSocketOption.IPV6) == 1; }
			set { SetOption(ZSocketOption.IPV6, value ? 1 : 0); }
		}

		/// <summary>
		/// Gets or sets the linger period for socket shutdown. (Default = <see cref="TimeSpan.MaxValue"/>, infinite).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public TimeSpan Linger
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.LINGER)); }
			set { SetOption(ZSocketOption.LINGER, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the maximum size for inbound messages (bytes). (Default = -1, no limit).
		/// </summary>
		/// <exception cref="ZmqVersionException">This socket option was used in ZeroMQ 2.x or lower.</exception>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public long MaxMessageSize
		{
			get { return GetOptionInt64(ZSocketOption.MAX_MSG_SIZE); }
			set { SetOption(ZSocketOption.MAX_MSG_SIZE, value); }
		}

		/// <summary>
		/// Gets or sets the time-to-live field in every multicast packet sent from this socket (network hops). (Default = 1 hop).
		/// </summary>
		/// <exception cref="ZmqVersionException">This socket option was used in ZeroMQ 2.x or lower.</exception>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int MulticastHops
		{
			get { return GetOptionInt32(ZSocketOption.MULTICAST_HOPS); }
			set { SetOption(ZSocketOption.MULTICAST_HOPS, value); }
		}

		public string PlainPassword
		{
			get { return GetOptionString(ZSocketOption.PLAIN_PASSWORD); }
			set { SetOption(ZSocketOption.PLAIN_PASSWORD, value); }
		}

		public bool PlainServer
		{
			get { return GetOptionInt32(ZSocketOption.PLAIN_SERVER) == 1; }
			set { SetOption(ZSocketOption.PLAIN_SERVER, value ? 1 : 0); }
		}

		public string PlainUserName
		{
			get { return GetOptionString(ZSocketOption.PLAIN_USERNAME); }
			set { SetOption(ZSocketOption.PLAIN_USERNAME, value); }
		}

		public bool ProbeRouter
		{
			get { return GetOptionInt32(ZSocketOption.PROBE_ROUTER) == 1; }
			set { SetOption(ZSocketOption.PROBE_ROUTER, value ? 1 : 0); }
		}

		/// <summary>
		/// Gets or sets the maximum send or receive data rate for multicast transports (kbps). (Default = 100 kbps).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int MulticastRate
		{
			get { return GetOptionInt32(ZSocketOption.RATE); }
			set { SetOption(ZSocketOption.RATE, value); }
		}

		/// <summary>
		/// Gets or sets the underlying kernel receive buffer size for the current socket (bytes). (Default = 0, OS default).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int ReceiveBufferSize
		{
			get { return GetOptionInt32(ZSocketOption.RCVBUF); }
			set { SetOption(ZSocketOption.RCVBUF, value); }
		}

		/// <summary>
		/// Gets or sets the high water mark for inbound messages (number of messages). (Default = 0, no limit).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int ReceiveHighWatermark
		{
			get { return GetOptionInt32(ZSocketOption.RCVHWM); }
			set { SetOption(ZSocketOption.RCVHWM, value); }
		}

		/// <summary>
		/// Gets or sets the timeout for receive operations. (Default = <see cref="TimeSpan.MaxValue"/>, infinite).
		/// </summary>
		/// <exception cref="ZmqVersionException">This socket option was used in ZeroMQ 2.x or lower.</exception>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public TimeSpan ReceiveTimeout
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RCVTIMEO)); }
			set { SetOption(ZSocketOption.RCVTIMEO, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the initial reconnection interval. (Default = 100 milliseconds).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public TimeSpan ReconnectInterval
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RECONNECT_IVL)); }
			set { SetOption(ZSocketOption.RECONNECT_IVL, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the maximum reconnection interval. (Default = 0, only use <see cref="ReconnectInterval"/>).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public TimeSpan ReconnectIntervalMax
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RECONNECT_IVL_MAX)); }
			set { SetOption(ZSocketOption.RECONNECT_IVL_MAX, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the recovery interval for multicast transports. (Default = 10 seconds).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public TimeSpan MulticastRecoveryInterval
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RECOVERY_IVL)); }
			set { SetOption(ZSocketOption.RECOVERY_IVL, (int)value.TotalMilliseconds); }
		}

		public bool RequestCorrelate
		{
			get { return GetOptionInt32(ZSocketOption.REQ_CORRELATE) == 1; }
			set { SetOption(ZSocketOption.REQ_CORRELATE, value ? 1 : 0); }
		}

		public bool RequestRelaxed
		{
			get { return GetOptionInt32(ZSocketOption.REQ_RELAXED) == 1; }
			set { SetOption(ZSocketOption.REQ_RELAXED, value ? 1 : 0); }
		}

		public bool RouterHandover
		{
			get { return GetOptionInt32(ZSocketOption.ROUTER_HANDOVER) == 1; }
			set { SetOption(ZSocketOption.ROUTER_HANDOVER, value ? 1 : 0); }
		}

		public RouterMandatory RouterMandatory
		{
			get { return (RouterMandatory)GetOptionInt32(ZSocketOption.ROUTER_MANDATORY); }
			set { SetOption(ZSocketOption.ROUTER_MANDATORY, (int)value); }
		}

		public bool RouterRaw
		{
			get { return GetOptionInt32(ZSocketOption.ROUTER_RAW) == 1; }
			set { SetOption(ZSocketOption.ROUTER_RAW, value ? 1 : 0); }
		}

		/// <summary>
		/// Gets or sets the underlying kernel transmit buffer size for the current socket (bytes). (Default = 0, OS default).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int SendBufferSize
		{
			get { return GetOptionInt32(ZSocketOption.SNDBUF); }
			set { SetOption(ZSocketOption.SNDBUF, value); }
		}

		/// <summary>
		/// Gets or sets the high water mark for outbound messages (number of messages). (Default = 0, no limit).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int SendHighWatermark
		{
			get { return GetOptionInt32(ZSocketOption.SNDHWM); }
			set { SetOption(ZSocketOption.SNDHWM, value); }
		}

		/// <summary>
		/// Gets or sets the timeout for send operations. (Default = <see cref="TimeSpan.MaxValue"/>, infinite).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public TimeSpan SendTimeout
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.SNDTIMEO)); }
			set { SetOption(ZSocketOption.SNDTIMEO, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the override value for the SO_KEEPALIVE TCP socket option. (where supported by OS). (Default = -1, OS default).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public TcpKeepaliveBehaviour TcpKeepAlive
		{
			get { return (TcpKeepaliveBehaviour)GetOptionInt32(ZSocketOption.TCP_KEEPALIVE); }
			set { SetOption(ZSocketOption.TCP_KEEPALIVE, (int)value); }
		}

		/// <summary>
		/// Gets or sets the override value for the 'TCP_KEEPCNT' socket option (where supported by OS). (Default = -1, OS default).
		/// The default value of '-1' means to skip any overrides and leave it to OS default.
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int TcpKeepAliveCount
		{
			get { return GetOptionInt32(ZSocketOption.TCP_KEEPALIVE_CNT); }
			set { SetOption(ZSocketOption.TCP_KEEPALIVE_CNT, value); }
		}

		/// <summary>
		/// Gets or sets the override value for the TCP_KEEPCNT (or TCP_KEEPALIVE on some OS). (Default = -1, OS default).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int TcpKeepAliveIdle
		{
			get { return GetOptionInt32(ZSocketOption.TCP_KEEPALIVE_IDLE); }
			set { SetOption(ZSocketOption.TCP_KEEPALIVE_IDLE, value); }
		}

		/// <summary>
		/// Gets or sets the override value for the TCP_KEEPINTVL socket option (where supported by OS). (Default = -1, OS default).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public int TcpKeepAliveInterval
		{
			get { return GetOptionInt32(ZSocketOption.TCP_KEEPALIVE_INTVL); }
			set { SetOption(ZSocketOption.TCP_KEEPALIVE_INTVL, value); }
		}

		public int TypeOfService
		{
			get { return GetOptionInt32(ZSocketOption.TOS); }
			set { SetOption(ZSocketOption.TOS, value); }
		}

		public bool XPubVerbose
		{
			get { return GetOptionInt32(ZSocketOption.XPUB_VERBOSE) == 1; }
			set { SetOption(ZSocketOption.XPUB_VERBOSE, value ? 1 : 0); }
		}

		public string ZAPDomain
		{
			get { return GetOptionString(ZSocketOption.ZAP_DOMAIN); }
			set { SetOption(ZSocketOption.ZAP_DOMAIN, value); }
		}

		/// <summary>
		/// Add a filter that will be applied for each new TCP transport connection on a listening socket.
		/// Example: "127.0.0.1", "mail.ru/24", "::1", "::1/128", "3ffe:1::", "3ffe:1::/56"
		/// </summary>
		/// <seealso cref="ClearTcpAcceptFilter"/>
		/// <remarks>
		/// If no filters are applied, then TCP transport allows connections from any IP. If at least one
		/// filter is applied then new connection source IP should be matched.
		/// </remarks>
		/// <param name="filter">IPV6 or IPV4 CIDR filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="filter"/> is empty string.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public void AddTcpAcceptFilter(string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
			{
				throw new ArgumentNullException("filter");
			}

			SetOption(ZSocketOption.TCP_ACCEPT_FILTER, filter);
		}

		/// <summary>
		/// Reset all TCP filters assigned by <see cref="AddTcpAcceptFilter"/> and allow TCP transport to accept connections from any IP.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public void ClearTcpAcceptFilter()
		{
			SetOption(ZSocketOption.TCP_ACCEPT_FILTER, (string)null);
		}

		public bool IPv4Only
		{
			get { return GetOptionInt32(ZSocketOption.IPV4_ONLY) == 1; }
			set { SetOption(ZSocketOption.IPV4_ONLY, value ? 1 : 0); }
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
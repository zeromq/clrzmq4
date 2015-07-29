using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using ZeroMQ.lib;

namespace ZeroMQ
{
	/// <summary>
	/// Sends and receives messages, single frames and byte frames across ZeroMQ.
	/// </summary>
	public class ZSocket : IDisposable
	{
		/// <summary>
		/// Create a <see cref="ZSocket"/> instance.
		/// </summary>
		/// <returns><see cref="ZSocket"/></returns>
		public static ZSocket Create(ZContext context, ZSocketType socketType)
		{

			return new ZSocket(context, socketType);
		}

		/// <summary>
		/// Create a <see cref="ZSocket"/> instance.
		/// </summary>
		/// <returns><see cref="ZSocket"/></returns>
		public static ZSocket Create(ZContext context, ZSocketType socketType, out ZError error)
		{
			var socket = new ZSocket();
			socket._context = context;
			socket._socketType = socketType;

			if (!socket.Initialize(out error))
			{
				return default(ZSocket);
			}
			return socket;
		}

		private ZContext _context;

		private IntPtr _socketPtr;

		private ZSocketType _socketType;

		/// <summary>
		/// Create a <see cref="ZSocket"/> instance.
		/// </summary>
		/// <returns><see cref="ZSocket"/></returns>
		public ZSocket(ZContext context, ZSocketType socketType)
		{
			_context = context;
			_socketType = socketType;

			ZError error;
			if (!Initialize(out error))
			{
				throw new ZException(error);
			}
		}

		protected ZSocket()
		{ }

		protected bool Initialize(out ZError error)
		{
			error = default(ZError);

			if (IntPtr.Zero == (_socketPtr = zmq.socket(_context.ContextPtr, (Int32)_socketType)))
			{
				error = ZError.GetLastErr();
				return false;
			}
			return true;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="ZSocket"/> class.
		/// </summary>
		~ZSocket()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="ZSocket"/>, and optionally disposes of the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				ZError error;
				Close(out error);
			}
		}

		/// <summary>
		/// Close the current socket.
		/// </summary>
		public void Close()
		{
			ZError error;
			if (!Close(out error))
			{
				throw new ZException(error);
			}
		}

		/// <summary>
		/// Close the current socket.
		/// </summary>
		public bool Close(out ZError error)
		{
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

		/// <summary>
		/// Bind the specified endpoint.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
		public void Bind(string endpoint)
		{
			ZError error;
			if (!Bind(endpoint, out error))
			{
				throw new ZException(error);
			}
		}

		/// <summary>
		/// Bind the specified endpoint.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
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

		/// <summary>
		/// Unbind the specified endpoint.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
		public void Unbind(string endpoint)
		{
			ZError error;
			if (!Unbind(endpoint, out error))
			{
				throw new ZException(error);
			}
		}

		/// <summary>
		/// Unbind the specified endpoint.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
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

		/// <summary>
		/// Connect the specified endpoint.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
		public void Connect(string endpoint)
		{
			ZError error;
			if (!Connect(endpoint, out error))
			{
				throw new ZException(error);
			}
		}

		/// <summary>
		/// Connect the specified endpoint.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
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

		/// <summary>
		/// Disconnect the specified endpoint.
		/// </summary>
		public void Disconnect(string endpoint)
		{
			ZError error;
			if (!Disconnect(endpoint, out error))
			{
				throw new ZException(error);
			}
		}

		/// <summary>
		/// Disconnect the specified endpoint.
		/// </summary>
		/// <param name="endpoint">A string consisting of a transport and an address, formatted as <c><em>transport</em>://<em>address</em></c>.</param>
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

		public int ReceiveBytes(byte[] buffer, int offset, int count)
		{
			ZError error;
			int length;
			if (0 > (length = ReceiveBytes(buffer, offset, count, ZSocketFlags.None, out error)))
			{
				throw new ZException(error);
			}
			return length;
		}

		public int ReceiveBytes(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			error = ZError.None;

			// int zmq_recv(void* socket, void* buf, size_t len, int flags);

			var pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IntPtr pinPtr = pin.AddrOfPinnedObject() + offset;

			int length;
			while (0 > (length = zmq.recv(this.SocketPtr, pinPtr, count, (int)flags)))
			{
				error = ZError.GetLastErr();
				if (error == ZError.EINTR)
				{
					error = default(ZError);
					continue;
				}
				
				break;
			}

			pin.Free();
			return length;
		}

		public bool SendBytes(byte[] buffer, int offset, int count)
		{
			ZError error;
			if (!SendBytes(buffer, offset, count, ZSocketFlags.None, out error))
			{
				throw new ZException(error);
			}
			return true;
		}

		public bool SendBytes(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			error = ZError.None;
			
			// int zmq_send (void *socket, void *buf, size_t len, int flags);

			var pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IntPtr pinPtr = pin.AddrOfPinnedObject() + offset;

			int length;
			while (0 > (length = zmq.send(SocketPtr, pinPtr, count, (int)flags)))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = default(ZError);
					continue;
				}

				pin.Free();
				return false;
			}

			pin.Free();
			return true;
		}

		public bool Send(byte[] buffer, int offset, int count) {
			return SendBytes(buffer, offset, count);
		} // just Send*
		public bool Send(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error) {
			return SendBytes(buffer, offset, count, flags, out error);
		} // just Send*

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
			ZMessage message = null;
			ReceiveMessage(ref message, flags, out error);
			return message;
		}

		public bool ReceiveMessage(ref ZMessage message, out ZError error)
		{
			return ReceiveMessage(ref message, ZSocketFlags.None, out error);
		}

		public bool ReceiveMessage(ref ZMessage message, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			int count = int.MaxValue;
			return ReceiveFrames(ref count, ref message, flags, out error);
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
			IEnumerable<ZFrame> frames = ReceiveFrames(1, flags & ~ZSocketFlags.More, out error);
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
			List<ZFrame> frames = null;
			while (!ReceiveFrames(ref framesToReceive, ref frames, flags, out error))
			{
				if (error == ZError.EAGAIN && ((flags & ZSocketFlags.DontWait) == ZSocketFlags.DontWait))
				{
					break;
				}
				return null;
			}
			return frames;
		}

		public bool ReceiveFrames<ListT>(ref int framesToReceive, ref ListT frames, ZSocketFlags flags, out ZError error)
			where ListT : IList<ZFrame>, new()
		{
			EnsureNotDisposed();

			error = default(ZError);
			flags = flags | ZSocketFlags.More;

			do {
				var frame = ZFrame.CreateEmpty();

				if (framesToReceive == 1) 
				{
					flags = flags & ~ZSocketFlags.More;
				}

				while (-1 == zmq.msg_recv(frame.Ptr, _socketPtr, (int)flags))
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

				if (frames == null)
				{
					frames = new ListT();
				}
				frames.Add(frame);

			} while (--framesToReceive > 0 && this.ReceiveMore);

			return true;
		}

		public virtual void Send(ZMessage msg) {
			SendMessage(msg);
		} // just Send*
		public virtual bool Send(ZMessage msg, out ZError error) {
			return SendMessage(msg, out error);
		} // just Send*
		public virtual void Send(ZMessage msg, ZSocketFlags flags) {
			SendMessage(msg, flags);
		} // just Send*
		public virtual bool Send(ZMessage msg, ZSocketFlags flags, out ZError error) {
			return SendMessage(msg, flags, out error);
		} // just Send*
		public virtual void Send(IEnumerable<ZFrame> frames) {
			SendFrames(frames);
		} // just Send*
		public virtual bool Send(IEnumerable<ZFrame> frames, out ZError error) {
			return SendFrames(frames, out error);
		} // just Send*
		public virtual void Send(IEnumerable<ZFrame> frames, ZSocketFlags flags) {
			SendFrames(frames, flags);
		} // just Send*
		public virtual bool Send(IEnumerable<ZFrame> frames, ZSocketFlags flags, out ZError error) {
			return SendFrames(frames, flags, out error);
		} // just Send*
		public virtual bool Send(IEnumerable<ZFrame> frames, ref int sent, ZSocketFlags flags, out ZError error) {
			return SendFrames(frames, ref sent, flags, out error);
		} // just Send*
		public virtual void Send(ZFrame frame) {
			SendFrame(frame);
		} // just Send*
		public virtual bool Send(ZFrame msg, out ZError error) {
			return SendFrame(msg, out error);
		} // just Send*
		public virtual void SendMore(ZFrame frame) {
			SendFrameMore(frame);
		} // just Send*
		public virtual bool SendMore(ZFrame msg, out ZError error) {
			return SendFrameMore(msg, out error);
		} // just Send*
		public virtual void SendMore(ZFrame frame, ZSocketFlags flags) {
			SendFrameMore(frame, flags);
		} // just Send*
		public virtual bool SendMore(ZFrame msg, ZSocketFlags flags, out ZError error) {
			return SendFrameMore(msg, flags, out error);
		} // just Send*
		public virtual void Send(ZFrame frame, ZSocketFlags flags) {
			SendFrame(frame, flags);
		} // just Send*
		public virtual bool Send(ZFrame frame, ZSocketFlags flags, out ZError error) {
			return SendFrame(frame, flags, out error);
		} // just Send*

		public virtual void SendMessage(ZMessage msg)
		{
			SendMessage(msg, ZSocketFlags.None);
		}

		public virtual bool SendMessage(ZMessage msg, out ZError error)
		{
			return SendMessage(msg, ZSocketFlags.None, out error);
		}

		public virtual void SendMessage(ZMessage msg, ZSocketFlags flags)
		{
			ZError error;
			if (!SendMessage(msg, flags, out error))
			{
				throw new ZException(error);
			}
		}

		public virtual bool SendMessage(ZMessage msg, ZSocketFlags flags, out ZError error)
		{
			return SendFrames((IEnumerable<ZFrame>)msg, flags, out error);
		}

		public virtual void SendFrames(IEnumerable<ZFrame> frames)
		{
			SendFrames(frames, ZSocketFlags.None);
		}

		public virtual bool SendFrames(IEnumerable<ZFrame> frames, out ZError error)
		{
			return SendFrames(frames, ZSocketFlags.None, out error);
		}

		public virtual void SendFrames(IEnumerable<ZFrame> frames, ZSocketFlags flags)
		{
			ZError error;
			int sent = 0;
			if (!SendFrames(frames, ref sent, flags, out error))
			{
				throw new ZException(error);
			}
		}

		public virtual bool SendFrames(IEnumerable<ZFrame> frames, ZSocketFlags flags, out ZError error)
		{
			int sent = 0;
			if (!SendFrames(frames, ref sent, flags, out error))
			{
				return false;
			}
			return true;
		}

		public virtual bool SendFrames(IEnumerable<ZFrame> frames, ref int sent, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			error = ZError.None;

			bool more = (flags & ZSocketFlags.More) == ZSocketFlags.More;
			flags = flags | ZSocketFlags.More;

			bool framesIsList = frames is IList<ZFrame>;
			ZFrame[] _frames = frames.ToArray();

			for (int i = 0, l = _frames.Length; i < l; ++i)
			{
				ZFrame frame = _frames[i];

				if (i == l - 1 && !more)
				{
					flags = flags & ~ZSocketFlags.More;
				}

				if (!SendFrame(frame, flags, out error))
				{
					return false;
				}

				if (framesIsList)
				{
					((IList<ZFrame>)frames).Remove(frame);
				}

				++sent;
			}

			return true;
		}

		public virtual void SendFrame(ZFrame frame)
		{
			SendFrame(frame, ZSocketFlags.None);
		}

		public virtual bool SendFrame(ZFrame msg, out ZError error)
		{
			return SendFrame(msg, ZSocketFlags.None, out error);
		}

		public virtual void SendFrameMore(ZFrame frame)
		{
			SendFrame(frame, ZSocketFlags.More);
		}

		public virtual bool SendFrameMore(ZFrame msg, out ZError error)
		{
			return SendFrame(msg, ZSocketFlags.More, out error);
		}

		public virtual void SendFrameMore(ZFrame frame, ZSocketFlags flags)
		{
			SendFrame(frame, flags | ZSocketFlags.More);
		}

		public virtual bool SendFrameMore(ZFrame msg, ZSocketFlags flags, out ZError error)
		{
			return SendFrame(msg, flags | ZSocketFlags.More, out error);
		}

		public virtual void SendFrame(ZFrame frame, ZSocketFlags flags)
		{
			ZError error;
			if (!SendFrame(frame, flags, out error))
			{
				throw new ZException(error);
			}
		}

		public virtual bool SendFrame(ZFrame frame, ZSocketFlags flags, out ZError error)
		{
			EnsureNotDisposed();

			if (frame.IsDismissed)
			{
				throw new ObjectDisposedException("frame");
			}

			error = default(ZError);

			while (-1 == zmq.msg_send(frame.Ptr, _socketPtr, (int)flags))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = default(ZError);
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
					while (-1 == zmq.msg_recv(msg.Ptr, this.SocketPtr, (int)ZSocketFlags.None))
					{
						error = ZError.GetLastErr();

						if (error == ZError.EINTR)
						{
							error = default(ZError);
							continue;
						}

						return false;
					}

					// will have to receive more?
					more = ReceiveMore;

					// sending scope
					while (-1 != zmq.msg_send(msg.Ptr, destination.SocketPtr, more ? (int)ZSocketFlags.More : (int)ZSocketFlags.None))
					{
						error = ZError.GetLastErr();

						if (error == ZError.EINTR)
						{
							error = default(ZError);
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
						error = default(ZError);
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
					error = default(ZError);
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
			int optionLength = Marshal.SizeOf(typeof(Int64));
			using (var optionValue = DispoIntPtr.Alloc(optionLength))
			{
				Marshal.WriteInt64(optionValue, value);

				return SetOption(option, optionValue.Ptr, optionLength);
			}
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
		public virtual void Subscribe(byte[] prefix)
		{
			SetOption(ZSocketOption.SUBSCRIBE, prefix);
		}

		/// <summary>
		/// Subscribe to messages that begin with a specified prefix.
		/// </summary>
		/// <remarks>
		/// Only applies to <see cref="ZeroMQ.ZSocketType.SUB"/> and <see cref="ZeroMQ.ZSocketType.XSUB"/> sockets.
		/// </remarks>
		/// <param name="prefix">Prefix for subscribed messages.</param>
		public virtual void Subscribe(string prefix)
		{
			SetOption(ZSocketOption.SUBSCRIBE, ZContext.Encoding.GetBytes(prefix));
		}

		/// <summary>
		/// Unsubscribe from all messages.
		/// </summary>
		/// <remarks>
		/// Only applies to <see cref="ZeroMQ.ZSocketType.SUB"/> and <see cref="ZeroMQ.ZSocketType.XSUB"/> sockets.
		/// </remarks>
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
		public virtual void Unsubscribe(byte[] prefix)
		{
			SetOption(ZSocketOption.UNSUBSCRIBE, prefix);
		}

		/// <summary>
		/// Unsubscribe from messages that begin with a specified prefix.
		/// </summary>
		/// <remarks>
		/// Only applies to <see cref="ZeroMQ.ZSocketType.SUB"/> and <see cref="ZeroMQ.ZSocketType.XSUB"/> sockets.
		/// </remarks>
		/// <param name="prefix">Prefix for subscribed messages.</param>
		public virtual void Unsubscribe(string prefix)
		{
			SetOption(ZSocketOption.UNSUBSCRIBE, ZContext.Encoding.GetBytes(prefix));
		}

		/// <summary>
		/// Gets a value indicating whether the multi-part message currently being read has more message parts to follow.
		/// </summary>
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
		public ulong Affinity
		{
			get { return GetOptionUInt64(ZSocketOption.AFFINITY); }
			set { SetOption(ZSocketOption.AFFINITY, value); }
		}

		/// <summary>
		/// Gets or sets the maximum length of the queue of outstanding peer connections. (Default = 100 connections).
		/// </summary>
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
		/// Gets or sets the Identity.
		/// </summary>
		/// <value>Identity as byte[]</value>
		public byte[] Identity
		{
			get { return GetOptionBytes(ZSocketOption.IDENTITY); }
			set { SetOption(ZSocketOption.IDENTITY, value); }
		}

		/// <summary>
		/// Gets or sets the Identity.
		/// Note: The string contains chars like \0 (null terminator,
		/// which are NOT printed (in Console.WriteLine)!
		/// </summary>
		/// <value>Identity as string</value>
		public string IdentityString
		{
			get { return ZContext.Encoding.GetString(Identity); }
			set { Identity = ZContext.Encoding.GetBytes(value); }
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
		public TimeSpan Linger
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.LINGER)); }
			set { SetOption(ZSocketOption.LINGER, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the maximum size for inbound messages (bytes). (Default = -1, no limit).
		/// </summary>
		public long MaxMessageSize
		{
			get { return GetOptionInt64(ZSocketOption.MAX_MSG_SIZE); }
			set { SetOption(ZSocketOption.MAX_MSG_SIZE, value); }
		}

		/// <summary>
		/// Gets or sets the time-to-live field in every multicast packet sent from this socket (network hops). (Default = 1 hop).
		/// </summary>
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
		public int MulticastRate
		{
			get { return GetOptionInt32(ZSocketOption.RATE); }
			set { SetOption(ZSocketOption.RATE, value); }
		}

		/// <summary>
		/// Gets or sets the underlying kernel receive buffer size for the current socket (bytes). (Default = 0, OS default).
		/// </summary>
		public int ReceiveBufferSize
		{
			get { return GetOptionInt32(ZSocketOption.RCVBUF); }
			set { SetOption(ZSocketOption.RCVBUF, value); }
		}

		/// <summary>
		/// Gets or sets the high water mark for inbound messages (number of messages). (Default = 0, no limit).
		/// </summary>
		public int ReceiveHighWatermark
		{
			get { return GetOptionInt32(ZSocketOption.RCVHWM); }
			set { SetOption(ZSocketOption.RCVHWM, value); }
		}

		/// <summary>
		/// Gets or sets the timeout for receive operations. (Default = <see cref="TimeSpan.MaxValue"/>, infinite).
		/// </summary>
		public TimeSpan ReceiveTimeout
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RCVTIMEO)); }
			set { SetOption(ZSocketOption.RCVTIMEO, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the initial reconnection interval. (Default = 100 milliseconds).
		/// </summary>
		public TimeSpan ReconnectInterval
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RECONNECT_IVL)); }
			set { SetOption(ZSocketOption.RECONNECT_IVL, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the maximum reconnection interval. (Default = 0, only use <see cref="ReconnectInterval"/>).
		/// </summary>
		public TimeSpan ReconnectIntervalMax
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RECONNECT_IVL_MAX)); }
			set { SetOption(ZSocketOption.RECONNECT_IVL_MAX, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the recovery interval for multicast transports. (Default = 10 seconds).
		/// </summary>
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
		public int SendBufferSize
		{
			get { return GetOptionInt32(ZSocketOption.SNDBUF); }
			set { SetOption(ZSocketOption.SNDBUF, value); }
		}

		/// <summary>
		/// Gets or sets the high water mark for outbound messages (number of messages). (Default = 0, no limit).
		/// </summary>
		public int SendHighWatermark
		{
			get { return GetOptionInt32(ZSocketOption.SNDHWM); }
			set { SetOption(ZSocketOption.SNDHWM, value); }
		}

		/// <summary>
		/// Gets or sets the timeout for send operations. (Default = <see cref="TimeSpan.MaxValue"/>, infinite).
		/// </summary>
		public TimeSpan SendTimeout
		{
			get { return TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.SNDTIMEO)); }
			set { SetOption(ZSocketOption.SNDTIMEO, (int)value.TotalMilliseconds); }
		}

		/// <summary>
		/// Gets or sets the override value for the SO_KEEPALIVE TCP socket option. (where supported by OS). (Default = -1, OS default).
		/// </summary>
		public TcpKeepaliveBehaviour TcpKeepAlive
		{
			get { return (TcpKeepaliveBehaviour)GetOptionInt32(ZSocketOption.TCP_KEEPALIVE); }
			set { SetOption(ZSocketOption.TCP_KEEPALIVE, (int)value); }
		}

		/// <summary>
		/// Gets or sets the override value for the 'TCP_KEEPCNT' socket option (where supported by OS). (Default = -1, OS default).
		/// The default value of '-1' means to skip any overrides and leave it to OS default.
		/// </summary>
		public int TcpKeepAliveCount
		{
			get { return GetOptionInt32(ZSocketOption.TCP_KEEPALIVE_CNT); }
			set { SetOption(ZSocketOption.TCP_KEEPALIVE_CNT, value); }
		}

		/// <summary>
		/// Gets or sets the override value for the TCP_KEEPCNT (or TCP_KEEPALIVE on some OS). (Default = -1, OS default).
		/// </summary>
		public int TcpKeepAliveIdle
		{
			get { return GetOptionInt32(ZSocketOption.TCP_KEEPALIVE_IDLE); }
			set { SetOption(ZSocketOption.TCP_KEEPALIVE_IDLE, value); }
		}

		/// <summary>
		/// Gets or sets the override value for the TCP_KEEPINTVL socket option (where supported by OS). (Default = -1, OS default).
		/// </summary>
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
			if (_socketPtr == IntPtr.Zero)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

	}
}
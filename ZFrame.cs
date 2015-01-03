using System.Text;

namespace ZeroMQ
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.IO;
	using lib;

    /// <summary>
    /// A single or multi-part message sent or received via a <see cref="ZSocket"/>.
    /// </summary>
    public class ZFrame : System.IO.Stream, IDisposable
	{
		public const int DefaultFrameSize = 2048;

		public static ZFrame CreateEmpty()
		{
			DispoIntPtr msg = CreateEmptyNative();
			return new ZFrame(msg, -1);
		}

		public static ZFrame Create(int size)
		{
			return new ZFrame (CreateNative(size), size);
		}

        public static ZFrame Create(byte[] buffer, int offset, int count)
        {
            var frame = new ZFrame(CreateNative(count), count);
            frame.Write(buffer, offset, count);
            return frame;
        }

        public static ZFrame Create(string str)
        {
            return Create(str, ZContext.Encoding);
        }

        public static ZFrame Create(string str, Encoding encoding)
        {
            return WriteStringNative(null, str, encoding);
        }

		/* public static ZmqFrame Create(IntPtr data, int size)
		{
			return new ZmqFrame (Alloc(data, size), size);
		} */

		internal static DispoIntPtr CreateEmptyNative()
		{
			var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);

			while (-1 == zmq.msg_init(msg)) {
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR) {
					continue;
				}

				msg.Dispose();

				throw new ZException (error, "zmq_msg_init");
			}

			return msg;
		}

		internal static DispoIntPtr CreateNative(int size) 
		{
			var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);

			while (-1 == zmq.msg_init_size(msg, size)) {
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR) {
					continue;
				}

				msg.Dispose();

				if (error == ZError.ENOMEM) {
					throw new OutOfMemoryException ("zmq_msg_init_size");
				}
				throw new ZException (error, "zmq_msg_init_size");
			}
			return msg;
		}

		unsafe internal static ZFrame WriteStringNative(ZFrame frame, string str, Encoding encoding) 
		{
			int charCount = str.Length;
			Encoder enc = encoding.GetEncoder();
			bool create = (frame == null);

			fixed (char* strP = str) {
				int byteCount = enc.GetByteCount(strP, charCount, false);

				if (create) {
					// return a new one
					frame = ZFrame.Create(byteCount);
				}
				else {
					if (frame._position + byteCount > frame.Length) {
						// fail if frame is too small
						throw new InvalidOperationException ();
					}
				}

				byteCount = enc.GetBytes(strP, charCount, (byte*)(frame.DataPtr() + frame._position), byteCount, true);
				frame._position += byteCount;
			}
			return frame;
		}

		unsafe public static string ReadStringNative(ZFrame frame, int byteCount, Encoding encoding) 
		{
			int remaining = Math.Min(byteCount, Math.Max(0, (int)(frame.Length - frame._position)));
			if (remaining <= 0) {
				return null;
			}

			var bytes = (byte*)(frame.DataPtr() + frame._position);

			Decoder dec = encoding.GetDecoder();
			int charCount = dec.GetCharCount(bytes, remaining, false);

			var resultChars = new char[charCount];
			string resultString;
			fixed (char* chars = resultChars) {
				charCount = dec.GetChars(bytes, remaining, chars, charCount, true);
				resultString = new string (chars, 0, charCount);
			}
			return resultString;
		}

		/* internal static DispoIntPtr Alloc(IntPtr data, int size) 
		{
			var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);

			while (-1 == zmq.msg_init_data(msg, data, size, /* msg_free_delegate null, /* hint IntPtr.Zero)) {
				ZmqError error = ZmqContext.GetLastError();

				if (error == ZmqError.EINTR) {
					continue;
				}

				msg.Dispose();

				if (error == ZmqError.ENOMEM) {
					throw new OutOfMemoryException ("zmq_msg_init_size");
				}
				throw new ZmqException (error, "zmq_msg_init_size");
			}
			return msg;
		} */

		private DispoIntPtr _framePtr;
		private int _capacity;
		private int _position;
		// private LibZmq.FreeMessageDataDelegate _freePtrr;

		private ZFrame(DispoIntPtr msgPtr, int capacity) {
			_framePtr = msgPtr;
			_capacity = capacity;
			_position = 0;
		}

		protected override void Dispose(bool disposing)
		{
			if (_framePtr != null) {
				if (_framePtr.Ptr != IntPtr.Zero) {
					Close();
				}
				Dismiss();
			}
			base.Dispose(disposing);
		}

		public void Dismiss()
		{
			if (_framePtr == null)
				return;

			// Unalloc the HGlobal ? (currently leads to SIGSEGV)
			_framePtr.Dispose();
			_framePtr = null;
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return true;
			}
		}

		public override bool CanTimeout {
			get {
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return true;
			}
		}

		private void EnsureCapacity()
		{
			if (_framePtr != IntPtr.Zero) {
				_capacity = zmq.msg_size(_framePtr);
			} else {
				_capacity = -1;
			}
		}

		public override long Length {
			get {
				EnsureCapacity();
				return _capacity;
			}
		}

		public override void SetLength(long length)
		{
			throw new NotSupportedException ();
		}

		public override long Position {
			get {
				return _position;
			}
			set {
				if (value == 0) {
					_position = 0;
					return;
				}
				if (value < 0 || (Length == -1 || value > Length)) {
					throw new IndexOutOfRangeException ();
				}
				_position = (int)value;
			}
		}

		public IntPtr Ptr { get { return _framePtr; } }

		public IntPtr DataPtr() { 
			if (_framePtr == IntPtr.Zero) {
				return IntPtr.Zero;
			}
			return zmq.msg_data(_framePtr); 
		}

		/*
		public byte* PositionPtr()
		{
			return (byte*)_ptr + _position;
		}*/

		public override long Seek(long offset, SeekOrigin origin)
		{
			long pos;
			if (origin == SeekOrigin.Current)
				pos = Position + offset;
			else if (origin == SeekOrigin.End)
				pos = Length + offset;
			else // if (origin == SeekOrigin.Begin)
				pos = offset; 

			if (pos < 0 || (Length > 0 && pos > Length))
				throw new ArgumentOutOfRangeException ("offset");

			_position = (int)pos;
			return pos;
		}

		unsafe public override int Read(byte[] buffer, int offset, int count)
		{
			int remaining = Math.Min(count, Math.Max(0, (int)(Length - Position)));
			if (remaining <= 0) {
				return -1;
			}
			// MemoryUtils.PinAndCopyMemory((byte*)(DataPtr() + _position), buffer, offset, count);
			Marshal.Copy(DataPtr() + _position, buffer, offset, (int)remaining);

			_position += remaining;
			return remaining;
		}
		
		public override int ReadByte()
		{
			if (Position + 1 > Length)
				return -1;

			int byt = Marshal.ReadByte(DataPtr() + _position);
			++_position;
			return byt;
        }

        public virtual Int16 ReadInt16()
        {
            var bytes = new byte[2];
            int len = Read(bytes, 0, 2);
            if (len < 2)
            {
                return default(Int16);
            }

            return BitConverter.ToInt16(bytes, 0);
        }

        public virtual Int32 ReadInt32()
        {
            var bytes = new byte[4];
            int len = Read(bytes, 0, 4);
            if (len < 4)
            {
                return default(Int16);
            }

            return BitConverter.ToInt16(bytes, 0);
        }

        public virtual Int32 ReadInt64()
        {
            var bytes = new byte[8];
            int len = Read(bytes, 0, 8);
            if (len < 8)
            {
                return default(Int16);
            }

            return BitConverter.ToInt16(bytes, 0);
        }

		public string ReadString() {
			return ReadString(ZContext.Encoding);
		}

		public string ReadString(Encoding encoding) {
			return ReadString( /* byteCount */ (int)Length, encoding );
		}

		public string ReadString(int length) {
			return ReadString( /* byteCount */ length, ZContext.Encoding );
		}

		public string ReadString(int length, Encoding encoding) {
			int byteCount = encoding.GetMaxByteCount(length);
			return ReadStringNative(this, byteCount, encoding);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (Position + count > Length) {
				throw new InvalidOperationException ();
			}
			// MemoryUtils.PinAndCopyMemory(buffer, offset, (byte*)(DataPtr() + _position), count);
			Marshal.Copy(buffer, offset, DataPtr() + _position, count);
			_position += count;
		}

		public override void WriteByte(byte value)
		{
			if (Position + 1 > Length) {
				throw new InvalidOperationException ();
			}
			Marshal.WriteByte(DataPtr() + _position, value);
			++_position;
		}

		public void WriteString(string str) {
			WriteString(str, ZContext.Encoding);
		}
		
		public void WriteString(string str, Encoding encoding) 
		{
			ZFrame me = WriteStringNative(this, str, encoding);

			if (!object.ReferenceEquals(this, me)) {
				// shouldn't have returned a new one
				throw new InvalidOperationException ();
			}
		}

		public void ZeroCopyTo(ZFrame other) {

			// zmq.msg_copy(dest, src)
			while (-1 == zmq.msg_copy(other._framePtr, _framePtr)) {
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR) {
					continue;
				}
				if (error == ZError.EFAULT) {
					// Invalid message. 
				}
				throw new ZException (error, "zmq_msg_copy");
			}
		}

		public void ZeroMoveTo(ZFrame other) {

			// zmq.msg_copy(dest, src)
			while (-1 == zmq.msg_move(other._framePtr, _framePtr)) {
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR) {
					continue;
				}
				if (error == ZError.EFAULT) {
					// Invalid message. 
				}
				throw new ZException (error, "zmq_msg_move");
			}

			// When move, msg_close this _framePtr
			Close();
		}

		public override void Flush()
		{
			throw new NotSupportedException ();
		}

		public override void Close()
		{
			if (_framePtr == null)
				return;
			
			if (_framePtr.Ptr == IntPtr.Zero) {
				_framePtr = null;
				return;
			}

			while (-1 == zmq.msg_close(_framePtr)) {
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR) {
					continue;
				}
				if (error == ZError.EFAULT) {
					// Ignore: Invalid message.
					break;
				}
				throw new ZException (error, "zmq_msg_close");
			}

			// Go unallocating the HGlobal
			Dismiss();
		}
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using ZeroMQ.lib;

namespace ZeroMQ
{

	/// <summary>
	/// A single or multi-part message sent or received via a <see cref="ZSocket"/>.
	/// </summary>
	public class ZFrame : System.IO.Stream, IDisposable
	{
		public const int DefaultFrameSize = 4096;

		public static readonly int MinimumFrameSize = zmq.sizeof_zmq_msg_t;

		private DispoIntPtr _framePtr;

		private int _capacity;

		private int _position;

		// private ZeroMQ.lib.FreeMessageDelegate _freePtr;

		public ZFrame(byte[] buffer)
			: this (CreateNative(buffer.Length), buffer.Length)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		public ZFrame(byte[] buffer, int offset, int count)
			: this(CreateNative(count), count)
		{
			this.Write(buffer, offset, count);
		}

		public ZFrame(string str)
			: this(str, ZContext.Encoding)
		{ }

		public ZFrame(string str, Encoding encoding)
		{
			WriteStringNative(str, encoding, true);
		}

		public ZFrame()
			: this(CreateNative(0), 0)
		{ }

		/* protected ZFrame(IntPtr data, int size)
			: this(Alloc(data, size), size)
		{ } */

		public static ZFrame Create(int size) 
		{
			if (size < 0)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			return new ZFrame(CreateNative(size), size);
		}

		public static ZFrame CreateEmpty()
		{
			return new ZFrame(CreateEmptyNative(), -1);
		}

		protected ZFrame(DispoIntPtr framePtr, int size)
		{
			_framePtr = framePtr;
			_capacity = size;
			_position = 0;
		}

		internal static DispoIntPtr CreateEmptyNative()
		{
			var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);

			while (-1 == zmq.msg_init(msg))
			{
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					continue;
				}

				msg.Dispose();

				throw new ZException(error, "zmq_msg_init");
			}

			return msg;
		}

		internal static DispoIntPtr CreateNative(int size)
		{
			var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);

			while (-1 == zmq.msg_init_size(msg, size))
			{
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					continue;
				}

				msg.Dispose();

				if (error == ZError.ENOMEM)
				{
					throw new OutOfMemoryException("zmq_msg_init_size");
				}
				throw new ZException(error, "zmq_msg_init_size");
			}
			return msg;
		}

		unsafe internal void WriteStringNative(string str, Encoding encoding, bool createOnWrongLength)
		{
			if (string.IsNullOrEmpty(str))
			{
				return;
			}

			int charCount = str.Length;
			Encoder enc = encoding.GetEncoder();

			fixed (char* strP = str)
			{
				int byteCount = enc.GetByteCount(strP, charCount, false);

				if (createOnWrongLength)
				{
					this._framePtr = CreateNative(byteCount);
					this._position = 0;
					this._capacity = byteCount;
				}

				if (this._position + byteCount > this.Length)
				{
					// fail if frame is too small
					throw new InvalidOperationException();
				}

				byteCount = enc.GetBytes(strP, charCount, (byte*)(this.DataPtr() + this._position), byteCount, true);
				this._position += byteCount;
			}
		}

		unsafe internal string ReadStringNative(int byteCount, Encoding encoding)
		{
			int remaining = Math.Min(byteCount, Math.Max(0, (int)(this.Length - this._position)));
			if (remaining <= 0)
			{
				return null;
			}

			var bytes = (byte*)(this.DataPtr() + this._position);

			Decoder dec = encoding.GetDecoder();
			int charCount = dec.GetCharCount(bytes, remaining, false);

			var resultChars = new char[charCount];
			string resultString;
			fixed (char* chars = resultChars)
			{
				charCount = dec.GetChars(bytes, remaining, chars, charCount, true);
				resultString = new string(chars, 0, charCount);
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

		protected override void Dispose(bool disposing)
		{
			if (_framePtr != null)
			{
				if (_framePtr.Ptr != IntPtr.Zero)
				{
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

			_framePtr.Dispose();
			_framePtr = null;
		}

		public bool IsDismissed
		{
			get { return _framePtr == null || _framePtr == IntPtr.Zero; }
		}

		public override bool CanRead { get { return true; } }

		public override bool CanSeek { get { return true; } }

		public override bool CanTimeout { get { return false; } }

		public override bool CanWrite { get { return true; } }

		private void EnsureCapacity()
		{
			if (_framePtr != IntPtr.Zero)
			{
				_capacity = zmq.msg_size(_framePtr);
			}
			else
			{
				_capacity = -1;
			}
		}

		public override long Length
		{
			get
			{
				EnsureCapacity();
				return _capacity;
			}
		}

		public override void SetLength(long length)
		{
			throw new NotSupportedException();
		}

		public override long Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (value == 0)
				{
					_position = 0;
					return;
				}
				if (value < 0 || (Length == -1 || value > Length))
				{
					throw new IndexOutOfRangeException();
				}
				_position = (int)value;
			}
		}

		public IntPtr Ptr { get { return _framePtr; } }

		public IntPtr DataPtr()
		{
			if (_framePtr == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			return zmq.msg_data(_framePtr);
		}

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
				throw new ArgumentOutOfRangeException("offset");

			_position = (int)pos;
			return pos;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int remaining = Math.Min(count, Math.Max(0, (int)(Length - Position)));
			if (remaining == 0) {
				return 0;
			}
			if (remaining < 0)
			{
				return -1;
			}
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

		public virtual UInt16 ReadUInt16()
		{
			var bytes = new byte[2];
			int len = Read(bytes, 0, 2);
			if (len < 2)
			{
				return default(UInt16);
			}

			return BitConverter.ToUInt16(bytes, 0);
		}

		public virtual Char ReadChar()
		{
			var bytes = new byte[2];
			int len = Read(bytes, 0, 2);
			if (len < 2)
			{
				return default(Char);
			}

			return BitConverter.ToChar(bytes, 0);
		}

		public virtual Int32 ReadInt32()
		{
			var bytes = new byte[4];
			int len = Read(bytes, 0, 4);
			if (len < 4)
			{
				return default(Int32);
			}

			return BitConverter.ToInt32(bytes, 0);
		}

		public virtual UInt32 ReadUInt32()
		{
			var bytes = new byte[4];
			int len = Read(bytes, 0, 4);
			if (len < 4)
			{
				return default(UInt32);
			}

			return BitConverter.ToUInt32(bytes, 0);
		}

		public virtual Int64 ReadInt64()
		{
			var bytes = new byte[8];
			int len = Read(bytes, 0, 8);
			if (len < 8)
			{
				return default(Int64);
			}

			return BitConverter.ToInt64(bytes, 0);
		}

		public virtual UInt64 ReadUInt64()
		{
			var bytes = new byte[8];
			int len = Read(bytes, 0, 8);
			if (len < 8)
			{
				return default(UInt64);
			}

			return BitConverter.ToUInt64(bytes, 0);
		}

		public string ReadString()
		{
			return ReadString(ZContext.Encoding);
		}

		public string ReadString(Encoding encoding)
		{
			return ReadString( /* byteCount */ (int)Length, encoding);
		}

		public string ReadString(int length)
		{
			return ReadString( /* byteCount */ length, ZContext.Encoding);
		}

		public string ReadString(int length, Encoding encoding)
		{
			int byteCount = encoding.GetMaxByteCount(length);
			return ReadStringNative(byteCount, encoding);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (Position + count > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.Copy(buffer, offset, DataPtr() + _position, count);
			_position += count;
		}

		public virtual void Write(byte value)
		{
			if (Position + 1 > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.WriteByte(DataPtr() + _position, value);
			++_position;
		}

		public override void WriteByte(byte value)
		{
			Write(value);
		}

		public void Write(Int16 value)
		{
			if (Position + 2 > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.WriteInt16(DataPtr() + _position, value);
			_position += 2;
		}

		public void Write(UInt16 value)
		{
			Write((Int16)value);
		}

		public void Write(Char value)
		{
			Write((Int16)value);
		}

		public void Write(Int32 value)
		{
			if (Position + 4 > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.WriteInt32(DataPtr() + _position, value);
			_position += 4;
		}

		public void Write(UInt32 value)
		{
			Write((Int32)value);
		}

		public void Write(Int64 value)
		{
			if (Position + 8 > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.WriteInt64(DataPtr() + _position, value);
			_position += 8;
		}

		public void Write(UInt64 value)
		{
			Write((Int64)value);
		}

		public void Write(string str)
		{
			Write(str, ZContext.Encoding);
		}

		public void Write(string str, Encoding encoding)
		{
			WriteStringNative(str, encoding, false);
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override void Close()
		{
			if (_framePtr == null)
				return;

			if (_framePtr.Ptr == IntPtr.Zero)
			{
				_framePtr = null;
				return;
			}

			while (-1 == zmq.msg_close(_framePtr))
			{
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					continue;
				}
				if (error == ZError.EFAULT)
				{
					// Ignore: Invalid message.
					break;
				}
				throw new ZException(error, "zmq_msg_close");
			}

			// Go unallocating the HGlobal
			Dismiss();
		}

		public void CopyZeroTo(ZFrame other)
		{

			// zmq.msg_copy(dest, src)
			while (-1 == zmq.msg_copy(other._framePtr, _framePtr))
			{
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					continue;
				}
				if (error == ZError.EFAULT)
				{
					// Invalid message. 
				}
				throw new ZException(error, "zmq_msg_copy");
			}
		}

		public void MoveZeroTo(ZFrame other)
		{

			// zmq.msg_copy(dest, src)
			while (-1 == zmq.msg_move(other._framePtr, _framePtr))
			{
				var error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					continue;
				}
				if (error == ZError.EFAULT)
				{
					// Invalid message. 
				}
				throw new ZException(error, "zmq_msg_move");
			}

			// When move, msg_close this _framePtr
			Close();
		}

		public int GetOption(int property)
		{
			ZError error;
			int result;
			if (-1 == (result = GetOption(property, out error)))
			{
				throw new ZException(error);
			}
			return result;
		}

		public int GetOption(int property, out ZError error)
		{
			error = ZError.None;

			int result;
			if (-1 == (result = zmq.msg_get(this._framePtr, property)))
			{
				error = ZError.GetLastErr();
				return -1;
			}
			return result;
		}

		public string GetOption(string property)
		{
			ZError error;
			string result;
			if (null == (result = GetOption(property, out error))) {
				if (error != ZError.None)
				{
					throw new ZException(error);
				}
			}
			return result;
		}

		public string GetOption(string property, out ZError error)
		{
			error = ZError.None;

			string result = null;
			using (var propertyPtr = DispoIntPtr.AllocString(property))
			{
				IntPtr resultPtr;
				if (IntPtr.Zero == (resultPtr = zmq.msg_gets(this._framePtr, propertyPtr)))
				{
					error = ZError.GetLastErr();
					return null;
				}
				else
				{
					result = Marshal.PtrToStringAnsi(resultPtr);
				}
			}
			return result;
		}
	}
}
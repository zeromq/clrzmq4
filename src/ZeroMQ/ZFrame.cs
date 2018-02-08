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
	/// A single part message, sent or received via a <see cref="ZSocket"/>.
	/// </summary>
	public sealed class ZFrame : Stream, ICloneable
	{
		public static ZFrame FromStream(Stream stream, long i, int l)
		{
			stream.Position = i;
			if (l == 0) return new ZFrame();

			var frame = ZFrame.Create(l);
			var buf = new byte[65535];
			int bufLen, remaining, current = -1;
			do {
				++current;
				remaining = Math.Min(Math.Max(0, l - current * buf.Length), buf.Length);
				if (remaining < 1) break;
				
				bufLen = stream.Read(buf, 0, remaining);
				frame.Write(buf, 0, bufLen);

			} while (bufLen > 0);

			frame.Position = 0;
			return frame;
		}

		public static ZFrame CopyFrom(ZFrame frame)
		{
			return frame.Duplicate();
		}

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

		public const int DefaultFrameSize = 4096;

		public static readonly int MinimumFrameSize = zmq.sizeof_zmq_msg_t;

		private DispoIntPtr framePtr;

		private int length;

		private int position;

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

		public ZFrame(byte value)
			: this(CreateNative(1), 1)
		{ 
			this.Write(value);
		}

		public ZFrame(Int16 value)
			: this(CreateNative(2), 2)
		{ 
			this.Write(value);
		}

		public ZFrame(UInt16 value)
			: this(CreateNative(2), 2)
		{ 
			this.Write(value);
		}

		public ZFrame(Char value)
			: this(CreateNative(2), 2)
		{ 
			this.Write(value);
		}

		public ZFrame(Int32 value)
			: this(CreateNative(4), 4)
		{ 
			this.Write(value);
		}

		public ZFrame(UInt32 value)
			: this(CreateNative(4), 4)
		{ 
			this.Write(value);
		}

		public ZFrame(Int64 value)
			: this(CreateNative(8), 8)
		{ 
			this.Write(value);
		}

		public ZFrame(UInt64 value)
			: this(CreateNative(8), 8)
		{ 
			this.Write(value);
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

		internal ZFrame(DispoIntPtr frameIntPtr, int size)
			: base()
		{
			framePtr = frameIntPtr;
			length = size;
			position = 0;
		}

		~ZFrame()
		{
			Dispose(false);
		}

		internal static DispoIntPtr CreateEmptyNative()
		{
			var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);

			ZError error;
			while (-1 == zmq.msg_init(msg))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = default(ZError);
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

			ZError error;
			while (-1 == zmq.msg_init_size(msg, size))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = default(ZError);
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

		/* internal static DispoIntPtr Alloc(IntPtr data, int size) 
		{
			var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);

			ZError error;
			while (-1 == zmq.msg_init_data(msg, data, size, /* msg_free_delegate null, /* hint IntPtr.Zero)) {
				error = ZError.GetLastError();

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
		} */

		protected override void Dispose(bool disposing)
		{
			if (framePtr != null)
			{
				if (framePtr.Ptr != IntPtr.Zero)
				{
					Close();
				}
			}
			GC.SuppressFinalize(this);
			base.Dispose(disposing);
		}

		public void Dismiss()
		{
			if (framePtr != null)
			{
				framePtr.Dispose();
				framePtr = null;
			}
			GC.SuppressFinalize(this);
		}

		public bool IsDismissed
		{
			get { return framePtr == null; }
		}

		public override bool CanRead { get { return true; } }

		public override bool CanSeek { get { return true; } }

		public override bool CanTimeout { get { return false; } }

		public override bool CanWrite { get { return true; } }

		private void EnsureCapacity()
		{
			if (framePtr != IntPtr.Zero)
			{
				length = zmq.msg_size(framePtr);
			}
			else
			{
				length = -1;
			}
		}

		public override long Length
		{
			get
			{
				EnsureCapacity();
				return length;
			}
		}

		public override void SetLength(long length)
		{
			throw new NotSupportedException();
		}

		public override long Position
		{
			get { return position; }
			set
			{
				Seek(value, SeekOrigin.Begin);
			}
		}

		public IntPtr Ptr { get { return framePtr; } }

		public IntPtr DataPtr()
		{
			if (framePtr == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			return zmq.msg_data(framePtr);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long pos;
			if (origin == SeekOrigin.Current)
				pos = position + offset;
			else if (origin == SeekOrigin.End)
				pos = Length + offset;
			else // if (origin == SeekOrigin.Begin)
				pos = offset;

			if (pos < 0 || pos > Length)
				throw new ArgumentOutOfRangeException("offset");

			position = (int)pos;
			return pos;
		}

		public byte[] Read()
		{
			int remaining = Math.Max(0, (int)(Length - position));
			return Read(remaining);
		}

		public byte[] Read(int count)
		{
			int remaining = Math.Min(count, Math.Max(0, (int)(Length - position)));
			if (remaining == 0) {
				return new byte[0];
			}
			if (remaining < 0)
			{
				return null;
			}
			var bytes = new byte[remaining];
			/* int bytesLength = */ Read(bytes, 0, remaining);
			return bytes;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int remaining = Math.Min(count, Math.Max(0, (int)(Length - position)));
			if (remaining == 0) {
				return 0;
			}
			if (remaining < 0)
			{
				return -1;
			}
			Marshal.Copy(DataPtr() + position, buffer, offset, (int)remaining);

			position += remaining;
			return remaining;
		}

		public override int ReadByte()
		{
			if (position + 1 > Length)
				return -1;

			int byt = Marshal.ReadByte(DataPtr() + (int)position);
			++position;
			return byt;
		}

		public byte ReadAsByte()
		{
			if (position + 1 > Length)
				return default(byte);

			byte byt = Marshal.ReadByte(DataPtr() + position);
			++position;
			return byt;
		}

		public Int16 ReadInt16()
		{
			var bytes = new byte[2];
			int len = Read(bytes, 0, 2);
			if (len < 2)
			{
				return default(Int16);
			}

			return BitConverter.ToInt16(bytes, 0);
		}

		public UInt16 ReadUInt16()
		{
			var bytes = new byte[2];
			int len = Read(bytes, 0, 2);
			if (len < 2)
			{
				return default(UInt16);
			}

			return BitConverter.ToUInt16(bytes, 0);
		}

		public Char ReadChar()
		{
			var bytes = new byte[2];
			int len = Read(bytes, 0, 2);
			if (len < 2)
			{
				return default(Char);
			}

			return BitConverter.ToChar(bytes, 0);
		}

		public Int32 ReadInt32()
		{
			var bytes = new byte[4];
			int len = Read(bytes, 0, 4);
			if (len < 4)
			{
				return default(Int32);
			}

			return BitConverter.ToInt32(bytes, 0);
		}

		public UInt32 ReadUInt32()
		{
			var bytes = new byte[4];
			int len = Read(bytes, 0, 4);
			if (len < 4)
			{
				return default(UInt32);
			}

			return BitConverter.ToUInt32(bytes, 0);
		}

		public Int64 ReadInt64()
		{
			var bytes = new byte[8];
			int len = Read(bytes, 0, 8);
			if (len < 8)
			{
				return default(Int64);
			}

			return BitConverter.ToInt64(bytes, 0);
		}

		public UInt64 ReadUInt64()
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
			return ReadString( /* byteCount */ (int)Length - position, encoding);
		}

		public string ReadString(int length)
		{
			return ReadString(/* byteCount */ length, ZContext.Encoding);
		}

		public string ReadString(int byteCount, Encoding encoding)
		{
			int remaining = Math.Min(byteCount, Math.Max(0, (int)Length - position));
			if (remaining == 0)
			{
				return string.Empty;
			}
			if (remaining < 0)
			{
				return null;
			}

			unsafe
			{
				var bytes = (byte*)(this.DataPtr() + position);

				Decoder dec = encoding.GetDecoder();
				int charCount = dec.GetCharCount(bytes, remaining, false);
				if (charCount == 0)
				{
					return string.Empty;
				}

				var resultChars = new char[charCount];
				fixed (char* chars = resultChars)
				{
					charCount = dec.GetChars(bytes, remaining, chars, charCount, true);

					int i = -1, z = 0;
					while (i < charCount)
					{
						++i;

						if (chars[i] == '\0')
						{
							charCount = i;
							++z;

							break;
						}
					}

					Encoder enc = encoding.GetEncoder();
					position += enc.GetByteCount(chars, charCount + z, true);

					if (charCount == 0) return string.Empty;
					return new string(chars, 0, charCount);
				}
			}
		}

		public string ReadLine()
		{
			return ReadLine((int)Length - position, ZContext.Encoding);
		}

		public string ReadLine(Encoding encoding)
		{
			return ReadLine((int)Length - position, encoding);
		}

		public string ReadLine(int byteCount, Encoding encoding)
		{
			int remaining = Math.Min(byteCount, Math.Max(0, (int)Length - position));
			if (remaining == 0)
			{
				return string.Empty;
			}
			if (remaining < 0)
			{
				return null;
			}

			unsafe
			{
				var bytes = (byte*)(this.DataPtr() + position);

				Decoder dec = encoding.GetDecoder();
				int charCount = dec.GetCharCount(bytes, remaining, false);
				if (charCount == 0) return string.Empty;

				var resultChars = new char[charCount];
				fixed (char* chars = resultChars)
				{
					charCount = dec.GetChars(bytes, remaining, chars, charCount, true);

					int i = -1, z = 0;
					while (i < charCount)
					{
						++i;

						if (chars[i] == '\n')
						{
							charCount = i;
							++z;

							if (i - 1 > -1 && chars[i - 1] == '\r')
							{
								--charCount;
								++z;
							}

							break;
						}
						if (chars[i] == '\0')
						{
							charCount = i;
							++z;

							break;
						}
					}

					Encoder enc = encoding.GetEncoder();
					position += enc.GetByteCount(chars, charCount + z, true);

					if (charCount == 0) return string.Empty;
					return new string(chars, 0, charCount);
				}
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (position + count > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.Copy(buffer, offset, DataPtr() + position, count);
			position += count;
		}

		public void Write(byte value)
		{
			if (position + 1 > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.WriteByte(DataPtr() + position, value);
			++position;
		}

		public override void WriteByte(byte value)
		{
			Write(value);
		}

		public void Write(Int16 value)
		{
			if (position + 2 > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.WriteInt16(DataPtr() + position, value);
			position += 2;
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
			if (position + 4 > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.WriteInt32(DataPtr() + position, value);
			position += 4;
		}

		public void Write(UInt32 value)
		{
			Write((Int32)value);
		}

		public void Write(Int64 value)
		{
			if (position + 8 > Length)
			{
				throw new InvalidOperationException();
			}
			Marshal.WriteInt64(DataPtr() + position, value);
			position += 8;
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

		public void WriteLine(string str)
		{
			WriteLine(str, ZContext.Encoding);
		}

		public void WriteLine(string str, Encoding encoding)
		{
			WriteStringNative(string.Format("{0}\r\n", str), encoding, false);
		}

		unsafe internal void WriteStringNative(string str, Encoding encoding, bool create)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			if (str == string.Empty)
			{
				if (create)
				{
					this.framePtr = CreateNative(0);
					this.length = 0;
					this.position = 0;
				}
				return;
			}

			int charCount = str.Length;
			Encoder enc = encoding.GetEncoder();

			fixed (char* strP = str)
			{
				int byteCount = enc.GetByteCount(strP, charCount, false);

				if (create)
				{
					this.framePtr = CreateNative(byteCount);
					this.length = byteCount;
					this.position = 0;
				}
				else if (this.position + byteCount > this.Length)
				{
					// fail if frame is too small
					throw new InvalidOperationException();
				}

				byteCount = enc.GetBytes(strP, charCount, (byte*)(this.DataPtr() + this.position), byteCount, true);
				this.position += byteCount;
			}
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override void Close()
		{
			if (framePtr == null)
				return;

			if (framePtr.Ptr == IntPtr.Zero)
			{
				Dismiss();
				return;
			}

			ZError error;
			while (-1 == zmq.msg_close(framePtr))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = default(ZError);
					continue;
				}
				if (error == ZError.EFAULT)
				{
					// Ignore: Invalid message.
					break;
				}
				return;
			}

			// Go unallocating the HGlobal
			Dismiss();
		}

		public void CopyZeroTo(ZFrame other)
		{
			// zmq.msg_copy(dest, src)
			ZError error;
			while (-1 == zmq.msg_copy(other.framePtr, framePtr))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = default(ZError);
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
			ZError error;
			while (-1 == zmq.msg_move(other.framePtr, framePtr))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = default(ZError);
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

		public Int32 GetOption(ZFrameOption property)
		{
			Int32 result;
			ZError error;
			if (-1 == (result = GetOption(property, out error)))
			{
				throw new ZException(error);
			}
			return result;
		}

		public Int32 GetOption(ZFrameOption property, out ZError error)
		{
			error = ZError.None;

			int result;
			if (-1 == (result = zmq.msg_get(this.framePtr, (Int32)property)))
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
				if (IntPtr.Zero == (resultPtr = zmq.msg_gets(this.framePtr, propertyPtr)))
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

		#region ICloneable implementation

		public object Clone()
		{
			return Duplicate();
		}

		public ZFrame Duplicate()
		{
			var frame = ZFrame.CreateEmpty();
			this.CopyZeroTo(frame);
			return frame;
		}

		#endregion

		public override string ToString()
		{
			return ToString(ZContext.Encoding);
		}

		public string ToString(Encoding encoding)
		{
			if (Length > -1)
			{
				long old = position;
				Position = 0;
				string retur = ReadString(encoding);
				Position = old;
				return retur;
			}
			return GetType().FullName;
		}
	}
}
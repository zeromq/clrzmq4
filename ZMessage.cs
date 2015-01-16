using System.Text;

namespace ZeroMQ
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// A single or multi-part message sent or received via a <see cref="ZSocket"/>.
	/// ZmqMessage is a List(Of <see cref="ZFrame"/>) also a Stream, 
	/// which allocates pages of ZmqFrame.DefaultAllocSize for writing.
	/// </summary>
	public class ZMessage : Stream, IList<ZFrame>, IDisposable
	{
		private List<ZFrame> _frames;
		private int _current = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="ZMessage"/> class.
		/// Creates an empty message.
		/// </summary>
		public ZMessage()
			: this(Enumerable.Empty<ZFrame>())
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ZMessage"/> class.
		/// Creates a message that contains the given <see cref="Frame"/> objects.
		/// </summary>
		/// <param name="frames">A collection of <see cref="Frame"/> objects to be stored by this <see cref="ZMessage"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="frames"/> is null.</exception>
		public ZMessage(IEnumerable<ZFrame> frames)
		{
			if (frames == null)
			{
				throw new ArgumentNullException("frames");
			}

			_frames = new List<ZFrame>(frames);
			_current = _frames.Count - 1;
		}

		protected override void Dispose(bool disposing)
		{
			if (_frames != null)
			{
				foreach (ZFrame frame in _frames)
				{
					frame.Dispose();
				}
			}
			_frames = null;
			base.Dispose(disposing);
		}

		public void Dismiss()
		{
			if (_frames != null)
			{
				foreach (ZFrame frame in _frames)
				{
					frame.Dismiss();
				}
			}
			_frames = null;
		}

		public ZFrame CurrentFrame
		{
			get
			{
				if (_current > -1)
				{
					return _frames[_current];
				}
				return null;
			}
		}

		public override bool CanRead { get { return true; } }

		public override bool CanSeek { get { return false; } }

		public override bool CanTimeout { get { return false; } }

		public override bool CanWrite { get { return true; } }

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override long Length
		{
			get
			{
				long size = 0;
				for (int i = 0, l = _frames.Count; i < l; ++i)
				{
					size += _frames[i].Length;
				}
				return size;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override long Position
		{
			get
			{
				long pos = 0;
				for (int i = 0; i < _current; ++i)
				{
					pos += _frames[i].Length;
				}
				if (_current > -1 && _current < _frames.Count)
				{
					pos += _frames[_current].Position;
				}
				return pos;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		private int EnsureCapacity(ZFrame frame, int required)
		{

			// Check if the current or has some bytes to write to
			if (frame != null)
			{
				return Math.Min(required, Math.Max(0, (int)(frame.Length - frame.Position)));
			}
			return 0;
		}

		public ZFrame AppendFrame(int size)
		{

			// int size = Math.Max(requiredBytes, ZmqFrame.DefaultFrameSize);

			// new ZmqFrame, DefaultAllocSize
			var frame = ZFrame.Create(size);

			_frames.Add(frame);
			_current = _frames.Count - 1;

			return frame; // Math.Min(requiredBytes, size); // remaining bytes or how much are required
		}

		private ZFrame MoveNextFrame() 
		{
			return MoveNextFrame(ZFrame.DefaultFrameSize);
		}

		private ZFrame MoveNextFrame(int size)
		{
			if (_current + 1 >= _frames.Count)
			{
				if (size > 0)
				{
					return AppendFrame(size);
				}
				return null;
			}

			++_current;
			ZFrame frame = _frames[_current];
			frame.Position = 0;
			return frame;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			ZFrame current = CurrentFrame;
			int toRead, length = 0;

			do
			{
				if (0 == (toRead = EnsureCapacity(current, count)))
				{
					if (null != (current = MoveNextFrame(0)))
					{
						continue;
					}
					break;
				}
				int haveRead = current.Read(buffer, offset, toRead);

				count -= haveRead;
				offset += haveRead;
				length += haveRead;

			} while (count > 0 && null != (current = MoveNextFrame(0)));

			return length;
		}

		public override int ReadByte()
		{
			ZFrame frame = CurrentFrame;
			if (0 == Math.Min(1, EnsureCapacity(frame, 1)))
			{
				if (null == (frame = MoveNextFrame(0)))
				{
					return -1;
				}
			}
			return frame.ReadByte();
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

		public virtual Single ReadSingle()
		{
			var bytes = new byte[4];
			int len = Read(bytes, 0, 4);
			if (len < 4)
			{
				return default(Single);
			}

			return BitConverter.ToSingle(bytes, 0);
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

		public virtual Double ReadDouble()
		{
			var bytes = new byte[8];
			int len = Read(bytes, 0, 8);
			if (len < 8)
			{
				return default(Double);
			}

			return BitConverter.ToDouble(bytes, 0);
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
			var bytes = new byte[byteCount];
			Read(bytes, 0, byteCount);
			return encoding.GetString(bytes);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			ZFrame current = CurrentFrame;
			int toWrite; //, length = 0;

			do
			{
				if (0 == (toWrite = EnsureCapacity(current, count)))
				{
					if (null != (current = MoveNextFrame(ZFrame.DefaultFrameSize)))
					{
						continue;
					}
					throw new InvalidOperationException();
				}

				current.Write(buffer, offset, toWrite);

				offset += toWrite;
				// length += toWrite;
				count -= toWrite;

			} while (count > 0 && null != (current = MoveNextFrame()));
			// return length;
		}

		public override void WriteByte(byte value)
		{
			ZFrame frame = CurrentFrame;

			while (0 == Math.Min(1, EnsureCapacity(frame, 1)))
			{
				if (null != (frame = MoveNextFrame(ZFrame.DefaultFrameSize)))
				{
					continue;
				}
				throw new InvalidOperationException();
			}
			frame.WriteByte(value);
		}

		public void WriteInt16(Int16 value)
		{
			if (Position + 2 > Length)
			{
				throw new InvalidOperationException();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			Write(bytes, 0, bytes.Length);
		}

		public void WriteUInt16(UInt16 value)
		{
			WriteInt16((Int16)value);
		}

		public void WriteChar(Char value)
		{
			WriteInt16((Int16)value);
		}

		public void WriteInt32(Int32 value)
		{
			if (Position + 4 > Length)
			{
				throw new InvalidOperationException();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			Write(bytes, 0, bytes.Length);
		}

		public void WriteUInt32(UInt32 value)
		{
			WriteInt32((Int32)value);
		}

		public void WriteSingle(Single value)
		{
			WriteInt32((Int32)value);
		}

		public void WriteInt64(Int64 value)
		{
			if (Position + 8 > Length)
			{
				throw new InvalidOperationException();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			Write(bytes, 0, bytes.Length);
		}

		public void WriteUInt64(UInt64 value)
		{
			WriteInt64((Int64)value);
		}

		public void WriteDouble(Double value)
		{
			WriteInt64((Int64)value);
		}

		public void WriteString(string str)
		{
			WriteString(str, ZContext.Encoding);
		}

		public void WriteString(string str, Encoding encoding)
		{
			var bytes = encoding.GetBytes(str);
			Write(bytes, 0, bytes.Length);
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override void Close()
		{
			if (_frames != null)
			{
				for (int i = 0, l = _frames.Count; i < l; i++)
				{
					_frames[i].Close();
				}
				// _frames.Clear();
				_frames = null;
			}
			_current = 0;
		}

		#region IList implementation

		public int IndexOf(ZFrame item)
		{
			return _frames.IndexOf(item);
		}

		public void Insert(int index, ZFrame item)
		{
			_frames.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_frames.RemoveAt(index);
		}

		public ZFrame this[int index]
		{
			get
			{
				return _frames[index];
			}
			set
			{
				_frames[index] = value;
			}
		}

		#endregion

		#region ICollection implementation

		public void Add(ZFrame item)
		{
			_frames.Add(item);
		}

		public void AddRange(IEnumerable<ZFrame> items)
		{
			_frames.AddRange(items);
		}

		public void Clear()
		{
			_frames.Clear();
		}

		public bool Contains(ZFrame item)
		{
			return _frames.Contains(item);
		}

		public void CopyTo(ZFrame[] array, int arrayIndex)
		{
			_frames.CopyTo(array, arrayIndex);
		}

		public bool Remove(ZFrame item)
		{
			return _frames.Remove(item);
		}

		public int Count
		{
			get
			{
				return _frames.Count;
			}
		}

		bool ICollection<ZFrame>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<ZFrame> GetEnumerator()
		{
			return _frames.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
	}
}
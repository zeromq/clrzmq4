using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeroMQ
{
	/// <summary>
	/// A single or multi-part message, sent or received via a <see cref="ZSocket"/>.
	/// </summary>
	public class ZMessage : IList<ZFrame>, ICloneable, IDisposable
	{
		private List<ZFrame> _frames;

		/// <summary>
		/// Initializes a new instance of the <see cref="ZMessage"/> class.
		/// Creates an empty message.
		/// </summary>
		public ZMessage()
		{ 
			_frames = new List<ZFrame>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZMessage"/> class.
		/// Creates a message that contains the given <see cref="ZFrame"/> objects.
		/// </summary>
		/// <param name="frames">A collection of <see cref="ZFrame"/> objects to be stored by this <see cref="ZMessage"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="frames"/> is null.</exception>
		public ZMessage(IEnumerable<ZFrame> frames)
		{
			if (frames == null)
			{
				throw new ArgumentNullException("frames");
			}

			_frames = new List<ZFrame>(frames);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_frames != null)
			{
				foreach (ZFrame frame in _frames)
				{
					frame.Dispose();
				}
			}
			_frames = null;
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

		public void ReplaceAt(int index, ZFrame replacement)
		{
			ReplaceAt(index, replacement, true);
		}

		public ZFrame ReplaceAt(int index, ZFrame replacement, bool dispose) 
		{
			ZFrame old = _frames[index];
			_frames[index] = replacement;
			if (dispose)
			{
				old.Dispose();
				return null;
			}
			return old;
		}

		#region IList implementation

		public int IndexOf(ZFrame item)
		{
			return _frames.IndexOf(item);
		}

		public void Prepend(ZFrame item) 
		{
			Insert(0, item);
		}

		public void Insert(int index, ZFrame item)
		{
			_frames.Insert(index, item);
		}

		/// <summary>
		/// Removes ZFrames. Note: Disposes the ZFrame.
		/// </summary>
		/// <returns>The <see cref="ZeroMQ.ZFrame"/>.</returns>
		public void RemoveAt(int index)
		{
			RemoveAt(index, true);
		}

		/// <summary>
		/// Removes ZFrames.
		/// </summary>
		/// <returns>The <see cref="ZeroMQ.ZFrame"/>.</returns>
		/// <param name="dispose">If set to <c>false</c>, do not dispose the ZFrame.</param>
		public ZFrame RemoveAt(int index, bool dispose)
		{
			ZFrame frame = _frames[index];
			_frames.RemoveAt(index);

			if (dispose)
			{
				frame.Dispose();
				return null;
			}
			return frame;
		}

		public ZFrame Pop()
		{
			return RemoveAt(0, false);
		}

		public int PopBytes(byte[] buffer, int offset, int count)
		{
			using (var frame = Pop())
			{
				return frame.Read(buffer, offset, count);
			}
		}

		public int PopByte()
		{
			using (var frame = Pop())
			{
				return frame.ReadByte();
			}
		}

		public byte PopAsByte()
		{
			using (var frame = Pop())
			{
				return frame.ReadAsByte();
			}
		}

		public Int16 PopInt16()
		{
			using (var frame = Pop())
			{
				return frame.ReadInt16();
			}
		}

		public UInt16 PopUInt16()
		{
			using (var frame = Pop())
			{
				return frame.ReadUInt16();
			}
		}

		public Char PopChar()
		{
			using (var frame = Pop())
			{
				return frame.ReadChar();
			}
		}

		public Int32 PopInt32()
		{
			using (var frame = Pop())
			{
				return frame.ReadInt32();
			}
		}

		public UInt32 PopUInt32()
		{
			using (var frame = Pop())
			{
				return frame.ReadUInt32();
			}
		}

		public Int64 PopInt64()
		{
			using (var frame = Pop())
			{
				return frame.ReadInt64();
			}
		}

		public UInt64 PopUInt64()
		{
			using (var frame = Pop())
			{
				return frame.ReadUInt64();
			}
		}

		public String PopString()
		{
			return PopString(ZContext.Encoding);
		}

		public String PopString(Encoding encoding)
		{
			using (var frame = Pop())
			{
				return frame.ReadString((int)frame.Length, encoding);
			}
		}

		public String PopString(int bytesCount, Encoding encoding)
		{
			using (var frame = Pop())
			{
				return frame.ReadString(bytesCount, encoding);
			}
		}

		public void Wrap(ZFrame frame) 
		{
			Insert(0, new ZFrame());
			Insert(0, frame);
		}

		public ZFrame Unwrap() 
		{
			ZFrame frame = RemoveAt(0, false);

			if (Count > 0 && this[0].Length == 0)
			{
				RemoveAt(0);
			}

			return frame;
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

		public void Append(ZFrame item)
		{
			Add(item);
		}

		public void AppendRange(IEnumerable<ZFrame> items)
		{
			AddRange(items);
		}

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
			foreach (ZFrame frame in _frames)
			{
				frame.Dispose();
			}
			_frames.Clear();
		}

		public bool Contains(ZFrame item)
		{
			return _frames.Contains(item);
		}

		void ICollection<ZFrame>.CopyTo(ZFrame[] array, int arrayIndex)
		{
			int i = 0, count = this.Count;
			foreach (ZFrame frame in this)
			{
				array[arrayIndex + i] = ZFrame.CopyFrom(frame);

				i++; if (i >= count) break;
			}
		}

		public bool Remove(ZFrame item)
		{
			if (null != Remove(item, true))
			{
				return false;
			}
			return true;
		}

		public ZFrame Remove(ZFrame item, bool dispose)
		{
			if (_frames.Remove(item))
			{
				if (dispose)
				{
					item.Dispose();
					return null;
				}
			}
			return item;
		}

		public int Count { get { return _frames.Count; } }

		bool ICollection<ZFrame>.IsReadOnly { get { return false; } }

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

		#region ICloneable implementation

		public object Clone()
		{
			return Duplicate();
		}

		public ZMessage Duplicate() 
		{
			var message = new ZMessage();
			foreach (ZFrame frame in this)
			{
				message.Add(frame.Duplicate());
			}
			return message;
		}

		#endregion
	}
}
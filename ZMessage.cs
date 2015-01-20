using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZeroMQ
{
	/// <summary>
	/// A single or multi-part message.
	/// </summary>
	public class ZMessage : IList<ZFrame>, IDisposable
	{
		private List<ZFrame> _frames;

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
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
	}
}
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
			: this (Enumerable.Empty<ZFrame>())
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
			if (_frames != null) {
				foreach (ZFrame frame in _frames) {
					frame.Dispose();
				}
			}
			_frames = null;
			base.Dispose(disposing);
		}

		public void Dismiss()
		{
			if (_frames != null) {
				foreach (ZFrame frame in _frames) {
					frame.Dismiss();
				}
			}
			_frames = null;
		}

		public ZFrame CurrentFrame {
			get {
				if (_current > -1) {
					return _frames [_current];
				}
				return null;
			}
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return false;
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

		public override void SetLength(long value)
		{
			throw new NotSupportedException ();
		}

		public override long Length {
			get {
				long size = 0;
				for (int i = 0, l = _frames.Count; i < l; ++i) {
					size += _frames [i].Length;
				}
				return size;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{	
			throw new NotImplementedException();
		}

		public override long Position {
			get {
				long pos = 0;
				for (int i = 0; i < _current; ++i) {
					pos += _frames [i].Length;
				}
				if (_current > -1 && _current < _frames.Count) {
					pos += _frames [_current].Position;
				}
				return pos;
			}
			set {
				throw new NotImplementedException();
			}
		}

		private int EnsureCapacity(ZFrame frame, int required) {

			// Check if the current or has some bytes to write to
			if (frame != null) {
				return Math.Min(required, Math.Max(0, (int)(frame.Length - frame.Position)));
			}
			return 0;
		}

		public ZFrame AppendFrame(int size) {

			// int size = Math.Max(requiredBytes, ZmqFrame.DefaultFrameSize);

			// new ZmqFrame, DefaultAllocSize
			var frame = ZFrame.Create(size);

			_frames.Add(frame);
			_current = _frames.Count - 1;

			return frame; // Math.Min(requiredBytes, size); // remaining bytes or how much are required
		}

		private ZFrame MoveNextFrame(int size) {
			if (_current + 1 >= _frames.Count) {
				if (size > 0) {
					return AppendFrame(size);
				}
				return null;
			}

			++_current;
			ZFrame frame = _frames [_current];
			frame.Position = 0;
			return frame;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			ZFrame current = CurrentFrame;
			int toRead, length = 0;

			do {
				if (0 == (toRead = EnsureCapacity(current, count))) {
					if (null != (current = MoveNextFrame(0))) {
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
			if (0 == Math.Min(1, EnsureCapacity(frame, 1))) {
				if (null == (frame = MoveNextFrame(0))) {
					return -1;
				}
			}
			return frame.ReadByte();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			ZFrame current = CurrentFrame;
			int toWrite; //, length = 0;

			do {
				if (0 == (toWrite = EnsureCapacity(current, count))) {
					if (null != (current = MoveNextFrame(ZFrame.DefaultFrameSize))) {
						continue;
					}
					throw new InvalidOperationException ();
				}

				current.Write(buffer, offset, toWrite);

				offset += toWrite;
				// length += toWrite;
				count -= toWrite;

			} while (count > 0 && null != (current = MoveNextFrame(ZFrame.DefaultFrameSize)));
			// return length;
		}

		public override void WriteByte(byte value)
		{
			ZFrame frame = CurrentFrame;

			while (0 == Math.Min(1, EnsureCapacity(frame, 1))) {
				if (null != (frame = MoveNextFrame(ZFrame.DefaultFrameSize))) {
					continue;
				}
				throw new InvalidOperationException ();
			}
			frame.WriteByte(value);
		}

		public override void Flush()
		{
			throw new NotSupportedException ();
		}

		public override void Close()
		{
			if (_frames != null) {
				for (int i = 0, l = _frames.Count; i < l; i++) {
					_frames [i].Close();
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

		public ZFrame this[int index] {
			get {
				return _frames [index];
			}
			set {
				_frames [index] = value;
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

		public int Count {
			get {
				return _frames.Count;
			}
		}

		bool ICollection<ZFrame>.IsReadOnly {
			get {
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

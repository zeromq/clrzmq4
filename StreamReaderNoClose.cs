using System.IO;
using System.Text;

namespace ZeroMQ
{
	internal class StreamReaderNoClose : StreamReader
	{
		public StreamReaderNoClose(Stream stream) : base(stream)
		{
		}

		public StreamReaderNoClose(Stream stream, bool detectEncodingFromByteOrderMarks) : base(stream, detectEncodingFromByteOrderMarks)
		{
		}

		public StreamReaderNoClose(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public StreamReaderNoClose(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(stream, encoding, detectEncodingFromByteOrderMarks)
		{
		}

		public StreamReaderNoClose(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
		{
		}

		public StreamReaderNoClose(string path) : base(path)
		{
		}

		public StreamReaderNoClose(string path, bool detectEncodingFromByteOrderMarks) : base(path, detectEncodingFromByteOrderMarks)
		{
		}

		public StreamReaderNoClose(string path, Encoding encoding) : base(path, encoding)
		{
		}

		public StreamReaderNoClose(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(path, encoding, detectEncodingFromByteOrderMarks)
		{
		}

		public StreamReaderNoClose(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize)
		{
		}

		protected override void Dispose(bool disposing)
		{
			// do nothing so the underlying stream will not be closed
		}

		/// <summary>
		/// Description of StreamReader.ReadLine from MSDN:
		/// A line is defined as a sequence of characters followed by a line feed ("\n"),
		/// a carriage return ("\r"), or a carriage return immediately followed by a line
		/// feed ("\r\n"). The string that is returned does not contain the terminating
		/// carriage return or line feed. The returned value is null if the end of the
		/// input stream is reached.
		/// 
		/// here we use Peek to check the EOF and return the line CONTAINING the terminating
		/// \r, \n or \r\n to move the ZFrame position forward correctly
		/// </summary>
		public StringBuilder ReadLineWithTerminator()
		{
			if (Peek() == -1) return null;
			StringBuilder sb = new StringBuilder();
			while (true)
			{
				int c = Read();
				if (c == -1) break;

				sb.Append((char)c);
				if (c == '\n')
					break;
				else if (c == '\r')
				{
					int c2 = Peek();
					if (c2 == '\n')
						sb.Append((char)Read());
					break;
				}
			}
			return sb;
		}
	}
}
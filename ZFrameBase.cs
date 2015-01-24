using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ZeroMQ
{
	public abstract class ZFrameBase : Stream
	{
		internal ZFrameBase() { }

		public abstract IntPtr DataPtr();

		public override int ReadByte()
		{
			if (Position + 1 > Length)
				return default(int);

			int byt = Marshal.ReadByte(DataPtr() + (int)Position);
			++Position;
			return byt;
		}
	}
}
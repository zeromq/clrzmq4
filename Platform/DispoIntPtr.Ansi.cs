using System.Threading;

namespace ZeroMQ.lib
{
	using System;
	using System.Text;
	using System.Threading;
	using System.Runtime.InteropServices;

	internal sealed partial class DispoIntPtr : IDisposable
	{
		public static class Ansi
		{
			unsafe internal static DispoIntPtr AllocStringNative(string str, out int byteCount)
			{
				// use builtin allocation
				var dispPtr = new DispoIntPtr();
				dispPtr._ptr = Marshal.StringToHGlobalAnsi(str);
				dispPtr.isAllocated = true;

				byteCount = Encoding.Default.GetByteCount(str);
				return dispPtr;  /**/

				/* use encoding or Encoding.Default ( system codepage of ANSI )
				var enc = Encoding.Default.GetEncoder();

				// var encoded = new byte[length];
				// Marshal.Copy(encoded, 0, dispPtr._ptr, length);

				IntPtr ptr;
				int charCount = str.Length;

				fixed (char* strP = str) 
				{
					byteCount = enc.GetByteCount(strP, charCount, false);

					ptr = Marshal.AllocHGlobal(byteCount + 1);

					enc.GetBytes(strP, charCount, (byte*)ptr, byteCount, true);

					*((byte*)ptr + byteCount) = 0x00;
				}

				var dispPtr = new DispoIntPtr ();
				dispPtr._ptr = ptr; 
				dispPtr.isAllocated = true;

				// and a C char 0x00 terminator
				// Marshal.WriteByte(dispPtr._ptr + length, byte.MinValue);
				// *((byte*)dispPtr._ptr + length) = 0x00;

				return dispPtr; /**/
			}
		}
	}
}
using System;
using System.Runtime.InteropServices;
using System.Threading;
using ZeroMQ.lib;

namespace ZeroMQ
{
	public static class Z85
	{
		/* public static byte[] ToZ85Bytes(this string encoded) 
		{
			return Decode(encoded);
		} */

		public static string Decode(string encoded)
		{
			using (var data = DispoIntPtr.Alloc(encoded.Length + 1))
			using (var dest = DispoIntPtr.Alloc((Int32)(encoded.Length * .8) + 1))
			{
				byte[] txt = ZContext.Encoding.GetBytes(encoded);
				Marshal.Copy(txt, 0, data, txt.Length);

				if (IntPtr.Zero == zmq.z85_decode(dest, data))
				{
					throw new InvalidOperationException();
				}

				var decoded = new byte[(Int32)(encoded.Length * .8) + 1];
				Marshal.Copy(dest, decoded, 0, decoded.Length);
				return ZContext.Encoding.GetString(decoded);
			}
		}

		/* public static string ToZ85String(this byte[] decoded) 
		{
			return Encode(decoded);
		} */

		public static string Encode(string decoded)
		{
			using (var data = DispoIntPtr.Alloc(decoded.Length + 1))
			using (var dest = DispoIntPtr.Alloc((Int32)(decoded.Length * 1.25) + 1))
			{
				byte[] bytes = ZContext.Encoding.GetBytes(decoded);

				Marshal.Copy(bytes, 0, data, bytes.Length);

				if (IntPtr.Zero == zmq.z85_encode(dest, data, bytes.Length))
				{
					throw new InvalidOperationException();
				}
				return Marshal.PtrToStringAnsi(dest);
			}
		}
	}
}


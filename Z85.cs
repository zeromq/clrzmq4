using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ZeroMQ.lib;

namespace ZeroMQ
{
	public static class Z85
	{
		public static byte[] Encode(byte[] decoded)
		{
			int dataLen = decoded.Length;
			if (dataLen % 4 > 0)
			{
				throw new InvalidOperationException("decoded.Length must be divisible by 4");
			}
			int destLen = (Int32)(decoded.Length * 1.25) + 1;

			var data = GCHandle.Alloc(decoded, GCHandleType.Pinned);

			using (var dest = DispoIntPtr.Alloc(destLen))
			{
				if (IntPtr.Zero == zmq.z85_encode(dest, data.AddrOfPinnedObject(), dataLen))
				{
					data.Free();
					throw new InvalidOperationException();
				}
				data.Free();

				var bytes = new byte[destLen];

				Marshal.Copy(dest, bytes, 0, destLen);

				return bytes;
			}
		}

		public static byte[] ToZ85Encoded(this byte[] decoded) 
		{
			return Encode(decoded);
		}

		public static string ToZ85Encoded(this string decoded) 
		{
			return Encode(decoded, ZContext.Encoding);
		}

		public static string ToZ85Encoded(this string decoded, Encoding encoding) 
		{
			return Encode(decoded, encoding);
		}

		public static byte[] ToZ85EncodedBytes(this string decoded) 
		{
			return EncodeBytes(decoded, ZContext.Encoding);
		}

		public static byte[] ToZ85EncodedBytes(this string decoded, Encoding encoding) 
		{
			return EncodeBytes(decoded, encoding);
		}

		public static string Encode(string strg)
		{
			return Encode(strg, ZContext.Encoding);
		}

		public static string Encode(string strg, Encoding encoding)
		{
			byte[] encoded = EncodeBytes(strg, encoding);
			return encoding.GetString(encoded);
		}

		public static byte[] EncodeBytes(string strg)
		{
			return EncodeBytes(strg, ZContext.Encoding);
		}

		public static byte[] EncodeBytes(string strg, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(strg);
			return Encode(bytes);
		}


		public static byte[] Decode(byte[] encoded)
		{
			int dataLen = encoded.Length;
			if (dataLen % 5 > 0)
			{
				throw new InvalidOperationException("encoded.Length must be divisible by 5");
			}
			int destLen = (Int32)(encoded.Length * .8);

			var data = GCHandle.Alloc(encoded, GCHandleType.Pinned);

			using (var dest = DispoIntPtr.Alloc(destLen))
			{
				if (IntPtr.Zero == zmq.z85_decode(dest, data.AddrOfPinnedObject()))
				{
					data.Free();
					throw new InvalidOperationException();
				}
				data.Free();

				var decoded = new byte[destLen];

				Marshal.Copy(dest, decoded, 0, decoded.Length);

				return decoded;
			}
		}

		public static byte[] ToZ85Decoded(this byte[] encoded) 
		{
			return Decode(encoded);
		}

		public static string ToZ85Decoded(this string encoded) 
		{
			return Decode(encoded, ZContext.Encoding);
		}

		public static string ToZ85Decoded(this string encoded, Encoding encoding) 
		{
			return Decode(encoded, encoding);
		}

		public static byte[] ToZ85DecodedBytes(this string encoded) 
		{
			return DecodeBytes(encoded, ZContext.Encoding);
		}

		public static byte[] ToZ85DecodedBytes(this string encoded, Encoding encoding) 
		{
			return DecodeBytes(encoded, encoding);
		}

		public static string Decode(string strg)
		{
			return Decode(strg, ZContext.Encoding);
		}

		public static string Decode(string strg, Encoding encoding)
		{
			byte[] encoded = DecodeBytes(strg, encoding);
			return encoding.GetString(encoded);
		}
		
		public static byte[] DecodeBytes(string strg, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(strg);
			return Decode(bytes);
		}
	}
}


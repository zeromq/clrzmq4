namespace ZeroMQ.lib.sys
{
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;

	public enum sockaddr_family : ushort
	{
		inet = 2,
		inet6 = 23
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct sockaddr
	{
		public UInt16 family;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
		public Byte[] data;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct sockaddr_in 
	{
		public UInt16 family;
		public UInt16 port;

		[StructLayout(LayoutKind.Explicit)]
		public struct in_addr
		{
			[FieldOffsetAttribute(0)]
			public uint s_addr;

			[StructLayout(LayoutKind.Sequential)]
			public struct _s_un_b
			{
				public byte s_b1, s_b2, s_b3, s_b4;
			}
			[FieldOffsetAttribute(0)]
			public _s_un_b s_un_b;

			[StructLayout(LayoutKind.Sequential)]
			public struct _s_un_w
			{
				public ushort s_w1, s_w2;
			}
			[FieldOffsetAttribute(0)]
			public _s_un_w s_un_w;
		}
		public in_addr addr;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public Byte[] zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct sockaddr_in6 
	{
		public UInt16 family;
		public UInt16 port;
		public UInt32 flow_info;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] 
		public Byte[] addr;
		public UInt32 scope_id;
	};

	public static class socket
	{
		// private const string LibraryName = "libc";

		// private static readonly UnmanagedLibrary NativeLib;

		static socket()
		{
			// NativeLib = Platform.LoadUnmanagedLibrary(LibraryName);

			// Initialize static Fields
			Platform.SetupPlatformImplementation(typeof(socket));
		}

		// unsafe native delegate
		delegate IPAddress GetPeerNameCDelegate(Socket socket);
		static readonly GetPeerNameCDelegate GetPeerNameC;

		// unsafe native delegate
		delegate IPAddress GetPeerNameZMQDelegate(ZFrame frame);
		static readonly GetPeerNameZMQDelegate GetPeerNameZMQ;

		public static IPAddress GetPeerName(this Socket socket)
		{
			return GetPeerNameC(socket);
		}

		public static IPAddress GetPeerName(this ZFrame message)
		{
			return GetPeerNameZMQ(message);
		}

		static class Posix 
		{
			const string LibraryName = "libc";

			[DllImport(Posix.LibraryName, EntryPoint = "getpeername")]
			static extern Int32 getpeername(Int32 sock, ref sockaddr addr, ref Int32 addrLen);

			static IPAddress GetPeerNameNative(Int32 sock)
			{
				Int32 addrLen = Marshal.SizeOf(typeof(sockaddr_in6));

				using (var addrPtr = DispoIntPtr.Alloc(addrLen))
				{
					var sockaddr = (sockaddr)Marshal.PtrToStructure(addrPtr, typeof(sockaddr));

					if (0 == getpeername(sock, ref sockaddr, ref addrLen))
					{
						if (sockaddr.family == (UInt16)sockaddr_family.inet)
						{
							var sockaddr_in = (sockaddr_in)Marshal.PtrToStructure(addrPtr, typeof(sockaddr_in));

							// int port = (((int)sockaddr[2])<<8) + (int)sockaddr[3];

							long address
								= (((long)sockaddr_in.addr.s_un_b.s_b3) << 24)
								+ (((long)sockaddr_in.addr.s_un_b.s_b4) << 16)
								+ (((long)sockaddr_in.addr.s_un_b.s_b1) << 8)
								+ (long)sockaddr_in.addr.s_un_b.s_b2;

							// ipe = new IPEndPoint(address, port);

							return new IPAddress( address );
						}
						else if (sockaddr.family == (UInt16)sockaddr_family.inet6)
						{
							var sockaddr_in6 = (sockaddr_in6)Marshal.PtrToStructure(addrPtr, typeof(sockaddr_in6));
							//
							return new IPAddress(sockaddr_in6.addr, sockaddr_in6.scope_id);
						}
						else
						{
							throw new NotSupportedException();
						}
					}
				}
				throw new ZException(ZError.GetLastErr());
			}

			static IPAddress GetPeerNameC(Socket socket)
			{
				return GetPeerNameNative(socket.Handle.ToInt32());
			}

			static IPAddress GetPeerNameZMQ(ZFrame message)
			{
				Int32 fd = message.GetOption(ZFrameOption.SRCFD);
				return GetPeerNameNative(fd);
			}
		}

		static class Win32
		{ 
			const string LibraryName = "ws2_32";

			[DllImport(Win32.LibraryName, EntryPoint = "getpeername")]
			static extern Int32 getpeername(IntPtr sock, ref sockaddr addr, ref Int32 addrLen);

			static IPAddress GetPeerNameNative(IntPtr sock)
			{
				Int32 addrLen = Marshal.SizeOf(typeof(sockaddr_in6));

				using (var addrPtr = DispoIntPtr.Alloc(addrLen))
				{
					var sockaddr = (sockaddr)Marshal.PtrToStructure(addrPtr, typeof(sockaddr));

					if (0 == getpeername(sock, ref sockaddr, ref addrLen))
					{
						if (sockaddr.family == (UInt16)sockaddr_family.inet)
						{
							var sockaddr_in = (sockaddr_in)Marshal.PtrToStructure(addrPtr, typeof(sockaddr_in));

							// int port = (((int)sockaddr[2])<<8) + (int)sockaddr[3];

							long address
							= (((long)sockaddr_in.addr.s_un_b.s_b3) << 24)
								+ (((long)sockaddr_in.addr.s_un_b.s_b4) << 16)
								+ (((long)sockaddr_in.addr.s_un_b.s_b1) << 8)
								+ (long)sockaddr_in.addr.s_un_b.s_b2;

							// ipe = new IPEndPoint(address, port);

							return new IPAddress( address );
						}
						else if (sockaddr.family == (UInt16)sockaddr_family.inet6)
						{
							var sockaddr_in6 = (sockaddr_in6)Marshal.PtrToStructure(addrPtr, typeof(sockaddr_in6));
							//
							return new IPAddress(sockaddr_in6.addr, sockaddr_in6.scope_id);
						}
						else
						{
							throw new NotSupportedException();
						}
					}
				}
				throw new ZException(ZError.GetLastErr());
			}

			static IPAddress GetPeerNameC(Socket socket)
			{
				return GetPeerNameNative(socket.Handle);
			}

			static IPAddress GetPeerNameZMQ(ZFrame message)
			{
				var fd = new IntPtr(message.GetOption(ZFrameOption.SRCFD));
				return GetPeerNameNative(fd);
			}
		}
	}
}
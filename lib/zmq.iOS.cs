namespace ZeroMQ.lib
{
	using System;
	using System.Runtime.InteropServices;
	
	public static unsafe partial class zmq
	{
		public static unsafe class iOS
		{
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_version", CallingConvention = CallingConvention.Cdecl)]
			private static extern void version(out int major, out int minor, out int patch);
							
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_ctx_new", CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr ctx_new();
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_ctx_get", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 ctx_get(IntPtr context, Int32 option);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_ctx_set", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 ctx_set(IntPtr context, Int32 option, Int32 optval);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_ctx_shutdown", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 ctx_shutdown(IntPtr context);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_ctx_term", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 ctx_term(IntPtr context);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_init", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_init(IntPtr msg);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_init_size", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_init_size(IntPtr msg, Int32 size);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_init_data", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_init_data(IntPtr msg, IntPtr data, Int32 size, ZeroMQ.lib.zmq.FreeMessageDataDelegate ffn, IntPtr hint);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_send", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_send(IntPtr msg, IntPtr socket, Int32 flags);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_recv", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_recv(IntPtr msg, IntPtr socket, Int32 flags);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_close", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_close(IntPtr msg);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_data", CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr msg_data(IntPtr msg);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_size", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_size(IntPtr msg);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_more", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_more(IntPtr msg);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_gets", CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr msg_gets(IntPtr msg, IntPtr property);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_get", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_get(IntPtr msg, Int32 property);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_set", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_set(IntPtr msg, Int32 property, Int32 value);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_copy", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_copy(IntPtr dest, IntPtr src);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_msg_move", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 msg_move(IntPtr dest, IntPtr src);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_socket", CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr socket(IntPtr context, Int32 type);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_close", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 close(IntPtr socket);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_getsockopt", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 getsockopt(IntPtr socket, Int32 option_name, IntPtr option_value, IntPtr option_len);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_setsockopt", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 setsockopt(IntPtr socket, Int32 option_name, IntPtr option_value, Int32 option_len);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_bind", /*CharSet = CharSet.Ansi,*/ CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 bind(IntPtr socket, IntPtr endpoint);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_unbind", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 unbind(IntPtr socket, IntPtr endpoint);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_connect", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 connect(IntPtr socket, IntPtr endpoint);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_disconnect", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 disconnect(IntPtr socket, IntPtr endpoint);
							
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_poll", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 poll(void* items, Int32 numItems, long timeout);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_send", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 send(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_recv", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 recv(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_has", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 has(IntPtr capability);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_socket_monitor", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 socket_monitor(IntPtr socket, IntPtr endpoint, Int32 events);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_proxy", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 proxy(IntPtr frontend, IntPtr backend, IntPtr capture);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_proxy_steerable", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 proxy_steerable(IntPtr frontend, IntPtr backend, IntPtr capture, IntPtr control);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_curve_keypair", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 curve_keypair(IntPtr z85_public_key, IntPtr z85_secret_key);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_z85_encode", CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr z85_encode(IntPtr dest, IntPtr data, Int32 size);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_z85_decode", CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr z85_decode(IntPtr dest, IntPtr data);
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_errno", CallingConvention = CallingConvention.Cdecl)]
			private static extern Int32 errno();
			
			[DllImport(Platform.iOS.LibraryName, EntryPoint = "zmq_strerror", CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr strerror(int errnum);
		}
	}
}
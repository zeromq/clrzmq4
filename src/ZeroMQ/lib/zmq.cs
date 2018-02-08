namespace ZeroMQ.lib
{
	using System;
	using System.Runtime.InteropServices;

	public static unsafe class zmq
	{
		private const CallingConvention CCCdecl = CallingConvention.Cdecl;

		// Use a const for the library name
		private const string LibraryName = "libzmq";

		// From zmq.h (v3):
		// typedef struct {unsigned char _ [32];} zmq_msg_t;
		private static readonly int sizeof_zmq_msg_t_v3 = 32 * Marshal.SizeOf(typeof(byte));

		// From zmq.h (not v4, but v4.2 and later):
		// typedef struct zmq_msg_t {unsigned char _ [64];} zmq_msg_t;
		private static readonly int sizeof_zmq_msg_t_v4 = 64 * Marshal.SizeOf(typeof(byte));

		public static readonly int sizeof_zmq_msg_t = sizeof_zmq_msg_t_v4;

		// The static constructor prepares static readonly fields
		static zmq()
		{
			// Set once LibVersion to libversion()
			int major, minor, patch;
			version(out major, out minor, out patch);
			LibraryVersion = new Version(major, minor, patch);

			// Trigger static constructor
            // TODO this is also done in the static initializer of ZError. Can this be unified?
			var noSym = ZSymbol.None;

			if (major >= 4)
			{
				// Current Version 4

				// Use default delegate settings from field initializers.
				// "Compatibility" is done by "disabling" old methods, or "redirecting" to new methods,
				// so the developer is forced to work against the latest API

				if (minor == 0)
				{
					sizeof_zmq_msg_t = sizeof_zmq_msg_t_v3;
				}
			}
			else 
			{ 
				throw VersionNotSupported(null, ">= v4");
			}
		}

		private static NotSupportedException VersionNotSupported(string methodName, string requiredVersion)
		{
			return new NotSupportedException(
				string.Format(
					"{0}libzmq version not supported. Required version {1}",
					methodName == null ? string.Empty : methodName + ": ",
					requiredVersion));
		}

		// (2) Declare privately the extern entry point
		[DllImport(LibraryName, EntryPoint = "zmq_version", CallingConvention = CCCdecl)]
		public static extern void version(out int major, out int minor, out int patch);

		public static readonly Version LibraryVersion;

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_new", CallingConvention = CCCdecl)]
		public static extern IntPtr ctx_new();

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_get", CallingConvention = CCCdecl)]
		public static extern Int32 ctx_get(IntPtr context, Int32 option);

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_set", CallingConvention = CCCdecl)]
		public static extern Int32 ctx_set(IntPtr context, Int32 option, Int32 optval);

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_shutdown", CallingConvention = CCCdecl)]
		public static extern Int32 ctx_shutdown(IntPtr context);

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_term", CallingConvention = CCCdecl)]
		public static extern Int32 ctx_term(IntPtr context);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_init", CallingConvention = CCCdecl)]
		public static extern Int32 msg_init(IntPtr msg);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_init_size", CallingConvention = CCCdecl)]
		public static extern Int32 msg_init_size(IntPtr msg, Int32 size);

		[UnmanagedFunctionPointer(CCCdecl)]
		public delegate void FreeMessageDataDelegate(IntPtr data, IntPtr hint);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_init_data", CallingConvention = CCCdecl)]
		public static extern Int32 msg_init_data(IntPtr msg, IntPtr data, Int32 size, FreeMessageDataDelegate ffn, IntPtr hint);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_send", CallingConvention = CCCdecl)]
		public static extern Int32 msg_send(IntPtr msg, IntPtr socket, Int32 flags);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_recv", CallingConvention = CCCdecl)]
		public static extern Int32 msg_recv(IntPtr msg, IntPtr socket, Int32 flags);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_close", CallingConvention = CCCdecl)]
		public static extern Int32 msg_close(IntPtr msg);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_data", CallingConvention = CCCdecl)]
		public static extern IntPtr msg_data(IntPtr msg);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_size", CallingConvention = CCCdecl)]
		public static extern Int32 msg_size(IntPtr msg);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_more", CallingConvention = CCCdecl)]
		public static extern Int32 msg_more(IntPtr msg);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_gets", CallingConvention = CCCdecl)]
		public static extern IntPtr msg_gets(IntPtr msg, IntPtr property);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_get", CallingConvention = CCCdecl)]
		public static extern Int32 msg_get(IntPtr msg, Int32 property);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_set", CallingConvention = CCCdecl)]
		public static extern Int32 msg_set(IntPtr msg, Int32 property, Int32 value);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_copy", CallingConvention = CCCdecl)]
		public static extern Int32 msg_copy(IntPtr dest, IntPtr src);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_move", CallingConvention = CCCdecl)]
		public static extern Int32 msg_move(IntPtr dest, IntPtr src);

		[DllImport(LibraryName, EntryPoint = "zmq_socket", CallingConvention = CCCdecl)]
		public static extern IntPtr socket(IntPtr context, Int32 type);

		[DllImport(LibraryName, EntryPoint = "zmq_close", CallingConvention = CCCdecl)]
		public static extern Int32 close(IntPtr socket);

		[DllImport(LibraryName, EntryPoint = "zmq_getsockopt", CallingConvention = CCCdecl)]
		public static extern Int32 getsockopt(IntPtr socket, Int32 option_name, IntPtr option_value, IntPtr option_len);

		[DllImport(LibraryName, EntryPoint = "zmq_setsockopt", CallingConvention = CCCdecl)]
		public static extern Int32 setsockopt(IntPtr socket, Int32 option_name, IntPtr option_value, Int32 option_len);

		[DllImport(LibraryName, EntryPoint = "zmq_bind", CallingConvention = CCCdecl)]
		public static extern Int32 bind(IntPtr socket, IntPtr endpoint);

		[DllImport(LibraryName, EntryPoint = "zmq_unbind", CallingConvention = CCCdecl)]
		public static extern Int32 unbind(IntPtr socket, IntPtr endpoint);

		[DllImport(LibraryName, EntryPoint = "zmq_connect", CallingConvention = CCCdecl)]
		public static extern Int32 connect(IntPtr socket, IntPtr endpoint);

		[DllImport(LibraryName, EntryPoint = "zmq_disconnect", CallingConvention = CCCdecl)]
		public static extern Int32 disconnect(IntPtr socket, IntPtr endpoint);

		// Using void* to be liberal for zmq_pollitem_windows_t and _posix_t
		[DllImport(LibraryName, EntryPoint = "zmq_poll", CallingConvention = CCCdecl)]
		public static extern Int32 poll(void* items, Int32 numItems, long timeout);


		[DllImport(LibraryName, EntryPoint = "zmq_send", CallingConvention = CCCdecl)]
		public static extern Int32 send(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);

		// [DllImport(LibraryName, CallingConvention = CCCdecl)]
		// private static extern Int32 zmq_send_const(IntPtr socket, IntPtr buf, Int32 size, Int32 flags);

		[DllImport(LibraryName, EntryPoint = "zmq_recv", CallingConvention = CCCdecl)]
		public static extern Int32 recv(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);


		[DllImport(LibraryName, EntryPoint = "zmq_has", CallingConvention = CCCdecl)]
		public static extern Int32 has(IntPtr capability);

		[DllImport(LibraryName, EntryPoint = "zmq_socket_monitor", CallingConvention = CCCdecl)]
		public static extern Int32 socket_monitor(IntPtr socket, IntPtr endpoint, Int32 events);


		[DllImport(LibraryName, EntryPoint = "zmq_proxy", CallingConvention = CCCdecl)]
		public static extern Int32 proxy(IntPtr frontend, IntPtr backend, IntPtr capture);

		[DllImport(LibraryName, EntryPoint = "zmq_proxy_steerable", CallingConvention = CCCdecl)]
		public static extern Int32 proxy_steerable(IntPtr frontend, IntPtr backend, IntPtr capture, IntPtr control);


		[DllImport(LibraryName, EntryPoint = "zmq_curve_keypair", CallingConvention = CCCdecl)]
		public static extern Int32 curve_keypair(IntPtr z85_public_key, IntPtr z85_secret_key);

		[DllImport(LibraryName, EntryPoint = "zmq_z85_encode", CallingConvention = CCCdecl)]
		public static extern IntPtr z85_encode(IntPtr dest, IntPtr data, Int32 size);

		[DllImport(LibraryName, EntryPoint = "zmq_z85_decode", CallingConvention = CCCdecl)]
		public static extern IntPtr z85_decode(IntPtr dest, IntPtr data);


		[DllImport(LibraryName, EntryPoint = "zmq_errno", CallingConvention = CCCdecl)]
		public static extern Int32 errno();

		[DllImport(LibraryName, EntryPoint = "zmq_strerror", CallingConvention = CCCdecl)]
		public static extern IntPtr strerror(int errnum);

	}
}
namespace ZeroMQ.lib
{
	using System;
	using System.Runtime.InteropServices;

	public static unsafe class zmq
	{
		private const string __Internal = "__Internal";

		private const CallingConvention CCCdecl = CallingConvention.Cdecl;

		// Use a const for the library name
		private const string LibraryName = "libzmq";

		// Hold a handle to the static instance
		private static readonly UnmanagedLibrary NativeLib;

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

#if !NETSTANDARD && !NETCORE
			// (0) Initialize Library handle
			NativeLib = Platform.LoadUnmanagedLibrary(LibraryName);

			// (1) Initialize Platform information 
			Platform.SetupImplementation(typeof(zmq));
#endif

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
			else if (major >= 3)
			{
				// Backwards compatibility for v3

				sizeof_zmq_msg_t = sizeof_zmq_msg_t_v3;

				zmq.ctx_shutdown = (ctxPtr) 
					=> { throw VersionNotSupported("zmq_ctx_shutdown", "v4"); };
				zmq.msg_gets = (msgPtr, propertyPtr) 
					=> { throw VersionNotSupported("zmq_msg_gets", "v4"); };
				zmq.has = (capabilityPtr) 
					=> { throw VersionNotSupported("zmq_has", "v4"); };
				zmq.proxy_steerable = (frontendPtr, backendPtr, capturePtr, controlPtr)
					=> { throw VersionNotSupported("zmq_proxy_steerable", "v4"); };
				zmq.curve_keypair = (z85_public_key, z85_secret_key) 
					=> { throw VersionNotSupported("zmq_curve_keypair", "v4"); };
				zmq.z85_encode = (dest, data, size) 
					=> { throw VersionNotSupported("zmq_z85_encode", "v4"); };
				zmq.z85_decode = (dest, data) 
					=> { throw VersionNotSupported("zmq_z85_decode", "v4"); };

				if (!Platform.Is__Internal) {
					zmq.ctx_term = zmq.zmq_term;
				}
				else {
					zmq.ctx_term = zmq.zmq_term__Internal;
				}
			}
			else 
			{ 
				throw VersionNotSupported(null, ">= v3");
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
		private static extern void zmq_version(out int major, out int minor, out int patch);
		[DllImport(__Internal, EntryPoint = "zmq_version", CallingConvention = CCCdecl)]
		private static extern void zmq_version__Internal(out int major, out int minor, out int patch);

		// (3) Describe the extern function using a delegate
		public delegate void zmq_version_delegate(out int major, out int minor, out int patch);

		// (4) Save and return the managed delegate to the unmanaged function
		//     This static readonly field definition allows to be 
		//     initialized and possibly redirected by the static constructor.
		//
		//     By default this is set to the extern function declaration,
		//     it may be set to the __Internal extern function declaration.
		public static readonly zmq_version_delegate version = zmq_version;

		public static readonly Version LibraryVersion;

		/* Deprecated. Use zmq_ctx_new instead.
		[DllImport(LibraryName, EntryPoint = "zmq_init", CallingConvention = CCCdecl)]
		private static extern IntPtr init(int io_threads); /**/

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_new", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_ctx_new();
		[DllImport(__Internal, EntryPoint = "zmq_ctx_new", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_ctx_new__Internal();
		public delegate IntPtr zmq_ctx_new_delegate();
		public static readonly zmq_ctx_new_delegate ctx_new = zmq_ctx_new;

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_get", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_ctx_get(IntPtr context, Int32 option);
		[DllImport(__Internal, EntryPoint = "zmq_ctx_get", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_ctx_get__Internal(IntPtr context, Int32 option);
		public delegate Int32 zmq_ctx_get_delegate(IntPtr context, Int32 option);
		public static readonly zmq_ctx_get_delegate ctx_get = zmq_ctx_get;

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_set", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_ctx_set(IntPtr context, Int32 option, Int32 optval);
		[DllImport(__Internal, EntryPoint = "zmq_ctx_set", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_ctx_set__Internal(IntPtr context, Int32 option, Int32 optval);
		public delegate Int32 zmq_ctx_set_delegate(IntPtr context, Int32 option, Int32 optval);
		public static readonly zmq_ctx_set_delegate ctx_set = zmq_ctx_set;

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_shutdown", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_ctx_shutdown(IntPtr context);
		[DllImport(__Internal, EntryPoint = "zmq_ctx_shutdown", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_ctx_shutdown__Internal(IntPtr context);
		public delegate Int32 zmq_ctx_shutdown_delegate(IntPtr context);
		public static readonly zmq_ctx_shutdown_delegate ctx_shutdown = zmq_ctx_shutdown;

		/* Deprecated. Use zmq_ctx_term instead.
		[DllImport(LibraryName, CallingConvention = CCCdecl)]
		private static extern Int32 zmq_term(IntPtr context); /**/

		/* Deprecated. Use zmq_ctx_term instead. */
		[DllImport(LibraryName, EntryPoint = "zmq_term", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_term(IntPtr context);
		[DllImport(__Internal, EntryPoint = "zmq_term", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_term__Internal(IntPtr context);

		[DllImport(LibraryName, EntryPoint = "zmq_ctx_term", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_ctx_term(IntPtr context);
		[DllImport(__Internal, EntryPoint = "zmq_ctx_term", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_ctx_term__Internal(IntPtr context);
		public delegate Int32 zmq_ctx_term_delegate(IntPtr context);
		public static readonly zmq_ctx_term_delegate ctx_term = zmq_ctx_term;


		[DllImport(LibraryName, EntryPoint = "zmq_msg_init", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_init(IntPtr msg);
		[DllImport(__Internal, EntryPoint = "zmq_msg_init", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_init__Internal(IntPtr msg);
		public delegate Int32 zmq_msg_init_delegate(IntPtr msg);
		public static readonly zmq_msg_init_delegate msg_init = zmq_msg_init;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_init_size", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_init_size(IntPtr msg, Int32 size);
		[DllImport(__Internal, EntryPoint = "zmq_msg_init_size", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_init_size__Internal(IntPtr msg, Int32 size);
		public delegate Int32 zmq_msg_init_size_delegate(IntPtr msg, Int32 size);
		public static readonly zmq_msg_init_size_delegate msg_init_size = zmq_msg_init_size;

		[UnmanagedFunctionPointer(CCCdecl)]
		public delegate void FreeMessageDataDelegate(IntPtr data, IntPtr hint);

		[DllImport(LibraryName, EntryPoint = "zmq_msg_init_data", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_init_data(IntPtr msg, IntPtr data, Int32 size, FreeMessageDataDelegate ffn, IntPtr hint);
		[DllImport(__Internal, EntryPoint = "zmq_msg_init_data", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_init_data__Internal(IntPtr msg, IntPtr data, Int32 size, FreeMessageDataDelegate ffn, IntPtr hint);
		public delegate Int32 zmq_msg_init_data_delegate(IntPtr msg, IntPtr data, Int32 size, FreeMessageDataDelegate ffn, IntPtr hint);
		public static readonly zmq_msg_init_data_delegate msg_init_data = zmq_msg_init_data;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_send", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_send(IntPtr msg, IntPtr socket, Int32 flags);
		[DllImport(__Internal, EntryPoint = "zmq_msg_send", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_send__Internal(IntPtr msg, IntPtr socket, Int32 flags);
		public delegate Int32 zmq_msg_send_delegate(IntPtr msg, IntPtr socket, Int32 flags);
		public static readonly zmq_msg_send_delegate msg_send = zmq_msg_send;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_recv", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_recv(IntPtr msg, IntPtr socket, Int32 flags);
		[DllImport(__Internal, EntryPoint = "zmq_msg_recv", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_recv__Internal(IntPtr msg, IntPtr socket, Int32 flags);
		public delegate Int32 zmq_msg_recv_delegate(IntPtr msg, IntPtr socket, Int32 flags);
		public static readonly zmq_msg_recv_delegate msg_recv = zmq_msg_recv;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_close", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_close(IntPtr msg);
		[DllImport(__Internal, EntryPoint = "zmq_msg_close", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_close__Internal(IntPtr msg);
		public delegate Int32 zmq_msg_close_delegate(IntPtr msg);
		public static readonly zmq_msg_close_delegate msg_close = zmq_msg_close;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_data", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_msg_data(IntPtr msg);
		[DllImport(__Internal, EntryPoint = "zmq_msg_data", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_msg_data__Internal(IntPtr msg);
		public delegate IntPtr zmq_msg_data_delegate(IntPtr msg);
		public static readonly zmq_msg_data_delegate msg_data = zmq_msg_data;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_size", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_size(IntPtr msg);
		[DllImport(__Internal, EntryPoint = "zmq_msg_size", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_size__Internal(IntPtr msg);
		public delegate Int32 zmq_msg_size_delegate(IntPtr msg);
		public static readonly zmq_msg_size_delegate msg_size = zmq_msg_size;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_more", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_more(IntPtr msg);
		[DllImport(__Internal, EntryPoint = "zmq_msg_more", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_more__Internal(IntPtr msg);
		public delegate Int32 zmq_msg_more_delegate(IntPtr msg);
		public static readonly zmq_msg_more_delegate msg_more = zmq_msg_more;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_gets", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_msg_gets(IntPtr msg, IntPtr property);
		[DllImport(__Internal, EntryPoint = "zmq_msg_gets", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_msg_gets__Internal(IntPtr msg, IntPtr property);
		public delegate IntPtr zmq_msg_gets_delegate(IntPtr msg, IntPtr property);
		public static readonly zmq_msg_gets_delegate msg_gets = zmq_msg_gets;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_get", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_get(IntPtr msg, Int32 property);
		[DllImport(__Internal, EntryPoint = "zmq_msg_get", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_get__Internal(IntPtr msg, Int32 property);
		public delegate Int32 zmq_msg_get_delegate(IntPtr msg, Int32 property);
		public static readonly zmq_msg_get_delegate msg_get = zmq_msg_get;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_set", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_set(IntPtr msg, Int32 property, Int32 value);
		[DllImport(__Internal, EntryPoint = "zmq_msg_set", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_set__Internal(IntPtr msg, Int32 property, Int32 value);
		public delegate Int32 zmq_msg_set_delegate(IntPtr msg, Int32 property, Int32 value);
		public static readonly zmq_msg_set_delegate msg_set = zmq_msg_set;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_copy", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_copy(IntPtr dest, IntPtr src);
		[DllImport(__Internal, EntryPoint = "zmq_msg_copy", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_copy__Internal(IntPtr dest, IntPtr src);
		public delegate Int32 zmq_msg_copy_delegate(IntPtr dest, IntPtr src);
		public static readonly zmq_msg_copy_delegate msg_copy = zmq_msg_copy;

		[DllImport(LibraryName, EntryPoint = "zmq_msg_move", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_move(IntPtr dest, IntPtr src);
		[DllImport(__Internal, EntryPoint = "zmq_msg_move", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_msg_move__Internal(IntPtr dest, IntPtr src);
		public delegate Int32 zmq_msg_move_delegate(IntPtr dest, IntPtr src);
		public static readonly zmq_msg_move_delegate msg_move = zmq_msg_move;


		[DllImport(LibraryName, EntryPoint = "zmq_socket", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_socket(IntPtr context, Int32 type);
		[DllImport(__Internal, EntryPoint = "zmq_socket", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_socket__Internal(IntPtr context, Int32 type);
		public delegate IntPtr zmq_socket_delegate(IntPtr context, Int32 type);
		public static readonly zmq_socket_delegate socket = zmq_socket;

		[DllImport(LibraryName, EntryPoint = "zmq_close", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_close(IntPtr socket);
		[DllImport(__Internal, EntryPoint = "zmq_close", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_close__Internal(IntPtr socket);
		public delegate Int32 zmq_close_delegate(IntPtr socket);
		public static readonly zmq_close_delegate close = zmq_close;

		[DllImport(LibraryName, EntryPoint = "zmq_getsockopt", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_getsockopt(IntPtr socket, Int32 option_name, IntPtr option_value, IntPtr option_len);
		[DllImport(__Internal, EntryPoint = "zmq_getsockopt", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_getsockopt__Internal(IntPtr socket, Int32 option_name, IntPtr option_value, IntPtr option_len);
		public delegate Int32 zmq_getsockopt_delegate(IntPtr socket, Int32 option_name, IntPtr option_value, IntPtr option_len);
		public static readonly zmq_getsockopt_delegate getsockopt = zmq_getsockopt;

		[DllImport(LibraryName, EntryPoint = "zmq_setsockopt", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_setsockopt(IntPtr socket, Int32 option_name, IntPtr option_value, Int32 option_len);
		[DllImport(__Internal, EntryPoint = "zmq_setsockopt", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_setsockopt__Internal(IntPtr socket, Int32 option_name, IntPtr option_value, Int32 option_len);
		public delegate Int32 zmq_setsockopt_delegate(IntPtr socket, Int32 option_name, IntPtr option_value, Int32 option_len);
		public static readonly zmq_setsockopt_delegate setsockopt = zmq_setsockopt;

		[DllImport(LibraryName, EntryPoint = "zmq_bind", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_bind(IntPtr socket, IntPtr endpoint);
		[DllImport(__Internal, EntryPoint = "zmq_bind", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_bind__Internal(IntPtr socket, IntPtr endpoint);
		public delegate Int32 zmq_bind_delegate(IntPtr socket, IntPtr endpoint);
		public static readonly zmq_bind_delegate bind = zmq_bind;

		[DllImport(LibraryName, EntryPoint = "zmq_unbind", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_unbind(IntPtr socket, IntPtr endpoint);
		[DllImport(__Internal, EntryPoint = "zmq_unbind", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_unbind__Internal(IntPtr socket, IntPtr endpoint);
		public delegate Int32 zmq_unbind_delegate(IntPtr socket, IntPtr endpoint);
		public static readonly zmq_unbind_delegate unbind = zmq_unbind;

		[DllImport(LibraryName, EntryPoint = "zmq_connect", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_connect(IntPtr socket, IntPtr endpoint);
		[DllImport(__Internal, EntryPoint = "zmq_connect", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_connect__Internal(IntPtr socket, IntPtr endpoint);
		public delegate Int32 zmq_connect_delegate(IntPtr socket, IntPtr endpoint);
		public static readonly zmq_connect_delegate connect = zmq_connect;

		[DllImport(LibraryName, EntryPoint = "zmq_disconnect", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_disconnect(IntPtr socket, IntPtr endpoint);
		[DllImport(__Internal, EntryPoint = "zmq_disconnect", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_disconnect__Internal(IntPtr socket, IntPtr endpoint);
		public delegate Int32 zmq_disconnect_delegate(IntPtr socket, IntPtr endpoint);
		public static readonly zmq_disconnect_delegate disconnect = zmq_disconnect;

		// Using void* to be liberal for zmq_pollitem_windows_t and _posix_t
		[DllImport(LibraryName, EntryPoint = "zmq_poll", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_poll(void* items, Int32 numItems, long timeout);
		[DllImport(__Internal, EntryPoint = "zmq_poll", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_poll__Internal(void* items, Int32 numItems, long timeout);
		// private static extern Int32 zmq_poll(IntPtr items, Int32 numItems, long timeout);
		public delegate Int32 zmq_poll_delegate(void* items, Int32 numItems, long timeout);
		public static readonly zmq_poll_delegate poll = zmq_poll;


		[DllImport(LibraryName, EntryPoint = "zmq_send", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_send(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);
		[DllImport(__Internal, EntryPoint = "zmq_send", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_send__Internal(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);
		public delegate Int32 zmq_send_delegate(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);
		public static readonly zmq_send_delegate send = zmq_send;

		// [DllImport(LibraryName, CallingConvention = CCCdecl)]
		// private static extern Int32 zmq_send_const(IntPtr socket, IntPtr buf, Int32 size, Int32 flags);

		[DllImport(LibraryName, EntryPoint = "zmq_recv", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_recv(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);
		[DllImport(__Internal, EntryPoint = "zmq_recv", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_recv__Internal(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);
		public delegate Int32 zmq_recv_delegate(IntPtr socket, IntPtr buf, Int32 len, Int32 flags);
		public static readonly zmq_recv_delegate recv = zmq_recv;


		[DllImport(LibraryName, EntryPoint = "zmq_has", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_has(IntPtr capability);
		[DllImport(__Internal, EntryPoint = "zmq_has", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_has__Internal(IntPtr capability);
		public delegate Int32 zmq_has_delegate(IntPtr capability);
		public static readonly zmq_has_delegate has = zmq_has;

		[DllImport(LibraryName, EntryPoint = "zmq_socket_monitor", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_socket_monitor(IntPtr socket, IntPtr endpoint, Int32 events);
		[DllImport(__Internal, EntryPoint = "zmq_socket_monitor", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_socket_monitor__Internal(IntPtr socket, IntPtr endpoint, Int32 events);
		public delegate Int32 zmq_socket_monitor_delegate(IntPtr socket, IntPtr endpoint, Int32 events);
		public static readonly zmq_socket_monitor_delegate socket_monitor = zmq_socket_monitor;


		[DllImport(LibraryName, EntryPoint = "zmq_proxy", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_proxy(IntPtr frontend, IntPtr backend, IntPtr capture);
		[DllImport(__Internal, EntryPoint = "zmq_proxy", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_proxy__Internal(IntPtr frontend, IntPtr backend, IntPtr capture);
		public delegate Int32 zmq_proxy_delegate(IntPtr frontend, IntPtr backend, IntPtr capture);
		public static readonly zmq_proxy_delegate proxy = zmq_proxy;

		[DllImport(LibraryName, EntryPoint = "zmq_proxy_steerable", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_proxy_steerable(IntPtr frontend, IntPtr backend, IntPtr capture, IntPtr control);
		[DllImport(__Internal, EntryPoint = "zmq_proxy_steerable", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_proxy_steerable__Internal(IntPtr frontend, IntPtr backend, IntPtr capture, IntPtr control);
		public delegate Int32 zmq_proxy_steerable_delegate(IntPtr frontend, IntPtr backend, IntPtr capture, IntPtr control);
		public static readonly zmq_proxy_steerable_delegate proxy_steerable = zmq_proxy_steerable;


		[DllImport(LibraryName, EntryPoint = "zmq_curve_keypair", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_curve_keypair(IntPtr z85_public_key, IntPtr z85_secret_key);
		[DllImport(__Internal, EntryPoint = "zmq_curve_keypair", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_curve_keypair__Internal(IntPtr z85_public_key, IntPtr z85_secret_key);
		public delegate Int32 zmq_curve_keypair_delegate(IntPtr z85_public_key, IntPtr z85_secret_key);
		public static readonly zmq_curve_keypair_delegate curve_keypair = zmq_curve_keypair;

		[DllImport(LibraryName, EntryPoint = "zmq_z85_encode", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_z85_encode(IntPtr dest, IntPtr data, Int32 size);
		[DllImport(__Internal, EntryPoint = "zmq_z85_encode", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_z85_encode__Internal(IntPtr dest, IntPtr data, Int32 size);
		public delegate IntPtr zmq_z85_encode_delegate(IntPtr dest, IntPtr data, Int32 size);
		public static readonly zmq_z85_encode_delegate z85_encode = zmq_z85_encode;

		[DllImport(LibraryName, EntryPoint = "zmq_z85_decode", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_z85_decode(IntPtr dest, IntPtr data);
		[DllImport(__Internal, EntryPoint = "zmq_z85_decode", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_z85_decode__Internal(IntPtr dest, IntPtr data);
		public delegate IntPtr zmq_z85_decode_delegate(IntPtr dest, IntPtr data);
		public static readonly zmq_z85_decode_delegate z85_decode = zmq_z85_decode;


		[DllImport(LibraryName, EntryPoint = "zmq_errno", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_errno();
		[DllImport(__Internal, EntryPoint = "zmq_errno", CallingConvention = CCCdecl)]
		private static extern Int32 zmq_errno__Internal();
		public delegate Int32 zmq_errno_delegate();
		public static readonly zmq_errno_delegate errno = zmq_errno;

		[DllImport(LibraryName, EntryPoint = "zmq_strerror", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_strerror(int errnum);
		[DllImport(__Internal, EntryPoint = "zmq_strerror", CallingConvention = CCCdecl)]
		private static extern IntPtr zmq_strerror__Internal(int errnum);
		public delegate IntPtr zmq_strerror_delegate(int errnum);
		public static readonly zmq_strerror_delegate strerror = zmq_strerror;

	}
}
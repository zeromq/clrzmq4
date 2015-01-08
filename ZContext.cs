using System;
using System.Text;
using System.Threading;

using ZeroMQ.lib;

namespace ZeroMQ
{
	/// <summary>
	/// Creates <see cref="ZSocket"/> instances within a process boundary.
	/// </summary>
	/// <remarks>
	/// The <see cref="ZContext"/> object is a container for all sockets in a single process,
	/// and acts as the transport for inproc sockets. <see cref="ZContext"/> is thread safe.
	/// A <see cref="ZContext"/> must not be terminated until all spawned sockets have been
	/// successfully closed.
	/// </remarks>
	public class ZContext : IDisposable
	{
		internal static Encoding _encoding = System.Text.Encoding.UTF8;

		/// <summary>
		/// Gets or sets the default encoding
		/// </summary>
		public static Encoding Encoding
		{
			get { return _encoding; }
			protected set { _encoding = value; }
		}

		static ZContext()
		{
			// System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(zmq).TypeHandle);
		}

		public static bool Has(string capability)
		{
			using (var capabilityPtr = DispoIntPtr.AllocString(capability))
			{
				if (0 < zmq.has(capabilityPtr))
				{
					return true;
				}
			}
			return false;
		}

		public static bool Proxy(ZSocket frontend, ZSocket backend, out ZError error)
		{
			return Proxy(frontend, backend, null, out error);
		}

		public static bool Proxy(ZSocket frontend, ZSocket backend, ZSocket capture, out ZError error)
		{
			error = ZError.None;

			while (-1 == zmq.proxy(frontend.SocketPtr, backend.SocketPtr, capture == null ? IntPtr.Zero : capture.SocketPtr))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = ZError.None;
					continue;
				}

				return false;
			}
			return true;
		}

		public static bool Proxy(ZSocket frontend, ZSocket backend, ZSocket capture, ZSocket control, out ZError error)
		{
			error = ZError.None;

			while (-1 == zmq.proxy_steerable(frontend.SocketPtr, backend.SocketPtr, capture == null ? IntPtr.Zero : capture.SocketPtr, control == null ? IntPtr.Zero : control.SocketPtr))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = ZError.None;
					continue;
				}

				return false;
			}
			return true;
		}

		private readonly IntPtr _contextPtr;

		private bool _disposed;

		internal ZContext()
		{
			_contextPtr = zmq.ctx_new();

			if (_contextPtr == IntPtr.Zero)
			{
				throw new InvalidProgramException("zmq_ctx_new");
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="ZContext"/> class.
		/// </summary>
		~ZContext()
		{
			Dispose(false);
		}

		/// <summary>
		/// Gets a handle to the native 0MQ context.
		/// </summary>
		/// <remarks>
		/// May be used to share a single ZmqContext with native libraries that use the
		/// 0MQ API directly. Allows the inproc transport to be used if a single process
		/// has a heterogeneous codebase.
		/// </remarks>
		public IntPtr ContextPtr
		{
			get { return _contextPtr; }
		}

		/// <summary>
		/// Create a <see cref="ZContext"/> instance.
		/// </summary>
		/// <returns>A <see cref="ZContext"/> instance with the default thread pool size (1).</returns>
		public static ZContext Create()
		{
			return new ZContext();
		}

		public void SetOption(ZContextOption option, int optionValue)
		{
			int rc = zmq.ctx_set(_contextPtr, option.Number, optionValue);
			if (rc == -1)
			{
				var error = ZError.GetLastErr();

				if (error == ZError.EINVAL)
				{
					throw new ArgumentOutOfRangeException(
						string.Format("The requested option optionName \"{0}\" is invalid.",
							option));
				}
				throw new ZException(error);
			}
		}

		public int GetOption(ZContextOption option)
		{
			int rc = zmq.ctx_get(_contextPtr, option.Number);
			if (rc == -1)
			{
				var error = ZError.GetLastErr();

				if (error == ZError.EINVAL)
				{
					throw new ArgumentOutOfRangeException(
						string.Format("The requested option optionName \"{0}\" is invalid.",
								  option));
				}
				throw new ZException(error);
			}
			return rc;
		}

		/// <summary>
		/// Gets or sets the size of the thread pool for the current context (default = 1).
		/// </summary>
		public int ThreadPoolSize
		{
			get { return GetOption(ZContextOption.IO_THREADS); }
			set { SetOption(ZContextOption.IO_THREADS, value); }
		}

		/// <summary>
		/// Gets or sets the maximum number of sockets for the current context (default = 1024).
		/// </summary>
		public int MaxSockets
		{
			get { return GetOption(ZContextOption.MAX_SOCKETS); }
			set { SetOption(ZContextOption.MAX_SOCKETS, value); }
		}

		/// <summary>
		/// Gets or sets the supported socket protocol(s) when using TCP transports. (Default = <see cref="ProtocolType.Ipv4Only"/>).
		/// </summary>
		/// <exception cref="ZmqSocketException">An error occurred when getting or setting the socket option.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="ZSocket"/> has been closed.</exception>
		public bool IPv6Enabled
		{
			get { return GetOption(ZContextOption.IPV6) == 1; }
			set { SetOption(ZContextOption.IPV6, value ? 1 : 0); }
		}

		/// <summary>
		/// Terminate the ZeroMQ context.
		/// </summary>
		/// <exception cref="System.ObjectDisposedException">The <see cref="ZContext"/> has already been disposed.</exception>
		public void Terminate()
		{
			EnsureNotDisposed();

			// int retry = 3;
			ZError error;
			while (/*--retry > -1 &&*/ -1 == zmq.ctx_term(_contextPtr))
			{
				error = ZError.GetLastErr();

				if (error == ZError.EINTR)
				{
					error = ZError.None;
					Thread.Sleep(1);

					continue;
				}

				// Maybe ZmqStdError.EFAULT
				break;
			}
		}

		/// <summary>
		/// Releases all resources used by the current instance of the <see cref="ZContext"/> class.
		/// </summary>
		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="ZContext"/>, and optionally disposes of the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				Terminate();
			}

			_disposed = true;
		}

		private void EnsureNotDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}
	}
}
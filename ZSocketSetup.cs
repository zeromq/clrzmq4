namespace ZeroMQ
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Reflection;

	/// <summary>
	/// Defines a fluent interface for configuring device sockets.
	/// </summary>
	public class ZSocketSetup
	{
		readonly ZSocket _socket;
		readonly List<Action<ZSocket>> _socketInitializers;
		readonly List<string> _bindings;
		readonly List<string> _connections;

		bool _isConfigured;

		public ZSocketSetup(ZSocket socket)
		{
			if (socket == null)
			{
				throw new ArgumentNullException("socket");
			}

			_socket = socket;
			_socketInitializers = new List<Action<ZSocket>>();
			_bindings = new List<string>();
			_connections = new List<string>();
		}

		/// <summary>
		/// Configure the socket to bind to a given endpoint. See <see cref="ZSocket.Bind"/> for details.
		/// </summary>
		/// <param name="endpoint">A string representing the endpoint to which the socket will bind.</param>
		/// <returns>The current <see cref="ZSocketSetup"/> object.</returns>
		public ZSocketSetup Bind(string endpoint)
		{
			if (endpoint == null)
			{
				throw new ArgumentNullException("endpoint");
			}

			_bindings.Add(endpoint);

			return this;
		}

		/// <summary>
		/// Configure the socket to connect to a given endpoint. See <see cref="ZSocket.Connect"/> for details.
		/// </summary>
		/// <param name="endpoint">A string representing the endpoint to which the socket will connect.</param>
		/// <returns>The current <see cref="ZSocketSetup"/> object.</returns>
		public ZSocketSetup Connect(string endpoint)
		{
			if (endpoint == null)
			{
				throw new ArgumentNullException("endpoint");
			}

			_connections.Add(endpoint);

			return this;
		}

		/// <summary>
		/// Set an int-based socket option.
		/// </summary>
		/// <param name="property">The <see cref="ZSocket"/> property to set.</param>
		/// <param name="value">The int value to assign.</param>
		/// <returns>The current <see cref="ZSocketSetup"/> object.</returns>
		public ZSocketSetup SetSocketOption(Expression<Func<ZSocket, int>> property, int value)
		{
			return SetSocketOption<int>(property, value);
		}

		/// <summary>
		/// Set a long-based socket option.
		/// </summary>
		/// <param name="property">The <see cref="ZSocket"/> property to set.</param>
		/// <param name="value">The long value to assign.</param>
		/// <returns>The current <see cref="ZSocketSetup"/> object.</returns>
		public ZSocketSetup SetSocketOption(Expression<Func<ZSocket, long>> property, long value)
		{
			return SetSocketOption<long>(property, value);
		}

		/// <summary>
		/// Set a ulong-based socket option.
		/// </summary>
		/// <param name="property">The <see cref="ZSocket"/> property to set.</param>
		/// <param name="value">The ulong value to assign.</param>
		/// <returns>The current <see cref="ZSocketSetup"/> object.</returns>
		public ZSocketSetup SetSocketOption(Expression<Func<ZSocket, ulong>> property, ulong value)
		{
			return SetSocketOption<ulong>(property, value);
		}

		/// <summary>
		/// Set a byte array-based socket option.
		/// </summary>
		/// <param name="property">The <see cref="ZSocket"/> property to set.</param>
		/// <param name="value">The byte array value to assign.</param>
		/// <returns>The current <see cref="ZSocketSetup"/> object.</returns>
		public ZSocketSetup SetSocketOption(Expression<Func<ZSocket, byte[]>> property, byte[] value)
		{
			return SetSocketOption<byte[]>(property, value);
		}

		/// <summary>
		/// Set a <see cref="TimeSpan"/>-based socket option.
		/// </summary>
		/// <param name="property">The <see cref="ZSocket"/> property to set.</param>
		/// <param name="value">The <see cref="TimeSpan"/> value to assign.</param>
		/// <returns>The current <see cref="ZSocketSetup"/> object.</returns>
		public ZSocketSetup SetSocketOption(Expression<Func<ZSocket, TimeSpan>> property, TimeSpan value)
		{
			return SetSocketOption<TimeSpan>(property, value);
		}

		private byte[] _subscription;

		/// <summary>
		/// Configure the socket to subscribe to a specific prefix. See <see cref="ZSocket.Subscribe"/> for details.
		/// </summary>
		/// <param name="prefix">A byte array containing the prefix to which the socket will subscribe.</param>
		/// <returns>The current <see cref="ZSocketSetup"/> object.</returns>
		public ZSocketSetup Subscribe(byte[] prefix)
		{
			_subscription = prefix;
			return this;
		}

		/// <summary>
		/// Configure the socket to subscribe to all incoming messages. See <see cref="ZSocket.SubscribeAll"/> for details.
		/// </summary>
		/// <returns>The current <see cref="ZSocketSetup"/> object.</returns>
		public ZSocketSetup SubscribeAll()
		{
			_subscription = new byte[2] { 0x01, 0x00 };
			return this;
		}

		internal ZSocketSetup AddSocketInitializer(Action<ZSocket> setupMethod)
		{
			_socketInitializers.Add(setupMethod);

			return this;
		}

		internal void Configure()
		{
			if (_isConfigured)
			{
				return;
			}

			if (_bindings.Count == 0 && _connections.Count == 0)
			{
				throw new InvalidOperationException("Device sockets must bind or connect to at least one endpoint.");
			}

			foreach (Action<ZSocket> initializer in _socketInitializers)
			{
				initializer.Invoke(_socket);
			}

			_isConfigured = true;
		}

		internal void BindConnect()
		{
			foreach (string endpoint in _bindings)
			{
				_socket.Bind(endpoint);
			}

			foreach (string endpoint in _connections)
			{
				_socket.Connect(endpoint);
			}

			if (_subscription != null)
			{
				// _socket.Subscribe(_subscription);

				using (var subscription = new ZFrame(_subscription))
				{
					_socket.Send(subscription);
				}
			}
		}

		internal void UnbindDisconnect()
		{

			/* if (_subscription != null)
			{
					_socket.Unsubscribe(_subscription);
			} */

			ZError error;

			foreach (string endpoint in _bindings)
			{
				_socket.Unbind(endpoint, out error);
			}

			foreach (string endpoint in _connections)
			{
				_socket.Disconnect(endpoint, out error);
			}
		}

		private ZSocketSetup SetSocketOption<T>(Expression<Func<ZSocket, T>> property, T value)
		{
			PropertyInfo propertyInfo;

			if (property.Body is MemberExpression)
			{
				propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
			}
			else
			{
				propertyInfo = ((MemberExpression)((UnaryExpression)property.Body).Operand).Member as PropertyInfo;
			}

			if (propertyInfo == null)
			{
				throw new InvalidOperationException("The specified ZSocket member is not a property: " + property.Body);
			}

			_socketInitializers.Add(s => propertyInfo.SetValue(s, value, null));

			return this;
		}
	}
}
namespace ZeroMQ
{
	using System;
	using System.Runtime.Serialization;

	using lib;
	using System.Runtime.InteropServices;

	/// <summary>
	/// An exception thrown by the result of libzmq.
	/// </summary>
	[Serializable]
	public class ZException : Exception
	{
		private int? _errno = -1;
		private string _errname = null;
		private string _errtext = null;

		/// <summary>
		/// Gets the error code returned by libzmq.
		/// </summary>
		public int ErrNo
		{
			get
			{
				return !_errno.HasValue ? 0 : _errno.Value;
			}
		}
		/// <summary>
		/// Gets the error code returned by libzmq.
		/// </summary>
		public string ErrName
		{
			get
			{
				return _errname == null ? string.Empty : _errname;
			}
		}

		/// <summary>
		/// Gets the error text returned by libzmq.
		/// </summary>
		public string ErrText
		{
			get
			{
				return _errtext == null ? string.Empty : _errtext;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZException"/> class.
		/// </summary>
		/// <param name="errorCode">The error code returned by the ZeroMQ library call.</param>
		public ZException()
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ZException"/> class.
		/// </summary>
		/// <param name="errorCode">The error code returned by the ZeroMQ library call.</param>
		public ZException(ZError errorSymbol)
			: this(errorSymbol, default(string), default(Exception))
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ZException"/> class.
		/// </summary>
		/// <param name="errorCode">The error code returned by the ZeroMQ library call.</param>
		public ZException(ZError errorSymbol, string message)
			: this(errorSymbol, message, default(Exception))
		{ }

		public ZException(ZError errorSymbol, string message, Exception inner)
			: base(default(string), inner)
		{
			if (errorSymbol != null)
			{
				this._errno = errorSymbol.Number;
				this._errname = errorSymbol.Name;
				this._errtext = errorSymbol.Text;
			}
			else
			{
				this._errno = -1;
			}
			_message = message;
		}

		private string _message;

		public override string Message
		{
			get
			{
				if (!string.IsNullOrEmpty(_message))
				{
					return
						string.Format("{0}({1}): {2}: {3}",
							ErrName, ErrNo, ErrText, _message);
				}
				return
					string.Format("{0}({1}): {2}",
						ErrName, ErrNo, ErrText);
			}
		}

		public override string ToString()
		{
			return Message;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZException"/> class.
		/// </summary>
		/// <param name="errorCode">The error code returned by the ZeroMQ library call.</param>
		public ZException(int errorCode)
			: this(errorCode, default(string), default(Exception))
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ZException"/> class
		/// using zmq_strerror(int errno)
		/// </summary>
		/// <param name="errorCode">The error code returned by the ZeroMQ library call.</param>
		public ZException(int errorCode, string errorText)
			: this(errorCode, errorText, default(Exception))
		{ }

		public ZException(int errorCode, string errorText, Exception inner)
			: base(default(string), inner)
		{
			this._errno = errorCode;
			this._errtext = errorText;
			if (this._errtext == null && this._errno.HasValue)
			{
				this._errtext = Marshal.PtrToStringAnsi(zmq.strerror(this._errno.Value));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZException"/> class.
		/// </summary>
		/// <param name="info"><see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context"><see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected ZException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }

	}
}
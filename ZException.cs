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
	    private ZError _error;

		/// <summary>
		/// Gets the error code returned by libzmq.
		/// </summary>
        [Obsolete("Use Error property instead")]
        public int ErrNo
		{
            // TODO why is "0" the default?
			get { return _error != null ? _error.Number : 0; }
		}
		/// <summary>
		/// Gets the error code returned by libzmq.
		/// </summary>
        [Obsolete("Use Error property instead")]
        public string ErrName
		{
			get
			{
				return _error != null ? _error.Name : string.Empty;
			}
		}

        /// <summary>
        /// Gets the error text returned by libzmq.
        /// </summary>
        [Obsolete("Use Error property instead")]
        public string ErrText
		{
			get
			{
				return _error != null ? _error.Text : string.Empty;
			}
		}

	    public ZError Error
	    {
	        get { return _error; }
	    }

	    /// <summary>
        /// Initializes a new instance of the <see cref="ZException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code returned by the ZeroMQ library call.</param>
        protected ZException()
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
			: base(MakeMessage(errorSymbol, message), inner)
		{
		    this._error = errorSymbol;
		}

		static string MakeMessage(ZError error, string additionalMessage)
		{
		    return error != null
		        ? (string.IsNullOrEmpty(additionalMessage)
		            ? error.ToString()
		            : string.Format("{0}: {1}", error, additionalMessage))
		        : additionalMessage;
		}

	    public override string ToString()
		{
			return Message;
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
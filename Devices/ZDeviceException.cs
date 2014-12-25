namespace ZeroMQ.Devices
{
    using System;
    using System.Runtime.Serialization;

    using lib;

    /// <summary>
    /// The exception that is thrown when a ZeroMQ device error occurs.
    /// </summary>
    [Serializable]
    public class ZDeviceException : ZException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZDeviceException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code returned by the ZeroMQ library call.</param>
		public ZDeviceException(ZError errorCode)
            : base(errorCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZDeviceException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code returned by the ZeroMQ library call.</param>
        /// <param name="message">The message that describes the error</param>
		public ZDeviceException(ZError errorCode, string message)
            : base(errorCode, message)
        {
        }

        /*/ <summary>
        /// Initializes a new instance of the <see cref="ZmqDeviceException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code returned by the ZeroMQ library call.</param>
        /// <param name="message">The message that describes the error</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ZmqDeviceException(ZmqError errorCode, string message, Exception inner)
            : base(errorCode, message, inner)
        {
        } */

        /// <summary>
        /// Initializes a new instance of the <see cref="ZDeviceException"/> class.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context"><see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ZDeviceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

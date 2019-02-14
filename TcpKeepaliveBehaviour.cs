namespace ZeroMQ
{
	/// <summary>
	/// Keep-alive packets behavior for a <see cref="ZSocket"/> connection.
	/// </summary>
	public enum TcpKeepaliveBehaviour : int
	{
		/// <summary>
		/// Use Operating System default behavior.
		/// </summary>
		Default = -1,

		/// <summary>
		/// Disable keep-alive packets.
		/// </summary>
		Disable = 0,

		/// <summary>
		/// Enable keep-alive packets.
		/// </summary>
		Enable = 1,
	}
}
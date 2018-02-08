namespace ZeroMQ
{
	using System;

	[Flags]
	public enum ZPoll : short
	{
		None = 0x0,
		In = 0x1,
		Out = 0x2,
		Err = 0x4
	}
}
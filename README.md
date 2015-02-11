
**ZeroMQ CLR namespace**

Hello. I've made a new ZeroMQ namespace for .NET Framework 4+ and mono 3+.

ZeroMQ is built AnyCPU, to run on Windows (VC2010) and on GNU/Linux (C 4.8.2), i386 and amd64.

You can get it
- by [downloading the Release](https://github.com/zeromq/clrzmq4/releases/tag/master00),
- using [nuget](https://www.nuget.org/packages/ZeroMQ/) `PM> Install-Package ZeroMQ` or by [downloading the nupkg](https://packages.nuget.org/api/v1/package/ZeroMQ/),
- using `git clone https://github.com/zeromq/clrzmq4`

Read: [ZeroMQ - The Guide](http://zguide.zeromq.org/page:all)
- ZeroMQ - [The Guide Examples for C#](http://github.com/metadings/zguide/tree/master/examples/C%23)
- ZeroMQ - [C# Projects](http://github.com/metadings/clrzmq-test)

**[Simple REQ-REP](https://github.com/metadings/zguide/blob/master/examples/C%23/Beispiel.cs)**

```csharp
using System;
using System.Collections.Generic;
using System.Threading;

using ZeroMQ;

namespace Examples
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//
			// Simple REQ-REP
			//
			// Author: metadings
			//

			if (args == null || args.Length < 1)
			{
				args = new string[] { "World", "You" };
			}

			// Setup the ZContext
			using (var ctx = new ZContext())
			{
				// Create a cancellor
				var cancellor = new CancellationTokenSource();

				// Start the "Server"
				var server = new Thread( () => Server(ctx, cancellor.Token) );
				server.Start();

				// Now we are the Client, asking the Server
				foreach (string arg in args)
				{
					Console.WriteLine( Client(ctx, arg) );
				}

				// Shutdown the ZContext
				// ctx.Shutdown();
				// server.Join();

				// Cancel the Server
				cancellor.Cancel();
				server.Join();
			}
		}

		static void Server(ZContext ctx, CancellationToken cancellus)
		{
			ZError error;

			using (var socket = new ZSocket(ctx, ZSocketType.REP))
			{
				socket.Bind("inproc://helloworld");

				ZFrame request;

				while (!cancellus.IsCancellationRequested)
				{
					if (null == (request = socket.ReceiveFrame(ZSocketFlags.DontWait, out error)))
					{
						if (error == ZError.EAGAIN)
						{
							Thread.Sleep(1);
							continue;
						}
						if (error == ZError.ETERM)
							break;  // Interrupted
						throw new ZException(error);
					}

					using (request)
					{
						// Let the response be "Hello " + input
						socket.Send(new ZFrame("Hello " + request.ReadString()));
					}
				}
			}
		}

		static string Client(ZContext ctx, string name)
		{
			using (var socket = new ZSocket(ctx, ZSocketType.REQ))
			{
				socket.Connect("inproc://helloworld");

				socket.Send(new ZFrame(name));

				using (ZFrame response = socket.ReceiveFrame())
				{
					return response.ReadString();
				}
			}
		}
	}
}
```

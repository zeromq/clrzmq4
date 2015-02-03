
**ZeroMQ CLR namespace**

Hello. I've made a new ZeroMQ namespace for .NET Framework 4+ and mono 3+.

Also read: [ZeroMQ - The Guide](http://zguide.zeromq.org/page:all)

- ZeroMQ - [The Guide Examples for C#](http://github.com/metadings/zguide/tree/master/examples/C%23)
- ZeroMQ - [C# Projects](http://github.com/metadings/clrzmq-test)

**Simple REQ connect to REP bind**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ZeroMQ;

namespace ZeroMQ.Test
{
	class Program
	{
		static ZContext context;

		static void Main(string[] args)
		{
			if (args == null || args.Length < 1)
			{
				args = new string[] { "World" };
			}

			// Setup the ZContext
			using (context = new ZContext())
			{
				// Create a cancellor
				var cancellor = new CancellationTokenSource();

				// Start the "Server"
				new Thread(() => Server(cancellor.Token)).Start();

				// Now we are the Client, asking the Server
				foreach (string arg in args)
				{
					Client(arg);
				}

				// Cancel the Server
				cancellor.Cancel();
			}
		}

		static void Server(CancellationToken cancellus)
		{
			using (var socket = new ZSocket(context, ZSocketType.REP))
			{
				socket.Bind("inproc://helloworld");

				ZFrame request;
				ZError error;

				while (!cancellus.IsCancellationRequested)
				{
					if (null == (request = socket.ReceiveFrame(ZSocketFlags.DontWait, out error)))
					{
						if (error == ZError.EAGAIN) {
							error = ZError.None;
							Thread.Sleep(1);

							continue;
						}
						if (error = ZError.ETERM)
							break;	// Interrupted
						throw new ZException(error);
					}

					// Let the response be "Hello " + input
					using (request)
					using (var response = new ZFrame("Hello " + request.ReadString()))
					{
						socket.Send(response);
					}
				}
			}
		}

		static void Client(string name)
		{
			using (var socket = new ZSocket(context, ZSocketType.REQ))
			{
				socket.Connect("inproc://helloworld");
				
				using (var request = new ZFrame(name))
				{
					socket.Send(request);
				}

				using (ZFrame response = socket.ReceiveFrame())
				{
					Console.WriteLine( response.ReadString() );
				}
			}
		}
	}
}
```

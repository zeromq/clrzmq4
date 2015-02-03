
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
			// Setup the ZContext
			using (context = new ZContext())
			{
				// Start the "Server"
				var cancellor = new CancellationTokenSource();
				new Thread(() => Server(cancellor.Token)).Start();

				if (args == null || args.Length < 1)
				{
					// say there were some arguments...
					args = new string[] { "World" };
				}

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

				ZError error;
				ZMessage request;

				while (!cancellus.IsCancellationRequested)
				{
					if (null == (request = socket.ReceiveMessage(ZSocketFlags.DontWait, out error)))
					{
						if (error == ZError.EAGAIN) {
							error = ZError.None;
							Thread.Sleep(1);

							continue;
						}

						throw new ZException(error);
					}

					// Let the response be "Hello " + input
					using (request)
					using (var response = new ZFrame("Hello " + request[0].ReadString()))
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

				using (ZMessage response = socket.ReceiveMessage())
				{
					Console.WriteLine( response[0].ReadString() );
				}
			}
		}
	}
}
```

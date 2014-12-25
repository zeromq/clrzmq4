
**ZeroMQ CLR namespace**

Hello. I've made a new ZeroMQ namespace for .NET Framework 4+ and mono 3+.

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
            context = ZContext.Create();

            // Start the "Server"
            var cancellor = new CancellationTokenSource();
            new Thread(Server).Start(cancellor.Token);

            if (args == null || args.Length < 1)
            {
                // say here were some arguments...
                args = new string[] { "World" };
            }

            // foreach arg we are the Client, asking the Server
            foreach (string arg in args)
            {
                Console.WriteLine( Client(arg) );
            }

            // Cancel the Server
            cancellor.Cancel();
            // we could have done here context.Terminate()
        }

        static void Server(object cancelluS)
        {
            var cancellus = (CancellationToken)cancelluS;

            ZError error;
            using (ZSocket socket = context.CreateSocket(ZSocketType.REP, out error))
            {
                socket.Bind("inproc://helloworld", out error);

                while (!cancellus.IsCancellationRequested)
                {
                    ZMessage request;
                    if (null == (request = socket.ReceiveMessage(ZSocketFlags.DontWait, out error)))
                    {
                        if (error == ZError.EAGAIN) {
                            error = ZError.None;
                            Thread.Sleep(1);

                            continue;
                        }

                        if (error == ZError.ETERM) return;
                        throw new ZException(error);
                    }

                    // Let the response be "Hello " + input
                    using (request)
                    using (var response = new ZMessage())
                    {
                        response.Add(ZFrame.CreateFromString("Hello " + request[0].ReadString()));
                        socket.SendMessage(response, out error);
                    }
                    // using (var response = ZFrame.CreateFromString("Hello " + request[0].ReadString())) {
                    //     socket.SendFrame(response, out error);
                    // }
                }
            }
        }

        static string Client(string name)
        {
            string output = null;

            ZError error;
            using (ZSocket socket = context.CreateSocket(ZSocketType.REQ, out error))
            {
                socket.Connect("inproc://helloworld", out error);

                using (var request = new ZMessage())
                {
                    request.Add(ZFrame.CreateFromString(name));
                    socket.SendMessage(request, out error);
                }
                // using (var request = ZFrame.CreateFromString(name)) {
                //     socket.SendFrame(request, out error);
                // }

                using (ZMessage response = socket.ReceiveMessage(out error))
                {
                    output = response[0].ReadString();
                }
                // using (var response = socket.ReceiveFrame(out error)) {
                //     output = response.ReadString();
                // }
            }

            return output;
        }
    }
}
```

**ZeroMQ CLR namespace**

Hello. I've made originally a fork of the clrzmq project, however I've touched nearly every file.

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
                    if (request.Length < 1)
                    {
                        continue;
                    }

                    // Let the response be "Hello " + input
                    var response = new ZMessage();
                    response.Add(ZFrame.CreateFromString("Hello " + request[0].ReadString()));
                    // var response = ZFrame.CreateFromString("Hello " + request[0].ReadString());

                    socket.SendMessage(response, out error);
                    // socket.SendFrame(response, out error);
                }
            }
        }

        static string Client(string name)
        {
            string output = null;

            ZError error;
            using (ZSocket socket = context.CreateSocket(ZSocketType.REQ, out error))
            {
                // var message = ZFrame.CreateFromString("Hello World");

                socket.Connect("inproc://helloworld", out error);

                var request = new ZMessage();
                request.Add(ZFrame.CreateFromString(name));
                // var request = ZFrame.CreateFromString(name);

                socket.SendMessage(request, out error);
                // socket.SendFrame(request, out error);

                ZMessage response = socket.ReceiveMessage(out error);
                // var response = socket.ReceiveFrame(out error);
                output = response[0].ReadString();
                // output = response.ReadString();
            }

            return output;
        }
    }
}
```
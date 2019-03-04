using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZeroMQ;

namespace Examples
{
    static partial class Program
    {
        public static void IronhouseServer(string[] args)
        {
            //
            // Hello World server with ironhouse security
            //
            // Author: hawkans
            //

            if (args == null || args.Length < 1)
            {
                Console.WriteLine();
                Console.WriteLine("Usage: ./{0} Ironhouse HWServer [Name]", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine();
                Console.WriteLine("    Name   Your name. Default: World");
                Console.WriteLine();
                args = new string[] { "World" };
            }

            string name = args[0];
            // Create or load certificates
            ZCert clientCert = GetOrCreateCert("clienttest");
            ZCert serverCert = GetOrCreateCert("servertest");

            using (var responder = new ZSocket(ZSocketType.REP))
            using (var actor = new ZActor(ZAuth.Action0, null))
            {
                actor.Start();
                // send CURVE settings to ZAuth
                actor.Frontend.Send(new ZFrame("VERBOSE"));
                actor.Frontend.Send(new ZMessage(new List<ZFrame>()
                    { new ZFrame("ALLOW"), new ZFrame("127.0.0.1") } ));
                actor.Frontend.Send(new ZMessage(new List<ZFrame>()
                    { new ZFrame("CURVE"), new ZFrame(".curve") }));

                responder.CurvePublicKey = serverCert.PublicKey;
                responder.CurveSecretKey = serverCert.SecretKey;
                responder.CurveServer = true;
                // Bind
                responder.Bind("tcp://*:5555");

                while (true)
                {
                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        Console.WriteLine("Received {0}", request.ReadString());

                        // Do some work
                        Thread.Sleep(1);

                        // Send
                        responder.Send(new ZFrame(name));
                    }
                }
            }
        }

        private static ZCert GetOrCreateCert(string name, string curvpath = ".curve")
        {
            ZCert cert;
            string keyfile = Path.Combine(curvpath, name + ".pub");
            if (!File.Exists(keyfile))
            {
                cert = new ZCert();
                Directory.CreateDirectory(curvpath);
                cert.SetMeta("name", name);
                cert.Save(keyfile);
            }
            else
            {
                cert = ZCert.Load(keyfile);
            }
            return cert;
        }
    }
}
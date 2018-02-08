using NUnit.Framework;
using ZeroMQ;
using ZeroMQ.Devices;

namespace ZeroMQTest
{
    [TestFixture]
    public class ZDeviceTest
    {
        [Test]
        public void Create_Default()
        {
            using (var context = new ZContext())
            {
                using (var device = new RouterDealerDevice(context, "inproc://frontend", "inproc://backend"))
                {
                    device.Start();

                    using (var client = new ZSocket(context, ZSocketType.DEALER))
                    {
                        client.Connect("inproc://frontend");
                        using (var server = new ZSocket(context, ZSocketType.DEALER))
                        {
                            server.Connect("inproc://backend");
                            client.SendMessage(new ZMessage(new[] { new ZFrame(81) }));
                            var msg = server.ReceiveMessage();
                            // TODO what is the first part of the received message? the client identity? but shouldn't it be separated from the rest of the message?
                            Assert.AreEqual(2, msg.Count);
                            Assert.AreEqual(new ZFrame(81), msg[1]);
                        }
                    }

                    device.Stop();
                    device.Join();
                }
            }
        }
    }
}

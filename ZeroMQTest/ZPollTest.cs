using NUnit.Framework;
using System;
using System.Linq;
using ZeroMQ;

namespace ZeroMQTest
{
    [TestFixture]
    public class ZPollTest
    {
        [Test]
        public void PollSingle_Timeout()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    ZMessage message = null;
                    ZError error;
                    Assert.IsFalse(socket.Poll(ZPollItem.CreateReceiver(), ZPoll.In, ref message, out error, TimeSpan.Zero));

                    Assert.IsNull(message);
                    Assert.AreEqual(ZError.EAGAIN, error);
                }
            }
        }

        [Test]
        public void PollInSingle_Timeout()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    ZMessage message;
                    ZError error;
                    Assert.IsFalse(socket.PollIn(ZPollItem.CreateReceiver(), out message, out error, TimeSpan.Zero));

                    Assert.IsNull(message);
                    Assert.AreEqual(ZError.EAGAIN, error);
                }
            }
        }

        [Test, Ignore("TODO: this behaviour must be revised")]
        public void PollMany_Empty()
        {
            var sockets = Enumerable.Empty<ZSocket>();
            var pollItems = Enumerable.Empty<ZPollItem>();

            ZMessage[] messages = null;
            ZError error;
            Assert.IsFalse(sockets.Poll(pollItems, ZPoll.In, ref messages, out error));
        }

        [Test]
        public void PollInMany_Single_Timeout()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    var sockets = new[] { socket };
                    var pollItems = new[] { ZPollItem.CreateReceiver() };

                    ZMessage[] messages;
                    ZError error;
                    Assert.IsFalse(sockets.PollIn(pollItems, out messages, out error, TimeSpan.Zero));

                    CollectionAssert.AreEqual(new ZMessage[] { null }, messages);
                    Assert.AreEqual(ZError.EAGAIN, error);
                }
            }
        }

        [Test]
        public void PollMany_Single_Timeout()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    var sockets = new[] { socket };
                    var pollItems = new[] { ZPollItem.CreateReceiver() };

                    ZMessage[] messages = null;
                    ZError error;
                    Assert.IsFalse(sockets.Poll(pollItems, ZPoll.In, ref messages, out error, TimeSpan.Zero));

                    CollectionAssert.AreEqual(new ZMessage[] { null }, messages);
                    Assert.AreEqual(ZError.EAGAIN, error);
                }
            }
        }

        [Test]
        public void PollMany_Single_Ready()
        {
            using (var context = new ZContext())
            {
                using (var socket1 = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket1.Bind("inproc://foo");
                    using (var socket2 = new ZSocket(context, ZSocketType.PAIR))
                    {
                        socket2.Connect("inproc://foo");
                        socket2.Send(new ZMessage(new[] { new ZFrame(32) }));

                        var sockets = new[] { socket1 };
                        var pollItems = new[] { ZPollItem.CreateReceiver() };

                        ZMessage[] messages = null;
                        ZError error;
                        Assert.IsTrue(sockets.Poll(pollItems, ZPoll.In, ref messages, out error, TimeSpan.Zero));

                        CollectionAssert.AreEqual(new[] { new ZMessage(new[] { new ZFrame(32) }) }, messages);
                        Assert.IsNull(error);
                    }
                }
            }
        }
    }
}

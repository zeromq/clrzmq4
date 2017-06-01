using NUnit.Framework;
using System;
using System.Linq;
using ZeroMQ;

namespace ZeroMQTest
{
    [TestFixture]
    public class ZPollTest
    {
        private const string DefaultEndpoint = "inproc://foo";

        // TODO: polling on a single socket makes (almost) no sense; these overloads should be removed from the library

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

        [Test]
        public void PollOutSingle_Ready()
        {
            using (var context = new ZContext())
            {
                using (var socket1 = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket1.Bind(DefaultEndpoint);
                    using (var socket2 = new ZSocket(context, ZSocketType.PAIR))
                    {
                        socket2.Connect(DefaultEndpoint);

                        ZError error;
                        Assert.IsTrue(socket1.PollOut(ZPollItem.CreateSender(), new ZMessage(new[] { new ZFrame(32) }), out error, TimeSpan.Zero));
                        Assert.IsNull(error);

                        var message = socket2.ReceiveMessage();
                        Assert.AreEqual(new ZMessage(new[] { new ZFrame(32) }), message);
                    }
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
        public void PollMany_Single_In_Ready()
        {
            using (var context = new ZContext())
            {
                using (var socket1 = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket1.Bind(DefaultEndpoint);
                    using (var socket2 = new ZSocket(context, ZSocketType.PAIR))
                    {
                        socket2.Connect(DefaultEndpoint);
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

        [Test]
        public void PollMany_Single_Out_Ready()
        {
            using (var context = new ZContext())
            {
                using (var socket1 = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket1.Bind(DefaultEndpoint);
                    using (var socket2 = new ZSocket(context, ZSocketType.PAIR))
                    {
                        socket2.Connect(DefaultEndpoint);

                        var sockets = new[] { socket1 };
                        var pollItems = new[] { ZPollItem.CreateSender() }; // TODO: erroneously using CreateReceiver here causes a SEHException below. Shouldn't this be caught somehow?

                        ZMessage[] messages = new[] { new ZMessage(new[] { new ZFrame(32) }) };
                        ZError error;
                        Assert.IsTrue(sockets.Poll(pollItems, ZPoll.Out, ref messages, out error, TimeSpan.Zero));
                        Assert.IsNull(error);

                        var message = socket2.ReceiveMessage();
                        Assert.AreEqual(new ZMessage(new[] { new ZFrame(32) }), message);
                    }
                }
            }
        }

        [Test]
        public void PollOutMany_Single_Ready()
        {
            using (var context = new ZContext())
            {
                using (var socket1 = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket1.Bind(DefaultEndpoint);
                    using (var socket2 = new ZSocket(context, ZSocketType.PAIR))
                    {
                        socket2.Connect(DefaultEndpoint);

                        var sockets = new[] { socket1 };
                        var pollItems = new[] { ZPollItem.CreateSender() }; // TODO: erroneously using CreateReceiver here causes a SEHException below. Shouldn't this be caught somehow?

                        ZMessage[] messages = new[] { new ZMessage(new[] { new ZFrame(32) }) };
                        ZError error;
                        Assert.IsTrue(sockets.PollOut(pollItems, messages, out error, TimeSpan.Zero));
                        Assert.IsNull(error);

                        var message = socket2.ReceiveMessage();
                        Assert.AreEqual(new ZMessage(new[] { new ZFrame(32) }), message);
                    }
                }
            }
        }

        [Test, Ignore("The interface and/or behaviour must probably be revised")]
        public void PollMany_Multiple_InOut()
        {
            using (var context = new ZContext())
            {
                using (var socket1 = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket1.Bind(DefaultEndpoint);
                    using (var socket2 = new ZSocket(context, ZSocketType.PAIR))
                    {
                        socket2.Connect(DefaultEndpoint);

                        var sockets = new[] { socket1, socket2 };
                        var pollItems = new[] { ZPollItem.CreateSender(), ZPollItem.CreateReceiver() };

                        ZMessage[] messages = new[] { new ZMessage(new[] { new ZFrame(32) }), null };
                        ZError error;

                        // TODO: why does this return false?
                        Assert.IsTrue(sockets.Poll(pollItems, ZPoll.In | ZPoll.Out, ref messages, out error));
                        Assert.IsNull(error);

                        if (messages[1] == null)
                        {
                            Assert.IsTrue(sockets.Poll(pollItems, ZPoll.In | ZPoll.Out, ref messages, out error));
                            Assert.IsNull(error);
                        }

                        CollectionAssert.AreEqual(new[] { null, new ZMessage(new[] { new ZFrame(32) }) }, messages);
                    }
                }
            }
        }
    }
}

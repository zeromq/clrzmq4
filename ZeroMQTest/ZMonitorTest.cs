﻿using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZeroMQ;
using ZeroMQ.Monitoring;

namespace ZeroMQTest
{
    [TestFixture]
    public class ZMonitorTest
    {
        private const string MonitorSocket = "inproc://test";

        [Test, Ignore("Requires a fix in ZThread")]
        public void Create()
        {
            using (var context = new ZContext())
            {
                using (ZMonitor.Create(context, MonitorSocket)) {}
            }
        }

        [Test]
        public void Start()
        {
            using (var context = new ZContext())
            {
                using (var monitor = ZMonitor.Create(context, MonitorSocket))
                {
                    monitor.Start();

                    // TODO: the following is necessary since Dispose is not properly implemented
                    monitor.Close();
                    monitor.Join();
                }
            }
        }

        [Test]
        public void AllEvents()
        {
            using (var context = new ZContext())
            {
                using (var socket = ZSocket.Create(context, ZSocketType.PAIR))
                {
                    Assert.IsTrue(socket.Monitor(MonitorSocket));
                    using (var monitor = ZMonitor.Create(context, MonitorSocket))
                    {
                        var events = new ConcurrentQueue<Tuple<object, ZMonitorEventArgs>>();
                        monitor.AllEvents += (sender, args) => events.Enqueue(Tuple.Create(sender, args));

                        monitor.Start();

                        using (var socket2 = ZSocket.Create(context, ZSocketType.PAIR))
                        {
                            socket2.Bind("inproc://foo");
                            socket.Connect("inproc://foo");
                            socket.Close();
                        }

                        Assert.That(() => events.Count >= 1, Is.True.After(1).Minutes.PollEvery(100).MilliSeconds);

                        // TODO: the following is necessary since Dispose is not properly implemented
                        monitor.Close();
                        monitor.Join();

                        // TODO: assert that the appropriate event(s) are received
                        CollectionAssert.IsNotEmpty(events);
                    }
                }
            }
        }
    }
}
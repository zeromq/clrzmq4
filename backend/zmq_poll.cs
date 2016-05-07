using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroMQ
{
    public static partial class backend
    {
        static ZPollItem make_pi(ZPoll f)
        {
            switch (f)
            {
                case ZPoll.In:
                    return ZPollItem.CreateReceiver();
                case ZPoll.Out:
                    return ZPollItem.CreateSender();
                case ZPoll.In | ZPoll.Out:
                    return ZPollItem.CreateReceiverSender();
            }
            throw new System.ArgumentException("Poll flag must be valid ZPoll combination");
        }
        public static object zmq_poll(IEnumerable<dynamic> socks, int timeout)
        {
            var s2 = (from p in socks select (ZSocket)p[0]._ZSocket).ToList();
            var i2 = (from p in socks select make_pi((ZPoll)p[1])).ToList();
            ZError err;
            TimeSpan? ts = null;
            if (timeout >= 0)
                ts = TimeSpan.FromMilliseconds(timeout);
            ZPollItems.PollMany(s2, i2, ZPoll.In|ZPoll.Out, out err, ts);
            return Enumerable.Zip(socks, i2, (p, i) => new object[] { p[0], (int)i.ReadyEvents }).Where(p => (int)p[1] != (int)ZPoll.None).ToList();
        }
    }
}

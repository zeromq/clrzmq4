﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroMQ
{
    public static partial class backend
    {
        public class Socket
        {
            internal readonly ZSocket _ZSocket;
            internal Socket(ZSocket s)
            {
                _ZSocket = s;
            }
            public Socket(Context ctx, ZSocketType st)
            {
                _ZSocket = ctx.zsocket(st);
            }
            public Socket()
            {
                throw new NotImplementedException("ZeroMQ.backend Socket default ctor should not be used");
                // required to exist for python to subclass it.
                // subclass instantiation seems to call default ctor first, then __init__?
            }
            public void set(ZSocketOption so, IEnumerable<Byte> buf)
            {
                _ZSocket.SetOption(so, buf.ToArray());
            }
            public object get(ZSocketOption so)
            {
                switch (so)
                {
                    case ZSocketOption.RCVMORE:
                        return _ZSocket.GetOptionInt32(so);
                    default:
                        return _ZSocket.GetOptionBytes(so);
                }
            }

            bool _closed = false;
            public bool closed { get { return _closed || _ZSocket.SocketPtr == null; } }

            public void close()
            {
                _ZSocket.Close();
                _closed = true;
            }

            public int linger
            {
                get { return _ZSocket.GetOptionInt32(ZSocketOption.LINGER); }
                set { _ZSocket.SetOption(ZSocketOption.LINGER, value);  }
            }

            public IEnumerable<Byte> identity
            {
                get { return Encoding.UTF8.GetBytes(_ZSocket.IdentityString); }
                set { _ZSocket.IdentityString = Encoding.UTF8.GetString(value.ToArray()); }
            }

            public void connect(string url)
            {
                _ZSocket.Connect(url);
            }

            public void send(ZFrame msg, int flags = 0, bool copy = false, bool track = false)
            {
                if (copy)
                    msg = msg.Duplicate();
                _ZSocket.SendFrame(msg, (ZSocketFlags)flags);
                if (track)
                    throw new NotImplementedException("tracking not yet wired up");
            }

            public void send(string msg, int flags = 0, bool copy = false, bool track = false)
            {
                send(Encoding.UTF8.GetBytes(msg), flags, copy, track);
            }
            public void send(IEnumerable<Byte> msg, int flags = 0, bool copy = false, bool track = false)
            {
                 var frm = new ZFrame(msg.ToArray());
                _ZSocket.SendFrame(frm, (ZSocketFlags)flags);
                if (track)
                    throw new NotImplementedException("tracking not yet wired up");
            }

            public object recv(int flags = 0, bool copy = false, bool track = false)
            {
                ZError err;
                var zf = _ZSocket.ReceiveFrame((ZSocketFlags)flags, out err);
                if (err != ZError.None)
                    throw new ZException(err);

                if (copy)
                    return new IronPython.Runtime.Bytes(zf.Read());
                else
                    return new Frame(zf);
            }
        }
    }
}

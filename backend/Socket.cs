using System;
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
            //public Socket(Context ctx, ZSocketType st)
            //{
            //    _ZSocket = ctx.socket(st);
            //}
            public Socket()
            {
                throw new NotImplementedException("ZeroMQ.backend Socket default ctor should not be used");
                // required to exist for python to subclass it.
            }
            public void set(ZSocketOption so, IEnumerable<Byte> buf)
            {
                _ZSocket.SetOption(so, buf.ToArray());
            }
            public IEnumerable<Byte> get(ZSocketOption so)
            {
                return _ZSocket.GetOptionBytes(so);
            }

            public bool closed { get { return _ZSocket.SocketPtr == null; } }
        }
    }
}

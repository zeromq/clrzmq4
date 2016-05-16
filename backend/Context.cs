using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroMQ
{
    public static partial class backend
    {
        public class Context
        {
            internal readonly ZContext _ZContext;
            internal Context(ZContext zc)
            {
                if (zc == null)
                    zc = ZContext.Create();
                _ZContext = zc;
            }
            public Context(int io_threads = 1)
            {
                var zc = ZContext.Create();
                zc.ThreadPoolSize = io_threads;
                _ZContext = zc;
            }
            public bool closed { get { return _ZContext == null || _ZContext.ContextPtr == IntPtr.Zero; } }
            public Socket socket(ZSocketType st)
            {
                return new Socket(zsocket(st));
            }
            internal ZSocket zsocket(ZSocketType st)
            {
                return new ZSocket(_ZContext, st);
            }
        }
    }
}
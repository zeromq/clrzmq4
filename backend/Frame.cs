using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroMQ
{
    public static partial class backend
    {
        public class Frame
        {
            public readonly ZFrame _ZFrame;
            internal Frame(ZFrame f)
            {
                _ZFrame = f;
            }
            public Frame(IEnumerable<Byte> data, bool track = false)
            {
                _ZFrame = new ZFrame(data.ToArray());
                if (track)
                    throw new NotImplementedException("Frame in clrzmq4.backend with track=true");
            }

            public Byte[] bytes
            {
                get
                {
                    return _ZFrame.Read();
                }
            }
        }
    }
}

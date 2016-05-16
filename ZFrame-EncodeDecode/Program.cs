using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZeroMQ;

namespace ZFrame_EncodeDecode
{
    internal class Program
    {
        #region Private Methods

        private static void Main(string[] args)
        {
            const string text = "abc-123-äöü-#+*-ß?=-ÄÖÜ-ABC";
            using (ZFrame frame = new ZFrame(text))
            {
                var decoded = frame.ToString();

                Debug.Assert(text == decoded);
            }
        }

        #endregion Private Methods
    }
}
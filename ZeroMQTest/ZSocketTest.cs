using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ZeroMQ;

namespace ZeroMQTest
{
    [TestFixture]
    public class ZSocketTest
    {
        static IEnumerable<ZSocketType> ValidSocketTypes { get { return Enum.GetValues(typeof(ZSocketType)).Cast<ZSocketType>().Except(new[] { ZSocketType.None }); } }

        [Test, TestCaseSource(nameof(ValidSocketTypes))]
        public void Create_WithContext(ZSocketType socketType)
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, socketType))
                {
                }
            }
        }

        [Test]
        public void Create_WithContext_None_Fails()
        {
            using (var context = new ZContext())
            {
                // TODO: Should this be converted to ArgumentException?
                Assert.Throws<ZException>(() => new ZSocket(context, ZSocketType.None));
            }
        }
    }
}

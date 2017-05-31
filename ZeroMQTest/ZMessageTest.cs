using NUnit.Framework;
using ZeroMQ;

namespace ZeroMQTest
{
    [TestFixture]
    public class ZMessageTest
    {
        [Test]
        public void Create_Default()
        {
            using (var msg = new ZMessage())
            {
                Assert.AreEqual(0, msg.Count);
            }
        }

        [Test]
        public void Create_FromFrames_Empty()
        {
            using (var msg = new ZMessage(new ZFrame[] { }))
            {
                Assert.AreEqual(0, msg.Count);
            }
        }

        [Test]
        public void Create_FromFrames_NonEmpty()
        {
            using (var msg = new ZMessage(new ZFrame[] { new ZFrame() }))
            {
                Assert.AreEqual(1, msg.Count);
            }
        }
    }
}

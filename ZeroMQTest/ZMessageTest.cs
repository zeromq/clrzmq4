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

        [Test]
        public void Pop()
        {
            const string test1 = "__TEST1__", test2 = "__TEST2__";
            var msg = new ZMessage { new ZFrame(test1), new ZFrame(test2) };
            var frame = msg.Pop();
            Assert.AreEqual(0, frame.Position);
            Assert.AreEqual(new ZFrame(test1), frame);
        }

        [Test]
        public void PopString()
        {
            const string test = "__TEST__";
            var msg = new ZMessage { new ZFrame(test) };
            Assert.AreEqual(test, msg.PopString());
        }
    }
}

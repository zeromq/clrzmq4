using NUnit.Framework;
using System.IO;
using ZeroMQ;

namespace ZeroMQTest
{
    [TestFixture]
    public class ZFrameTest
    {
        [Test]
        public void Create_Default()
        {
            using (var msg = new ZFrame())
            {
                Assert.AreEqual(0, msg.Length);
            }
        }

        [Test]
        public void Create_Byte()
        {
            using (var msg = new ZFrame((byte)42))
            {
                AssertAfterCreate(new byte[] { 42 }, msg);
            }
        }

        [Test]
        public void Create_Char()
        {
            using (var msg = new ZFrame((char)42))
            {
                AssertAfterCreate(new byte[] { 42, 0 }, msg);
            }
        }

        [Test]
        public void Create_Int()
        {
            using (var msg = new ZFrame((int)-1))
            {
                AssertAfterCreate(new byte[] { 255, 255, 255, 255 }, msg);
            }
        }

        [Test]
        public void Create_Uint()
        {
            using (var msg = new ZFrame((uint)42))
            {
                AssertAfterCreate(new byte[] { 42, 0, 0, 0 }, msg);
            }
        }

        [Test]
        public void Create_Long()
        {
            using (var msg = new ZFrame((long)-1))
            {
                AssertAfterCreate(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, msg);
            }
        }

        [Test]
        public void Create_Ulong()
        {
            using (var msg = new ZFrame((ulong)42))
            {
                AssertAfterCreate(new byte[] { 42, 0, 0, 0, 0, 0, 0, 0 }, msg);
            }
        }

        [Test]
        public void Create_String()
        {
            using (var msg = new ZFrame("abc"))
            {
                AssertAfterCreate(new byte[] { (byte)'a', (byte)'b', (byte)'c' }, msg);
            }
        }

        private static void AssertAfterCreate(byte[] expected, ZFrame msg)
        {
            Assert.AreEqual(expected.Length, msg.Length);
            Assert.AreEqual(msg.Length, msg.Position);

            msg.Seek(0, SeekOrigin.Begin);
            var bytes = msg.Read();
            CollectionAssert.AreEqual(expected, bytes);
        }
    }
}

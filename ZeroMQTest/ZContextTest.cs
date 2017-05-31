using NUnit.Framework;
using ZeroMQ;

namespace ZeroMQTest
{
    [TestFixture]
    public class ZContextTest
    {
        [Test]
        public void Create_ViaFactoryMethod()
        {
            using (var context = ZContext.Create())
            {
                Assert.IsNotNull(context);
            }
        }

        [Test]
        public void Create_ViaCtor()
        {
            using (var context = new ZContext())
            {
                Assert.IsNotNull(context);
            }
        }

        [Test]
        public void StaticInstance()
        {
            // TODO: Is it a good idea to supply a static instance in the library?
            Assert.IsNotNull(ZContext.Current);
        }

        [Test]
        public void Terminate()
        {
            using (var context = new ZContext())
            {
                context.Terminate();
            }
        }

        [Test]
        public void Terminate_Twice()
        {
            using (var context = new ZContext())
            {
                context.Terminate();
                context.Terminate();
            }
        }

        [Test]
        public void Shutdown()
        {
            using (var context = new ZContext())
            {
                context.Shutdown();
            }
        }

        [Test]
        public void Shutdown_Twice()
        {
            using (var context = new ZContext())
            {
                context.Shutdown();
                context.Shutdown();
            }
        }
    }
}

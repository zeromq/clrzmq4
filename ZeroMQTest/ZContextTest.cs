using NUnit.Framework;
using System;
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
        public void Static_Instance()
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

        [Test]
        public void ContextPtr()
        {
            // TODO: Make this property internal?
            using (var context = new ZContext())
            {
                Assert.IsNotNull(context.ContextPtr);
            }
        }

        [Test]
        public void Static_Has()
        {
            // we should not assert on the result of these capabilities, since different builds of 
            // the libzmq dynamic library should be allowed
            Console.WriteLine("Loaded libzmq dynamic library has the following capabilities:");
            Console.WriteLine(string.Format("ipc: {0}", ZContext.Has("ipc")));
            Console.WriteLine(string.Format("pgm: {0}", ZContext.Has("pgm")));
            Console.WriteLine(string.Format("tipc: {0}", ZContext.Has("tipc")));
            Console.WriteLine(string.Format("norm: {0}", ZContext.Has("norm")));
            Console.WriteLine(string.Format("curve: {0}", ZContext.Has("curve")));
            Console.WriteLine(string.Format("gssapi: {0}", ZContext.Has("gssapi")));
            Console.WriteLine(string.Format("draft: {0}", ZContext.Has("draft")));
        }

        [Test]
        public void GetOption_ThreadPoolSize_Default()
        {
            using (var context = new ZContext())
            {
                Assert.AreEqual(1, context.ThreadPoolSize);
            }
        }

        [Test]
        public void GetOption_MaxSockets_Default()
        {
            using (var context = new ZContext())
            {
                // TODO: according to the libzmq documentation, the default should be 1024 rather than 1023
                Assert.AreEqual(1023, context.MaxSockets);
            }
        }

        [Test]
        public void GetOption_IPv6Enabled_Default()
        {
            using (var context = new ZContext())
            {
                Assert.AreEqual(false, context.IPv6Enabled);
            }
        }

        [Test]
        public void SetOption_ThreadPoolSize()
        {
            using (var context = new ZContext())
            {
                context.ThreadPoolSize = 2;
                Assert.AreEqual(2, context.ThreadPoolSize);
            }
        }

        [Test]
        public void SetOption_MaxSockets()
        {
            using (var context = new ZContext())
            {
                context.MaxSockets = 1;
                Assert.AreEqual(1, context.MaxSockets);
            }
        }

        [Test]
        public void SetOption_IPv6Enabled()
        {
            using (var context = new ZContext())
            {
                context.IPv6Enabled = true;
                Assert.AreEqual(true, context.IPv6Enabled);
            }
        }

        [Test]
        public void SetOption_IPV6Enabled_Invalid_Fails()
        {
            using (var context = new ZContext())
            {
                // TODO: the exception message is misleading, as the option name is correct, but the value is invalid
                Assert.Throws<ArgumentOutOfRangeException>(() => context.SetOption(ZContextOption.IPV6, -1));
            }
        }
    }
}

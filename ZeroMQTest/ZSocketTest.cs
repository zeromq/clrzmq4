using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroMQ;
using ZeroMQ.lib;

namespace ZeroMQTest
{
    [TestFixture]
    public class ZSocketTest
    {
        private const string DefaultAddress = "inproc://foo";
        private const string InvalidAddress = "__BAD_ADDRESS__";

        static IEnumerable<ZSocketType> ValidSocketTypes { get { return Enum.GetValues(typeof(ZSocketType)).Cast<ZSocketType>().Except(new[] { ZSocketType.None }); } }

        #region Create
        [Test, TestCaseSource(nameof(ValidSocketTypes))]
        public void Create_WithContext(ZSocketType socketType)
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, socketType))
                {
                    Assert.AreSame(context, socket.Context);
                    Assert.IsNotNull(socket.SocketPtr);
                    Assert.AreEqual(socketType, socket.SocketType);
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
        #endregion

        #region Close
        [Test, TestCaseSource(nameof(ValidSocketTypes))]
        public void Close(ZSocketType socketType)
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, socketType))
                {
                    socket.Close();
                }
            }
        }

        /// <summary>
        /// TODO: this behaviour is deviating from that of libzmq and it may mask erroneous uses. an exception should probably be thrown
        /// </summary>
        /// <param name="socketType"></param>
        [Test, TestCaseSource(nameof(ValidSocketTypes))]
        public void Close_Twice(ZSocketType socketType)
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, socketType))
                {
                    socket.Close();
                    socket.Close();
                }
            }
        }
        #endregion

        // TODO The socket options test only test whether the options can be read or set and read, 
        // not if they have the desired effect. This would need to be tested separately.
        #region socket options
        [Test]
        public void GetOption_Affinity()
        {
            DoWithUnconnectedPairSocket(socket => Assert.AreEqual(0, socket.Affinity));
        }

        [Test]
        public void SetOption_Affinity()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.Affinity = 1;
                Assert.AreEqual(1, socket.Affinity);
            }
            );
        }

        [Test]
        public void GetOption_Backlog()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(100, socket.Backlog);
            });
            
        }

        [Test]
        public void SetOption_Backlog()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.Backlog = 0;
                Assert.AreEqual(0, socket.Backlog);
            });            
        }

        [Test]
        public void GetOption_Conflate()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(false, socket.Conflate);
            });           
        }

        [Test]
        public void GetOption_ConnectRID()
        {
            DoWithUnconnectedRouterSocket(socket =>
            {
                // TODO: Probably this socket option cannot be queried. The property getter should be removed.
                Assert.Throws<ZException>(() => { var x = socket.ConnectRID; });
            });
        }

        [Test]
        public void SetOption_ConnectRID()
        {
            DoWithUnconnectedRouterSocket(socket =>
                {
                    socket.ConnectRID = new byte[] { 1, 2, 3, 4 };
                });
        }

        [Test, Ignore("Issue in underlying libzmq fixed in commit https://github.com/zeromq/libzmq/commit/f86795350d2c37753b961018b5185cd1af33a38a")]
        public void GetOption_CurvePublicKey()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                CollectionAssert.AreEqual(Enumerable.Repeat<byte>(0, ZSocket.BinaryKeySize).ToArray(), socket.CurvePublicKey);
            });            
        }

        [Test, Ignore("Issue in underlying libzmq fixed in commit https://github.com/zeromq/libzmq/commit/f86795350d2c37753b961018b5185cd1af33a38a")]
        public void GetOption_CurveSecretKey()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                CollectionAssert.AreEqual(Enumerable.Repeat<byte>(0, ZSocket.BinaryKeySize).ToArray(), socket.CurveSecretKey);
            });            
        }

        [Test, Ignore("Issue in underlying libzmq fixed in commit https://github.com/zeromq/libzmq/commit/f86795350d2c37753b961018b5185cd1af33a38a")]
        public void GetOption_CurveServerKey()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                CollectionAssert.AreEqual(Enumerable.Repeat<byte>(0, ZSocket.BinaryKeySize).ToArray(), socket.CurveServerKey);
            });            
        }

        [Test]
        public void SetOption_CurvePublicKey()
        {
            if (!ZContext.Has("curve")) Assert.Ignore("Ignored due to missing curve support");
            Assume.Equals(PlatformKind.Win32, Platform.Kind); // TODO: fix the test and remove this

            DoWithUnconnectedPairSocket(socket =>
            {
                // TODO: the interface is confusing; Z85-encoded keys should always be strings
                byte[] publicKeyZ85, secretKeyZ85;
                Z85.CurveKeypair(out publicKeyZ85, out secretKeyZ85);
                socket.CurvePublicKey = publicKeyZ85;
                byte[] publicKeyBinary = Z85.Decode(publicKeyZ85);
                CollectionAssert.AreEqual(publicKeyBinary, socket.CurvePublicKey);
            });            
        }

        [Test]
        public void SetOption_CurveServerKey()
        {
            if (!ZContext.Has("curve")) Assert.Ignore("Ignored due to missing curve support");
            Assume.Equals(PlatformKind.Win32, Platform.Kind); // TODO: fix the test and remove this

            DoWithUnconnectedPairSocket(socket =>
            {
                // TODO: the interface is confusing; Z85-encoded keys should always be strings
                byte[] publicKeyZ85, secretKeyZ85;
                Z85.CurveKeypair(out publicKeyZ85, out secretKeyZ85);
                socket.CurveServerKey = publicKeyZ85;
                byte[] publicKeyBinary = Z85.Decode(publicKeyZ85);
                CollectionAssert.AreEqual(publicKeyBinary, socket.CurveServerKey);
            });            
        }

        [Test]
        public void SetOption_CurveSecretKey()
        {
            if (!ZContext.Has("curve")) Assert.Ignore("Ignored due to missing curve support");
            Assume.Equals(PlatformKind.Win32, Platform.Kind); // TODO: fix the test and remove this

            DoWithUnconnectedPairSocket(socket =>
            {
                // TODO: the interface is confusing; Z85-encoded keys should always be strings
                byte[] publicKeyZ85, secretKeyZ85;
                Z85.CurveKeypair(out publicKeyZ85, out secretKeyZ85);
                socket.CurveSecretKey = secretKeyZ85;
                byte[] secretKeyBinary = Z85.Decode(secretKeyZ85);
                CollectionAssert.AreEqual(secretKeyBinary, socket.CurveSecretKey);
            });            
        }

        [Test]
        public void GetOption_GSSAPIPlainText()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                if (!ZContext.Has("gssapi"))
                {
                    Assert.Throws<ZException>(() => { var res = socket.GSSAPIPlainText; });
                }
                else
                {
                    Assert.AreEqual(false, socket.GSSAPIPlainText);
                }
            });            
        }

        [Test]
        public void SetOption_GSSAPIPlainText()
        {
            if (!ZContext.Has("gssapi")) Assert.Ignore("libzmq does not support gssapi");

            DoWithUnconnectedPairSocket(socket =>
            {
                socket.GSSAPIPlainText = true;
                Assert.AreEqual(true, socket.GSSAPIPlainText);
            });            
        }

        [Test]
        public void GetOption_GSSAPIPrincipal()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                if (!ZContext.Has("gssapi"))
                {
                    Assert.Throws<ZException>(() => { var res = socket.GSSAPIPrincipal; });
                }
                else
                {
                    Assert.IsEmpty(socket.GSSAPIPrincipal);
                }
            });           
        }

        [Test]
        public void SetOption_GSSAPIPrincipal()
        {
            if (!ZContext.Has("gssapi")) Assert.Ignore("libzmq does not support gssapi");

            DoWithUnconnectedPairSocket(socket =>
            {
                socket.GSSAPIPrincipal = "foo";
                Assert.AreEqual("foo", socket.GSSAPIPrincipal);
            });            
        }

        [Test]
        public void GetOption_HandshakeInterval()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(30000, socket.HandshakeInterval);
            });
        }

        [Test]
        public void SetOption_HandshakeInterval()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.HandshakeInterval = 0;
                Assert.AreEqual(0, socket.HandshakeInterval);
            });
        }

        [Test]
        public void SetOption_Identity()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.Identity = new byte[] { 42 };
                Assert.AreEqual(new byte[] { 42 }, socket.Identity);
            });
        }

        [Test]
        public void GetOption_Identity()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(new byte[] { }, socket.Identity);
            });

        }

        [Test]
        public void SetOption_IdentityString()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.IdentityString = "abc";
                Assert.AreEqual(Encoding.ASCII.GetBytes("abc"), socket.Identity);
            });
        }

        [Test]
        public void GetOption_IdentityString()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual("", socket.IdentityString);
            });

        }

        [Test]
        public void GetOption_Immediate()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(false, socket.Immediate);
            });
        }

        [Test]
        public void SetOption_Immediate()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.Immediate = true;
                Assert.AreEqual(true, socket.Immediate);
            });
        }

        [Test]
        public void GetOption_IPv6()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(false, socket.IPv6);
            });
        }

        [Test]
        public void SetOption_IPv6()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.IPv6 = true;
                Assert.AreEqual(true, socket.IPv6);
            });
        }

        [Test]
        public void GetOption_Linger()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                // TODO Use TimeSpan? as the type of Linger instead, to better encode "infinity"?
                Assert.AreEqual(TimeSpan.FromMilliseconds(-1), socket.Linger);
            });
        }

        [Test]
        public void SetOption_Linger()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.Linger = TimeSpan.FromMilliseconds(50);
                Assert.AreEqual(TimeSpan.FromMilliseconds(50), socket.Linger);
            });
        }

        [Test]
        public void GetOption_MaxMessageSize()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                // TODO use int? as the type of MaxMessageSize to better encode infinity?
                Assert.AreEqual(-1, socket.MaxMessageSize);
            });
        }

        [Test]
        public void SetOption_MaxMessageSize()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.MaxMessageSize = 0;
                Assert.AreEqual(0, socket.MaxMessageSize);
            });
        }

        [Test]
        public void GetOption_MulticastHops()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(1, socket.MulticastHops);
            });
        }

        [Test, Ignore("0 seems to be an invalid value but it is ignored")]
        public void SetOption_MulticastHops_Invalid()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.MulticastHops = 0;
                Assert.AreEqual(0, socket.MulticastHops);
            });
        }

        [Test]
        public void SetOption_MulticastHops()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.MulticastHops = 2;
                Assert.AreEqual(2, socket.MulticastHops);
            });
        }

        [Test]
        public void GetOption_ProbeRouter_InvalidSocketType_Fails()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                var exc = Assert.Throws<ZException>(() => { var res = socket.ProbeRouter; });
                Assert.AreEqual(ZError.EINVAL, exc.Error);
            });
        }

        [Test, Ignore("Investigate why this fails with EINVAL")]
        public void GetOption_ProbeRouter()
        {
            DoWithUnconnectedRouterSocket(socket =>
            {
                Assert.AreEqual(false, socket.ProbeRouter);
            });
        }

        [Test, Ignore("Investigate why this fails with EINVAL")]
        public void SetOption_ProbeRouter()
        {
            DoWithUnconnectedRouterSocket(socket =>
            {
                socket.Connect("inproc://foo");
                socket.ProbeRouter = true;
                Assert.AreEqual(true, socket.ProbeRouter);
            });
        }

        [Test]
        public void GetOption_MulticastRate()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(100, socket.MulticastRate);
            });
        }

        [Test]
        public void SetOption_MulticastRate()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.MulticastRate = 50;
                Assert.AreEqual(50, socket.MulticastRate);
            });
        }

        [Test]
        public void GetOption_MulticastRecoveryInterval()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(TimeSpan.FromSeconds(10), socket.MulticastRecoveryInterval);
            });
        }

        [Test]
        public void SetOption_MulticastRecoveryInterval()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.MulticastRecoveryInterval = TimeSpan.FromMilliseconds(50);
                Assert.AreEqual(TimeSpan.FromMilliseconds(50), socket.MulticastRecoveryInterval);
            });
        }

        [Test]
        public void GetOption_ReconnectInterval()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(TimeSpan.FromMilliseconds(100), socket.ReconnectInterval);
            });
        }

        [Test]
        public void SetOption_ReconnectInterval()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.ReconnectInterval = TimeSpan.FromMilliseconds(50);
                Assert.AreEqual(TimeSpan.FromMilliseconds(50), socket.ReconnectInterval);
            });
        }

        [Test]
        public void GetOption_ReconnectIntervalMax()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                // TODO Use TimeSpan? as the type of ReconnectIntervalMax instead, to better encode "none"?
                Assert.AreEqual(TimeSpan.Zero, socket.ReconnectIntervalMax);
            });
        }

        [Test]
        public void SetOption_ReconnectIntervalMax()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.ReconnectIntervalMax = TimeSpan.FromMilliseconds(50);
                Assert.AreEqual(TimeSpan.FromMilliseconds(50), socket.ReconnectIntervalMax);
            });
        }

        [Test]
        public void GetOption_ReceiveBufferSize()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(0, socket.ReceiveBufferSize);
            });
        }

        [Test]
        public void SetOption_ReceiveBufferSize()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.ReceiveBufferSize = 50;
                Assert.AreEqual(50, socket.ReceiveBufferSize);
            });
        }

        [Test]
        public void GetOption_ReceiveHighWatermark()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(1000, socket.ReceiveHighWatermark);
            });
        }

        [Test]
        public void SetOption_ReceiveHighWatermark()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.ReceiveHighWatermark = 50;
                Assert.AreEqual(50, socket.ReceiveHighWatermark);
            });
        }

        [Test]
        public void GetOption_ReceiveTimeout()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                // TODO Use TimeSpan? as the type of ReceiveTimeout instead, to better encode "infinity"?
                Assert.AreEqual(TimeSpan.FromMilliseconds(-1), socket.ReceiveTimeout);
            });
        }

        [Test]
        public void SetOption_ReceiveTimeout()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.ReceiveTimeout = TimeSpan.FromMilliseconds(50);
                Assert.AreEqual(TimeSpan.FromMilliseconds(50), socket.ReceiveTimeout);
            });
        }

        [Test]
        public void GetOption_SendBufferSize()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(0, socket.SendBufferSize);
            });
        }

        [Test]
        public void SetOption_SendBufferSize()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.SendBufferSize = 50;
                Assert.AreEqual(50, socket.SendBufferSize);
            });
        }

        [Test]
        public void GetOption_SendHighWatermark()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(1000, socket.SendHighWatermark);
            });
        }

        [Test]
        public void SetOption_SendHighWatermark()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.SendHighWatermark = 50;
                Assert.AreEqual(50, socket.SendHighWatermark);
            });
        }

        [Test]
        public void GetOption_SendTimeout()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                // TODO Use TimeSpan? as the type of SendTimeout instead, to better encode "infinity"?
                Assert.AreEqual(TimeSpan.FromMilliseconds(-1), socket.SendTimeout);
            });
        }

        [Test]
        public void SetOption_SendTimeout()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.SendTimeout = TimeSpan.FromMilliseconds(50);
                Assert.AreEqual(TimeSpan.FromMilliseconds(50), socket.SendTimeout);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void GetOption_RequestCorrelate()
        {
            DoWithUnconnectedReqSocket(socket =>
            {
                Assert.AreEqual(false, socket.RequestCorrelate);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void SetOption_RequestCorrelate()
        {
            DoWithUnconnectedReqSocket(socket =>
            {
                socket.RequestCorrelate = true;
                Assert.AreEqual(true, socket.RequestCorrelate);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void GetOption_RequestRelaxed()
        {
            DoWithUnconnectedReqSocket(socket =>
            {
                Assert.AreEqual(false, socket.RequestRelaxed);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void SetOption_RequestRelaxed()
        {
            DoWithUnconnectedReqSocket(socket =>
            {
                socket.RequestRelaxed = true;
                Assert.AreEqual(true, socket.RequestRelaxed);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void GetOption_RouterHandover()
        {
            DoWithUnconnectedRouterSocket(socket =>
            {
                Assert.AreEqual(false, socket.RouterHandover);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void SetOption_RouterHandover()
        {
            DoWithUnconnectedRouterSocket(socket =>
            {
                socket.RouterHandover = true;
                Assert.AreEqual(true, socket.RouterHandover);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void GetOption_RouterRaw()
        {
            DoWithUnconnectedRouterSocket(socket =>
            {
                Assert.AreEqual(false, socket.RouterRaw);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void SetOption_RouterRaw()
        {
            DoWithUnconnectedRouterSocket(socket =>
            {
                socket.RouterRaw = true;
                Assert.AreEqual(true, socket.RouterRaw);
            });
        }

        [Test]
        public void GetOption_TcpKeepAlive()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(TcpKeepaliveBehaviour.Default, socket.TcpKeepAlive);
            });
        }

        [Test]
        public void SetOption_TcpKeepAlive()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.TcpKeepAlive = TcpKeepaliveBehaviour.Enable;
                Assert.AreEqual(TcpKeepaliveBehaviour.Enable, socket.TcpKeepAlive);
            });
        }

        [Test]
        public void GetOption_TcpKeepAliveCount()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(-1, socket.TcpKeepAliveCount);
            });
        }

        [Test]
        public void SetOption_TcpKeepAliveCount()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.TcpKeepAliveCount = 1;
                Assert.AreEqual(1, socket.TcpKeepAliveCount);
            });
        }

        [Test]
        public void GetOption_TcpKeepAliveIdle()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(-1, socket.TcpKeepAliveIdle);
            });
        }

        [Test]
        public void SetOption_TcpKeepAliveIdle()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.TcpKeepAliveIdle = 1;
                Assert.AreEqual(1, socket.TcpKeepAliveIdle);
            });
        }

        [Test]
        public void GetOption_TcpKeepAliveInterval()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(-1, socket.TcpKeepAliveInterval);
            });
        }

        [Test]
        public void SetOption_TcpKeepAliveInterval()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.TcpKeepAliveInterval = 1;
                Assert.AreEqual(1, socket.TcpKeepAliveInterval);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void GetOption_RouterMandatory()
        {
            DoWithUnconnectedRouterSocket(socket =>
            {
                Assert.AreEqual(RouterMandatory.Discard, socket.RouterMandatory);
            });
        }

        [Test, Ignore("Investigate why this fails, maybe libzmq too old?")]
        public void SetOption_RouterMandatory()
        {
            DoWithUnconnectedRouterSocket(socket =>
            {
                socket.RouterMandatory = RouterMandatory.Report;
                Assert.AreEqual(RouterMandatory.Report, socket.RouterMandatory);
            });
        }

        [Test]
        public void GetOption_TypeOfService()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(0, socket.TypeOfService);
            });
        }

        [Test]
        public void SetOption_TypeOfService()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.TypeOfService = 1;
                Assert.AreEqual(1, socket.TypeOfService);
            });
        }

        [Test, Ignore("Supported from libzmq 4.2.0")]
        public void GetOption_XPubVerbose()
        {
            DoWithUnconnectedSocket((context, socket) =>
            {
                Assert.AreEqual(false, socket.XPubVerbose);
            }, ZSocketType.XPUB);
        }

        [Test, Ignore("Supported from libzmq 4.2.0")]
        public void SetOption_XPubVerbose()
        {
            DoWithUnconnectedSocket((context, socket) =>
            {
                socket.XPubVerbose = true;
                Assert.AreEqual(true, socket.XPubVerbose);
            }, ZSocketType.XPUB);
        }

        [Test, Ignore("0 termination byte must be stripped when getting the property")]
        public void GetOption_ZAPDomain()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual("", socket.ZAPDomain);
            });
        }

        [Test, Ignore("0 termination byte must be stripped when getting the property")]
        public void SetOption_ZAPDomain()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.ZAPDomain = "abc";
                Assert.AreEqual("abc", socket.ZAPDomain);
            });
        }

        [Test]
        public void GetOption_IPv4Only()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(true, socket.IPv4Only);
            });
        }

        [Test]
        public void SetOption_IPv4Only()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.IPv4Only = false;
                Assert.AreEqual(false, socket.IPv4Only);
            });
        }

        [Test, Ignore("0 termination byte must be stripped when getting the property")]
        public void GetOption_PlainUserName()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual("", socket.PlainUserName);
            });
        }

        [Test, Ignore("0 termination byte must be stripped when getting the property")]
        public void SetOption_PlainUserName()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.PlainUserName = "abc";
                Assert.AreEqual("abc", socket.PlainUserName);
            });
        }

        [Test, Ignore("0 termination byte must be stripped when getting the property")]
        public void GetOption_PlainPassword()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual("", socket.PlainPassword);
            });
        }

        [Test, Ignore("0 termination byte must be stripped when getting the property")]
        public void SetOption_PlainPassword()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.PlainPassword = "abc";
                Assert.AreEqual("abc", socket.PlainPassword);
            });
        }

        [Test]
        public void GetOption_PlainServer()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(false, socket.PlainServer);
            });
        }

        [Test]
        public void SetOption_PlainServer()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.PlainServer = true;
                Assert.AreEqual(true, socket.PlainServer);
            });
        }

        [Test]
        public void GetOption_CurveServer()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                Assert.AreEqual(false, socket.CurveServer);
            });
        }

        [Test]
        public void SetOption_CurveServer()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.CurveServer = true;
                Assert.AreEqual(true, socket.CurveServer);
            });
        }

        [Test]
        public void AddTcpAcceptFilter()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.AddTcpAcceptFilter("127.0.0.1");
            });
        }
        
        [Test]
        public void ClearTcpAcceptFilters()
        {
            DoWithUnconnectedPairSocket(socket =>
            {
                socket.ClearTcpAcceptFilter();
            });
        }
        #endregion

        #region bind
        [Test]
        public void Bind()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket.Bind(DefaultAddress);
                }
            }
        }

        [Test]
        public void Bind_InvalidAddress()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    var exc = Assert.Throws<ZException>(() => socket.Bind(InvalidAddress));
                    Assert.AreEqual(ZError.EINVAL, exc.Error);
                }
            }
        }

        [Test]
        public void Bind_UnsupportedProtocol()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    var exc = Assert.Throws<ZException>(() => socket.Bind("xyz://foo"));
                    Assert.AreEqual(ZError.EPROTONOSUPPORT, exc.Error);
                }
            }
        }

        [Test]
        public void Unbind_Bound()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket.Bind(DefaultAddress);
                    socket.Unbind(DefaultAddress);
                }
            }
        }

        [Test]
        public void Unbind_Unbound_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    Assert.Throws<ZException>(() => socket.Unbind(DefaultAddress));
                }
            }
        }
        #endregion

        #region connect
        [Test]
        public void Connect()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket.Connect(DefaultAddress);
                }
            }
        }

        [Test]
        public void Connect_InvalidAddress()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    Assert.Throws<ZException>(() => socket.Connect(InvalidAddress));
                }
            }
        }

        [Test]
        public void Disconnect_Connected()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket.Connect(DefaultAddress);
                    socket.Disconnect(DefaultAddress);
                }
            }
        }

        [Test]
        public void Disconnect_Unconnected_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    Assert.Throws<ZException>(() => socket.Disconnect(DefaultAddress));
                }
            }
        }
        #endregion

        #region send
        [Test]
        public void Send_ZMessage_Empty()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket.Send(new ZMessage());
                }
            }
        }

        [Test]
        public void Send_ZMessage_NonEmpty()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    // TODO: if we leave out the Connect, this blocks indefinitely, even if Linger is set to Zero; probably this is due to a bug in libzmq
                    socket.Connect(DefaultAddress);
                    //socket.Linger = TimeSpan.Zero;
                    socket.Send(new ZMessage(new ZFrame[] { new ZFrame('a') }));
                }
            }
        }

        [Test]
        public void Send_ZMessage_Exception_IllegalState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REP))
                {
                    var exc = Assert.Throws<ZException>(() => socket.Send(CreateSingleFrameTestMessage()));
                    Assert.AreEqual(ZError.EFSM, exc.Error);
                }
            }
        }

        [Test]
        public void Send_ZMessage_WithFlags_Exception_IllegalState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REP))
                {
                    var exc = Assert.Throws<ZException>(() => socket.Send(CreateSingleFrameTestMessage(), ZSocketFlags.DontWait));
                    Assert.AreEqual(ZError.EFSM, exc.Error);
                }
            }
        }

        [Test]
        public void Send_ZMessage_WithFlags_IllegalState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REP))
                {
                    ZError error;
                    Assert.IsFalse(socket.Send(CreateSingleFrameTestMessage(), ZSocketFlags.DontWait, out error));
                    Assert.AreEqual(ZError.EFSM, error);
                }
            }
        }

        [Test]
        public void SendFrames_Exception_IllegalState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REP))
                {
                    var exc = Assert.Throws<ZException>(() => socket.Send(new ZFrame[] { new ZFrame('a') }));
                    Assert.AreEqual(ZError.EFSM, exc.Error);
                }
            }
        }

        [Test]
        public void SendFrames_Exception_WithFlags_IllegalState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REP))
                {
                    var exc = Assert.Throws<ZException>(() => socket.Send(new ZFrame[] { new ZFrame('a') }, ZSocketFlags.DontWait));
                    Assert.AreEqual(ZError.EFSM, exc.Error);
                }
            }
        }

        [Test]
        public void SendFrames_WithFlags_IllegalState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REP))
                {
                    ZError error;
                    Assert.IsFalse(socket.Send(new ZFrame[] { new ZFrame('a') }, ZSocketFlags.DontWait, out error));
                    Assert.AreEqual(ZError.EFSM, error);
                }
            }
        }

        [Test]
        public void SendFrames_IllegalState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REP))
                {
                    ZError error;
                    Assert.IsFalse(socket.Send(new ZFrame[] { new ZFrame('a') }, out error));
                    Assert.AreEqual(ZError.EFSM, error);
                }
            }
        }

        [Test]
        public void SendBytes_Exception_IllegalState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REP))
                {
                    var exc = Assert.Throws<ZException>(() => socket.Send(new byte[] { 42 }, 0, 1));
                    Assert.AreEqual(ZError.EFSM, exc.Error);
                }
            }
        }

        [Test]
        public void SendBytes_IllegalState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REP))
                {
                    ZError error;
                    Assert.IsFalse(socket.Send(new byte[] { 42 }, 0, 1, ZSocketFlags.DontWait, out error));
                    Assert.AreEqual(ZError.EFSM, error);
                }
            }
        }
        #endregion

        #region receive
        [Test]
        public void ReceiveMessage_DontWait_NoneAvailable()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    ZError error;
                    ZMessage message = socket.ReceiveMessage(ZSocketFlags.DontWait, out error);
                    Assert.AreEqual(ZError.EAGAIN, error);
                    Assert.IsNull(message);
                }
            }
        }

        [Test]
        public void ReceiveFrames_DontWait_NoneAvailable()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    ZError error;
                    var message = socket.ReceiveFrames(1, ZSocketFlags.DontWait, out error);
                    Assert.AreEqual(ZError.EAGAIN, error);
                    Assert.IsNull(message);
                }
            }
        }

        [Test]
        public void ReceiveFrame_DontWait_NoneAvailable()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    ZError error;
                    var message = socket.ReceiveFrame(ZSocketFlags.DontWait, out error);
                    Assert.AreEqual(ZError.EAGAIN, error);
                    Assert.IsNull(message);
                }
            }
        }

        [Test]
        public void ReceiveBytes_InvalidState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REQ))
                {
                    var buffer = new byte[1];
                    var exc = Assert.Throws<ZException>(() => socket.ReceiveBytes(buffer, 0, 1));
                    Assert.AreEqual(ZError.EFSM, exc.Error);
                }
            }
        }

        [Test]
        public void ReceiveMessage_InvalidState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REQ))
                {
                    ZError error;
                    var message = socket.ReceiveMessage(out error);
                    Assert.AreEqual(ZError.EFSM, error);
                    Assert.IsNull(message);
                }
            }
        }

        [Test]
        public void ReceiveMessage_WithFlags_InvalidState_Fails()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REQ))
                {
                    var exc = Assert.Throws<ZException>(() => socket.ReceiveMessage(ZSocketFlags.DontWait));
                    Assert.AreEqual(ZError.EFSM, exc.Error);
                }
            }
        }

        [Test]
        public void ReceiveFrames_InvalidState()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REQ))
                {
                    ZError error;
                    var message = socket.ReceiveFrames(1, out error);
                    Assert.AreEqual(ZError.EFSM, error);
                    Assert.IsNull(message);
                }
            }
        }

        [Test]
        public void ReceiveFrames_Exception_InvalidState()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REQ))
                {
                    var exc = Assert.Throws<ZException>(() => socket.ReceiveFrames(1));
                    Assert.AreEqual(ZError.EFSM, exc.Error);
                }
            }
        }

        [Test]
        public void ReceiveFrame_Exception_InvalidState()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.REQ))
                {
                    var exc = Assert.Throws<ZException>(() => socket.ReceiveFrame());
                    Assert.AreEqual(ZError.EFSM, exc.Error);
                }
            }
        }
        #endregion

        #region send-and-receive
        [Test]
        public void SendAndReceiveMessage()
        {
            DoWithConnectedSocketPair((sendSocket, receiveSocket) =>
            {
                sendSocket.Send(CreateSingleFrameTestMessage());

                ZError error;
                var message = receiveSocket.ReceiveMessage(ZSocketFlags.None, out error);
                Assert.AreEqual(null, error);
                Assert.AreEqual(CreateSingleFrameTestMessage(), message);
            });
        }

        [Test]
        public void SendBytesAndReceiveBytes()
        {
            DoWithConnectedSocketPair((sendSocket, receiveSocket) =>
            {
                var sentMessage = new byte[] { 42 };
                sendSocket.Send(sentMessage, 0, 1);

                byte[] receivedMessage = new byte[1];
                var result = receiveSocket.ReceiveBytes(receivedMessage, 0, 1);
                Assert.AreEqual(1, result);
                CollectionAssert.AreEqual(sentMessage, receivedMessage);
            });
        }

        [Test]
        public void SendAndReceiveFrame()
        {
            DoWithConnectedSocketPair((sendSocket, receiveSocket) =>
            {
                sendSocket.Send(CreateSingleFrameTestMessage());

                var frame = receiveSocket.ReceiveFrame();
                Assert.AreEqual(CreateSingleFrameTestMessage()[0], frame);
            });
        }

        [Test]
        public void SendAndReceiveFrames_Exception()
        {
            DoWithConnectedSocketPair((sendSocket, receiveSocket) =>
            {
                sendSocket.Send(CreateSingleFrameTestMessage());

                var message = receiveSocket.ReceiveFrames(1);
                Assert.AreEqual(CreateSingleFrameTestMessage(), message);
            });
        }

        [Test]
        public void SendAndReceiveFrames()
        {
            DoWithConnectedSocketPair((sendSocket, receiveSocket) =>
            {
                sendSocket.Send(CreateSingleFrameTestMessage());

                ZError error;
                var message = receiveSocket.ReceiveFrames(1, out error);
                Assert.AreEqual(null, error);
                Assert.AreEqual(CreateSingleFrameTestMessage(), message);
            });
        }

        [Test]
        public void SendAndReceiveFrames_LessFrames()
        {
            DoWithConnectedSocketPair((sendSocket, receiveSocket) =>
            {
                sendSocket.Send(CreateMultipleFrameTestMessage());

                ZError error;
                var message = receiveSocket.ReceiveFrames(1, out error);
                Assert.AreEqual(null, error);
                Assert.AreEqual(CreateSingleFrameTestMessage(), message);
            });
        }

        [Test, Ignore("The implementation must be fixed")]
        public void SendAndReceiveFrames_NoFrames()
        {
            DoWithConnectedSocketPair((sendSocket, receiveSocket) =>
            {
                sendSocket.Send(CreateSingleFrameTestMessage());

                ZError error;
                var message = receiveSocket.ReceiveFrames(0, out error);
                Assert.AreEqual(null, error);
                Assert.AreEqual(new ZMessage(), message);
            });
        }

        [Test]
        public void SendAndReceiveFrames_TooManyFrames()
        {
            DoWithConnectedSocketPair((sendSocket, receiveSocket) =>
            {
                sendSocket.Send(CreateSingleFrameTestMessage());

                ZError error;
                var message = receiveSocket.ReceiveFrames(2, out error);
                Assert.AreEqual(null, error);
                Assert.AreEqual(CreateSingleFrameTestMessage(), message);

                // TODO is this intended? shouldn't it yield an error if we want to receive more frames than the message contains?
            });
        }
        #endregion

        private static void DoWithUnconnectedRouterSocket(Action<ZSocket> action)
        {
            DoWithUnconnectedSocket((context, socket) => action(socket), ZSocketType.ROUTER);
        }

        private static void DoWithUnconnectedPairSocket(Action<ZSocket> action)
        {
            DoWithUnconnectedSocket((context, socket) => action(socket), ZSocketType.PAIR);
        }

        private static void DoWithUnconnectedReqSocket(Action<ZSocket> action)
        {
            DoWithUnconnectedSocket((context, socket) => action(socket), ZSocketType.REQ);
        }

        private static void DoWithUnconnectedSocket(Action<ZContext, ZSocket> action, ZSocketType socketType)
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, socketType))
                {
                    action(context, socket);
                }
            }
        }

        private static void DoWithConnectedSocketPair(Action<ZSocket, ZSocket> action)
        {
            DoWithUnconnectedSocket((context, sendSocket) =>
            {
                sendSocket.Connect(DefaultAddress);
                using (var receiveSocket = new ZSocket(context, ZSocketType.PAIR))
                {
                    receiveSocket.Bind(DefaultAddress);
                    action(sendSocket, receiveSocket);
                }
            }, ZSocketType.PAIR);
        }

        private static ZMessage CreateSingleFrameTestMessage()
        {
            return new ZMessage(new ZFrame[] { new ZFrame('a') });
        }

        private static ZMessage CreateMultipleFrameTestMessage()
        {
            return new ZMessage(new ZFrame[] { new ZFrame('a'), new ZFrame('b') });
        }

    }
}

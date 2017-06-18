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
        private const string DefaultAddress = "inproc://foo";
        private const string InvalidAddress = "__BAD_ADDRESS__";

        static IEnumerable<ZSocketType> ValidSocketTypes { get { return Enum.GetValues(typeof(ZSocketType)).Cast<ZSocketType>().Except(new[] { ZSocketType.None }); } }

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

        #region socket options
        [Test]
        public void GetOption_Affinity()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    Assert.AreEqual(0, socket.Affinity);
                }
            }
        }

        [Test]
        public void SetOption_Affinity()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket.Affinity = 1;
                    Assert.AreEqual(1, socket.Affinity);
                }
            }
        }

        [Test]
        public void GetOption_Backlog()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    Assert.AreEqual(100, socket.Backlog);
                }
            }
        }

        [Test]
        public void SetOption_Backlog()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket.Backlog = 0;
                    Assert.AreEqual(0, socket.Backlog);
                }
            }
        }

        [Test]
        public void GetOption_Conflate()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    Assert.AreEqual(false, socket.Conflate);
                }
            }
        }

        [Test]
        public void GetOption_ConnectRID()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.ROUTER))
                {
                    // TODO: Probably this socket option cannot be queried. The property getter should be removed.
                    Assert.Throws<ZException>(() => { var x = socket.ConnectRID; });
                }
            }
        }

        [Test]
        public void SetOption_ConnectRID()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.ROUTER))
                {
                    socket.ConnectRID = new byte[] { 1, 2, 3, 4 };
                }
            }
        }

        [Test, Ignore("Issue in underlying libzmq fixed in commit https://github.com/zeromq/libzmq/commit/f86795350d2c37753b961018b5185cd1af33a38a")]
        public void GetOption_CurvePublicKey()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    CollectionAssert.AreEqual(Enumerable.Repeat<byte>(0, ZSocket.BinaryKeySize).ToArray(), socket.CurvePublicKey);
                }
            }
        }

        [Test, Ignore("Issue in underlying libzmq fixed in commit https://github.com/zeromq/libzmq/commit/f86795350d2c37753b961018b5185cd1af33a38a")]
        public void GetOption_CurveSecretKey()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    CollectionAssert.AreEqual(Enumerable.Repeat<byte>(0, ZSocket.BinaryKeySize).ToArray(), socket.CurveSecretKey);
                }
            }
        }

        [Test, Ignore("Issue in underlying libzmq fixed in commit https://github.com/zeromq/libzmq/commit/f86795350d2c37753b961018b5185cd1af33a38a")]
        public void GetOption_CurveServerKey()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    CollectionAssert.AreEqual(Enumerable.Repeat<byte>(0, ZSocket.BinaryKeySize).ToArray(), socket.CurveServerKey);
                }
            }
        }

        [Test]
        public void SetOption_CurvePublicKey()
        {
            if (!ZContext.Has("curve")) Assert.Ignore("Ignored due to missing curve support");

            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    // TODO: the interface is confusing; Z85-encoded keys should always be strings
                    byte[] publicKeyZ85, secretKeyZ85;
                    Z85.CurveKeypair(out publicKeyZ85, out secretKeyZ85);
                    socket.CurvePublicKey = publicKeyZ85;
                    byte[] publicKeyBinary = Z85.Decode(publicKeyZ85);
                    CollectionAssert.AreEqual(publicKeyBinary, socket.CurvePublicKey);
                }
            }
        }

        [Test]
        public void SetOption_CurveServerKey()
        {
            if (!ZContext.Has("curve")) Assert.Ignore("Ignored due to missing curve support");

            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    // TODO: the interface is confusing; Z85-encoded keys should always be strings
                    byte[] publicKeyZ85, secretKeyZ85;
                    Z85.CurveKeypair(out publicKeyZ85, out secretKeyZ85);
                    socket.CurveServerKey = publicKeyZ85;
                    byte[] publicKeyBinary = Z85.Decode(publicKeyZ85);
                    CollectionAssert.AreEqual(publicKeyBinary, socket.CurveServerKey);
                }
            }
        }

        [Test]
        public void SetOption_CurveSecretKey()
        {
            if (!ZContext.Has("curve")) Assert.Ignore("Ignored due to missing curve support");

            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    // TODO: the interface is confusing; Z85-encoded keys should always be strings
                    byte[] publicKeyZ85, secretKeyZ85;
                    Z85.CurveKeypair(out publicKeyZ85, out secretKeyZ85);
                    socket.CurveSecretKey = secretKeyZ85;
                    byte[] secretKeyBinary = Z85.Decode(secretKeyZ85);
                    CollectionAssert.AreEqual(secretKeyBinary, socket.CurveSecretKey);
                }
            }
        }

        [Test]
        public void GetOption_GSSAPIPlainText()
        {

            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    if (!ZContext.Has("gssapi"))
                    {
                        Assert.Throws<ZException>(() => { var res = socket.GSSAPIPlainText; });
                    }
                    else
                    {
                        Assert.AreEqual(false, socket.GSSAPIPlainText);
                    }
                }
            }
        }

        [Test]
        public void SetOption_GSSAPIPlainText()
        {
            if (!ZContext.Has("gssapi")) Assert.Ignore("libzmq does not support gssapi");

            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket.GSSAPIPlainText = true;
                    Assert.AreEqual(true, socket.GSSAPIPlainText);
                }
            }
        }

        [Test]
        public void GetOption_GSSAPIPrincipal()
        {
            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    if (!ZContext.Has("gssapi"))
                    {
                        Assert.Throws<ZException>(() => { var res = socket.GSSAPIPrincipal; });
                    }
                    else
                    {
                        Assert.IsEmpty(socket.GSSAPIPrincipal);
                    }
                }
            }
        }

        [Test]
        public void SetOption_GSSAPIPrincipal()
        {
            if (!ZContext.Has("gssapi")) Assert.Ignore("libzmq does not support gssapi");

            using (var context = new ZContext())
            {
                using (var socket = new ZSocket(context, ZSocketType.PAIR))
                {
                    socket.GSSAPIPrincipal = "foo";
                    Assert.AreEqual("foo", socket.GSSAPIPrincipal);
                }
            }
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


        private static void DoWithConnectedSocketPair(Action<ZSocket, ZSocket> action)
        {
            using (var context = new ZContext())
            {
                using (var sendSocket = new ZSocket(context, ZSocketType.PAIR))
                {
                    sendSocket.Connect(DefaultAddress);
                    using (var receiveSocket = new ZSocket(context, ZSocketType.PAIR))
                    {
                        receiveSocket.Bind(DefaultAddress);
                        action(sendSocket, receiveSocket);
                    }
                }
            }
        }

        private static ZMessage CreateSingleFrameTestMessage()
        {
            return new ZMessage(new ZFrame[] { new ZFrame('a') });
        }

        private static ZMessage CreateMultipleFrameTestMessage()
        {
            return new ZMessage(new ZFrame[] { new ZFrame('a'), new ZFrame('b') });
        }
        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZeroMQ
{
    /// <summary>
    /// This class is a port of zauth.c provided in CZMQ, http://czmq.zeromq.org.
    /// 
    /// A ZAuth actor takes over authentication for all incoming connections in
    /// its context. You can whitelist or blacklist peers based on IP address,
    /// and define policies for securing PLAIN, CURVE, and GSSAPI (not implemented!) connections.
    /// </summary>
    public class ZAuth : IDisposable
    {

        /// <summary>
        /// A small class for working with ZAP requests and replies.
        /// Used internally in ZAuth to simplify working with RFC 27 messages.
        /// </summary>
        private class ZAP
        {

            ZSocket handler;
            bool Verbose;

            public string Version { get; private set; }
            public string Sequence { get; private set; }
            public string Domain { get; private set; }
            public string Address { get; private set; }
            public string Identity { get; private set; }
            public string Mechanism { get; private set; }
            public string Password { get; private set; }
            public string Username { get; private set; }

            public string UserId { get; set; }

            public string Principal { get; set; }
            public string ClientTxt { get; set; }

            /// <summary>
            /// Receive a valid ZAP request from the handler socket
            /// </summary>
            /// <param name="handler"></param>
            /// <param name="request"></param>
            /// <param name="verbose"></param>
            public ZAP(ZSocket handler, ZMessage request, bool verbose)
            {
                //  Store handler socket so we can send a reply easily
                this.handler = handler;
                Verbose = verbose;

                if (request.Count == 0)
                {
                    return;
                }

                //  Get all standard frames off the handler socket
                Version = request.Pop().ReadLine();
                Sequence = request.Pop().ReadLine();
                Domain = request.Pop().ReadLine();
                Address = request.Pop().ReadLine();
                Identity = request.Pop().ReadLine();
                Mechanism = request.Pop().ReadLine();

                Mechanism = string.IsNullOrEmpty(Mechanism) ? "" : Mechanism;
                Version = string.IsNullOrEmpty(Version) ? "" : Version;
                Sequence = string.IsNullOrEmpty(Sequence) ? "" : Sequence;
                Domain = string.IsNullOrEmpty(Domain) ? "" : Domain;
                Address = string.IsNullOrEmpty(Address) ? "" : Address;
                Identity = string.IsNullOrEmpty(Identity) ? "" : Identity;


                //  If the version is wrong, we're linked with a bogus libzmq, so die
                if (Version != "1.0")
                {
                    return;
                }

                //  Get mechanism-specific frames
                if (Mechanism == "PLAIN")
                {
                    Username = request.Pop().ReadLine();
                    Password = request.Pop().ReadLine();
                    Username = string.IsNullOrEmpty(Username) ? "" : Username;
                    Password = string.IsNullOrEmpty(Password) ? "" : Password;
                }
                else
                if (Mechanism == "CURVE")
                {
                    ZFrame frame = request.Pop();

                    if (frame.Length != 32)
                    {
                        return;
                    }
                    ZCert cert = new ZCert(frame.Read(), new byte[32]);
                    ClientTxt = cert.PublicTxt;
                }
                else
                if (Mechanism == "GSSAPI")
                    Principal = request.Pop().ReadLine();

                if (Verbose)
                    ZAuth.Info(string.Format("zauth: ZAP request mechanism={0} ipaddress={1}", Mechanism, Address));
            }

            /// <summary>
            /// Send a ZAP reply to the handler socket
            /// </summary>
            /// <param name="status_code"></param>
            /// <param name="status_text"></param>
            /// <param name="metadata"></param>
            /// <returns></returns>
            public int RequestReply(string status_code, string status_text, byte[] metadata)
            {
                if (Verbose)
                    ZAuth.Info(string.Format("zauth: - ZAP reply status_code={0} status_text={1}", status_code, status_text));

                ZMessage msg = new ZMessage();
                msg.Add(new ZFrame("1.0"));
                msg.Add(new ZFrame(Sequence));
                msg.Add(new ZFrame(status_code));
                msg.Add(new ZFrame(status_text));
                msg.Add(new ZFrame(UserId != null ? UserId : ""));
                // rc = zmsg_addmem(msg, metadata, metasize);
                msg.Add(new ZFrame(metadata));
                handler.SendMessage(msg);
                return 0;
            }
        }

        /// <summary>
        /// Socket pollers
        /// </summary>
        ZPollItem[] pollers;
        /// <summary>
        /// Contains the actor front end pipe and ZAP handler socket
        /// </summary>
        ZSocket[] sockets;

        /// <summary>
        /// Whitelisted addresses
        /// </summary>
        HashSet<string> whitelist = new HashSet<string>();
        /// <summary>
        /// Blacklisted addresses
        /// </summary>
        HashSet<string> blacklist = new HashSet<string>();
        /// <summary>
        /// PLAIN passwords, if loaded
        /// </summary>
        Dictionary<string, string> passwords = new Dictionary<string, string>();

        /// <summary>
        /// CURVE allows arbitrary clients
        /// </summary>
        bool allowAny;

        /// <summary>
        /// Verbose logging enabled?
        /// </summary>
        bool verbose;

        ZCertStore certStore;

        /// <summary>
        /// Actor command pipe
        /// </summary>
        private static readonly int PIPE = 0;
        /// <summary>
        /// ZAP handler socket
        /// </summary>
        private static readonly int HANDLER = 1;

        private static readonly string ZAP_ENDPOINT = "inproc://zeromq.zap.01";
        private static readonly string CURVE_ALLOW_ANY = "*";

        /// <summary>
        /// Construct authourization handler
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pipe"></param>
        /// <param name="certStore"></param>
        private ZAuth(ZContext context, ZSocket pipe, ZCertStore certStore = null)
        {
            if (context != null)
            {
                sockets = new ZSocket[] { pipe, new ZSocket(context, ZSocketType.REP) };
            }
            else
            {
                sockets = new ZSocket[] { pipe, new ZSocket(ZSocketType.REP) };
            }
            sockets[HANDLER].Bind(ZAP_ENDPOINT);
            pollers = new ZPollItem[] { ZPollItem.CreateReceiver(), ZPollItem.CreateReceiver() };
            allowAny = true;
            verbose = false;
            Terminated = false;
            this.certStore = certStore;
        }

        private ZAuth(ZSocket pipe, ZCertStore certStore = null) : this(null, pipe, certStore)
        {
        }

        /// <summary>
        /// Did caller ask us to quit?
        /// </summary>
        private bool Terminated { get; set; }

        /// <summary>
        /// Start an authorization action on the given context by polling the backend socket of a ZActor.
        /// </summary>
        /// <param name="context">The context used to create the ZSockets.</param>
        /// <param name="backend">ZActor backend socket.</param>
        /// <param name="cancellor">Thread cancellation called when ZActor is disposed.</param>
        /// <param name="args">Arguments given to the ZActor. If the first object in this list is a a ZCertStore 
        /// this ZCertStore is used for ZCert handling.</param>
        public static void Action(ZContext context, ZSocket backend, System.Threading.CancellationTokenSource cancellor, object[] args)
        {
            ZCertStore certStore = args != null && args.Length > 0 && args[0] is ZCertStore ? args[0] as ZCertStore : null;
            using (ZAuth self = new ZAuth(context, backend, certStore))
            {
                Run(cancellor, self);
            }
        }

        /// <summary>
        /// Start an authorization action on the default context by polling the backend socket of a ZActor.
        /// </summary>
        /// <param name="backend">ZActor backend socket.</param>
        /// <param name="cancellor">Thread cancellation called when ZActor is disposed.</param>
        /// <param name="args">Arguments given to the ZActor. If the first object in this list is a a ZCertStore 
        /// this ZCertStore is used for ZCert handling.</param>
        public static void Action0(ZSocket backend, System.Threading.CancellationTokenSource cancellor, object[] args)
        {
            ZCertStore certStore = args != null && args.Length > 0 && args[0] is ZCertStore ? args[0] as ZCertStore : null;
            using (ZAuth self = new ZAuth(backend, certStore))
            {
                Run(cancellor, self);
            }
        }

        private static void Run(System.Threading.CancellationTokenSource cancellor, ZAuth self)
        {
            while (!self.Terminated && !cancellor.IsCancellationRequested)
            {
                ZMessage[] msg;
                ZError error;
                // we only poll for 50 ms so we can look at the cancellation flags from time to time...
                TimeSpan? wait = TimeSpan.FromMilliseconds(50);
                if (self.sockets.PollIn(self.pollers, out msg, out error, wait))
                {
                    if (msg != null && msg.Length == 2)
                    {
                        if (msg[PIPE] != null)
                        {
                            // process self
                            self.HandlePipe(msg[PIPE]);
                        }

                        if (msg[HANDLER] != null)
                        {
                            // process authorization
                            self.Authenticate(msg[HANDLER]);
                        }
                    }
                }
                else
                {
                    if (error == ZError.ETERM)
                        break;
                    if (error != ZError.EAGAIN)
                        throw new ZException(error);
                }
            }
        }

        private void Authenticate(ZMessage zMessage)
        {
            ZAP request = new ZAP(sockets[HANDLER], zMessage, verbose);
            //  Is address explicitly whitelisted or blacklisted?
            bool allowed = false;
            bool denied = false;
            if (whitelist.Count > 0)
            {
                if (whitelist.Contains(request.Address))
                {
                    allowed = true;
                    if (verbose)
                        Info("zauth: - passed (whitelist) address=" + request.Address);
                }
                else
                {
                    denied = true;
                    if (verbose)
                        Info("zauth: - denied (whitelist) address=" + request.Address);
                }
            }

            if (blacklist.Count > 0)
            {
                if (blacklist.Contains(request.Address))
                {
                    denied = true;
                    if (verbose)
                        Info("zauth: - denied (blacklist) address=" + request.Address);
                }
                else
                {
                    allowed = true;
                    if (verbose)
                        Info("zauth: -  passed (not in blacklist) address=" + request.Address);
                }
            }

            //  Curve certificate metadata
            List<byte> metabuf = new List<byte>(512);
            int metadataLength = 0;

            //  Mechanism-specific checks
            if (!denied)
            {
                if (request.Mechanism.ToUpper() == "NULL" && !allowed)
                {
                    //  For NULL, we allow if the address wasn't blacklisted
                    if (verbose)
                        Info("zauth: - allowed (NULL)");
                    allowed = true;
                }
                else
                if (request.Mechanism == "PLAIN")
                    //  For PLAIN, even a whitelisted address must authenticate
                    allowed = AuthenticatePlain(request);
                else
                if (request.Mechanism == "CURVE")
                    //  For CURVE, even a whitelisted address must authenticate
                    allowed = AuthenticateCurve(request, metabuf, out metadataLength);
                else
                if (request.Mechanism == "GSSAPI")
                    //  For GSSAPI, even a whitelisted address must authenticate
                    allowed = AuthenticateGssapi(request);
            }

            if (allowed)
            {
                //size_t metasize = metadata - metabuf;
                //s_zap_request_reply(request, "200", "OK", metabuf, metasize);                
                request.RequestReply("200", "OK", metabuf.GetRange(0, metadataLength).ToArray());
            }
            else

            {
                //s_zap_request_reply(request, "400", "No access", (unsigned char *) "", 0);
                request.RequestReply("400", "No access", new byte[0]);
                
            }
        }

        private bool AuthenticateGssapi(ZAP request)
        {
            if (verbose)
                Info(string.Format("zauth: - allowed (GSSAPI) principal={0} identity={1}",
                           request.Principal, request.Identity));
            request.UserId = request.Principal;
            return true;
        }

        private bool AuthenticateCurve(ZAP request, List<byte> metabuf, out int metadataLength)
        {
            metadataLength = 0;
            if (allowAny)
            {
                if (verbose)
                    Info("zauth: - allowed (CURVE allow any client)");
                return true;
            }
            else
            if (certStore != null)
            {
                ZCert cert = certStore.Lookup(request.ClientTxt);
                if (cert != null)
                {
                    Dictionary<string, string> meta = cert.MetaData;
                    foreach (var pair in meta)
                    {
                        string key = pair.Key;
                        string val = pair.Value;
                        AddProperty(metabuf, key, val);
                    }

                    if (verbose)
                        Info("zauth: - allowed (CURVE) client_key=" + request.ClientTxt.ToString());
                    request.UserId = request.ClientTxt;
                    return true;
                }
            }

            if (verbose)
                Info("zauth: - denied (CURVE) client_key=" + request.ClientTxt.ToString());
            return false;
        }

        private static int AddProperty(List<byte> metabuf, string name, string value)
        {
            byte name_len = (byte)name.Length;
            metabuf.Add(name_len);
            metabuf.AddRange(name.ToCharArray().Select(c => (byte)c));

            PutUint32(metabuf, (uint)value.Length);
            metabuf.AddRange(value.ToCharArray().Select(c => (byte)c));

            return 1 + name.Length + 4 + value.Length;
        }

        private static void PutUint32(List<byte> buffer, UInt32 value)
        {
            buffer.Add((byte)(((value) >> 24) & 0xff));
            buffer.Add((byte)(((value) >> 16) & 0xff));
            buffer.Add((byte)(((value) >> 8) & 0xff));
            buffer.Add((byte)(value & 0xff));
        }

        private bool AuthenticatePlain(ZAP request)
        {
            if (passwords.Count > 0)
            {
                if (passwords.ContainsKey(request.Username) && passwords[request.Username] == request.Password)
                {
                    if (verbose)
                        Info(string.Format("zauth: - allowed (PLAIN) username={0} password={1}",
                           request.Username, request.Password));
                    return true;
                }
                else
                {
                    if (verbose)
                        Info(string.Format("zauth: - denied (PLAIN) username={0} password={1}",
                               request.Username, request.Password));
                    return false;
                }
            }
            else
            {
                if (verbose)
                    Info("zauth: - denied (PLAIN) no password file defined");
                return false;
            }
        }

        private int HandlePipe(ZMessage request)
        {
            if (request.Count == 0)
                return -1;                  //  Interrupted

            ZFrame commandFrame = request.Pop();
            string command = commandFrame.ReadLine();
            if (verbose)
                Info("zauth: API command=" + command);

            if (command == "ALLOW")
            {
                while (request.Count > 0)
                {
                    ZFrame frame = request.Pop();
                    string address = frame.ReadLine();
                    if (verbose)
                        Info("zauth: - whitelisting ipaddress=" + address);

                    if (!whitelist.Contains(address))
                        whitelist.Add(address);
                }
                // 
                sockets[PIPE].SendFrame(new ZFrame(0));
            }
            else
            if (command == "DENY")
            {
                while (request.Count > 0)
                {
                    ZFrame frame = request.Pop();
                    string address = frame.ReadLine();
                    if (verbose)
                        Info("zauth: - blacklisting ipaddress=" + address);

                    if (!blacklist.Contains(address))
                        blacklist.Add(address);
                    if (whitelist.Contains(address))
                        whitelist.Remove(address);
                }
                sockets[PIPE].SendFrame(new ZFrame(0));
            }
            else
            if (command == "PLAIN")
            {
                //  Get password file and load into zhash table
                //  If the file doesn't exist we'll get an empty table
                ZFrame frame = request.Pop();
                string filename = frame.ReadLine();
                if (Load(out passwords, filename) != 0 && verbose)
                    Info("zauth: could not load file=" + filename);
                sockets[PIPE].SendFrame(new ZFrame(0));
            }
            else
            if (command == "CURVE")
            {
                //  If location is CURVE_ALLOW_ANY, allow all clients. Otherwise
                //  treat location as a directory that holds the certificates.
                ZFrame frame = request.Pop();
                string location = frame.ReadLine();
                if (location == CURVE_ALLOW_ANY)
                {
                    allowAny = true;
                }
                else
                {
                    certStore = new ZCertStore(location);
                    allowAny = false;
                }
                sockets[PIPE].SendFrame(new ZFrame(0));
            }
            else
            if (command == "GSSAPI")
                //  GSSAPI authentication is not yet implemented here
                sockets[PIPE].SendFrame(new ZFrame(0));
            else
            if (command == "VERBOSE")
            {
                verbose = true;
                sockets[PIPE].SendFrame(new ZFrame(0));
            }
            else
            if (command == "$TERM")
                Terminated = true;
            else
            {
                Error("zauth: - invalid command: " + command);
            }
            return 0;
        }

        private static void Info(string msg)
        {
            Console.WriteLine("I: " + DateTime.Now.ToLongDateString() + " ZAuth: " + msg);
        }

        private static void Error(string msg)
        {
            Console.WriteLine("E: " + DateTime.Now.ToLongDateString() + " ZAuth: " + msg);
        }


        /// <summary>
        /// Finalizes an instance of the <see cref="ZSocket"/> class.
        /// </summary>
        ~ZAuth()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ZSocket"/>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        /// <summary>
		/// Close the current  zauth/zactor.
		/// </summary>
		public void Close()
        {
            Terminated = true;
        }

        /// <summary>
        /// Load hash table from a text file in name=value format; hash table must
        ///  already exist. Hash values must printable strings.
        ///  Returns 0 if OK, else -1 if a file was not readable.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        static private int Load(out Dictionary<string, string> self, string filename)
        {
            self = new Dictionary<string, string>();

            if (!File.Exists(filename))
                return -1;
            //  Whether or not file exists, we'll track the filename and last
            //  modification date (0 for unknown files), so that zhashx_refresh ()
            //  will always work after zhashx_load (), to load a newly-created
            //  file.

            //  Take copy of filename in case self->filename is same string.
            Stack<string> lines = new Stack<string>(File.ReadAllLines(filename).ToList());
            while (lines.Count > 0)
            {
                string buffer = lines.Pop();
                //  Skip lines starting with "#" or that do not look like
                //  name=value data.
                if (buffer.TrimStart().StartsWith("#"))
                    continue;
                string[] pair = Split(buffer);
                if (pair.Length == 2)
                {
                    self.Add(pair[0], pair[1]);
                }
            }
            return 0;
        }

        private static string[] Split(string str)
        {
            int splitindex = str.IndexOf('"');
            string[] metadata = new string[0];
            if (splitindex > 2)
            {
                metadata = new string[2] { str.Substring(0, splitindex - 2).Trim(), str.Substring(splitindex).Trim() };
            }

            return metadata;
        }
    }
}

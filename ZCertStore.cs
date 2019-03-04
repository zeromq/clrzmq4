using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeroMQ
{
    /// <summary>
    /// This class is a port of zcertstore.c provided in CZMQ, http://czmq.zeromq.org.
    /// 
    /// To authenticate new clients using the ZeroMQ CURVE security mechanism,
    /// we have to check that the client's public key matches a key we know and
    /// accept.There are numerous ways to store accepted client public keys.
    /// The mechanism CZMQ implements is "certificates" (plain text files) held
    /// in a "certificate store" (a disk directory). This class works with such
    /// certificate stores, and lets you easily load them from disk, and check
    /// if a given client public key is known or not. The ZCert class does the
    /// work of managing a single certificate.
    /// </summary>
    /// <remarks>
    /// The certificate store can be memory-only, in which case you can load it
    /// yourself by inserting certificate objects one by one, or it can be loaded
    /// from disk, in which case you can add, modify, or remove certificates on
    /// disk at any time, and the store will detect such changes and refresh
    /// itself automatically.In most applications you won't use this class
    /// directly but through the ZAuth class, which provides a high-level API for
    /// authentication(and manages certificate stores for you). To actually
    /// create certificates on disk, use the ZCert class in code or any text editor.
    /// The format of a certificate file is defined in the ZCert man page of CZMQ.
    /// </remarks>
    class ZCertStore
    {
        // a dictionary with public keys (in text) and their certificates.
        Dictionary<string, ZCert> certs = new Dictionary<string, ZCert>();

        FileSystemWatcher watcher = new FileSystemWatcher();

        /// <summary>
        /// The path to the certificate store (e.g. ".curve") or null if in memory only.
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// Certificate store in memory constructor,
        /// </summary>
        public ZCertStore() : this(null)
        {
        }

        /// <summary>
        /// Create a new certificate store, loading and indexing all certificates.
        /// Specifying the location argument will setup the directory loader for this
        /// ZCertStore instance. The directory itself may be absent, and created later,
        /// or modified at any time. The certificate store is automatically refreshed. 
        /// If the location is specified as NULL, creates a pure-memory store,
        /// which you can work with by inserting certificates at runtime.
        /// </summary>
        /// <param name="location">The location of the certificate store. May be null if a pure in memory store should be used.</param>
        public ZCertStore(string location)
        {
            if (location != null)
            {
                Location = location;
                watcher.Path = location;
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.LastAccess;
                watcher.Filter = "*.*";
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.EnableRaisingEvents = true;
                Load(location);
            }
            else
            {
                Location = null;
            }
        }

        /// <summary>
        /// Lookup a certificate by the public key. Null is returned if the certificate isn't found.
        /// </summary>
        /// <param name="publicTxt">Public key if certificate to search for.</param>
        /// <returns>Return the found certificate or null if it isn't found.</returns>
        public ZCert Lookup(string publicTxt)
        {
            lock (certs)
            {
                if (certs.ContainsKey(publicTxt))
                {
                    return certs[publicTxt];
                }
            }
            return null;
        }

        /// <summary>
        /// Insert a certificate to this ZCertStore. Note that this will override any existing certificate in the store
        /// which has the same public key.
        /// </summary>
        /// <param name="cert">Certificate to store in ZCertStore.</param>
        public void Insert(ZCert cert)
        {
            lock (certs)
            { 
                certs[cert.PublicTxt] = cert;
            }
        }

        /// <summary>
        /// Clear this certificate store from all certificates.
        /// </summary>
        public void Clear()
        {
            lock (certs)
            {
                certs.Clear();
            }
        }

        /// <summary>
        /// Get a list with all certificates in this ZCertStore.
        /// </summary>
        /// <returns></returns>
        public List<ZCert> Certs()
        {
            lock (certs)
            {
                return certs.Values.ToList();
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Load(Location);
        }

        private void Load(string path)
        {
            lock (certs)
            {
                certs.Clear();
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                    foreach (var filename in files)
                    {
                        ZCert cert = ZCert.Load(Path.Combine(filename));
                        if (cert != null && (filename.EndsWith("_secret") || !certs.ContainsKey(cert.PublicTxt)))
                        {
                            certs[cert.PublicTxt] = cert;
                        }
                    }
                }
            }
        }
    }
}

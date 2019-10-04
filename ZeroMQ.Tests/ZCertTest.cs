using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZeroMQ;


namespace ZeroMQ.Tests
{
    [TestClass]
    public class ZCertTest
    {
        [TestMethod]
        public void TestConstructors()
        {
            ZCert cert1 = new ZCert();

            String pub_key = cert1.PublicTxt;
            String sec_key = cert1.SecretTxt;

            Console.WriteLine("Generated public key: " + pub_key);
            Console.WriteLine("Generated secret key: " + sec_key);

            /*
            * Test string constructor
            */
            ZCert cert2 = new ZCert(pub_key, sec_key);
            
            CollectionAssert.AreEqual(cert1.PublicKey, cert2.PublicKey);
            CollectionAssert.AreEqual(cert1.SecretKey, cert2.SecretKey);
            Assert.IsTrue(cert1.Equals(cert2));

            /*
            * Test byte constructor
            */
            
            ZCert cert3 = new ZCert(cert1.PublicKey, cert1.SecretKey);

            Assert.AreEqual(cert1.PublicTxt, cert3.PublicTxt);
            Assert.AreEqual(cert1.SecretTxt, cert3.SecretTxt);

            CollectionAssert.AreEqual(cert1.PublicKey, cert3.PublicKey);
            CollectionAssert.AreEqual(cert1.SecretKey, cert3.SecretKey);
            Assert.IsTrue(cert1.Equals(cert3));
        }

        [TestMethod]
        public void TestDuplicate()
        {
            ZCert cert1 = new ZCert();
            ZCert cert2 = ZCert.Dup(cert1);
            Assert.IsTrue(cert1.Equals(cert2));
        }
    }
}

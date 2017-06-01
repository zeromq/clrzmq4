using NUnit.Framework;
namespace ZeroMQTest
{
	[TestFixture]
	public class Z85Test
	{
		[Test]
		public void CurveKeypair()
		{
			byte[] publicKey;
			byte[] secretKey;
			ZeroMQ.Z85.CurveKeypair(out publicKey, out secretKey);

			// TODO: apparently, CurveKeyPair generates Z85 keys... which is confusing as it has byte[] rather than 
			// string output parameters
			Assert.AreEqual(40, publicKey.Length);
			Assert.AreEqual(40, secretKey.Length);
			CollectionAssert.AreNotEqual(publicKey, secretKey);
		}
	}
}

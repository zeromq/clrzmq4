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

			Assert.AreEqual(32, publicKey.Length);
			Assert.AreEqual(32, secretKey.Length);
			CollectionAssert.AreNotEqual(publicKey, secretKey);
		}
	}
}

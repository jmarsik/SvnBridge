using NUnit.Framework;
using SvnBridge.Utility;

namespace Tests
{
    [TestFixture]
    public class XmlEncodeTests
    {
        [Test]
        public void VerifyDecodeCorrectlyDecodesSpecialCharacters()
        {
            string result = Helper.Decode("%20%25&amp;");

            Assert.AreEqual(" %&", result);
        }

        [Test]
        public void VerifyEncodeCorrectlyEncodesSpecialCharacters()
        {
            string result = Helper.Encode(" %&");

            Assert.AreEqual("%20%25&amp;", result);
        }

        [Test]
        public void VerifyDecodeBCorrectlyDecodesSpecialCharacters()
        {
            string result = Helper.Decode("&amp;");

            Assert.AreEqual("&", result);
        }

        [Test]
        public void VerifyEncodeBCorrectlyEncodesSpecialCharacters()
        {
            string result = Helper.Encode("&");

            Assert.AreEqual("&amp;", result);
        }

        [Test]
        public void VerifyEncodeWorksCorrectlyStringWithSpacesAndPercentages()
        {
            string result = Helper.Encode(" % %");

            Assert.AreEqual("%20%25%20%25", result);
        }

        [Test]
        public void VerifyDecodeWorksCorrectlyStringWithSpacesAndPercentages()
        {
            string result = Helper.Decode("%2520");

            Assert.AreEqual("%20", result);
        }
    }
}
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
            string result = Helper.Decode("%20%25%23%5e%7b%5b%7d%5d%3b%60&amp;");

            Assert.AreEqual(" %#^{[}];`&", result);
        }

        [Test]
        public void VerifyEncodeCorrectlyEncodesSpecialCharacters()
        {
            string result = Helper.Encode(" %#^{[}];`&");

            Assert.AreEqual("%20%25%23%5e%7b%5b%7d%5d%3b%60&amp;", result);
        }

        [Test]
        public void VerifyEncodeWithCapitalizeCorrectlyEncodesUsingCapitals()
        {
            string result = Helper.Encode(" %#^{[}];`", true);

            Assert.AreEqual("%20%25%23%5E%7B%5B%7D%5D%3B%60", result);
        }

        [Test]
        public void VerifyEncodeWithCapitalizeDoesNotCapitalizeAmpersandEncoding()
        {
            string result = Helper.Encode("&", true);

            Assert.AreEqual("&amp;", result);
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
        public void VerifyEncodeWorksCorrectlyWithSpacesAndPercentages()
        {
            string result = Helper.Encode(" % %");

            Assert.AreEqual("%20%25%20%25", result);
        }

        [Test]
        public void VerifyDecodeWorksCorrectlyWithSpacesAndPercentages()
        {
            string result = Helper.Decode("%2520");

            Assert.AreEqual("%20", result);
        }

        [Test]
        public void VerifyEncodeCCorrectlyEncodesString()
        {
            string result = Helper.EncodeC(" %#^{[}];`");

            Assert.AreEqual("%20%25%23%5E%7B%5B%7D%5D%3B%60", result);
        }

        [Test]
        public void VerifyEncodeCDoesNotEncodeAmpersand()
        {
            string result = Helper.EncodeC("&");

            Assert.AreEqual("&", result);
        }
    }
}
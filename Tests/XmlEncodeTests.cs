using NUnit.Framework;
using SvnBridge.Utility;

namespace Tests
{
    [TestFixture]
    public class XmlEncodeTests
    {
        [Test]
        public void VerifyDecodeCorrectlyDecodesSpaces()
        {
            string result = Helper.Decode("Test%20With%20Space");

            Assert.AreEqual("Test With Space", result);
        }

        [Test]
        public void VerifyDecodeCorrectlyDecodesPercentage()
        {
            string result = Helper.Decode("Test%25With%25Percentages");

            Assert.AreEqual("Test%With%Percentages", result);
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
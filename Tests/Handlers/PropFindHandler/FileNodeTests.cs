using System.Xml;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using SvnBridge.Nodes;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class FileNodeTests : HandlerTestsBase
    {
        [Test]
        public void VerifyBaselineRelativePathPropertyGetsEncoded()
        {
            XmlDocument xml = new XmlDocument();
            ItemMetaData item = new ItemMetaData();
            item.Name = "A !@#$%^&()_-+={[}];',~`..txt";
            FileNode node = new FileNode(item, null);

            string result = node.GetProperty(xml.CreateElement("baseline-relative-path"));

            Assert.AreEqual(
                "<lp2:baseline-relative-path>A !@#$%^&amp;()_-+={[}];',~`..txt</lp2:baseline-relative-path>", result);
        }

        [Test]
        public void VerifyCheckedInPropertyGetsEncoded()
        {
            XmlDocument xml = new XmlDocument();
            ItemMetaData item = new ItemMetaData();
            item.ItemRevision = 5718;
            item.Name = "A !@#$%^&()_-+={[}];',~`..txt";
            FileNode node = new FileNode(item, null);

            string result = node.GetProperty(xml.CreateElement("checked-in"));

            Assert.AreEqual(
                "<lp1:checked-in><D:href>/!svn/ver/5718/A%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',~%60..txt</D:href></lp1:checked-in>",
                result);
        }
    }
}
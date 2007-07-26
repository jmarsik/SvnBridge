using System.IO;
using System.Text;
using NUnit.Framework;
using SvnBridge;
using SvnBridge.Protocol;
using Assert=CodePlex.NUnitExtensions.Assert;
using SvnBridge.SourceControl;

namespace Tests
{
    [TestFixture]
    public class PropFindTests : WebDavServiceTestsBase
    {
        [Test]
        public void VerifyPropFindOfFolderWithSpaceInNameReturnsDecodedSpaceInBaselineRelativePathProperty()
        {
            mock.Attach(provider.ItemExists, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Folder With Spaces";
            mock.Attach(provider.GetItems, item);
            mock.Attach(provider.IsDirectory, true);
            string path = "/Folder%20With%20Spaces";
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";
            MemoryStream stream = new MemoryStream(Constants.BufferSize);

            service.PropFind(DeserializeRequest<PropFindData>(xml), path, "0", null, stream);
            string result = Encoding.Default.GetString(stream.ToArray());

            string expected = "<lp2:baseline-relative-path>Folder With Spaces</lp2:baseline-relative-path>";
            Assert.NotEqual(-1, result.IndexOf(expected));
        }
    }
}
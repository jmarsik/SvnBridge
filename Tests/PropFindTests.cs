using NUnit.Framework;
using SvnBridge.Protocol;
using Assert=CodePlex.NUnitExtensions.Assert;

namespace Tests
{
    [TestFixture]
    public class PropFindTests : WebDavServiceTestsBase
    {
        [Test]
        public void VerifyPropFindOfFolderWithSpaceInNameReturnsDecodedSpaceInBaselineRelativePathProperty()
        {
            mock.Attach(provider.ItemExists, true);
            mock.Attach(provider.IsDirectory, true);
            string path = "/Folder%20With%20Spaces";
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";

            service.PropFind(DeserializeRequest<PropFindData>(xml), path, "0", null, GetStream());
            string result = ReadStream();

            string expected = "<lp2:baseline-relative-path>Folder With Spaces</lp2:baseline-relative-path>";
            Assert.NotEqual(-1, result.IndexOf(expected));
        }
    }
}
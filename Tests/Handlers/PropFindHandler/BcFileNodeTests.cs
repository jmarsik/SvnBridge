using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using SvnBridge.Nodes;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using Attach;
using SvnBridge.Utility;

namespace SvnBridge.Nodes
{
    [TestFixture]
    public class BcFileNodeTests : HandlerTestsBase
    {
        [Test]
        public void Md5ChecksumShouldUseBcVersionNotLatestVersion()
        {
            ItemMetaData item = new ItemMetaData();
            item.Name = "Foo.txt";
            item.Revision = 1234;
            Results r = mock.Attach(provider.GetItems, item);
            mock.Attach(provider.ReadFile, new byte[4]);
            BcFileNode node = new BcFileNode(1234, item, provider);
            PropFindData propfind = Helper.DeserializeXml<PropFindData>(
                new MemoryStream(
                    Encoding.Default.GetBytes(
                        "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:propfind xmlns:D='DAV:'><D:prop><D:md5-checksum/></D:prop></D:propfind>")));

            node.GetProperty(propfind.Prop.Properties[0]);

            Assert.AreEqual(1234, r.Parameters[0]);
        }
    }
}

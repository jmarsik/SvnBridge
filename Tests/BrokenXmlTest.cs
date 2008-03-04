using System.Xml;
using NUnit.Framework;
using SvnBridge.Infrastructure;

namespace SvnBridge
{
    [TestFixture]
    public class BrokenXmlTest
    {
        [Test]
        public void CanEscapeBrokenXml()
        {
            string brokenXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><D:propertyupdate xmlns:D=\"DAV:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:C=\"http://subversion.tigris.org/xmlns/custom/\" xmlns:S=\"http://subversion.tigris.org/xmlns/svn/\"><D:set><D:prop><C:bugtraq:label>Work Item:</C:bugtraq:label><C:bugtraq:url>http://www.codeplex.com/SvnBridge/WorkItem/View.aspx?WorkItemId=%BUGID%</C:bugtraq:url><C:bugtraq:message> Work Item: %BUGID%</C:bugtraq:message><C:bugtraq:number>true</C:bugtraq:number><C:bugtraq:warnifnoissue>true</C:bugtraq:warnifnoissue></D:prop></D:set></D:propertyupdate>";
            string validXml = BrokenXml.Escape(brokenXml);
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(validXml);
        }
    }
}
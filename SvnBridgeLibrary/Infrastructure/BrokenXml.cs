using System.Text.RegularExpressions;

namespace SvnBridge.Infrastructure
{
    /// <summary>
    /// This is a sadly needed class, because SVN has properties that uses a colon in their names,
    /// and will send that as element names in the XML request for PROPPATCH.
    /// This is not legal XML, and cause issues.
    /// We work around that problem by escapring the colon to __COLON__ and unescaping it when we write back
    /// to the client.
    /// </summary>
    public class BrokenXml
    {
        static readonly Regex findDuplicateNamespacesInTagStart = new Regex(@"<([\w\d]+):([\w\d]+):([\w\d]+)>", RegexOptions.Compiled);
        static readonly Regex findDuplicateNamespacesInTagEnd = new Regex(@"</([\w\d]+):([\w\d]+):([\w\d]+)>", RegexOptions.Compiled);

        public static string Escape(string brokenXml)
        {
            string replaced = findDuplicateNamespacesInTagStart.Replace(brokenXml, "<$1:$2__COLON__$3>");
            return findDuplicateNamespacesInTagEnd.Replace(replaced, "</$1:$2__COLON__$3>");
        }
    }
}
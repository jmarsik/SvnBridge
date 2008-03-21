using System.IO;
using Xunit;

namespace TestsEndToEnd
{
	using System;
	using SvnBridge;
	using SvnBridge.Infrastructure;
	using SvnBridge.Interfaces;
	using SvnBridge.SourceControl;

	/// <summary>
    /// This set of tests are here for scenarios that were found by
    /// users
    /// </summary>
    public class IntegrationsTests : EndToEndTestBase
    {
        /// <summary>
        /// http://www.codeplex.com/SvnBridge/WorkItem/View.aspx?WorkItemId=9315
        /// </summary>
        [Fact]
        public void FailToAddFileOnUpdateAfterAdd()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "blah");
            Svn("add test.txt");
            Svn("commit -m test");
            Svn("update");
        }

        [Fact]
        public void CommitRenamesOfFiles()
        {
            CheckoutAndChangeDirectory();
            for (int i = 0; i < 25; i++)
            {
                File.WriteAllText("test." + i, i.ToString());
            }
            Svn("add test.*");
            Svn("commit -m blah");

            for (int i = 0; i < 25; i += 2)
            {
                Svn("rename test." + i + " ren." + i);
            }

            Svn("commit -m ren");
        }

        [Fact]
        public void CommitRenamesAndModificationOfFiles()
        {
            CheckoutAndChangeDirectory();
            for (int i = 0; i < 25; i++)
            {
                File.WriteAllText("test." + i, i.ToString());
            }
            Svn("add test.*");
            Svn("commit -m blah");

            for (int i = 0; i < 25; i++)
            {
                if (i % 2 == 0)
                    Svn("rename test." + i + " ren." + i);
                else
                    File.WriteAllText("test." + i, "blah");
            }

            Svn("commit -m ren");
        }

    	[Fact]
    	public void WillClearExistingTempWorkSpacesOnCommit()
    	{
    		WriteFile(testPath + "/test1234", "!", true);

			RenameItem(testPath+"/test1234", testPath+"/test5678", false);

			CheckoutAndChangeDirectory();
			File.WriteAllText("test1234", "blah");
			Svn("commit -m test");
    	}
    }
}

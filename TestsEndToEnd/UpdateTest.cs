using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Xunit;

namespace TestsEndToEnd
{
    public class UpdateTest : EndToEndTestBase
    {
        [SvnBridgeFact]
        public void CanUpdateWorkingCopy()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            Svn("commit -m blah");
            WriteFile(testPath + "/test2.txt", "blah", true);
            string output = Svn("update");
            Assert.True(
                output.Contains("A    test2.txt")
                );
        }

        [SvnBridgeFact]
        public void CanUpdateWorkingCopyToPreviousVersion()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            Svn("add test.txt");
            Svn("commit -m blah");
            Svn("update");
            Svn("update test.txt --revision PREV");
        }

        [SvnBridgeFact]
        public void CanUpdateWorkingCopyToPreviousVersion_AndRemoveFolder()
        {
            CheckoutAndChangeDirectory();
            Directory.CreateDirectory("foo");
            File.WriteAllText("foo/test.txt", "hab");
            Svn("add foo");
            Svn("commit -m blah");
            Svn("update");
            Svn("update foo --revision PREV");
        }

        [SvnBridgeFact]
        public void AfterAnErrorWhenGettingFile_WillBeAbleToUpdateAgain()
        {
            CheckoutAndChangeDirectory();

            File.WriteAllText("foo.bar", "blah");

            Svn("add foo.bar");
            Svn("commit -m blah");

            WriteFile(testPath + "/test.txt", "as", true);
            File.WriteAllText("test.txt", "hab");
            ExecuteCommandAndGetError("update");
            File.Delete("test.txt");

            string svn = Svn("update");
            Assert.True(
                Regex.IsMatch(svn,@"^At revision \d+\.\r\n$")
                );
        }


        [SvnBridgeFact]
        public void AfterAnErrorWhenGettingFile_WillBeAbleToUpdateAgain_AndGetModifiedFile()
        {
            CheckoutAndChangeDirectory();

            File.WriteAllText("foo.bar", "blah");

            Svn("add foo.bar");
            Svn("commit -m blah");

            WriteFile(testPath + "/test.txt", "as", true);
            File.WriteAllText("test.txt", "hab");
            ExecuteCommandAndGetError("update");
            File.Delete("test.txt");

            WriteFile(testPath + "/foo.bar", "12312", true);

            Svn("update");

            Assert.Equal("12312", File.ReadAllText("foo.bar"));
        }


        [SvnBridgeFact]
        public void UpdatingFileWhenItIsMissingInWorkingCopy()
        {
            CheckoutAndChangeDirectory();

            File.WriteAllText("foo.bar", "12312");

            Svn("add foo.bar");
            Svn("commit -m blah");

            Svn("propset blah b .");
            Svn("commit -m blah");

            Svn("update foo.bar --revision PREV");

            Svn("update");

            Assert.Equal("12312", File.ReadAllText("foo.bar"));
        }

        [SvnBridgeFact]
        public void UpdatingFolderWhenItIsMissingInWorkingCopy()
        {
            CheckoutAndChangeDirectory();

            Directory.CreateDirectory("foo");

            Svn("add foo");
            Svn("commit -m blah");

            Svn("propset blah b .");
            Svn("commit -m blah");

            Svn("update foo --revision PREV");

            Assert.False(Directory.Exists("foo"));

            Svn("update");

            Assert.True(Directory.Exists("foo"));
        }


        [SvnBridgeFact]
        public void CanGetLatestChangesWhenMovingBackward()
        {
            CheckoutAndChangeDirectory();

            // v 1.0
            File.WriteAllText("test.txt", "hab");
            Svn("add test.txt");
            Svn("commit -m blah");

            // v 2.0
            File.WriteAllText("test2.txt", "hab");
            Svn("add test2.txt");
            Svn("commit -m blah");

            // v 3.0
            File.WriteAllText("test.txt", "hab123");
            Svn("commit -m blah2");

            int previousVersion = _provider.GetLatestVersion() - 1;

            Svn("update");

            Svn("update test.txt --revision " + previousVersion);

            Assert.Equal("hab", File.ReadAllText("test.txt"));
        }

        [SvnBridgeFact]
        public void WhenFileInFolderIsInPreviousVersionAndUpdatingToLatestShouldUpdateFile()
        {
            CheckoutAndChangeDirectory();

            CreateFolder(testPath + "/TestFolder1", true);
            WriteFile(testPath + "/TestFolder1/blah.txt", "abc", true);
            UpdateFile(testPath + "/TestFolder1/blah.txt", "def", true);

            Svn("update");
            Svn("update TestFolder1/blah.txt --revision PREV");
            Assert.Equal("abc", File.ReadAllText("TestFolder1/blah.txt"));
            Svn("update");
            Assert.Equal("def", File.ReadAllText("TestFolder1/blah.txt"));
        }


		[SvnBridgeFact]
		public void UpdateAfterEditAndRenameOperation()
		{
			CheckoutAndChangeDirectory();

			CreateFolder(testPath + "/TestFolder1", true);
			WriteFile(testPath + "/TestFolder1/blah.txt", "abc", true);
			RenameItem(testPath + "/TestFolder1/blah.txt", testPath+"/TestFolder1/blah2.txt", false);
			UpdateFile(testPath + "/TestFolder1/blah2.txt", "bcd", true);

			Svn("update");

			Assert.Equal("bcd", File.ReadAllText("TestFolder1/blah2.txt"));
		}


		[SvnBridgeFact]
		public void UpdateAfterEditThenBackOneVersion()
		{
			CheckoutAndChangeDirectory();

			CreateFolder(testPath + "/TestFolder1", true);
			WriteFile(testPath + "/TestFolder1/blah.txt", "abc", true);
			Svn("update");
			WriteFile(testPath + "/test.txt", "abc", true);
			CreateFolder(testPath + "/TestFolder2", true);
			Svn("update TestFolder1 -r PREV");
		}

		[SvnBridgeFact]
		public void UpdateAfterEditAndMovePathOperation()
		{
			CheckoutAndChangeDirectory();

			CreateFolder(testPath + "/TestFolder1", true);
			WriteFile(testPath + "/TestFolder1/blah.txt", "abc", true);
			Svn("update");
			WriteFile(testPath + "/test.txt", "abc", true);
			CreateFolder(testPath + "/TestFolder2", true);
			Svn("update");

			RenameItem(testPath + "/TestFolder1/blah.txt", testPath + "/TestFolder2/blah.txt", true);

			Svn("propset test file .");

			// the root directory will now be at the head revision, but the other 
			// directories are not updated, so we have different versions
			Svn("commit -m \"force different versions in directories\" ");

			UpdateFile(testPath + "/TestFolder1/blah.txt", "143", true);
			UpdateFile(testPath + "/TestFolder2/blah.txt", "bcd", true);
			UpdateFile(testPath + "/TestFolder1/blah.txt", "cvb", true);


			Svn("update");

			Assert.Equal("bcd", File.ReadAllText("TestFolder2/blah.txt"));
		}

		[SvnBridgeFact]
		public void UpdateFileInSubSubDirectoryThenUpdateRepositoryWillUpdateAllRevisions()
		{
			CheckoutAndChangeDirectory();

			CreateFolder(testPath + "/trunk", true);
			WriteFile(testPath + "/test.txt", "blah", true);
			CreateFolder(testPath + "/trunk/b", true);
			WriteFile(testPath + "/trunk/test.txt", "blah", true);
			WriteFile(testPath + "/trunk/b/asdf.txt", "blah", true);

			Svn("update");

			File.WriteAllText("trunk/b/asdf.txt", "adsa");

			Svn("commit trunk/b/asdf.txt -m test");
			Svn("update");
			XmlDocument xml = SvnXml("info --xml -R");
			int version = base._provider.GetLatestVersion();
			foreach (XmlNode node in xml.SelectNodes("/info/entry/@revision"))
			{
				Assert.Equal(version, int.Parse(node.Value));
			}

		}
    }
}

using System;
using System.IO;
using Xunit;

namespace TestsEndToEnd
{
    public class CommitTest : EndToEndTestBase
    {
        [Fact]
        public void CanCommitNewFile()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            Svn("add test.txt");
            string command = Svn("commit -m blah");
            Assert.True(
                command.Contains("Committed")
                );
        }

        [Fact]
        public void CanCommitBigFile()
        {
            CheckoutAndChangeDirectory();
            GenerateFile();
            string originalPath = Path.GetFullPath("test.txt");
            Svn("add test.txt");
            Svn("commit -m \"big file\" ");

            CheckoutAgainAndChangeDirectory();
            string newPath = Path.GetFullPath("test.txt");
            string actual = File.ReadAllText(newPath);
            string expected = File.ReadAllText(originalPath);
            Assert.Equal(expected, actual);
        }

        private static void GenerateFile()
        {
            int lines = 1024 * 10;
            using (TextWriter writer = File.CreateText("test.txt"))
            {
                for (int i = 0; i < lines; i++)
                {
					int lineWidth = 128;
                    string [] items = new string[lineWidth];
                    for (int j = 0; j < lineWidth; j++)
                    {
                        items[j] = (j*i).ToString();
                    }
                    writer.WriteLine(string.Join(", ", items));
                }
                writer.Flush();
            }
        }
    }
}

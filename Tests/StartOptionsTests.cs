using System;
using NUnit.Framework;
using Assert = CodePlex.NUnitExtensions.Assert;

namespace SvnBridge
{
    [TestFixture]
    public class StartOptionsTests
    {
        [Test]
        public void TestInvalidArgumentsWithHideGuiFlag()
        {
            string[] commandLineArguments = new string[4] { "foo", "/gui-", "foo", "foo" };

            try
            {
                new StartOptions(commandLineArguments);
                NUnit.Framework.Assert.Fail();
            }
            catch(StartOptionsException ex)
            {
                Assert.False(ex.DisplayGui);
            }
        }

        [Test]
        public void TestNullCommandLineArgumentThrowsEx()
        {
            Assert.Throws<ArgumentNullException>(delegate { new StartOptions(null); });
        }

        [Test]
        public void TestInvalidArgumentsWithDisplayGuiFlag()
        {
            string[] commandLineArguments = new string[4] { "foo", "/gui", "foo", "foo" };

            try
            {
                new StartOptions(commandLineArguments);
                NUnit.Framework.Assert.Fail();
            }
            catch (StartOptionsException ex)
            {
                Assert.True(ex.DisplayGui);
            }
        }

        [Test]
        public void TestInvalidArgumentsWithNoGuiFlag()
        {
            string[] commandLineArguments = new string[4] { "foo", "foo", "foo", "foo" };

            try
            {
                new StartOptions(commandLineArguments);
                NUnit.Framework.Assert.Fail();
            }
            catch (StartOptionsException ex)
            {
                Assert.True(ex.DisplayGui);
            }
        }

        [Test]
        public void TestEmptyCommandLineArguments()
        {
            string[] commandLineArguments = new string[0] {};

            StartOptions startOptions = new StartOptions(commandLineArguments);

            Assert.True(startOptions.DisplayGui);
            Assert.Equal(0, startOptions.Port);
            Assert.Equal(null, startOptions.TfsServerUrl);
        }

        [Test]
        public void TestOnlyShowGuiFlag()
        {
            string[] commandLineArguments = new string[1] { "/gui" };

            StartOptions startOptions = new StartOptions(commandLineArguments);

            Assert.True(startOptions.DisplayGui);
        }

        [Test]
        public void TestOnlyShortShowGuiFlag()
        {
            string[] commandLineArguments = new string[1] { "/g" };

            StartOptions startOptions = new StartOptions(commandLineArguments);

            Assert.True(startOptions.DisplayGui);
        }

        [Test]
        public void TestOnlyHideGuiFlag()
        {
            string[] commandLineArguments = new string[1] { "/gui-" };

            StartOptions startOptions = new StartOptions(commandLineArguments);

            Assert.False(startOptions.DisplayGui);
        }

        [Test]
        public void TestOnlyShortHideGuiFlag()
        {
            string[] commandLineArguments = new string[1] { "/g-" };

            StartOptions startOptions = new StartOptions(commandLineArguments);

            Assert.False(startOptions.DisplayGui);
        }

        [Test]
        public void TestShowGuiFlagAsThirdArg()
        {
            string[] commandLineArguments = new string[3] { "8081", "http://foo", "/gui" };

            StartOptions startOptions = new StartOptions(commandLineArguments);

            Assert.True(startOptions.DisplayGui);
        }

        [Test]
        public void TestHideGuiFlagAsThirdArg()
        {
            string[] commandLineArguments = new string[3] { "8081", "http://foo", "/gui-" };

            StartOptions startOptions = new StartOptions(commandLineArguments);

            Assert.False(startOptions.DisplayGui);
        }

        [Test]
        public void TestValidPort()
        {
            int expected = 8081;
            string[] commandLineArguments = new string[2] { "8081", "http://foo" };

            StartOptions startOptions = new StartOptions(commandLineArguments);

            Assert.Equal(expected, startOptions.Port);
        }

        [Test]
        public void TestNonNumericPort()
        {
            string[] commandLineArguments = new string[2] { "foo", "http://foo" };

            Assert.Throws<StartOptionsException>(delegate { new StartOptions(commandLineArguments); });
        }

        [Test]
        public void TestPortZero()
        {
            string[] commandLineArguments = new string[2] { "0", "http://foo" };

            Assert.Throws<StartOptionsException>(delegate { new StartOptions(commandLineArguments); });
        }

        [Test]
        public void TestMaxPortPlusOne()
        {
            int port = Constants.MaxPort + 1;
            string[] commandLineArguments = new string[2] { port.ToString(), "http://foo" };

            Assert.Throws<StartOptionsException>(delegate { new StartOptions(commandLineArguments); });
        }

        [Test]
        public void TestValidTfsServerUrl()
        {
            string expected = "http://foo/";
            string[] commandLineArguments = new string[2] { "8081", "http://foo/" };

            StartOptions startOptions = new StartOptions(commandLineArguments);

            Assert.Equal(expected, startOptions.TfsServerUrl);
        }

        [Test]
        public void TestInvalidTfsServerUrl()
        {
            string[] commandLineArguments = new string[2] { "8081", "foo" };

            Assert.Throws<StartOptionsException>(delegate { new StartOptions(commandLineArguments); });
        }
    }
}

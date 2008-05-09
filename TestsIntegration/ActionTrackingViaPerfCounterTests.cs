using SvnBridge.Handlers;
using SvnBridge.Infrastructure.Statistics;
using Xunit;

namespace IntegrationTests
{
    public class ActionTrackingViaPerfCounterTests
    {
        private readonly IActionTracking actionTracking;

        public ActionTrackingViaPerfCounterTests()
        {
            actionTracking = new ActionTrackingViaPerfCounter();
        }

        [Fact]
        public void CanTrackRequest()
        {
            if (ActionTrackingViaPerfCounter.Enabled == false)
                return;
            long numberOfGets = actionTracking.GetStatistics()["Get"];
            actionTracking.Request(new GetHandler());
            Assert.True(numberOfGets < actionTracking.GetStatistics()["Get"]);
        }

        [Fact]
        public void CanTrackError()
        {
            if (ActionTrackingViaPerfCounter.Enabled == false)
                return;
            long numberOfGets = actionTracking.GetStatistics()["Errors"];
            actionTracking.Error();
            Assert.True(numberOfGets < actionTracking.GetStatistics()["Errors"]);
        }
    }
}
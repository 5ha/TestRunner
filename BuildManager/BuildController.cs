using Common;
using Model;
using System.Threading.Tasks;

namespace BuildManager
{
    public class BuildController
    {
        public async Task KickOffBuild(BuildRunRequest request, ITestRunObserver observer = null)
        {
            using (var processor = new BuildProcessor("my-rabbit", "remote", "remote", @"C:\Users\shawn\source\repos\TestNUnitRunner\Publish", @"C:\Users\shawn\source\repos\TestNUnitRunner\Publish\SystemUnderTest", "shawnseabrook", "myimage", request.Build))
            {
                ToConsoleObserver consoleObserver = new ToConsoleObserver();
                ToLogObserver logObserver = new ToLogObserver("BuildManager");

                if (observer != null)
                {
                    processor.SubscribeTestResult(observer);
                    processor.SubscribeStatusMessage(observer);
                }

                processor.SubscribeTestResult(consoleObserver);
                processor.SubscribeStatusMessage(consoleObserver);

                processor.SubscribeTestResult(logObserver);
                processor.SubscribeStatusMessage(logObserver);

                await processor.StartBuild(request);
            }
        }
    }
}

using MessageModels;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildManager
{
    public class BuildController
    {
        public async Task KickOffBuild(string build, ITestRunObserver observer = null)
        {
            if (observer == null)  observer = new ToConsoleObserver();
            using (var processor = new BuildProcessor("my-rabbit", "remote", "remote", @"C:\Users\shawn\source\repos\TestNUnitRunner\Publish", @"C:\Users\shawn\source\repos\TestNUnitRunner\Publish\SystemUnderTest", "shawnseabrook", "myimage", build))
            {
                processor.SubscribeTestResult(observer);
                processor.SubscribeStatusMessage(observer);

                await processor.StartBuild(build);
            }
        }
    }
}

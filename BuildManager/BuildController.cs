using Common;
using Model;
using System.Threading;
using System.Threading.Tasks;

namespace BuildManager
{
    public class BuildController
    {
        private readonly string _host;
        private readonly string _vHost;
        private readonly string _username;
        private readonly string _password;

        public BuildController(string host, string vHost, string username, string password)
        {
            _host = host;
            _vHost = vHost;
            _username = username;
            _password = password;
        }
        public async Task KickOffBuild(BuildRunRequest request, ITestRunObserver observer = null)
        {
            using (var processor = new BuildProcessor(_host, _vHost, _username, _password))
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

using DockerUtilities;
using JobModels;
using Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestAnalyser.Services
{
    public interface ITestListService
    {
        Task<List<TestInfo>> ListTests(string image, string command);
    }

    public class TestListService : ITestListService
    {
        private readonly ILogger<TestListService> _logger;
        private readonly IDockerWrapper _dockerWrapper;

        public TestListService(ILogger<TestListService> logger, IDockerWrapper dockerWrapper)
        {
            _logger = logger;
            _dockerWrapper = dockerWrapper;
        }

        public async Task<List<TestInfo>> ListTests(string image, string command)
        {
            Dictionary<string, string> environmentVariable = new Dictionary<string, string>
                    {
                        { "TESTER_LISTTESTS", "true"}
                    };

            string testOutput = await _dockerWrapper.Run(image, environmentVariables: environmentVariable, command: command);

            List<TestInfo> tests = new List<TestInfo>();

            string[] testNames = testOutput.Split('\n');

            foreach (string testName in testNames)
            {
                if (!string.IsNullOrEmpty(testName))
                {
                    TestInfo item = new TestInfo
                    {
                        FullName = testName.Trim()
                    };

                    tests.Add(item);
                }
            }

            return tests;
        }
    }
}

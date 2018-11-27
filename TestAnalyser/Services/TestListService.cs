using DockerUtilities;
using Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestAnalyser.Services
{
    public interface ITestListService
    {
        Task<List<RunTest>> ListTests(string build, string image, string command);
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

        public async Task<List<RunTest>> ListTests(string build, string image, string command)
        {
            Dictionary<string, string> environmentVariable = new Dictionary<string, string>
                    {
                        { "TESTER_LISTTESTS", "true"}
                    };

            string testOutput = await _dockerWrapper.Run(image, environmentVariables: environmentVariable, command: command);

            List<RunTest> tests = new List<RunTest>();

            string[] testNames = testOutput.Split('\n');

            foreach (string testName in testNames)
            {
                if (!string.IsNullOrEmpty(testName))
                {
                    RunTest item = new RunTest
                    {
                        Build = build,
                        FullName = testName.Trim()
                    };

                    tests.Add(item);
                }
            }

            return tests;
        }
    }
}

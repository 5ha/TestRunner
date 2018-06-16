using BuildManager.Model;
using DockerUtils;
using HiQ.Builders;
using HiQ.Interfaces;
using MessageModels;
using NUnitUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BuildManager
{
    public class BuildProcessor : IDisposable
    {
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly string _pathToBuildFolder;
        private readonly string _testSearchDirectory;
        private readonly string _repository;
        private readonly string _image;
        private readonly string _build;
        private TestResultMonitor _monitor;
        private Dictionary<string,string> _expectedTests;
        private bool _allReceived = false;

        CancellationTokenSource _cancellationTokenSource;

        ISender testInstructionSender;
        ISender buildInstructionSender;
        IReceiver testResultReceiver;


        public BuildProcessor(string host, string username, string password, string pathToBuildFolder, string testSearchDirectory, string repository, string image, string build)
        {
            _monitor = new TestResultMonitor();
            _host = host;
            _username = username;
            _password = password;
            _pathToBuildFolder = pathToBuildFolder;
            _testSearchDirectory = testSearchDirectory;
            _repository = repository;
            _image = image;
            _build = build;
            _cancellationTokenSource = new CancellationTokenSource();
            _expectedTests = new Dictionary<string, string>();
        }

        public async Task StartBuild(string build)
        {
            // TODO: What if current build already exists
            Console.WriteLine($"[{build}] Staring");
            // Create the docker image and add it to the repository
            var containerHelper = new ContainerHelper();
            var imageBuildResult = await containerHelper.BuildImage(_pathToBuildFolder, _repository, _image, _build);
            Console.WriteLine("[{build}] Image Build Result: ");
            Console.WriteLine(imageBuildResult);
            // TODO: Add to image repository

            // Add the build to the database

            // Add all the tests to the database
            List<RunTest> tests;
            using (TestExplorer testExplorer = new TestExplorer())
            {
                tests = testExplorer.GetTests(build, _testSearchDirectory);
            }
                
            AddTestsToDictionary(_expectedTests, tests);

            // Add all the tests to the queue
            Console.WriteLine($"[{build}] Sending Test Instructions ...");
            testInstructionSender = ConfigureSender($"{build}_request");
            SendTestInstructions(testInstructionSender, tests);
            Console.WriteLine($"[{build}] Test Instructions sent.");

            // Add the build instruction to the queue
            Console.WriteLine($"[{build}] Sending Build Instruction ...");
            buildInstructionSender = ConfigureSender("build_queue");
            buildInstructionSender.Send(CreateBuildInstruction(build));
            Console.WriteLine($"[{build}] Build Instruction sent.");


            testResultReceiver = ConfigureReceiver($"{build}_response");

            // Subscribe to the test result queue until all the tests have been completed (notifying subscribers)
            testResultReceiver.Receive<TestResult>(TestResultReceived);

            // Wait for tests to complete
            await TestsStillRunning(_cancellationTokenSource.Token);

            // Flag the build as completed notifying subscribers

            Console.WriteLine($"[{build}] All complete.");
        }

        private Task TestsStillRunning(CancellationToken cancellationToken)
        {
            return Task.Run(async () => {
                while (!_allReceived && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                }
                return Task.CompletedTask;
            });
        }

        private void TestResultReceived(TestResult result)
        {
            // Add the result to the database

            if (_expectedTests.ContainsKey(result.FullName))
            {
                _expectedTests.Remove(result.FullName);
            }


            // Notify subscribers

            // Check if all results have been received
            if (_expectedTests.Count == 0)
            {
                _allReceived = true; // this should be the only thread writing to that value

                
            }
        }

        private void AddTestsToDictionary(Dictionary<string, string> dict, List<RunTest> tests)
        {
            foreach(var test in tests)
            {
                dict.Add(test.FullName, test.FullName);
            }
        }

        private void SendTestInstructions(ISender sender, List<RunTest> tests)
        {
            foreach (var test in tests)
            {
                sender.Send(test);
            }
        }

        private ISender ConfigureSender(string queue)
        {
            IQueueBuilder queueBuilder = new RabbitBuilder();
            ISender sender = queueBuilder.ConfigureTransport(_host, _username, _password)
                .ISendTo(queue)
                .Build();
            return sender;
        }

        private IReceiver ConfigureReceiver(string queue)
        {
            IQueueBuilder queueBuilder = new RabbitBuilder();
            IReceiver receiver = queueBuilder.ConfigureTransport(_host, _username, _password)
                .IReceiveFrom(queue)
                .IReceiveForever()
                .Build();
            return receiver;
        }

        private RunBuild CreateBuildInstruction(string build)
        {
            List<string> commands = new List<string>
            {
                "TestRunner", "my-rabbit", "remote", "remote", $"{build}_request", $"{build}_response", "c:\\app"
            };

            string containerImage = string.Format("{0}/{1}:{2}", _repository, _image, build);

            return new RunBuild { Build = build, Commands = commands, ContainerImage = containerImage };
        }

        public IDisposable Subscribe(IObserver<TestExecutionResult> observer)
        {
            return _monitor.Subscribe(observer);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            if(testInstructionSender != null)
            {
                testInstructionSender.DeleteQueue();
                testInstructionSender.Dispose();
            }
            if(buildInstructionSender != null)
            {
                // Don't delete this queue, it should be permanent
                buildInstructionSender.Dispose();
            }
            if(testResultReceiver != null)
            {
                testResultReceiver.DeleteQueue();
                testResultReceiver.Dispose();
            }
        }
    }
}

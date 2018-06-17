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
        private TestResultMonitor _testResultMonitor;
        private StatusMessageMonitor _statusMessageMonitor;
        private Dictionary<string, string> _expectedTests;
        private bool _allReceived = false;

        CancellationTokenSource _cancellationTokenSource;

        ISender testInstructionSender;
        ISender buildInstructionSender;
        IReceiver testResultReceiver;
        IReceiver statusMessageReceiver;

        NunitParser _parser;


        public BuildProcessor(string host, string username, string password, string pathToBuildFolder, string testSearchDirectory, string repository, string image, string build)
        {
            _testResultMonitor = new TestResultMonitor();
            _statusMessageMonitor = new StatusMessageMonitor();

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

            _parser = new NunitParser();
        }

        public async Task StartBuild(string build)
        {
            // TODO: What if current build already exists

            ReportStatus($"[{build}] Staring");

            // Create the docker image and add it to the repository
            var containerHelper = new ContainerHelper();
            var imageBuildResult = await containerHelper.BuildImage(_pathToBuildFolder, _repository, _image, _build);

            ReportStatus("[{build}] Image Build Result: ");
            ReportStatus(imageBuildResult);
            // TODO: Add to image repository

            // Add the build to the database

            // Add all the tests to the database
            List<RunTest> tests;
            using (TestExplorer testExplorer = new TestExplorer())
            {
                tests = testExplorer.GetTests(build, _testSearchDirectory);
            }

            AddTestsToDictionary(_expectedTests, tests);

            // Configure receivers
            statusMessageReceiver = ConfigureReceiver(QueueNames.Status(build));
            testResultReceiver = ConfigureReceiver(QueueNames.TestResponse(build));

            // Add all the tests to the queue
            ReportStatus($"[{build}] Sending Test Instructions ...");
            testInstructionSender = ConfigureSender(QueueNames.TestRequest(build));
            SendTestInstructions(testInstructionSender, tests);
            ReportStatus($"[{build}] Test Instructions sent.");

            // Add the build instruction to the queue
            ReportStatus($"[{build}] Sending Build Instruction ...");
            buildInstructionSender = ConfigureSender(QueueNames.Build());
            buildInstructionSender.Send(CreateBuildInstruction(build));
            ReportStatus($"[{build}] Build Instruction sent.");

            // Subscribe to the test result queue until all the tests have been completed (notifying subscribers)
            testResultReceiver.Receive<TestResult>(TestResultReceived);
            statusMessageReceiver.Receive<StatusMessage>(StatusMessageReceived);

            // Wait for tests to complete
            await TestsStillRunning(_cancellationTokenSource.Token);

            // Notify cubscribers that the run is complete
            _testResultMonitor.notifyComplete();

            ReportStatus($"[{build}] All complete.");
            _statusMessageMonitor.notifyComplete();
        }

        private Task TestsStillRunning(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                while (!_allReceived && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                }
                return Task.CompletedTask;
            });
        }

        private void StatusMessageReceived(StatusMessage message)
        { 
            _statusMessageMonitor.notifyNext(message);

            // if there is an error in the message stop everything because we have no guarantee
            // that we will get all test results back
            if (!string.IsNullOrEmpty(message.Error))
            {
                _allReceived = true; // TODO:  bit of a hack for the moment
            }
        }

        private void TestResultReceived(TestResult result)
        {
            // TODO: Add the result to the database

            TestExecutionResult testExecutionResult = _parser.Parse(result);

            // Notify subscribers of the result
            _testResultMonitor.notifyNext(testExecutionResult);

            if (_expectedTests.ContainsKey(result.FullName))
            {
                _expectedTests.Remove(result.FullName);
            }

            // Check if all results have been received
            if (_expectedTests.Count == 0)
            {
                _allReceived = true; // this should be the only thread writing to that value
            }
        }

        private void AddTestsToDictionary(Dictionary<string, string> dict, List<RunTest> tests)
        {
            foreach (var test in tests)
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
                "TestRunner", "my-rabbit", "remote", "remote", QueueNames.TestRequest(build), QueueNames.TestResponse(build), "c:\\app"
            };

            string containerImage = string.Format("{0}/{1}:{2}", _repository, _image, build);

            return new RunBuild { Build = build, Commands = commands, ContainerImage = containerImage };
        }

        public IDisposable SubscribeTestResult(IObserver<TestExecutionResult> observer)
        {
            return _testResultMonitor.Subscribe(observer);
        }

        public IDisposable SubscribeStatusMessage(IObserver<StatusMessage> observer)
        {
            return _statusMessageMonitor.Subscribe(observer);
        }

        private StatusMessage PrepareStatusMessage()
        {
            StatusMessage message = new StatusMessage
            {
                Machine = Environment.MachineName,
                Application = "BuildManager"
            };
            return message;
        }

        private void ReportStatus(string message)
        {
            StatusMessage m = PrepareStatusMessage();
            m.Message = message;
            _statusMessageMonitor.notifyNext(m);
        }

        private void ReportError(string message)
        {
            StatusMessage m = PrepareStatusMessage();
            m.Error = message;
            _statusMessageMonitor.notifyNext(m);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            if (testInstructionSender != null)
            {
                testInstructionSender.DeleteQueue();
                testInstructionSender.Dispose();
            }
            if (buildInstructionSender != null)
            {
                // Don't delete this queue, it should be permanent
                buildInstructionSender.Dispose();
            }
            if (testResultReceiver != null)
            {
                testResultReceiver.DeleteQueue();
                testResultReceiver.Dispose();
            }
            if(statusMessageReceiver != null)
            {
                statusMessageReceiver.DeleteQueue();
                statusMessageReceiver.Dispose();
            }
        }
    }
}

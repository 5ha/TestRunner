using BuildManager.Model;
using Docker.DotNet.Models;
using DockerUtils;
using HiQ.Builders;
using HiQ.Interfaces;
using MessageModels;
using Model;
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

        public async Task StartBuild(BuildRunRequest request)
        {
            try
            {
                // TODO: What if current build already exists

                ReportStatus($"[{request.Build}] Staring");

                
                var containerHelper = new ContainerHelper();

                // Pull the image
                await containerHelper.PullImage(request.Image, ReportStatus, ReportError, ReportStatus);

                List<RunTest> tests = new List<RunTest>();

                // Get the tests from the docker image

                CreateContainerResponse createContainerResponse = await containerHelper.CreateContainer(request.Image, request.Build, new List<string> { "ListTests" });

                await containerHelper.StartContainer(createContainerResponse.ID);

                string testOutput = await containerHelper.AttachContainer(createContainerResponse.ID);
                string[] testNames = testOutput.Split('\n');

                foreach(string testName in testNames)
                {
                    RunTest item = new RunTest
                    {
                        Build = request.Build,
                        FullName = testName.Trim()
                    };

                    tests.Add(item);
                }

                AddTestsToDictionary(_expectedTests, tests);

                // Configure receivers
                statusMessageReceiver = ConfigureReceiver(QueueNames.Status(request.Build));
                testResultReceiver = ConfigureReceiver(QueueNames.TestResponse(request.Build));

                // Add all the tests to the queue
                ReportStatus($"[{request.Build}] Sending Test Instructions ...");
                testInstructionSender = ConfigureSender(QueueNames.TestRequest(request.Build));
                SendTestInstructions(testInstructionSender, tests);
                ReportStatus($"[{request.Build}] Test Instructions sent.");

                // Add the build instruction to the queue
                ReportStatus($"[{request.Build}] Sending Build Instruction ...");
                buildInstructionSender = ConfigureSender(QueueNames.Build());
                buildInstructionSender.Send(CreateBuildInstruction(request));
                ReportStatus($"[{request.Build}] Build Instruction sent.");

                // Subscribe to the test result queue until all the tests have been completed (notifying subscribers)
                testResultReceiver.Receive<TestResult>(TestResultReceived);
                statusMessageReceiver.Receive<StatusMessage>(StatusMessageReceived);

                // Wait for tests to complete
                await TestsStillRunning(_cancellationTokenSource.Token);

                ReportStatus($"DONE");

                // Notify cubscribers that the run is complete
                _testResultMonitor.notifyComplete();
                _statusMessageMonitor.notifyComplete();
            } catch (Exception ex)
            {
                ReportError(ex.Message);
            }
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
            Console.WriteLine($"{ message.Error}{message.Warning}{message.Message}");
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

            Console.WriteLine($"TEST RESULT : {result.FullName}");

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

        private RunBuild CreateBuildInstruction(BuildRunRequest request)
        {
            List<string> commands = new List<string>
            {
                "RunTests", 
                QueueNames.TestRequest(request.Build),
                QueueNames.TestResponse(request.Build)
            };

            return new RunBuild { Build = request.Build, Commands = commands, ContainerImage = request.Image };
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

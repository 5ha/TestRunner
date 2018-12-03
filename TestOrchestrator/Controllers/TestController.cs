using DataService.Entities;
using DataService.Services;
using JobModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestOrchestrator.Models;
using TestOrchestrator.Services;
using System.Linq;

namespace TestOrchestrator.Controllers
{
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IJobService _jobService;
        private readonly IQueues _queues;
        private readonly ITestListService _testListService;
        private readonly IOptions<ComposerSettings> _composerSettings;
        private readonly IComposerService _composerService;

        public TestController(ILogger<TestController> logger, IJobService jobService, IQueues queues, ITestListService testListService, 
            IOptions<ComposerSettings> composerSettings, IComposerService composerService)
        {
            _logger = logger;
            _jobService = jobService;
            _queues = queues;
            _testListService = testListService;
            _composerSettings = composerSettings;
            _composerService = composerService;
        }

        [HttpGet("/")]
        public ActionResult<string> Index()
        {
            var x = _composerSettings;

            return "Success";
        }

        [HttpPost("/createjob")]
        public ActionResult<string> InitiateJob([FromBody]StartJobRequest request)
        {
            if(request == null)
            {
                throw new ArgumentException("Invalid request");
            }
            // Get a list of tests
            //List<TestInfo> tests = await _testListService.ListTests(new TestRequestDescription { TestRunCommand = request.TestRunCommand, TestRunImage = request.TestRunImage});

            List<TestInfo> tests = new List<TestInfo>
            {
                new TestInfo { FullName = "WebTests.NavigateFeature.NavigateToSpecflow"}
            };
            // Create the job
            Job job = _jobService.CreateJob("NONE", tests);

            // Enqueue the tests
            _queues.EnqueueTests(job);

            // Call the compose endpoints
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _composerService.RunCompose(request, job.JobId).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            // Return the polling url
            return PollUrl(job.JobId, 0);
        }

        [HttpGet("/poll/{jobId}/{lastTestRequestId}")]
        public ActionResult<JobResult> GetResults(int jobId, int lastTestRequestId = 0)
        {
            JobResult result = _jobService.GetResults(jobId, lastTestRequestId);

            if (!result.IsComplete)
            {
                int lastId = result.JobTestResults.OrderByDescending(x => x.TestRequestId).FirstOrDefault()?.TestRequestId ?? 0;
                result.NextResult = PollUrl(jobId, lastId);
            }

            return result;
        }

        private string PollUrl(int jobId, int testRequestId)
        {
            return Url.Action("GetResults", "Test", new { jobId = jobId, lastTestRequestId = testRequestId },"http");
        }
    }
}
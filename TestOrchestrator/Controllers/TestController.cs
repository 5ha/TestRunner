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
        public async Task<string> InitiateJob([FromBody]StartJobRequest request)
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
            _composerService.RunCompose(request, job.JobId).ConfigureAwait(false);

            // Return the polling url
            return "Started";
        }

        //[HttpPost("/createjob")]
        //public ActionResult<int> CreateJob([FromBody]CreateJobRequest request)
        //{
        //    Job job = _jobService.CreateJob(request.Description, request.Tests);

        //    _queues.EnqueueTests(job);

        //    return job.JobId;
        //}
    }
}

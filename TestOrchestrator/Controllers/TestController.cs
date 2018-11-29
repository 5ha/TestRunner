using DataService.Entities;
using DataService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestOrchestrator.Models;
using TestOrchestrator.Services;

namespace TestOrchestrator.Controllers
{
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IJobService _jobService;
        private readonly IQueues _queues;

        public TestController(ILogger<TestController> logger, IJobService jobService, IQueues queues)
        {
            _logger = logger;
            _jobService = jobService;
            _queues = queues;
        }

        [HttpPost("/createjob")]
        public ActionResult<int> CreateJob([FromBody]CreateJobRequest request)
        {
            Job job = _jobService.CreateJob(request.Description, request.Tests);

            _queues.EnqueueTests(job);

            return job.JobId;
        }
    }
}

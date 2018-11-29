using DataService.Entities;
using DataService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TestOrchestrator.Models;

namespace TestOrchestrator.Controllers
{
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IJobService _jobService;

        public TestController(ILogger<TestController> logger, IJobService jobService)
        {
            _logger = logger;
            _jobService = jobService;
        }

        [HttpPost("/createjob")]
        public ActionResult<int> CreateJob([FromBody]CreateJobRequest request)
        {
            Job job = _jobService.CreateJob(request.Description, request.Tests);

            return job.JobId;
        }
    }
}

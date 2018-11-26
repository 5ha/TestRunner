using ComposeManager.Services;
using JobModels;
using Microsoft.AspNetCore.Mvc;

namespace ComposeManager.Controllers
{
    public class ManagerController : ControllerBase
    {
        private readonly IJobRunnerService _jobRunnerService;

        public ManagerController(IJobRunnerService jobRunnerService)
        {
            _jobRunnerService = jobRunnerService;
        }

        [HttpGet("/health")]
        public ActionResult<string> Index()
        {
            return "SUCCESS";

        }

        // Start a job
        [HttpPost("/start")]
        public ActionResult<string> StartJob([FromBody]JobDescription jobDescription)
        {
            _jobRunnerService.RunJob(jobDescription);
            return $"STARTED: {jobDescription.Build}";
        }
    }
}

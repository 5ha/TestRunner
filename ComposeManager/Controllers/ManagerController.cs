using ComposeManager.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ComposeManager.Controllers
{
    public class ManagerController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public ManagerController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        [HttpGet("/health")]
        public ActionResult<string> Index()
        {
            var runner = (IJobRunner)_serviceProvider.GetService(typeof(IJobRunner));

            return runner.GetType().FullName;
            return "SUCCESS";
        }

        // Start a job
        [HttpGet("/start")]
        public ActionResult<string> StartJob()
        {
            // Kill all current processes

            // Start the new job

            return "";
        }

        
        [HttpGet("/inprogress")]
        public ActionResult<bool> inprogress()
        {
            // return whether a job is running
            return true;
        }
    }
}

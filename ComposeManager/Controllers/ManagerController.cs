using Microsoft.AspNetCore.Mvc;

namespace ComposeManager.Controllers
{
    public class ManagerController : ControllerBase
    {
        [HttpGet("/health")]
        public ActionResult<string> Index()
        {
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

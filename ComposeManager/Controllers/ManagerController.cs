using Microsoft.AspNetCore.Mvc;

namespace ComposeManager.Controllers
{
    public class ManagerController : ControllerBase
    {
        [HttpGet("/healthcheck")]
        public ActionResult<string> Index()
        {
            return "SUCCESS";
        }
    }
}

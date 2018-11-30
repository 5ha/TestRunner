using JobModels;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestAnalyser.Services;

namespace TestAnalyser.Controllers
{
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly ITestListService _testListService;

        public TestController(ILogger<TestController> logger, ITestListService testListService)
        {
            _logger = logger;
            _testListService = testListService;
        }

        [HttpPost("/listtests")]
        public async Task<ActionResult<List<TestInfo>>> ListTests([FromBody]TestRequestDescription request)
        {
            return await _testListService.ListTests(request.TestRunImage, request.TestRunCommand);
        }
    }
}

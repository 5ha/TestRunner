using JobModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TestOrchestrator.Models;

namespace TestOrchestrator.Services
{
    public interface ITestListService
    {
        Task<List<TestInfo>> ListTests(TestRequestDescription request);
    }

    public class TestListService : ITestListService
    {
        private readonly IOptions<TestAnalyserSettings> _settings;

        public TestListService(IOptions<TestAnalyserSettings> settings)
        {
            _settings = settings;
        }

        public async Task<List<TestInfo>> ListTests(TestRequestDescription request)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(_settings.Value.Url, request);
                return JsonConvert.DeserializeObject<List<TestInfo>>(response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}

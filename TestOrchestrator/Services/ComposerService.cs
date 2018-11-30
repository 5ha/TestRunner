using CommonModels.Config;
using DataService.Services;
using JobModels;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TestOrchestrator.Models;

namespace TestOrchestrator.Services
{
    public interface IComposerService
    {
        Task RunCompose(StartJobRequest request, int jobId);
    }

    public class ComposerService : IComposerService
    {
        private readonly IOptions<ComposerSettings> _composerSettings;
        private readonly IOptions<QueueSettings> _queueSettings;
        private readonly IJobService _jobService;

        public ComposerService(IOptions<ComposerSettings> composerSettings, IOptions<QueueSettings> queueSettings, IJobService jobService)
        {
            _composerSettings = composerSettings;
            _queueSettings = queueSettings;
            _jobService = jobService;
        }

        public async Task RunCompose(StartJobRequest request, int jobId)
        {
            JobDescription jobDescription = CreateJobDescription(request, jobId);

            //return Task.Run(() => {

            List<Task<HttpResponseMessage>> calls = new List<Task<HttpResponseMessage>>();

            foreach (Endpoint endpoint in _composerSettings.Value.Endpoints)
            {
                await CallEndpoint(endpoint, jobDescription);
                //calls.Add(CallEndpoint(endpoint, jobDescription));
            }

            //Task.WaitAll(calls.ToArray());

            // Set the entire job as completed
            _jobService.MarkJobAsComplete(jobId);

            //});


        }

        private async Task CallEndpoint(Endpoint endpoint, JobDescription jobDescription)
        {
            HttpResponseMessage res = null;

            HttpClient client = new HttpClient();

            res = await client.PostAsJsonAsync(endpoint.Url, jobDescription);

            //return Task.FromResult<HttpResponseMessage>(res);
        }

        private JobDescription CreateJobDescription(StartJobRequest request, int jobId)
        {
            return new JobDescription
            {
                StartJobRequest = new StartJobRequest
                {
                    TestRunImage = request.TestRunImage,
                    TestRunCommand = request.TestRunCommand,
                    Yaml = request.Yaml
                },
                EnvironmentVariables = new Dictionary<string, string>
                {
                    {"TESTER_SERVER", _queueSettings.Value.Server },
                    {"TESTER_VHOST", _queueSettings.Value.Vhost },
                    {"TESTER_USERNAME", _queueSettings.Value.Username },
                    {"TESTER_PASSWORD", _queueSettings.Value.Password },
                    {"TESTER_REQUEST_QUEUE", jobId.ToString() },
                    {"TESTER_RESPONSE_QUEUE", QueueSubscriberService.RESPONSE_QUEUE_NAME },
                }
            };
        }
    }
}

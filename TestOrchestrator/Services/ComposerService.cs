using CommonModels.Config;
using DataService.Services;
using JobModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TestOrchestrator.Models;
using Microsoft.Extensions.DependencyInjection;

namespace TestOrchestrator.Services
{
    public interface IComposerService
    {
        Task RunCompose(StartJobRequest request, int jobId);
    }

    public class ComposerService : IComposerService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<ComposerSettings> _composerSettings;
        private readonly IOptions<QueueSettings> _queueSettings;

        public ComposerService(IServiceProvider serviceProvider, IOptions<ComposerSettings> composerSettings, 
            IOptions<QueueSettings> queueSettings)
        {
            _serviceProvider = serviceProvider;
            _composerSettings = composerSettings;
            _queueSettings = queueSettings;
        }

        public Task RunCompose(StartJobRequest request, int jobId)
        {
            JobDescription jobDescription = CreateJobDescription(request, jobId);

            return Task.Run(() =>
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    List<Task<HttpResponseMessage>> calls = new List<Task<HttpResponseMessage>>();
                    foreach (Endpoint endpoint in _composerSettings.Value.Endpoints)
                    {
                        //await CallEndpoint(endpoint, jobDescription);
                        calls.Add(CallEndpoint(endpoint, jobDescription));

                        Task.WaitAll(calls.ToArray());

                        IJobService jobService = scope.ServiceProvider.GetService<IJobService>();

                        // Set the entire job as completed
                        jobService.MarkJobAsComplete(jobId);
                    }
                }
            });
        }

        private Task<HttpResponseMessage> CallEndpoint(Endpoint endpoint, JobDescription jobDescription)
        {
            HttpResponseMessage res = null;

            HttpClient client = new HttpClient();

            return client.PostAsJsonAsync(endpoint.Url, jobDescription);
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

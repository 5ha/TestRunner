using ComposeManager.Config;
using JobModels;
using Microsoft.Extensions.Options;
using System;

namespace ComposeManager.Services
{
    public interface IJobRunnerService
    {
        void RunJob(JobDescription jobDescription);
    }

    public class JobRunnerService : IJobRunnerService
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IServiceProvider _serviceProvider;

        public JobRunnerService(IOptions<AppSettings> appSettings, IServiceProvider serviceProvider)
        {
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
        }

        public void RunJob(JobDescription jobDescription)
        {
            for (int i = 0; i < _appSettings.Value.ProcessCount; i++)
            {
                var runner = (IJobRunner)_serviceProvider.GetService(typeof(IJobRunner));
                runner.RunJob(jobDescription, $"Process{i}").ConfigureAwait(false);
            }
        }
    }
}

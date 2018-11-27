using ComposeManager.Config;
using DockerUtilities;
using JobModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComposeManager.Services
{
    public interface IJobRunner
    {
        Task RunJob(JobDescription jobDescription, string instanceName);
    }
    public class JobRunner : IJobRunner
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ILogger<JobRunner> _logger;
        private readonly IComposeWrapper _composeWrapper;

        public JobRunner(IOptions<AppSettings> appSettings, ILogger<JobRunner> logger, IComposeWrapper composeWrapper)
        {
            _appSettings = appSettings;
            _logger = logger;
            _composeWrapper = composeWrapper;
        }

        public Task RunJob(JobDescription jobDescription, string instanceName)
        {
            _logger.LogInformation("Running job {0}", jobDescription.Build);

            string enhancedYaml = EnhanceYaml(jobDescription, instanceName);

            return _composeWrapper.RunCompose(instanceName, _appSettings.Value.YamlBasePath,
                                            TimeSpan.FromMinutes(_appSettings.Value.MaxComposeExecutionTimeInMinutes), enhancedYaml);
        }

        private string EnhanceYaml(JobDescription jobDescription, string instanceName)
        {
            ComposeFileParser parser = new ComposeFileParser(jobDescription.Yaml);

            if (jobDescription.EnvironmentVariables == null)
                jobDescription.EnvironmentVariables = new Dictionary<string, string>();

            jobDescription.EnvironmentVariables.Add("TESTER_INSTANCE", instanceName);

            parser.AddEnvironmentVariables(jobDescription.EnvironmentVariables);

            return parser.Save();
        }
    }
}

using ComposeManager.Config;
using DockerUtilities;
using JobModels;
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

        public JobRunner(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task RunJob(JobDescription jobDescription, string instanceName)
        {
            ComposeWrapper compose = new ComposeWrapper(instanceName, _appSettings.Value.YamlBasePath,
                                            TimeSpan.FromMinutes(_appSettings.Value.MaxComposeExecutionTimeInMinutes));

            string enhancedYaml = EnhanceYaml(jobDescription, instanceName);

            await compose.RunCompose(enhancedYaml);
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

using CliWrap;
using CliWrap.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerUtilities
{
    public interface IDockerWrapper
    {
        Task<string> Run(string imageName, Dictionary<string, string> environmentVariables = null, string command = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    public class DockerWrapper : IDockerWrapper
    {
        private readonly ILogger<DockerWrapper> _logger;

        public DockerWrapper(ILogger<DockerWrapper> logger)
        {
            _logger = logger;
        }

        public async Task<string> Run(string imageName, Dictionary<string, string> environmentVariables = null, string command = null, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            StringBuilder result = new StringBuilder();

            _logger.LogInformation("Running: docker run -rm {0} {1} {2}", GetEnvironmentVariables(environmentVariables), imageName, command);

            try
            {
                await new Cli("docker")
                    .SetArguments($"run --rm {GetEnvironmentVariables(environmentVariables)} {imageName} {command}")
                    .SetStandardOutputCallback(stdOutLine => result.Append(stdOutLine))
                    .SetCancellationToken(cancellationToken)
                    .EnableStandardErrorValidation()
                    .EnableExitCodeValidation()
                    .ExecuteAsync();
            }
            catch (ExitCodeValidationException ve)
            {
                _logger.LogError("Docker Error exit code {0}: {1}", ve.ExecutionResult.ExitCode, ve.ExecutionResult.StandardError);
                _logger.LogError("Standard Output: {1}", ve.ExecutionResult.StandardOutput);
            }

            _logger.LogInformation("Completed: docker run -rm {0} {1} {2}", GetEnvironmentVariables(environmentVariables), imageName, command);

            return result.ToString();
        }

        private string GetEnvironmentVariables(Dictionary<string, string> environmentVariables)
        {
            if (environmentVariables == null)
                return string.Empty;

            StringBuilder s = new StringBuilder();

            foreach (var key in environmentVariables.Keys)
            {
                s.Append($" -e {key}={environmentVariables[key]} ");
            }

            return s.ToString();
        }
    }
}

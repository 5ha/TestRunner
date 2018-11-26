using CliWrap;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerUtilities
{
    public class DockerWrapper
    {
        public async Task<string> Run(string imageName, Dictionary<string, string> environmentVariables = null, string command = null, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            StringBuilder result = new StringBuilder();

            await new Cli("docker")
                .SetArguments($"run --rm {GetEnvironmentVariables(environmentVariables)} {imageName} {command}")
                .SetStandardOutputCallback(stdOutLine => result.Append(stdOutLine))
                .SetCancellationToken(cancellationToken)
                .EnableStandardErrorValidation()
                .EnableExitCodeValidation()
                .ExecuteAsync();

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

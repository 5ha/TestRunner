using CliWrap;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerUtilities
{
    public class ComposeWrapper
    {
        private Cli _cliCompose;
        private Cli _cliDocker;
        private readonly string _projectName;
        private string _executionPath;
        private CancellationTokenSource _cancellationTokenSource;

        public ComposeWrapper(string projectName, string basePath, TimeSpan maxExecutionTime)
        {
            _cancellationTokenSource = new CancellationTokenSource(maxExecutionTime);
            _executionPath = Path.Combine(basePath, projectName);
            Directory.CreateDirectory(_executionPath);
            _cliCompose = new Cli("docker-compose");
            _cliDocker = new Cli("docker");
            _projectName = projectName;
        }

        public string GetComposeVersion()
        {
            string version;

            var output = _cliCompose
                .SetArguments("-v")
                .EnableStandardErrorValidation()
                .EnableExitCodeValidation()
                .Execute();
            version = output.StandardOutput;

            return version;
        }

        private async Task WaitForContainerToDie()
        {
            CancellationTokenSource monitorTokenSource = new CancellationTokenSource();
            var linkedTokens = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, monitorTokenSource.Token);

            try
            {
                await _cliDocker
                    .SetArguments($@"events --filter com.docker.compose.project={_projectName} --filter event=die")
                    .SetCancellationToken(linkedTokens.Token)
                    .SetStandardOutputCallback(s => monitorTokenSource.Cancel())// We received the info we were waiting for
                    .SetStandardErrorCallback(s => monitorTokenSource.Cancel())
                    .EnableStandardErrorValidation()
                    .EnableExitCodeValidation()
                    .ExecuteAsync();
            }
            catch (OperationCanceledException)
            {
                // Only throw the cancellation exception if the global token was cancelled
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    throw;
                }
            }
        }

        public async Task RunCompose(string yaml)
        {
            File.WriteAllText(Path.Combine(_executionPath, "docker-compose.yml"), yaml);

            _cliCompose
                .SetStandardInput($@"-p {_projectName} -f {_executionPath}\docker-compose.yml up")
                .ExecuteAndForget();

            await WaitForContainerToDie();

            ComposeDown();
        }

        private void ComposeDown()
        {
            _cliCompose
                .SetStandardInput($@"-p {_projectName} -f {_executionPath}\docker-compose.yml down")
                .SetCancellationToken(_cancellationTokenSource.Token)
                .EnableStandardErrorValidation()
                .EnableExitCodeValidation()
                .Execute();
        }
    }
}
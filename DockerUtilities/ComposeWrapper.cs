using CliWrap;
using CliWrap.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerUtilities
{
    public class ComposeWrapper
    {
        //private Cli _cliCompose;
        //private Cli _cliDocker;
        private readonly string _projectName;
        private string _executionPath;
        private CancellationTokenSource _cancellationTokenSource;

        private readonly string DOCKER_COMPOSE = "docker-compose";
        private readonly string DOCKER = "docker";

        public ComposeWrapper(string projectName, string basePath, TimeSpan maxExecutionTime)
        {
            _cancellationTokenSource = new CancellationTokenSource(maxExecutionTime);
            _executionPath = Path.Combine(basePath, projectName);
            Directory.CreateDirectory(_executionPath);
            //_cliCompose = new Cli("docker-compose");
            // = new Cli("docker");
            _projectName = projectName;
        }

        public string GetComposeVersion()
        {
            string version;

            var output = new Cli(DOCKER_COMPOSE)
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
                await new Cli(DOCKER)
                    .SetArguments($@"events --filter com.docker.compose.project={_projectName} --filter event=die")
                    .SetCancellationToken(linkedTokens.Token)
                    .SetStandardOutputCallback(s => monitorTokenSource.Cancel())// We received the info we were waiting for
                    .SetStandardErrorCallback(s => monitorTokenSource.Cancel())
                    //.EnableStandardErrorValidation()
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

            var dockerUp = Task.Run(async() =>
             {
                 await new Cli(DOCKER_COMPOSE)
                        .SetWorkingDirectory(_executionPath)
                        .SetArguments($@"up")
                        //.EnableStandardErrorValidation()
                        .EnableExitCodeValidation()
                        .ExecuteAsync();
             });


            await WaitForContainerToDie();

            await ComposeDown();
        }

        public async Task ComposeDown()
        {
            await
            new Cli(DOCKER_COMPOSE)
                .SetWorkingDirectory(_executionPath)
                .SetArguments($@"kill")
                .SetCancellationToken(_cancellationTokenSource.Token)
                .EnableStandardErrorValidation(false)
                .EnableExitCodeValidation()
                .ExecuteAsync();

            await
            new Cli(DOCKER_COMPOSE)
                .SetWorkingDirectory(_executionPath)
                .SetArguments($@"down")
                .SetCancellationToken(_cancellationTokenSource.Token)
                .EnableStandardErrorValidation(false)
                .EnableExitCodeValidation()
                .ExecuteAsync();

            File.Delete(YamlFilename);
        }

        private string YamlFilename
        {
            get
            {
                return Path.Combine(_executionPath, "docker-compose.yml");
            }
        }
    }
}
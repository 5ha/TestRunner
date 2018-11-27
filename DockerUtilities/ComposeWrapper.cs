using CliWrap;
using CliWrap.Exceptions;
using CliWrap.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerUtilities
{
    public interface IComposeWrapper
    {
        Task RunCompose(string projectName, string basePath, TimeSpan maxExecutionTime, string yaml);
    }

    public class ComposeWrapper : IComposeWrapper//, IDisposable
    {
        private string _projectName;
        private string _executionPath;
        private CancellationTokenSource _cancellationTokenSource;

        private readonly string DOCKER_COMPOSE = "docker-compose";
        private readonly string DOCKER = "docker";
        private readonly ILogger<ComposeWrapper> _logger;

        public ComposeWrapper(ILogger<ComposeWrapper> logger)
        {
            _logger = logger;
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

        public async Task RunCompose(string projectName, string basePath, TimeSpan maxExecutionTime, string yaml)
        {
            Initialise(projectName, basePath, maxExecutionTime);

            _logger.LogInformation("[{0}] Composing", projectName);

            File.WriteAllText(Path.Combine(_executionPath, "docker-compose.yml"), yaml);

            Task.WaitAny(Compose(), WaitForContainerToDie());

            await ComposeDown();

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            
            _logger.LogInformation("[{0}] Compose Complete", projectName);
        }

        private void Initialise(string projectName, string basePath, TimeSpan maxExecutionTime)
        {
            _cancellationTokenSource = new CancellationTokenSource(maxExecutionTime);
            _executionPath = Path.Combine(basePath, projectName);
            Directory.CreateDirectory(_executionPath);
            _projectName = projectName;
        }

        private async Task Compose()
        {
            ExecutionResult result = null;

            try
            {
                await new Cli(DOCKER_COMPOSE)
                       .SetWorkingDirectory(_executionPath)
                       .SetCancellationToken(_cancellationTokenSource.Token)
                       .SetArguments($@"up --force-recreate")
                       .EnableStandardErrorValidation(false)
                       .EnableExitCodeValidation()
                       //.SetStandardOutputEncoding(Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage))
                       .SetStandardOutputEncoding(Encoding.GetEncoding(437))
                       .ExecuteAsync();
            }
            catch (ExitCodeValidationException ve)
            {
                _logger.LogError("Compose Error: {0}", ve.ExecutionResult.StandardError);
            }

            _logger.LogDebug("'docker-compose up' completed with output: {0}", result.StandardOutput);
        }

        private async Task WaitForContainerToDie()
        {
            CancellationTokenSource monitorTokenSource = new CancellationTokenSource();
            var linkedTokens = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, monitorTokenSource.Token);

            Action<string> containerComplete = s => 
            {
                _logger.LogDebug("[{0}] Waiting for container to die completed with output: {1}", _projectName, s);
                monitorTokenSource.Cancel();
            };

            try
            {
                _logger.LogDebug("[{0}] Waiting for container to die", _projectName);

                await new Cli(DOCKER)
                    .SetArguments($@"events --filter com.docker.compose.project={_projectName} --filter event=die")
                    .SetCancellationToken(linkedTokens.Token)
                    .SetStandardOutputCallback(s => containerComplete(s))// We received the info we were waiting for
                    .SetStandardErrorCallback(s => containerComplete(s))
                    .EnableStandardErrorValidation(false)
                    .EnableExitCodeValidation()
                    .ExecuteAsync();
            }
            catch (OperationCanceledException)
            {
                // Only throw the cancellation exception if the global token was cancelled
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _logger.LogDebug("[{0}] Cancellation token cancelled", _projectName);
                    throw;
                }
            }
        }

        private async Task ComposeDown()
        {
            StringBuilder s = new StringBuilder();

            Action<string> appendLine = (res) => s.AppendLine(res);
            
            try
            {
                _logger.LogDebug("[{0}] Executing docker-compose kill", _projectName);

                await
                new Cli(DOCKER_COMPOSE)
                    .SetWorkingDirectory(_executionPath)
                    .SetArguments($@"kill")
                    .SetCancellationToken(_cancellationTokenSource.Token)
                    .EnableStandardErrorValidation(false)
                    .EnableExitCodeValidation(false)
                    .SetStandardOutputCallback(appendLine)
                    .SetStandardErrorCallback(appendLine)
                    .ExecuteAsync();

                _logger.LogDebug("[{0}] 'docker-compose kill' returned: {1}", _projectName, s.ToString());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[{0}] Error in 'docker-compose kill'", _projectName);
            }

            s = new StringBuilder();

            try
            {
                _logger.LogDebug("[{0}] Executing 'docker-compose down'", _projectName);

                await
                new Cli(DOCKER_COMPOSE)
                    .SetWorkingDirectory(_executionPath)
                    .SetArguments($@"down")
                    .SetCancellationToken(_cancellationTokenSource.Token)
                    .EnableStandardErrorValidation(false)
                    .EnableExitCodeValidation(false)
                    .SetStandardOutputCallback(appendLine)
                    .SetStandardErrorCallback(appendLine)
                    .ExecuteAsync();

                _logger.LogDebug("[{0}] 'docker-compose down' returned: {1}", _projectName, s.ToString());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[{0}] Error in 'docker-compose down'", _projectName);
            }

            try
            {
                _logger.LogDebug("[{0}] Deleting directory: {1}", _projectName, _executionPath);

                Directory.Delete(_executionPath, true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[{0}] Error deleting directory {0}", _projectName, _executionPath);
            }
        }

        private string YamlFilename
        {
            get
            {
                return Path.Combine(_executionPath, "docker-compose.yml");
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();
                        _cancellationTokenSource = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ComposeWrapper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
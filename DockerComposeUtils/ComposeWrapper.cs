using CliWrap;
using CliWrap.Models;
using CliWrap.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerComposeUtils
{
    public class ComposeWrapper : IDisposable
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

            var output = _cliCompose.Execute("-v");
            version = output.StandardOutput;

            return version;
        }

        private async Task WaitForContainerToDie()
        {
            ExecutionInput input = new ExecutionInput($@"events --filter com.docker.compose.project={_projectName} --filter event=die");
            CancellationTokenSource monitorTokenSource = new CancellationTokenSource();
            var linkedTokens = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, monitorTokenSource.Token);

            var handler = new BufferHandler(
            stdOutLine => monitorTokenSource.Cancel(), // We received the info we were waiting for
            stdErrLine => monitorTokenSource.Cancel());

            try
            {
                await _cliDocker.ExecuteAsync(input, linkedTokens.Token, handler);
            }
            catch (OperationCanceledException e)
            {
                // Only throw the cancelltion exception if the global token was cancelled
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    throw;
                }
            }
        }

        public async Task RunCompose(string yaml)
        {
            File.WriteAllText(Path.Combine(_executionPath, "docker-compose.yml"), yaml);
            ExecutionInput input = new ExecutionInput($@"-p {_projectName} -f {_executionPath}\docker-compose.yml up");
            
            _cliCompose.ExecuteAndForget(input);

            await WaitForContainerToDie();

            ComposeDown();
        }

        private void ComposeDown()
        {
            ExecutionInput input = new ExecutionInput($@"-p {_projectName} -f {_executionPath}\docker-compose.yml down");

            _cliCompose.Execute(input, _cancellationTokenSource.Token);
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_cliCompose != null)
                    {
                        _cliCompose.CancelAll();
                        _cliCompose.Dispose();
                    }

                    if (_cliDocker != null)
                    {
                        _cliDocker.CancelAll();
                        _cliDocker.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                //if (Directory.Exists(_executionPath)) Directory.Delete(_executionPath, true);

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //~ComposeWrapper()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //    Dispose(false);
        //}

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            //GC.SuppressFinalize(this);
        }
        #endregion
    }
}

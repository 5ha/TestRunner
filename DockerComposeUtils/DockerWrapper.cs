using CliWrap;
using CliWrap.Models;
using CliWrap.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerComposeUtils
{
    public class DockerWrapper : IDisposable
    {
        private Cli _cliDocker;

        public DockerWrapper()
        {
            _cliDocker = new Cli("docker");
        }

        public async Task<string> Run(string imageName, Dictionary<string, string> environmentVariables = null, string command = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            StringBuilder result = new StringBuilder();
            StringBuilder error = new StringBuilder();

            ExecutionInput input = new ExecutionInput($"run --rm {imageName} {command}");

            var handler = new BufferHandler(stdOutLine => result.Append(stdOutLine), stdErrLine => error.Append(stdErrLine));

            if (environmentVariables != null)
            {
                input.EnvironmentVariables = environmentVariables;
            }
            await _cliDocker.ExecuteAsync(input, cancellationToken, handler);

            string errorString = error.ToString();
            if (!string.IsNullOrEmpty(errorString))
            {
                throw new CliException("Could not run image",result.ToString(),errorString);
            }
            return result.ToString();
        }

        

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_cliDocker != null)
                    {
                        _cliDocker.CancelAll();
                        _cliDocker.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DockerWrapper() {
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

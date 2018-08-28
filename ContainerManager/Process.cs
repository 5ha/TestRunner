using Docker.DotNet.Models;
using DockerComposeUtils;
using DockerUtils;
using HiQ.Builders;
using HiQ.Interfaces;
using MessageModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerManager
{
    public class Process
    {
        private SynchronizedCollection<RunBuild> Builds = new SynchronizedCollection<RunBuild>();
        private string _instanceName;
        CancellationToken _cancellationToken;

        string _queueServer;
        string _queueVhost;
        string _queueUsername;
        string _queuePassword;

        public Process(string instanceName, CancellationToken cancellationToken)
        {
            _instanceName = instanceName;
            _cancellationToken = cancellationToken;

            _queueServer = ConfigurationManager.AppSettings["queueServer"];
            _queueVhost = ConfigurationManager.AppSettings["queueVhost"];
            _queueUsername = ConfigurationManager.AppSettings["queueUsername"];
            _queuePassword = ConfigurationManager.AppSettings["queuePassword"];
        }

        public void Initialise()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    RunBuild currentBuild = null;
                    currentBuild = await GetCurrentBuild();

                    IQueueBuilder queueBuilder = new RabbitBuilder();
                    using (ISender statusMessageSender = queueBuilder.ConfigureTransport(_queueServer, _queueVhost, _queueUsername, _queuePassword)
                        .ISendTo(QueueNames.Status(currentBuild.Build))
                        .Build())
                    {
                        try
                        {
                            Action<(string status, string warning, string error)> notify = ((string status, string warning, string error) state) =>
                            {
                                if (!string.IsNullOrEmpty(state.status))
                                {
                                    Console.WriteLine($"INFO: {state.status}");
                                }

                                if (!string.IsNullOrEmpty(state.warning))
                                {
                                    Console.WriteLine($"WARNING: {state.warning}");
                                }

                                if (!string.IsNullOrEmpty(state.error))
                                {
                                    Console.WriteLine($"ERROR: {state.error}");
                                }
                                statusMessageSender.Send(StatusReport(state));
                            };

                            await RunProcess(currentBuild, notify);
                        }
                        catch (Exception ex)
                        {
                            statusMessageSender.Send(StatusReport((null, null, SerialiseError(ex))));

                            // TODO : Log the exception
                            // Rethrowing the exception from this point would kill the thread which we don't want to do
                        }
                        finally
                        {
                            RemoveBuild(currentBuild);
                        }
                    }
                }
            }, _cancellationToken);
        }

        private string SerialiseError(Exception ex)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine(ex.Message);
            s.AppendLine(ex.StackTrace);
            return s.ToString();
        }

        private Task<RunBuild> GetCurrentBuild()
        {
            return Task.Run(async () =>
            {

                RunBuild currentBuild = null;

                while (currentBuild == null)
                {
                    currentBuild = Builds.FirstOrDefault();
                    if (currentBuild == null) await Task.Delay(1000);
                }

                return currentBuild;
            }, _cancellationToken);
        }

        public Task RunBuild(RunBuild build)
        {
            Builds.Add(build);

            return Task.Run(async () =>
            {
                while (true)
                {
                    if (!Builds.Contains(build)) return Task.CompletedTask;
                    await Task.Delay(1000);
                }
            }, _cancellationToken);
        }

        public void RemoveBuild(RunBuild build)
        {
            Builds.Remove(build);
        }

        private async Task<(string stdOut, string stdErr)> RunProcess(RunBuild build, Action<(string status, string warning, string error)> notify)
        {
            ContainerHelper helper = new ContainerHelper();
            string stdOut = null;
            string stdErr = null;
            string containerName = _instanceName;

            try
            {
                using (ComposeWrapper compose = new ComposeWrapper(_instanceName, ConfigurationManager.AppSettings["yamlBasePath"], TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["composeExecutionTimeoutMinutes"]))))
                {
                    await compose.RunCompose(build.Yaml);
                }


                notify(($"{_instanceName} Completed", null, null));
            }
            catch (Exception ex)
            {
                notify((null, null, SerialiseError(ex)));
            }

            return (stdOut, stdErr);
        }

        private StatusMessage PrepareStatusMessage()
        {
            StatusMessage message = new StatusMessage
            {
                Machine = Environment.MachineName,
                Application = "ContainerManager",
                Process = _instanceName
            };
            return message;
        }

        private StatusMessage StatusReport((string status, string warning, string error) state)
        {
            StatusMessage m = PrepareStatusMessage();
            m.Message = state.status;
            m.Warning = state.warning;
            m.Error = state.error;
            return m;
        }
    }
}

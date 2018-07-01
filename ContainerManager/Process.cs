using Docker.DotNet.Models;
using DockerUtils;
using HiQ.Builders;
using HiQ.Interfaces;
using MessageModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public Process(string instanceName, CancellationToken cancellationToken)
        {
            _instanceName = instanceName;
            _cancellationToken = cancellationToken;
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
                    using (ISender statusMessageSender = queueBuilder.ConfigureTransport("my-rabbit", "remote", "remote")
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
                if (helper.ContainerExists(containerName))
                {
                    await helper.RemoveContainer(containerName);
                }

                CreateContainerResponse createContainerResponse = await helper.CreateContainer(build.ContainerImage, containerName, build.Commands);

                if (createContainerResponse.Warnings != null)
                {
                    notify((null, String.Join("\r\n", createContainerResponse.Warnings.ToArray()),null));
                }

                bool started = await helper.StartContainer(createContainerResponse.ID);

                notify((null, $"Could not start container {createContainerResponse.ID}", null));

                (stdOut, stdErr) = await helper.AwaitContainer(createContainerResponse.ID);

                if (!string.IsNullOrEmpty(stdOut))
                {
                    notify((stdOut, null, null));
                }

                if (!string.IsNullOrEmpty(stdErr))
                {
                    notify((null, null, stdErr));
                }

                notify(($"Container {createContainerResponse.ID} completed", null, null));


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

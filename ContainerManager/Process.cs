using Docker.DotNet.Models;
using DockerUtils;
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

                    try
                    {
                        currentBuild = await GetCurrentBuild();

                        await RunProcess(currentBuild);

                    }
                    catch (Exception ex)
                    {
                        var mess = ex.Message;
                        // TODO : Log the exception
                        // Rethrowing the exception from this point would kill the thread which we don't want to do
                    }
                    finally
                    {
                        RemoveBuild(currentBuild);
                    }
                }
            }, _cancellationToken);
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

        private async Task<(string stdOut, string stdErr)> RunProcess(RunBuild build)
        {
            ContainerHelper helper = new ContainerHelper();
            string stdOut = null;
            string stdErr = null;
            string containerName = _instanceName;

            if (helper.ContainerExists(containerName))
            {
                await helper.RemoveContainer(containerName);
            }

            CreateContainerResponse createContainerResponse = null;
            try
            {
                createContainerResponse = await helper.CreateContainer(build.ContainerImage, containerName, build.Commands);
            }
            catch (Exception ex)
            {
                stdErr += ex.Message;
                stdErr += ex.StackTrace;
            }

            await helper.StartContainer(createContainerResponse.ID);

            (stdOut, stdErr) = await helper.AwaitContainer(createContainerResponse.ID);

            return (stdOut, stdErr);
        }
    }
}

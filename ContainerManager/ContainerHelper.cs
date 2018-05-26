using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerManager
{
    public class ContainerHelper
    {
        private DockerClientConfiguration __clientConfiguration;
        private DockerClientConfiguration _clientConfiguration
        {
            get
            {
                if (__clientConfiguration == null)
                {
                    __clientConfiguration = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"));
                }
                return __clientConfiguration;
            }
        }

        private DockerClient __client;
        private DockerClient _client
        {
            get
            {
                if(__client == null)
                {
                    __client = _clientConfiguration.CreateClient();
                }
                return __client;
            }
        }

        public Task<IList<ContainerListResponse>> ListContainers(bool all = true)
        {
            return _client.Containers.ListContainersAsync(new ContainersListParameters()
            {
                All = all
            });
        }

        public ContainerListResponse FindContainer(string name)
        {
            IList<ContainerListResponse> allContainers = ListContainers().Result;

            return allContainers.Where(x => x.Names.Contains("/" + name)).FirstOrDefault();
        }

        public bool ContainerExists(string name)
        {
            return FindContainer(name) != null;
        }

        public CreateContainerResponse CreateContainer(string name)
        {
            return _client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = "testrunner",
                Name = name,
                Cmd = new List<string> { "TestRunner", "c:\\app" },
                ArgsEscaped = false,
                AttachStderr = false,
                AttachStdin = false,
                AttachStdout = false,
                NetworkingConfig = new NetworkingConfig
                {
                    EndpointsConfig = new Dictionary<string, EndpointSettings>
                    {
                        { "my-net", new EndpointSettings() }
                    }
                }

            }).Result;
        }

        public Task<bool> StartContainer(string id)
        {
            return _client.Containers.StartContainerAsync(id, new ContainerStartParameters() {

            });
        }

        public Task RemoveContainer(string id)
        {
            return _client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters { Force = true });
        }
    }
}

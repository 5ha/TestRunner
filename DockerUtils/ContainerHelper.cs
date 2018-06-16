using Docker.DotNet;
using Docker.DotNet.Models;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerUtils
{
    public class ContainerHelper : IDisposable
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

        public Task<CreateContainerResponse> CreateContainer(string image, string name, List<string> commands)
        {
            return _client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = image,
                Name = name,
                Cmd = commands,
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

            });
        }

        public Task<bool> StartContainer(string id)
        {
            return _client.Containers.StartContainerAsync(id, new ContainerStartParameters() {

            });

        }

        public async Task<(string stdOut, string stdErr)> AwaitContainer(string id)
        {
                var config = new ContainerAttachParameters
                {
                    Stream = true,
                    Stderr = true,
                    Stdin = false,
                    Stdout = true,
                    Logs = "1"
                };

                (string stdOut, string stdErr) containerResult;

                using (var stream = await _client.Containers.AttachContainerAsync(id, false, config, default(CancellationToken)))
                {
                    containerResult = await stream.ReadOutputToEndAsync(default(CancellationToken));
                }

                return containerResult;
        }

        public Task RemoveContainer(string id)
        {
            return _client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters { Force = true });
        }

        public async Task BuildImage(string contextRoot, string repository, string image, string tag)
        {
            var tarFileName = "build.tar.gz";
            var tarFilePath = Path.Combine(Path.GetTempPath(), tarFileName);
            if (File.Exists(tarFilePath)) File.Delete(tarFilePath);

            TarFolder(tarFilePath, contextRoot);

            var parameters = new ImageBuildParameters
            {
                Tags = new List<string> { $"{repository}/{image}:{tag}" }
            };

            using (FileStream fs = new FileStream(tarFilePath, FileMode.Open))
            {
                var res = await _client.Images.BuildImageFromDockerfileAsync(fs, parameters);

                TextReader reader = new StreamReader(res);
                var s = reader.ReadToEnd();
            }
        }

        public void TarFolder(string archivePath, string tarRoot)
        {
            using (Stream stream = File.OpenWrite(archivePath))
            using (var writer = WriterFactory.Open(stream, ArchiveType.Tar, CompressionType.GZip))
            {
                writer.WriteAll(tarRoot, "*", SearchOption.AllDirectories);
            }
        }


        public async Task PublishImage(string image)
        {
            var authConfig = new AuthConfig()
            {
                Username = "shawnseabrook",
                Password = "r8d1M5prvHZtIydkCNeS"
            };

            var parameters = new ImagePushParameters()
            {

            };

            await _client.Images.PushImageAsync(image, parameters, authConfig, new ProgressMonitor());
        }

        public void Dispose()
        {
            //TODO: stop any running container
        }

        

        private class ProgressMonitor : IProgress<JSONMessage>
        {
            public void Report(JSONMessage value)
            {
                var x = value.ProgressMessage;
                //throw new NotImplementedException();
            }
        }
    }


}

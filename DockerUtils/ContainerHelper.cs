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
                   // __clientConfiguration = new DockerClientConfiguration(new Uri("tcp://localhost:4589"));
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

        public async Task<string> AttachContainer(string id)
        {
            var config = new ContainerAttachParameters
            {
                Stream = true,
                Stderr = true,
                Stdin = false,
                Stdout = true,
            };

            string stdOut;
            string stdErr;
            var buffer = new byte[1024];
            using (var stream = await _client.Containers.AttachContainerAsync(id, false, config, default(CancellationToken)))
            {
                (stdOut, stdErr) = await stream.ReadOutputToEndAsync(default(CancellationToken));
            }

            return stdOut;
        }

        public async Task<(string stdOut, string stdErr)> AwaitContainer(string id)
        {
            try
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
            } catch (Exception ex)
            {
                return ("", ex.Message);
            }
        }

        public Task RemoveContainer(string id)
        {
            return _client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters { Force = true });
        }

        public async Task<string> BuildImage(string contextRoot, string repository, string image, string tag)
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
                return reader.ReadToEnd();
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

        public async Task PullImage(string image, Action<string> onStatus, Action<string> onError, Action<string> onProgress)
        {
            await _client.Images.CreateImageAsync(new ImagesCreateParameters()
            { FromImage = image }, null, new ProgressMonitor(onStatus, onError, onProgress));
        }

        public async Task PublishImage(string image, Action<string> onStatus, Action<string> onError, Action<string> onProgress)
        {
            var authConfig = new AuthConfig()
            {
                Username = "shawnseabrook",
                Password = "r8d1M5prvHZtIydkCNeS"
            };

            var parameters = new ImagePushParameters()
            {

            };

            // TODO: send progress results
            await _client.Images.PushImageAsync(image, parameters, authConfig, new ProgressMonitor(onStatus, onError, onProgress));
        }

        public void Dispose()
        {
            //TODO: stop any running container
        }

        

        private class ProgressMonitor : IProgress<JSONMessage>
        {
            private readonly Action<string> _onStatus;
            private readonly Action<string> _onError;
            private readonly Action<string> _onProgress;

            public ProgressMonitor(Action<string> onStatus, Action<string> onError, Action<string> onProgress)
            {
                _onStatus = onStatus;
                _onError = onError;
                _onProgress = onProgress;
            }
            public void Report(JSONMessage value)
            {
                if (_onStatus != null && !string.IsNullOrEmpty(value.Status))
                {
                    _onStatus(value.Status);
                }
                if (_onError != null && !string.IsNullOrEmpty(value.ErrorMessage))
                {
                    _onError(value.ErrorMessage);
                }
                if (_onProgress != null && !string.IsNullOrEmpty(value.ProgressMessage))
                {
                    _onProgress(value.ProgressMessage);
                }
            }
        }
    }


}

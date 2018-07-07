using ContainerManager;
using Docker.DotNet.Models;
using DockerUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerManagerTests
{
    [TestFixture]
    public class ExploratoryTests
    {
        [Test]
        public async Task CanCreateImage()
        {
            var sut = new ContainerHelper();

            await sut.BuildImage(@"C:\Users\shawn\source\repos\TestNUnitRunner\Publish", "shawnseabrook", "myimage", "v5");
        }

        [Test]
        public async Task CanPushImage()
        {
            var sut = new ContainerHelper();

            await sut.PublishImage(@"shawnseabrook\myimage:v5",null,null,null);
        }

        [Test]
        public async Task CanPullImage()
        {
            var sut = new ContainerHelper();

            Action<string> write = (s) => Console.WriteLine(s);

            await sut.PullImage(@"shawnseabrook/build:20", write, write, write);
        }

        [Test]
        public async Task CanAttachContainer()
        {
            var sut = new ContainerHelper();

            CreateContainerResponse createContainerResponse = await sut.CreateContainer(@"shawnseabrook/build:50", "canattach", new List<string> { "ListTests" });

            await sut.StartContainer(createContainerResponse.ID);

            string res = await sut.AttachContainer(createContainerResponse.ID);

            Console.WriteLine(res);
        }
    }
}

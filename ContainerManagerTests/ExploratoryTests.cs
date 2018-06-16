using ContainerManager;
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

            await sut.PublishImage(@"shawnseabrook\myimage:v5");
        }
    }
}

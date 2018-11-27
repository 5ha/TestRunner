using CliWrap.Exceptions;
using DockerUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DockerUtilitiesTests
{
    [TestFixture]
    public class DockerWrapperTests
    {
        private DockerWrapper _sut;
        Mock<ILogger<DockerWrapper>> _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<DockerWrapper>>();
            _sut = new DockerWrapper(_logger.Object);
        }

        [Test]
        public async Task CanRunImage()
        {
            string res = await _sut.Run("microsoft/nanoserver", null, "cmd dir");

            Console.WriteLine(res);

            Assert.IsFalse(string.IsNullOrEmpty(res));
        }

        [Test]
        public async Task ThrowsException()
        {
            try
            {
                string res = await _sut.Run("microsoft/unknown", null, "cmd dir");
            }
            catch (ExitCodeValidationException e)
            {
                Assert.IsTrue(e.ExecutionResult.StandardError.Contains("Unable to find image"));
            }
        }

        [Test]
        public async Task CanRunTester()
        {
            Dictionary<string, string> environmentVariable = new Dictionary<string, string>
                    {
                        { "TESTER_LISTTESTS", "true"}
                    };

            string res = await _sut.Run("shawnseabrook/build:144", environmentVariable, "RunTests.exe");

            Console.WriteLine(res);
        }
    }
}

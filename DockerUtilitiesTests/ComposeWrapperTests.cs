using CliWrap.Exceptions;
using DockerUtilities;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace DockerUtilitiesTests
{
    [TestFixture]
    public class ComposeWrapperTests
    {
        ComposeWrapper _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ComposeWrapper("unittest", @"C:\Users\shawn\test", TimeSpan.FromMinutes(10));
        }

        [Test]
        public void CanGetVersion()
        {
            string res = _sut.GetComposeVersion();

            Console.WriteLine(res);

            Assert.False(string.IsNullOrEmpty(res));
        }

        [Test]
        public async Task Up()
        {
            string yaml =
@"
version: ""3""
services:
  tester:
    image: blue_tester
    depends_on:
      - ""server""
    command: [""RunTestsLocally.exe""]
  server:
    image: selenium/standalone-chrome
";
            try
            {
                await _sut.RunCompose(yaml);
            } catch(ExitCodeValidationException e)
            {
                Console.WriteLine("=================== STANDARD OUTPUT ======================");
                Console.WriteLine(e.ExecutionResult.StandardOutput);
                Console.WriteLine("=================== STANDARD ERROR ======================");
                Console.WriteLine(e.ExecutionResult.StandardError);
                Console.WriteLine("=================== ============== ======================");

                throw;
            } catch(StandardErrorValidationException e)
            {
                Console.WriteLine(e.ExecutionResult.StandardOutput);
                Console.WriteLine("=================== STANDARD ERROR ======================");
                Console.WriteLine(e.ExecutionResult.StandardError);
                Console.WriteLine("=================== ============== ======================");
                throw;
            }

        }
    }
}

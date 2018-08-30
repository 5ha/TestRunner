using DockerComposeUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerComposeUtilsTests
{
    [TestFixture]
    public class DockerWrapperTests
    {
        private DockerWrapper _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new DockerWrapper();
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task CanRunImage()
        {
            string res = await _sut.Run("microsoft/nanoserver",null,"cmd dir");

            Console.WriteLine(res);

            Assert.IsFalse(string.IsNullOrEmpty(res));
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

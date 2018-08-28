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
            await _sut.RunCompose(yaml);

        }
    }
}

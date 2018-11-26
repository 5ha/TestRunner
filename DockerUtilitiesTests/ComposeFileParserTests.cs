using DockerUtilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DockerUtilitiesTests
{
    [TestFixture]
    public class ComposeFileParserTests
    {
        [Test]
        public void CanFindServices()
        {
            ComposeFileParser sut = new ComposeFileParser(yaml);

            var res = sut.ServiceNode;

            Assert.IsNotNull(res);
        }

        [Test]
        public void CanFindTesterNode()
        {
            ComposeFileParser sut = new ComposeFileParser(yaml);

            var res = sut.TesterNode;

            Assert.IsNotNull(res);
        }

        [Test]
        public void CanFindImage()
        {
            ComposeFileParser sut = new ComposeFileParser(yaml);

            var res = sut.GetTesterImageName();

            Assert.AreEqual("shawnseabrook/build:140", res);
        }

        [TestCase(@"""tester.exe""")]
        [TestCase(@"[""tester.exe""]")]
        public void CanFindCommandNode(string cmd)
        {
            ComposeFileParser sut = new ComposeFileParser(string.Format(yamlCommand, cmd));

            var res = sut.CommandNode;

            Assert.IsNotNull(res);
        }

        [TestCase(@"""RunTests.exe""", "RunTests.exe")]
        [TestCase(@"""c:\\mydir\\RunTests.exe""", @"c:\mydir\RunTests.exe")]
        [TestCase(@"""c:/mydir/RunTests.exe""", @"c:/mydir/RunTests.exe")]
        [TestCase(@"c:\\mydir\\RunTests.exe omearg", @"c:\\mydir\\RunTests.exe")]
        [TestCase(@"[""RunTests.exe""]", "RunTests.exe")]
        [TestCase(@"[""c:\\mydir\\RunTests.exe""]", @"c:\mydir\RunTests.exe")]
        [TestCase(@"[""c:\\mydir\\RunTests.exe"", ""arg23""]", @"c:\mydir\RunTests.exe")]
        public void CanGetTesterLocation(string cmd, string expected)
        {
            ComposeFileParser sut = new ComposeFileParser(string.Format(yamlCommand, cmd));

            var res = sut.GetTesterLocation();

            Assert.AreEqual(expected, res);
        }

        [Test]
        public void CanAddEnvironmentVariables()
        {
            ComposeFileParser sut = new ComposeFileParser(yaml);

            var variables = new Dictionary<string, string>
            {
                {"var1", "val1" },
                {"var2", "val2"}
            };

            sut.AddEnvironmentVariables(variables);

            var res = sut.Save();

            Assert.IsTrue(res.IndexOf("var2") > -1);
        }

        [Test]
        public void ParseString()
        {
            ComposeFileParser sut = new ComposeFileParser(yamlEscaped);

            var variables = new Dictionary<string, string>
            {
                {"var1", "val1" },
                {"var2", "val2"}
            };

            sut.AddEnvironmentVariables(variables);

            var res = sut.Save();

            Assert.IsTrue(res.IndexOf("var2") > -1);
        }

        private string yaml = @"version: ""3""
services:
  tester:
    image: shawnseabrook/build:140
    depends_on:
      - ""server""
    command: [""c:\\RunTestsLocally.exe""]
  server:
    image: selenium/standalone-chrome
    ports:
      - ""4444:4444""";

        private string yamlCommand = @"version: ""3""
services:
  tester:
    image: shawnseabrook/build:140
    depends_on:
      - ""server""
    command: {0}
  server:
    image: selenium/standalone-chrome
    ports:
      - ""4444:4444""";

        private string yamlEscaped = "version: \"3\"\nservices:\n  tester:\n    image: shawnseabrook/build:151\n    depends_on: [\"server\"]\n    command: [\"RunTests.exe\"]\n  server:\n    image: selenium/standalone-chrome\n    ports:\n    - \"4444:4444\"\n\n";
    }
}

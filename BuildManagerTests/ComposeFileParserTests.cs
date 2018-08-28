using BuildManager;
using NUnit.Framework;

namespace BuildManagerTests
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
    }
}




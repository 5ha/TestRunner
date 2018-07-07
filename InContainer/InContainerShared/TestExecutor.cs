using MessageModels;
using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace InContainerShared
{
    public class TestExecutor
    {
        private ITestEngine __testEngine;

        private ITestEngine _testEngine
        {
            get
            {
                if (__testEngine == null)
                {
                    __testEngine = TestEngineActivator.CreateInstance();
                }

                return __testEngine;
            }
        }

        private ITestFilterService __filterService;

        private ITestFilterService _filterService
        {
            get
            {
                if (__filterService == null)
                {
                    __filterService = _testEngine.Services.GetService<ITestFilterService>();
                }

                return __filterService;
            }
        }

        private TestPackage __package;

        private TestPackage _package
        {
            get
            {
                if(__package == null)
                {
                    __package = new TestPackage(_testFiles);
                }
                return __package;
            }
        }

        private ITestRunner __runner;

        private ITestRunner _runner
        {
            get
            {
                if (__runner == null)
                {
                    __runner = _testEngine.GetRunner(_package);
                }

                return __runner;
            }
        }

        private List<string> _testFiles;

        public TestExecutor(List<string> testFiles)
        {
            
            _testFiles = testFiles;
        }

        public XmlNode Execute(RunTest instruction)
        {
            var filterBuilder = _filterService.GetTestFilterBuilder();
            filterBuilder.AddTest(instruction.FullName);
            var res = _runner.Run(null, filterBuilder.GetFilter());
            return res;
        }

        public void ExecuteAll()
        {
            var filterBuilder = _filterService.GetTestFilterBuilder();
            _runner.Run(new Listener(), filterBuilder.GetFilter());
        }
    }
}
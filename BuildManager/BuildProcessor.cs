using BuildManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildManager
{
    public class BuildProcessor
    {
        private TestResult _testResult;
        private TestResultMonitor _monitor;

        public BuildProcessor(TestResult testResult)
        {
            _testResult = testResult;
            _monitor = new TestResultMonitor();
        }

        public void StartBuild(string pathToBuildFolder)
        {
            // TODO: What if current build already exists

            // Create the docker image and add it to the repository

            // Add the build to the database

            // Add all the tests to the database

            // Add all the tests to the queue

            // Add the build instruction to the queue

            // LOOP
            // Subscribe to the test result queue until all the tests have been completed (notifying subscribers)

            // Flag the build as complted notifying subscribers

            // Remove all the temporary queues
        }

        public IDisposable Subscribe(IObserver<TestResult> observer)
        {
            return _monitor.Subscribe(observer);
        }
    }
}

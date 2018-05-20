using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Engine;


namespace TestRunner
{
    class Program 
    {
        static void Main(string[] args)
        {
            ITestEngine testEngine = TestEngineActivator.CreateInstance();

            var filterService = testEngine.Services.GetService<ITestFilterService>();
            var filterBuilder = filterService.GetTestFilterBuilder();
            filterBuilder.AddTest("SystemUnderTest.TestClass.PassingTest");

            TestPackage package = new TestPackage(@"C:\\Users\\shawn\\source\\repos\\TestNUnitRunner\\SystemUnderTest\\bin\\Debug\\SystemUnderTest.dll");

            ITestRunner runner = testEngine.GetRunner(package);

            var res = runner.Run(new Listener(), filterBuilder.GetFilter());

            //var resultService = testEngine.Services.GetService<IResultService>();

            //foreach(var result in resultService.Formats)
            //{
            //    Console.WriteLine(result);
            //}


            //var tests = runner.Explore(TestFilter.Empty);

            //var filter = new TestFilter()


            Console.ReadLine();
        }


    }

    public class Listener : ITestEventListener
    {
        public void OnTestEvent(string report)
        {
            var x = report;
        }
    }
}

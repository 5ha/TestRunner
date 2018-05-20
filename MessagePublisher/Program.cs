using MassTransit;
using MessageModels;
using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MessagePublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                    
                });

                

                //sbc.ReceiveEndpoint(host, "test_queue", ep =>
                //{
                //    ep.Handler<YourMessage>(context =>
                //    {
                //        return Console.Out.WriteLineAsync($"Received: {context.Message.Text}");
                //    });
                //});
            });

            bus.Start();

            ITestEngine testEngine = TestEngineActivator.CreateInstance();

            var filterService = testEngine.Services.GetService<ITestFilterService>();
            var filterBuilder = filterService.GetTestFilterBuilder();
            filterBuilder.AddTest("SystemUnderTest.TestClass.PassingTest");

            TestPackage package = new TestPackage(@"C:\\Users\\shawn\\source\\repos\\TestNUnitRunner\\SystemUnderTest\\bin\\Debug\\SystemUnderTest.dll");

            ITestRunner runner = testEngine.GetRunner(package);

            var testSuites = runner.Explore(TestFilter.Empty);

            var testCases = testSuites.SelectNodes("//test-case");

            foreach(XmlNode n in testCases)
            {
                RunTest message = new RunTest
                {
                    FullName = n.Attributes["fullname"].Value
                };
                
                bus.Publish(message);
            }


            Console.WriteLine("Done");
            Console.ReadLine();

            bus.Stop();
        }
    }
}

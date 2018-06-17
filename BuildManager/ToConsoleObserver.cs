using BuildManager.Model;
using MessageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildManager
{
    public class ToConsoleObserver : IObserver<TestExecutionResult>, IObserver<StatusMessage>
    {
        public void OnCompleted()
        {
            Console.WriteLine("Observer: Test Run Complete");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("Observer: ERROR: ", error.Message);
            Console.WriteLine(error.StackTrace);
        }

        public void OnNext(TestExecutionResult value)
        {
            string testResult = value.Passed ? "PASSED" : "FAILED";
            Console.WriteLine($"Observer: [{testResult}] {value.FullName}");
        }

        public void OnNext(StatusMessage value)
        {
            if (value.IsError)
            {
                Console.WriteLine("Observer: ERROR: ", value.Error.Message);
                Console.WriteLine(value.Error.StackTrace);
            } else
            {
                Console.WriteLine($"Observer: {value.Message}");
            }
        }
    }
}

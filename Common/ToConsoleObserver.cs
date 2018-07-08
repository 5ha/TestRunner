using BuildManager.Model;
using MessageModels;
using System;
using System.Text;

namespace Common
{
    public class ToConsoleObserver : BaseObserver, ITestRunObserver
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

        protected override void OutputMessage(StringBuilder s, StatusMessage mess)
        {
            if (!String.IsNullOrEmpty(mess.Message))
            {
                Console.WriteLine($"{s.ToString()} : {mess.Message}");
            }
        }

        protected override void OutputWarning(StringBuilder s, StatusMessage mess)
        {
            if (!String.IsNullOrEmpty(mess.Warning))
            {
                Console.WriteLine($"{s.ToString()} WARNING: {mess.Warning}");
            }
        }

        protected override void OutputError(StringBuilder s, StatusMessage mess)
        {
            if (!String.IsNullOrEmpty(mess.Error))
            {
                Console.WriteLine($"{s.ToString()} ERROR: {mess.Error}");
            }
        }


    }
}

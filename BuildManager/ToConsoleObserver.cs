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
            StringBuilder s = new StringBuilder();
            ComposePrefix(s, value);
            OutputMessage(s, value);
            OutputWarning(s, value);
            OutputError(s, value);
        }

        private void OutputMessage(StringBuilder s, StatusMessage mess)
        {
            if (!String.IsNullOrEmpty(mess.Message))
            {
                Console.WriteLine($"{s.ToString()} : {mess.Message}");
            }
        }

        private void OutputWarning(StringBuilder s, StatusMessage mess)
        {
            if (!String.IsNullOrEmpty(mess.Warning))
            {
                Console.WriteLine($"{s.ToString()} WARNING: {mess.Warning}");
            }
        }

        private void OutputError(StringBuilder s, StatusMessage mess)
        {
            if (!String.IsNullOrEmpty(mess.Error))
            {
                Console.WriteLine($"{s.ToString()} ERROR: {mess.Error}");
            }
        }

        private void ComposePrefix(StringBuilder s, StatusMessage mess)
        {
            s.Append($"Observer:");
            if (!string.IsNullOrEmpty(mess.Machine)) s.Append($"[{mess.Machine}]");
            if (!string.IsNullOrEmpty(mess.Application)) s.Append($"[{mess.Application}]");
            if (!string.IsNullOrEmpty(mess.Process)) s.Append($"[{mess.Process}]");
        }
    }
}

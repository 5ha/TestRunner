using System;
using System.Collections.Generic;
using System.Text;

namespace BuildManager.Model
{
    public class TestExecutionResult
    {
        public int TestResultId { get; set; }
        public DateTime DateUtc { get; set; }
        public string FullName { get; set; }
        public bool Passed { get; set; }
        //public string ResultData { get; set; }

        public override string ToString()
        {
            return $"TEST RESULT: {FullName}";
        }
    }
}

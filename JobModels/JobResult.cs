using System.Collections.Generic;

namespace JobModels
{
    public class JobResult
    {
        /// <summary>
        /// The URL to poll for the next set of results
        /// If null there are no more results
        /// </summary>
        public string NextResult { get; set; }

        /// <summary>
        /// If true then the job is complete and there are no more results after this
        /// </summary>
        public bool IsComplete { get; set; }

        public List<JobTestResult> JobTestResults { get; set; }
    }
}

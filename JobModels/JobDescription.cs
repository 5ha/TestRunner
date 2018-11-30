using System.Collections.Generic;

namespace JobModels
{
    public class JobDescription
    {
        public StartJobRequest StartJobRequest { get; set; }

        public Dictionary<string, string> EnvironmentVariables { get; set; }
    }
}

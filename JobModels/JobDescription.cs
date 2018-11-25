using System.Collections.Generic;

namespace JobModels
{
    public class JobDescription
    {
        public string Yaml { get; set; }

        public string Build { get; set; }

        public string Image { get; set; }

        public string Command { get; set; }

        public Dictionary<string, string> EnvironmentVariables { get; set; }
    }
}

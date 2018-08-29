using System;
using System.Collections.Generic;

namespace MessageModels
{
    public class RunBuild
    {
        public string Yaml { get; set; }

        public string Build { get; set; }

        public string Image { get; set; }

        public string Command { get; set; }

        public Dictionary<string, string> EnvironmentVariables { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null) return false;
            RunBuild other = obj as RunBuild;
            if (other == null) return false;
            return  string.Equals(Build, other.Build);
        }

        public override int GetHashCode()
        {
            return Build.GetHashCode();
        }
    }
}

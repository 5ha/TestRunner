using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModels
{
    public class RunBuild
    {
        public string ContainerImage { get; set; }

        public string Build { get; set; }

        public List<string> Commands { get; set; }

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class BuildRunRequest
    {
        public string Build { get; set; }
        public string TestContainerImage { get; set; }
        public string Yaml { get; set; }
    }
}

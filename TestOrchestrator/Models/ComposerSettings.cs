using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestOrchestrator.Models
{
    public class ComposerSettings
    {
        public Endpoint[] Endpoints { get; set; }
    }

    public class Endpoint
    {
        public string Url { get; set; }
        public int Concurrency { get; set; }
    }
}

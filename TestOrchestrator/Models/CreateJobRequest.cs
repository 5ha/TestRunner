using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestOrchestrator.Models
{
    public class CreateJobRequest
    {
        public string Description { get; set; }

        public List<string> Tests { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace DataService.Entities
{
    public class Job
    {
        public int JobId { get; set; }

        public DateTime DateCreated { get; set; }

        public string Description { get; set; }

        public DateTime? DateCompleted { get; set; }

        public virtual List<TestRequest> TestRequests { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataService.Entities
{
    public class TestResponse
    {
        public int TestResponseId { get; set; }

        public int JobId { get; set; }

        public DateTime DateCreated { get; set; }
    }
}

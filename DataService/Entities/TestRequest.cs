using System;

namespace DataService.Entities
{
    public class TestRequest 
    {
        public int TestRequestId { get; set; }

        public int JobId { get; set; }

        public string TestName { get; set; }

        public virtual Job Job { get; set; }
    }
}

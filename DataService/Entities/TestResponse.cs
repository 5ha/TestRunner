using System;

namespace DataService.Entities
{
    public class TestResponse
    {
        public int TestRequestId { get; set; }

        public TestRequest TestRequest { get; set; }

        public DateTime DateCreated { get; set; }
    }
}

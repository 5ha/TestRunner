using System;

namespace Messages
{
    public class TestResult
    {
        public int TestRequestId { get; set; }
        public string Build { get; set; }
        public string FullName { get; set; }
        public bool Passed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan Runtime { get; set; }
        public string Output { get; set; }
        //public XmlNode Result { get; set; }
    }
}

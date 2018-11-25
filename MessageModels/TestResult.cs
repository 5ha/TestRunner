using System;
using System.Text;
using System.Xml;

namespace MessageModels
{
    public class TestResult
    {
        public string Build { get; set; }
        public string FullName { get; set; }
        public bool Passed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan Runtime { get; set; }
        public string Output { get; set; }
        public XmlNode Result { get; set; }

        public override string ToString()
        {
            return $"[{Build}] TEST RESULT: {FullName}";
        }
    }
}

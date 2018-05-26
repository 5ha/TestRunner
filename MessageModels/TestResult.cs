using System.Xml;

namespace MessageModels
{
    public class TestResult
    {
        public string Build { get; set; }
        public string FullName { get; set; }
        public XmlNode Result { get; set; }
    }
}

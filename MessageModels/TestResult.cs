﻿using System.Text;
using System.Xml;

namespace MessageModels
{
    public class TestResult
    {
        public string Build { get; set; }
        public string FullName { get; set; }
        public XmlNode Result { get; set; }

        public override string ToString()
        {
            return $"[{Build}] TEST RESULT: {FullName}";
        }
    }
}

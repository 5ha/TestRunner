using MessageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InContainerShared
{
    public class NunitParser
    {
        public void PopulateTestResult(TestResult testResult, XmlNode outPut)
        {
            XmlNodeList nodes = outPut.SelectNodes($"\\test-case[@fullname={testResult.FullName}]");

            XmlNode node = nodes[0];

            string result = node.Attributes["result"].Value;

            testResult.Passed = string.Compare(result, "Passed", true) > -1;

            string duration = node.Attributes["duration"].Value;

            testResult.Runtime = TimeSpan.Parse(duration);

            XmlNode outputNode = node.SelectSingleNode("reason/message");

            testResult.Output = outputNode.InnerText;
        }
    }
}

﻿using InContainerShared;
using MessageModels;
using NUnit.Framework;
using System;
using System.Xml;

namespace InContainerSharedTests
{
    /// <summary>
    /// See: https://github.com/nunit/docs/wiki/Test-Result-XML-Format
    /// </summary>
    [TestFixture]
    public class NunitParserTests
    {
        XmlDocument _testResults;
        TestResult _testResult;
        NunitParser _sut;

        [SetUp]
        public void SetUp()
        {
            _testResults = new XmlDocument();
            _testResults.LoadXml(_testOutput);
            _testResult = new TestResult();
            _sut = new NunitParser();
        }

        [Test]
        public void PassPopulated()
        {
            TestResult res = new TestResult { FullName = "SystemUnderTest.TestClass.FifthTest" };

            _sut.PopulateTestResult(res, _testResults);

            Assert.IsTrue(res.Passed);
        }

        [Test]
        public void FailPopulated()
        {
            TestResult res = new TestResult { FullName = "SystemUnderTest.TestClass.FailingTest" };

            _sut.PopulateTestResult(res, _testResults);

            Assert.IsFalse(res.Passed);
        }

        [Test]
        public void RuntimePopulated()
        {
            TestResult res = new TestResult { FullName = "SystemUnderTest.TestClass.FifthTest" };

            _sut.PopulateTestResult(res, _testResults);
            //2018-08-20 07:55:00
            Assert.AreEqual(TimeSpan.FromSeconds(1), res.Runtime);
        }

        [Test]
        public void StartTimePopulated()
        {
            TestResult res = new TestResult { FullName = "SystemUnderTest.TestClass.FifthTest" };

            _sut.PopulateTestResult(res, _testResults);

            Assert.AreEqual("2018-08-20 07:55:00", res.StartDate.ToString("yyyy-MM-dd hh:mm:ss"));
        }

        [Test]
        public void EndTimePopulated()
        {
            TestResult res = new TestResult { FullName = "SystemUnderTest.TestClass.FifthTest" };

            _sut.PopulateTestResult(res, _testResults);

            Assert.AreEqual("2018-08-20 07:55:01", res.EndDate.ToString("yyyy-MM-dd hh:mm:ss"));
        }

        string _testOutput = @"{<start-run count='0'/><start-suite id=""2-1012"" parentId="""" name=""SystemUnderTest.dll"" fullname=""C:\Users\shawn\source\repos\sut\Publish\SystemUnderTest\UnitTests\SystemUnderTest.dll"" type=""Assembly""/><start-suite id=""2-1013"" parentId=""2-1012"" name=""SystemUnderTest"" fullname=""SystemUnderTest"" type=""TestSuite""/><start-suite id=""2-1000"" parentId=""2-1013"" name=""TestClass"" fullname=""SystemUnderTest.TestClass"" type=""TestFixture""/><start-test id=""2-1002"" parentId=""2-1000"" name=""FailingTest"" fullname=""SystemUnderTest.TestClass.FailingTest"" type=""TestMethod""/><test-case id=""2-1002"" name=""FailingTest"" fullname=""SystemUnderTest.TestClass.FailingTest"" methodname=""FailingTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""2119000476"" result=""Failed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.090259"" asserts=""0"" parentId=""2-1000""><failure><message><![CDATA[Your first failing test]]></message><stack-trace><![CDATA[   at SystemUnderTest.TestClass.FailingTest() in C:\Users\shawn\source\repos\sut\SystemUnderTest\TestClass.cs:line 24
]]></stack-trace></failure><assertions><assertion result=""Failed""><message><![CDATA[Your first failing test]]></message><stack-trace><![CDATA[   at SystemUnderTest.TestClass.FailingTest() in C:\Users\shawn\source\repos\sut\SystemUnderTest\TestClass.cs:line 24
]]></stack-trace></assertion></assertions></test-case><start-test id=""2-1005"" parentId=""2-1000"" name=""FifthTest"" fullname=""SystemUnderTest.TestClass.FifthTest"" type=""TestMethod""/><test-case id=""2-1005"" name=""FifthTest"" fullname=""SystemUnderTest.TestClass.FifthTest"" methodname=""FifthTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1964292888"" result=""Passed"" start-time=""2018-08-20 07:55:00Z"" end-time=""2018-08-20 07:55:01Z"" duration=""1"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><start-test id=""2-1004"" parentId=""2-1000"" name=""FourthTest"" fullname=""SystemUnderTest.TestClass.FourthTest"" type=""TestMethod""/><test-case id=""2-1004"" name=""FourthTest"" fullname=""SystemUnderTest.TestClass.FourthTest"" methodname=""FourthTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1760657549"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.001414"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Fourth test passed]]></message></reason></test-case><start-test id=""2-1001"" parentId=""2-1000"" name=""PassingTest"" fullname=""SystemUnderTest.TestClass.PassingTest"" type=""TestMethod""/><test-case id=""2-1001"" name=""PassingTest"" fullname=""SystemUnderTest.TestClass.PassingTest"" methodname=""PassingTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""804625848"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.001182"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Your first passing test]]></message></reason></test-case><start-test id=""2-1010"" parentId=""2-1000"" name=""Test10"" fullname=""SystemUnderTest.TestClass.Test10"" type=""TestMethod""/><test-case id=""2-1010"" name=""Test10"" fullname=""SystemUnderTest.TestClass.Test10"" methodname=""Test10"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1897016657"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.001154"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><start-test id=""2-1011"" parentId=""2-1000"" name=""Test11"" fullname=""SystemUnderTest.TestClass.Test11"" type=""TestMethod""/><test-case id=""2-1011"" name=""Test11"" fullname=""SystemUnderTest.TestClass.Test11"" methodname=""Test11"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1286486632"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000818"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><start-test id=""2-1006"" parentId=""2-1000"" name=""Test6"" fullname=""SystemUnderTest.TestClass.Test6"" type=""TestMethod""/><test-case id=""2-1006"" name=""Test6"" fullname=""SystemUnderTest.TestClass.Test6"" methodname=""Test6"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""773928355"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000746"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><start-test id=""2-1007"" parentId=""2-1000"" name=""Test7"" fullname=""SystemUnderTest.TestClass.Test7"" type=""TestMethod""/><test-case id=""2-1007"" name=""Test7"" fullname=""SystemUnderTest.TestClass.Test7"" methodname=""Test7"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""417144179"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000892"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><start-test id=""2-1008"" parentId=""2-1000"" name=""Test8"" fullname=""SystemUnderTest.TestClass.Test8"" type=""TestMethod""/><test-case id=""2-1008"" name=""Test8"" fullname=""SystemUnderTest.TestClass.Test8"" methodname=""Test8"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1768549561"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000855"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><start-test id=""2-1009"" parentId=""2-1000"" name=""Test9"" fullname=""SystemUnderTest.TestClass.Test9"" type=""TestMethod""/><test-case id=""2-1009"" name=""Test9"" fullname=""SystemUnderTest.TestClass.Test9"" methodname=""Test9"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""729522526"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000863"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><start-test id=""2-1003"" parentId=""2-1000"" name=""ThirdTest"" fullname=""SystemUnderTest.TestClass.ThirdTest"" type=""TestMethod""/><test-case id=""2-1003"" name=""ThirdTest"" fullname=""SystemUnderTest.TestClass.ThirdTest"" methodname=""ThirdTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""181595431"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000906"" asserts=""0"" parentId=""2-1000""><reason><message><![CDATA[Third test passed]]></message></reason></test-case><test-suite type=""TestFixture"" id=""2-1000"" name=""TestClass"" fullname=""SystemUnderTest.TestClass"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" testcasecount=""11"" result=""Failed"" site=""Child"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.117728"" total=""11"" passed=""10"" failed=""1"" warnings=""0"" inconclusive=""0"" skipped=""0"" asserts=""0"" parentId=""2-1013""><failure><message><![CDATA[One or more child tests had errors]]></message></failure></test-suite><test-suite type=""TestSuite"" id=""2-1013"" name=""SystemUnderTest"" fullname=""SystemUnderTest"" runstate=""Runnable"" testcasecount=""11"" result=""Failed"" site=""Child"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.123703"" total=""11"" passed=""10"" failed=""1"" warnings=""0"" inconclusive=""0"" skipped=""0"" asserts=""0"" parentId=""2-1012""><failure><message><![CDATA[One or more child tests had errors]]></message></failure></test-suite><test-suite type=""Assembly"" id=""2-1012"" name=""SystemUnderTest.dll"" fullname=""C:\Users\shawn\source\repos\sut\Publish\SystemUnderTest\UnitTests\SystemUnderTest.dll"" runstate=""Runnable"" testcasecount=""11"" result=""Failed"" site=""Child"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.147378"" total=""11"" passed=""10"" failed=""1"" warnings=""0"" inconclusive=""0"" skipped=""0"" asserts=""0"" parentId=""""><properties><property name=""_PID"" value=""12964"" /><property name=""_APPDOMAIN"" value=""domain-9aabecf1-SystemUnderTest.dll"" /></properties><failure><message><![CDATA[One or more child tests had errors]]></message></failure></test-suite><test-suite type=""Assembly"" id=""2-1012"" name=""SystemUnderTest.dll"" fullname=""C:\Users\shawn\source\repos\sut\Publish\SystemUnderTest\UnitTests\SystemUnderTest.dll"" runstate=""Runnable"" testcasecount=""11"" result=""Failed"" site=""Child"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.147378"" total=""11"" passed=""10"" failed=""1"" warnings=""0"" inconclusive=""0"" skipped=""0"" asserts=""0""><environment framework-version=""3.10.1.0"" clr-version=""4.0.30319.42000"" os-version=""Microsoft Windows NT 10.0.17134.0"" platform=""Win32NT"" cwd=""C:\Users\shawn\source\repos\TestNUnitRunner\Publish\InContainer"" machine-name=""LAPRAT"" user=""shawn"" user-domain=""LAPRAT"" culture=""en-GB"" uiculture=""en-US"" os-architecture=""x64"" /><settings><setting name=""ImageRuntimeVersion"" value=""4.0.30319"" /><setting name=""ImageTargetFrameworkName"" value="".NETFramework,Version=v4.5"" /><setting name=""ImageRequiresX86"" value=""False"" /><setting name=""ImageRequiresDefaultAppDomainAssemblyResolver"" value=""False"" /><setting name=""NumberOfTestWorkers"" value=""8"" /></settings><properties><property name=""_PID"" value=""12964"" /><property name=""_APPDOMAIN"" value=""domain-9aabecf1-SystemUnderTest.dll"" /></properties><failure><message><![CDATA[One or more child tests had errors]]></message></failure><test-suite type=""TestSuite"" id=""2-1013"" name=""SystemUnderTest"" fullname=""SystemUnderTest"" runstate=""Runnable"" testcasecount=""11"" result=""Failed"" site=""Child"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.123703"" total=""11"" passed=""10"" failed=""1"" warnings=""0"" inconclusive=""0"" skipped=""0"" asserts=""0""><failure><message><![CDATA[One or more child tests had errors]]></message></failure><test-suite type=""TestFixture"" id=""2-1000"" name=""TestClass"" fullname=""SystemUnderTest.TestClass"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" testcasecount=""11"" result=""Failed"" site=""Child"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.117728"" total=""11"" passed=""10"" failed=""1"" warnings=""0"" inconclusive=""0"" skipped=""0"" asserts=""0""><failure><message><![CDATA[One or more child tests had errors]]></message></failure><test-case id=""2-1002"" name=""FailingTest"" fullname=""SystemUnderTest.TestClass.FailingTest"" methodname=""FailingTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""2119000476"" result=""Failed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.090259"" asserts=""0""><failure><message><![CDATA[Your first failing test]]></message><stack-trace><![CDATA[   at SystemUnderTest.TestClass.FailingTest() in C:\Users\shawn\source\repos\sut\SystemUnderTest\TestClass.cs:line 24
]]></stack-trace></failure><assertions><assertion result=""Failed""><message><![CDATA[Your first failing test]]></message><stack-trace><![CDATA[   at SystemUnderTest.TestClass.FailingTest() in C:\Users\shawn\source\repos\sut\SystemUnderTest\TestClass.cs:line 24
]]></stack-trace></assertion></assertions></test-case><test-case id=""2-1005"" name=""FifthTest"" fullname=""SystemUnderTest.TestClass.FifthTest"" methodname=""FifthTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1964292888"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.004251"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1004"" name=""FourthTest"" fullname=""SystemUnderTest.TestClass.FourthTest"" methodname=""FourthTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1760657549"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.001414"" asserts=""0""><reason><message><![CDATA[Fourth test passed]]></message></reason></test-case><test-case id=""2-1001"" name=""PassingTest"" fullname=""SystemUnderTest.TestClass.PassingTest"" methodname=""PassingTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""804625848"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.001182"" asserts=""0""><reason><message><![CDATA[Your first passing test]]></message></reason></test-case><test-case id=""2-1010"" name=""Test10"" fullname=""SystemUnderTest.TestClass.Test10"" methodname=""Test10"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1897016657"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.001154"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1011"" name=""Test11"" fullname=""SystemUnderTest.TestClass.Test11"" methodname=""Test11"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1286486632"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000818"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1006"" name=""Test6"" fullname=""SystemUnderTest.TestClass.Test6"" methodname=""Test6"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""773928355"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000746"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1007"" name=""Test7"" fullname=""SystemUnderTest.TestClass.Test7"" methodname=""Test7"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""417144179"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000892"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1008"" name=""Test8"" fullname=""SystemUnderTest.TestClass.Test8"" methodname=""Test8"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1768549561"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000855"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1009"" name=""Test9"" fullname=""SystemUnderTest.TestClass.Test9"" methodname=""Test9"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""729522526"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000863"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1003"" name=""ThirdTest"" fullname=""SystemUnderTest.TestClass.ThirdTest"" methodname=""ThirdTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""181595431"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000906"" asserts=""0""><reason><message><![CDATA[Third test passed]]></message></reason></test-case></test-suite></test-suite></test-suite><test-run id=""2"" testcasecount=""11"" result=""Failed"" total=""11"" passed=""10"" failed=""1"" inconclusive=""0"" skipped=""0"" asserts=""0"" engine-version=""3.8.0.0"" clr-version=""4.0.30319.42000"" start-time=""2018-08-20 07:55:02Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.891649""><command-line><![CDATA[""C:\Users\shawn\source\repos\TestNUnitRunner\Publish\InContainer\RunTestsLocally.exe"" ]]></command-line><test-suite type=""Assembly"" id=""1-1"" name=""nunit.framework.dll"" fullname=""C:\Users\shawn\source\repos\sut\Publish\SystemUnderTest\UnitTests\nunit.framework.dll"" testcasecount=""0"" runstate=""NotRunnable"" result=""Failed"" label=""Invalid""><properties><property name=""_SKIPREASON"" value=""No suitable tests found in 'C:\Users\shawn\source\repos\sut\Publish\SystemUnderTest\UnitTests\nunit.framework.dll'.&#xA;Either assembly contains no tests or proper test driver has not been found."" /></properties><reason><message>No suitable tests found in 'C:\Users\shawn\source\repos\sut\Publish\SystemUnderTest\UnitTests\nunit.framework.dll'.
Either assembly contains no tests or proper test driver has not been found.</message></reason></test-suite><test-suite type=""Assembly"" id=""2-1012"" name=""SystemUnderTest.dll"" fullname=""C:\Users\shawn\source\repos\sut\Publish\SystemUnderTest\UnitTests\SystemUnderTest.dll"" runstate=""Runnable"" testcasecount=""11"" result=""Failed"" site=""Child"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.147378"" total=""11"" passed=""10"" failed=""1"" warnings=""0"" inconclusive=""0"" skipped=""0"" asserts=""0""><environment framework-version=""3.10.1.0"" clr-version=""4.0.30319.42000"" os-version=""Microsoft Windows NT 10.0.17134.0"" platform=""Win32NT"" cwd=""C:\Users\shawn\source\repos\TestNUnitRunner\Publish\InContainer"" machine-name=""LAPRAT"" user=""shawn"" user-domain=""LAPRAT"" culture=""en-GB"" uiculture=""en-US"" os-architecture=""x64"" /><settings><setting name=""ImageRuntimeVersion"" value=""4.0.30319"" /><setting name=""ImageTargetFrameworkName"" value="".NETFramework,Version=v4.5"" /><setting name=""ImageRequiresX86"" value=""False"" /><setting name=""ImageRequiresDefaultAppDomainAssemblyResolver"" value=""False"" /><setting name=""NumberOfTestWorkers"" value=""8"" /></settings><properties><property name=""_PID"" value=""12964"" /><property name=""_APPDOMAIN"" value=""domain-9aabecf1-SystemUnderTest.dll"" /></properties><failure><message><![CDATA[One or more child tests had errors]]></message></failure><test-suite type=""TestSuite"" id=""2-1013"" name=""SystemUnderTest"" fullname=""SystemUnderTest"" runstate=""Runnable"" testcasecount=""11"" result=""Failed"" site=""Child"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.123703"" total=""11"" passed=""10"" failed=""1"" warnings=""0"" inconclusive=""0"" skipped=""0"" asserts=""0""><failure><message><![CDATA[One or more child tests had errors]]></message></failure><test-suite type=""TestFixture"" id=""2-1000"" name=""TestClass"" fullname=""SystemUnderTest.TestClass"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" testcasecount=""11"" result=""Failed"" site=""Child"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.117728"" total=""11"" passed=""10"" failed=""1"" warnings=""0"" inconclusive=""0"" skipped=""0"" asserts=""0""><failure><message><![CDATA[One or more child tests had errors]]></message></failure><test-case id=""2-1002"" name=""FailingTest"" fullname=""SystemUnderTest.TestClass.FailingTest"" methodname=""FailingTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""2119000476"" result=""Failed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.090259"" asserts=""0""><failure><message><![CDATA[Your first failing test]]></message><stack-trace><![CDATA[   at SystemUnderTest.TestClass.FailingTest() in C:\Users\shawn\source\repos\sut\SystemUnderTest\TestClass.cs:line 24
]]></stack-trace></failure><assertions><assertion result=""Failed""><message><![CDATA[Your first failing test]]></message><stack-trace><![CDATA[   at SystemUnderTest.TestClass.FailingTest() in C:\Users\shawn\source\repos\sut\SystemUnderTest\TestClass.cs:line 24
]]></stack-trace></assertion></assertions></test-case><test-case id=""2-1005"" name=""FifthTest"" fullname=""SystemUnderTest.TestClass.FifthTest"" methodname=""FifthTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1964292888"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.004251"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1004"" name=""FourthTest"" fullname=""SystemUnderTest.TestClass.FourthTest"" methodname=""FourthTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1760657549"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.001414"" asserts=""0""><reason><message><![CDATA[Fourth test passed]]></message></reason></test-case><test-case id=""2-1001"" name=""PassingTest"" fullname=""SystemUnderTest.TestClass.PassingTest"" methodname=""PassingTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""804625848"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.001182"" asserts=""0""><reason><message><![CDATA[Your first passing test]]></message></reason></test-case><test-case id=""2-1010"" name=""Test10"" fullname=""SystemUnderTest.TestClass.Test10"" methodname=""Test10"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1897016657"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.001154"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1011"" name=""Test11"" fullname=""SystemUnderTest.TestClass.Test11"" methodname=""Test11"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1286486632"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000818"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1006"" name=""Test6"" fullname=""SystemUnderTest.TestClass.Test6"" methodname=""Test6"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""773928355"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000746"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1007"" name=""Test7"" fullname=""SystemUnderTest.TestClass.Test7"" methodname=""Test7"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""417144179"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000892"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1008"" name=""Test8"" fullname=""SystemUnderTest.TestClass.Test8"" methodname=""Test8"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""1768549561"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000855"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1009"" name=""Test9"" fullname=""SystemUnderTest.TestClass.Test9"" methodname=""Test9"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""729522526"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000863"" asserts=""0""><reason><message><![CDATA[Fifth test passed]]></message></reason></test-case><test-case id=""2-1003"" name=""ThirdTest"" fullname=""SystemUnderTest.TestClass.ThirdTest"" methodname=""ThirdTest"" classname=""SystemUnderTest.TestClass"" runstate=""Runnable"" seed=""181595431"" result=""Passed"" start-time=""2018-08-20 07:55:03Z"" end-time=""2018-08-20 07:55:03Z"" duration=""0.000906"" asserts=""0""><reason><message><![CDATA[Third test passed]]></message></reason></test-case></test-suite></test-suite></test-suite></test-run>}";

    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemUnderTest
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void PassingTest()
        {
            // TODO: Add your test code here
            Assert.Pass("Your first passing test");
        }

        [Test]
        public void FailingTest()
        {
            // TODO: Add your test code here
            Assert.Fail("Your first failing test");
        }
    }
}

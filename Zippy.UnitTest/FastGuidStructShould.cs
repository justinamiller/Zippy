using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy.Serialize;

namespace Zippy.UnitTest
{
    [TestClass]
    public class FastGuidStructShould
    {
        [TestMethod]
        public void TestFastGuidStruct()
        {
            var guid = Guid.NewGuid();
            var g = new FastGuidStruct(guid);
            Assert.AreEqual(g.Raw, guid);
            Assert.AreEqual(g.ToString(), guid.ToString());

        }
    }
}

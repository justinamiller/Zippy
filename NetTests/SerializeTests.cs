using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTests.TestObjects;
using Zippy;

namespace NetTests
{
    [TestClass]
    public class SerializeTests
    {
        private LargeObject _largeObject = new LargeObject();

        [TestMethod]
        public void TestNull()
        {
            Assert.AreEqual(Zippy.JSON.SerializeObject(null, new StringWriter()), null);
            Assert.AreEqual(Zippy.JSON.SerializeObjectToString(null), null);
        }

        [TestMethod]
        public void TestSerializeObjectComplexObject()
        {
            var sw = new StringWriter();
            Zippy.JSON.SerializeObject(_largeObject, sw);
            Assert.IsTrue(sw.ToString().Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringComplexObject()
        {
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(_largeObject).Length > 0);
        }
    }
}

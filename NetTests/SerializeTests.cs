using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy;

namespace NetTests
{


    [TestClass]
    public class SerializeTests
    {
        private static Models.ComplexModelObject _ComplexModelObject = new Models.ComplexModelObject();
        private static Models.SimpleModelType _SimpleModelType = new Models.SimpleModelType();
        private static Models.ModelWithCommonTypes _ModelWithCommonTypes = Models.ModelWithCommonTypes.Create(3);


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
            Zippy.JSON.SerializeObject(_ComplexModelObject, sw);
            Assert.IsTrue(sw.ToString().Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringComplexObject()
        {
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(_ComplexModelObject).Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectSimpleModelType()
        {
            var sw = new StringWriter();
            Zippy.JSON.SerializeObject(_SimpleModelType, sw);
            Assert.IsTrue(sw.ToString().Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringSimpleModelType()
        {
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(_SimpleModelType).Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectWithCommonTypes()
        {
            var sw = new StringWriter();
            Zippy.JSON.SerializeObject(_ModelWithCommonTypes, sw);
            Assert.IsTrue(sw.ToString().Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringWithCommonTypes()
        {
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(_ModelWithCommonTypes).Length > 0);
        }
    }
}

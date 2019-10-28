using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy;

namespace NetTests
{
    [TestClass]
    public class JSONShould
    {
        [TestMethod]
        public void TestSerializeObjectToString()
        {
            Assert.IsNull(JSON.SerializeObjectToString(null));
            Assert.AreEqual(JSON.SerializeObjectToString(""),"\"\"");
        }

        [TestMethod]
        public void TestOptions()
        {
            Assert.IsNotNull(JSON.Options);
        }

        private static Models.SimpleModelType _SimpleModelType = new Models.SimpleModelType();
        [TestMethod]
        public void TestSerializeObject()
        {
    
        Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));
            try
            {
                Assert.IsNull(JSON.SerializeObject(_SimpleModelType, null));
            }
            catch(Exception) { }

            try
            {
                Assert.IsNull(JSON.SerializeObject(null, null));
            }
            catch (Exception) { }

     
            Assert.IsNull(JSON.SerializeObject(null, new System.IO.StringWriter()));
        }
    }
}

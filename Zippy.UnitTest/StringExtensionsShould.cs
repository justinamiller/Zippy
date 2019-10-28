using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy.Utility;
namespace Zippy.UnitTest
{
    [TestClass]
    public class StringExtensionsShould
    {
        [TestMethod]
        public void TestIsNullOrEmpty()
        {
            Assert.IsTrue(((string)null).IsNullOrEmpty());
            Assert.IsTrue("".IsNullOrEmpty());
            Assert.IsFalse("a".IsNullOrEmpty());
        }

        [TestMethod]
        public void TestIsNullOrWhiteSpace()
        {
            Assert.IsTrue(((string)null).IsNullOrWhiteSpace());
            Assert.IsTrue("".IsNullOrWhiteSpace());
            Assert.IsFalse("a".IsNullOrWhiteSpace());
            Assert.IsFalse("   a".IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void TestGetEncodeString()
        {
            for(var i=0; i < 1000; i++)
            {
              var a=  StringExtension.GetEncodeString(((char)i).ToString(), false, false);
          var b=     StringExtension.GetEncodeString(((char)i).ToString(), true, false);
           var c=  StringExtension.GetEncodeString(((char)i).ToString(), true, true);
        var d=    StringExtension.GetEncodeString(((char)i).ToString(), false, true);
            }
        }

        [TestMethod]
        public void TestToCamelCase()
        {
            Assert.AreEqual("".ToCamelCase(), "");
            Assert.AreEqual("ISPropertyTest".ToCamelCase(), "isPropertyTest");
            Assert.AreEqual("isPropertyTest".ToCamelCase(), "isPropertyTest");
        }

        [TestMethod]
        public void TestToLowercaseUnderscore()
        {
            Assert.AreEqual("".ToLowercaseUnderscore(), "");
            Assert.AreEqual("ISPropertyTest".ToLowercaseUnderscore(), "is_property_test");
            Assert.AreEqual("isPropertyTest".ToLowercaseUnderscore(), "is_property_test");
        }
    }
}

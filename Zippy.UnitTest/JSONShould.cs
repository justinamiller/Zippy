using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy;
using Zippy.Serialize;

namespace NetTests
{
    [TestClass]
    public class JSONShould
    {
        [TestMethod]
        public void TestSerializeObjectToString()
        {
            Assert.IsNull(JSON.SerializeObjectToString(null));
            Assert.IsTrue(JSON.SerializeObjectToString("").Length>0);
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

        [TestMethod]
        public void TestSerializeObjectOptions()
        {
            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.TextCase = TextCase.CamelCase;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));
            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.TextCase = TextCase.SnakeCase;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));
            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.TextCase = TextCase.Default;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));
            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.EscapeHtmlChars = true;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));
            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.PrettyPrint = true;
            JSON.Options.SerializationErrorHandling = SerializationErrorHandling.ReportValueAsNull;
            JSON.Options.DateHandler = DateHandler.ISO8601;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));


            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.DateHandler = DateHandler.ISO8601;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));

            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.DateHandler = DateHandler.ISO8601DateOnly;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));

            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.DateHandler = DateHandler.ISO8601DateTime;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));

            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.DateHandler = DateHandler.RFC1123;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));

            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            JSON.Options.DateHandler = DateHandler.TimestampOffset;
            Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType, new System.IO.StringWriter()));

            Zippy.Options.CurrentJsonSerializerStrategy.Reset();
            var str1 = JSON.SerializeObjectToString(_SimpleModelType);

            JSON.Options.ExcludeNulls = true;
            var str2 = JSON.SerializeObjectToString(_SimpleModelType);

            Assert.IsTrue(str1.Length != str2.Length);

            try
            {
                JSON.Options.RecursionLimit = -2;
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }

            try
            {
                JSON.Options.RecursionLimit =0;
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }

            //    Assert.IsNotNull(JSON.SerializeObject(_SimpleModelType,  new System.IO.StringWriter()));
            //var json1 = JSON.SerializeObjectToString(_SimpleModelType);
            //int currentLength = JSON.Options.MaxJsonLength;
            //JSON.Options.MaxJsonLength = 25;
            //var json = JSON.SerializeObjectToString(_SimpleModelType);
            //Assert.IsTrue(json1.Length>json.Length);

            //JSON.Options.MaxJsonLength = currentLength;

        }
    }
}

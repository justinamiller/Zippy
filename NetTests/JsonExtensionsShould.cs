using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

namespace NetTests
{
    [TestClass]
    public class JsonExtensionsShould
    {
        [TestMethod]
        public void TestBeautifyJson()
        {
            var a = new SimpleModelType().ToJson();
            Assert.IsNotNull(Newtonsoft.Json.JsonConvert.DeserializeObject(JsonExtensions.BeautifyJson(a)));

            Assert.IsNull(JsonExtensions.BeautifyJson(null));
        }

        [TestMethod]
        public void TestToJson()
        {
            var a = new SimpleModelType().ToJson();
            Assert.IsNotNull(a);

            Assert.IsNull(((object)null).ToJson());
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy.Serialize;

namespace Zippy.UnitTest
{
    [TestClass]
    public class JsonWriterShould
    {
        [TestMethod]
        public void TestJsonWriter()
        {
            var w = new JsonWriter(new System.IO.StringWriter());
            w.WriteBool(true);
            w.WriteBool(false);
            w.WriteBool(null);

            w.WriteByte((byte)0);
            w.WriteByte(null);

            w.WriteBytes(new byte[2] { 0, 1 });
            w.WriteBytes(null) ;
        }
    }
}

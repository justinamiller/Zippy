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
            Assert.IsTrue(w.ToString().Length == 0);
            w.WriteBool(true);
            w.WriteBool(false);
            w.WriteBool(null);

            w.WriteByte((byte)0);
            w.WriteByte(null);

            w.WriteBytes(new byte[2] { 0, 1 });
            w.WriteBytes(null) ;

            w.WriteInt16(null);
            w.WriteInt16((short)1);
            w.WriteDoubleNullable(null);
            w.WriteDoubleNullable((double)3);
            w.WriteInt32Nullable(null);
            w.WriteInt32Nullable(1);
            w.WriteInt64Nullable(null);
            w.WriteInt64Nullable((long)1);

            Assert.IsTrue(w.ToString().Length > 0);
        }
    }
}

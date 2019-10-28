using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy.Serialize.Writers;

namespace Zippy.UnitTest
{
    [TestClass]
    public class NullTextWriterShould
    {
        [TestMethod]
        public void TestNullTextWriter()
        {
            var sw = new NullTextWriter();
            Assert.IsNotNull(sw.FormatProvider);
            Assert.IsNotNull(sw.NewLine);
            Assert.IsNotNull(sw.Encoding);
            Assert.IsNull(sw.ToString());
            sw.Write("ABC");
            Assert.IsNull(sw.ToString());
            sw.Write('a');
            Assert.IsNull(sw.ToString());

            sw.Write(new char[3] { '1', '1', '2' }, 0, 3);
            Assert.IsNull(sw.ToString());

            sw.Write(new char[3] { '1', '1', '2' });
            Assert.IsNull(sw.ToString());

        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy.Serialize.Writers;

namespace Zippy.UnitTest
{
    [TestClass]
    public class StringBuilderWriterShould
    {
        [TestMethod]
        public void TestStringBuilderWriter()
        {
            var sw = new StringBuilderWriter();
            sw = new StringBuilderWriter(1);
            sw = new StringBuilderWriter(1234);

            sw = new StringBuilderWriter(-2);
            sw = new StringBuilderWriter(0);
            Assert.IsNotNull(sw.FormatProvider);
            Assert.IsNotNull(sw.NewLine);
            Assert.IsNotNull(sw.Encoding);
            Assert.IsTrue(sw.Length == 0);
            sw.Write("ABC");
            Assert.IsTrue(sw.Length == 3);
            sw.Write('a');
            Assert.IsTrue(sw.Length == 4);

            sw.Write(new char[3] { '1', '1', '2' }, 0, 3);
            Assert.IsTrue(sw.Length == 7);

            sw.Write(new char[3] { '1', '1', '2' });
            Assert.IsTrue(sw.ToString().Length == 10);
        }
    }
}

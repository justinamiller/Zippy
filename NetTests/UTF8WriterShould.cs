using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy.Serialize.Writers;

namespace Zippy.UnitTest
{
    [TestClass]
    public class UTF8WriterShould
    {
        [TestMethod]
        public void TestUTF8Writer()
        {
            var sw = new UTF8Writer();
            Assert.IsNotNull(sw.FormatProvider);
            Assert.IsNotNull(sw.NewLine);
            Assert.IsNotNull(sw.Encoding);
            Assert.IsTrue(sw.ToString().Length == 0);
            sw.Write("ABC");
            Assert.IsTrue(sw.ToString().Length == 3);
            sw.Write('a');
            Assert.IsTrue(sw.ToString().Length == 4);

            sw.Write(new char[3] { '1', '1', '2' }, 0, 3);
            Assert.IsTrue(sw.ToString().Length == 7);

            sw.Write(new char[3] { '1', '1', '2' });
            Assert.IsTrue(sw.ToString().Length == 10);

        }
    }
}

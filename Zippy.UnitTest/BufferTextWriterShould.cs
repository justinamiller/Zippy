using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy.Serialize.Writers;

namespace Zippy.UnitTest
{
    [TestClass]
    public class BufferTextWriterShould
    {
        [TestMethod]
        public void TestBufferTextWriter()
        {
            var sw = new BufferTextWriter();
            Assert.IsNotNull(sw.FormatProvider);
            Assert.IsNotNull(sw.NewLine);
            Assert.IsNotNull(sw.Encoding);
            Assert.IsTrue(sw.ToString().Length == 0);
            sw.Write("ABC");
            Assert.IsTrue(sw.ToString().Length == 3);
            sw.Write('a');
            Assert.IsTrue(sw.ToString().Length == 4);

            sw.Write(new char[3] { '1', '1', '2' },0,3);
            Assert.IsTrue(sw.ToString().Length == 7);

            sw.Write(new char[3] { '1', '1', '2' });
            Assert.IsTrue(sw.ToString().Length == 10);

        }
    }
}

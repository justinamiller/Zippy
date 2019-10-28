using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy.Internal;

namespace Zippy.UnitTest
{
    [TestClass]
    public class ValueMemberInfoShould
    {
        [TestMethod]
        public void TestValueMemberInfo()
        {
            var mi = typeof(Models.SimpleModelType).GetProperty("Test1");
            var t = new ValueMemberInfo(mi);
            Assert.IsTrue(t.IsType);
            Assert.IsNull(t.ExtendedValueInfo);
            Assert.AreEqual(t.Code, Utility.TypeSerializerUtils.TypeCode.String);
            Assert.IsFalse(t.GetHashCode() == 0);
            Assert.IsNotNull(t.ToString());
            Assert.AreEqual(t.ObjectType, typeof(string));
            Assert.IsNotNull(t.WriteDelegate);
            Assert.IsNotNull(t.Name);

            Assert.AreEqual(t, new ValueMemberInfo(mi));

            Assert.AreEqual(t, new ValueMemberInfo(typeof(Models.SimpleModelType).GetProperty("Test1")));

            Assert.AreNotEqual(t, new ValueMemberInfo(typeof(Models.SimpleModelType).GetProperty("Test2")));
            Assert.AreNotEqual(t, null);
        }
    }
}

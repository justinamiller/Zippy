using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zippy;

namespace NetTests
{


    [TestClass]
    public class SerializeTests
    {
        private static Models.ComplexModelObject _ComplexModelObject = new Models.ComplexModelObject();
        private static Models.SimpleModelType _SimpleModelType = new Models.SimpleModelType();
        private static Models.ModelWithCommonTypes _ModelWithCommonTypes = Models.ModelWithCommonTypes.Create(3);


        [TestMethod]
        public void TestNull()
        {
            Assert.AreEqual(Zippy.JSON.SerializeObject((object)null, new StringWriter()), null);
            Assert.AreEqual(Zippy.JSON.SerializeObjectToString((object)null), null);
        }

        [TestMethod]
        public void TestSerializeObjectComplexObject()
        {
            var sw = new StringWriter();
            Zippy.JSON.SerializeObject(_ComplexModelObject, sw);
            Assert.IsTrue(sw.ToString().Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringComplexObject()
        {
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(_ComplexModelObject).Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectSimpleModelType()
        {
            var sw = new StringWriter();
            Zippy.JSON.SerializeObject(_SimpleModelType, sw);
            Assert.IsTrue(sw.ToString().Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringSimpleModelType()
        {
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(_SimpleModelType).Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectWithCommonTypes()
        {
            var sw = new StringWriter();
            Zippy.JSON.SerializeObject(_ModelWithCommonTypes, sw);
            Assert.IsTrue(sw.ToString().Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringWithCommonTypes()
        {
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(_ModelWithCommonTypes).Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringDataSet()
        {
          var ds=new   System.Data.DataSet();
            var dt = new System.Data.DataTable();
            ds.Tables.Add(dt);
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Id", typeof(int));
            var dr = dt.NewRow();
            dr[0] = "HELLO";
            dr[1] = 1234;
            dt.Rows.Add(dr);

            dt = new System.Data.DataTable();
            ds.Tables.Add(dt);
            dt.Columns.Add("Name1", typeof(string));
            dt.Columns.Add("Id1", typeof(int));
             dr = dt.NewRow();
            dr[0] = "HELLO";
            dr[1] = 1234;
            dt.Rows.Add(dr);
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(ds).Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringDataTable ()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Data", typeof(object));
            var dr = dt.NewRow();
            dr[0] = "HELLO";
            dr[1] = 1234;
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dt.Rows.Add(dr);
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(dt).Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringAppDomain()
        {
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(System.AppDomain.CurrentDomain).Length > 0);
        }

        [TestMethod]
        public void TestSerializeObjectToStringNameValueCollection()
        {
            var a = new System.Collections.Specialized.NameValueCollection();
            a.Add("Test", "Me");
            a.Add("Test", "Me again");
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(a).Length > 0);
        }


        [TestMethod]
        public void TestSerializeObjectToStringMultidimensionalArray()
        {
            // Two-dimensional array.
            int[,] array2D = new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(array2D).Length > 0);

            // The same array with dimensions specified.
            int[,] array2Da = new int[4, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(array2Da).Length > 0);
            // A similar array with string elements.
            string[,] array2Db = new string[3, 2] { { "one", "two" }, { "three", "four" },
                                        { "five", "six" } };
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(array2Db).Length > 0);
            // Three-dimensional array.
            int[,,] array3D = new int[,,] { { { 1, 2, 3 }, { 4, 5, 6 } },
                                 { { 7, 8, 9 }, { 10, 11, 12 } } };
            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(array3D).Length > 0);
            // The same array with dimensions specified.
            int[,,] array3Da = new int[2, 2, 3] { { { 1, 2, 3 }, { 4, 5, 6 } },
                                       { { 7, 8, 9 }, { 10, 11, 12 } } };

            Assert.IsTrue(Zippy.JSON.SerializeObjectToString(array3Da).Length > 0);

        }
    }
}

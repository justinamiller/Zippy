using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class TestObject
    {
        public enum TType
        {
            None=0,
            Expert=1
        }

        public class Result
        {
            public bool IsGood { get; set; }
        }

        public string Version = "1.0";
        public System.Collections.IList Collect = new System.Collections.ArrayList();
        public Guid Id = Guid.NewGuid();
        public TType Type { get; set; } = TType.Expert;
        public string Name { get; set; }
        public string Address { get; set; }
        public bool IsReady { get; }
        public DateTime CreateDate { get; } = DateTime.Now;

        public DateTime CreateDateUTC { get; } = DateTime.UtcNow;
        public TimeSpan Duration {
            get
            {
                return _sw.Elapsed;
            }
        }
        private Stopwatch _sw = Stopwatch.StartNew();
        public int[] Items { get; set; }
        public int? Age { get; set; } = 10;
        public short S { get; }
        public double D { get; }
        public byte B { get; }
        public sbyte SB { get; }

        //    public IDictionary<string, string> Data = new Dictionary<string, string>();
        //  public DataTable DT = new DataTable();
        //  public DataSet DS = new DataSet();
        public Result[] Results { get; set; } = Array.Empty<Result>();
        public IList<Result> ResultList { get; set; } = new List<Result>();
      //  public System.Collections.ArrayList ResultAL { get; set; } = new System.Collections.ArrayList();
       // public System.Collections.IList ResultSL { get; set; } = new System.Collections.ArrayList();
       public Dictionary<string, int> ChildrenAges { get; set; }=new Dictionary<string,int>();
        public System.Collections.Specialized.NameValueCollection Names = new System.Collections.Specialized.NameValueCollection();

        public System.Collections.IDictionary OldAges { get; set; } = new System.Collections.Hashtable();

        public TestObject()
        {
            this.Name = "Test";
            this.Address = "8755 Lakeview Terrace";

            this.Items = (new List<int>() { 1, 3, 5, 6, 7, 88 }).ToArray();

            ResultList.Add(new Result());
            ResultList.Add(new Result());
            Collect.Add(1);
            Collect.Add("Test");
            OldAges.Add("test", "data");
            ChildrenAges.Add("bill", 13);
            // Data.Add("Item1", "is ready");
            //    DT.Columns.Add("Column1", typeof(string));
            //   var r=DT.NewRow();
            //   r[0] = "Test";
            // DT.Rows.Add(r);
        }

        public static void Testing()
        {
            //do nothing
        }
    }

    public class SimpleClass
    {
        public string Name { get; }
        public int Age { get; }
        public bool Hired { get; }
        public SimpleClass()
        {
            this.Name = "Joe Pickett";
            this.Age = 30;
            this.Hired = true;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            int testCount = 100000;
            var data = new Dictionary<string, double>();

            //  System.AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

            var c = new SimpleClass(); //new TestObject();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            // var a1=  Newtonsoft.Json.JsonConvert.SerializeObject(c);
            //    var d = sw.Elapsed.TotalMilliseconds;
            //    sw.Restart();
            //    var a2 = JsonSerializer.Serializer.SerializeObject(c);
            //    var dd = sw.Elapsed.TotalMilliseconds;
            //    dd.ToString();

            //    sw.Restart();
            Newtonsoft.Json.JsonConvert.SerializeObject(c);
            //    var d1 = sw.Elapsed.TotalMilliseconds;
            //    sw.Restart();
            //    var aa2 = JsonSerializer.Serializer.SerializeObject(c);
            //    var dd1 = sw.Elapsed.TotalMilliseconds;

            Utf8Json.JsonSerializer.ToJsonString<object>(c);
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                Utf8Json.JsonSerializer.ToJsonString<object>(c);
            }
            data.Add("Utf8Json", sw.Elapsed.TotalMilliseconds);

            Jil.JSON.Serialize<object>(c);
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                Jil.JSON.Serialize<object>(c);
            }
            data.Add("Jil", sw.Elapsed.TotalMilliseconds);


            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                Newtonsoft.Json.JsonConvert.SerializeObject(c);
            }
            data.Add("Newtonsoft", sw.Elapsed.TotalMilliseconds);

            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                JsonSerializer.Serializer.SerializeObject(c);
            }
            data.Add("Serializer", sw.Elapsed.TotalMilliseconds);

            //        // serializable.
            ServiceStack.Text.Config.Defaults.IncludePublicFields = true;
            ServiceStack.Text.JsConfig.IncludePublicFields = true;
            var a4 = ServiceStack.Text.JsonSerializer.SerializeToString(c);
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                ServiceStack.Text.JsonSerializer.SerializeToString(c);
            }
            data.Add("ServiceStack", sw.Elapsed.TotalMilliseconds);

            var xyz = JsonSerializer.Serializer2.SerializeObjectToString(c);
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                JsonSerializer.Serializer2.SerializeObjectToString(c);
            }
            data.Add("Serializer2-stringwriter", sw.Elapsed.TotalMilliseconds);

            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                JsonSerializer.Serializer2.SerializeObjectToString2(c);
            }
            data.Add("Serializer2-bufferwriter", sw.Elapsed.TotalMilliseconds);


            foreach (var item in data.OrderBy(v => v.Value))
            {
                Console.WriteLine(item.Key + " : "  + item.Value.ToString("#,##0.00"));
            }


            c.ToString();


            

            Console.WriteLine("DONE");
            Console.ReadLine();
        }

        public class ContractlessSample
        {
            public int MyProperty1 { get; set; }
            public int MyProperty2 { get; set; }
        }


        //private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        //{
        //    var ex = e.Exception;

        //    throw new NotImplementedException();
        //}
    }
}

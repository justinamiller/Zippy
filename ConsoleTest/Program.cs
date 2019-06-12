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
            public Guid Id { get; } = Guid.NewGuid();
            public DateTime Date { get; } = DateTime.Now;
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
       // public DataTable DT = new DataTable();
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
        private string _test;
        public string test = "";
        public string Test1
        {
            get
            {
                return test;
            }
        }
        public string Test2
        {
            get
            {
                return _test;
            }
        }

        public string Name { get; }
        public int Age { get; }
        public bool Hired { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public long BigNumber = 1234565672234;
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
            System.Threading.Thread.Sleep(250);

            //var c = new SimpleClass(); //new TestObject();
            var c = new TestObject();
            var sb = new Zippy.Serialize.Writers.StringBuilderWriter();
            for (var i = 0; i < 100000; i++)
            {
                Zippy.JSON.SerializeObject(c, sb);
                sb.Clear();
            }
            Console.WriteLine("DONE");
            Console.ReadLine();
            return;


            for (var i = 0; i < 10; i++)
            {

                TestJson();
                TestWriters();
                Console.WriteLine("========================");
            }
            Console.WriteLine("DONE");
            Console.ReadLine();
        }

        static void TestWriters()
        {
            int testCount = 1000;
            var data = new Dictionary<string, double>();

                         var sw = System.Diagnostics.Stopwatch.StartNew();
            var charArray = new char[3] { 'a', 'b', 'c' };
            var sb0 = new Zippy.Serialize.Writers.StringBuilderWriter();
            sw.Restart();
            for (var i = 0; i < testCount * 2; i++)
            {
                sb0.Write('a');
                sb0.Write(charArray, 0, 3);
                sb0.Write("hello world");
            }
            data.Add("StringBuilderWriter", sw.Elapsed.TotalMilliseconds);


            var sb00 = new Zippy.Serialize.Writers.StringBuilderWriter(0);
            sw.Restart();
            for (var i = 0; i < testCount * 2; i++)
            {
                sb00.Write('a');
                sb00.Write(charArray, 0, 3);
                sb00.Write("hello world");
            }
            data.Add("StringBuilderWriter2", sw.Elapsed.TotalMilliseconds);

            var sb1 = new StringBuilder(1024);
            sw.Restart();
            for (var i = 0; i < testCount * 2; i++)
            {
                sb1.Append('a');
                sb1.Append(charArray, 0, 3);

                sb1.Append("hello world");
            }
            data.Add("StringBuilder", sw.Elapsed.TotalMilliseconds);



            var sb67 = new StringWriter(new StringBuilder(1024));
            sw.Restart();
            for (var i = 0; i < testCount * 2; i++)
            {
                sb67.Write('a');
                sb67.Write(charArray, 0, 3);
                sb67.Write("hello world");
            }
            data.Add("StringWriter", sw.Elapsed.TotalMilliseconds);

            sb67 = new StringWriter();
            sw.Restart();
            for (var i = 0; i < testCount * 2; i++)
            {
                sb67.Write('a');
                sb67.Write(charArray, 0, 3);
                sb67.Write("hello world");
            }
            data.Add("StringWriter2", sw.Elapsed.TotalMilliseconds);





            foreach (var item in data.OrderBy(v => v.Value))
            {
                Console.WriteLine(item.Key + " : " + item.Value.ToString("#,##0.00"));
            }


        }


        static void TestJson()
        {
            int testCount = 10000;
          //  var data = new Dictionary<string, double>();
            var data = new List<Tuple<string, double, string>>();
            //   var c = new SimpleClass(); 
            var c = new TestObject();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            string json = null;

            try
            {
                json = Utf8Json.JsonSerializer.ToJsonString<object>(c);
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    Utf8Json.JsonSerializer.ToJsonString<object>(c);
                }
                data.Add(new Tuple<string, double, string>("Utf8Json", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Utf8Json", Int16.MaxValue, ex.ToString()));
            }


            try
            {
                json = Jil.JSON.Serialize<object>(c);
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    Jil.JSON.Serialize<object>(c);
                }
                data.Add(new Tuple<string, double, string>("Jil", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Jil", Int16.MaxValue, ex.ToString()));
            }

            try
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(c);
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    Newtonsoft.Json.JsonConvert.SerializeObject(c);
                }
                data.Add(new Tuple<string, double, string>("Newtonsoft", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Newtonsoft", Int16.MaxValue, ex.ToString()));
            }

         
            try
            {
                var js = new System.Web.Script.Serialization.JavaScriptSerializer();
                json = js.Serialize(c);
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    js.Serialize(c);
                }
                data.Add(new Tuple<string, double, string>("JavaScriptSerializer", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("JavaScriptSerializer", Int16.MaxValue, ex.ToString()));
            }



            try
            {
                ServiceStack.Text.Config.Defaults.IncludePublicFields = true;
                ServiceStack.Text.JsConfig.IncludePublicFields = true;
                json = ServiceStack.Text.JsonSerializer.SerializeToString(c);
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    ServiceStack.Text.JsonSerializer.SerializeToString(c);
                }
                data.Add(new Tuple<string, double, string>("ServiceStack", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("ServiceStack", Int16.MaxValue, ex.ToString()));
            }

            try
            {
                json = Zippy.JSON.SerializeObjectToString(c);
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    Zippy.JSON.SerializeObjectToString(c);
                }
                data.Add(new Tuple<string, double, string>("Serializer.V2-StringWriter", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Serializer.V2-StringWriter", Int16.MaxValue, ex.ToString()));
            }



            try
            {
                var nullwriter = new Zippy.Serialize.Writers.NullTextWriter();
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    Zippy.JSON.SerializeObject(c, nullwriter);
                }
                data.Add(new Tuple<string, double, string>("Serializer.V2-NullWriter", sw.Elapsed.TotalMilliseconds, ""));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Serializer.V2-NullWriter", Int16.MaxValue, ex.ToString()));
            }

            try
            {
                var sb = new Zippy.Serialize.Writers.StringBuilderWriter();
                json = Zippy.JSON.SerializeObject(c, sb).ToString();
                sb.Clear();
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    Zippy.JSON.SerializeObject(c, sb);
                    sb.Clear();
                }
                data.Add(new Tuple<string, double, string>("Serializer.V2-StringBuilderWriter-512", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Serializer.V2-StringBuilderWriter-512", Int16.MaxValue, ex.ToString()));
            }



            try
            {
                var sb2 = new Zippy.Serialize.Writers.StringBuilderWriter(0);
            GC.KeepAlive(sb2);
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                Zippy.JSON.SerializeObject(c, sb2);
                sb2.Clear();
            }
            data.Add(new Tuple<string, double, string>("Serializer.V2-StringBuilderWriter-0", sw.Elapsed.TotalMilliseconds, json));
            }
            catch(Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Serializer.V2-StringBuilderWriter-0", Int16.MaxValue,ex.ToString()));
            }

            foreach (var item in data.OrderBy(v => v.Item2))
            {
                Console.WriteLine(item.Item1 + " : "  + (item.Item2==Int16.MaxValue ? "NA" : item.Item2.ToString("#,##0.00")) +  " | " + item.Item3.Length);
            }

            c.ToString();
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

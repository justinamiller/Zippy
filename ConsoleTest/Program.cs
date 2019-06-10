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
            //System.Threading.Thread.Sleep(250);

            //var c = new SimpleClass(); //new TestObject();
            //var sb = new JsonSerializer.StringBuilderWriter();
            //for (var i = 0; i < 1000; i++)
            //{
            //    JsonSerializer.Serializer2.SerializeObject(c, sb);
            //    sb.Clear();
            //}
            //Console.ReadLine();
            //return;


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
            var sb0 = new JsonSerializer.StringBuilderWriter();
            sw.Restart();
            for (var i = 0; i < testCount * 2; i++)
            {
                sb0.Write('a');
                sb0.Write(charArray, 0, 3);
                sb0.Write("hello world");
            }
            data.Add("StringBuilderWriter", sw.Elapsed.TotalMilliseconds);

            var sb00 = new JsonSerializer.StringBuilderWriter2();
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


            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            js.Serialize(c);
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                js.Serialize(c);
            }
            data.Add("JavaScriptSerializer", sw.Elapsed.TotalMilliseconds);

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

            //sw.Restart();
            //for (var i = 0; i < testCount; i++)
            //{
            //    JsonSerializer.Serializer2.SerializeObjectToString2(c);
            //}
            //data.Add("Serializer2-bufferwriter", sw.Elapsed.TotalMilliseconds);

            var z = SimpleJson.SerializeObject(c);
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                SimpleJson.SerializeObject(c);
            }
            data.Add("SimpleJson", sw.Elapsed.TotalMilliseconds);


            var nullwriter = new JsonSerializer.NullTextWriter();
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                JsonSerializer.Serializer2.SerializeObject(c, nullwriter);
            }
            data.Add("NullTextWriter", sw.Elapsed.TotalMilliseconds);


            var sb = new JsonSerializer.StringBuilderWriter();
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                JsonSerializer.Serializer2.SerializeObject(c, sb);
                sb.Clear();
            }
            data.Add("StringBuilderWriter", sw.Elapsed.TotalMilliseconds);

            var sb2 = new JsonSerializer.StringBuilderWriter2();
            GC.KeepAlive(sb2);
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                JsonSerializer.Serializer2.SerializeObject(c, sb2);
                sb2.Clear();
            }
            data.Add("StringBuilderWriter2", sw.Elapsed.TotalMilliseconds);


            //var charArray = new char[3] { 'a', 'b', 'c' };
            //var sb0 = new JsonSerializer.StringBuilderWriter();
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb0.Write('a');
            //    sb0.Write(charArray, 0, 3);
            //    sb0.Write("hello world");
            //}
            //data.Add("StringBuilderWriter", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);

            //var sb00 = new JsonSerializer.StringBuilderWriter2();
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb00.Write('a');
            //    sb00.Write(charArray, 0, 3);
            //    sb00.Write("hello world");
            //}
            //data.Add("StringBuilderWriter2", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);

            //var sb1 = new StringBuilder(1024);
            //sw.Restart();
            //for (var i = 0; i < testCount*2; i++)
            //{
            //    sb1.Append('a');
            //    sb1.Append(charArray, 0, 3);

            //    sb1.Append("hello world");
            //}
            //data.Add("StringBuilder", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);

            //var sb2 = new JsonSerializer.FastString.FastString(1024);
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb2.Append("hello world");
            //}
            //data.Add("FastString", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);


            //var sb3 = new JsonSerializer.FastString.StringBuffer(1024);
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb3.Append("hello world");
            //}
            //data.Add("StringBuffer", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);

            //var sb4 = new JsonSerializer.FastString.FastStringBuilder(1024);
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb4.Append("hello world");
            //}
            //data.Add("FastStringBuilder", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);

            //var sb5 = new JsonSerializer.FastString.FastStringBuilder2(1024);
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb5.Append("hello world");
            //}
            //data.Add("FastStringBuilder2", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);

            //var sb6 = new JsonSerializer.FastString.FastStringBuilder3(1024);
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb6.Append("hello world");
            //}
            //data.Add("FastStringBuilder3", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);

            //var sb67 = new StringWriter(new StringBuilder(1024));
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb67.Write('a');
            //    sb67.Write(charArray, 0, 3);
            //    sb67.Write("hello world");
            //}
            //data.Add("StringWriter", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);

            //sb67 = new StringWriter();
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb67.Write('a');
            //    sb67.Write(charArray, 0, 3);
            //    sb67.Write("hello world");
            //}
            //data.Add("StringWriter2", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);


            //var sb7 = new JsonSerializer.NullTextWriter();
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb7.Write('a');
            //    sb7.Write(charArray, 0, 3);
            //    sb7.Write("hello world");
            //}
            //data.Add("NullTextWriter", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);

            //var ms = new MemoryStream(1024);
            //var m = new System.IO.BufferedStream(ms);
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    m.WriteByte((byte)'a');

            //    var b = Encoding.Default.GetBytes(charArray, 0, 3);
            //    m.Write(b, 0, b.Length);

            //    b = Encoding.Default.GetBytes("hello world");
            //    m.Write(b,0,b.Length);

            //}
            //data.Add("BufferedStream", sw.Elapsed.TotalMilliseconds);
            //GC.Collect(GC.MaxGeneration);


            foreach (var item in data.OrderBy(v => v.Value))
            {
                Console.WriteLine(item.Key + " : "  + item.Value.ToString("#,##0.00"));
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

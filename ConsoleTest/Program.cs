using System;
using System.Collections.Generic;
using System.Data;
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

        public TType Type { get; set; } = TType.Expert;
        public string Name { get; set; }
        public string Address { get; set; }
        public int[] Items { get; set; }
        public int? Age { get; set; } = 10;
        public IDictionary<string, string> Data = new Dictionary<string, string>();
        public DataTable DT = new DataTable();
        public DataSet DS = new DataSet();
        public Result[] Results { get; set; } = Array.Empty<Result>();
        public IList<Result> ResultList { get; set; } = new List<Result>();
        public System.Collections.ArrayList ResultAL { get; set; } = new System.Collections.ArrayList();
        public System.Collections.IList ResultSL { get; set; } = new System.Collections.ArrayList();
        public Dictionary<string, int> ChildrenAges { get; set; }=new Dictionary<string,int>();
        public TestObject()
        {
            this.Name = "Test";
            this.Address = "Here";
            this.Items = (new List<int>() { 1, 3, 5, 6, 7, 88 }).ToArray();
            Data.Add("Item1", "is ready");
            DT.Columns.Add("Column1", typeof(string));
            var r=DT.NewRow();
            r[0] = "Test";
            DT.Rows.Add(r);
        }

        public void Testing()
        {

        }
        public bool IsReady()
        {
            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var c = new TestObject();

            var sw = System.Diagnostics.Stopwatch.StartNew();
          var a1=  Newtonsoft.Json.JsonConvert.SerializeObject(c);
            var d = sw.Elapsed.TotalMilliseconds;
            sw.Restart();
            var a2 = JsonSerializer.Serializer.SerializeObject(c);
            var dd = sw.Elapsed.TotalMilliseconds;
            dd.ToString();

            sw.Restart();
           Newtonsoft.Json.JsonConvert.SerializeObject(c);
            var d1 = sw.Elapsed.TotalMilliseconds;
            sw.Restart();
            var aa2 = JsonSerializer.Serializer.SerializeObject(c);
            var dd1 = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            for(var i = 0; i < 100000; i++)
            {
                Newtonsoft.Json.JsonConvert.SerializeObject(c);
            }
            var d2 = sw.Elapsed.TotalMilliseconds;
            sw.Restart();
            for (var i = 0; i < 100000; i++)
            {
                JsonSerializer.Serializer.SerializeObject(c);
            }
            var dd2 = sw.Elapsed.TotalMilliseconds;
            dd.ToString();

            dd.ToString();
        }
    }
}

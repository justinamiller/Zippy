using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class TestObject
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int[] Items { get; set; }
       
        public TestObject()
        {
            this.Name = "Test";
            this.Address = "Here";
            this.Items = (new List<int>() { 1, 3, 5, 6, 7, 88 }).ToArray();
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

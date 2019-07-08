using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Zippy.Serialize.Writers;

namespace ConsoleTest
{
    class Program
    {
        const int testCount = 10000;
        private readonly static TextWriter _nullWriter = new NullTextWriter();

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Test(object c)
        {
            //Zippy.JSON.SerializeObjectToString(c);
            Zippy.JSON.SerializeObject(c, _nullWriter);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Test1(object c)
        {
            //Zippy.JSON.SerializeObjectToString(c);
            for(var i=0; i < testCount; i++)
            {
                Zippy.JSON.SerializeObject(c, _nullWriter);
            }
        }

        static void Main(string[] args)
        {
         //   AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            System.Threading.Thread.Sleep(250);
            ////var c = new Models.ComplexModelObject();
            ////Zippy.JSON.SerializeObjectToString1(c);



            //   var c = new Models.ComplexModelObject();
            var c = Enumerable.Range(1000, 1000).Select(x => new Models.TinyObject("Name" + x.ToString(), x, Models.TinyObject.State.Running)).ToArray();
            Zippy.JSON.SerializeObjectToString(c);
         //   c = new Models.ComplexModelObject();
            Test(c);
       //     c = new Models.ComplexModelObject();
            Test1(c);
            return;

            //for (var i = 0; i < 10000; i++)
            //{
            //    Zippy.JSON.SerializeObjectToString(c);
            //}
            //Console.WriteLine("DONE");
            //Console.ReadLine();
            //return;

            for (var i = 0; i < 10; i++)
            {

                TestJson();
             //   TestWriters();
                Console.WriteLine("========================");
            }
            Console.WriteLine("DONE");
            Console.ReadLine();
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
            Console.ReadLine();
        }

        static void TestJson()
        {
          //  var data = new Dictionary<string, double>();
            var data = new List<Tuple<string, double, string>>();
          // var c = new Models.SimpleModelType(); 
           var c = new Models.ComplexModelObject();
            //var c = Models.ModelWithCommonTypes.Create(23);
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
                var rj = new Revenj.Serialization.JsonSerialization(new RevenjBinder());

                json = rj.Serialize(c);
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    json = rj.Serialize(c);
                }
                data.Add(new Tuple<string, double, string>("Revenj", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Revenj", Int16.MaxValue, ex.ToString()));
            }

            try
            {
                json = Zippy.JSON.SerializeObjectToString(c);
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    Zippy.JSON.SerializeObjectToString(c);
                }
                data.Add(new Tuple<string, double, string>("Zippy", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Zippy", Int16.MaxValue, ex.ToString()));
            }

            try
            {
                json = System.Text.Json.Serialization.JsonSerializer.ToString(c);
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    System.Text.Json.Serialization.JsonSerializer.ToString(c);
                }
                data.Add(new Tuple<string, double, string>("System.Text.Json", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("System.Text.Json", Int16.MaxValue, ex.ToString()));
            }

            try
            {
                var sb2 = new StringWriter();
            GC.KeepAlive(sb2);
            sw.Restart();
            for (var i = 0; i < testCount; i++)
            {
                Zippy.JSON.SerializeObject(c, sb2);
                    sw.Stop();
               sb2.GetStringBuilder().Length = 0;
                    sw.Start();
            }
            data.Add(new Tuple<string, double, string>("Zippy-StringWriter", sw.Elapsed.TotalMilliseconds, json));
            }
            catch(Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Zippy-StringWriter", Int16.MaxValue,ex.ToString()));
            }

            try
            {
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    Zippy.JSON.SerializeObject(c, _nullWriter);
                }
                data.Add(new Tuple<string, double, string>("Zippy-NullWriter", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Zippy-NullWriter", Int16.MaxValue, ex.ToString()));
            }

            try
            {
                var buffer = new Zippy.Serialize.Writers.BufferTextWriter();
                sw.Restart();
                for (var i = 0; i < testCount; i++)
                {
                    Zippy.JSON.SerializeObject(c, buffer);
                    sw.Stop();
                    buffer = new Zippy.Serialize.Writers.BufferTextWriter();
                    sw.Start();
                }
                data.Add(new Tuple<string, double, string>("Zippy-BufferText", sw.Elapsed.TotalMilliseconds, json));
            }
            catch (Exception ex)
            {
                data.Add(new Tuple<string, double, string>("Zippy-BufferText", Int16.MaxValue, ex.ToString()));
            }

            foreach (var item in data.OrderBy(v => v.Item2))
            {
                Console.WriteLine(item.Item1 + " : "  + (item.Item2==Int16.MaxValue ? "NA" : item.Item2.ToString("#,##0.00")) +  " | " + item.Item3.Length);
            }

            c.ToString();
        }

        class RevenjBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return null;
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.FullName;
            }
        }

        static void TestWriters()
        {
            int testCount = 1000;
            var data = new Dictionary<string, double>();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var charArray = new char[3] { 'a', 'b', 'c' };
            //var sb0 = new Zippy.Serialize.Writers.StringBuilderWriter();
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb0.Write('a');
            //    sb0.Write(charArray, 0, 3);
            //    sb0.Write("hello world");
            //}
            //data.Add("StringBuilderWriter", sw.Elapsed.TotalMilliseconds);


            //var sb00 = new Zippy.Serialize.Writers.StringBuilderWriter(0);
            //sw.Restart();
            //for (var i = 0; i < testCount * 2; i++)
            //{
            //    sb00.Write('a');
            //    sb00.Write(charArray, 0, 3);
            //    sb00.Write("hello world");
            //}
            //data.Add("StringBuilderWriter2", sw.Elapsed.TotalMilliseconds);

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

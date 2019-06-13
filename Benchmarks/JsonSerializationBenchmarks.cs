using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks
{
    //[ShortRunJob]
    [RankColumn, HtmlExporter, CsvExporter]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Arabic)]
    [RankColumn(NumeralSystem.Stars)]
    public class JsonSerializationBenchmarks
    {
        private static Models.ComplexModelObject _ComplexModelObject=new Models.ComplexModelObject();
        private static Models.SimpleModelType _SimpleModelType = new Models.SimpleModelType();
        private static Models.ModelWithCommonTypes _ModelWithCommonTypes = Models.ModelWithCommonTypes.Create(3);

        static JsonSerializationBenchmarks()
        {
            ServiceStack.Text.Config.Defaults.IncludePublicFields = true;
            ServiceStack.Text.JsConfig.IncludePublicFields = true;
        }

        //[Benchmark]
        //public void Zippy_ComplexModelObject()
        //{
        //    string result = Zippy.JSON.SerializeObjectToString(_ComplexModelObject);
        //}

        //[Benchmark]
        //public void Zippy_SimpleModelType()
        //{
        //    string result = Zippy.JSON.SerializeObjectToString(_SimpleModelType);
        //}

        [Benchmark]
        public void Zippy_ModelWithCommonType()
        {
            string result = Zippy.JSON.SerializeObjectToString(_ModelWithCommonTypes);
        }

        //[Benchmark]
        //public void NewtonSoft_ComplexModelObject()
        //{
        //    string result = Newtonsoft.Json.JsonConvert.SerializeObject(_ComplexModelObject);
        //}

        //[Benchmark]
        //public void NewtonSoft_SimpleModelType()
        //{
        //    string result = Newtonsoft.Json.JsonConvert.SerializeObject(_SimpleModelType); 
        //}

        [Benchmark]
        public void NewtonSoft_ModelWithCommonType()
        {
            string result = Newtonsoft.Json.JsonConvert.SerializeObject(_ModelWithCommonTypes);
        }

        //[Benchmark]
        //public void Jil_ComplexModelObject()
        //{
        //    string result = Jil.JSON.Serialize<object>(_ComplexModelObject);
        //}

        //[Benchmark]
        //public void Jil_SimpleModelType()
        //{
        //    string result = Jil.JSON.Serialize<object>(_SimpleModelType);
        //}

        [Benchmark]
        public void Jil_ModelWithCommonType()
        {
            string result = Jil.JSON.Serialize<object>(_ModelWithCommonTypes);
        }

        //[Benchmark]
        //public void ServiceStack_ComplexModelObject()
        //{
        //    string result = ServiceStack.Text.JsonSerializer.SerializeToString(_ComplexModelObject);
        //}

        //[Benchmark]
        //public void ServiceStack_SimpleModelType()
        //{
        //    string result = ServiceStack.Text.JsonSerializer.SerializeToString(_SimpleModelType);
        //}

        [Benchmark]
        public void ServiceStack_ModelWithCommonType()
        {
            string result = ServiceStack.Text.JsonSerializer.SerializeToString(_ModelWithCommonTypes);
        }
    }
}

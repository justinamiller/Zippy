using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<JsonSerializationBenchmarks>(
               ManualConfig
                 .Create(DefaultConfig.Instance)
                 //.With(Job.RyuJitX64)
                .With(Job.Clr)
           //      .With(new BenchmarkDotNet.Diagnosers.CompositeDiagnoser())
          // .With(new BenchmarkDotNet.Diagnosers.CompositeDiagnoser())
                 .With(ExecutionValidator.FailOnError)
             );
        }
    }
}

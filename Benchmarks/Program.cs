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
          //  AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            BenchmarkRunner.Run<JsonSerializationBenchmarks>(
               ManualConfig
                 .Create(DefaultConfig.Instance)
                 //.With(Job.RyuJitX64)
                .With(Job.Clr)
           //      .With(new BenchmarkDotNet.Diagnosers.CompositeDiagnoser())
          // .With(new BenchmarkDotNet.Diagnosers.CompositeDiagnoser())
                 .With(ExecutionValidator.FailOnError)
             );
            Console.WriteLine("DONE!!!!");
            Console.ReadLine();
           
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
            Console.ReadLine();
        }
    }
}

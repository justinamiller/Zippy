#### Zippy
.NET Standard, Fast JSON Serializer.

## Usage

### Serializing

```C#
using(var output = new StringWriter())
{
    JSON.Serialize(
        new
        {
            Id = 1,
            Message = "hello world",
            // etc.
        },
        output
    );
}
```

The first time Zippy is used to serialize a given configuration and type pair, it will spend extra time building the serializer.
Subsequent invocations will be much faster, so if a consistently fast runtime is necessary in your code you may want to "prime the pump"
with an earlier "throw away" serialization.

Built-in support types
---
These types can serialize by default.

Primitives(`int`, `string`, etc...), `Enum`, `Nullable<>`,  `TimeSpan`,  `DateTime`, `DateTimeOffset`, `Nil`, `Guid`, `Uri`, `Version`, `StringBuilder`, `BitArray`, `ArraySegment<>`, `BigInteger`, `Complext`, `Task`, `Array[]`, `Array[,]`, `Array[,,]`, `Array[,,,]`, `KeyValuePair<,>`, `Tuple<,...>`, `ValueTuple<,...>`, `List<>`, `LinkedList<>`, `Queue<>`, `Stack<>`, `HashSet<>`, `ReadOnlyCollection<>`, `IList<>`, `ICollection<>`, `IEnumerable<>`, `Dictionary<,>`, `IDictionary<,>`, `SortedDictionary<,>`, `SortedList<,>`, `ILookup<,>`, `IGrouping<,>`, `ObservableCollection<>`, `ReadOnlyOnservableCollection<>`, `IReadOnlyList<>`, `IReadOnlyCollection<>`, `ISet<>`, `ConcurrentBag<>`, `ConcurrentQueue<>`, `ConcurrentStack<>`, `ReadOnlyDictionary<,>`, `IReadOnlyDictionary<,>`, `ConcurrentDictionary<,>`, `Lazy<>`, `Task<>`, custom inherited `ICollection<>` or `IDictionary<,>` with paramterless constructor, `IList`, `IDictionary` and custom inherited `ICollection` or `IDictionary` with paramterless constructor(includes `ArrayList` and `Hashtable`), `DataTable`, `DataSet`

Serializes public fields and properties; the order in which they are serialized is not defined (it is unlikely to be in
declaration order).  The [`DataMemberAttribute.Name` property](http://msdn.microsoft.com/en-us/library/ms584759(v=vs.110).aspx) and [`IgnoreDataMemberAttribute`](http://msdn.microsoft.com/en-us/library/system.runtime.serialization.ignoredatamemberattribute.aspx) are respected by Zippy, as is the [ShouldSerializeXXX() pattern](http://msdn.microsoft.com/en-us/library/53b8022e(v=vs.110).aspx).  For situations where `DataMemberAttribute` and `IgnoreDataMemberAttribute` cannot be used, Zippy provides the `ZippyDirectiveAttribute` which provides equivalent functionality.

## Benchmarks

Zippy aims to be the fastest general purpose JSON serializer for .NET.  Flexibility and "nice to have" features are explicitly discounted in the pursuit of speed.

As with all benchmarks, take these with a grain of salt.

### Serialization

For comparison, here's how Zippy stacks up against other popular .NET serializers in a synthetic benchmark
 - [Json.NET](http://james.newtonking.com/json), version 6.0.7
 - [ServiceStack.Text](https://github.com/ServiceStack/ServiceStack.Text), version 3.9.71
 - [protobuf-net](https://code.google.com/p/protobuf-net/), version 2.0.0.688
   
## Configuration

#### Custom Date Formatting
 - ISO8601: can be unrolled into a smaller number of `/` and `%` instructions
 - RFC1123: can be similarly decomposed
 - Microsoft-style: is a subtraction and division, then fed into the custom `long` writing code
 - Milliseconds since the unix epoch: is essentially the same
 - Seconds since the unix epoch: just has a different divisor
 
 ## Benchmarks
 ``` ini
 BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.753 (1803/April2018Update/Redstone4)
Intel Core i7-6820HQ CPU 2.70GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
Frequency=2648441 Hz, Resolution=377.5806 ns, Timer=TSC
  [Host] : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3394.0
```

|                          Method |     Mean |     Error |     StdDev |   Median | Rank | Rank |
|-------------------------------- |---------:|----------:|-----------:|---------:|-----:|-----:|
|        Zippy_ComplexModelObject | 12.79 us | 0.1165 us |  0.0973 us | 12.80 us |    1 |    * |
| ServiceStack_ComplexModelObject | 15.99 us | 0.2597 us |  0.2303 us | 15.98 us |    2 |   ** |
|   NewtonSoft_ComplexModelObject | 18.58 us | 0.2679 us |  0.2506 us | 18.62 us |    3 |  *** |
|          Jil_ComplexModelObject | 50.17 us | 5.4556 us | 15.9142 us | 44.74 us |    4 | **** |
 
   
   



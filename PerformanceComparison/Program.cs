
using MsgPack.Serialization;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ZeroFormatter;

[ZeroFormattable]
[Serializable]
[ProtoContract]
[DataContract]
public class Person : IEquatable<Person>
{
    [Index(0)]
    [MessagePackMember(0)]
    [ProtoMember(1)]
    [DataMember]
    public virtual int Age { get; set; }
    [Index(1)]
    [MessagePackMember(1)]
    [ProtoMember(2)]
    [DataMember]
    public virtual string FirstName { get; set; }
    [Index(2)]
    [MessagePackMember(2)]
    [ProtoMember(3)]
    [DataMember]
    public virtual string LastName { get; set; }
    [Index(3)]
    [MessagePackMember(3)]
    [ProtoMember(4)]
    [DataMember]
    public virtual Sex Sex { get; set; }

    public bool Equals(Person other)
    {
        return Age == other.Age && FirstName == other.FirstName && LastName == other.LastName && Sex == other.Sex;
    }
}

public enum Sex : sbyte
{
    Unknown, Male, Female,
}

[ZeroFormattable]
[Serializable]
[ProtoContract]
[DataContract]
public struct Vector3
{
    [Index(0)]
    [MessagePackMember(0)]
    [ProtoMember(1)]
    [DataMember]
    public long x;

    [Index(1)]
    [MessagePackMember(1)]
    [ProtoMember(2)]
    [DataMember]
    public long y;

    [Index(2)]
    [MessagePackMember(3)]
    [ProtoMember(3)]
    [DataMember]
    public long z;

    public Vector3(long x, long y, long z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

class Program
{
    const int WarmUpIteration = 2;
    const int RunIteration = 1000;//10000;

    static int Iteration = 1;
    static bool dryRun = true;

    static NetSerializer.Serializer netSerializer;

    static void Main(string[] args)
    {
        var p = new Person
        {
            Age = 99999,
            FirstName = "Windows",
            LastName = "Server",
            Sex = Sex.Male,
        };

        IList<Person> l = Enumerable.Range(1000, 1000).Select(x => new Person { Age = x, FirstName = "Windows", LastName = "Server", Sex = Sex.Female }).ToArray();

        var integer = 1;
        //   var v3 = new Vector3 { x = 12345.12345f, y = 3994.35226f, z = 325125.52426f };
        var v3 = new Vector3 { x = 12345, y = 3994, z = 325125 };
        // IList<Vector3> v3List = Enumerable.Range(1, 100).Select(_ => new Vector3 { x = 12345.12345f, y = 3994.35226f, z = 325125.52426f }).ToArray();
        IList<Vector3> v3List = Enumerable.Range(1, 100).Select(_ => new Vector3 { x = 12345, y = 3994, z = 325125}).ToArray();
        var largeString = File.ReadAllText("CSharpHtml.txt");

        netSerializer = new NetSerializer.Serializer(new[] { p.GetType(), typeof(Person[]), integer.GetType(), v3.GetType(), typeof(Vector3[]), largeString.GetType() });

        Iteration = WarmUpIteration;
        Console.WriteLine("Warming-up"); Console.WriteLine();
        SerializeZeroFormatter(p); SerializeZeroFormatter(l);
        SerializeZeroFormatter(integer); SerializeZeroFormatter(v3); SerializeZeroFormatter(largeString); SerializeZeroFormatter(v3List);
        SerializeProtobuf(p); SerializeProtobuf(l);
        SerializeProtobuf(integer); SerializeProtobuf(v3); SerializeProtobuf(largeString); SerializeProtobuf(v3List);
        SerializeMsgPack(p); SerializeMsgPack(l);
        SerializeMsgPack(integer); SerializeMsgPack(v3); SerializeMsgPack(largeString); SerializeMsgPack(v3List);
        SerializeJsonNet(p); SerializeJsonNet(l);
        SerializeJil(p); SerializeJil(l);
        SerializeFsPickler(p); SerializeFsPickler(l);
        SerializeBinaryFormatter(p); SerializeBinaryFormatter(l);
        SerializeDataContract(p); SerializeDataContract(l);
        SerializeWire(p); SerializeWire(l);
        SerializeWire(integer); SerializeWire(v3); SerializeWire(largeString); SerializeWire(v3List);
        SerializeNetSerializer(p); SerializeNetSerializer((Person[])(object)l);
        SerializeNetSerializer(integer); SerializeNetSerializer(v3); SerializeNetSerializer(largeString); SerializeNetSerializer((Vector3[])(object)v3List);

                    SerializeServiceStack(p); SerializeServiceStack((Person[])(object)l);
        SerializeServiceStack(integer); SerializeServiceStack(v3); SerializeServiceStack(largeString); SerializeServiceStack((Vector3[])(object)v3List);

        SerializeZippySerializer(p); SerializeZippySerializer((Person[])(object)l);
        SerializeZippySerializer(integer); SerializeZippySerializer(v3); SerializeZippySerializer(largeString); SerializeZippySerializer((Vector3[])(object)v3List);


        dryRun = false;
        Iteration = RunIteration;

        Console.WriteLine();
        Console.WriteLine($"Small Object(int,string,string,enum) {Iteration} Iteration"); Console.WriteLine();
        var a = SerializeZeroFormatter(p); Console.WriteLine();
        var b = SerializeProtobuf(p); Console.WriteLine();
        var c = SerializeMsgPack(p); Console.WriteLine();
        var d = SerializeJsonNet(p); Console.WriteLine();
        var e = SerializeJil(p); Console.WriteLine();
        var f = SerializeFsPickler(p); Console.WriteLine();
        var g = SerializeBinaryFormatter(p); Console.WriteLine();
        var h = SerializeDataContract(p); Console.WriteLine();
        var k = SerializeWire(p); Console.WriteLine();
        var l2 = SerializeNetSerializer(p); Console.WriteLine();
        var m = SerializeZippySerializer(p); Console.WriteLine();
     SerializeServiceStack(p); Console.WriteLine();


        Console.WriteLine($"Large Array(SmallObject[1000]) {Iteration} Iteration"); Console.WriteLine();
        var M = SerializeZippySerializer(l); Console.WriteLine();
        var A = SerializeZeroFormatter(l); Console.WriteLine();
        var B = SerializeProtobuf(l); Console.WriteLine();
        var C = SerializeMsgPack(l); Console.WriteLine();
        var D = SerializeJsonNet(l); Console.WriteLine();
        var E = SerializeJil(l); Console.WriteLine();
        var F = SerializeFsPickler(l); Console.WriteLine();
        var G = SerializeBinaryFormatter(l); Console.WriteLine();
        var H = SerializeDataContract(l); Console.WriteLine();
        var K = SerializeWire(l); Console.WriteLine();
        var L = SerializeNetSerializer((Person[])(object)l); Console.WriteLine();
        SerializeServiceStack((Person[])(object)l); Console.WriteLine();

        //Validate("ZeroFormatter", p, l, a, A);
        //Validate("protobuf-net", p, l, b, B);
        //Validate("MsgPack-CLI", p, l, c, C);
        //Validate("JSON.NET", p, l, d, D);
        //Validate("Jil", p, l, e, E);
        //Validate("FsPickler", p, l, f, F);
        //Validate("BinaryFormatter", p, l, g, G);
        //Validate("DataContract", p, l, h, H);
        //Validate("Wire", p, l, k, K);
        //Validate("NetSerializer", p, l, l2, L);
        //Validate("Zippy", p, l, m, M);

        Console.WriteLine();
        Console.WriteLine("Additional Benchmarks"); Console.WriteLine();

        Console.WriteLine($"Int32(1) {Iteration} Iteration"); Console.WriteLine();

        SerializeZeroFormatter(integer); Console.WriteLine();
        SerializeProtobuf(integer); Console.WriteLine();
        SerializeMsgPack(integer); Console.WriteLine();
        SerializeJsonNet(integer); Console.WriteLine();
        SerializeJil(integer); Console.WriteLine();
        SerializeFsPickler(integer); Console.WriteLine();
        SerializeBinaryFormatter(integer); Console.WriteLine();
        SerializeDataContract(integer); Console.WriteLine();
        SerializeWire(integer); Console.WriteLine();
        SerializeNetSerializer(integer); Console.WriteLine();
        SerializeZippySerializer(integer); Console.WriteLine();
        SerializeServiceStack(integer); Console.WriteLine();

        //var W1 = SerializeZeroFormatter(integer); Console.WriteLine();
        //var W2 = SerializeMsgPack(integer); Console.WriteLine();
        //var W3 = SerializeProtobuf(integer); Console.WriteLine();
        //var W4 = SerializeWire(integer); Console.WriteLine();
        //var W5 = SerializeNetSerializer(integer); Console.WriteLine();
        //var W6 = SerializeZippySerializer(integer); Console.WriteLine();

        Console.WriteLine($"Vector3(float, float, float) {Iteration} Iteration"); Console.WriteLine();

        SerializeZeroFormatter(v3); Console.WriteLine();
        SerializeProtobuf(v3); Console.WriteLine();
        SerializeMsgPack(v3); Console.WriteLine();
        SerializeJsonNet(v3); Console.WriteLine();
        SerializeJil(v3); Console.WriteLine();
        SerializeFsPickler(v3); Console.WriteLine();
        SerializeBinaryFormatter(v3); Console.WriteLine();
        SerializeDataContract(v3); Console.WriteLine();
        SerializeWire(v3); Console.WriteLine();
        SerializeNetSerializer(v3); Console.WriteLine();
        SerializeZippySerializer(v3); Console.WriteLine();
        SerializeServiceStack(v3); Console.WriteLine();

        Console.WriteLine($"HtmlString({Encoding.UTF8.GetByteCount(largeString)}bytes) {Iteration} Iteration"); Console.WriteLine();

        SerializeZeroFormatter(largeString); Console.WriteLine();
        SerializeProtobuf(largeString); Console.WriteLine();
        SerializeMsgPack(largeString); Console.WriteLine();
        SerializeJsonNet(largeString); Console.WriteLine();
        SerializeJil(largeString); Console.WriteLine();
        SerializeFsPickler(largeString); Console.WriteLine();
        SerializeBinaryFormatter(largeString); Console.WriteLine();
        SerializeDataContract(largeString); Console.WriteLine();
        SerializeWire(largeString); Console.WriteLine();
        SerializeNetSerializer(largeString); Console.WriteLine();
        SerializeZippySerializer(largeString); Console.WriteLine();
        SerializeServiceStack(largeString); Console.WriteLine();


        Console.WriteLine($"Vector3[100] {Iteration} Iteration"); Console.WriteLine();
        SerializeZeroFormatter(v3List); Console.WriteLine();
        SerializeProtobuf(v3List); Console.WriteLine();
        SerializeMsgPack(v3List); Console.WriteLine();
        SerializeJsonNet(v3List); Console.WriteLine();
        SerializeJil(v3List); Console.WriteLine();
        SerializeFsPickler(v3List); Console.WriteLine();
        SerializeBinaryFormatter(v3List); Console.WriteLine();
        SerializeDataContract(v3List); Console.WriteLine();
        SerializeWire(v3List); Console.WriteLine();
    //    SerializeNetSerializer(v3List); Console.WriteLine();
        SerializeZippySerializer(v3List); Console.WriteLine();
        SerializeServiceStack(v3List); Console.WriteLine();

        var order1 = Measure.Results.GroupBy(v => v.Key.Split('.')[1]);
        foreach(var data in order1)
        {
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(data.Key);
            Console.BackgroundColor = ConsoleColor.Black;
            int i = 1;
            foreach (var item in data.OrderBy(v=>v.Value))
            {
                if (item.Key.ToLower().Contains("zippy"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine(i.ToString() +") " + item.Key + " : " + item.Value.ToString("#,##0.00"));
                i++;
            }
        }

        Console.WriteLine();
        Console.WriteLine();

        var order=Measure.Results.OrderBy(v => v.Value);
        foreach(var item in order)
        {
            if (item.Key.ToLower().Contains("zippy"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine(item.Key + " : " + item.Value.ToString("#,##0.00"));
        }


        //Validate2("ZeroFormatter", W1, integer); Validate2("MsgPack-Cli", W2, integer); Validate2("MsgPack", W3, integer); Validate2("Wire", W4, integer); Validate2("NetSerializer", W5, integer); Validate2("Zippy", W6, integer);
        //Validate2("ZeroFormatter", X1, v3);  Validate2("MsgPack-Cli", X2, v3); Validate2("MsgPack", X3, v3); Validate2("Wire", X4, v3); Validate2("NetSerializer", X5, v3); Validate2("Zippy", X6, v3);
        //Validate2("ZeroFormatter", Y1, largeString); ; Validate2("MsgPack-Cli", Y2, largeString); Validate2("MsgPack", Y3, largeString); Validate2("Wire", Y4, largeString); Validate2("NetSerializer", Y5, largeString); Validate2("Zippy", Y6, largeString);
        //Validate2("ZeroFormatter", Z1, v3List); Validate2("MsgPack-Cli", Z2, v3List); Validate2("MsgPack", Z3, v3List); Validate2("Wire", Z4, v3List); Validate2("NetSerializer", Z5, v3List); Validate2("Zippy", Z6, v3List);

        Console.WriteLine("Press key to exit.");
        Console.ReadLine();
    }

    static void Validate(string label, Person original, IList<Person> originalList, Person copy, IList<Person> copyList)
    {
        if (!EqualityComparer<Person>.Default.Equals(original, copy)) Console.WriteLine(label + " Invalid Deserialize Small Object");
        if (!originalList.SequenceEqual(copyList)) Console.WriteLine(label + " Invalid Deserialize Large Array");
    }

    static void Validate2<T>(string label, T original, T copy)
    {
        if (!EqualityComparer<T>.Default.Equals(original, copy)) Console.WriteLine(label + " Invalid Deserialize");
    }

    static void Validate2<T>(string label, IList<T> original, IList<T> copy)
    {
        if (!original.SequenceEqual(copy)) Console.WriteLine(label + " Invalid Deserialize");
    }

    static T SerializeZeroFormatter<T>(T original)
    {
    //    Console.WriteLine("ZeroFormatter");

        T copy = default(T);
        byte[] bytes = null;

        var label = "ZeroFormatter." + original.GetType().Name;

        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                bytes = ZeroFormatterSerializer.Serialize(original);
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        copy = ZeroFormatterSerializer.Deserialize<T>(bytes);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(bytes.Length)));
        //}

        return copy;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static T SerializeZippySerializer<T>(T original)
    {
    //    Console.WriteLine("Zippy");

        T copy = default(T);
        MemoryStream stream = null;

        var nullwriter = new Zippy.Serialize.Writers.NullTextWriter();
        var label = "Zippy." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                //Zippy.JSON.SerializeObjectToString(original);
                Zippy.JSON.SerializeObject(original,nullwriter);
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        netSerializer.DeserializeDirect<T>(stream, out copy);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }

    static T SerializeNetSerializer<T>(T original)
    {
        //Console.WriteLine("NetSerializer");

        T copy = default(T);
        MemoryStream stream = null;
        var label = "NetSerializer." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                netSerializer.SerializeDirect<T>(stream = new MemoryStream(), original);
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        netSerializer.DeserializeDirect<T>(stream, out copy);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }

    static T SerializeServiceStack<T>(T original)
    {
        //Console.WriteLine("NetSerializer");
        ServiceStack.Text.Config.Defaults.IncludePublicFields = true;
        ServiceStack.Text.JsConfig.IncludePublicFields = true;
        T copy = default(T);
        MemoryStream stream = null;
        var label = "ServiceStack." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {

                ServiceStack.Text.JsonSerializer.SerializeToStream(original, stream = new MemoryStream());
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        netSerializer.DeserializeDirect<T>(stream, out copy);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }

    static Wire.Serializer wireSerializer = new Wire.Serializer();

    static T SerializeWire<T>(T original)
    {
      //  Console.WriteLine("Wire");

        T copy = default(T);
        MemoryStream stream = null;
        var label = "Wire." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                wireSerializer.Serialize(original, stream = new MemoryStream());
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        copy = wireSerializer.Deserialize<T>(stream);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }

    static T SerializeProtobuf<T>(T original)
    {
       // Console.WriteLine("protobuf-net");

        T copy = default(T);
        MemoryStream stream = null;
        var label = "protobuf-net." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                ProtoBuf.Serializer.Serialize<T>(stream = new MemoryStream(), original);
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        copy = ProtoBuf.Serializer.Deserialize<T>(stream);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }

    static T SerializeMsgPack<T>(T original)
    {
   //     Console.WriteLine("MsgPack-CLI");

        T copy = default(T);
        byte[] bytes = null;

        var label = "MsgPack-CLI." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                bytes = MsgPack.Serialization.MessagePackSerializer.Get<T>().PackSingleObject(original);
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        copy = MsgPack.Serialization.MessagePackSerializer.Get<T>().UnpackSingleObject(bytes);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(bytes.Length)));
        //}

        return copy;
    }

    static T SerializeJsonNet<T>(T original)
    {
   //     Console.WriteLine("JSON.NET");

        var jsonSerializer = new JsonSerializer();
        T copy = default(T);
        MemoryStream stream = null;


        var label = "JSON_NET." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                stream = new MemoryStream();
                using (var tw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                using (var jw = new JsonTextWriter(tw))
                {
                    jsonSerializer.Serialize(jw, original);
                }
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        using (var tr = new StreamReader(stream, Encoding.UTF8, false, 1024, true))
        //        using (var jr = new JsonTextReader(tr))
        //        {
        //            copy = jsonSerializer.Deserialize<T>(jr);
        //        }
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }

    static T SerializeJil<T>(T original)
    {
      //  Console.WriteLine("Jil");

        var jsonSerializer = new JsonSerializer();
        T copy = default(T);
        MemoryStream stream = null;

        var label = "Jil." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                stream = new MemoryStream();
                using (var tw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    Jil.JSON.Serialize(original, tw);
                }
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        using (var tr = new StreamReader(stream, Encoding.UTF8, false, 1024, true))
        //        {
        //            copy = Jil.JSON.Deserialize<T>(tr);
        //        }
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }

    static T SerializeBinaryFormatter<T>(T original)
    {
        //Console.WriteLine("BinaryFormatter");

        var serializer = new BinaryFormatter();
        T copy = default(T);
        MemoryStream stream = null;

        var label = "BinaryFormatter." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                serializer.Serialize(stream = new MemoryStream(), original);
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        copy = (T)serializer.Deserialize(stream);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }

    static T SerializeDataContract<T>(T original)
    {
       // Console.WriteLine("DataContractSerializer");

        T copy = default(T);
        MemoryStream stream = null;

        var label = "DataContractSerializer." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                new DataContractSerializer(typeof(T)).WriteObject(stream = new MemoryStream(), original);
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        copy = (T)new DataContractSerializer(typeof(T)).ReadObject(stream);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }

    static T SerializeFsPickler<T>(T original)
    {
        //Console.WriteLine("FsPickler");

        T copy = default(T);
        MemoryStream stream = null;
        var serializer = MBrace.CsPickler.CsPickler.CreateBinarySerializer();

        var label = "FsPickler." + original.GetType().Name;
        using (new Measure(label))
        {
            for (int i = 0; i < Iteration; i++)
            {
                serializer.Serialize<T>(stream = new MemoryStream(), original, leaveOpen: true);
            }
        }

        //using (new Measure("Deserialize"))
        //{
        //    for (int i = 0; i < Iteration; i++)
        //    {
        //        stream.Position = 0;
        //        copy = serializer.Deserialize<T>(stream, leaveOpen: true);
        //    }
        //}

        //if (!dryRun)
        //{
        //    Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(stream.Position)));
        //}

        return copy;
    }


    static void GenerateMsgPackSerializers()
    {
        var settings = new SerializerCodeGenerationConfiguration
        {
            OutputDirectory = Path.GetTempPath(),
            SerializationMethod = SerializationMethod.Array,
            Namespace = "Sandbox.Shared.GeneratedSerializers",
            IsRecursive = true,
            PreferReflectionBasedSerializer = false,
            WithNullableSerializers = true,
            EnumSerializationMethod = EnumSerializationMethod.ByName
        };

        var result = SerializerGenerator.GenerateSerializerSourceCodes(settings, typeof(Person));
        foreach (var item in result)
        {
            Console.WriteLine(item.FilePath);
        }
    }

    struct Measure : IDisposable
    {
        string label;
        Stopwatch s;
        public static IDictionary<string, double> Results = new Dictionary<string, double>();

        public Measure(string label)
        {
            this.label = label;
            System.GC.Collect(2, GCCollectionMode.Forced, blocking: true);
            this.s = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            s.Stop();
            if (!dryRun)
            {
                Results.Add(label, s.Elapsed.TotalMilliseconds);

         //       Console.WriteLine($"{ label,15}   {s.Elapsed.TotalMilliseconds} ms");
            }

            System.GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        }
    }

    static string ToHumanReadableSize(long size)
    {
        return ToHumanReadableSize(new Nullable<long>(size));
    }

    static string ToHumanReadableSize(long? size)
    {
        if (size == null) return "NULL";

        double bytes = size.Value;

        if (bytes <= 1024) return bytes.ToString("f2") + " B";

        bytes = bytes / 1024;
        if (bytes <= 1024) return bytes.ToString("f2") + " KB";

        bytes = bytes / 1024;
        if (bytes <= 1024) return bytes.ToString("f2") + " MB";

        bytes = bytes / 1024;
        if (bytes <= 1024) return bytes.ToString("f2") + " GB";

        bytes = bytes / 1024;
        if (bytes <= 1024) return bytes.ToString("f2") + " TB";

        bytes = bytes / 1024;
        if (bytes <= 1024) return bytes.ToString("f2") + " PB";

        bytes = bytes / 1024;
        if (bytes <= 1024) return bytes.ToString("f2") + " EB";

        bytes = bytes / 1024;
        return bytes + " ZB";
    }
}
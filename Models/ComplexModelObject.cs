using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Zippy;

namespace Models
{
   public class ComplexModelObject
    {

        public enum TType
        {
            None = 0,
            Expert = 1
        }

        public class Result
        {
            public bool IsGood { get; set; }
            public Guid Id { get; set; } = Guid.NewGuid();
            public DateTime Date { get; set; } = DateTime.Now;
        }


        public string Version = "1.0";
        public System.Collections.IList Collect = new System.Collections.ArrayList();

        public Guid Id = Guid.NewGuid();

        [ZippyDirective(true)]
        [IgnoreDataMember]
        public string IGNOREME { get; } = "SHOULD NOT BE SHOWN";


        public TType Type { get; set; } = TType.Expert;

        public string Name { get; set; }

        public string Address { get; set; }

        public bool IsReady { get; set; }
        public DateTime CreateDate { get; } = DateTime.Now;

        public DateTime CreateDateUTC { get; set; } = DateTime.UtcNow;
        public TimeSpan Duration
        {
            get
            {
                return _sw.Elapsed;
            }
        }
        private Stopwatch _sw = Stopwatch.StartNew();

        public int[] Items { get; set; }

        public int? Age { get; set; } = 10;

        public short S { get; set; }

        public double D { get; set; }

        public byte B { get; set; }

        public sbyte SB { get; set; }

       //public DataTable DT { get; } = new DataTable();
     //  public DataSet DS { get; } = new DataSet();

        public Result[] Results { get; set; } = Array.Empty<Result>();

        public List<object> MixObjects { get; set; } = new List<object>();
        public IList<Result> ResultList { get; set; } = new List<Result>();
        //  public System.Collections.ArrayList ResultAL { get; set; } = new System.Collections.ArrayList();
        // public System.Collections.IList ResultSL { get; set; } = new System.Collections.ArrayList();
        public Dictionary<string, int> ChildrenAges { get; set; } = new Dictionary<string, int>();
        public System.Collections.Specialized.NameValueCollection Names = new System.Collections.Specialized.NameValueCollection();

        public System.Collections.IDictionary OldAges { get; set; } = new System.Collections.Hashtable();


        public char CharValue { get; set; } = char.MaxValue;

        public byte ByteValue { get; set; } = byte.MaxValue;

        public sbyte SByteValue { get; set; } = sbyte.MaxValue;

        public short ShortValue { get; set; } = short.MaxValue;

        public ushort UShortValue { get; set; } = ushort.MaxValue;

        public int IntValue { get; set; } = int.MaxValue;

        public uint UIntValue { get; set; } = uint.MaxValue;

        public long LongValue { get; set; } = long.MaxValue;

        public ulong ULongValue { get; set; } = ulong.MaxValue;

        public float FloatValue { get; set; } = 234234234.2342F;

        public double DoubleValue { get; set; } = 234234234.2342;

        public decimal DecimalValue { get; set; } = 234234234.2342M;

        public DateTime DateTimeValue { get; set; } = DateTime.Now;

        public TimeSpan TimeSpanValue { get; set; } =DateTime.Now.TimeOfDay;

        public Guid GuidValue { get; set; } = Guid.NewGuid();

        private readonly static string BIGString= new StringBuilder(1000).Append('a', 1000).ToString();

        public ComplexModelObject()
        {
            this.Name = BIGString;
            this.Address = "8755 Lakeview Terrace";

            this.Items = (new List<int>() { 1, 3, 5, 6, 7, 88 }).ToArray();

            ResultList.Add(new Result());
            ResultList.Add(new Result());
            Collect.Add(1);
            Collect.Add("Test");
            OldAges.Add("test", "data");
            ChildrenAges.Add("bill", 13);
            //DT.Columns.Add("Column1", typeof(string));
            //DT.Columns.Add("Column2", typeof(int));

            //for (var i = 0; i < 10; i++)
            //{
            //    var r = DT.NewRow();
            //    r[0] = "Test_" + i.ToString();
            //    r[1] = i;
            //    DT.Rows.Add(r);
            //}

            MixObjects.Add("Test");
            MixObjects.Add(1);
            MixObjects.Add(true);
        }

        public static void Testing()
        {
            //do nothing
        }
    }
}

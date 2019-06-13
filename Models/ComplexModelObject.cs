﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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

        //    public IDictionary<string, string> Data = new Dictionary<string, string>();
        // public DataTable DT = new DataTable();
        //  public DataSet DS = new DataSet();

        public Result[] Results { get; set; } = Array.Empty<Result>();

        public IList<Result> ResultList { get; set; } = new List<Result>();
        //  public System.Collections.ArrayList ResultAL { get; set; } = new System.Collections.ArrayList();
        // public System.Collections.IList ResultSL { get; set; } = new System.Collections.ArrayList();
        public Dictionary<string, int> ChildrenAges { get; set; } = new Dictionary<string, int>();
        public System.Collections.Specialized.NameValueCollection Names = new System.Collections.Specialized.NameValueCollection();

        public System.Collections.IDictionary OldAges { get; set; } = new System.Collections.Hashtable();

        public ComplexModelObject()
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
}
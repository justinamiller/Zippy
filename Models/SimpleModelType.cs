using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class SimpleModelType
    {
        private readonly string _test;
        public string test = "";

        public string Test1
        {
            get
            {
                return test;
            }
        }
        public string Test2
        {
            get
            {
                return _test;
            }
        }


        public string Name { get; set; }

        public int Age { get; set; }

        public bool Hired { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid();

        public long BigNumber = 1234565672234;
        public SimpleModelType()
        {
            this.Name = "Joe Pickett";
            this.Age = 30;
            this.Hired = true;
        }
    }
}

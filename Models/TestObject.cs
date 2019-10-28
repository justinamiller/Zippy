using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class TestObject : ITest
    {
        public int Id { get; } = 1234;

        public string Name { get; } = "Tom";
    }
}

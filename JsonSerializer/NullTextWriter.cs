using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JsonSerializer
{
    public class NullTextWriter : TextWriter
    {
        public override Encoding Encoding { get; } = Encoding.Default;
    }
}

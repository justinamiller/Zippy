using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SwiftJson
{
    public sealed class NullTextWriter : TextWriter
    {
        private readonly static Encoding s_encoding= Encoding.Default;

        public override Encoding Encoding { get; } = s_encoding;

        public override void Write(string value)
        {
        }

        public override void Write(char value)
        {
        }

        public override void Write(char[] buffer, int index, int count)
        {
        }

        public override void Write(char[] buffer)
        {
        }
    }
}

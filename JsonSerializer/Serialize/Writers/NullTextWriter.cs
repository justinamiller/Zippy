using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zippy.Serialize.Writers
{
    public sealed class NullTextWriter : TextWriter
    {
        private readonly static Encoding s_encoding = Encoding.Default;

        public override Encoding Encoding {
            get
            {
                return s_encoding;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(string value)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char value)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char[] buffer, int index, int count)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char[] buffer)
        {
        }

        public override string ToString()
        {
            return null;
        }
    }
}

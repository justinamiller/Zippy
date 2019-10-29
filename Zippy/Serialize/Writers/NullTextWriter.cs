using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zippy.Serialize.Writers
{
    sealed class NullTextWriter : TextWriter
    {

        public override Encoding Encoding { get; } = Utility.StringExtension.DefaultEncoding;

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

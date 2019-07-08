using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Zippy.Serialize;
using Zippy.Serialize.Writers;
using Zippy.Utility;

namespace Zippy
{
    public sealed class JSON
    {
        public static IOptions Options { get; } = new Options();

        /// <summary>
        /// use default settings
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging should not affect program behavior.")]
        public static string SerializeObjectToString<T>(T Object)
        {
            if (Object == null)
            {
                return null;
            }

            var writer = StringWriterThreadStatic.Allocate();
            new Serializer().SerializeObjectInternal(Object, writer);
            var json = StringWriterThreadStatic.ReturnAndFree(writer);

            if (Options.PrettyPrint)
            {
                return StringExtension.PrettyPrint(json);
            }
            else
            {
                return json;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging should not affect program behavior.")]
        public static TextWriter SerializeObject<T>(T Object, TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            else if (Object == null)
            {
                return null;
            }

            new Serializer().SerializeObjectInternal(Object, writer);

            return writer;
        }
    }
}

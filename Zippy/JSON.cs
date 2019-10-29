using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Zippy.Serialize;
using Zippy.Serialize.Writers;
using Zippy.Utility;
[assembly: InternalsVisibleTo("Zippy.UnitTest")]
[assembly: InternalsVisibleTo("ConsoleTest")]
[assembly: InternalsVisibleTo("PerformanceComparison")]


namespace Zippy
{
    public sealed class JSON
    {
        private static Options s_DefaultOptions = new Options();

        /// <summary>
        /// Gets the Options objectwill use to calls of Serialize....
        /// if no Options object is provided.
        /// </summary>
        public static Options GetDefaultOptions()
        {
            return s_DefaultOptions;
        } 

        /// <summary>
        /// use default settings
        /// </summary>
        /// <param name="Object">serializable object</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging should not affect program behavior.")]
        public static string SerializeObjectToString(object Object, Options options=null)
        {
            if (Object == null)
            {
                return null;
            }
            options = options ?? s_DefaultOptions;
            var writer = StringWriterThreadStatic.Allocate();
            new Serializer(options).SerializeObjectInternal(Object, writer);
            var json = StringWriterThreadStatic.ReturnAndFree(writer);

            if (options.PrettyPrint)
            {
                return StringExtension.PrettyPrint(json);
            }
            else
            {
                return json;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging should not affect program behavior.")]
        public static TextWriter SerializeObject(object Object, TextWriter writer, Options options=null)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            else if (Object == null)
            {
                return null;
            }

            new Serializer(options ?? s_DefaultOptions).SerializeObjectInternal(Object, writer);

            return writer;
        }
    }
}

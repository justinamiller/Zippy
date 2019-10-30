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
[assembly: InternalsVisibleTo("Benchmarks")]


namespace Zippy
{
    public sealed class JSON
    {
        private static readonly Options s_DefaultOptions = new Options();

        /// <summary>
        /// Gets the Options objectwill use to calls of Serialize....
        /// if no Options object is provided.
        /// </summary>
        public static Options GetDefaultOptions()
        {
            return s_DefaultOptions;
        }

        /// <summary>
        /// Serializes the given data to string.
        /// </summary>
        /// <param name="Object">serializable object</param>
        /// <param name="options">when not provided will use default</param>
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

        /// <summary>
        /// Serializes the given data to the provided TextWriter.
        /// </summary>
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

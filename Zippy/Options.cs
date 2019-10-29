using System;
using Zippy.Serialize;

namespace Zippy
{

    public enum SerializationErrorHandling
    {
        /// <summary>
        /// report value as null
        /// </summary>
        ReportValueAsNull = 0,
        /// <summary>
        /// will not report property & value
        /// </summary>
        SkipProperty = 1,
        /// <summary>
        /// will throw exception to calling method.
        /// </summary>
        ThrowException = 2
    }

    public enum DateHandler
    {
        TimestampOffset = 0,
        ISO8601 = 1,
        ISO8601DateOnly = 2,
        ISO8601DateTime = 3,
        RFC1123 = 4
    }

    public enum TextCase
    {
        /// <summary>
        /// If unspecified use as is.
        /// </summary>
        Default,
        /// <summary>
        /// camelCase
        /// </summary>
        CamelCase,
        /// <summary>
        /// snake_case
        /// </summary>
        SnakeCase,
    }
    public sealed class Options 
    {
        internal readonly static IJsonSerializerStrategy CurrentJsonSerializerStrategy = new LambdaJsonSerializerStrategy();

        private const int MAXJSONLENGTH = 2097152;
        private const int RECURSIONLIMIT = 7;

   //     int _MaxJsonLength;
        int _RecursionLimit;

        /// <summary>
        ///  if HTML entity chars [&gt; &lt; &amp; = '] should be escaped as "\uXXXX".
        /// </summary>
        public bool EscapeHtmlChars { get; set; }

        public Options()
        {
            //default
         //   this._MaxJsonLength = MAXJSONLENGTH;
            this._RecursionLimit = RECURSIONLIMIT;
            SerializationErrorHandling = SerializationErrorHandling.ReportValueAsNull;
            TextCase = TextCase.Default;
        }
        /// <summary>
        /// whether or not to include whitespace and newlines for ease of reading
        /// </summary>
        public bool PrettyPrint { get; set; }
        /// <summary>
        /// What is done when unable to serialize field or property.
        /// </summary>
        public SerializationErrorHandling SerializationErrorHandling { get; set; }
        /// <summary>
        /// Skip property when value is null
        /// </summary>
        public bool ExcludeNulls { get; set; }
        /// <summary>
        /// Text case to use for property names
        /// </summary>
        public TextCase TextCase { get; set; }


        //public int MaxJsonLength
        //{
        //    get
        //    {
        //        return _MaxJsonLength;
        //    }

        //    set
        //    {
        //        if (0 >= value)
        //            throw new ArgumentException("MaxJsonLength must be greater than 0.");
        //        _MaxJsonLength = value;

        //    }
        //}

        /// <summary>
        /// depth to drill into object graph
        /// </summary>
        public int RecursionLimit
        {
            get
            {
                return _RecursionLimit;
            }

            set
            {
                if (0 >= value)
                    throw new ArgumentException("RecursionLimit must be greater than 0.");
                _RecursionLimit = value;
            }
        }

        public DateHandler DateHandler { get; set; } = DateHandler.TimestampOffset;
    }
}

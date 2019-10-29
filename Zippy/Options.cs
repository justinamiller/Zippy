using System;
using Zippy.Serialize;

namespace Zippy
{
    sealed class Options : IOptions
    {
        internal readonly static IJsonSerializerStrategy CurrentJsonSerializerStrategy = new LambdaJsonSerializerStrategy();

        internal const int MAXJSONLENGTH = 2097152;
        internal const int RECURSIONLIMIT = 7;

        int _MaxJsonLength = MAXJSONLENGTH;
        int _RecursionLimit = RECURSIONLIMIT;

        public bool EscapeHtmlChars { get; set; }


        internal Options(int maxJsonLength=MAXJSONLENGTH, int recursionLimit=RECURSIONLIMIT)
        {
            CurrentJsonSerializerStrategy.Reset();
            this.MaxJsonLength = maxJsonLength;
            this.RecursionLimit = recursionLimit;
        }
        public bool PrettyPrint { get; set; }
        public SerializationErrorHandling SerializationErrorHandling { get; set; }
        public bool ExcludeNulls { get; set; }

        public TextCase TextCase { get; set; }


        public int MaxJsonLength
        {
            get
            {
                return _MaxJsonLength;
            }

            set
            {
                if (0 >= value)
                    throw new ArgumentException("MaxJsonLength must be greater than 0.");
                _MaxJsonLength = value;

            }
        }

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

using System;
using System.Collections.Generic;
using System.Text;

namespace JsonSerializer
{
    sealed class JsonSerializerSetting : IJsonSerializerSetting
    {
        internal const int MAXJSONLENGTH = 2097152;
        internal const int RECURSIONLIMIT = 7;

        int _MaxJsonLength = MAXJSONLENGTH;
        int _RecursionLimit = RECURSIONLIMIT;

        public JsonSerializerSetting()
        {

        }


        public JsonSerializerSetting(int maxJsonLength, int recursionLimit, bool isElasticSearchReady)
        {
            this.MaxJsonLength = maxJsonLength;
            this.RecursionLimit = recursionLimit;
            this.IsElasticSearchReady = isElasticSearchReady;
        }

        /// <summary>
        /// ensure format takes account for elastic search
        /// </summary>
        public bool IsElasticSearchReady { get; set; } = true;

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
    }
}

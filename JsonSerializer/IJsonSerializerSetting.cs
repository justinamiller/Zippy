using System;
using System.Collections.Generic;
using System.Text;

namespace JsonSerializer
{
    public interface IJsonSerializerSetting
    {
        int MaxJsonLength { get; set; }

        int RecursionLimit { get; set; }

        /// <summary>
        /// Addtional encoding for supporting direct elastic search write.
        /// </summary>
        bool IsElasticSearchReady { get; set; }

        DateHandler DateHandler { get; set; }
    }

    public enum DateHandler
    {
        TimestampOffset,
        DCJSCompatible,
        ISO8601,
        ISO8601DateOnly,
        ISO8601DateTime,
        RFC1123
    }
}

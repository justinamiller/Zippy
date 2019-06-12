using System;
using System.Collections.Generic;
using System.Text;

namespace SwiftJson
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
        TimestampOffset=0,
        DCJSCompatible=1,
        ISO8601=2,
        ISO8601DateOnly=3,
        ISO8601DateTime=4,
        RFC1123=5
    }
}

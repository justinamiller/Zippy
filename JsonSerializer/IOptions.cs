using System;
using System.Collections.Generic;
using System.Text;

namespace Zippy
{
    public interface IOptions
    {
        int MaxJsonLength { get; set; }

        int RecursionLimit { get; set; }

        /// <summary>
        /// Addtional encoding for supporting direct elastic search write.
        /// </summary>
     //   bool IsElasticSearchReady { get; set; }

        DateHandler DateHandler { get; set; }

    bool EscapeHtmlChars { get; set; }

        /// <summary>
        /// whether or not to include whitespace and newlines for ease of reading
        /// </summary>
        bool PrettyPrint { get; set; }

        /// <summary>
        /// whether or not to write object members whose value is null
        /// </summary>
       bool ShouldExcludeNulls { get; set; }
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

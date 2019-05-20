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
    }
}

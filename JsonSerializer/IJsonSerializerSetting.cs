using System;
using System.Collections.Generic;
using System.Text;

namespace JsonSerializer
{
    public interface IJsonSerializerSetting
    {
        int MaxJsonLength { get; set; }

        int RecursionLimit { get; set; }

        bool IsElasticSearchReady { get; set; }
    }
}

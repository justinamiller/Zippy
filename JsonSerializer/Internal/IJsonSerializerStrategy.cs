using System;
using System.Collections.Generic;
using System.Text;

namespace JsonSerializer.Internal
{
    interface IJsonSerializerStrategy
    {
        bool TrySerializeNonPrimitiveObject(object input, out IDictionary<string, object> output);

        bool TrySerializeNonPrimitiveObjectImproved(object input, out IList<ValueMemberInfo> output);
    }
}

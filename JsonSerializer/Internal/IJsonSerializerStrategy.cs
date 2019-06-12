using System;
using System.Collections.Generic;
using System.Text;

namespace SwiftJson.Internal
{
    interface IJsonSerializerStrategy
    {
        bool TrySerializeNonPrimitiveObject(object input, out IDictionary<string, object> output);

        bool TrySerializeNonPrimitiveObjectImproved(object input, Type type, out IValue[] output);
    }
}

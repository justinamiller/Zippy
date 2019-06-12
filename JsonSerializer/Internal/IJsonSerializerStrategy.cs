using System;
using System.Collections.Generic;
using System.Text;

namespace SwiftJson.Internal
{
    interface IJsonSerializerStrategy
    {
        bool TrySerializeNonPrimitiveObject(object input, Type type, out IValue[] output);
    }
}

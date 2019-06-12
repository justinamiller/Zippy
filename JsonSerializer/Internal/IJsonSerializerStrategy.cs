using System;
using System.Collections.Generic;
using System.Text;

namespace Zippy.Internal
{
    interface IJsonSerializerStrategy
    {
        bool TrySerializeNonPrimitiveObject(object input, Type type, out IValue[] output);
    }
}

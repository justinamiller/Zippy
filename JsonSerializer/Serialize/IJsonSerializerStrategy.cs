using System;
using System.Collections.Generic;
using System.Text;
using Zippy.Internal;

namespace Zippy.Serialize
{
    interface IJsonSerializerStrategy
    {
        bool TrySerializeNonPrimitiveObject(object input, Type type, out IValue[] output);
    }
}

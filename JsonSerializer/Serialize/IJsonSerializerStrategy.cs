using System;
using Zippy.Internal;

namespace Zippy.Serialize
{
    interface IJsonSerializerStrategy
    {
        bool TrySerializeNonPrimitiveObject(object input, Type type, out IValueMemberInfo[] output);
    }
}

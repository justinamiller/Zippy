using System;
using Zippy.Internal;

namespace Zippy.Serialize
{
    interface IJsonSerializerStrategy
    {
        bool TrySerializeNonPrimitiveObject(Type type, out IValueMemberInfo[] output);
    }
}

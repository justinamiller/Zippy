using System;
using Zippy.Serialize;

namespace Zippy.Internal
{
    interface IValue
    {
        string Name { get; }
        Utility.TypeSerializerUtils.TypeCode Code { get; }
        object GetValue(object instance, ref bool isError);
        Type ValueType { get; }
    }
}

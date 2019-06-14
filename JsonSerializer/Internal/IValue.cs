using System;
using Zippy.Serialize;

namespace Zippy.Internal
{
    interface IValue
    {
        char[] NameChar { get; }
        Utility.TypeSerializerUtils.TypeCode Code { get; }
        object GetValue(object instance);
        WriteObjectDelegate WriteObject { get; }
        Type ValueType { get; }
    }
}

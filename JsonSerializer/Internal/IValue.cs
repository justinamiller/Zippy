using System;
using System.Collections.Generic;
using System.Text;
using Zippy.Serialize;
using static Zippy.Utility.TypeSerializerUtils;

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

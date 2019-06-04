using System;
using System.Collections.Generic;
using System.Text;
using static JsonSerializer.Utility.ConvertUtils;

namespace JsonSerializer.Internal
{
    interface IValue
    {
        string Name { get; }
        char[] NameChar { get; }
        Utility.ConvertUtils.TypeCode Code { get; }
        object GetValue(object instance);

        WriteObjectDelegate WriteObject { get; }
        Type ValueType { get; }
    }
}

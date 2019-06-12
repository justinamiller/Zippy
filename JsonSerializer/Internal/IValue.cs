using System;
using System.Collections.Generic;
using System.Text;
using static Zippy.Utility.ConvertUtils;

namespace Zippy.Internal
{
    interface IValue
    {
        char[] NameChar { get; }
        Utility.ConvertUtils.TypeCode Code { get; }
        object GetValue(object instance);

        WriteObjectDelegate WriteObject { get; }
        Type ValueType { get; }
    }
}

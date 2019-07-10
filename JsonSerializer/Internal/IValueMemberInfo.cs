using System;
using Zippy.Serialize;

namespace Zippy.Internal
{
    interface IValueMemberInfo
    {
        string Name { get; }
        Utility.TypeSerializerUtils.TypeCode Code { get; }
        bool TryGetValue(object instance, ref object value);
        Type ObjectType { get; }
        bool IsType { get; }

        IValueMemberInfo ExtendedValueInfo { get; }

        IValueMemberInfo[] GetCustomObjectMemberInfo();
    }
}

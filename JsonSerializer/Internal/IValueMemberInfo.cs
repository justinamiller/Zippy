using System;
using Zippy.Serialize;

namespace Zippy.Internal
{
    interface IValueMemberInfo
    {
        string Name { get; }
        Utility.TypeSerializerUtils.TypeCode Code { get; }
        object GetValue(object instance, ref bool isError);
        Type ObjectType { get; }
        bool IsType { get; }

        IValueMemberInfo ExtendedValueInfo { get; }

        IValueMemberInfo[] GetCustomObjectMemberInfo();
    }
}

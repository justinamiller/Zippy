using System;
using System.Collections.Generic;
using Zippy.Internal;
using Zippy.Utility;

namespace Zippy.Serialize
{
    class LambdaJsonSerializerStrategy : IJsonSerializerStrategy
    {
        public IDictionary<Type, ValueMemberInfo[]> GetCache;

        public LambdaJsonSerializerStrategy()
        {
            this.GetCache = new Dictionary<Type, ValueMemberInfo[]>();
        }

        private static ValueMemberInfo[] GetterValueFactory(Type type)
        {
            var allMembers = ReflectionExtension.GetFieldsAndProperties(type);

            int len = allMembers.Count;
            var data = new ValueMemberInfo[len];
            int dataIndex = 0;
            for (int i = 0; i < len; i++)
            {
                //get item
                var valueInfo = new ValueMemberInfo(allMembers[i]);
                if(!valueInfo.Name.IsNullOrEmpty())
                {
                    //must have property name.
                    data[dataIndex++] = valueInfo;
                }
            }

            if (dataIndex != len)
            {
                Array.Resize(ref data, dataIndex);
            }

            return data;
        }

        private bool GetValue(Type type, out ValueMemberInfo[] data)
        {
            if (this.GetCache.TryGetValue(type, out data))
            {
                //cache type
            }
            else
            {
                //reflection on type
                data = GetterValueFactory(type);
                if (type.Name.IndexOf("AnonymousType", StringComparison.Ordinal) == -1)
                {
                    //cache type
                    this.GetCache[type] = data;
                }
            }

            return (data != null);
        }

        public bool TrySerializeNonPrimitiveObject(object input, Type type, out IValue[] output)
        {
            output = null;
            ValueMemberInfo[] data;

            if (!GetValue(type, out data))
            {
                return false;
            }
            //ref
            output = data;

            return output != null;
        }
    }
}

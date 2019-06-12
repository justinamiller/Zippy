using Zippy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Zippy.Internal;

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
            var allMembers = ReflectionExtension.GetFieldsAndProperties(type);//.Where(m => !ReflectionExtension.IsIndexedProperty(m)).ToArray();

            int len = allMembers.Count;
            var data = new ValueMemberInfo[len];
            int dataIndex = 0;
            for (int i = 0; i < len; i++)
            {
                //get item
                data[dataIndex++] = new ValueMemberInfo(allMembers[i]);
            }

            return data;
        }

        private bool GetValue(Type type, out ValueMemberInfo[] data, out bool fromCache)
        {
            fromCache = false;
            data = null;

            if (this.GetCache.TryGetValue(type, out data))
            {
                //cache type
                fromCache = true;
            }
            else
            {
        //reflection on type
                data = GetterValueFactory(type);
               if (type.Name.IndexOf("AnonymousType", StringComparison.Ordinal) ==-1)
                {
                    //cache type
                    this.GetCache[type] = data;
                    fromCache = true;
                }
            }

            return (data != null);
        }

        public bool TrySerializeNonPrimitiveObject(object input, Type type, out IValue[] output)
        {
            output = null;
            ValueMemberInfo[] data;
            bool fromCache = false;

            if (type == null)
            {
                type = input.GetType();
            }

            if (!GetValue(type, out data, out fromCache))
            {
                return false;
            }

            int len = data.Length;
            output = new ValueMemberInfo[len];
            for (var i = 0; i < len; i++)
            {
                output[i] = data[i];
            }

            return output != null;
        }
    }
}

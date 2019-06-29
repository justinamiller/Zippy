using System;
using System.Collections.Generic;
using Zippy.Internal;
using Zippy.Utility;
using System.Text;
using System.IO;

namespace Zippy.Serialize
{

    sealed class LambdaJsonSerializerStrategy : IJsonSerializerStrategy
    {
        public FastLookup<Type, ValueMemberInfo[]> GetCache = new FastLookup<Type, ValueMemberInfo[]>();

        public LambdaJsonSerializerStrategy()
        {
        }

        private bool GetValue(Type type, out ValueMemberInfo[] data)
        {
            if (!this.GetCache.GetValue(type, out data))
            {
                //reflection on type
                data = TypeSerializerUtils.GetterValueFactory(type);
                if (type.Name.IndexOf("AnonymousType", StringComparison.Ordinal) == -1)
                {
                    //cache type
                    this.GetCache.Add(type, data);
                }
            }

            return (data != null);
        }

        public bool TrySerializeNonPrimitiveObject(object input, Type type, out IValueMemberInfo[] output)
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

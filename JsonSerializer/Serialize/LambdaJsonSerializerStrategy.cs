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
        public FastLookup<Type, IValueMemberInfo[]> GetCache = new FastLookup<Type, IValueMemberInfo[]>();

        public LambdaJsonSerializerStrategy()
        {
        }

        public void Reset()
        {
            this.GetCache.Clear();
        }


        public bool TrySerializeNonPrimitiveObject(Type type, out IValueMemberInfo[] output)
        {
            if (!this.GetCache.TryGetValue(type, out output))
            {
                //reflection on type
                output = TypeSerializerUtils.GetterValueFactory(type);
                if (type.Name.IndexOf("AnonymousType", StringComparison.Ordinal) == -1)
                {
                    //cache type
                    this.GetCache.Add(type, output);
                }
            }

            return (output != null);
        }
    }
}

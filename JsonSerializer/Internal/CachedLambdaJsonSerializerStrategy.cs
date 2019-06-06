using JsonSerializer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JsonSerializer.Internal
{
    class CachedLambdaJsonSerializerStrategy : IJsonSerializerStrategy
    {
        public IDictionary<Type, ValueMemberInfo[]> GetCache;

        public CachedLambdaJsonSerializerStrategy()
        {
            this.GetCache = new Dictionary<Type, ValueMemberInfo[]>();
        }


        protected virtual ValueMemberInfo[] GetterValueFactory(Type type)
        {
            var allMembers = ReflectionExtension.GetFieldsAndProperties(type);//.Where(m => !ReflectionExtension.IsIndexedProperty(m)).ToArray();

            int len = allMembers.Count;
            var data = new ValueMemberInfo[len];
            int dataIndex = 0;
       

            for (int i = 0; i < len; i++)
            {
                //get item
                MemberInfo item = allMembers[i];

                if (item != null)
                {
                    if (!ReflectionExtension.IsIndexedProperty(item))
                    {
                        data[dataIndex++] = new ValueMemberInfo(item);
                    }
                }
            }

            if (len != dataIndex)
            {
                Array.Resize(ref data, dataIndex);
            }

            return data;
        }


        protected virtual string MapClrMemberNameToJsonFieldName(string clrPropertyName)
        {
            return clrPropertyName;
        }

        public virtual bool TrySerializeNonPrimitiveObject(object input, out IDictionary<string, object> output)
        {
            return this.TrySerializeUnknownTypes(input, out output);
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
                //cache type
                data = this.GetterValueFactory(type);
                this.GetCache[type] = data;
                fromCache = true;
            }

            return (data != null);
        }


        protected virtual bool TrySerializeUnknownTypes(object input, out IDictionary<string, object> output)
        {
            output = null;

            if (input == null)
                return false;

            Type type = input.GetType();

            IDictionary<string, object> jsonObjects = new Dictionary<string, object>();// new PocoJsonObject();
           ValueMemberInfo[] data;
            bool fromCache;
            if (!GetValue(type, out data, out fromCache))
                return false;

            var len = data?.Length ?? 0;
            if (len > 0)
            {
                bool hasErrorsInFields = false;
                List<ValueMemberInfo> ErrorFields = new List<ValueMemberInfo>();

                for(var i=0; i< len; i++)
                {
                    var item = data[i];
                    try
                    {
                        //perform reflection here.
                        jsonObjects.Add(item.Name, item.GetValue(input));
                    }
                    catch (Exception)
                    {
                        if (fromCache)
                        {
                            ErrorFields.Add(item);
                            hasErrorsInFields = true;
                        }
                    }
                }
                //switch out object
                output = jsonObjects;
            }//end if

            return (output != null);
        }

        public bool TrySerializeNonPrimitiveObjectImproved(object input, Type type, out ValueMemberInfo[] output)
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

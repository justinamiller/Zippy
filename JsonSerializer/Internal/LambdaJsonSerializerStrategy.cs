using SwiftJson.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SwiftJson.Internal
{
    class LambdaJsonSerializerStrategy : IJsonSerializerStrategy
    {
        public IDictionary<Type, IDictionary<string, Func<object, object>>> GetCache;

        public LambdaJsonSerializerStrategy()
        {
            this.GetCache = new FactoryDictionary<Type, IDictionary<string, Func<object, object>>>(new ReflectionExtension.DictionaryValueFactory<Type, IDictionary<string, Func<object, object>>>(this.GetterValueFactory));
        }


        protected virtual IDictionary<string, Func<object, object>> GetterValueFactory(Type type)
        {
            MemberInfo[] allMembers = ReflectionExtension.GetFieldsAndProperties(type).Where(m => !ReflectionExtension.IsIndexedProperty(m)).ToArray();
            IDictionary<string, Func<object, object>> strs = new Dictionary<string, Func<object, object>>();

            for (int i = 0; i < allMembers.Length; i++)
            {
                MemberInfo item = allMembers[i];

                if (item != null && !item.Name.IsNullOrEmpty())
                {

                    Func<object, object> method = ReflectionExtension.CreateGet<object, object>(item);
                    if (method != null)
                        strs[item.Name] = method;
                }
            }

            return strs;
        }


        protected virtual string MapClrMemberNameToJsonFieldName(string clrPropertyName)
        {
            return clrPropertyName;
        }

        public virtual bool TrySerializeNonPrimitiveObject(object input, out IDictionary<string, object> output)
        {
            return this.TrySerializeUnknownTypes(input, out output);
        }

        private bool GetValue(Type type, out IDictionary<string, Func<object, object>> data, out bool fromCache)
        {
            fromCache = false;
            data = null;

            if (this.GetCache.TryGetValue(type, out data))
            {
                //cache type
                fromCache = true;
            }
            else if (type.Name.IndexOf("AnonymousType", StringComparison.Ordinal) >= 0)
            {
                //dont cache
                data = this.GetterValueFactory(type);
            }
            else
            {
                //cache type
                data = this.GetCache[type];
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
            IDictionary<string, Func<object, object>> data;
            bool fromCache;
            if (!GetValue(type, out data, out fromCache))
                return false;


            if (data != null && data.Count > 0)
            {
                bool hasErrorsInFields = false;
                List<string> ErrorFields = new List<string>();

                foreach (var item in data)
                {
                    Func<object, object> value = item.Value;

                    try
                    {
                        //perform reflection here.
                        jsonObjects.Add(item.Key, value(input));
                    }
                    catch (Exception)
                    {
                        if (fromCache)
                        {
                            ErrorFields.Add(item.Key);
                            hasErrorsInFields = true;
                        }
                    }
                }
                //switch out object
                output = jsonObjects;

                if (hasErrorsInFields)
                {
                    //remove bad items.
                    foreach (string item in ErrorFields)
                    {
                        data.Remove(item);
                    }
                }
            }//end if

            return (output != null);
        }

        public bool TrySerializeNonPrimitiveObjectImproved(object input, Type type, out IValue[] output)
        {
            throw new NotImplementedException();
        }
    }
}

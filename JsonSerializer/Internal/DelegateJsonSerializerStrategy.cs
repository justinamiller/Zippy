using SwiftJson.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SwiftJson.Internal
{
    class DelegateJsonSerializerStrategy : IJsonSerializerStrategy
    {
        public IDictionary<Type, IDictionary<string, ReflectionExtension.GetDelegate>> GetCache;

        public DelegateJsonSerializerStrategy()
        {
           this.GetCache = new FactoryDictionary<Type, IDictionary<string, ReflectionExtension.GetDelegate>>(new ReflectionExtension.DictionaryValueFactory<Type, IDictionary<string, ReflectionExtension.GetDelegate>>(GetterValueFactory));
        }

        static IDictionary<string, ReflectionExtension.GetDelegate> GetterValueFactory(Type type)
        {
            var allMembers = ReflectionExtension.GetFieldsAndProperties(type).Where(m => !ReflectionExtension.IsIndexedProperty(m)).ToArray();
            IDictionary<string, ReflectionExtension.GetDelegate> strs = new Dictionary<string, ReflectionExtension.GetDelegate>();

            for (int i = 0; i < allMembers.Length; i++)
            {
                MemberInfo item = allMembers[i];
                try
                {
                    if (item is PropertyInfo)
                    {
                        var property = (PropertyInfo)item;
                        string name = item.Name;
                        if (!name.IsNullOrEmpty())
                        {
                            var method = ReflectionExtension.GetGetMethod(property);
                            if (method != null)
                            {
                                strs[name] = method;
                            }
                        }
                    }
                    else if (item is FieldInfo)
                    {
                        var fieldInfo = (FieldInfo)item;
                        string name = item.Name;
                        if (!name.IsNullOrEmpty())
                        {
                            var method = ReflectionExtension.GetGetMethod(fieldInfo);
                            if (method != null)
                            {
                                strs[name] = method;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                  //do nothing
                }
            }

            return strs;
        }

        public virtual bool TrySerializeNonPrimitiveObject(object input, out IDictionary<string, object> output)
        {
            return this.TrySerializeUnknownTypes(input, out output);
        }

        private bool GetValue(Type type, out IDictionary<string, ReflectionExtension.GetDelegate> data, out bool fromCache)
        {

            fromCache = false;
            data = null;

            if (GetCache.TryGetValue(type, out data))
            {
                //cache type
                fromCache = true;
            }
            else if (type.Name.IndexOf("AnonymousType", StringComparison.Ordinal) >= 0)
            {
                //dont cache
                data = GetterValueFactory(type);
            }
            else
            {
                //cache type
                data = GetCache[type];
                fromCache = true;
            }

            return (data != null);
        }

        protected virtual bool TrySerializeUnknownTypes(object input, out IDictionary<string, object> output)
        {
            output = null;

            if (input == null)
            {
                return false;
            }
  
            IDictionary<string, object> jsonObjects = new JsonSerializerObject();
            IDictionary<string, ReflectionExtension.GetDelegate> data;

            bool fromCache;
            Type type = input.GetType();
            if (!GetValue(type, out data, out fromCache))
            {
                return false;
            }


            if (data != null && data.Count > 0)
            {
                bool hasErrorsInFields = false;
                List<string> ErrorFields = new List<string>();

                 foreach (var item in data)
                {
                    var value = item.Value;
                    if (value == null)
                    {
                        continue;
                    }

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
    }//end str
}

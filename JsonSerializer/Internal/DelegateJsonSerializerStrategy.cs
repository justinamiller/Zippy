using JsonSerializer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JsonSerializer.Internal
{
    class DelegateJsonSerializerStrategy : IJsonSerializerStrategy
    {
        public IDictionary<Type, IDictionary<string, ReflectionExtension.GetDelegate>> GetCache;

        public DelegateJsonSerializerStrategy()
        {
            DelegateJsonSerializerStrategy defaultJsonSerializerStrategy1 = this;
            this.GetCache = new FactoryDictionary<Type, IDictionary<string, ReflectionExtension.GetDelegate>>(new ReflectionExtension.DictionaryValueFactory<Type, IDictionary<string, ReflectionExtension.GetDelegate>>(defaultJsonSerializerStrategy1.GetterValueFactory));
        }



        internal virtual IDictionary<string, ReflectionExtension.GetDelegate> GetterValueFactory(Type type)
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
                        var method = ReflectionExtension.GetGetMethod((PropertyInfo)item);
                        if (method != null && !item.Name.IsNullOrEmpty())
                            strs[item.Name] = method;
                    }
                    else if (item is FieldInfo)
                    {
                        var method = ReflectionExtension.GetGetMethod((FieldInfo)item);
                        if (method != null && !item.Name.IsNullOrEmpty())
                            strs[item.Name] = method;
                    }
                    else
                    {
                        //nether?
                    }
                }
                catch (Exception)
                {
                    continue;
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

        private bool GetValue(Type type, out IDictionary<string, ReflectionExtension.GetDelegate> data, out bool fromCache)
        {

            fromCache = false;
            data = null;


            if (this.GetCache.TryGetValue(type, out data))
            {
                //cache type
                fromCache = true;
            }
            else if (type.Assembly == typeof(DelegateJsonSerializerStrategy).Assembly)
            {
                //cache type
                data = this.GetCache[type];
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
                var datas = data.ToArray();
                for (int i = 0; i < datas.Length; i++)
                {
                    var value = datas[i].Value;
                    if (value == null)
                    {
                        continue;
                    }
                       
                    string key = datas[i].Key;

                    try
                    {
                        //perform reflection here.
                        jsonObjects.Add(key, value(input));
                    }
                    catch (Exception)
                    {
                        if (fromCache)
                        {
                            ErrorFields.Add(key);
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
    }//end str
}

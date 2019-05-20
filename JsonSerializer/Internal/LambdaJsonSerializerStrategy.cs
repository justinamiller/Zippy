﻿using JsonSerializer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JsonSerializer.Internal
{
    class LambdaJsonSerializerStrategy : IJsonSerializerStrategy
    {
        public IDictionary<Type, IDictionary<string, Func<object, object>>> GetCache;

        public LambdaJsonSerializerStrategy()
        {
            LambdaJsonSerializerStrategy defaultJsonSerializerStrategy1 = this;
            this.GetCache = new FactoryDictionary<Type, IDictionary<string, Func<object, object>>>(new ReflectionExtension.DictionaryValueFactory<Type, IDictionary<string, Func<object, object>>>(defaultJsonSerializerStrategy1.GetterValueFactory));
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
            else if (type.Assembly == typeof(LambdaJsonSerializerStrategy).Assembly)
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
                return false;

            Type type = input.GetType();
            string typeFullName = type.FullName;

            //filter out unknown type
            if (typeFullName == null)
                return false;


            //filter out by namespace & Types
            if (typeFullName.IndexOf("System.", StringComparison.Ordinal) == 0)
            {
                if (typeFullName == "System.RuntimeType")
                    return false;
                if (typeFullName.IndexOf("System.Runtime.Serialization", StringComparison.Ordinal) == 0)
                    return false;
                if (typeFullName.IndexOf("System.Runtime.CompilerServices", StringComparison.Ordinal) == 0)
                    return false;
                if (typeFullName.IndexOf("System.Reflection", StringComparison.Ordinal) == 0)
                    return false;
                if (typeFullName.IndexOf("System.Threading.Task", StringComparison.Ordinal) == 0)
                    return false;
            }


            IDictionary<string, object> jsonObjects = new Dictionary<string, object>();// new PocoJsonObject();
            IDictionary<string, Func<object, object>> data;
            bool fromCache;
            if (!GetValue(type, out data, out fromCache))
                return false;


            if (data != null && data.Count > 0)
            {
                bool hasErrorsInFields = false;
                List<string> ErrorFields = new List<string>();
                var datas = data.ToArray();
                for (int i = 0; i < datas.Length; i++)
                {
                    Func<object, object> value = datas[i].Value;
                    if (value == null)
                        continue;

                    string key = datas[i].Key;

                    try
                    {
                        //perform reflection here.
                        jsonObjects.Add(key, value(input));
                    }
                    catch (TargetInvocationException)
                    {
                        if (fromCache)
                        {
                            ErrorFields.Add(key);
                            hasErrorsInFields = true;
                        }

                    }
                    catch (InvalidOperationException)
                    {
                        if (fromCache)
                        {
                            ErrorFields.Add(key);
                            hasErrorsInFields = true;
                        }

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
    }
}
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
        public IDictionary<Type, IList<ValueMemberInfo>> GetCache;

        public CachedLambdaJsonSerializerStrategy()
        {
            this.GetCache = new Dictionary<Type, IList<ValueMemberInfo>>();
        }


        protected virtual IList<ValueMemberInfo> GetterValueFactory(Type type)
        {
            MemberInfo[] allMembers = ReflectionExtension.GetFieldsAndProperties(type).Where(m => !ReflectionExtension.IsIndexedProperty(m)).ToArray();
            IList<ValueMemberInfo> strs = new List<ValueMemberInfo>();

            for (int i = 0; i < allMembers.Length; i++)
            {
                MemberInfo item = allMembers[i];

                if (item != null && !item.Name.IsNullOrEmpty())
                {
                    strs.Add(new ValueMemberInfo(item));
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

        private bool GetValue(Type type, out IList<ValueMemberInfo> data, out bool fromCache)
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
            IList<ValueMemberInfo> data;
            bool fromCache;
            if (!GetValue(type, out data, out fromCache))
                return false;


            if (data != null && data.Count > 0)
            {
                bool hasErrorsInFields = false;
                List<ValueMemberInfo> ErrorFields = new List<ValueMemberInfo>();

                foreach (var item in data)
                {
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

                if (hasErrorsInFields)
                {
                    //remove bad items.
                    foreach (var item in ErrorFields)
                    {
                        data.Remove(item);
                    }
                }
            }//end if

            return (output != null);
        }

        public bool TrySerializeNonPrimitiveObjectImproved(object input, out IList<ValueMemberInfo> output)
        {
            output = null;

            if (input == null)
                return false;

            Type type = input.GetType();

            IList<ValueMemberInfo> data;
            bool fromCache;
            if (!GetValue(type, out data, out fromCache))
                return false;

            output= new List<ValueMemberInfo>(data);

            return output != null;
        }
    }
}

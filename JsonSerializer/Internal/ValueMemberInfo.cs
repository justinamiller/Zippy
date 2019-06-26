using System;
using System.Reflection;
using Zippy.Serialize;
using Zippy.Utility;

namespace Zippy.Internal
{
    sealed class ValueMemberInfo : IValue
    {
        private readonly Func<object, object> _getter;

        public Type ValueType { get; }

        public TypeSerializerUtils.TypeCode Code { get; }

        public string Name { get; }

        private bool _errored = false;

        public ValueMemberInfo(MemberInfo memberInfo)
        {
            Type type = null;
            // this.MemberInfo = memberInfo;
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                type = propertyInfo.PropertyType;
                this._getter = Utility.ReflectionExtension.CreateGet<object, object>(propertyInfo);
            }
            else if (memberInfo is FieldInfo)
            {
                FieldInfo fieldInfo = (FieldInfo)memberInfo;
                type = fieldInfo.FieldType;
                this._getter = Utility.ReflectionExtension.CreateGet<object, object>(fieldInfo);
            }

            if (type != null)
            {
                this.ValueType = type;
                this.Code = Utility.TypeSerializerUtils.GetTypeCode(type);
                this.Name = TypeSerializerUtils.BuildPropertyName(memberInfo.GetSerializationName());
            }
            else
            {
                //not a field or property
                _errored = true;
            }
        }

        public object GetValue(object instance, ref bool isError)
        {
            if (!_errored && instance != null)
            {
                try
                {
                    return _getter(instance);
                }
                catch (Exception ex)
                {
                    if (JSON.Options.SerializationErrorHandling == SerializationErrorHandling.ThrowException)
                    {
                        throw ex;
                    }
                    _errored = true;

                    return null;
                }
            }

            isError = true;
            //has errored
            return null;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}

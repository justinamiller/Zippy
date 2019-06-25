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
            // this.MemberInfo = memberInfo;
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                ValueType = propertyInfo.PropertyType;
            }
            else if (memberInfo is FieldInfo)
            {
                ValueType = ((FieldInfo)memberInfo).FieldType;
            }

            if (ValueType != null)
            {
                this.Code = Utility.TypeSerializerUtils.GetTypeCode(ValueType);

                var name = memberInfo.GetSerializationName();
                this.Name = TypeSerializerUtils.BuildPropertyName(name);
                this._getter = Utility.ReflectionExtension.CreateGet<object, object>(memberInfo);
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

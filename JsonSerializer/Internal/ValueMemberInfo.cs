using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Zippy.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Zippy.Internal
{
    sealed class ValueMemberInfo : IValue
    {
        private readonly Func<object, object> _getter;

        public Type ValueType{get;}

        public ConvertUtils.TypeCode Code  { get; }

        public WriteObjectDelegate WriteObject { get; }

        public char[] NameChar { get; }

        private bool _errored = false;

        public ValueMemberInfo(MemberInfo memberInfo)
        {
            // this.MemberInfo = memberInfo;
            var propertyInfo = memberInfo as PropertyInfo; 
            if (propertyInfo!=null)
            {
                ValueType = propertyInfo.PropertyType;
            }
            else if (memberInfo is FieldInfo)
            {
                ValueType = ((FieldInfo)memberInfo).FieldType;
            }

            if (ValueType != null)
            {
                this.Code = Utility.ConvertUtils.GetTypeCode(ValueType);
                WriteObject = JsonTypeSerializer.GetValueTypeToStringMethod(Code);
                this.NameChar = StringExtension.GetEncodeString(memberInfo.GetSerializationName(), true);
                this._getter = Utility.ReflectionExtension.CreateGet<object, object>(memberInfo);
            }
            else
            {
                //not a field or property
                _errored = true;
            }
        }

        public object GetValue(object instance)
        {
            if (!_errored)
            {
                try
                {
                    return _getter(instance);
                }
                catch (Exception)
                {
                    _errored = true;
                }
            }

            //has errored
            return null;
        }

        public override string ToString()
        {
            return new string(this.NameChar);
        }
    }
}

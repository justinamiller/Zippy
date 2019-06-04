using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using JsonSerializer.Utility;
using System.Diagnostics;

namespace JsonSerializer.Internal
{
    sealed class ValueMemberInfo : IValue
    {
        private readonly Func<object, object> _getter;
        private ConvertUtils.TypeCode _typeCode;

        //public MemberInfo MemberInfo { get; }
        public Type ValueType{get;}

        public ConvertUtils.TypeCode Code
        {
            get
            {
                return _typeCode;
            }
        }
        public string Name { get;}

        public WriteObjectDelegate WriteObject { get; }

        public char[] NameChar { get; }

        private bool _errored = false;

        public ValueMemberInfo(MemberInfo memberInfo)
        {
           //    this.MemberInfo = memberInfo;
            this.NameChar = StringExtension.GetEncodeString(memberInfo.Name, true);
            this.Name = new string(StringExtension.GetEncodeString(memberInfo.Name, false));
            this._getter = Utility.ReflectionExtension.CreateGet<object, object>(memberInfo);

            if (memberInfo is PropertyInfo)
            {
                ValueType = ((PropertyInfo)memberInfo).PropertyType;
            }
            else if (memberInfo is FieldInfo)
            {
                ValueType = ((FieldInfo)memberInfo).FieldType;
            }
            if (ValueType != null)
            {
                _typeCode = Utility.ConvertUtils.GetTypeCode(ValueType);

                WriteObject = FastJsonWriter.GetValueTypeToStringMethod(Code);
            }
        }

        public object GetValue(object instance)
        {
            if (!_errored)
            {
                try
                {
                    object value = _getter(instance);

                    if (_typeCode == Utility.ConvertUtils.TypeCode.NotSetObject)
                    {
                        //now try to get value type.
                        _typeCode = Utility.ConvertUtils.GetInstanceObjectTypeCode(value);
                    }
                    return value;
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
            return this.Name;
        }
    }
}

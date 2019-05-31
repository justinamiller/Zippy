using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using JsonSerializer.Utility;

namespace JsonSerializer.Internal
{
    sealed class ValueMemberInfo : IValue
    {
        //  private readonly MemberInfo _memberInfo;
        private readonly Func<object, object> _getter;
        private Utility.ConvertUtils.TypeCode _typeCode;
        public Utility.ConvertUtils.TypeCode Code
        {
            get
            {
                return _typeCode;
            }
        }
        public string Name { get; }

        public WriteObjectDelegate WriteObject { get; }

        public char[] NameChar { get; }

        private bool _errored = false;

        public ValueMemberInfo(MemberInfo memberInfo)
        {
            //   this._memberInfo = memberInfo;
            this.NameChar = StringExtension.GetEncodeString(memberInfo.Name, true);
            this.Name = new string(StringExtension.GetEncodeString(memberInfo.Name, false));
            this._getter = Utility.ReflectionExtension.CreateGet<object, object>(memberInfo);

            Type valueType = null;
            if (memberInfo is PropertyInfo)
            {
                valueType = ((PropertyInfo)memberInfo).PropertyType;
            }
            else if (memberInfo is FieldInfo)
            {
                valueType = ((FieldInfo)memberInfo).FieldType;
            }
            if (valueType != null)
            {
                _typeCode = Utility.ConvertUtils.GetTypeCode(valueType);
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

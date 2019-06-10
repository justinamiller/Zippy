using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using JsonSerializer.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace JsonSerializer.Internal
{
    sealed class ValueMemberInfo : IValue
    {
        private readonly Func<object, object> _getter;


        private ConvertUtils.TypeCode _typeCode;

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
                _typeCode = Utility.ConvertUtils.GetTypeCode(ValueType);

                WriteObject = JsonTypeSerializer.GetValueTypeToStringMethod(Code);

                string name = memberInfo.Name;
                this.NameChar = StringExtension.GetEncodeString(name, true);
                this.Name = new string(StringExtension.GetEncodeString(name, false));
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
                   var value = _getter(instance);

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

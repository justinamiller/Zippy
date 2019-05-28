using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace JsonSerializer.Internal
{
  sealed  class ValueMemberInfo: IValue
    {
      //  private readonly MemberInfo _memberInfo;
        private readonly Func<object, object> _getter;
        public Utility.ConvertUtils.TypeCode Code { get; private set; }
        public string Name { get; }
        private bool _errored = false;

        public ValueMemberInfo(MemberInfo memberInfo)
        {
            //   this._memberInfo = memberInfo;
            this.Name = new string(JsonWriter.GetEncodeString(memberInfo.Name,false));
            this._getter = Utility.ReflectionExtension.CreateGet<object, object>(memberInfo);

            Type valueType = null;
            if(memberInfo is PropertyInfo)
            {
                valueType = ((PropertyInfo)memberInfo).PropertyType;
            }
            else if (memberInfo is FieldInfo)
            {
                valueType = ((FieldInfo)memberInfo).FieldType;
            }
            if (valueType != null)
            {
                Code = Utility.ConvertUtils.GetTypeCode(valueType);
            }
        }

        public object GetValue(object instance)
        {
            if (!_errored)
            {
                try
                {
                    object value= _getter(instance);

                    if(Code== Utility.ConvertUtils.TypeCode.NotSetObject)
                    {
                        //now try to get value type.
                        Code = Utility.ConvertUtils.GetInstanceObjectTypeCode(value);
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

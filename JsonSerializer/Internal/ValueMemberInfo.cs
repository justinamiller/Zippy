using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace JsonSerializer.Internal
{
  sealed  class ValueMemberInfo
    {
        private readonly MemberInfo _memberInfo;
        private readonly Func<object, object> _getter;
        public Utility.ConvertUtils.PrimitiveTypeCode TypeCode { get;  }
        public string Name { get; }
        private bool _errored = false;

        public Utility.ConvertUtils.ObjectTypeCode ObjectTypeCode { get; private set; }
        public Utility.ConvertUtils.PrimitiveTypeCode CollectionTypeCode { get; }

        public ValueMemberInfo(MemberInfo memberInfo)
        {
            this._memberInfo = memberInfo;
            this.Name = memberInfo.Name;
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
                TypeCode = Utility.ConvertUtils.GetTypeCode(valueType);
                if (TypeCode == Utility.ConvertUtils.PrimitiveTypeCode.Object)
                {
                    this.ObjectTypeCode = Utility.ConvertUtils.GetObjectTypeCode(valueType);
                }
            }
        }

        public object GetValue(object instance)
        {
            if (!_errored)
            {
                try
                {
                    var value= _getter(instance);

                    if(TypeCode== Utility.ConvertUtils.PrimitiveTypeCode.Object && ObjectTypeCode== Utility.ConvertUtils.ObjectTypeCode.Empty)
                    {
                        //now try to get value type.
                        ObjectTypeCode = Utility.ConvertUtils.GetInstanceObjectTypeCode(value);
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

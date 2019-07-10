using System;
using System.Reflection;
using Zippy.Serialize;
using Zippy.Utility;

namespace Zippy.Internal
{
    sealed class ValueMemberInfo : IValueMemberInfo
    {
        private readonly Func<object, object> _getter;

        public Type ObjectType { get; }

        public TypeSerializerUtils.TypeCode Code { get; }

        public string Name { get; }

        public bool IsType { get; private set; }

        private bool _errored;

        public IValueMemberInfo ExtendedValueInfo { get; private set; }

        public ValueMemberInfo(Type type)
        {
            this.ObjectType = type;
            this.Code = TypeSerializerUtils.GetTypeCode(type);
            this.IsType = type != typeof(object);// && this.Code != TypeSerializerUtils.TypeCode.NotSetObject;

            CheckForExtendedValueInfo();
        }

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
                this.Name = TypeSerializerUtils.BuildPropertyName(memberInfo.GetSerializationName());
                if (!this.Name.IsNullOrEmpty())
                {
                    this.ObjectType = type;
                    this.Code = Utility.TypeSerializerUtils.GetTypeCode(type);

                    this.IsType = type != typeof(object);// && this.Code != TypeSerializerUtils.TypeCode.NotSetObject;
                    CheckForExtendedValueInfo();
                }                
            }
            else
            {
                //not a field or property
                _errored = true;
            }
        }

        private void CheckForExtendedValueInfo()
        {
            if (!this.IsType)
            {
                return;
            }

            var code = this.Code;

            if (!TypeSerializerUtils.HasExtendedValueInformation(code))
            {
                return;
            }

            Type type = null;
            //array?
            if (code == TypeSerializerUtils.TypeCode.Array)
            {
                type = ObjectType.GetElementType();
            }
            else if (ObjectType.IsGenericType)
            {
                //generic list / dictionary
                var args = ObjectType.GetGenericArguments();
                var len = args.Length;
                if (len == 2)
                {
                    //key value pair
                    //need to check if key is string
                    var keyCodeType = TypeSerializerUtils.GetTypeCode(args[0]);
                    if (keyCodeType != TypeSerializerUtils.TypeCode.String)
                    {
                        _errored = true;
                        return;
                    }

                    type = args[1];
                }
                else if (len == 1)
                {
                    //value only
                    type = args[0];
                }
            }

            ExtendedValueInfo = new ValueMemberInfo(type ?? typeof(object));
        }

        public bool TryGetValue(object instance, ref object value)
        {
            if (!_errored)
            {
                try
                {
                    value= _getter(instance);
                    return true;
                }
                catch (Exception ex)
                {
                    if (JSON.Options.SerializationErrorHandling == SerializationErrorHandling.ThrowException)
                    {
                        throw ex;
                    }
                }
            }

            //has errored
            value = null;
            return false;
        }

        IValueMemberInfo[] _valueMemberInfos;
        public IValueMemberInfo[] GetCustomObjectMemberInfo()
        {
            if (_valueMemberInfos==null)
            {
                if (!Serializer.CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObject(this.ObjectType, out _valueMemberInfos))
                {
                    throw new Exception("Unable to serialize " + this.ObjectType.FullName);
                }
            }

            return _valueMemberInfos;
        }



        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var value = obj as ValueMemberInfo;

            return (value != null)
                && (Code == value.Code)
                && (ObjectType == value.ObjectType)
                && (Name == value.Name);
        }

        public override int GetHashCode()
        {
            return (int)this.Code ^ this.ObjectType.GetHashCode() ^ this.Name?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}

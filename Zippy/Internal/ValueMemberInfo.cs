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

        public JsonWriter.WriteObjectDelegate WriteDelegate { get; }

        private bool _errored;

        public IValueMemberInfo ExtendedValueInfo { get; private set; }

        public ValueMemberInfo(Type type)
        {
            this.ObjectType = type;
            this.Code = TypeSerializerUtils.GetTypeCode(type);
            this.IsType = type != typeof(object);// && this.Code != TypeSerializerUtils.TypeCode.NotSetObject;
            WriteDelegate = JsonWriter.GetWriteObjectDelegate(Code);
            if (this.IsType)
            {
                CheckForExtendedValueInfo();
            }
        }

        public ValueMemberInfo(MemberInfo memberInfo)
        {
            Type type = null;
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
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

            this.Name = string.Empty;
            if (type != null)
            {
                this.Name = memberInfo.GetSerializationName();
   
                if (!this.Name.IsNullOrEmpty())
                {
                    //encode name
                    this.Name = new string(StringExtension.GetEncodeString(this.Name, false, false));
                    this.ObjectType = type;
                    this.Code = Utility.TypeSerializerUtils.GetTypeCode(type);
                    WriteDelegate = JsonWriter.GetWriteObjectDelegate(Code);
                    this.IsType = type != typeof(object);// && this.Code != TypeSerializerUtils.TypeCode.NotSetObject;
                    if (this.IsType)
                    {
                        CheckForExtendedValueInfo();
                    }
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
#if !NETCOREAPP1_0
            else if (ObjectType.IsGenericType)
#else
            else if (ObjectType.GetTypeInfo().IsGenericType)
#endif
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
                    value = _getter(instance);
                    return true;
            }

            //has errored
            value = null;
            return false;
        }

        IValueMemberInfo[] _valueMemberInfos;
        public IValueMemberInfo[] GetCustomObjectMemberInfo()
        {
            if (_valueMemberInfos == null)
            {
                if (!Options.CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObject(this.ObjectType, out _valueMemberInfos))
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
            return (int)this.Code ^ this.ObjectType.GetHashCode() ^ this.Name.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}

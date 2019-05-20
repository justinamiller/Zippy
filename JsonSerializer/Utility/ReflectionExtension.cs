﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace JsonSerializer.Utility
{
    static class ReflectionExtension
    {
        private static readonly BindingFlags s_DefaultFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;//BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)

        public static IEnumerable<FieldInfo> GetFields(Type type, BindingFlags bindingFlags)
        {
            List<MemberInfo> fieldInfos = new List<MemberInfo>(type.GetFields(bindingFlags));

            GetChildPrivateFields(fieldInfos, type, bindingFlags);

            return fieldInfos.Cast<FieldInfo>();
        }

        private static void GetChildPrivateFields(IList<MemberInfo> initialFields, Type targetType, BindingFlags bindingAttr)
        {
            // fix weirdness with private FieldInfos only being returned for the current Type
            // find base type fields and add them to result
            if ((bindingAttr & BindingFlags.NonPublic) != 0)
            {
                // modify flags to not search for public fields
                BindingFlags nonPublicBindingAttr = bindingAttr.RemoveFlag(BindingFlags.Public);

                while ((targetType = targetType.BaseType) != null)
                {
                    // filter out protected fields
                    IEnumerable<MemberInfo> childPrivateFields =
                        targetType.GetFields(nonPublicBindingAttr).Where(f => f.IsPrivate).Cast<MemberInfo>();

                    foreach (var item in childPrivateFields)
                    {
                        initialFields.Add(item);
                    }
                }
            }
        }

        public static BindingFlags RemoveFlag(this BindingFlags bindingAttr, BindingFlags flag)
        {
            return ((bindingAttr & flag) == flag)
                ? bindingAttr ^ flag
                : bindingAttr;
        }


        public static GetDelegate GetGetMethod(PropertyInfo propertyInfo)
        {
            return GetGetMethodByReflection(propertyInfo);
        }


        public static ReflectionExtension.GetDelegate GetGetMethod(MethodInfo getterMethodInfo)
        {
            if (getterMethodInfo == null)
                return null;
            object[] objArray = new object[0];

            return (object source) => getterMethodInfo.Invoke(source, objArray);
        }


        private static Expression EnsureCastExpression(Expression expression, Type targetType, bool allowWidening = false)
        {
            Type expressionType = expression.Type;

            // check if a cast or conversion is required
            if (expressionType == targetType || (!expressionType.IsValueType && targetType.IsAssignableFrom(expressionType)))
            {
                return expression;
            }

            if (targetType.IsValueType)
            {
                Expression convert = Expression.Unbox(expression, targetType);

                if (allowWidening && targetType.IsPrimitive)
                {
                    MethodInfo toTargetTypeMethod = typeof(Convert)
                        .GetMethod("To" + targetType.Name, new[] { typeof(object) });

                    if (toTargetTypeMethod != null)
                    {
                        convert = Expression.Condition(
                            Expression.TypeIs(expression, targetType),
                            convert,
                            Expression.Call(toTargetTypeMethod, expression));
                    }
                }

                return Expression.Condition(
                    Expression.Equal(expression, Expression.Constant(null, typeof(object))),
                    Expression.Default(targetType),
                    convert);
            }

            return Expression.Convert(expression, targetType);
        }

        public static Func<TKey, TValue> CreateGet<TKey, TValue>(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return null;

            ParameterExpression instance = Expression.Parameter(typeof(TKey), "instance");
            Expression resultExpression;

            try
            {
                MethodInfo getMethod = propertyInfo.GetGetMethod(false);

                if (getMethod == null)
                    return null;

                if (getMethod.IsStatic)
                {
                    resultExpression = EnsureCastExpression(Expression.MakeMemberAccess(null, propertyInfo), typeof(TValue));
                }
                else
                {
                    Type declareType = propertyInfo.DeclaringType;
                    if (declareType != null)
                    {
                        UnaryExpression instanceCast = (!declareType.IsValueType) ? Expression.TypeAs(instance, declareType) : Expression.Convert(instance, declareType);
                        if (instanceCast == null)
                            return null;

                        resultExpression = Expression.Call(instanceCast, getMethod);
                    }
                    else
                    {
                        Expression readParameter = EnsureCastExpression(instance, propertyInfo.DeclaringType);
                        if (readParameter == null)
                            return null;

                        resultExpression = Expression.MakeMemberAccess(readParameter, propertyInfo);
                    }
                }

                if (resultExpression == null)
                    return null;

                return Expression.Lambda<Func<TKey, TValue>>(Expression.TypeAs(resultExpression, typeof(TValue)), instance).Compile();
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static Func<TKey, TValue> CreateGet<TKey, TValue>(MemberInfo item)
        {
            try
            {
                if (item is PropertyInfo)
                {
                    Func<TKey, TValue> method = CreateGet<TKey, TValue>((PropertyInfo)item);
                    if (method != null)
                        return method;
                }
                else if (item is FieldInfo)
                {
                    Func<TKey, TValue> method = CreateGet<TKey, TValue>((FieldInfo)item);
                    if (method != null)
                        return method;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static Func<TKey, TValue> CreateGet<TKey, TValue>(FieldInfo fieldInfo)
        {
            try
            {
                ParameterExpression sourceParameter = Expression.Parameter(typeof(TKey), "source");

                Expression fieldExpression;
                if (fieldInfo.IsStatic)
                {
                    fieldExpression = Expression.Field(null, fieldInfo);
                }
                else
                {
                    Expression sourceExpression = EnsureCastExpression(sourceParameter, fieldInfo.DeclaringType);

                    fieldExpression = Expression.Field(sourceExpression, fieldInfo);
                }

                fieldExpression = EnsureCastExpression(fieldExpression, typeof(TValue));


                return Expression.Lambda<Func<TKey, TValue>>(fieldExpression, sourceParameter).Compile();
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static ReflectionExtension.GetDelegate GetGetMethod(FieldInfo fieldInfo)
        {
            return ReflectionExtension.GetGetMethodByReflection(fieldInfo);
        }

        public static ReflectionExtension.GetDelegate GetGetMethodByReflection(PropertyInfo propertyInfo)
        {
            MethodInfo getterMethodInfo = ReflectionExtension.GetGetterMethodInfo(propertyInfo, false);
            return GetGetMethod(getterMethodInfo);
        }

        public static ReflectionExtension.GetDelegate GetGetMethodByReflection(FieldInfo fieldInfo)
        {
            return (object source) => fieldInfo.GetValue(source);
        }

        public static MethodInfo GetGetterMethodInfo(PropertyInfo propertyInfo, bool nonPublic)
        {
            return propertyInfo.GetGetMethod(nonPublic);
        }

        public static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags bindingFlags)
        {
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>(type.GetProperties(bindingFlags));

            // GetProperties on an interface doesn't return properties from its interfaces
            if (type.IsInterface)
            {
                foreach (Type i in type.GetInterfaces())
                {
                    propertyInfos.AddRange(i.GetProperties(bindingFlags));
                }
            }

            GetChildPrivateProperties(propertyInfos, type, bindingFlags);

            // a base class private getter/setter will be inaccessable unless the property was gotten from the base class
            for (int i = 0; i < propertyInfos.Count; i++)
            {
                PropertyInfo member = propertyInfos[i];
                if (member.DeclaringType != type)
                {
                    PropertyInfo declaredMember = (PropertyInfo)GetMemberInfoFromType(member.DeclaringType, member);
                    propertyInfos[i] = declaredMember;
                }
            }

            return propertyInfos;
        }

        public static List<MemberInfo> GetFieldsAndProperties(Type type)
        {
            return GetFieldsAndProperties(type, s_DefaultFlags);
        }

        public static List<MemberInfo> GetFieldsAndProperties(Type type, BindingFlags bindingAttr)
        {
            List<MemberInfo> targetMembers = new List<MemberInfo>();

            targetMembers.AddRange(GetFields(type, bindingAttr));
            targetMembers.AddRange(GetProperties(type, bindingAttr));

            // for some reason .NET returns multiple members when overriding a generic member on a base class
            // http://social.msdn.microsoft.com/Forums/en-US/b5abbfee-e292-4a64-8907-4e3f0fb90cd9/reflection-overriden-abstract-generic-properties?forum=netfxbcl
            // filter members to only return the override on the topmost class
            // update: I think this is fixed in .NET 3.5 SP1 - leave this in for now...
            List<MemberInfo> distinctMembers = new List<MemberInfo>(targetMembers.Count);

            var groupbyTargetMemebers = targetMembers.Where(l => !l.Name.Contains("k__BackingField")).GroupBy(m => m.Name).ToArray();
            foreach (var groupedMember in groupbyTargetMemebers)
            {
                IList<MemberInfo> members = groupedMember.ToList();
                int count = members.Count();

                if (count == 1)
                {
                    distinctMembers.Add(members.First());
                }
                else
                {
                    IList<MemberInfo> resolvedMembers = new List<MemberInfo>();
                    foreach (MemberInfo memberInfo in members)
                    {
                        // this is a bit hacky
                        // if the hiding property is hiding a base property and it is virtual
                        // then this ensures the derived property gets used
                        if (resolvedMembers.Count == 0)
                        {
                            resolvedMembers.Add(memberInfo);
                        }
                        else if (!IsOverridenGenericMember(memberInfo, bindingAttr) || memberInfo.Name == "Item")
                        {
                            resolvedMembers.Add(memberInfo);
                        }
                    }

                    distinctMembers.AddRange(resolvedMembers);
                }
            }

            return distinctMembers;
        }

        private static bool IsOverridenGenericMember(MemberInfo memberInfo, BindingFlags bindingAttr)
        {
            if (memberInfo.MemberType != MemberTypes.Property)
            {
                return false;
            }

            PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
            if (!IsVirtual(propertyInfo))
            {
                return false;
            }

            Type declaringType = propertyInfo.DeclaringType;
            if (!declaringType.IsGenericType)
            {
                return false;
            }
            Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
            if (genericTypeDefinition == null)
            {
                return false;
            }
            MemberInfo[] members = genericTypeDefinition.GetMember(propertyInfo.Name, bindingAttr);
            if (members.Length == 0)
            {
                return false;
            }
            Type memberUnderlyingType = GetMemberUnderlyingType(members[0]);
            if (!memberUnderlyingType.IsGenericParameter)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the member's underlying type.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The underlying type of the member.</returns>
        public static Type GetMemberUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                default:
                    throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo, EventInfo or MethodInfo", nameof(member));
            }
        }

        /// <summary>
        /// Determines whether the member is an indexed property.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>
        /// 	<c>true</c> if the member is an indexed property; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIndexedProperty(MemberInfo member)
        {

            if (member is PropertyInfo)
            {
                return IsIndexedProperty((PropertyInfo)member);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the property is an indexed property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>
        /// 	<c>true</c> if the property is an indexed property; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIndexedProperty(PropertyInfo property)
        {
            return (property.GetIndexParameters().Length > 0);
        }

        private static void GetChildPrivateProperties(IList<PropertyInfo> initialProperties, Type targetType, BindingFlags bindingAttr)
        {
            // fix weirdness with private PropertyInfos only being returned for the current Type
            // find base type properties and add them to result

            // also find base properties that have been hidden by subtype properties with the same name

            while ((targetType = targetType.BaseType) != null)
            {
                foreach (PropertyInfo propertyInfo in targetType.GetProperties(bindingAttr))
                {
                    PropertyInfo subTypeProperty = propertyInfo;

                    if (!IsPublic(subTypeProperty))
                    {
                        // have to test on name rather than reference because instances are different
                        // depending on the type that GetProperties was called on
                        int index = -1;
                        foreach (var item in initialProperties)
                        {
                            if (item.Name == subTypeProperty.Name)
                            {
                                index = initialProperties.IndexOf(item);
                                break;
                            }
                        }

                        //int index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name);
                        if (index == -1)
                        {
                            initialProperties.Add(subTypeProperty);
                        }
                        else
                        {
                            PropertyInfo childProperty = initialProperties[index];
                            // don't replace public child with private base
                            if (!IsPublic(childProperty))
                            {
                                // replace nonpublic properties for a child, but gotten from
                                // the parent with the one from the child
                                // the property gotten from the child will have access to private getter/setter
                                initialProperties[index] = subTypeProperty;
                            }
                        }
                    }
                    else
                    {
                        if (!IsVirtual(subTypeProperty))
                        {
                            int index = -1; //initialProperties.IndexOf(p => p.Name == subTypeProperty.Name
                                            //                             && p.DeclaringType == subTypeProperty.DeclaringType);

                            foreach (PropertyInfo item in initialProperties)
                            {
                                if (item.Name == subTypeProperty.Name && item.DeclaringType == subTypeProperty.DeclaringType)
                                {
                                    index = initialProperties.IndexOf(item);
                                    break;
                                }
                            }

                            if (index == -1)
                            {
                                initialProperties.Add(subTypeProperty);
                            }
                        }
                        else
                        {
                            int index = -1;
                            foreach (PropertyInfo item in initialProperties)
                            {
                                if (item.Name == subTypeProperty.Name && IsVirtual(item) && GetBaseDefinition(item) != null && GetBaseDefinition(item).DeclaringType.IsAssignableFrom(GetBaseDefinition(subTypeProperty).DeclaringType))
                                {
                                    index = initialProperties.IndexOf(item);
                                    break;
                                }
                            }

                            //int index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name
                            //                                           && p.IsVirtual()
                            //                                           && p.GetBaseDefinition() != null
                            //                                           && p.GetBaseDefinition().DeclaringType.IsAssignableFrom(subTypeProperty.GetBaseDefinition().DeclaringType));

                            if (index == -1)
                            {
                                initialProperties.Add(subTypeProperty);
                            }
                        }
                    }
                }
            }
        }

        public static MethodInfo GetBaseDefinition(PropertyInfo propertyInfo)
        {

            MethodInfo m = propertyInfo.GetGetMethod();
            if (m != null)
            {
                return m.GetBaseDefinition();
            }

            m = propertyInfo.GetSetMethod();
            if (m != null)
            {
                return m.GetBaseDefinition();
            }

            return null;
        }

        public static bool IsPublic(PropertyInfo property)
        {
            if (property.GetGetMethod() != null && property.GetGetMethod().IsPublic)
            {
                return true;
            }
            if (property.GetSetMethod() != null && property.GetSetMethod().IsPublic)
            {
                return true;
            }

            return false;
        }

        public static bool IsVirtual(PropertyInfo propertyInfo)
        {
            MethodInfo m = propertyInfo.GetGetMethod();
            if (m != null && m.IsVirtual)
            {
                return true;
            }

            m = propertyInfo.GetSetMethod();
            if (m != null && m.IsVirtual)
            {
                return true;
            }

            return false;
        }

        public static MemberInfo GetMemberInfoFromType(Type targetType, MemberInfo memberInfo)
        {
            BindingFlags bindingAttr = s_DefaultFlags; //BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)memberInfo;

                    Type[] types = propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray();

                    return targetType.GetProperty(propertyInfo.Name, bindingAttr, null, propertyInfo.PropertyType, types, null);
                default:
                    return targetType.GetMember(memberInfo.Name, memberInfo.MemberType, bindingAttr).SingleOrDefault();
            }
        }

        public static Type GetTypeInfo(Type type)
        {
            return type;
        }

        public static bool IsTypeGeneric(Type type)
        {
            return ReflectionExtension.GetTypeInfo(type).IsGenericType;
        }

        public delegate object GetDelegate(object source);



        public delegate TValue DictionaryValueFactory<TKey, TValue>(TKey key);
    }
}
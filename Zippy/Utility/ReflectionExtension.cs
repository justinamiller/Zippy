﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Zippy.Utility
{
    static class ReflectionExtension
    {
        private const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public static List<MemberInfo> GetFields(Type type, BindingFlags bindingFlags)
        {
            List<MemberInfo> fieldInfos = new List<MemberInfo>(type.GetFields(bindingFlags));

            // GetInterfaces on an interface doesn't return properties from its interfaces
            if (type.IsInterface)
            {
                foreach (Type i in type.GetInterfaces())
                {
                    fieldInfos.AddRange(i.GetFields(bindingFlags));
                }
            }


            GetChildPrivateFields(fieldInfos, type, bindingFlags);

            return fieldInfos;
            // return fieldInfos.Cast<FieldInfo>();
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
            if (propertyInfo.PropertyType.IsByRef)
                return null;//https://github.com/dotnet/corefx/issues/26053

            ParameterExpression parameterExpression = Expression.Parameter(typeof(TKey), "instance");
            Expression resultExpression;

            try
            {
                MethodInfo getMethod = propertyInfo.GetGetMethod(false);

                if (getMethod == null)
                    return null;

                Expression readParameter = null;
                if (!getMethod.IsStatic)
                {
                    readParameter = EnsureCastExpression(parameterExpression, propertyInfo.DeclaringType);
                }

                resultExpression = Expression.MakeMemberAccess(readParameter, propertyInfo);

                resultExpression = EnsureCastExpression(resultExpression, typeof(object));
                LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<TKey, TValue>), resultExpression, parameterExpression);

                return (Func<TKey, TValue>)lambdaExpression.Compile();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Func<TKey, TValue> CreateGet<TKey, TValue>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return null;
            if (fieldInfo.FieldType.IsByRef)
                return null;//https://github.com/dotnet/corefx/issues/26053

            Expression sourceExpression = null;
            try
            {
                ParameterExpression sourceParameter = Expression.Parameter(typeof(TKey), "source");

                if (!fieldInfo.IsStatic)
                {
                    sourceExpression = EnsureCastExpression(sourceParameter, fieldInfo.DeclaringType);
                }

                Expression fieldExpression = Expression.Field(sourceExpression, fieldInfo);

                fieldExpression = EnsureCastExpression(fieldExpression, typeof(TValue));

                LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<TKey, TValue>), fieldExpression, sourceParameter);
                return (Func<TKey, TValue>)lambdaExpression.Compile();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<PropertyInfo> GetProperties(Type type, BindingFlags bindingFlags)
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
            int len = propertyInfos.Count;
            for (int i = 0; i < len; i++)
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
            return GetFieldsAndProperties(type, DefaultFlags);
        }

        public static List<MemberInfo> GetFieldsAndProperties(Type type, BindingFlags bindingAttr)
        {
            List<MemberInfo> targetMembers = new List<MemberInfo>();
            targetMembers.AddRange(GetFields(type, bindingAttr));
            targetMembers.AddRange(GetProperties(type, bindingAttr));

            List<MemberInfo> filterMembers = new List<MemberInfo>();

            foreach (var member in targetMembers)
            {
                if (member.ShouldUseMember())
                {
                    filterMembers.Add(member);
                }
            }
            return filterMembers;
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
            BindingFlags bindingAttr = DefaultFlags; 
            if(memberInfo.MemberType== MemberTypes.Property)
            {
                PropertyInfo propertyInfo = (PropertyInfo)memberInfo;

                Type[] types = propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray();

                return targetType.GetProperty(propertyInfo.Name, bindingAttr, null, propertyInfo.PropertyType, types, null);
            }
            else
            {
                return targetType.GetMember(memberInfo.Name, memberInfo.MemberType, bindingAttr).SingleOrDefault();
            }
        }

        public delegate object GetDelegate(object source);


        public static bool ShouldUseMember(this MemberInfo memberInfo)
        {
            var propInfo = memberInfo as PropertyInfo;
            if (propInfo != null)
            {
                return propInfo.ShouldUseMember();
            }
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                //no backing fields
                if (fieldInfo.Name.Contains("k__BackingField"))
                {
                    return false;
                }

                return fieldInfo.ShouldUseMember();
            }
            return false;
        }

        public static bool ShouldUseMember(this PropertyInfo propertyInfo)
        {
            if (IsIndexedProperty(propertyInfo))
            {
                return false;
            }

            var attributes = propertyInfo.GetCustomAttributes(typeof(Attribute), true);
            if (attributes.Count() > 0)
            {
                foreach (var attr in attributes)
                {
                    if (attr is IgnoreDataMemberAttribute)
                    {
                        return false;
                    }
                    else if ((attr as ZippyDirectiveAttribute)?.Ignore ?? false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool ShouldUseMember(this FieldInfo fieldInfo)
        {
            var attributes = fieldInfo.GetCustomAttributes(typeof(Attribute), true);
            if (attributes.Count() > 0)
            {
                foreach (var attr in attributes)
                {
                    if (attr is IgnoreDataMemberAttribute)
                    {
                        return false;
                    }
                    else if ((attr as ZippyDirectiveAttribute)?.Ignore ?? false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static string GetSerializationName(this MemberInfo member)
        {
            var attributes = member.GetCustomAttributes(typeof(Attribute), true);
            if (attributes.Count() > 0)
            {
                foreach (var attr in attributes)
                {
                    if (attr is ZippyDirectiveAttribute)
                    {
                        ZippyDirectiveAttribute temp = (ZippyDirectiveAttribute)attr;
                        if (!temp.Name.IsNullOrEmpty())
                        {
                            return temp.Name;
                        }
                    }
                    else if (attr is DataMemberAttribute)
                    {
                        DataMemberAttribute temp = (DataMemberAttribute)attr;
                        if (!temp.Name.IsNullOrEmpty())
                        {
                            return temp.Name;
                        }
                    }
                }
            }

            return member.Name;
        }
    }
}

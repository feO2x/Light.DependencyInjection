using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.FrameworkExtensions
{
    public static class ReflectionExtensions
    {
        public static Func<object[], object> CompileStandardizedInstantiationFunction(this ConstructorInfo constructorInfo)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));

            if (constructorInfo.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
                return null;

            var constructorParameterInfos = constructorInfo.GetParameters();

            var parameterExpression = Expression.Parameter(typeof(object[]));
            if (constructorParameterInfos.Length == 0)
                return Expression.Lambda<Func<object[], object>>(Expression.New(constructorInfo), parameterExpression).Compile();

            var argumentExpressions = constructorParameterInfos.CreateParameterExpressions(parameterExpression);
            var newExpression = Expression.New(constructorInfo, argumentExpressions);
            return Expression.Lambda<Func<object[], object>>(newExpression, parameterExpression).Compile();
        }

        public static Func<object[], object> CompileStandardizedInstantiationFunction(this MethodInfo methodInfo)
        {
            methodInfo.MustNotBeNull(nameof(methodInfo));
            if (methodInfo.ReturnType.GetTypeInfo().IsGenericTypeDefinition)
                return null;

            var methodParameterInfos = methodInfo.GetParameters();

            var parameterExpression = Expression.Parameter(typeof(object[]));
            if (methodParameterInfos.Length == 0)
                return Expression.Lambda<Func<object[], object>>(Expression.Call(null, methodInfo), parameterExpression).Compile();

            var argumentExpressions = methodParameterInfos.CreateParameterExpressions(parameterExpression);
            var callStaticFactoryExpression = Expression.Call(null, methodInfo, argumentExpressions);
            return Expression.Lambda<Func<object[], object>>(callStaticFactoryExpression, parameterExpression).Compile();
        }

        private static List<Expression> CreateParameterExpressions(this ParameterInfo[] parameterInfos, ParameterExpression objectArrayExpression)
        {
            var argumentExpressions = new List<Expression>();
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var castExpression = Expression.Convert(Expression.ArrayIndex(objectArrayExpression, Expression.Constant(i)), parameterInfos[i].ParameterType);
                argumentExpressions.Add(castExpression);
            }
            return argumentExpressions;
        }

        public static ConstructorInfo FindDefaultConstructor(this IEnumerable<ConstructorInfo> constructors)
        {
            var constructorList = constructors.AsList();

            for (var i = 0; i < constructorList.Count; i++)
            {
                var constructor = constructorList[i];
                if (constructor.GetParameters().Length == 0)
                    return constructor;
            }

            return null;
        }

        public static ConstructorInfo FindConstructorWithArgumentTypes(this IEnumerable<ConstructorInfo> constructors, params Type[] parameterTypes)
        {
            parameterTypes.MustNotBeNull(nameof(parameterTypes));

            var constructorList = constructors.AsList();
            if (constructorList.Count == 0)
                return null;

            for (var i = 0; i < constructorList.Count; i++)
            {
                var constructorInfo = constructorList[i];
                var parameterInfos = constructorInfo.GetParameters();
                if (parameterInfos.Length != parameterTypes.Length)
                    continue;

                if (CheckParameterEquality(parameterInfos, parameterTypes))
                    return constructorInfo;
            }

            return null;
        }

        private static bool CheckParameterEquality(ParameterInfo[] parameterInfos, Type[] parameterTypes)
        {
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                if (parameterInfos[i].ParameterType != parameterTypes[i])
                    return false;
            }

            return true;
        }

        public static MethodInfo ExtractStaticFactoryMethod(this Expression<Func<object>> callStaticMethodExpression, Type targetType)
        {
            callStaticMethodExpression.MustNotBeNull(nameof(callStaticMethodExpression));

            var methodCallExpression = callStaticMethodExpression.Body as MethodCallExpression;
            CheckMethodCallExpression(methodCallExpression, targetType);

            // ReSharper disable once PossibleNullReferenceException
            return methodCallExpression.Method;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckMethodCallExpression(MethodCallExpression methodCallExpression, Type targetType)
        {
            if (methodCallExpression != null &&
                methodCallExpression.Method.IsPublicStaticCreationMethodForType(targetType))
                return;

            throw new TypeRegistrationException($"Your expression to select a static factory method for type {targetType} does not describe a public static method. A valid example would be \"() => MyType.Create(default(string), default(Foo))\".", targetType);
        }

        public static bool IsPublicStaticCreationMethodForType(this MethodInfo methodInfo, Type targetType)
        {
            methodInfo.MustNotBeNull(nameof(methodInfo));
            targetType.MustNotBeNull(nameof(targetType));

            return methodInfo.IsStatic &&
                   methodInfo.IsPublic &&
                   methodInfo.ReturnType == targetType;
        }

        public static PropertyInfo ExtractSettableInstancePropertyInfo<T, TProperty>(this Expression<Func<T, TProperty>> expression, Type targetType)
        {
            expression.MustNotBeNull(nameof(expression));

            var memberExpression = expression.Body as MemberExpression;
            CheckPropertyExpressionDowncast(memberExpression, targetType);

            // ReSharper disable once PossibleNullReferenceException
            var propertyInfo = memberExpression.Member as PropertyInfo;
            CheckPropertyInfo(propertyInfo, targetType);

            // ReSharper disable once PossibleNullReferenceException
            return propertyInfo;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckPropertyExpressionDowncast(Expression expression, Type targetType)
        {
            if (expression != null)
                return;

            throw new TypeRegistrationException($"The specified expression does not describe a settable instance property of type \"{targetType}\". Please use an expression like the following one: \"o => o.Property\".", targetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckPropertyInfo(PropertyInfo propertyInfo, Type targetType)
        {
            if (propertyInfo.IsPublicSettableInstancePropertyInfo() == false)
                throw new TypeRegistrationException($"The specified expression does not describe a settable instance property of type \"{targetType}\". Please use an expression like the following one: \"o => o.Property\".", targetType);
            if (propertyInfo.DeclaringType != targetType)
                throw new TypeRegistrationException($"The property info you provided does not belong to the target type \"{targetType}\".", targetType);
        }

        public static bool IsPublicSettableInstancePropertyInfo(this PropertyInfo propertyInfo)
        {
            return propertyInfo != null && propertyInfo.CanWrite && propertyInfo.SetMethod.IsStatic == false && propertyInfo.GetIndexParameters().Length == 0;
        }

        public static FieldInfo ExtractSettableInstanceFieldInfo<T, TField>(this Expression<Func<T, TField>> expression, Type targetType)
        {
            expression.MustNotBeNull(nameof(expression));

            var memberExpression = expression.Body as MemberExpression;
            CheckFieldExpressionDowncast(memberExpression, targetType);
            // ReSharper disable once PossibleNullReferenceException
            var fieldInfo = memberExpression.Member as FieldInfo;
            CheckFieldInfo(fieldInfo, targetType);

            // ReSharper disable once PossibleNullReferenceException
            return fieldInfo;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckFieldExpressionDowncast(MemberExpression expression, Type targetType)
        {
            if (expression != null)
                return;

            throw new TypeRegistrationException($"The specified expression does not describe a settable instance field of type \"{targetType}\". Please use an expression like the following one: \"o => o.Field\".", targetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckFieldInfo(FieldInfo fieldInfo, Type targetType)
        {
            if (fieldInfo.IsPublicSettableInstanceFieldInfo() == false)
                throw new TypeRegistrationException($"The specified expression does not describe a settable instance field of type \"{targetType}\". Please use an expression like the following one: \"o => o.Field\".", targetType);
            if (fieldInfo.DeclaringType != targetType)
                throw new TypeRegistrationException($"The field info you provided does not belong to the target type \"{targetType}\".", targetType);
        }

        public static bool IsPublicSettableInstanceFieldInfo(this FieldInfo fieldInfo)
        {
            return fieldInfo != null && fieldInfo.IsPublic && fieldInfo.IsStatic == false && fieldInfo.IsInitOnly == false;
        }

        public static ParameterDependency[] CreateDefaultInstantiationDependencies(this MethodBase methodInfo)
        {
            methodInfo.MustNotBeNull(nameof(methodInfo));

            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length == 0)
                return null;

            var instantiationDependencies = new ParameterDependency[parameterInfos.Length];
            for (var i = 0; i < parameterInfos.Length; ++i)
            {
                instantiationDependencies[i] = new ParameterDependency(parameterInfos[i]);
            }

            return instantiationDependencies;
        }

        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors.FindDefaultConstructor();
        }

        public static ConstructorInfo GetDefaultConstructor(this TypeInfo typeInfo)
        {
            typeInfo.MustNotBeNull(nameof(typeInfo));

            return typeInfo.DeclaredConstructors.FindDefaultConstructor();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.FrameworkExtensions
{
    public static class ReflectionExtensions
    {
        public static Func<object[], object> CompileStandardizedInstantiationFunction(this ConstructorInfo constructorInfo)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));

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

            if (constructorList.Count == 0)
                return null;

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
    }
}
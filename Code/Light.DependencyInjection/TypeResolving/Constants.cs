using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.TypeResolving
{
    public static class Constants
    {
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type ResolveContextType = typeof(ResolveContext);
        public static readonly Type DependencyOverridesType = typeof(DependencyOverrides);
        public static readonly Type ContainerType = typeof(DependencyInjectionContainer);
        public static readonly Type ContainerScopeType = typeof(ContainerScope);

        // ReSharper disable InconsistentNaming
        public static readonly Type IEnumerableGenericTypeDefinition = typeof(IEnumerable<>);
        public static readonly Type ICollectionGenericTypeDefinition = typeof(ICollection<>);
        public static readonly Type IDisposableType = typeof(IDisposable);
        // ReSharper restore InconsistentNaming

        public static readonly MethodInfo LifetimeResolveInstanceMethod = typeof(Lifetime).GetTypeInfo().GetDeclaredMethod(nameof(Lifetime.ResolveInstance));

        public static readonly MethodInfo ChangeResolvedTypeMethod = ResolveContextType.GetTypeInfo().GetDeclaredMethod(nameof(ResolveContext.ChangeResolvedType));
        public static readonly MethodInfo ChangeRegistrationMethod = ResolveContextType.GetTypeInfo().GetDeclaredMethod(nameof(ResolveContext.ChangeRegistration));
        public static readonly PropertyInfo ResolveContextContainerProperty = ResolveContextType.GetTypeInfo().GetDeclaredProperty(nameof(ResolveContext.Container));
        public static readonly FieldInfo ContainerScopeField = ContainerType.GetTypeInfo().GetDeclaredField(nameof(DependencyInjectionContainer.Scope));
        public static readonly MethodInfo ScopeAddDisposableMethod = ContainerScopeType.GetTypeInfo().GetDeclaredMethod(nameof(ContainerScope.AddDisposable));
        public static readonly MethodInfo GetDependencyOverridesProperty = ResolveContextType.GetTypeInfo().GetDeclaredProperty(nameof(ResolveContext.DependencyOverrides)).GetMethod;
        public static readonly MethodInfo GetDependencyInstanceMethod = DependencyOverridesType.GetTypeInfo().GetDeclaredMethod(nameof(DependencyOverrides.GetDependencyInstance));
        public static readonly MethodInfo GetOverriddenInstanceMethod = DependencyOverridesType.GetTypeInfo().GetDeclaredMethod(nameof(DependencyOverrides.GetOverriddenInstance));
        public static readonly ParameterExpression ResolveContextParameterExpression = Expression.Parameter(ResolveContextType);
    }
}
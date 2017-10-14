using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Lifetimes;

namespace Light.DependencyInjection.TypeResolving
{
    public static class CompilationConstants
    {
        public static readonly MethodInfo LifetimeResolveInstanceMethod = typeof(Lifetime).GetTypeInfo().GetDeclaredMethod(nameof(Lifetime.ResolveInstance));

        public static readonly MethodInfo ChangeResolvedTypeMethod = KnownTypes.ResolveContextType.GetTypeInfo().GetDeclaredMethod(nameof(ResolveContext.ChangeResolvedType));
        public static readonly MethodInfo ChangeRegistrationMethod = KnownTypes.ResolveContextType.GetTypeInfo().GetDeclaredMethod(nameof(ResolveContext.ChangeRegistration));
        public static readonly MethodInfo GetDependencyOverridesProperty = KnownTypes.ResolveContextType.GetTypeInfo().GetDeclaredProperty(nameof(ResolveContext.DependencyOverrides)).GetMethod;
        public static readonly MethodInfo GetDependencyInstanceMethod = KnownTypes.DependencyOverridesType.GetTypeInfo().GetDeclaredMethod(nameof(DependencyOverrides.GetDependencyInstance));
        public static readonly MethodInfo GetOverriddenInstanceMethod = KnownTypes.DependencyOverridesType.GetTypeInfo().GetDeclaredMethod(nameof(DependencyOverrides.GetOverriddenInstance));
        public static readonly ParameterExpression ResolveContextParameterExpression = Expression.Parameter(KnownTypes.ResolveContextType);
    }
}
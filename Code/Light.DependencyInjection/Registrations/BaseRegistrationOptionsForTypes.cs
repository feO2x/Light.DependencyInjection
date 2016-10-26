using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public abstract class BaseRegistrationOptionsForTypes<TConcreteOptions> : BaseRegistrationOptionsForExternalInstance<TConcreteOptions>, IBaseRegistrationOptionsForType<TConcreteOptions> where TConcreteOptions : class, IBaseRegistrationOptionsForType<TConcreteOptions>
    {
        protected readonly IConstructorSelector ConstructorSelector;
        protected List<InstanceInjection> InstanceInjections;
        protected InstantiationInfo InstantiationInfo;

        protected BaseRegistrationOptionsForTypes(Type targetType, IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes)
            : base(targetType, ignoredAbstractionTypes)
        {
            targetType.MustBeRegistrationCompliant();
            constructorSelector.MustNotBeNull(nameof(constructorSelector));

            ConstructorSelector = constructorSelector;
        }


        public TConcreteOptions UseConstructor(ConstructorInfo constructorInfo)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));

            AssignConstructor(constructorInfo);
            return This;
        }

        public TConcreteOptions UseDefaultConstructor()
        {
            var targetConstructor = TargetTypeInfo.DeclaredConstructors.FindDefaultConstructor();
            EnsureTargetConstructorIsNotNull(targetConstructor, null);
            AssignConstructor(targetConstructor);
            return This;
        }

        public TConcreteOptions UseConstructorWithParameters(params Type[] parameterTypes)
        {
            parameterTypes.MustNotBeNull(nameof(parameterTypes));

            var targetConstructor = TargetTypeInfo.DeclaredConstructors.FindConstructorWithArgumentTypes(parameterTypes);
            EnsureTargetConstructorIsNotNull(targetConstructor, parameterTypes);

            AssignConstructor(targetConstructor);
            return This;
        }

        public TConcreteOptions UseConstructorWithParameter<TArgument>()
        {
            return UseConstructorWithParameters(typeof(TArgument));
        }

        public TConcreteOptions UseConstructorWithParameters<T1, T2>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2));
        }

        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3));
        }

        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }

        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }

        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        }

        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        }

        public IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter<TParameter>()
        {
            AssignInstantiationMethodIfNeccessary();

            var parameterType = typeof(TParameter);
            var targetParameters = InstantiationInfo.InstantiationDependencies
                                                    ?.Where(p => p.ParameterType == parameterType)
                                                    .ToList();
            CheckTargetParametersWithoutName(targetParameters, parameterType);

            // ReSharper disable once PossibleNullReferenceException
            return new ChildRegistrationNameOptions<TConcreteOptions>(This, targetParameters[0]);
        }

        public TConcreteOptions ResolveAllForInstantiationParameter<TParameter>()
        {
            AssignInstantiationMethodIfNeccessary();

            var parameterType = typeof(TParameter);
            var targetParameters = InstantiationInfo.InstantiationDependencies?.Where(p => p.ParameterType == parameterType)
                                                    .ToList();
            CheckTargetParametersWithoutName(targetParameters, parameterType);

            // ReSharper disable once PossibleNullReferenceException
            targetParameters[0].DependencyResolver = ResolveAll.Create(parameterType.GetItemTypeOfEnumerable());
            return This;
        }

        public TConcreteOptions ResolveAllForInstantiationParameter(string parameterName)
        {
            AssignInstantiationMethodIfNeccessary();

            var targetParameters = InstantiationInfo.InstantiationDependencies
                                                    ?.Where(p => p.TargetParameter.Name == parameterName)
                                                    .ToList();
            CheckTargetParametersWithName(targetParameters, parameterName);

            // ReSharper disable once PossibleNullReferenceException
            var targetParameter = targetParameters[0];
            targetParameter.DependencyResolver = ResolveAll.Create(targetParameter.ParameterType.GetItemTypeOfEnumerable());

            return This;
        }

        public TConcreteOptions UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo)
        {
            staticFactoryMethodInfo.MustNotBeNull(nameof(staticFactoryMethodInfo));

            AssignStaticCreationMethod(staticFactoryMethodInfo);
            return This;
        }

        public TConcreteOptions AddPropertyInjection(PropertyInfo propertyInfo, string resolvedRegistrationName = null)
        {
            propertyInfo.MustNotBeNull(nameof(propertyInfo));
            CheckPropertyInfo(propertyInfo, TargetType);

            AddInstanceInjection(new PropertyInjection(propertyInfo, resolvedRegistrationName));
            return This;
        }

        public TConcreteOptions AddFieldInjection(FieldInfo fieldInfo, string resolvedRegistrationName = null)
        {
            fieldInfo.MustNotBeNull(nameof(fieldInfo));
            CheckFieldInfo(fieldInfo, TargetType);

            AddInstanceInjection(new FieldInjection(fieldInfo, resolvedRegistrationName));
            return This;
        }

        public IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter(string parameterName)
        {
            AssignInstantiationMethodIfNeccessary();

            var targetParameters = InstantiationInfo.InstantiationDependencies
                                                    ?.Where(p => p.TargetParameter.Name == parameterName)
                                                    .ToList();
            CheckTargetParametersWithName(targetParameters, parameterName);

            // ReSharper disable once PossibleNullReferenceException
            return new ChildRegistrationNameOptions<TConcreteOptions>(This, targetParameters[0]);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        protected void CheckTargetParametersWithoutName(List<InstantiationDependency> targetParameters, Type targetParameterType)
        {
            if (targetParameters == null || targetParameters.Count == 0)
                throw new TypeRegistrationException($"The specified instantiation method for type \"{TargetType}\" does not have a parameter of type \"{targetParameterType}\".", TargetType);

            if (targetParameters.Count == 1)
                return;

            throw new TypeRegistrationException($"The specified instantiation method for type \"{TargetType}\" has several parameters with type \"{targetParameterType}\". Please use the overload of \"{nameof(ResolveInstantiationParameter)}\" where an additional parameter name can be specified.", TargetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        protected void CheckTargetParametersWithName(List<InstantiationDependency> targetParameters, string parameterName)
        {
            if (targetParameters == null || targetParameters.Count == 1)
                return;

            throw new TypeRegistrationException($"The specified instantiation method for type \"{TargetType}\" does not have a parameter with name \"{parameterName}\".", TargetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        protected static void CheckStaticCreationMethodInfo(MethodInfo methodInfo, Type targetType)
        {
            if (methodInfo.IsPublicStaticCreationMethodForType(targetType))
                return;

            throw new TypeRegistrationException($"The specified method info does not describe a public, static method that returns an instance of type {targetType}", targetType);
        }

        protected void AssignStaticCreationMethod(MethodInfo staticCreationMethod)
        {
            InstantiationInfo = new StaticMethodInstantiationInfo(TargetType, staticCreationMethod);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        protected static void CheckStaticCreationMethodFromDelegate(MethodInfo methodInfo, Type targetType)
        {
            if (methodInfo.IsPublicStaticCreationMethodForType(targetType))
                return;

            throw new TypeRegistrationException($"The specified delegate does not describe a public, static method that returns an instance of type {targetType}.", targetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        protected static void CheckPropertyInfo(PropertyInfo propertyInfo, Type targetType)
        {
            if (propertyInfo.DeclaringType == targetType)
                return;

            throw new TypeRegistrationException($"The property info you provided does not belong to the target type \"{targetType}\".", targetType);
        }

        protected void AddInstanceInjection(InstanceInjection instanceInjection)
        {
            if (InstanceInjections == null)
                InstanceInjections = new List<InstanceInjection>();

            InstanceInjections.Add(instanceInjection);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        protected static void CheckFieldInfo(FieldInfo fieldInfo, Type targetType)
        {
            if (fieldInfo.DeclaringType == targetType)
                return;

            throw new TypeRegistrationException($"The field info you provided does not belong to the target type \"{targetType}\".", targetType);
        }

        protected void AssignInstantiationMethodIfNeccessary()
        {
            if (InstantiationInfo != null)
                return;

            var targetConstructor = ConstructorSelector.SelectTargetConstructor(TargetTypeInfo);
            AssignConstructor(targetConstructor);
        }


        protected void AssignConstructor(ConstructorInfo targetConstructor)
        {
            InstantiationInfo = new ConstructorInstantiationInfo(targetConstructor);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        protected void EnsureTargetConstructorIsNotNull(ConstructorInfo targetConstructor, Type[] parameterTypes)
        {
            if (targetConstructor != null)
                return;

            if (parameterTypes == null || parameterTypes.Length == 0)
                throw new TypeRegistrationException($"You specified that the DI container should use the default constructor of type \"{TargetType}\", but this type contains no default constructor.", TargetType);

            if (parameterTypes.Length == 1)
                throw new TypeRegistrationException($"You specified that the DI container should use the constructor with a single parameter of type \"{parameterTypes[0]}\", but type \"{TargetType}\" does not contain such a constructor.", TargetType);

            var message = new StringBuilder().Append("You specified that the DI container should use the constructor with the type parameters ")
                                             .AppendWordEnumeration(parameterTypes)
                                             .Append($", but type \"{TargetType}\" does not contain such a constructor.")
                                             .ToString();

            throw new TypeRegistrationException(message, TargetType);
        }

        public TypeCreationInfo BuildTypeCreationInfo()
        {
            AssignInstantiationMethodIfNeccessary();

            return new TypeCreationInfo(new TypeKey(TargetType, RegistrationName), InstantiationInfo, InstanceInjections?.ToArray());
        }

        public void CreateAndAddRegistration(DependencyInjectionContainer targetContainer, Lifetime lifetime)
        {
            targetContainer.MustNotBeNull(nameof(targetContainer));

            var registration = new Registration(lifetime,
                                                BuildTypeCreationInfo(),
                                                IsContainerTrackingDisposables);
            targetContainer.Register(registration, AbstractionTypes);
        }

        protected TConcreteOptions SetDelegateInstantiationInfo(Delegate @delegate)
        {
            InstantiationInfo = new DelegateInstantiationInfo(TargetType, @delegate);
            return This;
        }
    }
}
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
    /// <summary>
    ///     Represents the abstraction for configuring registrations that are instantiated by the DI container.
    /// </summary>
    /// <typeparam name="TConcreteOptions">The options type that is returned by the fluent API.</typeparam>
    public abstract class BaseRegistrationOptionsForType<TConcreteOptions> : BaseRegistrationOptionsForExternalInstance<TConcreteOptions>, IBaseRegistrationOptionsForType<TConcreteOptions> where TConcreteOptions : class, IBaseRegistrationOptionsForType<TConcreteOptions>
    {
        /// <summary>
        ///     Gets the service that selects a constructor from the target type when the client did not specify any.
        /// </summary>
        protected readonly IConstructorSelector ConstructorSelector;

        /// <summary>
        ///     Gets or sets the list of instance injections that will be executed when the target type was instantiated by the DI container.
        /// </summary>
        protected List<InstanceInjection> InstanceInjections;

        /// <summary>
        ///     Gets or sets the info describing how the target type is instantiated.
        /// </summary>
        protected InstantiationInfo InstantiationInfo;

        /// <summary>
        ///     Initializes a new instance of <see cref="BaseRegistrationOptionsForType{TConcreteOptions}" />
        /// </summary>
        /// <param name="targetType">The target type of these options.</param>
        /// <param name="constructorSelector">The service that selects a constructor from the target type when the client did not specify any.</param>
        /// <param name="ignoredAbstractionTypes">The list of abstraction types that are ignored when abstraction types are mapped to the target type.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when <paramref name="targetType" /> is an interface, an abstract class, a generic parameter type, or a open generic type.</exception>
        protected BaseRegistrationOptionsForType(Type targetType, IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes)
            : base(targetType, ignoredAbstractionTypes)
        {
            targetType.MustBeRegistrationCompliant();
            constructorSelector.MustNotBeNull(nameof(constructorSelector));

            ConstructorSelector = constructorSelector;
        }


        /// <inheritdoc />
        public TConcreteOptions UseConstructor(ConstructorInfo constructorInfo)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));

            AssignConstructor(constructorInfo);
            return This;
        }

        /// <inheritdoc />
        public TConcreteOptions UseDefaultConstructor()
        {
            var targetConstructor = TargetTypeInfo.DeclaredConstructors.FindDefaultConstructor();
            EnsureTargetConstructorIsNotNull(targetConstructor, null);
            AssignConstructor(targetConstructor);
            return This;
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters(params Type[] parameterTypes)
        {
            parameterTypes.MustNotBeNull(nameof(parameterTypes));

            var targetConstructor = TargetTypeInfo.DeclaredConstructors.FindConstructorWithArgumentTypes(parameterTypes);
            EnsureTargetConstructorIsNotNull(targetConstructor, parameterTypes);

            AssignConstructor(targetConstructor);
            return This;
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameter<TArgument>()
        {
            return UseConstructorWithParameters(typeof(TArgument));
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters<T1, T2>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2));
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3));
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        }

        /// <inheritdoc />
        public TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public TConcreteOptions UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo)
        {
            staticFactoryMethodInfo.MustNotBeNull(nameof(staticFactoryMethodInfo));

            AssignStaticCreationMethod(staticFactoryMethodInfo);
            return This;
        }

        /// <inheritdoc />
        public TConcreteOptions AddPropertyInjection(PropertyInfo propertyInfo, string targetRegistrationName = null)
        {
            propertyInfo.MustNotBeNull(nameof(propertyInfo));
            CheckPropertyInfo(propertyInfo, TargetType);

            AddInstanceInjection(new PropertyInjection(TargetType, propertyInfo, targetRegistrationName));
            return This;
        }

        /// <inheritdoc />
        public TConcreteOptions AddFieldInjection(FieldInfo fieldInfo, string targetRegistrationName = null)
        {
            fieldInfo.MustNotBeNull(nameof(fieldInfo));
            CheckFieldInfo(fieldInfo, TargetType);

            AddInstanceInjection(new FieldInjection(fieldInfo, targetRegistrationName));
            return This;
        }

        /// <inheritdoc />
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

        /// <summary>
        ///     Checks that the specified <paramref name="targetParameterType" /> is present in <paramref name="targetParameters" />.
        /// </summary>
        [Conditional(Check.CompileAssertionsSymbol)]
        protected void CheckTargetParametersWithoutName(List<InstantiationDependency> targetParameters, Type targetParameterType)
        {
            if (targetParameters == null || targetParameters.Count == 0)
                throw new TypeRegistrationException($"The specified instantiation method for type \"{TargetType}\" does not have a parameter of type \"{targetParameterType}\".", TargetType);

            if (targetParameters.Count == 1)
                return;

            throw new TypeRegistrationException($"The specified instantiation method for type \"{TargetType}\" has several parameters with type \"{targetParameterType}\". Please use the overload of \"{nameof(ResolveInstantiationParameter)}\" where an additional parameter name can be specified.", TargetType);
        }

        /// <summary>
        ///     Checks that a parameter with the specified name is present in <paramref name="targetParameters" />.
        /// </summary>
        [Conditional(Check.CompileAssertionsSymbol)]
        protected void CheckTargetParametersWithName(List<InstantiationDependency> targetParameters, string parameterName)
        {
            if (targetParameters == null || targetParameters.Count == 1)
                return;

            throw new TypeRegistrationException($"The specified instantiation method for type \"{TargetType}\" does not have a parameter with name \"{parameterName}\".", TargetType);
        }

        /// <summary>
        ///     Assigs the specified <paramref name="staticCreationMethod" /> as <see cref="InstantiationInfo" />.
        /// </summary>
        protected void AssignStaticCreationMethod(MethodInfo staticCreationMethod)
        {
            InstantiationInfo = new StaticMethodInstantiationInfo(TargetType, staticCreationMethod);
        }

        /// <summary>
        ///     Checks if the property info belongs to the target type.
        /// </summary>
        [Conditional(Check.CompileAssertionsSymbol)]
        protected static void CheckPropertyInfo(PropertyInfo propertyInfo, Type targetType)
        {
            if (propertyInfo.DeclaringType == targetType)
                return;

            throw new TypeRegistrationException($"The property info you provided does not belong to the target type \"{targetType}\".", targetType);
        }

        /// <summary>
        ///     Adds the specified instance injection to the list.
        /// </summary>
        protected void AddInstanceInjection(InstanceInjection instanceInjection)
        {
            if (InstanceInjections == null)
                InstanceInjections = new List<InstanceInjection>();

            InstanceInjections.Add(instanceInjection);
        }

        /// <summary>
        ///     Checks if the field info belongs to the target type.
        /// </summary>
        [Conditional(Check.CompileAssertionsSymbol)]
        protected static void CheckFieldInfo(FieldInfo fieldInfo, Type targetType)
        {
            if (fieldInfo.DeclaringType == targetType)
                return;

            throw new TypeRegistrationException($"The field info you provided does not belong to the target type \"{targetType}\".", targetType);
        }

        /// <summary>
        ///     Creates and assigns a default instantiation info using the <see cref="ConstructorSelector" />, if necessary.
        /// </summary>
        protected void AssignInstantiationMethodIfNeccessary()
        {
            if (InstantiationInfo != null)
                return;

            var targetConstructor = ConstructorSelector.SelectTargetConstructor(TargetTypeInfo);
            AssignConstructor(targetConstructor);
        }

        /// <summary>
        ///     Assigns the specified constructor as Instantiation Info.
        /// </summary>
        protected void AssignConstructor(ConstructorInfo targetConstructor)
        {
            InstantiationInfo = new ConstructorInstantiationInfo(targetConstructor);
        }

        /// <summary>
        ///     Ensures that the found constructor is not null.
        /// </summary>
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

        /// <summary>
        ///     Creates a new <see cref="TypeCreationInfo" /> instance from the information of these options.
        /// </summary>
        /// <returns></returns>
        public TypeCreationInfo BuildTypeCreationInfo()
        {
            AssignInstantiationMethodIfNeccessary();

            return new TypeCreationInfo(new TypeKey(TargetType, RegistrationName), InstantiationInfo, InstanceInjections?.ToArray());
        }

        /// <summary>
        ///     Creates a new <see cref="Registration" /> instance from the information of these options and registers it with the container.
        /// </summary>
        /// <param name="targetContainer">The container to be populated.</param>
        /// <param name="lifetime">The lifetime that should be associated with the target registration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetContainer" /> or <paramref name="lifetime" /> is null.</exception>
        public void CreateAndAddRegistration(DependencyInjectionContainer targetContainer, Lifetime lifetime)
        {
            targetContainer.MustNotBeNull(nameof(targetContainer));

            var registration = new Registration(lifetime,
                                                BuildTypeCreationInfo(),
                                                IsContainerTrackingDisposables);
            targetContainer.Register(registration, AbstractionTypes);
        }

        /// <summary>
        ///     Assigns the specified delegate a Instantiation Info.
        /// </summary>
        protected TConcreteOptions SetDelegateInstantiationInfo(Delegate @delegate)
        {
            InstantiationInfo = new DelegateInstantiationInfo(TargetType, @delegate);
            return This;
        }
    }
}
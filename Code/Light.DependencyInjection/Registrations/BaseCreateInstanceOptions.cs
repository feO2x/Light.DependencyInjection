using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.Lifetimes;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public abstract class BaseCreateInstanceOptions<TOptions> : CommonRegistrationOptions<TOptions>, ICreateInstanceOptions<TOptions> where TOptions : class, ICreateInstanceOptions<TOptions>
    {
        protected readonly IDefaultInstantiationInfoSelector DefaultInstantiationInfoSelector;
        protected readonly List<InstanceManipulationFactory> InstanceManipulationFactories = new List<InstanceManipulationFactory>();
        private InstantiationInfoFactory _instantiationInfoFactory;
        private Lifetime _lifetime = TransientLifetime.Instance;

        protected BaseCreateInstanceOptions(Type targetType, IReadOnlyList<Type> ignoredAbstractionTypes, IDefaultInstantiationInfoSelector defaultInstantiationInfoSelector) : base(targetType, ignoredAbstractionTypes)
        {
            DefaultInstantiationInfoSelector = defaultInstantiationInfoSelector.MustNotBeNull(nameof(defaultInstantiationInfoSelector));
        }

        protected Lifetime Lifetime
        {
            get => _lifetime;
            set => _lifetime = value.MustNotBeNull();
        }

        protected InstantiationInfoFactory InstantiationInfoFactory
        {
            get => _instantiationInfoFactory;
            set => _instantiationInfoFactory = value.MustNotBeNull();
        }

        public TOptions UseConstructor(ConstructorInfo constructorInfo)
        {
            _instantiationInfoFactory = new ConstructorInstantiationInfoFactory(constructorInfo);
            return This;
        }

        public TOptions UseDefaultConstructor()
        {
            var constructor = EnsureTargetConstructorIsNotNull(TargetTypeInfo.DeclaredConstructors.FindDefaultConstructor());
            return UseConstructor(constructor);
        }

        public TOptions UseConstructorWithParameters(params Type[] parameterTypes)
        {
            var constructor = EnsureTargetConstructorIsNotNull(TargetTypeInfo.DeclaredConstructors.FindConstructorWithArgumentTypes(parameterTypes));
            return UseConstructor(constructor);
        }

        public TOptions UseConstructorWithParameter<T>()
        {
            return UseConstructorWithParameters(typeof(T));
        }

        public TOptions UseConstructorWithParameter<T1, T2>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2));
        }

        public TOptions UseConstructorWithParameter<T1, T2, T3>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3));
        }

        public TOptions UseConstructorWithParameter<T1, T2, T3, T4>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6, T7>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }

        public TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }

        public TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        }

        public TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        }

        public TOptions InstantiateVia(MethodInfo methodInfo)
        {
            if (methodInfo.IsStatic == false)
                throw new NotSupportedException("Non-static methods are currently not supported.");

            InstantiationInfoFactory = new StaticMethodInstantiationInfoFactory(TargetType, methodInfo);
            return This;
        }

        public TOptions ConfigureInstantiationParameter<TParameter>(Action<IDependencyOptions> configureDependency)
        {
            return ConfigureInstantiationParameter(typeof(TParameter), configureDependency);
        }

        public TOptions ConfigureInstantiationParameter(Type parameterType, Action<IDependencyOptions> configureDependency)
        {
            parameterType.MustNotBeNull(nameof(parameterType));
            configureDependency.MustNotBeNull(nameof(configureDependency));

            AssignInstantiationInfoFactoryIfNecessary();

            var targetDependencyFactories = _instantiationInfoFactory.InstantiationDependencyFactories
                                                                     .Where(dependency => dependency.DependencyType == parameterType)
                                                                     .ToList();

            if (targetDependencyFactories.Count > 1)
                throw new RegistrationException($"There are several parameters with type \"{parameterType}\" - please use the overload of {nameof(ConfigureInstantiationParameter)} that accepts a unique parameter name.");
            if (targetDependencyFactories.Count == 0)
                throw new RegistrationException($"There is no parameter with type \"{parameterType}\".");

            configureDependency(targetDependencyFactories[0]);

            return This;
        }

        public TOptions ConfigureInstantiationParameter(string parameterName, Action<IDependencyOptions> configureDependency, StringComparison nameComparisonType = StringComparison.CurrentCulture)
        {
            parameterName.MustNotBeNullOrWhiteSpace(nameof(parameterName));
            configureDependency.MustNotBeNull(nameof(configureDependency));
            nameComparisonType.MustBeValidEnumValue(nameof(nameComparisonType));

            AssignInstantiationInfoFactoryIfNecessary();

            var targetDependencyFactories = _instantiationInfoFactory.InstantiationDependencyFactories
                                                                   .Where(dependency => dependency.Name.Equals(parameterName, nameComparisonType))
                                                                   .ToList();
            if (targetDependencyFactories.Count > 1)
                throw new RegistrationException($"There are several parameters that equal the parameterName \"{parameterName}\" (using comparison type \"{nameComparisonType}\").");

            if (targetDependencyFactories.Count == 0)
                throw new RegistrationException($"There is no parameter with name \"{parameterName}\".");

            configureDependency(targetDependencyFactories[0]);

            return This;
        }

        public TOptions AddPropertyInjection(PropertyInfo propertyInfo, string targetRegistrationName = "")
        {
            var propertyInjectionFactory = new PropertyInjectionFactory(propertyInfo);
            propertyInjectionFactory.DependencyFactory.WithTargetRegistrationName(targetRegistrationName);
            InstanceManipulationFactories.AddOrReplace(propertyInjectionFactory);
            return This;
        }

        public TOptions AddPropertyInjection(string propertyName, string targetRegistrationName = "")
        {
            return AddPropertyInjection(TargetType.GetRuntimeProperty(propertyName), targetRegistrationName);
        }

        public TOptions AddFieldInjection(FieldInfo fieldInfo, string targetRegistrationName = "")
        {
            var fieldInjectionFactory = new FieldInjectionFactory(fieldInfo);
            fieldInjectionFactory.DependencyFactory.WithTargetRegistrationName(targetRegistrationName);
            InstanceManipulationFactories.AddOrReplace(fieldInjectionFactory);
            return This;
        }

        public TOptions AddFieldInjection(string fieldName, string targetRegistrationName = "")
        {
            return AddFieldInjection(TargetType.GetRuntimeField(fieldName), targetRegistrationName);
        }

        public TOptions UseLifetime(Lifetime lifetime)
        {
            Lifetime = lifetime;
            return This;
        }

        protected ConstructorInfo EnsureTargetConstructorIsNotNull(ConstructorInfo targetConstructor, Type[] parameterTypes = null)
        {
            if (targetConstructor != null)
                return targetConstructor;

            if (parameterTypes.IsNullOrEmpty())
                throw new RegistrationException($"You specified that the DI container should use the default constructor of type \"{TargetType}\", but this type contains no default constructor.");

            // ReSharper disable once PossibleNullReferenceException
            if (parameterTypes.Length == 1)
                throw new RegistrationException($"You specified that the DI container should use the constructor with a single parameter of type \"{parameterTypes[0]}\", but type \"{TargetType}\" does not contain such a constructor.");

            var message = new StringBuilder().Append("You specified that the DI container should use the constructor with the type parameters ")
                                             .AppendItems(parameterTypes)
                                             .Append($", but type \"{TargetType}\" does not contain such a constructor.")
                                             .ToString();

            throw new RegistrationException(message);
        }

        protected void AssignInstantiationInfoFactoryIfNecessary()
        {
            if (InstantiationInfoFactory == null)
                InstantiationInfoFactory = DefaultInstantiationInfoSelector.GetDefaultInstantiationInfo(TargetTypeInfo);
        }

        protected TOptions SetDelegateInstantiationInfoFactory(Delegate @delegate)
        {
            _instantiationInfoFactory = new DelegateInstantiationInfoFactory(TargetType, @delegate);
            return This;
        }

        public virtual Registration CreateRegistration()
        {
            AssignInstantiationInfoFactoryIfNecessary();

            var typeKey = new TypeKey(TargetType, RegistrationName);
            return new Registration(typeKey,
                                    _lifetime,
                                    new TypeConstructionInfo(typeKey,
                                                             _instantiationInfoFactory.Create(RegistrationName),
                                                             InstanceManipulationFactories.CreateInstanceManipulations(RegistrationName)),
                                    MappedAbstractionTypes.Count > 0 ? MappedAbstractionTypes.AsReadOnlyList() : null,
                                    IsTrackingDisposables);
        }
    }
}
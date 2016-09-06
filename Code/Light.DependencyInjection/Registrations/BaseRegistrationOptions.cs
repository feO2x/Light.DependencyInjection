using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public abstract class BaseRegistrationOptions<TConcreteOptions> : IBaseRegistrationOptions<TConcreteOptions> where TConcreteOptions : class, IBaseRegistrationOptions<TConcreteOptions>
    {
        private readonly TConcreteOptions _this;
        protected readonly HashSet<Type> AbstractionTypes = new HashSet<Type>();
        protected readonly IConstructorSelector ConstructorSelector;
        protected readonly IReadOnlyList<Type> IgnoredAbstractionTypes;
        protected readonly Type TargetType;
        protected readonly TypeInfo TargetTypeInfo;
        protected List<InstanceInjection> InstanceInjections;
        protected InstantiationInfo InstantiationInfo;
        public bool IsContainerTrackingDisposables { get; protected set; } = true;

        protected BaseRegistrationOptions(Type targetType, IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes)
        {
            targetType.MustNotBeNull(nameof(targetType));
            targetType.MustBeInstantiatable();
            targetType.MustBeNonGenericOrClosedConstructedOrGenericTypeDefinition();
            ignoredAbstractionTypes.MustNotBeNull(nameof(ignoredAbstractionTypes));
            constructorSelector.MustNotBeNull(nameof(constructorSelector));
            _this = this as TConcreteOptions;
            EnsureThisIsNotNull();

            TargetType = targetType;
            TargetTypeInfo = targetType.GetTypeInfo();
            IgnoredAbstractionTypes = ignoredAbstractionTypes;
            ConstructorSelector = constructorSelector;
        }

        public IEnumerable<Type> MappedAbstractionTypes => AbstractionTypes;

        public string RegistrationName { get; private set; }

        public TConcreteOptions UseRegistrationName(string registrationName)
        {
            registrationName.MustNotBeNullOrEmpty(nameof(registrationName));

            RegistrationName = registrationName;
            return _this;
        }

        public TConcreteOptions UseConstructor(ConstructorInfo constructorInfo)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));

            AssignConstructor(constructorInfo);
            return _this;
        }

        public TConcreteOptions UseDefaultConstructor()
        {
            var targetConstructor = TargetTypeInfo.DeclaredConstructors.FindDefaultConstructor();
            EnsureTargetConstructorIsNotNull(targetConstructor, null);
            AssignConstructor(targetConstructor);
            return _this;
        }

        public TConcreteOptions UseConstructorWithParameters(params Type[] parameterTypes)
        {
            parameterTypes.MustNotBeNull(nameof(parameterTypes));

            var targetConstructor = TargetTypeInfo.DeclaredConstructors.FindConstructorWithArgumentTypes(parameterTypes);
            EnsureTargetConstructorIsNotNull(targetConstructor, parameterTypes);

            AssignConstructor(targetConstructor);
            return _this;
        }

        public TConcreteOptions MapTypeToAbstractions(params Type[] abstractionTypes)
        {
            return MapTypeToAbstractions((IEnumerable<Type>) abstractionTypes);
        }

        public TConcreteOptions MapTypeToAbstractions(IEnumerable<Type> abstractionTypes)
        {
            // ReSharper disable PossibleMultipleEnumeration
            abstractionTypes.MustNotBeNull(nameof(abstractionTypes));

            foreach (var abstractionType in abstractionTypes)
            {
                if (IgnoredAbstractionTypes.Contains(abstractionType))
                    continue;

                AbstractionTypes.Add(abstractionType);
            }
            return _this;
            // ReSharper restore PossibleMultipleEnumeration
        }

        public TConcreteOptions MapTypeToAllImplementedInterfaces()
        {
            return MapTypeToAbstractions(TargetTypeInfo.ImplementedInterfaces);
        }

        public TConcreteOptions UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo)
        {
            staticFactoryMethodInfo.MustNotBeNull(nameof(staticFactoryMethodInfo));

            AssignStaticCreationMethod(staticFactoryMethodInfo);
            return _this;
        }

        public TConcreteOptions UseStaticFactoryMethod(Delegate staticMethodDelegate)
        {
            staticMethodDelegate.MustNotBeNull();

            var methodInfo = staticMethodDelegate.GetMethodInfo();
            CheckStaticCreationMethodFromDelegate(methodInfo, TargetType);
            AssignStaticCreationMethod(methodInfo);
            return _this;
        }

        public TConcreteOptions AddPropertyInjection(PropertyInfo propertyInfo, string resolvedRegistrationName = null)
        {
            propertyInfo.MustNotBeNull(nameof(propertyInfo));
            CheckPropertyInfo(propertyInfo, TargetType);

            AddInstanceInjection(new PropertyInjection(propertyInfo, resolvedRegistrationName));
            return _this;
        }

        public TConcreteOptions AddFieldInjection(FieldInfo fieldInfo, string resolvedRegistrationName = null)
        {
            fieldInfo.MustNotBeNull(nameof(fieldInfo));
            CheckFieldInfo(fieldInfo, TargetType);

            AddInstanceInjection(new FieldInjection(fieldInfo, resolvedRegistrationName));
            return _this;
        }

        public IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter(string parameterName)
        {
            AssignInstantiationMethodIfNeccessary();

            var targetParameters = InstantiationInfo?.InstantiationDependencies?.Where(p => p.TargetParameter.Name == parameterName)
                                                    .ToList();
            CheckTargetParametersWithName(targetParameters, parameterName);

            // ReSharper disable once PossibleNullReferenceException
            return new ChildRegistrationNameOptions<TConcreteOptions>(_this, targetParameters[0]);
        }

        public TConcreteOptions DisableTrackingOfDisposables()
        {
            IsContainerTrackingDisposables = false;
            return _this;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        protected void CheckTargetParametersWithName(List<ParameterDependency> targetParameters, string parameterName)
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

        [Conditional(Check.CompileAssertionsSymbol)]
        private void EnsureThisIsNotNull()
        {
            if (_this != null)
                return;

            throw new InvalidOperationException($"The class {GetType()} does not implement {typeof(TConcreteOptions)}.");
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
    }
}
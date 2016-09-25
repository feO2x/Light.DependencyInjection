using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class RegistrationOptionsForType : BaseRegistrationOptionsForTypes<IRegistrationOptionsForType>, IRegistrationOptionsForType
    {
        public RegistrationOptionsForType(Type targetType, IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes) :
            base(targetType, constructorSelector, ignoredAbstractionTypes) { }

        public static DiContainer PerformRegistration(DiContainer container,
                                                      Type targetType,
                                                      Action<IRegistrationOptionsForType> configureOptions,
                                                      ILifetime lifetime,
                                                      Type abstractionType = null)
        {
            container.MustNotBeNull(nameof(container));

            var options = new RegistrationOptionsForType(targetType, container.TypeAnalyzer.ConstructorSelector, container.TypeAnalyzer.IgnoredAbstractionTypes);
            if (abstractionType != null)
                options.MapToAbstractions(abstractionType);
            configureOptions?.Invoke(options);

            options.CreateRegistration(container, lifetime);
            return container;
        }
    }

    public sealed class RegistrationOptionsForType<T> : BaseRegistrationOptionsForTypes<IRegistrationOptionsForType<T>>, IRegistrationOptionsForType<T>
    {
        public RegistrationOptionsForType(IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes)
            : base(typeof(T), constructorSelector, ignoredAbstractionTypes) { }

        public IRegistrationOptionsForType<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null)
        {
            selectPropertyExpression.MustNotBeNull(nameof(selectPropertyExpression));

            AddInstanceInjection(new PropertyInjection(selectPropertyExpression.ExtractSettableInstancePropertyInfo(TargetType), resolvedRegistrationName));
            return this;
        }

        public IRegistrationOptionsForType<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null)
        {
            selectFieldExpression.MustNotBeNull();

            AddInstanceInjection(new FieldInjection(selectFieldExpression.ExtractSettableInstanceFieldInfo(TargetType), resolvedRegistrationName));
            return this;
        }

        public static DiContainer PerformRegistration(DiContainer container,
                                                      Action<IRegistrationOptionsForType<T>> configureOptions,
                                                      ILifetime lifetime,
                                                      Type abstractionType = null)
        {
            container.MustNotBeNull(nameof(container));
            lifetime.MustNotBeNull(nameof(lifetime));

            var options = new RegistrationOptionsForType<T>(container.TypeAnalyzer.ConstructorSelector, container.TypeAnalyzer.IgnoredAbstractionTypes);
            if (abstractionType != null)
                options.MapToAbstractions(abstractionType);
            configureOptions?.Invoke(options);

            options.CreateRegistration(container, lifetime);
            return container;
        }
    }
}
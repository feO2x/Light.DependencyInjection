using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public abstract class InstanceInjection : ISetTargetRegistrationName
    {
        public readonly Type DeclaringType;
        public readonly string DisplayName;
        public readonly string MemberName;
        public readonly Type MemberType;
        public readonly Action<object, object> StandardizedSetValueFunction;
        private IDependencyResolver _dependencyResolver = DefaultDependencyResolver.Instance;
        public string TargetRegistrationName;

        protected InstanceInjection(string memberName,
                                    Type memberType,
                                    Type declaringType,
                                    Action<object, object> standardizedSetValueFunction,
                                    string targetRegistrationName = null)
        {
            memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            memberType.MustNotBeNull(nameof(memberType));
            declaringType.MustNotBeNull(nameof(declaringType));

            MemberName = memberName;
            MemberType = memberType;
            DeclaringType = declaringType;
            StandardizedSetValueFunction = standardizedSetValueFunction;
            TargetRegistrationName = targetRegistrationName;
            DisplayName = $"{GetType().Name} {DeclaringType.Name}.{MemberName}";
        }

        public IDependencyResolver DependencyResolver
        {
            get { return _dependencyResolver; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _dependencyResolver = value;
            }
        }

        string ISetTargetRegistrationName.TargetRegistrationName
        {
            set { TargetRegistrationName = value; }
        }

        public void InjectValue(object instance, ResolveContext context)
        {
            object value;
            if (context.ParameterOverrides.HasValue)
            {
                if (context.ParameterOverrides.Value.InstanceInjectionOverrides.TryGetValue(this, out value))
                    goto SetValue;
            }
            value = _dependencyResolver.Resolve(MemberType, TargetRegistrationName, context);

            SetValue:
            StandardizedSetValueFunction(instance, value);
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public InstanceInjection BindToClosedGenericType(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            closedGenericType.MustBeClosedVariantOf(DeclaringType);

            return BindToClosedGenericTypeInternal(closedGenericType, closedGenericTypeInfo);
        }

        protected abstract InstanceInjection BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo);
    }
}
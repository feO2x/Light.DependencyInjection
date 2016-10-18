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
        public readonly IDependencyResolver DependencyResolver = DefaultDependencyResolver.Instance;
        public readonly string DisplayName;
        public readonly string MemberName;
        public readonly Type MemberType;
        public readonly Action<object, object> StandardizedSetValueFunction;
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
            standardizedSetValueFunction.MustNotBeNull(nameof(standardizedSetValueFunction));

            MemberName = memberName;
            MemberType = memberType;
            DeclaringType = declaringType;
            StandardizedSetValueFunction = standardizedSetValueFunction;
            TargetRegistrationName = targetRegistrationName;
            DisplayName = $"{GetType().Name} {DeclaringType.Name}.{MemberName}";
        }

        string ISetTargetRegistrationName.TargetRegistrationName
        {
            set { TargetRegistrationName = value; }
        }

        public void InjectValue(object instance, CreationContext context)
        {
            object value;
            if (context.ParameterOverrides.HasValue)
            {
                if (context.ParameterOverrides.Value.InstanceInjectionOverrides.TryGetValue(this, out value))
                    goto SetValue;
            }
            value = DependencyResolver.Resolve(MemberType, TargetRegistrationName, context);

            SetValue:
            StandardizedSetValueFunction(instance, value);
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public InstanceInjection CloneForClosedConstructedGenericType(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo)
        {
            closedConstructedGenericType.MustBeClosedConstructedVariantOf(DeclaringType);

            return CloneForClosedConstructedGenericTypeInternal(closedConstructedGenericType, closedConstructedGenericTypeInfo);
        }

        protected abstract InstanceInjection CloneForClosedConstructedGenericTypeInternal(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo);
    }
}
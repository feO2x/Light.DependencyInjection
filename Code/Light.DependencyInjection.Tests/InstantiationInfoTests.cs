using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Light.DependencyInjection.TypeConstruction;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class InstantiationInfoTests
    {
        private int _callCount;
        private readonly object _instanceStub = new object();

        [Fact(DisplayName = "InstantiationInfo must throw a TypeRegistrationException when a generic parameter type is passed in.")]
        public void GenericParameterTypeError()
        {
            var genericParameterType = typeof(IDictionary<,>).GetTypeInfo().GenericTypeParameters.First(); // TKey of IDictionary<TKey, TValue>

            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new InstantiationInfoStub(genericParameterType, StandardizedInstantiationFuncitonMock, null);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You cannot register type {genericParameterType} because it is a generic parameter type. Only non-abstract types that are either non-generic, closed generic or generic type definitions are allowed.");
        }

        private object StandardizedInstantiationFuncitonMock(object[] parameters)
        {
            ++_callCount;
            return _instanceStub;
        }

        public class InstantiationInfoStub : InstantiationInfo
        {
            public InstantiationInfoStub(Type targetType,
                                         Func<object[], object> standardizedInstantiationFunction,
                                         InstantiationDependency[] instantiationDependencies)
                : base(targetType, standardizedInstantiationFunction, instantiationDependencies) { }

            protected override InstantiationInfo BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo)
            {
                throw new NotImplementedException();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeConstruction;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class InstantiationInfoTests
    {
        private int _callCount;
        private readonly object _instanceStub = new object();

        [Fact(DisplayName = "InstantiationInfo must call the StandardizedInstantiationFunction to create an instance.")]
        public void CallStandardizedMethod()
        {
            var instantiationInfo = new InstantiationInfoStub(typeof(object), StandardizedInstantiationFunctionMock, null);

            var createdInstance = instantiationInfo.Instantiate(new CreationContext());

            createdInstance.Should().BeSameAs(_instanceStub);
            _callCount.Should().Be(1);
        }

        [Fact(DisplayName = "InstantiationInfo must throw a TypeRegistrationException when an interface type is passed in.")]
        public void InterfaceTypeError()
        {
            var interfaceType = typeof(IC);

            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new InstantiationInfoStub(interfaceType, StandardizedInstantiationFunctionMock, null);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You cannot register type {interfaceType} because it is an interface which cannot be instantiated. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.");
        }

        [Fact]
        public void AbstractClassError()
        {
            var abstractClassType = typeof(DefaultDependencyInjectionContainerTest);

            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new InstantiationInfoStub(abstractClassType, StandardizedInstantiationFunctionMock, null);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You cannot register type {abstractClassType} because it is an abstract class which cannot be instantiated. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.");
        }

        [Fact(DisplayName = "InstantiationInfo must throw a TypeRegistrationException when a generic parameter type is passed in.")]
        public void GenericParameterTypeError()
        {
            var genericParameterType = typeof(IDictionary<,>).GetTypeInfo().GenericTypeParameters.First(); // TKey of IDictionary<TKey, TValue>

            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new InstantiationInfoStub(genericParameterType, StandardizedInstantiationFunctionMock, null);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You cannot register type {genericParameterType} because it is a generic parameter type. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.");
        }

        [Fact(DisplayName = "InstantiationInfo must throw a TypeRegistrationException when a bound open generic type is passed in.")]
        public void BoundOpenGenericTypeError()
        {
            var dictionaryGenericTypeDefintion = typeof(Dictionary<,>);
            var boundOpenGenericType = dictionaryGenericTypeDefintion.MakeGenericType(typeof(string), dictionaryGenericTypeDefintion.GetTypeInfo().GenericTypeParameters[1]); // Dictionary<string, TValue>

            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new InstantiationInfoStub(boundOpenGenericType, StandardizedInstantiationFunctionMock, null);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You cannot register type {boundOpenGenericType} because it is a bound open generic type. Please ensure that you provide the generic type definition of this type.");
        }

        private object StandardizedInstantiationFunctionMock(object[] parameters)
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
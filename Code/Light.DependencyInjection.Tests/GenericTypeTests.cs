using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class GenericTypeTests
    {
        [Fact(DisplayName = "The DI Container must be able to perform registrations and resolves for closed bound generic types.")]
        public void ClosedBoundGenericTypes()
        {
            var container = new DiContainer().Register<IList<string>, List<string>>(options => options.UseDefaultConstructor());

            var instance = container.Resolve<IList<string>>();

            instance.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI Container must be able to perform registrations of generic type definitions and resolve bound closed instances of it.")]
        public void GenericTypeDefinitions()
        {
            var container = new DiContainer().Register(typeof(List<>), options => options.UseDefaultConstructor()
                                                                                         .MapToAbstractions(typeof(IList<>)));

            var instance = container.Resolve<IList<string>>();

            instance.Should().BeAssignableTo<List<string>>();
        }

        [Fact(DisplayName = "The DI Container must be able to instantiate closed constructed generic types of registered generic type definitions using a generic static factory method.")]
        public void StaticFactoryMethod()
        {
            var container = new DiContainer().Register(typeof(ObservableCollection<>),
                                                       options => options.InstantiateVia(CreateObservableCollectionMethod));

            var resolvedCollection = container.Resolve<ObservableCollection<string>>();

            resolvedCollection.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI Container must be able to instantiate generic type definitions using a generic static factory method when an abstraction is requested.")]
        public void StaticFactoryMethodForAbstraction()
        {
            var container = new DiContainer().Register(typeof(IList<>),
                                                       options => options.InstantiateVia(CreateObservableCollectionMethod));

            var resolvedCollection = container.Resolve<IList<object>>();

            resolvedCollection.Should().BeAssignableTo<ObservableCollection<object>>();
        }

        public static readonly MethodInfo CreateObservableCollectionMethod = typeof(GenericTypeTests).GetTypeInfo().GetDeclaredMethod(nameof(CreateObservableCollection));

        public static ObservableCollection<T> CreateObservableCollection<T>()
        {
            return new ObservableCollection<T>();
        }

        [Fact(DisplayName = "The DI Container must use a dedicated lifetime instance for each different constructed generic type of a generic type definition.")]
        public void LifetimesForGenericRegistrations()
        {
            var container = new DiContainer().Register(typeof(Dictionary<,>),
                                                       options => options.UseDefaultConstructor()
                                                                         .UseSingletonLifetime());

            var firstDictionary = container.Resolve<Dictionary<string, object>>();
            var secondDictionary = container.Resolve<Dictionary<int, string>>();

            firstDictionary.Should().NotBeNull();
            secondDictionary.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI Container must throw a RegistrationException when the single specified type is a generic type definition.")]
        public void GenericParameterInvalid()
        {
            var container = new DiContainer();
            var genericTypeParameter = typeof(List<>).GetTypeInfo().GenericTypeParameters[0];

            Action act = () => container.Register(genericTypeParameter);

            act.ShouldThrow<RegistrationException>()
               .And.Message.Should().Contain($"You cannot register the generic type parameter \"{genericTypeParameter}\" with the DI Container.");
        }
    }
}
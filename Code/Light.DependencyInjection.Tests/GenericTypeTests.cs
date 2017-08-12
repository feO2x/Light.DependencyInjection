using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using FluentAssertions;
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
                                                       options => options.InstantiateVia(typeof(GenericTypeTests).GetTypeInfo().GetDeclaredMethod(nameof(CreateObservableCollection))));

            var resolvedCollection = container.Resolve<ObservableCollection<string>>();

            resolvedCollection.Should().NotBeNull();
        }

        public static ObservableCollection<T> CreateObservableCollection<T>()
        {
            return new ObservableCollection<T>();
        }
    }
}
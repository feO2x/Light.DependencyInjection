using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class GenericTypeDefinitionTests : DefaultDependencyInjectionContainerTest
    {
        [Fact(DisplayName = "Clients must be able to register generic type definitions, where the open generics are resolved by the DI Container when a closed constructed generic type is requested.")]
        public void ResolveOpenGenerics()
        {
            Container.RegisterTransient(typeof(List<>), options => options.UseDefaultConstructor());

            var list = Container.Resolve<List<int>>();

            list.Should().NotBeNull();
        }

        [Fact(DisplayName = "Clients must be able to register generic type definitions where a static generic method is used to instantiate concrete instances.")]
        public void ResolveWithGenericStaticMethod()
        {
            Container.RegisterTransient(typeof(Dictionary<,>), options => options.UseStaticFactoryMethod(GetType().GetMethod(nameof(CreateDictionary))));

            var dictionary = Container.Resolve<Dictionary<string, object>>();

            dictionary.Should().NotBeNull();
        }

        public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>()
        {
            return new Dictionary<TKey, TValue>(42);
        }

        [Fact(DisplayName = "Clients must be able to resolve generic type definitions by using a closed variant of a mapped abstraction.")]
        public void ResolveClosedGenericAbstraction()
        {
            Container.RegisterTransient(typeof(HashSet<>), options => options.UseDefaultConstructor()
                                                                             .MapToAbstractions(typeof(ISet<>)));

            var set = Container.Resolve<ISet<string>>();

            set.Should().BeAssignableTo<HashSet<string>>();
        }
    }
}
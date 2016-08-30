using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class GenericTypeDefinitionTests : DefaultDiContainerTests
    {
        [Fact(DisplayName = "Clients must be able to register unbound generic types, where the unbound generics are resolved by the DI Container when a bound generic type is requested.")]
        public void ResolveOpenGenerics()
        {
            Container.RegisterTransient(typeof(List<>), options => options.UseDefaultConstructor());

            var list = Container.Resolve<List<int>>();

            list.Should().NotBeNull();
        }

        [Fact(DisplayName = "Clients must be able to register an unbound generic type where a static generic method is used to instantiate concrete instances.")]
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
    }
}
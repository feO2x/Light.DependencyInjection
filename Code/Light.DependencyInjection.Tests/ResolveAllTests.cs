using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveAllTests : DefaultDependencyInjectionContainerTest
    {
        private static readonly IEnumerable<Type> AllDummyTypes = new[] { typeof(DummyA), typeof(DummyB), typeof(DummyC) };

        public ResolveAllTests()
        {
            foreach (var dummyType in AllDummyTypes)
            {
                Container.RegisterTransient(dummyType, options => options.UseTypeNameAsRegistrationName()
                                                                         .MapToAllImplementedInterfaces());
            }
        }

        [Fact(DisplayName = "The DI Container must be able to resolve all registrations of a certain abstraction type that were registered with a name.")]
        public void ResolveAll()
        {
            var allDummies = Container.ResolveAll<IDummyInterface>();

            allDummies.Select(dummy => dummy.GetType()).Should().BeEquivalentTo(AllDummyTypes);
        }

        [Fact(DisplayName = "Clients must be able to override a registration type before calling ResolveAll.")]
        public void ResolveAllWithOverrideMapping()
        {
            var dummyA = new DummyA();
            Container.OverrideMapping(dummyA, nameof(DummyA));

            var allDummies = Container.ResolveAll<IDummyInterface>();

            allDummies.First(o => o is DummyA).Should().BeSameAs(dummyA);
        }

        [Fact(DisplayName = "Clients must be able to override a registration type before calling the non-generic variant of ResolveAll.")]
        public void ResolveAllNonGenericWithOverrideMapping()
        {
            var dummyA = new DummyA();
            Container.OverrideMapping(dummyA, nameof(DummyA));

            var allDummies = Container.ResolveAll(typeof(IDummyInterface));

            allDummies.First(o => o is DummyA).Should().BeSameAs(dummyA);
        }

        public interface IDummyInterface { }

        public class DummyA : IDummyInterface { }

        public class DummyB : IDummyInterface { }

        public class DummyC : IDummyInterface { }
    }
}
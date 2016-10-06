using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveAllTests : DefaultDiContainerTest
    {
        [Fact(DisplayName = "The DI Container must be able to resolve all registrations of a certain abstraction type that were registered with a name.")]
        public void ResolveAll()
        {
            IEnumerable<Type> allDummyTypes = new[] { typeof(DummyA), typeof(DummyB), typeof(DummyC) };
            foreach (var dummyType in allDummyTypes)
            {
                Container.RegisterTransient(dummyType, options => options.UseTypeNameAsRegistrationName()
                                                                         .MapToAllImplementedInterfaces());
            }

            var allDummies = Container.ResolveAll<IDummyInterface>();

            allDummies.Select(dummy => dummy.GetType()).Should().BeEquivalentTo(allDummyTypes);
        }

        public interface IDummyInterface { }

        public class DummyA : IDummyInterface { }

        public class DummyB : IDummyInterface { }

        public class DummyC : IDummyInterface { }
    }
}
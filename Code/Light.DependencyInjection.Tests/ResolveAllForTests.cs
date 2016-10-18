using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveAllForTests : DefaultDiContainerTest
    {
        [Fact(DisplayName = "The DI container must be able to inject all instances of an interface in properties.")]
        public void ResolveAllForProperty()
        {
            IEnumerable<Type> types = new[] { typeof(A), typeof(B), typeof(C) };

            foreach (var type in types)
            {
                Container.RegisterTransient(type, options => options.UseTypeNameAsRegistrationName()
                                                                    .MapToAllImplementedInterfaces());
            }

            Container.RegisterTransient<Client>(options => options.AddPropertyInjection(o => o.Foos)
                                                                  .ResolveAllForProperty<IEnumerable<IFoo>, IFoo>(o => o.Foos));

            var client = Container.Resolve<Client>();

            client.Foos.Should().HaveCount(3);
            client.Foos.Select(o => o.GetType()).Should().BeEquivalentTo(types);
        }

        public interface IFoo { }

        public class A : IFoo { }

        public class B : IFoo { }

        public class C : IFoo { }

        public class Client
        {
            public IEnumerable<IFoo> Foos { get; set; }
        }
    }
}
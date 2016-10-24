using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveAllForTests : DefaultDiContainerTest
    {
        private static readonly IEnumerable<Type> Types = new[] { typeof(A), typeof(B), typeof(C) };

        public ResolveAllForTests()
        {
            foreach (var type in Types)
            {
                Container.RegisterTransient(type, options => options.UseTypeNameAsRegistrationName()
                                                                    .MapToAllImplementedInterfaces());
            }
        }

        [Fact(DisplayName = "The DI container must be able to inject all instances of an insterface in properties.")]
        public void ResolveAllForConstructorParameter()
        {
            Container.RegisterTransient<ConstructorClient>(options => options.ResolveAllForInstantiationParameter<IReadOnlyList<IFoo>>());

            var client = Container.Resolve<ConstructorClient>();

            ValidateInjectedValues(client.Foos);
        }

        [Fact(DisplayName = "The DI container must be able to inject all instances of an interface in properties.")]
        public void ResolveAllForProperty()
        {
            Container.RegisterTransient<PropertyClient>(options => options.AddPropertyInjection(o => o.Foos)
                                                                          .ResolveAllForProperty(o => o.Foos));

            var client = Container.Resolve<PropertyClient>();

            ValidateInjectedValues(client.Foos);
        }

        [Fact(DisplayName = "The DI container must be able to inject all instances of an interface in fields.")]
        public void ResolveAllForField()
        {
            Container.RegisterTransient<FieldClient>(options => options.AddFieldInjection(o => o.Foos)
                                                                       .ResolveAllForField(o => o.Foos));

            var client = Container.Resolve<FieldClient>();

            ValidateInjectedValues(client.Foos);
        }

        [Fact(DisplayName = "The DI container must be able to inject all instances of an interface in a particular constructor argument (constructor with two equal types).")]
        public void ResolveAllForDoubleConstructorParameter()
        {
            Container.RegisterTransient<DoubleConstructorClient>(options => options.ResolveAllForInstantiationParameter("foos1"))
                     .RegisterTransient(typeof(List<>), options => options.MapToAllImplementedInterfaces()
                                                                          .UseDefaultConstructor());

            var client = Container.Resolve<DoubleConstructorClient>();

            client.Foos2.Should().BeEmpty();
            ValidateInjectedValues(client.Foos1);
        }

        private static void ValidateInjectedValues(IEnumerable<IFoo> collection)
        {
            // ReSharper disable PossibleMultipleEnumeration
            collection.Should().HaveCount(3);
            collection.Select(foo => foo.GetType()).Should().BeEquivalentTo(Types);
            // ReSharper restore PossibleMultipleEnumeration
        }

        public interface IFoo { }

        public class A : IFoo { }

        public class B : IFoo { }

        public class C : IFoo { }

        public class ConstructorClient
        {
            public readonly IReadOnlyList<IFoo> Foos;

            public ConstructorClient(IReadOnlyList<IFoo> foos)
            {
                Foos = foos;
            }
        }

        public class PropertyClient
        {
            public IEnumerable<IFoo> Foos { get; set; }
        }

        public class FieldClient
        {
            public IEnumerable<IFoo> Foos;
        }

        public class DoubleConstructorClient
        {
            public readonly IReadOnlyList<IFoo> Foos1;
            public readonly IReadOnlyList<IFoo> Foos2;

            public DoubleConstructorClient(IReadOnlyList<IFoo> foos1, IReadOnlyList<IFoo> foos2)
            {
                Foos1 = foos1;
                Foos2 = foos2;
            }
        }
    }
}
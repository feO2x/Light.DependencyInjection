using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ReplaceMappingsTests : DefaultDependencyInjectionContainerTest
    {
        [Fact(DisplayName = "Clients must be able to replace existing registrations with one that registers an external instance.")]
        public void ReplaceRegistrationWithInstance()
        {
            Container.RegisterSingleton<ImplementationA>(options => options.MapToAllImplementedInterfaces());

            var implementationB = new ImplementationB();
            var resolvedInstance = Container.RegisterInstance(implementationB, options => options.MapToAllImplementedInterfaces())
                                            .Resolve<IAbstraction>();

            resolvedInstance.Should().BeSameAs(implementationB);
        }

        public interface IAbstraction { }

        public sealed class ImplementationA : IAbstraction { }

        public sealed class ImplementationB : IAbstraction { }
    }
}
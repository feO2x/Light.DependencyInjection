using System;
using FluentAssertions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class RegistrationTests
    {
        [Fact(DisplayName = "Registration must be created with an ExternalInstanceLifetime without specifying a TypeCreationInfo.")]
        public void ExternalLifetimeNoCreationInfo()
        {
            var instance = new A();

            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new Registration(instance.GetType(), new ExternalInstanceLifetime(instance), null);

            act.ShouldNotThrow();
        }

        [Fact(DisplayName = "Registration must throw a TypeRegistrationException when it is created with no TypeCreationInfo, but the lifetime requires one.")]
        public void NonExternalLifetimeWithoutCreationInfo()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new Registration(typeof(A), TransientLifetime.Instance, null);

            act.ShouldThrow<ArgumentNullException>()
               .And.Message.Should().Contain($"The type creation info must not be null because the {TransientLifetime.Instance} requires it to be present.");
        }
    }
}
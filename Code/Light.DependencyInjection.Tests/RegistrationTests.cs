﻿using System;
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
            Action act = () => new Registration(instance.GetType(), new ExternalInstanceLifetime(instance));

            act.ShouldNotThrow();
        }

        [Fact(DisplayName = "Registration must throw a TypeRegistrationException when it is created with no TypeCreationInfo, but the lifetime requires one.")]
        public void NonExternalLifetimeWithoutCreationInfo()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new Registration(typeof(A), TransientLifetime.Instance);

            act.ShouldThrow<ArgumentException>()
               .And.Message.Should().Contain($"You cannot call this constructor with the {TransientLifetime.Instance} because it requires a TypeCreationInfo. Use the other constructor instead.");
        }
    }
}
﻿using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class BasicResolveTests
    {
        [Fact(DisplayName = "The DI Container must create two instances when a type with a transient lifetime is resolved two times.")]
        public void TransientResolve()
        {
            var container = new DependencyInjectionContainer().Register<ClassWithoutDependencies>();

            var first = container.Resolve<ClassWithoutDependencies>();
            var second = container.Resolve<ClassWithoutDependencies>();

            first.Should().NotBeNull();
            second.Should().NotBeNull();
            first.Should().NotBeSameAs(second);
        }

        [Fact(DisplayName = "The DI Container must resolve the same instance when the target type is registered with a singleton lifetime.")]
        public void SingletonResolve()
        {
            var container = new DependencyInjectionContainer().Register<ClassWithoutDependencies>(options => options.UseSingletonLifetime());

            var first = container.Resolve<ClassWithoutDependencies>();
            var second = container.Resolve<ClassWithoutDependencies>();

            first.Should().NotBeNull();
            first.Should().BeSameAs(second);
        }

        [Theory(DisplayName = "The DI Container must resolve the reference to an instance that was passed to it on registration.")]
        [MemberData(nameof(ExternalInstanceData))]
        public void ExternalInstanceResolve<T>(T instance)
        {
            var container = new DependencyInjectionContainer().Register(instance);

            var resolvedInstance = container.Resolve<T>();

            resolvedInstance.Should().BeSameAs(instance);
        }

        public static readonly TestData ExternalInstanceData =
            new[]
            {
                new object[] { "Foo" },
                new[] { new object() }
            };

        [Fact(DisplayName = "The DI container must be able to resolve types with dependencies to other types.")]
        public void SimpleHierarchicalResolve()
        {
            var container = new DependencyInjectionContainer().Register<ClassWithoutDependencies>()
                                                              .Register<ClassWithDependency>();

            var instance = container.Resolve<ClassWithDependency>();

            instance.Should().NotBeNull();
            instance.A.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI container must be able to resolve a complex object graph where a singleton instance is injected in several other objects.")]
        public void TwoLevelHierarchicalResolveWithSingletonLeaf()
        {
            var container = new DependencyInjectionContainer().Register<ClassWithoutDependencies>(options => options.UseSingletonLifetime())
                                                              .Register<ClassWithDependency>()
                                                              .Register<ClassWithTwoDependencies>(options => options.UseSingletonLifetime());

            var instance = container.Resolve<ClassWithTwoDependencies>();

            instance.A.Should().BeSameAs(instance.B.A);
        }

        [Fact(DisplayName = "The DI container must be able to resolve a concrete type for an interface when this mapping was registered beforehand.")]
        public void InterfaceMapping()
        {
            var instance = new DependencyInjectionContainer().Register<IAbstractionA, ClassWithoutDependencies>()
                                                             .Resolve<IAbstractionA>();

            instance.Should().BeOfType<ClassWithoutDependencies>();
        }

        [Theory(DisplayName = "The DI Container must throw a TypeRegistrationException when the registered type does not derive from or implement the type that was registered.")]
        [InlineData(typeof(IComparable))]
        [InlineData(typeof(Random))]
        public void InvalidAbstractionType(Type invalidAbstractionType)
        {
            var container = new DependencyInjectionContainer();
            var targetType = typeof(ClassWithoutDependencies);
            Action act = () => container.Register(invalidAbstractionType, targetType);

            act.ShouldThrow<RegistrationException>()
               .And.Message.Should().Be($"Type \"{invalidAbstractionType}\" cannot be used as an abstraction for type \"{targetType}\" because the latter type does not derive from or implement the former one.");
        }

        [Fact(DisplayName = "The DI Container must implement IDisposable.")]
        public void Disposable()
        {
            typeof(DependencyInjectionContainer).Should().Implement<IDisposable>();
        }

        [Fact(DisplayName = "The DI Container must dispose of disposable instances by default when it is disposed itself.")]
        public void DisposeDisposableObjectsByDefault()
        {
            var container = new DependencyInjectionContainer().Register<DisposableSpy>();
            var disposableInstance = container.Resolve<DisposableSpy>();

            container.Dispose();

            disposableInstance.DisposeMustHaveBeenCalledExactlyOnce();
        }

        [Fact(DisplayName = "The DI Container must be able to automatically resolve GUIDs.")]
        public void AutomaticGuidResolving()
        {
            new DependencyInjectionContainer().Resolve<Guid>().Should().NotBeEmpty();
        }

        [Fact(DisplayName = "The DI Container must be able to automatically resolve itself.")]
        public void AutomaticContainerResolving()
        {
            var container = new DependencyInjectionContainer().Register<ServiceLocatorClient>();

            var serviceLocatorClient = container.Resolve<ServiceLocatorClient>();

            serviceLocatorClient.Container.Should().BeSameAs(container);
        }

        [Fact(DisplayName = "The DI Container must be able to perform Property Injection when this is configured for the target type.")]
        public void PropertyInjection()
        {
            var container = new DependencyInjectionContainer().Register<ClassWithoutDependencies>()
                                                              .Register<ClassWithProperty>(options => options.AddPropertyInjection(nameof(ClassWithProperty.InstanceWithoutDependencies)));

            var instanceWithProperty = container.Resolve<ClassWithProperty>();

            instanceWithProperty.InstanceWithoutDependencies.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI Container must be able to resolve a polymorphic graph where a child object performs property injection.")]
        public void ComplexGraphWithPropertyInjectionAndPolymorphism()
        {
            var container = new DependencyInjectionContainer().Register<ClassWithoutDependencies>()
                                                              .Register<IEmptyInterface, ClassWithProperty>(options => options.AddPropertyInjection(nameof(ClassWithProperty.InstanceWithoutDependencies)))
                                                              .Register<ClassWithPropertyInjectionDependency>();

            var instance = container.Resolve<ClassWithPropertyInjectionDependency>();

            instance.Should().NotBeNull();
            instance.InstanceWithProperty.Should().NotBeNull();
            instance.InstanceWithProperty.MustBeOfType<ClassWithProperty>().InstanceWithoutDependencies.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI Container must be able to perform Field Injection when this is configured for the target type.")]
        public void FieldInjection()
        {
            var container = new DependencyInjectionContainer().Register(true, options => options.UseRegistrationName("The Boolean"))
                                                              .Register<ClassWithPublicField>(options => options.AddFieldInjection(nameof(ClassWithPublicField.PublicField), "The Boolean"));

            var instanceWithPublicField = container.Resolve<ClassWithPublicField>();

            instanceWithPublicField.PublicField.Should().BeTrue();
        }

        [Theory(DisplayName = "The DI Container must be able to perform automatic registration when the specified type can be instantiated using the default registration settings.")]
        [InlineData(typeof(ClassWithoutDependencies))]
        [InlineData(typeof(ClassWithDependency))]
        public void AutomaticRegistration(Type targetType)
        {
            var instance = new DependencyInjectionContainer().Resolve(targetType);

            instance.Should().NotBeNull().And.BeAssignableTo(targetType);
        }

        [Fact(DisplayName = "The DI Container must be able to inject the same instance during a single Resolve call when the PerResolveLifetime is used.")]
        public void PerResolveInstance()
        {
            var container = new DependencyInjectionContainer().Register<ClassWithoutDependencies>(options => options.UsePerResolveLifetime())
                                                              .Register<ClassWithDependency>()
                                                              .Register<ClassWithTwoDependencies>();

            var objectGraph1 = container.Resolve<ClassWithTwoDependencies>();
            var objectGraph2 = container.Resolve<ClassWithTwoDependencies>();

            objectGraph1.A.Should().BeSameAs(objectGraph1.B.A);
            objectGraph2.A.Should().BeSameAs(objectGraph2.B.A);
            objectGraph1.A.Should().NotBeSameAs(objectGraph2.A);
        }

        [Fact(DisplayName = "The DI Container must be able to resolve instances per thread when they are configured with the PerThreadLifetime.")]
        public void PerThreadInstance()
        {
            var container = new DependencyInjectionContainer().Register<ThreadSaveClass>(options => options.UsePerThreadLifetime());
            var exceptions = new ConcurrentBag<Exception>();

            void ResolvePerThreadInstances()
            {
                var firstInstance = container.Resolve<ThreadSaveClass>();
                var secondInstance = container.Resolve<ThreadSaveClass>();

                try
                {
                    firstInstance.Should().BeSameAs(secondInstance);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            var thread1 = new Thread(ResolvePerThreadInstances);
            var thread2 = new Thread(ResolvePerThreadInstances);
            var thread3 = new Thread(ResolvePerThreadInstances);

            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread1.Join();
            thread2.Join();
            thread3.Join();

            ThreadSaveClass.NumberOfInstancesCreated.Should().Be(3);
            exceptions.Should().BeEmpty();
        }

        [Fact(DisplayName = "The DI Container must throw a RegistrationException when the specified type is a static class.")]
        public void StaticClassInvalid()
        {
            var container = new DependencyInjectionContainer();
            var staticClassType = typeof(Console);

            Action act = () => container.Register(staticClassType);

            act.ShouldThrow<RegistrationException>()
               .And.Message.Should().Contain($"You cannot register the static class \"{staticClassType}\" with the DI Container.");
        }

        [Fact(DisplayName = "The DI Container must be able to resolve types via a static method.")]
        public void ResolveViaMethodInfo()
        {
            var container = new DependencyInjectionContainer().Register<IAbstractionA, ClassWithoutDependencies>(options => options.UseSingletonLifetime())
                                                              .Register<ClassWithDependency>(options => options.InstantiateVia(GetType().GetMethod(nameof(CreateClassWithDependency))));

            var instance = container.Resolve<ClassWithDependency>();

            instance.A.Should().BeSameAs(container.Resolve<ClassWithoutDependencies>());
        }

        public static ClassWithDependency CreateClassWithDependency(ClassWithoutDependencies instance)
        {
            return new ClassWithDependency(instance);
        }

        [Fact(DisplayName = "The client must be able to specify a property injection via a LINQ expression.")]
        public void PropertyInjectionViaLinqExpression()
        {
            var container = new DependencyInjectionContainer().Register<ClassWithPropertyInjectionDependency>()
                                                              .Register<ClassWithProperty>(options => options.AddPropertyInjection(o => o.InstanceWithoutDependencies)
                                                                                                             .MapToAbstractions(typeof(IEmptyInterface)))
                                                              .Register<ClassWithoutDependencies>();

            var resolvedInstance = container.Resolve<ClassWithPropertyInjectionDependency>();

            ((ClassWithProperty) resolvedInstance.InstanceWithProperty).InstanceWithoutDependencies.Should().NotBeNull();
        }

        [Fact(DisplayName = "The client must be able to specify a field injection via a LINQ expression.")]
        public void FieldInjectionViaLinqExpression()
        {
            const string boolDependencyName = "My Boolean";
            var container = new DependencyInjectionContainer().Register<ClassWithPublicField>(options => options.AddFieldInjection(o => o.PublicField, boolDependencyName))
                                                              .Register(true, options => options.UseRegistrationName(boolDependencyName));

            var instance = container.Resolve<ClassWithPublicField>();

            instance.PublicField.Should().BeTrue();
        }
    }
}
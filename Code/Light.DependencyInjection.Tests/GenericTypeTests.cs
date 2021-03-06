﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class GenericTypeTests
    {
        [Fact(DisplayName = "The DI Container must be able to perform registrations and resolves for closed bound generic types.")]
        public void ClosedBoundGenericTypes()
        {
            var container = new DependencyInjectionContainer().Register<IList<string>, List<string>>(options => options.UseDefaultConstructor());

            var instance = container.Resolve<IList<string>>();

            instance.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI Container must be able to perform registrations of generic type definitions and resolve bound closed instances of it.")]
        public void GenericTypeDefinitions()
        {
            var container = new DependencyInjectionContainer().Register(typeof(List<>), options => options.UseDefaultConstructor()
                                                                                                          .MapToAbstractions(typeof(IList<>)));

            var instance = container.Resolve<IList<string>>();

            instance.Should().BeAssignableTo<List<string>>();
        }

        [Fact(DisplayName = "The DI Container must be able to instantiate closed constructed generic types of registered generic type definitions using a generic static factory method.")]
        public void StaticFactoryMethod()
        {
            var container = new DependencyInjectionContainer().Register(typeof(ObservableCollection<>),
                                                                        options => options.InstantiateVia(CreateObservableCollectionMethod));

            var resolvedCollection = container.Resolve<ObservableCollection<string>>();

            resolvedCollection.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI Container must be able to instantiate generic type definitions using a generic static factory method when an abstraction is requested.")]
        public void StaticFactoryMethodForAbstraction()
        {
            var container = new DependencyInjectionContainer().Register(typeof(IList<>),
                                                                        options => options.InstantiateVia(CreateObservableCollectionMethod));

            var resolvedCollection = container.Resolve<IList<object>>();

            resolvedCollection.Should().BeAssignableTo<ObservableCollection<object>>();
        }

        public static readonly MethodInfo CreateObservableCollectionMethod = typeof(GenericTypeTests).GetTypeInfo().GetDeclaredMethod(nameof(CreateObservableCollection));

        public static ObservableCollection<T> CreateObservableCollection<T>()
        {
            return new ObservableCollection<T>();
        }

        [Fact(DisplayName = "The DI Container must be able to instantiate generic type definitions using a static factory method residing in a generic type.")]
        public void StaticFactoryMethodInGenericType()
        {
            var container = new DependencyInjectionContainer().Register(typeof(IReadOnlyList<>),
                                                                        options => options.InstantiateVia(typeof(GenericFactory<>).GetMethod("Create")));

            var resolvedCollection = container.Resolve<IReadOnlyList<string>>();

            resolvedCollection.Should().BeAssignableTo<List<string>>();
        }

        public static class GenericFactory<T>
        {
            // ReSharper disable UnusedMember.Global
            public static List<T> Create()
            {
                return new List<T>();
            }

            public static List<object> InvalidCreate()
            {
                return new List<object>();
            }
            // ReSharper restore UnusedMember.Global
        }

        [Fact(DisplayName = "The DI Container must throw a RegistrationException when the specified method info in a Generic Type Definition is not returning an open constructed generic type that resides in the same inheritance hierarchy as the registration type.")]
        public void InvalidStaticFactoryMethodInGenericType()
        {
            var container = new DependencyInjectionContainer();
            var invalidStaticFactoryMethod = typeof(GenericFactory<>).GetMethod("InvalidCreate");

            Action act = () => container.Register(typeof(IList<>),
                                                  options => options.InstantiateVia(invalidStaticFactoryMethod));

            act.ShouldThrow<RegistrationException>()
               .And.Message.Should().Contain($"You cannot instantiate type \"{typeof(IList<>)}\" with the static factory method \"{invalidStaticFactoryMethod}\".");
        }

        [Fact(DisplayName = "The DI Container must use a dedicated lifetime instance for each different constructed generic type of a generic type definition.")]
        public void LifetimesForGenericRegistrations()
        {
            var container = new DependencyInjectionContainer().Register(typeof(Dictionary<,>),
                                                                        options => options.UseDefaultConstructor()
                                                                                          .UseSingletonLifetime());

            var firstDictionary = container.Resolve<Dictionary<string, object>>();
            var secondDictionary = container.Resolve<Dictionary<int, string>>();

            firstDictionary.Should().NotBeNull();
            secondDictionary.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI Container must throw a RegistrationException when the specified type is a generic type definition.")]
        public void GenericParameterInvalid()
        {
            var container = new DependencyInjectionContainer();
            var genericTypeParameter = typeof(List<>).GetTypeInfo().GenericTypeParameters[0];

            Action act = () => container.Register(genericTypeParameter);

            act.ShouldThrow<RegistrationException>()
               .And.Message.Should().Contain($"You cannot register the generic type parameter \"{genericTypeParameter}\" with the DI Container.");
        }

        [Fact(DisplayName = "The DI Container must throw a RegistrationException when specified type is an open constructed generic type.")]
        public void OpenConstructedGenericTypeInvalid()
        {
            var container = new DependencyInjectionContainer();
            var openConstructedGenericType = CreateObservableCollectionMethod.ReturnType;

            Action act = () => container.Register(openConstructedGenericType);

            act.ShouldThrow<RegistrationException>()
               .And.Message.Should().Contain($"You cannot register the open constructed generic type \"{openConstructedGenericType}\" with the DI Container");
        }

        [Fact(DisplayName = "The DI Container must be able to inject values into generic properties of generic types.")]
        public void GenericTypeWithGenericProperty()
        {
            var @object = new object();
            var container = new DependencyInjectionContainer().Register(typeof(GenericClassWithGenericProperty<>),
                                                                        options => options.AddPropertyInjection("Property"))
                                                              .Register(@object);

            var resolvedInstance = container.Resolve<GenericClassWithGenericProperty<object>>();

            resolvedInstance.Property.Should().BeSameAs(@object);
        }

        [Fact(DisplayName = "The DI Container must be able to inject values into generic fields of generic types.")]
        public void GenericTypeWithGenericField()
        {
            const string foo = "Foo";
            var container = new DependencyInjectionContainer().Register(typeof(GenericClassWithGenericField<>),
                                                                        options => options.AddFieldInjection("Field"))
                                                              .Register(foo);

            var resolvedInstace = container.Resolve<GenericClassWithGenericField<string>>();

            resolvedInstace.Field.Should().BeSameAs(foo);
        }

        [Fact(DisplayName = "The DI Container must be able to resolve generic types that have a dependency to another generic type.")]
        public void MultiLevelGenericTypes()
        {
            var containerServices = new ContainerServicesBuilder().DisallowAutomaticRegistrations()
                                                                  .Build();
            var container = new DependencyInjectionContainer(containerServices).Register(typeof(ObservableCollection<>),
                                                                                         options => options.UseDefaultConstructor()
                                                                                                           .MapToAbstractions(typeof(IList<>)))
                                                                               .Register(typeof(GenericTypeWithDependencyToOtherGenericType<>));

            var instance = container.Resolve<GenericTypeWithDependencyToOtherGenericType<int>>();

            instance.Collection.Should().BeOfType<ObservableCollection<int>>();
        }

        [Fact(DisplayName = "The DI Container must be able to resolve a generic type definition with scoped lifetime several times using different constructed generic types.")]
        public void MultipleResolvesWithDifferentConstructedTypes()
        {
            var containerServices = new ContainerServicesBuilder().DisallowAutomaticRegistrations()
                                                                  .Build();
            var container = new DependencyInjectionContainer(containerServices).Register(typeof(ObservableCollection<>),
                                                                                         options => options.UseDefaultConstructor()
                                                                                                           .UseScopedLifetime()
                                                                                                           .MapToAbstractions(typeof(IList<>)))
                                                                               .Register(typeof(GenericTypeWithDependencyToOtherGenericType<>));

            using (var childContainer = container.CreateChildContainer())
            {
                var instance1 = childContainer.Resolve<GenericTypeWithDependencyToOtherGenericType<string>>();
                instance1.Collection.Should().BeOfType<ObservableCollection<string>>();

                var instance2 = childContainer.Resolve<GenericTypeWithDependencyToOtherGenericType<int>>();
                instance2.Collection.Should().BeOfType<ObservableCollection<int>>();
            }
        }

        public class GenericTypeWithDependencyToOtherGenericType<T>
        {
            public readonly IList<T> Collection;

            public GenericTypeWithDependencyToOtherGenericType(IList<T> collection)
            {
                Collection = collection;
            }
        }
    }
}
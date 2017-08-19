﻿using System;
using System.Threading;
using FluentAssertions;

namespace Light.DependencyInjection.Tests
{
    public class ClassWithoutDependencies : IAbstractionA { }

    public class ClassWithDependency
    {
        public readonly ClassWithoutDependencies A;

        public ClassWithDependency(ClassWithoutDependencies a)
        {
            A = a;
        }
    }

    public class ClassWithTwoDependencies
    {
        public readonly ClassWithoutDependencies A;
        public readonly ClassWithDependency B;

        public ClassWithTwoDependencies(ClassWithoutDependencies a, ClassWithDependency b)
        {
            A = a;
            B = b;
        }
    }

    public interface IAbstractionA { }

    public class DisposableSpy : IDisposable
    {
        private int _disposeCallCount;

        public int DisposeCallCount => _disposeCallCount;

        public void Dispose()
        {
            _disposeCallCount++;
        }

        public void DisposeMustHaveBeenCalledExactlyOnce()
        {
            _disposeCallCount.Should().Be(1);
        }
    }

    public class ServiceLocatorClient
    {
        public readonly DiContainer Container;

        public ServiceLocatorClient(DiContainer container)
        {
            Container = container;
        }
    }

    public class ClassWithProperty : IEmptyInterface
    {
        public ClassWithoutDependencies InstanceWithoutDependencies { get; set; }
    }

    public interface IEmptyInterface { }

    public class ClassWithPropertyInjectionDependency
    {
        public readonly IEmptyInterface InstanceWithProperty;

        public ClassWithPropertyInjectionDependency(IEmptyInterface instanceWithProperty)
        {
            InstanceWithProperty = instanceWithProperty;
        }
    }

    public class ClassWithPublicField
    {
        public bool PublicField;
    }

    public class ThreadSaveClass
    {
        private static int _numberOfInstancesCreated;

        public static int NumberOfInstancesCreated => _numberOfInstancesCreated;
        
        public ThreadSaveClass()
        {
            Interlocked.Increment(ref _numberOfInstancesCreated);
        }
    }

    public class GenericClassWithGenericProperty<T>
    {
        public T Property { get; set; }
    }

    public class GenericClassWithGenericField<T>
    {
        public T Field;
    }

    public class Implementation1 : IAbstractionA { }
    public class Implementation2 : IAbstractionA { }
    public class Implementation3 : IAbstractionA { }
}
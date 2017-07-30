using System;
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

    public class ClassWithProperty
    {
        public ClassWithoutDependencies InstanceWithoutDependencies { get; set; }
    }
}
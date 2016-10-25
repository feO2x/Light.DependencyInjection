using System;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class DisposableTests : DefaultDependencyInjectionContainerTest
    {
        [Fact(DisplayName = "The DI container must dispose of tracked objects when it is disposed.")]
        public void DisposeTrackedObjects()
        {
            var instanceOfA = Container.Resolve<A>();

            Container.Dispose();

            instanceOfA.Disposable.ShouldHaveBeenCalledExactlyOnce();
            instanceOfA.ReferenceToB.Disposable.DisposeCallCount.Should().Be(1);
        }

        [Fact(DisplayName = "The DI container must only call Dispose once on a shared instance.")]
        public void EnsureSingleDispose()
        {
            var instanceOfA = Container.RegisterPerResolve<DisposableMock>()
                                       .Resolve<A>();

            Container.Dispose();

            instanceOfA.Disposable.ShouldHaveBeenCalledExactlyOnce();
        }

        public class A
        {
            public readonly DisposableMock Disposable;
            public readonly B ReferenceToB;

            public A(B referenceToB, DisposableMock disposable)
            {
                ReferenceToB = referenceToB;
                Disposable = disposable;
            }
        }

        public class B
        {
            public readonly DisposableMock Disposable;

            public B(DisposableMock disposable)
            {
                Disposable = disposable;
            }
        }
    }

    public sealed class DisposableMock : IDisposable
    {
        private int _disposeCallCount;

        public int DisposeCallCount => _disposeCallCount;

        public void Dispose()
        {
            ++_disposeCallCount;
        }

        public void ShouldHaveBeenCalledExactlyOnce()
        {
            _disposeCallCount.Should().Be(1);
        }

        public void ShouldNotHaveBeenCalled()
        {
            _disposeCallCount.Should().Be(0);
        }
    }
}
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ChildContainerTests : DefaultDependencyInjectionContainerTest
    {
        [Fact(DisplayName = "A child container must return the scoped objects of the parent container when possible.")]
        public void AccessParentSingletons()
        {
            var scopedAFromParent = Container.RegisterScoped<A>()
                                             .Resolve<A>();
            var childContainer = Container.CreateChildContainer();

            var scopedAFromChild = childContainer.Resolve<A>();

            scopedAFromChild.Should().BeSameAs(scopedAFromParent);
        }

        [Fact(DisplayName = "A child container must only dispose of objects that are tracked by its own scope , not the ones of the parent scope.")]
        public void DisposeOnlyOwnScope()
        {
            const string parentDisposableName = "Parent Disposable";
            const string childDisposableName = "Child Disposable";
            Container.RegisterScoped<DisposableMock>(options => options.UseRegistrationName(parentDisposableName))
                     .InstantiateAllScopedObjects();
            var childContainer = Container.CreateChildContainer()
                                          .RegisterScoped<DisposableMock>(options => options.UseRegistrationName(childDisposableName));
            var childDisposable = childContainer.Resolve<DisposableMock>(childDisposableName);
            var parentDisposable = childContainer.Resolve<DisposableMock>(parentDisposableName);

            childContainer.Dispose();

            childDisposable.ShouldHaveBeenCalledExactlyOnce();
            parentDisposable.ShouldNotHaveBeenCalled();
        }
    }
}
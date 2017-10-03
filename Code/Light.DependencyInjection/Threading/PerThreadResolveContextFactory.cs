using System;
using System.Threading;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Threading
{
    public sealed class PerThreadResolveContextFactory : IResolveContextFactory, IDisposable
    {
        private readonly ThreadLocal<ResolveContext> _perThreadResolveContext = new ThreadLocal<ResolveContext>();

        public void Dispose()
        {
            _perThreadResolveContext.Dispose();
        }

        public ResolveContext Create(DependencyInjectionContainer container, DependencyOverrides dependencyOverrides = null)
        {
            if (_perThreadResolveContext.IsValueCreated == false)
                return _perThreadResolveContext.Value = new ResolveContext(container, dependencyOverrides);

            return _perThreadResolveContext.Value.ChangeInitialContext(container, dependencyOverrides);
        }
    }
}
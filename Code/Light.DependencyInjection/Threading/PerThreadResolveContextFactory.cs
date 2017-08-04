﻿using System.Threading;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Threading
{
    public sealed class PerThreadResolveContextFactory : IResolveContextFactory
    {
        private readonly ThreadLocal<ResolveContext> _perThreadResolveContext = new ThreadLocal<ResolveContext>();

        public ResolveContext Create(DiContainer container)
        {
            if (_perThreadResolveContext.IsValueCreated == false)
                return _perThreadResolveContext.Value = new ResolveContext(container);

            var existingContext = _perThreadResolveContext.Value;
            if (ReferenceEquals(existingContext.Container, container) == false)
                existingContext.ChangeContainer(container);

            return existingContext.Clear();
        }
    }
}
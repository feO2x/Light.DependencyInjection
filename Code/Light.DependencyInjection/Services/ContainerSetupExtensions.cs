using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public static class ContainerSetupExtensions
    {
        public static DependencyInjectionContainer AddDefaultGuidRegistration(this DependencyInjectionContainer container)
        {
            return container.MustNotBeNull(nameof(container))
                            .Register<Guid>(options => options.InstantiateVia(Guid.NewGuid));
        }

        public static DependencyInjectionContainer AddDefaultContainerRegistration(this DependencyInjectionContainer container)
        {
            return container.MustNotBeNull(nameof(container))
                            .Register(container, options => options.DisableIDisposableTracking());
        }

        public static DependencyInjectionContainer AddDefaultListRegistration(this DependencyInjectionContainer container)
        {
            return container.MustNotBeNull(nameof(container))
                            .Register(typeof(List<>), options => options.UseDefaultConstructor()
                                                                        .MapToAbstractions(typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>), typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>)));
        }
    }
}
using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public static class DiContainerSetupExtensions
    {
        public static DiContainer AddDefaultGuidRegistration(this DiContainer container)
        {
            return container.MustNotBeNull(nameof(container))
                            .Register<Guid>(options => options.InstantiateVia(Guid.NewGuid));
        }

        public static DiContainer AddDefaultContainerRegistration(this DiContainer container)
        {
            return container.MustNotBeNull(nameof(container))
                            .Register(container, options => options.DisableIDisposableTracking());
        }

        public static DiContainer AddDefaultListRegistration(this DiContainer container)
        {
            return container.MustNotBeNull(nameof(container))
                            .Register(typeof(List<>), options => options.UseDefaultConstructor()
                                                                        .MapToAbstractions(typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>), typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>)));
        }
    }
}
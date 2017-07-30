using System;
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
    }
}
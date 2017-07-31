using System.Collections.Generic;

namespace Light.DependencyInjection.Registrations
{
    public interface IDependencyInfo
    {
        IReadOnlyList<Dependency> Dependencies { get; }
    }
}
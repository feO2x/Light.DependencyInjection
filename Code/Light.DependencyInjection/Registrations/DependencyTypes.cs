using System.Collections.Generic;

namespace Light.DependencyInjection.Registrations
{
    public class DependencyTypes
    {
        public const string InstantiationDependency = nameof(InstantiationDependency);
        public const string FieldInjection = nameof(FieldInjection);

        public const string PropertyInjection = nameof(PropertyInjection);

        public static readonly IReadOnlyList<string> InstanceManipulationTypes =
            new[]
            {
                PropertyInjection,
                FieldInjection
            };

        protected DependencyTypes() { }
    }
}
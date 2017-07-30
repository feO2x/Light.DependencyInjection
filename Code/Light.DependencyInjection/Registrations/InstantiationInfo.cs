﻿using System.Collections.Generic;

namespace Light.DependencyInjection.Registrations
{
    public abstract class InstantiationInfo
    {
        public readonly IReadOnlyList<Dependency> InstantiationDependencies;
        public readonly TypeKey TypeKey;

        protected InstantiationInfo(TypeKey typeKey, IReadOnlyList<Dependency> instantiationDependencies)
        {
            TypeKey = typeKey.MustNotBeEmpty();
            InstantiationDependencies = instantiationDependencies;
        }
    }
}
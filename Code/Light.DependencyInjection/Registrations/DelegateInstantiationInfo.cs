using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class DelegateInstantiationInfo : InstantiationInfo
    {
        public readonly Delegate Delegate;

        public DelegateInstantiationInfo(TypeKey typeKey,
                                         Delegate @delegate,
                                         IReadOnlyList<InstantiationDependency> instantiationDependencies)
            : base(typeKey, instantiationDependencies)
        {
            Delegate = @delegate.MustNotBeNull(nameof(@delegate));
        }
    }
}
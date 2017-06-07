using System;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeConstruction
{
    public interface IStandardizedConstructionFunctionFactory
    {
        Func<object> Create(Registration registration);
    }
}
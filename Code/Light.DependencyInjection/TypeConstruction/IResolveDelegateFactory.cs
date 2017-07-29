using System;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeConstruction
{
    public interface IResolveDelegateFactory
    {
        Func<object> Create(TypeKey typeKey, DiContainer container);
    }
}
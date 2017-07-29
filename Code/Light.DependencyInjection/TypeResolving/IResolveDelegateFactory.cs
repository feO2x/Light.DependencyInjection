using System;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IResolveDelegateFactory
    {
        Func<object> Create(TypeKey typeKey, DiContainer container);
    }
}
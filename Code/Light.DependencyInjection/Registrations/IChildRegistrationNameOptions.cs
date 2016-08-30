﻿namespace Light.DependencyInjection.Registrations
{
    public interface IChildRegistrationNameOptions<out TRegistrationOptions> where TRegistrationOptions : class, IBaseRegistrationOptions<TRegistrationOptions>
    {
        TRegistrationOptions WithName(string childValueRegistrationName);
    }
}
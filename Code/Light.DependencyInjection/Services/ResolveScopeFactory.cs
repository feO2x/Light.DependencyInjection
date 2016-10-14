using System;
using System.Collections.Generic;
using System.Threading;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public sealed class ResolveScopeFactory
    {
        private static readonly Func<Dictionary<TypeKey, object>> CreateDictionaryDelegate;
        private LazyThreadSafetyMode _lazyThreadSafetyMode = LazyThreadSafetyMode.None;

        static ResolveScopeFactory()
        {
            CreateDictionaryDelegate = CreateDictionary;
        }

        public LazyThreadSafetyMode LazyThreadSafetyMode
        {
            get { return _lazyThreadSafetyMode; }
            set
            {
                value.MustBeValidEnumValue(nameof(value));
                _lazyThreadSafetyMode = value;
            }
        }

        public Lazy<Dictionary<TypeKey, object>> CreateLazyScope()
        {
            return new Lazy<Dictionary<TypeKey, object>>(CreateDictionaryDelegate, _lazyThreadSafetyMode);
        }

        private static Dictionary<TypeKey, object> CreateDictionary()
        {
            return new Dictionary<TypeKey, object>();
        }
    }
}
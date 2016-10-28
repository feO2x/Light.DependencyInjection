using System;
using System.Collections.Generic;
using System.Threading;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents a factory that creates lazy scopes for each Resolve call.
    /// </summary>
    public sealed class ResolveScopeFactory
    {
        private static readonly Func<Dictionary<TypeKey, object>> CreateDictionaryDelegate;
        private LazyThreadSafetyMode _lazyThreadSafetyMode = LazyThreadSafetyMode.None;

        static ResolveScopeFactory()
        {
            CreateDictionaryDelegate = CreateDictionary;
        }

        /// <summary>
        ///     Gets or sets the lazy thread safety mode used for created scopes. This value defaults to LazyThreadSafetyMode.None.
        /// </summary>
        public LazyThreadSafetyMode LazyThreadSafetyMode
        {
            get { return _lazyThreadSafetyMode; }
            set
            {
                value.MustBeValidEnumValue(nameof(value));
                _lazyThreadSafetyMode = value;
            }
        }

        /// <summary>
        ///     Creates a lazy resolve scope.
        /// </summary>
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
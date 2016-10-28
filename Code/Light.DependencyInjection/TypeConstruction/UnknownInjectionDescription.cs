using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents a description of a member dependency override
    ///     that could not be associated with a configured <see cref="InstanceInjection" />.
    /// </summary>
    public struct UnknownInjectionDescription
    {
        /// <summary>
        ///     Gets the info describing the target member.
        /// </summary>
        public readonly MemberInfo MemberInfo;

        /// <summary>
        ///     Gets the value that should be injected into the target member.
        /// </summary>
        public readonly object Value;

        /// <summary>
        ///     Initializes a new instance of <see cref="UnknownInjectionDescription" />.
        /// </summary>
        /// <param name="memberInfo">The info describing the target member.</param>
        /// <param name="value">The value that should be injected into the target member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberInfo" /> is null.</exception>
        public UnknownInjectionDescription(MemberInfo memberInfo, object value)
        {
            memberInfo.MustNotBeNull(nameof(memberInfo));

            MemberInfo = memberInfo;
            Value = value;
        }
    }
}
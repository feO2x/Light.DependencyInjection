using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class FrameworkInspectionTests
    {
        [Fact(DisplayName = "The enumerator of an empty collection returns false on the first call to MoveNext.")]
        public void EmptyCollectionFirstMoveNextCall()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var emptyList = new List<object>();
            var enumerator = emptyList.GetEnumerator();

            var firstCallResult = enumerator.MoveNext();

            firstCallResult.Should().BeFalse();
            enumerator.Current.Should().Be(default(object));
            enumerator.Dispose();
        }
    }
}
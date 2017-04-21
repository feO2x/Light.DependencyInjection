using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            true.Should().BeTrue();
        }
    }
}
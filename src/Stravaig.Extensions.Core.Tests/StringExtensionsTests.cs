using NUnit.Framework;
using Shouldly;

namespace Stravaig.Extensions.Core.Tests
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void NullHasContentShouldBeFalse()
        {
            ((string) null).HasContent().ShouldBeFalse();
        }
        
        [Test]
        public void EmptyHasContentShouldBeFalse()
        {
            string.Empty.HasContent().ShouldBeFalse();
        }

        [Test]
        public void WhitespaceHasContentShouldBeFalse()
        {
            " \t\r\n".HasContent().ShouldBeFalse();
        }

        [Test]
        public void ValueHasContentShouldBeTrue()
        {
            "Some Value".HasContent().ShouldBeTrue();
        }
    }
}
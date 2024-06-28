using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn.v2.Labels.Renderers;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Labels.Renderers
{
    [TestClass]
    public class ZplImageOptionsTests
    {
        [TestMethod]
        public void ToCacheKey_IncludesAllProperties()
        {
            var options = new ZplImageOptions();
            var expectedSegmentCount = options.GetType().GetProperties().Length;

            var cacheKey = options.ToCacheKey();
            var keySegmentCount = cacheKey.Split( ':' ).Length;

            Assert.That.AreEqual( expectedSegmentCount, keySegmentCount );
        }
    }
}

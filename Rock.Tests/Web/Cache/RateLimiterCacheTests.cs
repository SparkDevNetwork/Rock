using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Web.Cache;

namespace Rock.Tests.Web.Cache
{
    [TestClass]
    public class RateLimiterCacheTests
    {
        [TestMethod]
        public async Task CanProcessPage_ShouldResetAfterPeriodExpires()
        {
            // A short period is used so the test does not spend several seconds
            // waiting. The delay is longer than the period so the rate-limit
            // window is guaranteed to have expired and the counter reset.
            var period = TimeSpan.FromSeconds( 1 );

            var result = RateLimiterCache.CanProcessPage(
                1,
                nameof( CanProcessPage_ShouldResetAfterPeriodExpires ),
                period,
                1,
                null );

            Assert.IsTrue( result );

            await Task.Delay( 1500 );

            result = RateLimiterCache.CanProcessPage(
                1,
                nameof( CanProcessPage_ShouldResetAfterPeriodExpires ),
                period,
                1,
                null );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void CanProcessPage_ShouldReturnFalseIfCallCountExceeded()
        {
            var result = RateLimiterCache.CanProcessPage(
                1,
                nameof( CanProcessPage_ShouldReturnFalseIfCallCountExceeded ),
                TimeSpan.FromSeconds( 5 ),
                1,
                null );

            Assert.IsTrue( result );

            result = RateLimiterCache.CanProcessPage(
                1,
                nameof( CanProcessPage_ShouldReturnFalseIfCallCountExceeded ),
                TimeSpan.FromSeconds( 5 ),
                1,
                null );

            Assert.IsFalse( result );
        }

        [TestMethod]
        public void CanProcessPage_ShouldReturnTrueIfCallCountNotExceeded()
        {
            var maxCount = 100;

            for ( var i = 0; i < maxCount; i++ )
            {
                var result = RateLimiterCache.CanProcessPage(
                    1,
                    nameof( CanProcessPage_ShouldReturnTrueIfCallCountNotExceeded ),
                    TimeSpan.FromSeconds( 5 ),
                    maxCount,
                    null );

                Assert.IsTrue( result );
            }

            var failedResult = RateLimiterCache.CanProcessPage(
                1,
                nameof( CanProcessPage_ShouldReturnTrueIfCallCountNotExceeded ),
                TimeSpan.FromSeconds( 5 ),
                maxCount,
                null );

            Assert.IsFalse( failedResult );
        }
    }
}

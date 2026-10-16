using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.CheckIn
{
    /// <summary>
    /// This suite checks that <see cref="PinAttemptLimiter"/> limits failed
    /// check-in PIN attempts per client IP address.
    /// </summary>
    [TestClass]
    public class PinAttemptLimiterTests : MockDatabaseTestsBase
    {
        /// <summary>
        /// Gets a client IP address that no other test uses, so tests do not
        /// share counts.
        /// </summary>
        private static string NewIpAddress()
        {
            return $"test-{Guid.NewGuid()}";
        }

        [TestMethod]
        public void TryReserveAttempt_AfterMaxFailures_IsRefused()
        {
            var ipAddress = NewIpAddress();

            for ( int i = 0; i < PinAttemptLimiter.MaxFailuresPerIpAddress; i++ )
            {
                Assert.IsTrue( PinAttemptLimiter.TryReserveAttempt( ipAddress ), $"Attempt {i + 1} should be allowed." );
            }

            Assert.IsFalse( PinAttemptLimiter.TryReserveAttempt( ipAddress ) );
            Assert.IsFalse( PinAttemptLimiter.TryReserveAttempt( ipAddress ) );

            PinAttemptLimiter.Reset( ipAddress );
        }

        [TestMethod]
        public void TryReserveAttempt_AfterMaxFailures_DoesNotAffectOtherIpAddress()
        {
            var ipAddress = NewIpAddress();
            var otherIpAddress = NewIpAddress();

            for ( int i = 0; i < PinAttemptLimiter.MaxFailuresPerIpAddress; i++ )
            {
                PinAttemptLimiter.TryReserveAttempt( ipAddress );
            }

            Assert.IsFalse( PinAttemptLimiter.TryReserveAttempt( ipAddress ) );
            Assert.IsTrue( PinAttemptLimiter.TryReserveAttempt( otherIpAddress ) );

            PinAttemptLimiter.Reset( ipAddress );
            PinAttemptLimiter.Reset( otherIpAddress );
        }

        [TestMethod]
        public void Reset_AfterMaxFailures_AllowsAttemptsAgain()
        {
            var ipAddress = NewIpAddress();

            for ( int i = 0; i < PinAttemptLimiter.MaxFailuresPerIpAddress; i++ )
            {
                PinAttemptLimiter.TryReserveAttempt( ipAddress );
            }

            PinAttemptLimiter.Reset( ipAddress );

            Assert.IsTrue( PinAttemptLimiter.TryReserveAttempt( ipAddress ) );

            PinAttemptLimiter.Reset( ipAddress );
        }

        [TestMethod]
        public void ReleaseAttempt_DoesNotCountAsFailure()
        {
            var ipAddress = NewIpAddress();

            for ( int i = 0; i < 100; i++ )
            {
                Assert.IsTrue( PinAttemptLimiter.TryReserveAttempt( ipAddress ) );
                PinAttemptLimiter.ReleaseAttempt( ipAddress );
            }

            for ( int i = 0; i < PinAttemptLimiter.MaxFailuresPerIpAddress; i++ )
            {
                Assert.IsTrue( PinAttemptLimiter.TryReserveAttempt( ipAddress ), $"Attempt {i + 1} should be allowed." );
            }

            Assert.IsFalse( PinAttemptLimiter.TryReserveAttempt( ipAddress ) );

            PinAttemptLimiter.Reset( ipAddress );
        }

        [TestMethod]
        public void ReleaseAttempt_WithNothingReserved_DoesNotGiveExtraAttempts()
        {
            var ipAddress = NewIpAddress();

            // Nothing counted yet, so this must not go below zero.
            PinAttemptLimiter.ReleaseAttempt( ipAddress );

            PinAttemptLimiter.TryReserveAttempt( ipAddress );
            PinAttemptLimiter.ReleaseAttempt( ipAddress );
            PinAttemptLimiter.ReleaseAttempt( ipAddress );
            PinAttemptLimiter.ReleaseAttempt( ipAddress );

            for ( int i = 0; i < PinAttemptLimiter.MaxFailuresPerIpAddress; i++ )
            {
                PinAttemptLimiter.TryReserveAttempt( ipAddress );
            }

            Assert.IsFalse( PinAttemptLimiter.TryReserveAttempt( ipAddress ) );

            PinAttemptLimiter.Reset( ipAddress );
        }

        [TestMethod]
        public void TryReserveAttempt_ParallelBurst_AllowsOnlyMaxFailures()
        {
            var ipAddress = NewIpAddress();
            var allowedCount = 0;

            Parallel.For( 0, 200, new ParallelOptions { MaxDegreeOfParallelism = 16 }, i =>
            {
                if ( PinAttemptLimiter.TryReserveAttempt( ipAddress ) )
                {
                    Interlocked.Increment( ref allowedCount );
                }
            } );

            Assert.AreEqual( PinAttemptLimiter.MaxFailuresPerIpAddress, allowedCount );

            PinAttemptLimiter.Reset( ipAddress );
        }

        [TestMethod]
        public void TryReserveAttempt_WithNullIpAddress_SharesEmptyIpAddressCount()
        {
            PinAttemptLimiter.Reset( null );

            for ( int i = 0; i < PinAttemptLimiter.MaxFailuresPerIpAddress; i++ )
            {
                Assert.IsTrue( PinAttemptLimiter.TryReserveAttempt( i % 2 == 0 ? null : string.Empty ) );
            }

            Assert.IsFalse( PinAttemptLimiter.TryReserveAttempt( null ) );
            Assert.IsFalse( PinAttemptLimiter.TryReserveAttempt( string.Empty ) );

            PinAttemptLimiter.Reset( null );
        }

        [TestMethod]
        public void TryReserveAttempt_WithIpAddressVariations_SharesOneCount()
        {
            var ipAddress = NewIpAddress();
            var padding = new string( 'x', 100 );
            var longIpAddress = ipAddress + padding;
            var otherLongIpAddress = ipAddress + padding + "y";

            for ( int i = 0; i < PinAttemptLimiter.MaxFailuresPerIpAddress; i++ )
            {
                Assert.IsTrue( PinAttemptLimiter.TryReserveAttempt( i % 2 == 0 ? $"  {longIpAddress.ToUpperInvariant()} " : longIpAddress ) );
            }

            Assert.IsFalse( PinAttemptLimiter.TryReserveAttempt( otherLongIpAddress ) );
            Assert.AreEqual( PinAttemptLimiter.MaxIpAddressKeyLength, PinAttemptLimiter.GetIpAddressKey( otherLongIpAddress ).Length );

            PinAttemptLimiter.Reset( longIpAddress );
        }

        [TestMethod]
        public void TryRecordAction_AfterPeriodEnds_AllowsActionsAgain()
        {
            // The limiter's own window is 10 minutes, so check the window
            // behavior it relies on with a short period.
            var key = "pin-attempt-limiter-test";
            var ipAddress = NewIpAddress();
            var period = TimeSpan.FromSeconds( 1 );

            Assert.IsTrue( RateLimiterCache.TryRecordAction( key, ipAddress, period, 2 ) );
            Assert.IsTrue( RateLimiterCache.TryRecordAction( key, ipAddress, period, 2 ) );
            Assert.IsFalse( RateLimiterCache.TryRecordAction( key, ipAddress, period, 2 ) );

            Thread.Sleep( TimeSpan.FromSeconds( 2 ) );

            Assert.IsTrue( RateLimiterCache.TryRecordAction( key, ipAddress, period, 2 ) );

            RateLimiterCache.ResetAction( key, ipAddress );
        }
    }
}

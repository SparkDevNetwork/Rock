using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;
using Rock.Utility;

namespace Rock.Tests.UnitTests.Rock.Utility
{
    [TestClass]
    public class MethodRetryTests
    {
        [TestMethod]
        public void ExecuteShouldReturnAfterMaxNumberOfTries()
        {
            var expectedCallCount = 5;
            var methodRetry = new MethodRetry( 10, 10, 5000, expectedCallCount );

            var actualCallCount = 0;
            var result = methodRetry.Execute( () => actualCallCount++, ( callCount ) => false );

            Assert.That.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public void ExecuteShouldStopCorrectly()
        {
            var expectedCallCount = 1;
            var methodRetry = new MethodRetry( 10, 10, 5000, 5 );

            var actualCallCount = 0;
            var result = methodRetry.Execute( () => actualCallCount++, ( callCount ) => true );

            Assert.That.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public void ExecuteShouldRunTheCorrectNumberOfTimes()
        {
            var expectedCallCount = 3;
            var methodRetry = new MethodRetry( 10, 10, 5000, 5 );

            var actualCallCount = 0;
            var result = methodRetry.Execute( () => actualCallCount++, ( callCount ) => callCount == ( expectedCallCount - 1 ) );

            Assert.That.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public void ExecuteShouldWaitBetweenTries()
        {
            var expectedCallCount = 4;
            var expectedMinWait = 1000;
            var expectedMaxWait = 1000;
            var methodRetry = new MethodRetry( 500, expectedMinWait, expectedMaxWait, 5 );

            var actualCallCount = 0;
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            var result = methodRetry.Execute( () => actualCallCount++, ( callCount ) => callCount == expectedCallCount );
            stopWatch.Stop();

            var minExpectedRuntime = expectedMinWait * ( expectedCallCount - 1 );
            if ( minExpectedRuntime > stopWatch.ElapsedMilliseconds )
            {
                Assert.That.Fail( $"Execute did not take long enough to run. Expected a minimum of {minExpectedRuntime}ms, but only for {stopWatch.ElapsedMilliseconds}ms" );
            }

            var maxExpectedRuntime = ( expectedMaxWait * expectedCallCount );
            // Add 5% fudge factor.
            maxExpectedRuntime += Convert.ToInt32( Math.Round( maxExpectedRuntime * .05 ) );
            if ( stopWatch.ElapsedMilliseconds > maxExpectedRuntime )
            {
                Assert.That.Fail( $"Execute took too long to run. Expected a maximum of {maxExpectedRuntime}ms, but ran for {stopWatch.ElapsedMilliseconds}ms" );
            }
        }

        [TestMethod]
        public async Task ExecuteAsyncShouldReturnAfterMaxNumberOfTries()
        {
            var expectedCallCount = 5;
            var methodRetry = new MethodRetry( 10, 10, 5000, expectedCallCount );

            var actualCallCount = 0;
            var result = await methodRetry.ExecuteAsync( async () => await Task.FromResult( actualCallCount++ ), ( callCount ) => false ).ConfigureAwait( false );

            Assert.That.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public async Task ExecuteAsyncShouldStopCorrectly()
        {
            var expectedCallCount = 1;
            var methodRetry = new MethodRetry( 10, 10, 5000, 5 );

            var actualCallCount = 0;
            var result = await methodRetry.ExecuteAsync( () => Task.FromResult( actualCallCount++ ), ( callCount ) => true );

            Assert.That.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public async Task ExecuteAsyncShouldRunTheCorrectNumberOfTimes()
        {
            var expectedCallCount = 3;
            var methodRetry = new MethodRetry( 10, 10, 5000, 5 );

            var actualCallCount = 0;
            var result = await methodRetry.ExecuteAsync<int>( () => Task.FromResult( actualCallCount++ ), ( callCount ) => callCount == ( expectedCallCount - 1 ) );

            Assert.That.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public async Task ExecuteAsyncShouldWaitBetweenTries()
        {
            var expectedCallCount = 4;
            var expectedMinWait = 1000;
            var expectedMaxWait = 1000;
            var methodRetry = new MethodRetry( 500, expectedMinWait, expectedMaxWait, 5 );

            var actualCallCount = 0;
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await methodRetry.ExecuteAsync( () => Task.FromResult( actualCallCount++ ), ( callCount ) => callCount == expectedCallCount );
            stopWatch.Stop();

            var minExpectedRuntime = expectedMinWait * ( expectedCallCount - 1 );
            if ( minExpectedRuntime > stopWatch.ElapsedMilliseconds )
            {
                Assert.That.Fail( $"Execute did not take long enough to run. Expected a minimum of {minExpectedRuntime}ms, but only for {stopWatch.ElapsedMilliseconds}ms" );
            }

            var maxExpectedRuntime = ( expectedMaxWait * expectedCallCount );
            // Add 5% fudge factor.
            maxExpectedRuntime += Convert.ToInt32( Math.Round( maxExpectedRuntime * .05 ) );
            if ( stopWatch.ElapsedMilliseconds > maxExpectedRuntime )
            {
                Assert.That.Fail( $"Execute took too long to run. Expected a maximum of {maxExpectedRuntime}ms, but ran for {stopWatch.ElapsedMilliseconds}ms" );
            }
        }
    }
}

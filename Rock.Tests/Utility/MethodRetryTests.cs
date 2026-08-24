using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Utility;

namespace Rock.Tests.Utility
{
    [TestClass]
    public class MethodRetryTests
    {
        [TestMethod]
        public void ExecuteShouldReturnAfterMaxNumberOfTries()
        {
            var expectedCallCount = 5;

            // This test only cares about the call count, so use a non-blocking
            // wait to avoid the real back-off delays between tries.
            var methodRetry = new MethodRetry( 10, 10, 5000, expectedCallCount )
            {
                WaitBetweenTries = _ => { }
            };

            var actualCallCount = 0;
            var result = methodRetry.Execute( () => actualCallCount++, ( callCount ) => false );

            Assert.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public void ExecuteShouldStopCorrectly()
        {
            var expectedCallCount = 1;
            var methodRetry = new MethodRetry( 10, 10, 5000, 5 );

            var actualCallCount = 0;
            var result = methodRetry.Execute( () => actualCallCount++, ( callCount ) => true );

            Assert.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public void ExecuteShouldRunTheCorrectNumberOfTimes()
        {
            var expectedCallCount = 3;
            var methodRetry = new MethodRetry( 10, 10, 5000, 5 );

            var actualCallCount = 0;
            var result = methodRetry.Execute( () => actualCallCount++, ( callCount ) => callCount == ( expectedCallCount - 1 ) );

            Assert.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public void ExecuteShouldWaitBetweenTries()
        {
            var expectedCallCount = 4;
            var expectedWait = 1000;

            // Substitute a non-blocking wait that records the requested intervals
            // instead of actually sleeping, so we can assert that Execute waits
            // between every try (and for how long) without the wall-clock cost.
            var recordedWaits = new List<TimeSpan>();
            var methodRetry = new MethodRetry( 1, expectedWait, expectedWait, expectedCallCount )
            {
                WaitBetweenTries = recordedWaits.Add
            };

            var actualCallCount = 0;
            methodRetry.Execute( () => actualCallCount++, _ => false );

            Assert.AreEqual( expectedCallCount, actualCallCount );
            Assert.HasCount( expectedCallCount, recordedWaits, "Execute should wait once between each try." );
            Assert.IsTrue( recordedWaits.All( w => w.TotalMilliseconds == expectedWait ), "Each wait should match the configured back-off interval." );
        }

        [TestMethod]
        public async Task ExecuteAsyncShouldReturnAfterMaxNumberOfTries()
        {
            var expectedCallCount = 5;

            // This test only cares about the call count, so use a non-blocking
            // wait to avoid the real back-off delays between tries.
            var methodRetry = new MethodRetry( 10, 10, 5000, expectedCallCount )
            {
                WaitBetweenTriesAsync = _ => Task.CompletedTask
            };

            var actualCallCount = 0;
            var result = await methodRetry.ExecuteAsync( async () => await Task.FromResult( actualCallCount++ ), ( callCount ) => false ).ConfigureAwait( false );

            Assert.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public async Task ExecuteAsyncShouldStopCorrectly()
        {
            var expectedCallCount = 1;
            var methodRetry = new MethodRetry( 10, 10, 5000, 5 );

            var actualCallCount = 0;
            var result = await methodRetry.ExecuteAsync( () => Task.FromResult( actualCallCount++ ), ( callCount ) => true );

            Assert.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public async Task ExecuteAsyncShouldRunTheCorrectNumberOfTimes()
        {
            var expectedCallCount = 3;
            var methodRetry = new MethodRetry( 10, 10, 5000, 5 );

            var actualCallCount = 0;
            var result = await methodRetry.ExecuteAsync<int>( () => Task.FromResult( actualCallCount++ ), ( callCount ) => callCount == ( expectedCallCount - 1 ) );

            Assert.AreEqual( expectedCallCount, actualCallCount );
        }

        [TestMethod]
        public async Task ExecuteAsyncShouldWaitBetweenTries()
        {
            var expectedCallCount = 4;
            var expectedWait = 1000;

            // Substitute a non-blocking wait that records the requested intervals
            // instead of actually delaying, so we can assert that ExecuteAsync waits
            // between every try (and for how long) without the wall-clock cost.
            var recordedWaits = new List<TimeSpan>();
            var methodRetry = new MethodRetry( 1, expectedWait, expectedWait, expectedCallCount )
            {
                WaitBetweenTriesAsync = duration =>
                {
                    recordedWaits.Add( duration );
                    return Task.CompletedTask;
                }
            };

            var actualCallCount = 0;
            await methodRetry.ExecuteAsync( () => Task.FromResult( actualCallCount++ ), _ => false );

            Assert.AreEqual( expectedCallCount, actualCallCount );
            Assert.HasCount( expectedCallCount, recordedWaits, "ExecuteAsync should wait once between each try." );
            Assert.IsTrue( recordedWaits.All( w => w.TotalMilliseconds == expectedWait ), "Each wait should match the configured back-off interval." );
        }
    }
}

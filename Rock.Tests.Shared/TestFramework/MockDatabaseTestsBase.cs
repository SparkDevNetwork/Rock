using System;
using System.Threading;
using System.Threading.Tasks;

using GreenPipes;

using MassTransit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Bus;
using Rock.Bus.Transport;
using Rock.Logging;
using Rock.Web.Cache;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// A base class for check-in tests that require a mock database. This
    /// configures a few required things to help ensure that tests work in
    /// isolation.
    /// </summary>
    public abstract class MockDatabaseTestsBase
    {
        /// <summary>
        /// Initializes all required database mocks before running the tests.
        /// </summary>
        /// <param name="context">The context.</param>
        [ClassInitialize( InheritanceBehavior.BeforeEachDerivedClass )]
        public static async Task ClassInitialize( TestContext context )
        {
            // Create the fake IBusControl so we can simulate a bus. This fake
            // bus just discards all messages.
            var busControlMock = new Mock<IBusControl>( MockBehavior.Strict );
            busControlMock.Setup( m => m.ConnectConsumeObserver( It.IsAny<IConsumeObserver>() ) )
                .Returns( new Mock<ConnectHandle>().Object );
            busControlMock.Setup( m => m.ConnectReceiveObserver( It.IsAny<IReceiveObserver>() ) )
                .Returns( new Mock<ConnectHandle>().Object );
            busControlMock.Setup( m => m.StartAsync( It.IsAny<CancellationToken>() ) )
                .Returns( Task.FromResult( new Mock<BusHandle>().Object ) );
            busControlMock.Setup( m => m.Publish( It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<IPipe<PublishContext>>(), It.IsAny<CancellationToken>() ) )
                .Returns( Task.CompletedTask );

            // Create the fake TransportComponent that provides the bus control.
            var busComponentMock = new Mock<TransportComponent>( MockBehavior.Strict, false );
            busComponentMock.Setup( m => m.GetBusControl( It.IsAny<Action<IBusFactoryConfigurator>>() ) )
                .Returns( busControlMock.Object );

            // Start the bus.
            await RockMessageBus.StartBusInternalAsync( busComponentMock.Object );

            // Register a fake IRockLogger that discards all logged messages.
            var loggerMock = new Mock<IRockLogger>( MockBehavior.Loose );
            RockLogger.SetLoggerInternal( loggerMock.Object );
        }

        /// <summary>
        /// Cleans up all mocked configuration after the tests have run.
        /// </summary>
        [ClassCleanup( InheritanceBehavior.BeforeEachDerivedClass )]
        public static void ClassCleanup()
        {
            // Reset the rock logger.
            RockLogger.SetLoggerInternal( null );

            // Stop the fake bus.
            RockMessageBus.StopBusInternal();
        }

        /// <summary>
        /// Cleans up any lingering data that might have crept in from the test.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            // Clear any cached items that were generated while running.
            RockCache.ClearAllCachedItems( false );
        }
    }
}

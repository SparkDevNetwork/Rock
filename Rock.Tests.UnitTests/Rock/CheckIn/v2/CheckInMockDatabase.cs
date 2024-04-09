using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GreenPipes;

using MassTransit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
using Moq.Protected;

using Rock.Bus;
using Rock.Bus.Transport;
using Rock.Data;
using Rock.Logging;
using Rock.Web.Cache;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2
{
    /// <summary>
    /// A base class for check-in tests that require a mock database. This
    /// configures a few required things to help ensure that tests work in
    /// isolation.
    /// </summary>
    public abstract class CheckInMockDatabase
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

        /// <summary>
        /// Gets a mocked <see cref="RockContext"/> that can be used to setup
        /// additional mocked values and then used for database access.
        /// </summary>
        /// <returns>An mocking instance for <see cref="RockContext"/>.</returns>
        protected static Mock<RockContext> GetRockContextMock()
        {
            var rockContextMock = new Mock<RockContext>( MockBehavior.Strict, "invalidConnectionString" );

            // TODO: This should be removed in v17 after all the DbSet properties are gone.
            // This is here so the base RockContext constructor doesn't throw an error when
            // trying to initialize all the DbSet properties.
            rockContextMock.Setup( m => m.Set<It.IsAnyType>() ).Returns( new InvocationFunc( invocation =>
            {
                var dbSetType = invocation.Method.GetGenericArguments()[0];
                var mockType = typeof( Mock<> ).MakeGenericType( typeof( DbSet<> ).MakeGenericType( dbSetType ) );

                var dbSetMock = ( Mock ) Activator.CreateInstance( mockType, new object[] { MockBehavior.Strict } );

                return dbSetMock.Object;
            } ) );

            // Ignore any call to dispose.
            rockContextMock.Protected().Setup( "Dispose", ItExpr.IsAny<bool>() );

            return rockContextMock;
        }
    }
}

using Moq;
using Moq.Protected;

using Rock.Data;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Helper methods for working with mock databases.
    /// </summary>
    /// <remarks>
    /// This is the low-level seam for mocking a <see cref="RockContext"/> and is
    /// rarely used directly. Most tests should use the higher-level
    /// <c>TestHelper.CreateScopedRockApp()</c>, which builds the mocked context
    /// through this helper, registers it with the dependency injection system as an
    /// <see cref="IRockContextFactory"/>, and clears the cache when the scope is
    /// disposed after each test.
    /// </remarks>
    internal static class MockDatabaseHelper
    {
        /// <summary>
        /// Create an <see cref="IRockContextFactory"/> object that always
        /// returns the <see cref="RockContext"/> from <paramref name="rockContextMock"/>.
        /// It is assumed this context will be configured to ignore the
        /// Dispose() method.
        /// </summary>
        /// <param name="rockContextMock">The mock that contains the <see cref="RockContext"/> to return.</param>
        /// <returns>An instance of <see cref="IRockContextFactory"/>.</returns>
        public static IRockContextFactory CreateRockContextFactory( Mock<RockContext> rockContextMock )
        {
            var factoryMock = new Mock<IRockContextFactory>();

            factoryMock.Setup( f => f.CreateRockContext() ).Returns( rockContextMock.Object );

            return factoryMock.Object;
        }

        /// <summary>
        /// Gets a mocked <see cref="RockContext"/> that can be used to setup
        /// additional mocked values and then used for database access.
        /// </summary>
        /// <returns>An mocking instance for <see cref="RockContext"/>.</returns>
        public static RockMock<RockContext> CreateRockContextMock( bool autoMode = true )
        {
            var rockContextMock = new RockMock<RockContext>( MockBehavior.Strict, "invalidConnectionString" );

            rockContextMock.Setup( m => m.ToString() ).Returns( "Mock RockContext" );

            if ( autoMode )
            {
                rockContextMock.SetupAutoDbSets();
                rockContextMock.SetupSaveChanges();
            }

            // Ignore any call to dispose.
            rockContextMock.Protected().Setup( "Dispose", ItExpr.IsAny<bool>() );

            return rockContextMock;
        }
    }
}

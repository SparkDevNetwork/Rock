using System;
using System.Collections.Generic;

using Moq;
using Moq.Protected;

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Helper methods for working with mock databases.
    /// </summary>
    public static class MockDatabaseHelper
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

        /// <summary>
        /// Creates a mock <typeparamref name="TEntity"/> object.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="guid">The entity unique identifier.</param>
        /// <returns>A mocking instance for <typeparamref name="TEntity"/>.</returns>
        public static Mock<TEntity> CreateEntityMock<TEntity>( int id, Guid guid )
            where TEntity : class, IEntity, new()
        {
            var entityMock = new RockMock<TEntity>( MockBehavior.Loose )
            {
                CallBase = true
            };

            entityMock.Setup( m => m.TypeId ).Returns( 0 );

            entityMock.SetupInitializer( instance =>
            {
                instance.Id = id;
                instance.Guid = guid;

                if ( instance is IHasAttributes attributeMock )
                {
                    attributeMock.Attributes = new Dictionary<string, AttributeCache>();
                    attributeMock.AttributeValues = new Dictionary<string, AttributeValueCache>();
                }
            } );

            return entityMock;
        }
    }
}

using System;

using Moq;

using Rock.Data;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2
{
    /// <summary>
    /// A base class for check-in tests that require a mock database. This
    /// configures a few required things to help ensure that tests work in
    /// isolation.
    /// </summary>
    public abstract class CheckInMockDatabase : MockDatabaseTestsBase
    {
        /// <inheritdoc cref="MockDatabaseHelper.GetRockContextMock"/>
        protected static Mock<RockContext> GetRockContextMock()
        {
            return MockDatabaseHelper.GetRockContextMock();
        }

        /// <inheritdoc cref="MockDatabaseHelper.CreateEntityMock{TEntity}(int, Guid)"/>
        public static Mock<TEntity> CreateEntityMock<TEntity>( int id, Guid guid )
            where TEntity : class, IEntity, new()
        {
            return MockDatabaseHelper.CreateEntityMock<TEntity>( id, guid );
        }
    }
}

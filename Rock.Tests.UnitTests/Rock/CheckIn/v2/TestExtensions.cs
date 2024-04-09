using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Moq;

using Rock.Data;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2
{
    /// <summary>
    /// Various extension methods to make check-in unit tests easier to write
    /// and read.
    /// </summary>
    internal static class TestExtensions
    {
        /// <summary>
        /// Sets up a mock DbSet for the model type <typeparamref name="TEntity"/> that
        /// will provide access to the items in <paramref name="entities"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="rockContextMock">The mocked <see cref="RockContext"/>.</param>
        /// <param name="entities">The entities to be included in the set.</param>
        /// <returns>A mocking instance for <see cref="DbSet{TEntity}"/>.</returns>
        public static Mock<DbSet<TEntity>> SetupDbSet<TEntity>( this Mock<RockContext> rockContextMock, params TEntity[] entities )
            where TEntity : class
        {
            var dbSetMock = entities.GetDbSetMock();

            rockContextMock.Setup( m => m.Set<TEntity>() ).Returns( dbSetMock.Object );

            return dbSetMock;
        }

        /// <summary>
        /// Gets a mocked <see cref="DbSet{TEntity}"/> instance that will
        /// provide access to the items in the <paramref name="sourceList"/>.
        /// </summary>
        /// <typeparam name="T">The type of entity provided by this <see cref="DbSet{TEntity}"/>.</typeparam>
        /// <param name="sourceList">The source list of objects.</param>
        /// <returns>A mocking instance for <see cref="DbSet{TEntity}"/>.</returns>
        public static Mock<DbSet<T>> GetDbSetMock<T>( this IReadOnlyCollection<T> sourceList ) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup( m => m.Provider ).Returns( queryable.Provider );
            dbSet.As<IQueryable<T>>().Setup( m => m.Expression ).Returns( queryable.Expression );
            dbSet.As<IQueryable<T>>().Setup( m => m.ElementType ).Returns( queryable.ElementType );
            dbSet.As<IQueryable<T>>().Setup( m => m.GetEnumerator() ).Returns( () => queryable.GetEnumerator() );

            return dbSet;
        }
    }
}

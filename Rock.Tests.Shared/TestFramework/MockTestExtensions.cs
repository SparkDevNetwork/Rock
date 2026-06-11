using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Moq;

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Various extension methods to make check-in unit tests easier to write
    /// and read.
    /// </summary>
    public static class MockTestExtensions
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

            rockContextMock.Setup( m => m.Set<TEntity>() ).Returns( () => dbSetMock.Object );

            return dbSetMock;
        }

        /// <summary>
        /// Sets up a mock DbSet for the model type <typeparamref name="TEntity"/> that
        /// will provide access to the items in <paramref name="entities"/>. The DbSet
        /// wills upport Add and Remove operations which will update the item list.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="rockContextMock">The mocked <see cref="RockContext"/>.</param>
        /// <param name="entities">The initial entities to be included in the set.</param>
        /// <returns>A mocking instance for <see cref="DbSet{TEntity}"/>.</returns>
        public static Mock<DbSet<TEntity>> SetupDbSet<TEntity>( this Mock<RockContext> rockContextMock, List<TEntity> entities )
            where TEntity : class
        {
            var dbSetMock = entities.GetDbSetMock();

            rockContextMock.Setup( m => m.Set<TEntity>() ).Returns( () => dbSetMock.Object );

            return dbSetMock;
        }

        /// <summary>
        /// Configures the mock <see cref="RockContext"/> to support automatic
        /// DbSet initialization. When something tries to access the
        /// <c>Set&lt;T&gt;()</c> method, a new DbSet will be initialized for
        /// use with an initially empty collection of items.
        /// </summary>
        /// <param name="rockContextMock">The mock <see cref="RockContext"/> to setup.</param>
        public static void SetupAutoDbSets( this RockMock<RockContext> rockContextMock )
        {
            var autoDbSets = new Dictionary<Type, IEnumerable>();

            rockContextMock.Setup( m => m.Set<It.IsAnyType>() ).Returns( new InvocationFunc( invocation =>
            {
                var typeArgument = invocation.Method.GetGenericArguments()[0];

                if ( !autoDbSets.TryGetValue( typeArgument, out var dbSet ) )
                {
                    var listType = typeof( List<> ).MakeGenericType( typeArgument );
                    var dbSetList = Activator.CreateInstance( listType );
                    var methods = typeof( MockTestExtensions ).GetMethods();
                    var getDbSetMockMethod = typeof( MockTestExtensions )
                        .GetMethods()
                        .Where( m => m.Name == nameof( GetDbSetMock )
                            && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType.Name == typeof( List<> ).Name )
                        .First();

                    var dbSetMock = ( Mock ) getDbSetMockMethod.MakeGenericMethod( typeArgument ).Invoke( null, new object[] { dbSetList } );
                    dbSet = ( IEnumerable ) dbSetMock.Object;

                    autoDbSets.Add( typeArgument, dbSet );
                }

                return dbSet;
            } ) );

            rockContextMock.CustomData["AutoDbSets"] = autoDbSets;
        }

        /// <summary>
        /// Configures the mock <see cref="RockContext"/> to support saving
        /// changes. Calls to the various <c>SaveChanges</c> methods will
        /// look for any entities that have been added to the sets with an Id
        /// of <c>0</c> and automatically set them to the next available value.
        /// </summary>
        /// <param name="rockContextMock">The mock <see cref="RockContext"/> to setup.</param>
        public static void SetupSaveChanges( this RockMock<RockContext> rockContextMock )
        {
            rockContextMock.Setup( m => m.SaveChanges() ).Returns( () => ExecuteSaveChanges( rockContextMock ) );
            rockContextMock.Setup( m => m.SaveChanges( It.IsAny<bool>() ) ).Returns( () => ExecuteSaveChanges( rockContextMock ) );
            rockContextMock.Setup( m => m.SaveChanges( It.IsAny<SaveChangesArgs>() ) ).Returns( () => new SaveChangesResult
            {
                RecordsUpdated = ExecuteSaveChanges( rockContextMock )
            } );
            rockContextMock.Setup( m => m.WrapTransaction( It.IsAny<Action>() ) ).Callback<Action>( a => a() );
            rockContextMock.Setup( m => m.WrapTransactionIf( It.IsAny<Func<bool>>() ) ).Returns<Func<bool>>( a => a() );
        }

        /// <summary>
        /// Executes a fake <c>SaveChanges</c> operation for a mock
        /// <see cref="RockContext"/>. This will look for any entities that
        /// have been added to the various sets with an Id of <c>0</c>. Any
        /// that are found will automatically have their Id set to the next
        /// available value.
        /// </summary>
        /// <param name="rockContextMock">The mock <see cref="RockContext"/> to setup.</param>
        /// <returns>The number of records "modified".</returns>
        public static int ExecuteSaveChanges( this RockMock<RockContext> rockContextMock )
        {
            var autoDbSets = ( Dictionary<Type, IEnumerable> ) rockContextMock.CustomData["AutoDbSets"];
            int modifiedCount = 0;

            foreach ( var kvp in autoDbSets )
            {
                foreach ( var obj in kvp.Value )
                {
                    if ( obj is IEntity entity && entity.Id == 0 )
                    {
                        entity.Id = kvp.Value.OfType<IEntity>().Max( a => a.Id ) + 1;
                        modifiedCount++;
                    }
                }
            }

            return modifiedCount;
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

            var dbSetMock = new Mock<DbSet<T>>( MockBehavior.Strict );
            dbSetMock.As<IQueryable<T>>().Setup( m => m.Provider ).Returns( queryable.Provider );
            dbSetMock.As<IQueryable<T>>().Setup( m => m.Expression ).Returns( queryable.Expression );
            dbSetMock.As<IQueryable<T>>().Setup( m => m.ElementType ).Returns( queryable.ElementType );
            dbSetMock.As<IQueryable<T>>().Setup( m => m.GetEnumerator() ).Returns( () => queryable.GetEnumerator() );
            dbSetMock.As<IEnumerable>().Setup( m => m.GetEnumerator() ).Returns( () => queryable.GetEnumerator() );
            dbSetMock.Setup( m => m.AsNoTracking() ).Returns( () => dbSetMock.Object );
            dbSetMock.Setup( m => m.Include( It.IsAny<string>() ) ).Returns( () => dbSetMock.Object );

            return dbSetMock;
        }

        /// <summary>
        /// Gets a mocked <see cref="DbSet{TEntity}"/> instance that will
        /// provide access to the items in the <paramref name="sourceList"/>.
        /// </summary>
        /// <typeparam name="T">The type of entity provided by this <see cref="DbSet{TEntity}"/>.</typeparam>
        /// <param name="sourceList">The source list of objects.</param>
        /// <returns>A mocking instance for <see cref="DbSet{TEntity}"/>.</returns>
        public static Mock<DbSet<T>> GetDbSetMock<T>( this List<T> sourceList ) where T : class
        {
            var dbSetMock = GetDbSetMock( ( IReadOnlyCollection<T> ) sourceList );

            dbSetMock.Setup( m => m.Add( It.IsAny<T>() ) ).Returns<T>( a =>
            {
                sourceList.Add( a );
                return a;
            } );

            dbSetMock.Setup( m => m.Remove( It.IsAny<T>() ) ).Returns<T>( a =>
            {
                sourceList.Remove( a );
                return a;
            } );

            dbSetMock.Setup( m => m.AddRange( It.IsAny<IEnumerable<T>>() ) ).Returns<IEnumerable<T>>( a =>
            {
                sourceList.AddRange( a );
                return a;
            } );

            dbSetMock.Setup( m => m.RemoveRange( It.IsAny<IEnumerable<T>>() ) ).Returns<IEnumerable<T>>( a =>
            {
                foreach ( var item in a )
                {
                    sourceList.Remove( item );
                }
                return a;
            } );

            return dbSetMock;
        }

        /// <summary>
        /// Sets an attribute value for a mocked entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity that is being mocked.</typeparam>
        /// <param name="entity">The entity that is being mocked.</param>
        /// <param name="key">The attribute key.</param>
        /// <param name="value">The raw attribute value.</param>
        public static void SetMockAttributeValue<TEntity>( this Mock<TEntity> entity, string key, string value )
            where TEntity : class, IHasAttributes
        {
            entity.Object.AttributeValues[key] = new AttributeValueCache( 0, 0, value );
        }
    }
}

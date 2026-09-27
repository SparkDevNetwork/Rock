using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Moq;

using Rock.Data;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Various extension methods to make check-in unit tests easier to write
    /// and read.
    /// </summary>
    public static class MockTestExtensions
    {
        /// <summary>
        /// The protected <see cref="object.MemberwiseClone"/> method, used to
        /// make the fresh copies returned by a mocked <c>AsNoTracking()</c>.
        /// </summary>
        private static readonly System.Reflection.MethodInfo _memberwiseCloneMethod = typeof( object )
            .GetMethod( "MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );

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
            /*
                9/9/26 - CLAUDE

                A single mocked RockContext is shared by everything in a test
                scope (RockApp.Current.CreateRockContext() returns the same
                instance every call). The main test thread is expected to be the
                only caller, so this dictionary - and the backing List<T> in each
                DbSet built by GetDbSetMock, and the enumeration in
                ExecuteSaveChanges - are deliberately NOT thread-safe.

                Background threads (exception logging, bus publishing) used to
                reach this mock via RockApp.Current and mutate these collections
                concurrently, which produced intermittent IndexOutOfRangeException
                / "collection was modified" failures in CI. Those background
                writers are now diverted to sinks (IExceptionLogSink,
                IBusMessageSink) before they touch the mock, so single-threaded
                access holds again.

                If a future change reintroduces concurrent access to the mocked
                context, make autoDbSets a ConcurrentDictionary, snapshot it in
                ExecuteSaveChanges, and guard the per-set List<T> operations
                (Add/Remove/AddRange and the queryable enumerations) in
                GetDbSetMock - a single lock per mock is simplest.

                Reason: Document why these collections are intentionally not
                thread-safe and how to harden them if that ever changes.
            */
            var autoDbSets = new Dictionary<Type, IEnumerable>();

            // The backing lists hold every entity, including unsaved ones that
            // the DbSet queries hide, so ExecuteSaveChanges can assign their Ids.
            var autoDbSetLists = new Dictionary<Type, IEnumerable>();

            rockContextMock.Setup( m => m.Set<It.IsAnyType>() ).Returns( new InvocationFunc( invocation =>
            {
                var typeArgument = invocation.Method.GetGenericArguments()[0];

                if ( !autoDbSets.TryGetValue( typeArgument, out var dbSet ) )
                {
                    var listType = typeof( List<> ).MakeGenericType( typeArgument );
                    var dbSetList = ( IEnumerable ) Activator.CreateInstance( listType );
                    var getAutoDbSetMockMethod = typeof( MockTestExtensions ).GetMethod( nameof( GetAutoDbSetMock ) );

                    var dbSetMock = ( Mock ) getAutoDbSetMockMethod.MakeGenericMethod( typeArgument ).Invoke( null, new object[] { dbSetList } );
                    dbSet = ( IEnumerable ) dbSetMock.Object;

                    autoDbSets.Add( typeArgument, dbSet );
                    autoDbSetLists.Add( typeArgument, dbSetList );
                }

                return dbSet;
            } ) );

            rockContextMock.CustomData["AutoDbSets"] = autoDbSets;
            rockContextMock.CustomData["AutoDbSetLists"] = autoDbSetLists;
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
            // Walk the backing lists rather than the DbSets, since the DbSets
            // hide the unsaved (Id == 0) entities this method needs to find.
            var autoDbSetLists = ( Dictionary<Type, IEnumerable> ) rockContextMock.CustomData["AutoDbSetLists"];
            int modifiedCount = 0;

            /*
                8/23/26 - CLAUDE

                This runs once per SaveChanges() call, and some tests (e.g. the
                AttendanceCode generation tests) call SaveChanges() thousands of
                times against a set that grows into the thousands. The previous
                implementation scanned each set twice per call - once in the outer
                loop to find new entities and again via Max() for every new entity -
                which made those tests O(N^2) and dominated the entire unit-test run.

                We now assign Ids in a single pass: track the highest existing Id
                while collecting the new (Id == 0) entities, then hand out sequential
                Ids from that maximum. The resulting Ids are identical to before.

                Reason: Avoid O(N^2) Id assignment in high-volume SaveChanges loops.
            */
            foreach ( var kvp in autoDbSetLists )
            {
                int maxId = 0;
                List<IEntity> newEntities = null;

                foreach ( var obj in kvp.Value )
                {
                    if ( !( obj is IEntity entity ) )
                    {
                        continue;
                    }

                    if ( entity.Id == 0 )
                    {
                        newEntities = newEntities ?? new List<IEntity>();
                        newEntities.Add( entity );
                    }
                    else if ( entity.Id > maxId )
                    {
                        maxId = entity.Id;
                    }
                }

                if ( newEntities == null )
                {
                    continue;
                }

                foreach ( var entity in newEntities )
                {
                    entity.Id = ++maxId;
                    modifiedCount++;
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
            return CreateDbSetMock( sourceList.AsQueryable() );
        }

        /// <summary>
        /// Gets a mocked <see cref="DbSet{TEntity}"/> instance whose query
        /// operations read from <paramref name="queryable"/>.
        /// </summary>
        /// <typeparam name="T">The type of entity provided by this <see cref="DbSet{TEntity}"/>.</typeparam>
        /// <param name="queryable">The queryable view of the items that queries will see.</param>
        /// <returns>A mocking instance for <see cref="DbSet{TEntity}"/>.</returns>
        private static Mock<DbSet<T>> CreateDbSetMock<T>( IQueryable<T> queryable ) where T : class
        {
            return CreateDbSetMock( queryable, isNoTracking: false );
        }

        /// <summary>
        /// Gets a mocked <see cref="DbSet{TEntity}"/> instance whose query
        /// operations read from <paramref name="queryable"/>.
        /// </summary>
        /// <typeparam name="T">The type of entity provided by this <see cref="DbSet{TEntity}"/>.</typeparam>
        /// <param name="queryable">The queryable view of the items that queries will see.</param>
        /// <param name="isNoTracking"><c>true</c> if this is the set returned by <c>AsNoTracking()</c>, whose items are already copies.</param>
        /// <returns>A mocking instance for <see cref="DbSet{TEntity}"/>.</returns>
        private static Mock<DbSet<T>> CreateDbSetMock<T>( IQueryable<T> queryable, bool isNoTracking ) where T : class
        {
            var dbSetMock = new Mock<DbSet<T>>( MockBehavior.Strict );
            dbSetMock.As<IQueryable<T>>().Setup( m => m.Provider ).Returns( queryable.Provider );
            dbSetMock.As<IQueryable<T>>().Setup( m => m.Expression ).Returns( queryable.Expression );
            dbSetMock.As<IQueryable<T>>().Setup( m => m.ElementType ).Returns( queryable.ElementType );
            dbSetMock.As<IQueryable<T>>().Setup( m => m.GetEnumerator() ).Returns( () => queryable.GetEnumerator() );
            dbSetMock.As<IEnumerable>().Setup( m => m.GetEnumerator() ).Returns( () => queryable.GetEnumerator() );
            dbSetMock.Setup( m => m.Include( It.IsAny<string>() ) ).Returns( () => dbSetMock.Object );

            if ( isNoTracking )
            {
                dbSetMock.Setup( m => m.AsNoTracking() ).Returns( () => dbSetMock.Object );
            }
            else
            {
                /*
                    9/27/26 - CLAUDE

                    Real EF materializes brand new instances for an AsNoTracking()
                    query, so nothing done to those results can affect an entity
                    the caller is tracking and editing. Previously the mocked set
                    returned itself, handing back the very instances under edit.
                    Cache loads (EntityCache.GetMany) query with AsNoTracking()
                    and then call LoadAttributes() on the results, which silently
                    discarded pending attribute values on the entity being saved.

                    The no-tracking set now yields a fresh shallow copy of each
                    item on every enumeration. Copies are shallow, so navigation
                    properties still point at the same related objects; only
                    assignments made to the copy itself stay isolated.

                    Reason: Match EF's fresh-instance behavior for AsNoTracking().
                */
                var noTrackingQueryable = queryable
                    .AsEnumerable()
                    .Select( item => ( T ) _memberwiseCloneMethod.Invoke( item, null ) )
                    .AsQueryable();

                var noTrackingMock = new Lazy<Mock<DbSet<T>>>( () => CreateDbSetMock( noTrackingQueryable, isNoTracking: true ) );

                dbSetMock.Setup( m => m.AsNoTracking() ).Returns( () => noTrackingMock.Value.Object );
            }

            // Hand back a plain instance for Create(), which some implementation
            // code calls to make a new entity. Real EF returns a change tracking
            // proxy that resolves auto-navigation properties (setting CampusId, for
            // example, makes the Campus navigation property load lazily). This plain
            // instance does no such wiring, so a test relying on a navigation
            // property resolving from its foreign key id must set it explicitly.
            dbSetMock.Setup( m => m.Create() ).Returns( () => Activator.CreateInstance<T>() );

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

            SetupDbSetMutations( dbSetMock, sourceList );

            return dbSetMock;
        }

        /// <summary>
        /// Gets a mocked <see cref="DbSet{TEntity}"/> for the automatic DbSet
        /// support of a mocked <see cref="RockContext"/>. Mutations update
        /// <paramref name="sourceList"/>, but queries only see entities that have
        /// been saved, meaning they have a non-zero Id.
        /// </summary>
        /// <typeparam name="T">The type of entity provided by this <see cref="DbSet{TEntity}"/>.</typeparam>
        /// <param name="sourceList">The backing list of all entities, saved or not.</param>
        /// <returns>A mocking instance for <see cref="DbSet{TEntity}"/>.</returns>
        public static Mock<DbSet<T>> GetAutoDbSetMock<T>( List<T> sourceList ) where T : class
        {
            /*
                9/27/26 - CLAUDE

                Real EF does not return entities that were added to a DbSet but
                not yet saved; a query goes to the database, which does not have
                them. Previously the mocked set returned them immediately, so
                code that adds an entity and then queries (for example, finding
                the max Order, or checking for a duplicate name through a cache
                load) saw its own pending entity. Hiding Id == 0 entities from
                queries matches EF, since ExecuteSaveChanges assigns Ids on save.

                This is deliberately limited to the automatic sets. Sets built
                with SetupDbSet have no SaveChanges Id assignment, so filtering
                them would hide added entities forever.

                Reason: Make pending (unsaved) entities invisible to queries like EF.
            */
            var savedEntities = sourceList
                .Where( item => !( item is IEntity entity ) || entity.Id != 0 )
                .AsQueryable();

            var dbSetMock = CreateDbSetMock( savedEntities );

            SetupDbSetMutations( dbSetMock, sourceList );

            return dbSetMock;
        }

        /// <summary>
        /// Configures the Add and Remove operations of a mocked
        /// <see cref="DbSet{TEntity}"/> to update <paramref name="sourceList"/>.
        /// </summary>
        /// <typeparam name="T">The type of entity provided by the <see cref="DbSet{TEntity}"/>.</typeparam>
        /// <param name="dbSetMock">The mocked set to configure.</param>
        /// <param name="sourceList">The backing list to be updated.</param>
        private static void SetupDbSetMutations<T>( Mock<DbSet<T>> dbSetMock, List<T> sourceList ) where T : class
        {
            dbSetMock.Setup( m => m.Add( It.IsAny<T>() ) ).Returns<T>( a =>
            {
                sourceList.Add( a );
                return a;
            } );

            dbSetMock.Setup( m => m.Remove( It.IsAny<T>() ) ).Returns<T>( a =>
            {
                RemoveFromSourceList( sourceList, a );
                return a;
            } );

            dbSetMock.Setup( m => m.AddRange( It.IsAny<IEnumerable<T>>() ) ).Returns<IEnumerable<T>>( a =>
            {
                sourceList.AddRange( a );
                return a;
            } );

            dbSetMock.Setup( m => m.RemoveRange( It.IsAny<IEnumerable<T>>() ) ).Returns<IEnumerable<T>>( a =>
            {
                // Materialize first in case the items are a lazy query over
                // this same set, which would otherwise change while enumerating.
                foreach ( var item in a.ToList() )
                {
                    RemoveFromSourceList( sourceList, item );
                }
                return a;
            } );
        }

        /// <summary>
        /// Removes an item from the backing list of a mocked set. The item is
        /// matched by reference first and then by its key values, so an item
        /// that is a copy (such as one returned by <c>AsNoTracking()</c>)
        /// still removes the stored row with the same key, the way EF
        /// identifies rows by key.
        /// </summary>
        /// <typeparam name="T">The type of entity in the list.</typeparam>
        /// <param name="sourceList">The backing list.</param>
        /// <param name="item">The item to be removed.</param>
        private static void RemoveFromSourceList<T>( List<T> sourceList, T item ) where T : class
        {
            if ( sourceList.Remove( item ) )
            {
                return;
            }

            var keyProperties = GetKeyProperties( typeof( T ) );

            if ( keyProperties.Count == 0 )
            {
                return;
            }

            var keyValues = keyProperties.Select( p => p.GetValue( item ) ).ToList();

            // An unsaved entity (Id 0) has no identity yet, so it can only be
            // matched by reference.
            if ( keyValues.Count == 1 && keyValues[0] is int id && id == 0 )
            {
                return;
            }

            var index = sourceList.FindIndex( existing => keyProperties
                .Select( p => p.GetValue( existing ) )
                .SequenceEqual( keyValues ) );

            if ( index >= 0 )
            {
                sourceList.RemoveAt( index );
            }
        }

        /// <summary>
        /// Gets the properties that make up the key of an entity type. These
        /// are the properties decorated with <see cref="System.ComponentModel.DataAnnotations.KeyAttribute"/>,
        /// or <c>Id</c> for an <see cref="IEntity"/> type that declares none.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The key properties, in declaration order; empty if the type has no known key.</returns>
        private static List<System.Reflection.PropertyInfo> GetKeyProperties( Type entityType )
        {
            var keyProperties = entityType.GetProperties()
                .Where( p => p.IsDefined( typeof( System.ComponentModel.DataAnnotations.KeyAttribute ), true ) )
                .ToList();

            if ( keyProperties.Count == 0 && typeof( IEntity ).IsAssignableFrom( entityType ) )
            {
                keyProperties.Add( entityType.GetProperty( nameof( IEntity.Id ) ) );
            }

            return keyProperties;
        }
    }
}

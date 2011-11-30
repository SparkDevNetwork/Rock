using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Rock.Models;
using Rock.Helpers;

namespace Rock.Repository
{
    /// <summary>
    /// Repository for working with the Entity Framework
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityRepository<T> : IRepository<T> where T : class
    {
        private DbContext _context;

        /// <summary>
        /// Gets the context.
        /// </summary>
        internal DbContext Context
        {
            get
            {
                if ( UnitOfWorkScope.CurrentObjectContext != null )
                    return UnitOfWorkScope.CurrentObjectContext;

                if ( _context == null )
                    _context = new Rock.EntityFramework.RockContext();

                return _context;
            }
        }

        internal DbSet<T> _objectSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRepository&lt;T&gt;"/> class.
        /// </summary>
        public EntityRepository() :
            this( new Rock.EntityFramework.RockContext() )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRepository&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="objectContext">The object context.</param>
        public EntityRepository( DbContext objectContext )
        {
            _context = objectContext;
            _objectSet = Context.Set<T>();
        }

        /// <summary>
        /// An <see cref="IQueryable{T}"/> list of entitities
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> AsQueryable()                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        {
            return _objectSet;
        }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> list of all entities
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetAll()
        {
            return _objectSet.ToList();
        }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> list of all entities that match the where clause
        /// </summary>
        /// <param name="where">
        /// <example>An example where clause: <c>t => t.Id == id</c></example>
        /// </param>
        /// <returns></returns>
        public IEnumerable<T> Find( Expression<Func<T, bool>> where )
        {
            return _objectSet.Where( where );
        }

        /// <summary>
        /// Gets the only entity that matches the where clause
        /// </summary>
        /// <remarks>If more than one entity matches the where clause, and exception occurs</remarks>
        /// <param name="where">
        /// <example>An example where clause: <c>t => t.Id == id</c></example>
        /// </param>
        /// <returns></returns>
        public T Single( Expression<Func<T, bool>> where )
        {
            return _objectSet.Single( where );
        }

        /// <summary>
        /// Get's the first entity that matches the where clause
        /// </summary>
        /// <remarks>If an entity that matches the where clause does not exist, an exception occurs</remarks>
        /// <param name="where">
        /// <example>An example where clause: <c>t => t.Id == id</c></example>
        /// </param>
        /// <returns></returns>
        public T First( Expression<Func<T, bool>> where )
        {
            return _objectSet.First( where );
        }

        /// <summary>
        /// Get's the first entity that matches the where clause
        /// </summary>
        /// <remarks>If an entity that matches the where clause does not exist, a null value is returned</remarks>
        /// <param name="where">
        /// <example>An example where clause: <c>t => t.Id == id</c></example>
        /// </param>
        /// <returns></returns>
        public T FirstOrDefault( Expression<Func<T, bool>> where )
        {
            return _objectSet.FirstOrDefault( where );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Delete( T entity )
        {
            _objectSet.Remove( entity );
        }

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Add( T entity )
        {
            _objectSet.Add( entity );
        }

        /// <summary>
        /// Attaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Attach( T entity )
        {
            _objectSet.Attach( entity );
        }

        /// <summary>
        /// Saves the entity and returns a list of any entity changes that 
        /// need to be logged
        /// </summary>
        /// <param name="PersonId">The id of the person making the change</param>
        /// <returns></returns>
        public List<Rock.Models.Core.EntityChange> Save( int? PersonId )
        {
            var entityChanges = new List<Models.Core.EntityChange>();

            _context.ChangeTracker.DetectChanges();

            List<object> addedEntities = new List<object>();
            List<object> deletedEntities = new List<object>();
            List<object> modifiedEntities = new List<object>();

            var contextAdapter = ( ( IObjectContextAdapter )Context );

            foreach ( ObjectStateEntry entry in contextAdapter.ObjectContext.ObjectStateManager.GetObjectStateEntries(
                System.Data.EntityState.Added | System.Data.EntityState.Deleted | System.Data.EntityState.Modified | System.Data.EntityState.Unchanged ) )
            {
                switch ( entry.State )
                {
                    case System.Data.EntityState.Added:

                        entityChanges.Concat( GetEntityChanges( entry.Entity ) );

                        addedEntities.Add( entry.Entity );

                        if ( entry.Entity is IAuditable )
                        {
                            IAuditable auditable = ( IAuditable )entry.Entity;
                            auditable.CreatedByPersonId = PersonId;
                            auditable.CreatedDateTime = DateTime.Now;
                            auditable.ModifiedByPersonId = PersonId;
                            auditable.ModifiedDateTime = DateTime.Now;
                        }
                        break;

                    case System.Data.EntityState.Deleted:
                        deletedEntities.Add( entry.Entity );
                        break;

                    case System.Data.EntityState.Modified:

                        var model = entry.Entity as Model<T>;
                        if ( model != null )
                        {
                            bool cancel = false;
                            model.RaiseUpdatingEvent( out cancel, PersonId );
                            if ( cancel )
                            {
                                contextAdapter.ObjectContext.Detach( entry );
                            }
                            else
                            {
                                entityChanges.Concat( GetEntityChanges( model ) );

                                modifiedEntities.Add( entry.Entity );

                                if ( model is IAuditable )
                                {
                                    IAuditable auditable = ( IAuditable )model;
                                    auditable.ModifiedByPersonId = PersonId;
                                    auditable.ModifiedDateTime = DateTime.Now;
                                }
                            }
                        }

                        break;
                }
            }

            Context.SaveChanges();

            foreach ( object modifiedEntity in addedEntities )
            {
                var model = modifiedEntity as Model<T>;
                if (model != null)
                    model.RaiseAddedEvent( PersonId );
            }

            foreach ( object deletedEntity in deletedEntities )
            {
                var model = deletedEntity as Model<T>;
                if ( model != null )
                    model.RaiseDeletedEvent( PersonId );
            }

            foreach ( object modifiedEntity in modifiedEntities )
            {
                var model = modifiedEntity as Model<T>;
                if ( model != null )
                    model.RaiseUpdatedEvent( PersonId );
            }

            return entityChanges;
        }

        private List<Rock.Models.Core.EntityChange> GetEntityChanges( object entity )
        {
            List<Rock.Models.Core.EntityChange> entityChanges = new List<Models.Core.EntityChange>();

            // Do not track changes on the 'EntityChange' entity type. 
            if ( !( entity is Rock.Models.Core.EntityChange ) )
            {
                Type entityType = entity.GetType();

                Guid changeSet = Guid.NewGuid();

                // Look for properties that have the "TrackChanges" attribute
                foreach ( PropertyInfo propInfo in entity.GetType().GetProperties() )
                {
                    if ( TrackChanges( propInfo.GetCustomAttributes( true ) ) )
                    {
                        var currentValue = Context.Entry( entity ).Property( propInfo.Name ).CurrentValue;
                        var originalValue = Context.Entry( entity ).State != System.Data.EntityState.Added ?
                            Context.Entry( entity ).Property( propInfo.Name ).OriginalValue : string.Empty;

                        if ( currentValue.ToString() != originalValue.ToString() )
                        {
                            if ( entityChanges == null )
                                entityChanges = new List<Models.Core.EntityChange>();

                            Rock.Models.Core.EntityChange change = new Models.Core.EntityChange();
                            change.ChangeSet = changeSet;
                            change.ChangeType = Context.Entry( entity ).State.ToString();
                            change.EntityType = entityType.Name;
                            change.Property = propInfo.Name;
                            change.OriginalValue = originalValue.ToString();
                            change.CurrentValue = currentValue.ToString();

                            entityChanges.Add( change );
                        }
                    }
                }
            }

            return entityChanges;
        }

        private static bool TrackChanges( Object[] customAttributes )
        {
            foreach ( object attribute in customAttributes )
                if ( attribute is Rock.Models.TrackChangesAttribute )
                    return true;
            return false;
        }
    }
}

//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Repository for working with the Entity Framework
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFRepository<T> : IRepository<T>, IDisposable
        where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// 
        /// </summary>
        private bool IsDisposed;

        /// <summary>
        /// 
        /// </summary>
        private DbContext _context;

        /// <summary>
        /// Gets the context.
        /// </summary>
        internal DbContext Context
        {
            get
            {
                if ( UnitOfWorkScope.CurrentObjectContext != null )
                {
                    return UnitOfWorkScope.CurrentObjectContext;
                }

                if ( _context == null )
                {
                    _context = new RockContext();
                }

                return _context;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal DbSet<T> _objectSet;
        internal DbSet<Audit> _auditSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="EFRepository&lt;T&gt;"/> class.
        /// </summary>
        public EFRepository() :
            this( new RockContext() )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EFRepository&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="objectContext">The object context.</param>
        public EFRepository( DbContext objectContext )
        {
            IsDisposed = false;
            _context = objectContext;
            _objectSet = Context.Set<T>();
            _auditSet = Context.Set<Audit>();
        }

        /// <summary>
        /// An <see cref="IQueryable{T}"/> list of entitities
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> AsQueryable()
        {
            return _objectSet;
        }

        /// <summary>
        /// An <see cref="IQueryable{T}"/> list of entitities
        /// with a eager load of includes properties
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> AsQueryable( string includes )
        {
            DbQuery<T> value = _objectSet;
            if ( !String.IsNullOrEmpty( includes ) )
            {
                foreach ( var include in includes.SplitDelimitedValues() )
                {
                    value = value.Include( include );
                }
            }
            return value;
        }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> list of all entities
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAll()
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
        public virtual IEnumerable<T> Find( Expression<Func<T, bool>> where )
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
        public virtual T Single( Expression<Func<T, bool>> where )
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
        public virtual T First( Expression<Func<T, bool>> where )
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
        public virtual T FirstOrDefault( Expression<Func<T, bool>> where )
        {
            return _objectSet.FirstOrDefault( where );
        }

        /// <summary>
        /// Date the entity was created.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual DateTime? DateCreated( T entity )
        {
            return DateCreated( entity.TypeId, entity.Id );
        }

        /// <summary>
        /// Date the entity was created.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual DateTime? DateCreated( int entityTypeId, int entityId )
        {
            return _auditSet
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.EntityId == entityId &&
                    a.AuditType == AuditType.Add
                )
                .OrderByDescending( a => a.DateTime )
                .Select( a => a.DateTime )
                .FirstOrDefault();
        }

        /// <summary>
        /// Date the entity was last modified.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual DateTime? DateLastModified( T entity )
        {
            return DateLastModified( entity.TypeId, entity.Id );
        }

        /// <summary>
        /// Date the entity was last modified.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual DateTime? DateLastModified( int entityTypeId, int entityId )
        {
            return _auditSet
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.EntityId == entityId &&
                    ( a.AuditType == AuditType.Modify && a.AuditType == AuditType.Add )
                )
                .OrderByDescending( a => a.DateTime )
                .Select( a => a.DateTime )
                .FirstOrDefault();
        }

        /// <summary>
        /// The person id who created entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual int? CreatedByPersonId( T entity )
        {
            return CreatedByPersonId( entity.TypeId, entity.Id );
        }

        /// <summary>
        /// The person id who created entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual int? CreatedByPersonId( int entityTypeId, int entityId )
        {
            return _auditSet
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.EntityId == entityId &&
                    a.AuditType == AuditType.Add
                )
                .OrderByDescending( a => a.DateTime )
                .Select( a => a.PersonId )
                .FirstOrDefault();
        }

        /// <summary>
        /// The person id who last modified the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual int? LastModifiedByPersonId( T entity )
        {
            return LastModifiedByPersonId( entity.TypeId, entity.Id );
        }

        /// <summary>
        /// The person id who last modified the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual int? LastModifiedByPersonId( int entityTypeId, int entityId )
        {
            return _auditSet
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.EntityId == entityId &&
                    ( a.AuditType == AuditType.Modify || a.AuditType == AuditType.Add )
                )
                .OrderByDescending( a => a.DateTime )
                .Select( a => a.PersonId )
                .FirstOrDefault();
        }

        /// <summary>
        /// All the audits made to the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual IQueryable<Audit> Audits( T entity )
        {
            return Audits( entity.TypeId, entity.Id );
        }

        /// <summary>
        /// All the audits made to the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual IQueryable<Audit> Audits( int entityTypeId, int entityId )
        {
            return _auditSet
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.EntityId == entityId
                );
        }

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Add( T entity )
        {
            _objectSet.Add( entity );
        }

        /// <summary>
        /// Attaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Attach( T entity )
        {
            _objectSet.Attach( entity );
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="targetItem">The target item.</param>
        /// <param name="sourceItem">The source item.</param>
        public virtual void SetValues( T sourceItem, T targetItem )
        {
            _context.Entry( targetItem ).CurrentValues.SetValues( sourceItem );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Delete( T entity )
        {
            _objectSet.Remove( entity );
        }

        /// <summary>
        /// Saves the entity and returns a list of any entity changes that
        /// need to be logged
        /// </summary>
        /// <param name="PersonId">The id of the person making the change</param>
        /// <param name="audits">The audits.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool Save( int? PersonId, out List<Audit> audits, out List<string> errorMessages )
        {
            audits = new List<Audit>();
            errorMessages = new List<string>();

            Context.ChangeTracker.DetectChanges();

            List<object> addedEntities = new List<object>();
            List<object> deletedEntities = new List<object>();
            List<object> modifiedEntities = new List<object>();

            var contextAdapter = ( (IObjectContextAdapter)Context );

            foreach ( ObjectStateEntry entry in contextAdapter.ObjectContext.ObjectStateManager.GetObjectStateEntries(
                EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged ) )
            {
                var rockEntity = entry.Entity as Entity<T>;
                var audit = new Rock.Model.Audit();

                switch ( entry.State )
                {
                    case EntityState.Added:
                        addedEntities.Add( entry.Entity );
                        audit.AuditType = AuditType.Add;
                        break;

                    case EntityState.Deleted:
                        deletedEntities.Add( entry.Entity );
                        audit.AuditType = AuditType.Delete;
                        break;

                    case EntityState.Modified:

                        if ( rockEntity != null )
                        {
                            bool cancel = false;
                            rockEntity.RaiseUpdatingEvent( out cancel, PersonId );
                            if ( cancel )
                            {
                                errorMessages.Add( string.Format( "Update cancelled by {0} event handler", rockEntity.TypeName ) );
                                contextAdapter.ObjectContext.Detach( entry );
                            }
                            else
                            {
                                modifiedEntities.Add( entry.Entity );
                                audit.AuditType = AuditType.Modify;
                            }
                        }

                        break;
                }

                if ( rockEntity != null )
                {
                    Type rockEntityType = rockEntity.GetType();
                    if ( rockEntityType.Namespace == "System.Data.Entity.DynamicProxies" )
                    {
                        rockEntityType = rockEntityType.BaseType;
                    }

                    if ( AuditClass( rockEntityType ) )
                    {
                        var dbEntity = Context.Entry( entry.Entity );

                        var modifiedProperties = new List<string>();
                        PropertyInfo[] properties = rockEntityType.GetProperties();

                        foreach ( PropertyInfo propInfo in properties )
                        {
                            if ( AuditProperty( propInfo ) )
                            {
                                var dbPropertyEntry = dbEntity.Property( propInfo.Name );
                                if ( dbPropertyEntry != null && dbPropertyEntry.IsModified )
                                {
                                    modifiedProperties.Add( propInfo.Name );
                                }
                            }
                        }

                        if ( modifiedProperties.Count > 0 )
                        {
                            var entityType = Rock.Web.Cache.EntityTypeCache.Read( rockEntity.TypeName, false );
                            if ( entityType != null )
                            {
                                audit.DateTime = DateTime.Now;
                                audit.PersonId = PersonId;
                                audit.EntityTypeId = entityType.Id;
                                audit.EntityId = rockEntity.Id;
                                audit.Title = rockEntity.ToString().Truncate( 195 );
                                audit.Properties = modifiedProperties.AsDelimited( ";" );
                                audits.Add( audit );
                            }
                        }
                    }
                }
            }

            if ( errorMessages.Count > 0 )
            {
                return false;
            }

            Context.SaveChanges();

            foreach ( object modifiedEntity in addedEntities )
            {
                var model = modifiedEntity as Entity<T>;
                if ( model != null )
                {
                    model.RaiseAddedEvent( PersonId );
                }
            }

            foreach ( object deletedEntity in deletedEntities )
            {
                var model = deletedEntity as Entity<T>;
                if ( model != null )
                {
                    model.RaiseDeletedEvent( PersonId );
                }
            }

            foreach ( object modifiedEntity in modifiedEntities )
            {
                var model = modifiedEntity as Entity<T>;
                if ( model != null )
                {
                    model.RaiseUpdatedEvent( PersonId );
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a raw sql query that will return entities
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<T> ExecuteQuery( string query, params object[] parameters )
        {
            return _objectSet.SqlQuery( query, parameters );
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetConfigurationValue( string key, string value )
        {
            switch ( key.ToUpper() )
            {
                case "PROXYCREATIONENABLED":

                    bool enabled = true;
                    if ( Boolean.TryParse( value, out enabled ) )
                    {
                        Context.Configuration.ProxyCreationEnabled = enabled;
                    }
                    break;
            }
        }

        /// <summary>
        /// Audits the class.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <returns></returns>
        private bool AuditClass( Type baseType )
        {
            var attribute = baseType.GetCustomAttribute( typeof( NotAuditedAttribute ) );
            return ( attribute == null );
        }

        /// <summary>
        /// Audits the property.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns></returns>
        private bool AuditProperty( PropertyInfo propertyInfo )
        {
            if ( propertyInfo.GetCustomAttribute( typeof( NotAuditedAttribute ) ) == null &&
                ( !propertyInfo.GetGetMethod().IsVirtual || propertyInfo.Name == "Id" || propertyInfo.Name == "Guid" || propertyInfo.Name == "Order" ) )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose( bool disposing )
        {
            if ( !IsDisposed )
            {
                if ( disposing )
                {
                    if ( _context != null )
                    {
                        _context.Dispose();
                    }
                }

                _context = null;
                IsDisposed = true;
            }
        }

    }

    /// <summary>
    /// Entity Framework repository for providing non entity specific methods
    /// </summary>
    public class EFRepository : IRepository, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private bool IsDisposed;

        /// <summary>
        /// 
        /// </summary>
        private DbContext _context;

        /// <summary>
        /// Gets the context.
        /// </summary>
        internal DbContext Context
        {
            get
            {
                if ( UnitOfWorkScope.CurrentObjectContext != null )
                {
                    return UnitOfWorkScope.CurrentObjectContext;
                }

                if ( _context == null )
                {
                    _context = new RockContext();
                }

                return _context;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EFRepository&lt;T&gt;"/> class.
        /// </summary>
        public EFRepository() :
            this( new RockContext() )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EFRepository&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="objectContext">The object context.</param>
        public EFRepository( DbContext objectContext )
        {
            IsDisposed = false;
            _context = objectContext;
        }

        /// <summary>
        /// Creates a raw SQL query that will return elements of the given type.  The
        /// type can be any type that has properties that match the names of the columns
        /// returned from the query, or can be a simple primitive type. The type does
        /// not have to be an entity type. The results of this query are never tracked
        /// by the context even if the type of object returned is an entity type. Use
        /// the SqlQuery(System.String,System.Object[]) method
        /// to return entities that are tracked by the context.
        /// </summary>
        /// <param name="elementType">Type of the element.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IEnumerable ExecuteQuery( Type elementType, string query, params object[] parameters )
        {
            return _context.Database.SqlQuery( elementType, query, parameters );
        }

        /// <summary>
        /// Gets a data adapter.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IDataAdapter GetDataAdapter( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            SqlCommand sqlCommand = GetCommand( query, commandType, parameters );
            if ( sqlCommand != null )
            {
                return new SqlDataAdapter( sqlCommand );
            }

            return null;
        }


        /// <summary>
        /// Gets the data set.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataSet GetDataSet( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            SqlCommand sqlCommand = GetCommand( query, commandType, parameters );
            if ( sqlCommand != null )
            {
                SqlDataAdapter adapter =new SqlDataAdapter( sqlCommand );
                DataSet dataSet = new DataSet( "rockDs" );
                adapter.Fill( dataSet );
                return dataSet;
            }

            return null;
        }

        /// <summary>
        /// Gets the data table.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataTable GetDataTable( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            DataSet dataSet = GetDataSet( query, commandType, parameters );
            if ( dataSet.Tables.Count > 0 )
            {
                return dataSet.Tables[0];
            }

            return null;
        }

        /// <summary>
        /// Gets the data set.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IDataReader GetDataReader( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            SqlCommand sqlCommand = GetCommand( query, commandType, parameters );
            if (sqlCommand != null)
            {
                return sqlCommand.ExecuteReader();
            }
            return null;
        }

        private SqlCommand GetCommand( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            if ( _context.Database.Connection is SqlConnection )
            {
                SqlConnection sqlConnection = (SqlConnection)_context.Database.Connection;

                SqlCommand sqlCommand = new SqlCommand( query, sqlConnection );
                sqlCommand.CommandType = commandType;
                if ( parameters != null )
                {
                    foreach ( var parameter in parameters )
                    {
                        SqlParameter sqlParam = new SqlParameter();
                        sqlParam.ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key;
                        sqlParam.Value = parameter.Value;
                        sqlCommand.Parameters.Add( sqlParam );
                    }
                }

                return sqlCommand;
            }

            return null;
        }

        /// <summary>
        /// Executes a SQL command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int ExecuteCommand( string command, params object[] parameters )
        {
            return _context.Database.ExecuteSqlCommand( command, parameters );
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose( bool disposing )
        {
            if ( !IsDisposed )
            {
                if ( disposing )
                {
                    if ( _context != null )
                    {
                        _context.Dispose();
                    }
                }

                _context = null;
                IsDisposed = true;
            }
        }
    }
}

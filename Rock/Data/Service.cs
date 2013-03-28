//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Rock.Model;
using Rock.Workflow;

namespace Rock.Data
{
    /// <summary>
    /// Generic POCO service class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Service<T> where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Gets or sets the save messages.
        /// </summary>
        /// <value>
        /// The save messages.
        /// </value>
        public virtual List<string> ErrorMessages { get; set; }

        private IRepository<T> _repository;
        /// <summary>
        /// Gets the Repository.
        /// </summary>
        public IRepository<T> Repository
        {
            get { return _repository; }
        }

        /// <summary>
        /// Gets a LINQ expression parameter.
        /// </summary>
        /// <value>
        /// The parameter expression.
        /// </value>
        public ParameterExpression ParameterExpression
        {
            get
            {
                return Expression.Parameter( typeof( T ), "p" );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service&lt;T&gt;"/> class.
        /// </summary>
        public Service()
        {
            var factory = new RepositoryFactory<T>();
            _repository = factory.FindRepository();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public Service( IRepository<T> repository )
        {
            _repository = repository;
        }

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> Queryable()
        {
            return _repository.AsQueryable();
        }

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models
        /// with eager loading of properties specified in includes
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> Queryable( string includes )
        {
            return _repository.AsQueryable( includes );
        }

        /// <summary>
        /// Gets the model with the id value
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public virtual T Get( int id )
        {
            return _repository.FirstOrDefault( t => t.Id == id );
        }

        /// <summary>
        /// Gets the model with the Guid value
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public virtual T Get( Guid guid )
        {
            return _repository.FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets a list of items that match the specified expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public IQueryable<T> Get( ParameterExpression parameterExpression, Expression whereExpression )
        {
            return Get( parameterExpression, whereExpression, null );
        }

        /// <summary>
        /// Gets a list of items that match the specified expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public IQueryable<T> Get( ParameterExpression parameterExpression, Expression whereExpression, Rock.Web.UI.Controls.SortProperty sortProperty )
        {
            if ( parameterExpression != null && whereExpression != null )
            {
                var lambda = Expression.Lambda<Func<T, bool>>( whereExpression, parameterExpression );
                var queryable = this.Queryable().Where( lambda );
                return sortProperty != null ? queryable.Sort( sortProperty ) : queryable;
            }

            return this.Queryable();
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public List<T> GetList( ParameterExpression parameterExpression, Expression whereExpression, Rock.Web.UI.Controls.SortProperty sortProperty)
        {
            return Get( parameterExpression, whereExpression, sortProperty ).ToList();
        }

        /// <summary>
        /// Anies the specified parameter expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public bool Any( ParameterExpression parameterExpression, Expression whereExpression )
        {
            var lambda = Expression.Lambda<Func<T, bool>>( whereExpression, parameterExpression );
            return this.Queryable().Any( lambda );
        }

        /// <summary>
        /// Trys to get the model with the id value
        /// </summary>
        /// <returns></returns>
        public virtual bool TryGet( int id, out T item )
        {
            item = Get( id );
            if ( item == null )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the model by the public encrypted key.
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <returns></returns>
        public virtual T GetByEncryptedKey( string encryptedKey )
        {
            string publicKey = Rock.Security.Encryption.DecryptString( encryptedKey );
            return GetByPublicKey( publicKey );
        }

        /// <summary>
        /// Gets the model by the public un-encrypted key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <returns></returns>
        public virtual T GetByPublicKey( string publicKey )
        {
            try
            {
                string[] idParts = publicKey.Split( '>' );
                if ( idParts.Length == 2 )
                {
                    int id = Int32.Parse( idParts[0] );
                    Guid guid = new Guid( idParts[1] );

                    T model = Get( id );

                    if ( model != null && model.Guid.CompareTo( guid ) == 0 )
                    {
                        return model;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /*
        /// <summary>
        /// Gets the current model from the page instance context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        //public T GetCurrent( Rock.Web.Cache.RockPage pageInstance )
        //{
        //    string key = typeof( T ).FullName;

        //    if ( pageInstance.Context.ContainsKey( key ) )
        //    {
        //        var keyModel = pageInstance.Context[key];
        //        if ( keyModel.Model == null )
        //        {
        //            keyModel.Model = GetByPublicKey( keyModel.Key );
        //            if ( keyModel.Model is Rock.Attribute.IHasAttributes )
        //                Rock.Attribute.Helper.LoadAttributes( keyModel.Model as Rock.Attribute.IHasAttributes );
        //        }

        //        return keyModel.Model as T;
        //    }

        //    return null;
        //}
         */

        /// <summary>
        /// Date the entity was created.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual DateTime? DateCreated( T entity )
        {
            return _repository.DateCreated( entity );
        }

        /// <summary>
        /// Date the entity was created.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual DateTime? DateCreated( int entityTypeId, int entityId )
        {
            return _repository.DateCreated( entityTypeId, entityId );
        }

        /// <summary>
        /// Date the entity was last modified.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual DateTime? DateLastModified( T entity )
        {
            return _repository.DateLastModified( entity );
        }

        /// <summary>
        /// Date the entity was last modified.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual DateTime? DateLastModified( int entityTypeId, int entityId )
        {
            return _repository.DateLastModified( entityTypeId, entityId );
        }

        /// <summary>
        /// The person id who created entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual int? CreatedByPersonId( T entity )
        {
            return _repository.CreatedByPersonId( entity );
        }

        /// <summary>
        /// The person id who created entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual int? CreatedByPersonId( int entityTypeId, int entityId )
        {
            return _repository.CreatedByPersonId( entityTypeId, entityId );
        }

        /// <summary>
        /// The person id who last modified the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual int? LastModifiedByPersonId( T entity )
        {
            return _repository.LastModifiedByPersonId( entity );
        }

        /// <summary>
        /// The person id who last modified the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual int? LastModifiedByPersonId( int entityTypeId, int entityId )
        {
            return _repository.LastModifiedByPersonId( entityTypeId, entityId );
        }

        /// <summary>
        /// All the audits made to the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual IQueryable<Audit> Audits( T entity )
        {
            return _repository.Audits( entity );
        }

        /// <summary>
        /// All the audits made to the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual IQueryable<Audit> Audits( int entityTypeId, int entityId )
        {
            return _repository.Audits( entityTypeId, entityId );
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public virtual bool Add( T item, int? personId )
        {
            bool cancel = false;
            item.RaiseAddingEvent( out cancel, personId );
            if ( !cancel )
            {
                _repository.Add( item );
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Attaches the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public virtual void Attach( T item )
        {
            _repository.Attach( item );
        }

        /// <summary>
        /// Triggers the workflows.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        private bool TriggerWorkflows( IEntity entity, WorkflowTriggerType triggerType, int? personId )
        {
            foreach ( var trigger in TriggerCache.Triggers( entity.TypeName, triggerType ) )
            {
                if ( triggerType == WorkflowTriggerType.PreSave || triggerType == WorkflowTriggerType.PreDelete )
                {
                    var workflowTypeService = new WorkflowTypeService();
                    var workflowType = workflowTypeService.Get( trigger.WorkflowTypeId );

                    if ( workflowType != null )
                    {
                        var workflow = Rock.Model.Workflow.Activate( workflowType, trigger.WorkflowName );

                        List<string> workflowErrors;
                        if ( !workflow.Process( entity, out workflowErrors ) )
                        {
                            ErrorMessages.AddRange( workflowErrors );
                            return false;
                        }
                        else
                        {
                            if ( workflowType.IsPersisted )
                            {
                                var workflowService = new Rock.Model.WorkflowService();
                                workflowService.Add( workflow, personId );
                                workflowService.Save( workflow, personId );
                            }
                        }
                    }
                }
                else
                {
                    var transaction = new Rock.Transactions.WorkflowTriggerTransaction();
                    transaction.Trigger = trigger;
                    transaction.Entity = entity.Clone();
                    transaction.PersonId = personId;
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                }
            }
            return true;
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public virtual bool Delete( T item, int? personId )
        {
            ErrorMessages = new List<string>();

            if ( !TriggerWorkflows( item, WorkflowTriggerType.PreDelete, personId ) )
            {
                return false;
            }

            bool cancel = false;
            item.RaiseDeletingEvent( out cancel, personId );
            if ( !cancel )
            {
                _repository.Delete( item );

                TriggerWorkflows( item, WorkflowTriggerType.PostDelete, personId );

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public virtual bool Save( T item, int? personId )
        {
            ErrorMessages = new List<string>();

            if ( !TriggerWorkflows( item, WorkflowTriggerType.PreSave, personId ) )
            {
                return false;
            }

            if ( item != null && item.Guid == Guid.Empty )
                item.Guid = Guid.NewGuid();

            List<Audit> audits;
            List<string> errorMessages;

            if ( _repository.Save( personId, out audits, out errorMessages ) )
            {
                if ( audits != null && audits.Count > 0 )
                {
                    var transaction = new Rock.Transactions.AuditTransaction();
                    transaction.Audits = audits;
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                }

                TriggerWorkflows( item, WorkflowTriggerType.PostSave, personId );

                return true;
            }
            else
            {
                ErrorMessages = errorMessages;
                return false;
            }
        }

        /// <summary>
        /// Reorders the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        /// <param name="personId">The person id.</param>
        public virtual void Reorder( List<T> items, int oldIndex, int newIndex, int? personId )
        {
            T movedItem = items[oldIndex];
            items.RemoveAt( oldIndex );
            if ( newIndex >= items.Count )
                items.Add( movedItem );
            else
                items.Insert( newIndex, movedItem );

            int order = 0;
            foreach ( T item in items )
            {
                IOrdered orderedItem = item as IOrdered;
                if ( orderedItem != null )
                {
                    if ( orderedItem.Order != order )
                    {
                        orderedItem.Order = order;
                        Save( item, personId );
                    }
                }
                order++;
            }
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
            return _repository.ExecuteQuery( query, parameters );
        }

    }

    /// <summary>
    /// Service class for non entity specific methods
    /// </summary>
    public class Service
    {
        private IRepository _repository;
        /// <summary>
        /// Gets the Repository.
        /// </summary>
        public IRepository Repository
        {
            get { return _repository; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service&lt;T&gt;"/> class.
        /// </summary>
        public Service()
        {
            var factory = new RepositoryFactory();
            _repository = factory.FindRepository();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public Service( IRepository repository )
        {
            _repository = repository;
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
            return _repository.ExecuteQuery( elementType, query, parameters );
        }

        /// <summary>
        /// Gets a data set.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataSet GetDataSet( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            return _repository.GetDataSet( query, commandType, parameters );
        }

        /// <summary>
        /// Gets a data table.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataTable GetDataTable( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            return _repository.GetDataTable( query, commandType, parameters );
        }

        /// <summary>
        /// Gets a data reader.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IDataReader GetDataReader( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            return _repository.GetDataReader( query, commandType, parameters );
        }

        /// <summary>
        /// Executes the SQL command.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int ExecuteCommand( string query, params object[] parameters )
        {
            return _repository.ExecuteCommand( query, parameters );
        }

    }

}
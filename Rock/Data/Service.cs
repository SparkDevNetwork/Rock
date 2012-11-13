//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Core;

namespace Rock.Data
{
    /// <summary>
    /// Generic POCO service class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Service<T> where T : Rock.Data.Entity<T>
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
        protected IRepository<T> Repository
        {
            get { return _repository; }
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
        public IQueryable<T> Queryable()
        {
            return _repository.AsQueryable();
        }

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models
        /// with eager loading of properties specified in includes
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> Queryable( string includes )
        {
            return _repository.AsQueryable( includes );
        }

        /// <summary>
        /// Gets the model with the id value
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public T Get( int id )
        {
            return _repository.FirstOrDefault( t => t.Id == id );
        }

        /// <summary>
        /// Gets the model with the Guid value
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public T Get( Guid guid )
        {
            return _repository.FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Trys to get the model with the id value
        /// </summary>
        /// <returns></returns>
        public bool TryGet( int id, out T item )
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
        public T GetByPublicKey( string publicKey )
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
        private bool TriggerWorkflows( IEntity entity, EntityTriggerType triggerType, int? personId )
        {
            var triggerService = new EntityTypeWorkflowTriggerService();
            foreach ( var trigger in triggerService.Get( entity.TypeName, triggerType ) )
            {
                var workflow = Rock.Util.Workflow.Activate( trigger.WorkflowType, trigger.WorkflowName );

                List<string> workflowErrors;
                if ( !workflow.Process( entity, out workflowErrors ) )
                {
                    ErrorMessages.AddRange( workflowErrors );
                    return false;
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

            if ( !TriggerWorkflows( item, EntityTriggerType.PreDelete, personId ) )
            {
                return false;
            }

            bool cancel = false;
            item.RaiseDeletingEvent( out cancel, personId );
            if ( !cancel )
            {
                _repository.Delete( item );

                TriggerWorkflows( item, EntityTriggerType.PostDelete, personId );

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

            if ( !TriggerWorkflows( item, EntityTriggerType.PreSave, personId ) )
            {
                return false;
            }

            if ( item != null && item.Guid == Guid.Empty )
                item.Guid = Guid.NewGuid();

            List<EntityChange> changes;
            List<AuditDto> audits;
            List<string> errorMessages;

            if ( _repository.Save( personId, out changes, out audits, out errorMessages ) )
            {
                if ( changes != null && changes.Count > 0 )
                {
                    var transaction = new Rock.Transactions.EntityChangeTransaction();
                    transaction.Changes = changes;
                    transaction.PersonId = personId;
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                }

                if ( audits != null && audits.Count > 0 )
                {
                    var transaction = new Rock.Transactions.AuditTransaction();
                    transaction.Audits = audits;
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                }

                TriggerWorkflows( item, EntityTriggerType.PostSave, personId );

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
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    public class Service<T, D> : Service<T>
        where T : Rock.Data.Entity<T>
        where D : Rock.Data.IDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Service{D}" /> class.
        /// </summary>
        public Service() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service{D}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public Service( IRepository<T> repository ) : base( repository ) { }

        /// <summary>
        /// Gets an <see cref="IQueryable{D}"/> list of DTO objects
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<D> QueryableDto()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates the new.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual T CreateNew()
        {
            throw new System.NotImplementedException();
        }
    }

}
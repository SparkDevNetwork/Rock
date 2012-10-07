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
		public IQueryable<T> Queryable(string includes)
		{
			return _repository.AsQueryable(includes);
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
        /// Trys to get the model with the id value
        /// </summary>
        /// <returns></returns>
        public bool TryGet( int id, out T item )
        {
            item = Get(id);
            if (item == null)
                return false;
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
                        return model;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current model from the page instance context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        //public T GetCurrent( Rock.Web.Cache.Page pageInstance )
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

		/// <summary>
		/// Date the entity was created.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public virtual DateTimeOffset? DateCreated( T entity )
		{
			return _repository.DateCreated( entity );
		}

		/// <summary>
		/// Date the entity was created.
		/// </summary>
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		public virtual DateTimeOffset? DateCreated( string entityTypeName, int entityId )
		{
			return _repository.DateCreated( entityTypeName, entityId );
		}

		/// <summary>
		/// Date the entity was last modified.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public virtual DateTimeOffset? DateLastModified( T entity )
		{
			return _repository.DateLastModified( entity );
		}

		/// <summary>
		/// Date the entity was last modified.
		/// </summary>
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		public virtual DateTimeOffset? DateLastModified( string entityTypeName, int entityId )
		{
			return _repository.DateLastModified( entityTypeName, entityId );
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
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		public virtual int? CreatedByPersonId( string entityTypeName, int entityId )
		{
			return _repository.CreatedByPersonId( entityTypeName, entityId );
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
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		public virtual int? LastModifiedByPersonId( string entityTypeName, int entityId )
		{
			return _repository.LastModifiedByPersonId( entityTypeName, entityId );
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
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		public virtual IQueryable<Audit> Audits( string entityTypeName, int entityId )
		{
			return _repository.Audits( entityTypeName, entityId );
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
				return false;
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
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
		public virtual bool Delete( T item, int? personId )
        {
            bool cancel = false;
            item.RaiseDeletingEvent( out cancel, personId );
            if ( !cancel )
            {
                _repository.Delete( item );
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
		public virtual void Save( T item, int? personId )
        {
            if ( item != null && item.Guid == Guid.Empty )
                item.Guid = Guid.NewGuid();

			var audits = new List<Core.AuditDto>();
            var entityChanges = _repository.Save( personId, audits );

			if ( entityChanges != null && entityChanges.Count > 0 )
            {
                var transaction = new Rock.Transactions.EntityChangeTransaction();
                transaction.Changes = entityChanges;
                transaction.PersonId = personId;
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
            }

			if ( audits != null && audits.Count > 0 )
			{
				var transaction = new Rock.Transactions.AuditTransaction();
				transaction.Audits = audits;
				Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
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

    public class Service<T, D> : Service<T>
        where T : Rock.Data.Entity<T>
        where D : Rock.Data.IDto
    {
        public Service() : base() { }
        public Service( IRepository<T> repository ) : base( repository ) { }

        /// <summary>
        /// Gets an <see cref="IQueryable{D}"/> list of DTO objects
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<D> QueryableDto()
        {
            throw new System.NotImplementedException();
        }

        public virtual T CreateNew()
        {
            throw new System.NotImplementedException();
        }
    }

}
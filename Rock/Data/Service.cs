//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Data
{
    /// <summary>
    /// Generic POCO service class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Service<T> where T : Rock.Data.Model<T>
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
            : this( new EFRepository<T>() )
        { }

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
        /// Gets the model with the id value
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public T Get( int id )
        {
            return _repository.FirstOrDefault( t => t.Id == id );
        }

        /// <summary>
        /// Gets the model by the public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <returns></returns>
        public virtual T GetByPublicKey( string publicKey )
        {
            try
            {
                string identifier = Rock.Security.Encryption.DecryptString( publicKey );

                string[] idParts = identifier.Split( '>' );
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
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public bool Add( T item, int? personId )
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
        public void Attach( T item )
        {
            _repository.Attach( item );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public bool Delete( T item, int? personId )
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
        public void Save( T item, int? personId )
        {
            if ( item != null && item.Guid == Guid.Empty )
                item.Guid = Guid.NewGuid();

            List<Rock.Core.EntityChange> entityChanges = _repository.Save( personId );

            if ( entityChanges != null && entityChanges.Count > 0 )
            {
                Core.EntityChangeService entityChangeService = new Core.EntityChangeService();
                foreach ( Rock.Core.EntityChange entityChange in entityChanges )
                {
                    entityChangeService.Add( entityChange, personId );
                    entityChangeService.Save( entityChange, personId );
                }
            }
        }

        /// <summary>
        /// Reorders the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        /// <param name="personId">The person id.</param>
        public void Reorder( List<T> items, int oldIndex, int newIndex, int? personId )
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
}
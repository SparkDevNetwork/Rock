// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

using Rock.Web.Cache;
using Z.EntityFramework.Plus;

namespace Rock.Data
{
    /// <summary>
    /// Generic POCO service class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Service<T> : IService where T : Rock.Data.Entity<T>, new()
    {

        #region Fields

        private DbContext _context;
        internal DbSet<T> _objectSet;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public DbContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        /// <summary>
        /// Gets or sets the save messages.
        /// </summary>
        /// <value>
        /// The save messages.
        /// </value>
        public virtual List<string> ErrorMessages { get; set; }

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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Service{T}"/> class.
        /// </summary>
        /// <param name="dbContext">The db context.</param>
        public Service( Rock.Data.DbContext dbContext )
        {
            _context = dbContext;
            _objectSet = _context.Set<T>();
        }

        #endregion

        #region Methods

        #region Queryable

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> Queryable()
        {
            return _objectSet;
        }

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models ensuring that any EntityFramework.Plus Filter or service specific filter is not applied
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> AsNoFilter()
        {
            return _objectSet.AsNoFilter();
        }

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models ensuring that any EntityFramework.Plus Filter or service specific filter is not applied
        /// with eager loading of the comma-delimited properties specified in includes
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> AsNoFilter( string includes )
        {
            return QueryableIncludes( _objectSet.AsNoFilter(), includes );
        }

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models
        /// with eager loading of the comma-delimited properties specified in includes
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> Queryable( string includes )
        {
            return QueryableIncludes( _objectSet as IQueryable<T>, includes );
        }

        /// <summary>
        /// Applies a comma-delimited list of includes to the Queryable
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="includes">The includes.</param>
        /// <returns></returns>
        private static IQueryable<T> QueryableIncludes( IQueryable<T> query, string includes )
        {
            if ( !String.IsNullOrEmpty( includes ) )
            {
                foreach ( var include in includes.SplitDelimitedValues() )
                {
                    query = query.Include( include );
                }
            }
            return query;
        }

        #endregion

        #region Get Methods

        /// <summary>
        /// Gets the model with the id value
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public virtual T Get( int id )
        {
            return AsNoFilter().FirstOrDefault( t => t.Id == id );
        }

        /// <summary>
        /// Gets the model with the Guid value
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public virtual T Get( Guid guid )
        {
            return AsNoFilter().FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets the model with the id value but doesn't load it into the EF ChangeTracker.
        /// Use this if you won't be making any changes to the record
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public virtual T GetNoTracking( int id )
        {
            return AsNoFilter().AsNoTracking().FirstOrDefault( t => t.Id == id );
        }

        /// <summary>
        /// Gets the model with the Guid value but doesn't load it into the EF ChangeTracker.
        /// Use this if you won't be making any changes to the record
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public virtual T GetNoTracking( Guid guid )
        {
            return AsNoFilter().AsNoTracking().FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets the model with the id value into the selected form
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public TResult GetSelect<TResult>( int id, System.Linq.Expressions.Expression<Func<T, TResult>> selector )
        {
            return AsNoFilter().Where( a => a.Id == id ).Select( selector ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the model with the Guid value into the selected form
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public TResult GetSelect<TResult>( Guid guid, System.Linq.Expressions.Expression<Func<T, TResult>> selector )
        {
            return AsNoFilter().Where( a => a.Guid == guid ).Select( selector ).FirstOrDefault();
        }

        /// <summary>
        /// Gets a list of items that match the specified expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public IQueryable<T> Get( ParameterExpression parameterExpression, Expression whereExpression )
        {
            return Get( parameterExpression, whereExpression, null, null );
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
            return Get( parameterExpression, whereExpression, sortProperty, null );
        }

        /// <summary>
        /// Gets the specified parameter expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="fetchTop">The fetch top.</param>
        /// <returns></returns>
        public IQueryable<T> Get( ParameterExpression parameterExpression, Expression whereExpression, Rock.Web.UI.Controls.SortProperty sortProperty, int? fetchTop = null )
        {
            return this.Queryable().Where( parameterExpression, whereExpression, sortProperty, fetchTop );
        }

        /// <summary>
        /// Gets the ids.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public IQueryable<int> GetIds( ParameterExpression parameterExpression, Expression whereExpression )
        {
            return Get( parameterExpression, whereExpression, null ).Select( t => t.Id );
        }

        /// <summary>
        /// Gets the Guid for the entity that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual Guid? GetGuid( int id )
        {
            return this.Queryable().Where( a => a.Id == id ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the Id for the entity that has the specified Guid
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public virtual int? GetId( Guid guid )
        {
            return this.Queryable().Where( a => a.Guid == guid ).Select( a => ( int? ) a.Id ).FirstOrDefault();
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
        /// Gets entities from a list of ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public virtual IQueryable<T> GetByIds( List<int> ids )
        {
            return Queryable().Where( t => ids.Contains( t.Id ) );
        }

        /// <summary>
        /// Gets entities from a list of guids
        /// </summary>
        /// <param name="guids">The guids.</param>
        /// <returns></returns>
        public virtual IQueryable<T> GetByGuids( List<Guid> guids )
        {
            return Queryable().Where( t => guids.Contains( t.Guid ) );
        }


        /// <summary>
        /// Gets a list of entities by ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public virtual List<T> GetListByIds( List<int> ids )
        {
            return GetByIds( ids ).ToList();
        }


        /// <summary>
        /// Gets a list of entities by guids.
        /// </summary>
        /// <param name="guids">The guids.</param>
        /// <returns></returns>
        public virtual List<T> GetListByGuids( List<Guid> guids )
        {
            return GetByGuids( guids ).ToList();
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
        /// Gets the model by URL encoded key.
        /// </summary>
        /// <param name="encodedKey">The encoded key.</param>
        /// <returns></returns>
        public virtual T GetByUrlEncodedKey( string encodedKey )
        {
            string key = encodedKey.Replace( '!', '%' );
            return GetByEncryptedKey( System.Web.HttpUtility.UrlDecode( key ) );
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

        #endregion

        #region Attach

        /// <summary>
        /// Attaches the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public virtual void Attach( T item )
        {
            _objectSet.Attach( item );
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual void Add( T item )
        {
            _objectSet.Add( item );
        }

        /// <summary>
        /// Calls _objectSet.RemoveRange which adds the given collection of items
        /// NOTE: Consider doing a SaveChanges(true) if there could be large number of items
        /// </summary>
        /// <param name="items">The items.</param>
        /// <remarks>
        ///   AddRange still ends up doing an INSERT statement for each item, but it's much faster than doing Add()
        /// </remarks>
        /// <returns></returns>
        public virtual bool AddRange( IEnumerable<T> items )
        {
            _objectSet.AddRange( items );
            return true;
        }

        #endregion

        #region Reorder

        /// <summary>
        /// Reorders the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        /// <returns>List of Ids who's order changed</returns>
        public virtual List<int> Reorder( List<T> items, int oldIndex, int newIndex )
        {
            var Ids = new List<int>();

            T movedItem = items[oldIndex];
            if ( movedItem != null )
            {
                Ids.Add( movedItem.Id );

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
                            Ids.Add( item.Id );
                            orderedItem.Order = order;
                        }
                    }
                    order++;
                }
            }

            return Ids;
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the specified item.  Will try to determine current person 
        /// alias from HttpContext.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual bool Delete( T item )
        {
            _objectSet.Remove( item );
            return true;
        }

        /// <summary>
        /// Calls _objectSet.RemoveRange which removes the given collection of items
        /// NOTE: Consider doing a SaveChanges(true) if there could be large number of items
        /// </summary>
        /// <param name="items">The items.</param>
        /// <remarks>
        ///   DeleteRange still ends up doing a DELETE statement for each item, but it's much faster than doing Delete()
        /// </remarks>
        /// <returns></returns>
        public virtual bool DeleteRange( IEnumerable<T> items )
        {
            if ( items == null )
                return false;

            _objectSet.RemoveRange( items );
            return true;
        }

        #endregion

        #region Following

        /// <summary>
        /// Gets a quety of the followers of a particular item
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IQueryable<Rock.Model.Person> GetFollowers( int id )
        {
            var rockContext = this.Context as RockContext;

            var entityType = EntityTypeCache.Get( typeof( T ), false, rockContext );
            if ( entityType != null )
            {
                var followerPersonIds = new Rock.Model.FollowingService( rockContext )
                    .Queryable()
                    .Where( f =>
                        f.EntityTypeId == entityType.Id &&
                        f.EntityId == id )
                    .Select( f => f.PersonAlias.PersonId );

                return new Rock.Model.PersonService( rockContext )
                    .Queryable()
                    .Where( p => followerPersonIds.Contains( p.Id ) );
            }

            return null;
        }

        /// <summary>
        /// Gets a query of the items that are followed by a specific person id 
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<T> GetFollowed( int personId )
        {
            var rockContext = this.Context as RockContext;

            var entityType = EntityTypeCache.Get( typeof( T ), false, rockContext );
            if ( entityType != null )
            {
                var ids = new Rock.Model.FollowingService( rockContext )
                    .Queryable()
                    .Where( f =>
                        f.EntityTypeId == entityType.Id &&
                        f.PersonAlias != null &&
                        f.PersonAlias.PersonId == personId )
                    .Select( f => f.PersonAlias.PersonId );

                return Queryable().Where( t => ids.Contains( t.Id ) );
            }

            return null;
        }

        #endregion

        #region Other

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
        /// Transforms the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="transformation">The transformation.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public IQueryable<T> Transform( IQueryable<T> source, Rock.Reporting.DataTransformComponent<T> transformation, Rock.Web.UI.Controls.SortProperty sortProperty = null )
        {
            var paramExpression = Expression.Parameter( source.ElementType, "p" );
            var whereExpression = transformation.GetExpression( this, source, paramExpression );
            return Get( paramExpression, whereExpression, sortProperty );
        }

        /// <summary>
        /// Copies the Values from a Source Entity into a Target Entity
        /// </summary>
        /// <param name="sourceItem">The source item.</param>
        /// <param name="targetItem">The target item.</param>
        public virtual void SetValues( T sourceItem, T targetItem )
        {
            _context.Entry( targetItem ).CurrentValues.SetValues( sourceItem );

            if ( ( targetItem is IModel ) && ( sourceItem is IModel ) )
            {
                // manually set the value of ModifiedAuditValuesAlreadyUpdated since it isn't a Database field
                ( targetItem as IModel ).ModifiedAuditValuesAlreadyUpdated = ( sourceItem as IModel ).ModifiedAuditValuesAlreadyUpdated;
            }
        }

        #endregion

        #endregion
    }

}
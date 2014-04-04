// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Rock.Model;
using Rock.Workflow;

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
        /// Gets an <see cref="IQueryable{T}"/> list of all models
        /// with eager loading of properties specified in includes
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> Queryable( string includes )
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

        #endregion

        #region Get Methods

        /// <summary>
        /// Gets the model with the id value
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public virtual T Get( int id )
        {
            return Queryable().FirstOrDefault( t => t.Id == id );
        }

        /// <summary>
        /// Gets the model with the Guid value
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public virtual T Get( Guid guid )
        {
            return Queryable().FirstOrDefault( t => t.Guid == guid );
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
                var queryable = Queryable().Where( lambda );
                return sortProperty != null ? queryable.Sort( sortProperty ) : queryable;
            }

            return this.Queryable();
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
        public virtual IEnumerable<T> GetByIds( List<int> ids )
        {
            return Queryable().Where( t => ids.Contains( t.Id ) ).ToList();
        }

        /// <summary>
        /// Gets entities from a list of guids
        /// </summary>
        /// <param name="guids">The guids.</param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetByGuids( List<Guid> guids )
        {
            return Queryable().Where( t => guids.Contains( t.Guid ) ).ToList();
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

        #endregion

        #region Reorder

        /// <summary>
        /// Reorders the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public virtual void Reorder( List<T> items, int oldIndex, int newIndex )
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
                    }
                }
                order++;
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the specified item.  Will try to determine current person 
        /// alias from HttpContext.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual bool Delete (T item )
        {
            _objectSet.Remove( item );
            return true;
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
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Service class for non entity specific methods
    /// </summary>
    public class Service
    {

        #region Fields

        private DbContext _context;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Service&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public Service( Rock.Data.DbContext dbContext )
        {
            _context = dbContext;
        }

        #endregion

        #region Methods

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
        /// Gets a data set.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataSet GetDataSet( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            using ( SqlConnection con = new SqlConnection( GetConnectionString() ) )
            {
                con.Open();

                using ( SqlCommand sqlCommand = new SqlCommand( query, con ) )
                {
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

                    SqlDataAdapter adapter = new SqlDataAdapter( sqlCommand );
                    DataSet dataSet = new DataSet( "rockDs" );
                    adapter.Fill( dataSet );
                    return dataSet;
                }
            }
        }

        private string GetConnectionString()
        {
            return ( _context.Database.Connection is SqlConnection ) ?
                ( (SqlConnection)_context.Database.Connection ).ConnectionString :
                System.Configuration.ConfigurationManager.ConnectionStrings["Rock"].ConnectionString;
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
            DataSet dataSet = GetDataSet( query, commandType, parameters );
            if ( dataSet.Tables.Count > 0 )
            {
                return dataSet.Tables[0];
            }

            return null;
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
            SqlConnection con = new SqlConnection( GetConnectionString() );
            con.Open();

            SqlCommand sqlCommand = new SqlCommand( query, con );
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

            return sqlCommand.ExecuteReader( CommandBehavior.CloseConnection );
        }

        /// <summary>
        /// Executes the query, and returns number of rows affected
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int ExecuteCommand( string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null )
        {
            using ( SqlConnection con = new SqlConnection( GetConnectionString() ) )
            {
                con.Open();

                using ( SqlCommand sqlCommand = new SqlCommand( query, con ) )
                {
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

                    return sqlCommand.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteScaler( string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null )
        {
            using ( SqlConnection con = new SqlConnection( GetConnectionString() ) )
            {
                con.Open();

                using ( SqlCommand sqlCommand = new SqlCommand( query, con ) )
                {
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

                    return sqlCommand.ExecuteScalar();
                }
            }
        }

        #endregion

    }

}
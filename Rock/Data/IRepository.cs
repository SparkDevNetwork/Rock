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

namespace Rock.Data
{
    /// <summary>
    /// Repository interface for POCO models
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models
        /// </summary>
        /// <returns></returns>
        IQueryable<T> AsQueryable();

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models, 
        /// with optional eager loading of properties specified in includes
        /// </summary>
        /// <returns></returns>
        IQueryable<T> AsQueryable( string includes );

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> list of all models.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> list of models matching the where expression
        /// </summary>
        /// <param name="where">where expression</param>
        /// <returns></returns>
        IEnumerable<T> Find( Expression<Func<T, bool>> where );

        /// <summary>
        /// Gets the only model matching the where expression.  Throws an exception if more than one
        /// model match.
        /// </summary>
        /// <param name="where">where expression</param>
        /// <returns></returns>
        T Single( Expression<Func<T, bool>> where );

        /// <summary>
        /// Gets the first model matching the where expression.  Throws an exception if no models 
        /// match.
        /// </summary>
        /// <param name="where">where expression</param>
        /// <returns></returns>
        T First( Expression<Func<T, bool>> where );

        /// <summary>
        /// Gets the first model matching the where expression.  Returns null if no models 
        /// match.
        /// </summary>
        /// <param name="where">where expression</param>
        /// <returns></returns>
        T FirstOrDefault( Expression<Func<T, bool>> where );

        /// <summary>
        /// Date the entity was created.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        DateTime? DateCreated( T entity );

        /// <summary>
        /// Date the entity was created.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        DateTime? DateCreated( int entityTypeId, int entityId );

        /// <summary>
        /// Date the entity was last modified.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        DateTime? DateLastModified( T entity );

        /// <summary>
        /// Date the entity was last modified.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        DateTime? DateLastModified( int entityTypeId, int entityId );

        /// <summary>
        /// The person id who created entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        int? CreatedByPersonId( T entity );

        /// <summary>
        /// The person id who created entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        int? CreatedByPersonId( int entityTypeId, int entityId );
        
        /// <summary>
        /// The person id who last modified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        int? LastModifiedByPersonId( T entity );

        /// <summary>
        /// The person id who last modified the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        int? LastModifiedByPersonId( int entityTypeId, int entityId );
        
        /// <summary>
        /// All the audits made to the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        IQueryable<Audit> Audits( T entity );

        /// <summary>
        /// All the audits made to the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        IQueryable<Audit> Audits( int entityTypeId, int entityId );

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Add( T entity );

        /// <summary>
        /// Attaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Attach( T entity );

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Delete( T entity );

        /// <summary>
        /// Saves any changes made in the current context
        /// </summary>
        /// <param name="PersonId">The person id.</param>
        /// <param name="audits">The audits.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        bool Save( int? PersonId, out List<Audit> audits, out List<string> errorMessages);

        /// <summary>
        /// Creates a raw query that will return entities
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        IEnumerable<T> ExecuteQuery( string query, params object[] parameters );

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void SetConfigurationValue( string key, string value );
    }

    /// <summary>
    /// Repository interface for non entity specific methods
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Creates a raw query that will return elements of the given type.  The
        /// type can be any type that has properties that match the names of the columns
        /// returned from the query, or can be a simple primitive type. The type does
        /// not have to be an entity type. 
        /// </summary>
        /// <param name="elementType">Type of the element.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        IEnumerable ExecuteQuery( Type elementType, string query, params object[] parameters );

        /// <summary>
        /// Gets a data set.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        DataSet GetDataSet( string query, CommandType commandType, Dictionary<string, object> parameters );

        /// <summary>
        /// Gets a data table.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        DataTable GetDataTable( string query, CommandType commandType, Dictionary<string, object> parameters );

        /// <summary>
        /// Gets a data reader.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        IDataReader GetDataReader( string query, CommandType commandType, Dictionary<string, object> parameters );

        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        int ExecuteCommand( string command, params object[] parameters );
    }

}
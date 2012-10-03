//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        List<Rock.Core.EntityChange> Save( int? personId );
    }
}
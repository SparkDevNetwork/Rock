//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Rock.Core;

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
		DateTimeOffset? DateCreated( T entity );

		/// <summary>
		/// Date the entity was created.
		/// </summary>
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		DateTimeOffset? DateCreated( string entityTypeName, int entityId );

		/// <summary>
		/// Date the entity was last modified.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		DateTimeOffset? DateLastModified( T entity );

		/// <summary>
		/// Date the entity was last modified.
		/// </summary>
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		DateTimeOffset? DateLastModified( string entityTypeName, int entityId );

		/// <summary>
		/// The person id who created entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		int? CreatedByPersonId( T entity );

		/// <summary>
		/// The person id who created entity.
		/// </summary>
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		int? CreatedByPersonId( string entityTypeName, int entityId );
		
		/// <summary>
		/// The person id who last modified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		int? LastModifiedByPersonId( T entity );

		/// <summary>
		/// The person id who last modified the entity.
		/// </summary>
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		int? LastModifiedByPersonId( string entityTypeName, int entityId );
		
		/// <summary>
		/// All the audits made to the entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		IQueryable<Audit> Audits( T entity );

		/// <summary>
		/// All the audits made to the entity.
		/// </summary>
		/// <param name="entityTypeName">Name of the entity type.</param>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		IQueryable<Audit> Audits( string entityTypeName, int entityId );

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
		List<Rock.Core.EntityChange> Save( int? PersonId, List<Rock.Core.AuditDto> audits);
    }
}
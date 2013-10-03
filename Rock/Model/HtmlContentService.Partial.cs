//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.HTMLContent"/> entity objects.
    /// </summary>
    public partial class HtmlContentService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.HTMLContent"/> entity objects by their Approver <see cref="Rock.Model.Person"/>
        /// </summary>
        /// <param name="approvedByPersonId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who approved the <see cref="Rock.Model.HTMLContent"/>. This 
        /// value can be null </param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.HTMLContent"/> entity objects that were approved by the specified <see cref="Rock.Model.Person"/>.</returns>
        public IEnumerable<HtmlContent> GetByApprovedByPersonId( int? approvedByPersonId )
        {
            return Repository.Find( t => ( t.ApprovedByPersonId == approvedByPersonId || ( approvedByPersonId == null && t.ApprovedByPersonId == null ) ) );
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.HTMLContent"/> entities by <see cref="Rock.Model.Block"/> (instance).
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Block"/>.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.HTMLContent">HTMLContents</see> for the specified <see cref="Rock.Model.Block"/>.</returns>
        public IEnumerable<HtmlContent> GetByBlockId( int blockId )
        {
            return Repository.Find( t => t.BlockId == blockId );
        }
        
        /// <summary>
        /// Returns a specific <see cref="Rock.Model.HTMLContent"/> by Block, entity value and version
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> the Id of the <see cref="Rock.Model.Block"/> that the <see cref="Rock.Model.HTMLContent"/> is used on. </param>
        /// <param name="entityValue">A <see cref="System.String"/> representing the EntityValue (qualifier) used to customize the <see cref="Rock.Model.HTMLContent"/> for a specific entity. 
        /// This value is nullable. </param>
        /// <param name="version">A <see cref="System.Int32" /> representing the <see cref="Rock.Model.HTMLContent">HTMLContent's</see> version number.</param>
        /// <returns>The first <see cref="Rock.Model.HTMLContent"/> that matches the provided criteria. If no match is found, this value will be null. </returns>
        public HtmlContent GetByBlockIdAndEntityValueAndVersion( int blockId, string entityValue, int version )
        {
            return Repository.FirstOrDefault( t => t.BlockId == blockId && ( t.EntityValue == entityValue || ( entityValue == null && t.EntityValue == null ) ) && t.Version == version );
        }

        /// <summary>
        /// Returns the active <see cref="Rock.Model.HTMLContent"/> for a specific <see cref="Rock.Model.Block"/> and/or EntityContext.
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Block"/>.</param>
        /// <param name="entityValue">A <see cref="System.String" /> representing the entityValue.</param>
        /// <returns>The active <see cref="Rock.Model.HTMLContent"/> for the specified <see cref="Rock.Model.Block"/> and/or EntityContext.</returns>
        public HtmlContent GetActiveContent( int blockId, string entityValue )
        {
            // Only consider approved content and content that is not prior to the start date 
            // or past the expire date
            var content = Queryable().
                Where( c => c.IsApproved &&
                    ( c.StartDateTime ?? (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue ) <= DateTime.Now &&
                    ( c.ExpireDateTime ?? (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue ) >= DateTime.Now );

            // If an entity value is specified, then return content specific to that context, 
            // otherewise return content for the current block instance
            if ( !string.IsNullOrEmpty( entityValue ) )
                content = content.Where( c => c.EntityValue == entityValue );
            else
                content = content.Where( c => c.BlockId == blockId );

            // return the most recently approved item
            return content.OrderByDescending( c => c.ApprovedDateTime ).FirstOrDefault();
        }

        /// <summary>
        /// Returns an enumerable collection containing all versions of <see cref="Rock.Model.HTMLContent"/> for a specific <see cref="Rock.Model.Block"/> and/or EntityContext.
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> representing the Id of a <see cref="Rock.Model.Block"/>.</param>
        /// <param name="entityValue">A <see cref="System.String"/> representing the EntityValue. This value is nullable.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.HTMLContent"/> for all versions of the specified <see cref="Rock.Model.Block"/> and/or EntityContext. </returns>
        public IEnumerable<HtmlContent> GetContent( int blockId, string entityValue )
        {
            var content = Queryable();

            // If an entity value is specified, then return content specific to that context, 
            // otherwise return content for the current block instance
            if ( !string.IsNullOrEmpty( entityValue ) )
                content = content.Where( c => c.EntityValue == entityValue );
            else
                content = content.Where( c => c.BlockId == blockId );

            // return the most recently approved item
            return content.OrderByDescending( c => c.Version );
        }
    }
}

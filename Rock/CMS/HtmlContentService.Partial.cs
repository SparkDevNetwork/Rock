//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rock.CMS
{
    public partial class HtmlContentService
    {
        /// <summary>
        /// Gets the active content for a specific block/context.
        /// </summary>
        /// <param name="blockId">The block id.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <returns></returns>
        public Rock.CMS.HtmlContent GetActiveContent( int blockId, string entityValue )
        {
            // Only consider approved content and content that is not prior to the start date 
            // or past the expire date
            var content = Repository.AsQueryable().
                Where( c => c.Approved &&
                    ( c.StartDateTime ?? DateTime.MinValue ) <= DateTime.Now &&
                    ( c.ExpireDateTime ?? DateTime.MaxValue ) >= DateTime.Now );

            // If an entity value is specified, then return content specific to that context, 
            // otherewise return content for the current block
            if ( !string.IsNullOrEmpty( entityValue ) )
                content = content.Where( c => c.EntityValue == entityValue );
            else
                content = content.Where( c => c.BlockId == blockId );

            // return the most recently approved item
            return content.OrderByDescending( c => c.ApprovedDateTime ).FirstOrDefault();
        }

        /// <summary>
        /// Gets all versions of content for a specific block/context.
        /// </summary>
        /// <param name="blockId">The block id.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <returns></returns>
        public IEnumerable<Rock.CMS.HtmlContent> GetContent( int blockId, string entityValue )
        {
            var content = Repository.AsQueryable();

            // If an entity value is specified, then return content specific to that context, 
            // otherewise return content for the current block
            if ( !string.IsNullOrEmpty( entityValue ) )
                content = content.Where( c => c.EntityValue == entityValue );
            else
                content = content.Where( c => c.BlockId == blockId );

            // return the most recently approved item
            return content.OrderByDescending( c => c.Version );
        }
    }
}

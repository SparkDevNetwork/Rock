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
using System.Linq;

using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.HtmlContent"/> entity objects.
    /// </summary>
    public partial class HtmlContentService
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.HtmlContent"/> entity objects by their Approver <see cref="Rock.Model.Person"/>
        /// </summary>
        /// <param name="approvedByPersonId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who approved the <see cref="Rock.Model.HtmlContent"/>. This 
        /// value can be null </param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.HtmlContent"/> entity objects that were approved by the specified <see cref="Rock.Model.Person"/>.</returns>
        public IQueryable<HtmlContent> GetByApprovedByPersonId( int? approvedByPersonId )
        {
            return Queryable().Where( t => ( t.ApprovedByPersonAliasId == approvedByPersonId || ( approvedByPersonId == null && t.ApprovedByPersonAliasId == null ) ) );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.HtmlContent"/> entities by <see cref="Rock.Model.Block"/> (instance).
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Block"/>.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.HtmlContent">HTMLContents</see> for the specified <see cref="Rock.Model.Block"/>.</returns>
        public IQueryable<HtmlContent> GetByBlockId( int blockId )
        {
            return Queryable().Where( t => t.BlockId == blockId );
        }

        /// <summary>
        /// Returns a specific <see cref="Rock.Model.HtmlContent"/> by Block, entity value and version
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> the Id of the <see cref="Rock.Model.Block"/> that the <see cref="Rock.Model.HtmlContent"/> is used on. </param>
        /// <param name="entityValue">A <see cref="System.String"/> representing the EntityValue (qualifier) used to customize the <see cref="Rock.Model.HtmlContent"/> for a specific entity. 
        /// This value is nullable. </param>
        /// <param name="version">A <see cref="System.Int32" /> representing the <see cref="Rock.Model.HtmlContent">HTMLContent's</see> version number.</param>
        /// <returns>The first <see cref="Rock.Model.HtmlContent"/> that matches the provided criteria. If no match is found, this value will be null. </returns>
        public HtmlContent GetByBlockIdAndEntityValueAndVersion( int blockId, string entityValue, int version )
        {
            return Queryable().OrderByDescending( o => o.ModifiedDateTime ).FirstOrDefault( t => t.BlockId == blockId && ( t.EntityValue == entityValue || ( entityValue == null && t.EntityValue == null ) ) && t.Version == version );
        }

        /// <summary>
        /// Returns an enumerable collection containing all versions of <see cref="Rock.Model.HtmlContent"/> for a specific <see cref="Rock.Model.Block"/> and/or EntityContext.
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> representing the Id of a <see cref="Rock.Model.Block"/>.</param>
        /// <param name="entityValue">A <see cref="System.String"/> representing the EntityValue. This value is nullable.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.HtmlContent"/> for all versions of the specified <see cref="Rock.Model.Block"/> and/or EntityContext. </returns>
        public IOrderedQueryable<HtmlContent> GetContent( int blockId, string entityValue )
        {
            var content = Queryable( "ModifiedByPersonAlias.Person" );

            // Add appropraite filtering (reused by other methods)
            content = AddFilterLogic( content, blockId, entityValue );

            // return the most recently approved item
            return content.OrderByDescending( c => c.Version );
        }

        /// <summary>
        /// Returns the latest version of <see cref="Rock.Model.HtmlContent"/> for a specific <see cref="Rock.Model.Block"/> and/or EntityContext.
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> representing the Id of a <see cref="Rock.Model.Block"/>.</param>
        /// <param name="entityValue">A <see cref="System.String"/> representing the EntityValue. This value is nullable.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.HtmlContent"/> for all versions of the specified <see cref="Rock.Model.Block"/> and/or EntityContext. </returns>
        public HtmlContent GetLatestVersion( int blockId, string entityValue )
        {
            var content = Queryable( "ModifiedByPersonAlias.Person" );

            // Add appropraite filtering (reused by other methods)
            content = AddFilterLogic( content, blockId, entityValue );

            return content.OrderByDescending( c => c.Version ).ThenByDescending( c => c.ApprovedDateTime ).FirstOrDefault();
        }

        /// <summary>
        /// Returns the active <see cref="Rock.Model.HtmlContent"/> for a specific <see cref="Rock.Model.Block"/> and/or EntityContext.
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Block"/>.</param>
        /// <param name="entityValue">A <see cref="System.String" /> representing the entityValue.</param>
        /// <returns>The active <see cref="Rock.Model.HtmlContent"/> for the specified <see cref="Rock.Model.Block"/> and/or EntityContext.</returns>
        [RockObsolete( "1.11" )]
        [Obsolete( "Use GetActiveContentHtml if you only need the HTML or GetActiveContentQueryable.FirstOrDefault() if you want the whole record" )]
        public HtmlContent GetActiveContent( int blockId, string entityValue )
        {
            return GetActiveContentQueryable( blockId, entityValue ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the active html content record (approved and within the start/expire daterange) ordered by the most recent approval date
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <returns></returns>
        public IOrderedQueryable<HtmlContent> GetActiveContentQueryable( int blockId, string entityValue )
        {
            // Only consider approved content and content that is not prior to the start date 
            // or past the expire date
            var content = Queryable()//.Include(a => a.ApprovedByPersonAlias.Person)
                .Where( c => c.IsApproved &&
                    ( c.StartDateTime == null || c.StartDateTime.Value <= RockDateTime.Now ) &&
                    ( c.ExpireDateTime == null || c.ExpireDateTime.Value >= RockDateTime.Now ) );

            // Add appropraite filtering (reused by other methods)
            content = AddFilterLogic( content, blockId, entityValue );

            // Return the most recently approved item
            return content.OrderByDescending( c => c.ApprovedDateTime );
        }

        /// <summary>
        /// Gets the active content HTML.
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <returns></returns>
        public string GetActiveContentHtml( int blockId, string entityValue )
        {
            return GetActiveContentQueryable( blockId, entityValue ).OrderByDescending( c => c.ApprovedDateTime ).Select( a => a.Content ).FirstOrDefault();
        }

        /// <summary>
        /// Adds the filter logic.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <returns></returns>
        public IQueryable<HtmlContent> AddFilterLogic( IQueryable<HtmlContent> qry, int blockId, string entityValue )
        {
            /*
                6/16/2020 - JME
                How the context value and context name work with the HTML block is a bit tricky. Updated the code below
                as it was not written to support the intended requirements. Documenting those requirements here in detail
                to ensure full understanding.

                There are several ways a HTML block can get it's content.

                Option 1: Simple
                The content is loaded purely from the HTML stored for the block by it's ID.

                Option 2: Context Value
                The content is loaded based on the block ID AND the context value (e.g. CampusId=1 or GroupId=1424). In
                this case the instance of the block could have different content for each unique campus value).

                Option 3: Context Name
                Context names allow you to link content across blocks. In this case the Block ID is not consider and instead
                the ContextName becomes the linkage. On top of this the ContextName can be joined with a Context Value to make
                a unique key.

                The 'entityValue' passed to this method will be in the format of: <ContentValue>=##&ContextName=AAAAA>
                Examples:
                     CampusId=1
                     CampusId=2
                     GroupId=1424
                     CampusId=1&ContextName=SharedKey
                     CampusId=2&ContextName=SharedKey
                     &ContextName=SharedKey (when context name is alone it still has the & in front, don't love it but it would break things to fix)
                                
                The previous logic did not filter on Block ID if the 'entityValue' had a value. This is incorrect. It should
                only do that if the 'entityValue' contains 'ContextName='.

                Changing this after so long could be considered a breaking change, but this is functionality is not working as
                intended and is preventing some very powerful usage.

            */

            var shouldFilterByBlockId = true;

            // If an entity value is specified, then return content specific to that context (entityValue), 
            // otherewise return content for the current block instance
            if ( entityValue.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( c => c.EntityValue == entityValue );

                // Don't consider Block Id if there is a ContextName
                if ( entityValue.Contains( "&ContextName=" ) )
                {
                    shouldFilterByBlockId = false;
                }
            }

            if ( shouldFilterByBlockId )
            {
                qry = qry.Where( c => c.BlockId == blockId );
            }

            return qry;
        }

        #region HtmlContent Caching Methods

        /// <summary>
        /// Returns the htmlcontent cachekey for a specific blockId or, if specified, a specific entityValue (Entity Context)
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <returns></returns>
        private static string HtmlContentCacheKey( int blockId, string entityValue )
        {
            // If an entity value is specified, then return content specific to that context (entityValue), 
            // otherwise return content for the current block instance
            string cacheKey;
            if ( !string.IsNullOrEmpty( entityValue ) )
            {
                cacheKey = "HtmlContent:" + entityValue;
            }
            else
            {
                cacheKey = "HtmlContent:" + blockId.ToString();
            }

            return cacheKey;
        }

        /// <summary>
        /// Returns the cached HTML for a specific blockId or, if specified, a specific entityValue (Entity Context)
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <returns></returns>
        public static string GetCachedContent( int blockId, string entityValue )
        {
            string cacheKey = HtmlContentCacheKey( blockId, entityValue );
            return RockCache.Get( cacheKey ) as string;
        }

        /// <summary>
        /// Adds the cached HTML for a specific blockId or, if specified, a specific entityValue (Entity Context)
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <param name="html">The HTML.</param>
        /// <param name="cacheDuration">Duration of the cache.</param>
        public static void AddCachedContent( int blockId, string entityValue, string html, int cacheDuration )
        {
            var expiration = RockDateTime.Now.AddSeconds( cacheDuration );
            RockCache.AddOrUpdate( HtmlContentCacheKey( blockId, entityValue ), string.Empty, html, expiration );
        }

        /// <summary>
        /// Adds the cached HTML for a specific blockId or, if specified, a specific entityValue (Entity Context)
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityValue">The entity value.</param>
        /// <param name="html">The HTML.</param>
        /// <param name="cacheDuration">Duration of the cache.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public static void AddCachedContent( int blockId, string entityValue, string html, int cacheDuration, string cacheTags )
        {
            var expiration = RockDateTime.Now.AddSeconds( cacheDuration );
            RockCache.AddOrUpdate( HtmlContentCacheKey( blockId, entityValue ), string.Empty, html, expiration, cacheTags );
        }

        /// <summary>
        /// Flushes the cached HTML for a specific blockId or, if specified, a specific entityValue (Entity Context)
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="entityValue">The entity value.</param>
        public static void FlushCachedContent( int blockId, string entityValue )
        {
            RockCache.Remove( HtmlContentCacheKey( blockId, entityValue ) );
        }

        #endregion
    }
}

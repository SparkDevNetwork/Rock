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

            // If an entity value is specified, then return content specific to that context, 
            // otherwise return content for the current block instance
            if ( !string.IsNullOrEmpty( entityValue ) )
            {
                content = content.Where( c => c.EntityValue == entityValue );
            }
            else
            {
                content = content.Where( c => c.BlockId == blockId );
            }

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

            // If an entity value is specified, then return content specific to that context, 
            // otherwise return content for the current block instance
            if ( !string.IsNullOrEmpty( entityValue ) )
            {
                content = content.Where( c => c.EntityValue == entityValue );
            }
            else
            {
                content = content.Where( c => c.BlockId == blockId );
            }

            return content.OrderByDescending( c => c.Version ).ThenByDescending( c => c.ApprovedDateTime ).FirstOrDefault();
        }

        /// <summary>
        /// Returns the active <see cref="Rock.Model.HtmlContent"/> for a specific <see cref="Rock.Model.Block"/> and/or EntityContext.
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Block"/>.</param>
        /// <param name="entityValue">A <see cref="System.String" /> representing the entityValue.</param>
        /// <returns>The active <see cref="Rock.Model.HtmlContent"/> for the specified <see cref="Rock.Model.Block"/> and/or EntityContext.</returns>
        public HtmlContent GetActiveContent( int blockId, string entityValue )
        {
            // Only consider approved content and content that is not prior to the start date 
            // or past the expire date
            var content = Queryable( "ApprovedByPersonAlias.Person" )
                .Where( c => c.IsApproved &&
                    ( c.StartDateTime ?? (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue ) <= RockDateTime.Now &&
                    ( c.ExpireDateTime ?? (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue ) >= RockDateTime.Now );

            // If an entity value is specified, then return content specific to that context (entityValue), 
            // otherewise return content for the current block instance
            if ( !string.IsNullOrEmpty( entityValue ) )
            {
                content = content.Where( c => c.EntityValue == entityValue );
            }
            else
            {
                content = content.Where( c => c.BlockId == blockId );
            }

            // return the most recently approved item
            return content.OrderByDescending( c => c.ApprovedDateTime ).FirstOrDefault();
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

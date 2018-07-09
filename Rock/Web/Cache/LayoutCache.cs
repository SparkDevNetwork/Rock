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

using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Layout that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.LayoutCache instead" )]
    public class LayoutCache : CachedModel<Layout>
    {
        #region Constructors

        private LayoutCache()
        {
        }

        private LayoutCache( CacheLayout cacheLayout )
        {
            CopyFromNewCache( cacheLayout );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the site id.
        /// </summary>
        /// <value>
        /// The site id.
        /// </value>
        public int SiteId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets the site.
        /// </summary>
        /// <value>
        /// The site.
        /// </value>
        public SiteCache Site => SiteCache.Read( SiteId );

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority => Site;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is Layout ) ) return;

            var layout = (Layout)model;
            IsSystem = layout.IsSystem;
            SiteId = layout.SiteId;
            FileName = layout.FileName;
            Name = layout.Name;
            Description = layout.Description;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheLayout ) ) return;

            var layout = (CacheLayout)cacheEntity;
            IsSystem = layout.IsSystem;
            SiteId = layout.SiteId;
            FileName = layout.FileName;
            Name = layout.Name;
            Description = layout.Description;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns Layout object from cache.  If Layout does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static LayoutCache Read( int id, RockContext rockContext = null )
        {
            return new LayoutCache( CacheLayout.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static LayoutCache Read( Guid guid, RockContext rockContext = null )
        {
            return new LayoutCache( CacheLayout.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds Layout model to cache, and returns cached object
        /// </summary>
        /// <param name="LayoutModel"></param>
        /// <returns></returns>
        public static LayoutCache Read( Layout LayoutModel )
        {
            return new LayoutCache( CacheLayout.Get( LayoutModel ) );
        }

        /// <summary>
        /// Removes Layout from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheLayout.Remove( id );
        }

        #endregion
    }
}
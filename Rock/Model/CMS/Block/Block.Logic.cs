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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Runtime.Serialization;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Block
    {
        #region Private Properties

        private int? _originalSiteId;
        private int? _originalLayoutId;
        private int? _originalPageId;

        #endregion Private Properties

        #region Navigation Properties

        /// <summary>
        /// Gets the location where this Block is being implemented on (Page, Layout, or Site) 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BlockLocation"/> where this Block is being implemented on.
        /// </value>
        /// <example>
        /// <c>BlockLocation.Page</c>
        /// </example>
        [DataMember]
        [NotMapped]
        public virtual BlockLocation BlockLocation
        {
            get
            {
                if ( this.PageId.HasValue )
                {
                    return BlockLocation.Page;
                }
                else if ( this.LayoutId.HasValue )
                {
                    return BlockLocation.Layout;
                }
                else if ( this.SiteId.HasValue )
                {
                    return BlockLocation.Site;
                }
                else
                {
                    return BlockLocation.None;
                }
            }
        }

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Gets the securable object that security permissions should be inherited from based on BlockLocation (Page, Layout, or Site)
        /// </summary>
        /// <value>
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                switch ( this.BlockLocation )
                {
                    case BlockLocation.Page:
                        return this.Page != null ? this.Page : base.ParentAuthority;
                    case BlockLocation.Layout:
                        return this.Layout != null ? this.Layout : base.ParentAuthority;
                    case BlockLocation.Site:
                        return this.Site != null ? this.Site : base.ParentAuthority;
                    default:
                        return base.ParentAuthority;
                }
            }
        }

        #endregion Methods

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return BlockCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            BlockCache.UpdateCachedEntity( this.Id, entityState );

            var model = this;

            if ( model.SiteId.HasValue && model.SiteId != _originalSiteId )
            {
                PageCache.FlushPagesForSite( model.SiteId.Value );
            }
            else if ( model.LayoutId.HasValue && model.LayoutId != _originalLayoutId )
            {
                PageCache.FlushPagesForLayout( model.LayoutId.Value );
            }

            if ( _originalSiteId.HasValue )
            {
                PageCache.FlushPagesForSite( _originalSiteId.Value );
            }
            else if ( _originalLayoutId.HasValue )
            {
                PageCache.FlushPagesForLayout( _originalLayoutId.Value );
            }
            else if ( _originalPageId.HasValue )
            {
                PageCache.FlushItem( _originalPageId.Value );
            }
        }

        #endregion ICacheable
    }
}

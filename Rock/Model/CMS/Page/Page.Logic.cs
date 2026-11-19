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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Page
    {
        /// <summary>
        /// Gets the parent authority for the page. Page security is automatically inherited from the parent page, unless 
        /// explicitly overridden.  If there is no parent page, it is inherited from the site (through the layout)
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.ParentPage != null )
                {
                    return this.ParentPage;
                }
                else if ( this.Layout != null && this.Layout.Site != null )
                {
                    return this.Layout.Site;
                }
                else
                {
                    return base.ParentAuthority;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result && Id != 0 && ParentPageId.HasValue )
                {
                    /*
                        10/28/2026 - MSE

                        A page cannot be moved beneath itself or one of its descendants.

                        Reason: Keep the stored page hierarchy free of parent cycles.
                    */
                    var visitedPageIds = new HashSet<int>();
                    int? ancestorPageId = ParentPageId;
                    while ( ancestorPageId.HasValue && visitedPageIds.Add( ancestorPageId.Value ) )
                    {
                        if ( ancestorPageId.Value == Id )
                        {
                            ValidationResults.Add( new ValidationResult( "Parent Page cannot be this page or one of its child pages (recursion)." ) );
                            return false;
                        }

                        ancestorPageId = PageCache.Get( ancestorPageId.Value )?.ParentPageId;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the cache control header. This shouldn't be used to set the properties directly but the json version should be used to set the CacheControlHeaderSettings property.
        /// </summary>
        /// <value>
        /// The cache control header.
        /// </value>
        [NotMapped]
        public RockCacheability CacheControlHeader
        {
            get
            {
                if ( _cacheControlHeader == null && CacheControlHeaderSettings.IsNotNullOrWhiteSpace() )
                {
                    _cacheControlHeader = Newtonsoft.Json.JsonConvert.DeserializeObject<RockCacheability>( CacheControlHeaderSettings );
                }

                return _cacheControlHeader;
            }
        }

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return PageCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            PageCache.UpdateCachedEntity( this.Id, entityState );

            if ( this.ParentPageId.HasValue )
            {
                PageCache.UpdateCachedEntity( this.ParentPageId.Value, EntityState.Detached );
            }

            if ( _originalParentPageId.HasValue && _originalParentPageId != this.ParentPageId )
            {
                PageCache.UpdateCachedEntity( _originalParentPageId.Value, EntityState.Detached );
            }
        }

        #endregion ICacheable

        #region Methods

        /// <summary>
        /// Gets the name of the fully qualified page.
        /// </summary>
        /// <returns></returns>
        public string GetFullyQualifiedPageName()
        {
            string result = InternalName.EncodeHtml();

            result = string.Format( "<a href='{0}'>{1}</a>", new PageReference( Id ).BuildUrl(), result );

            /*
                10/28/2026 - MSE

                Existing page data can contain a repeated parent.

                Reason: Stop the name lookup when an ancestor has already been visited.
            */
            var visitedPageIds = new HashSet<int> { Id };
            var parent = ParentPage;
            while ( parent != null && visitedPageIds.Add( parent.Id ) )
            {
                result = string.Format( "<a href='{0}'>{1}</a> / {2}", new PageReference( parent.Id ).BuildUrl(), parent.InternalName.EncodeHtml(), result );
                parent = parent.ParentPage;
            }

            return result;
        }

        #endregion
    }
}

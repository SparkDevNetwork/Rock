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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Content Channel Item Slug Cache
    /// </summary>
    [Serializable]
    [DataContract]
    public class ContentChannelItemSlugCache : ModelCache<ContentChannelItemSlugCache, ContentChannelItemSlug>
    {
        #region Properties

        /// <inheritdoc cref="ContentChannelItemSlug.ContentChannelItemId" />
        [DataMember]
        public int ContentChannelItemId { get; private set; }

        /// <inheritdoc cref="ContentChannelItemSlug.Slug" />
        [DataMember]
        public string Slug { get; private set; }

        /// <inheritdoc cref="ContentChannelItemSlug.IsPrimary" />
        [DataMember]
        public bool IsPrimary { get; private set; }

        /// <inheritdoc cref="ContentChannelItemSlug.ContentChannelItem" />
        public ContentChannelItemCache ContentChannelItem => ContentChannelItemCache.Get( ContentChannelItemId );

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Sets the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var contentChannelItemSlug = entity as ContentChannelItemSlug;
            if ( contentChannelItemSlug == null )
            {
                return;
            }

            ContentChannelItemId = contentChannelItemSlug.ContentChannelItemId;
            Slug = contentChannelItemSlug.Slug;
            IsPrimary = contentChannelItemSlug.IsPrimary;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Slug;
        }

        #endregion Public Methods

        #region Static Methods

        /// <summary>
        /// Flushes any <see cref="ContentChannelItemSlugCache"/> entries that are associated with the specified
        /// <see cref="Model.ContentChannelItem"/>.
        /// </summary>
        /// <param name="contentChannelItemId">The identifier of the <see cref="Model.ContentChannelItem"/>.</param>
        /// <param name="dbContext">
        /// The database context to use when retrieving the identifiers of the <see cref="ContentChannelItemSlugCache"/>
        /// entries to flush.
        /// </param>
        public static void FlushCachedSlugs( int contentChannelItemId, Rock.Data.DbContext dbContext )
        {
            var contentChannelItemSlugIds = new ContentChannelItemSlugService( ( RockContext ) dbContext )
                .Queryable()
                .Where( s => s.ContentChannelItemId == contentChannelItemId )
                .Select( s => s.Id )
                .ToList();

            foreach ( var contentChannelItemSlugId in contentChannelItemSlugIds )
            {
                FlushItem( contentChannelItemSlugId );
            }
        }

        #endregion Static Methods

        #region ISecured

        /// <inheritdoc cref="ContentChannelItem.ParentAuthority" />
        public override ISecured ParentAuthority => ContentChannelItem ?? base.ParentAuthority;

        #endregion ISecured
    }
}

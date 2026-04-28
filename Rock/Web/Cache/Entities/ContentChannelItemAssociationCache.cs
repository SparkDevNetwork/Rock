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
    /// Content Channel Item Association Cache
    /// </summary>
    [Serializable]
    [DataContract]
    public class ContentChannelItemAssociationCache : ModelCache<ContentChannelItemAssociationCache, ContentChannelItemAssociation>
    {
        #region Properties

        /// <inheritdoc cref="ContentChannelItemAssociation.ContentChannelItemId" />
        [DataMember]
        public int ContentChannelItemId { get; set; }

        /// <inheritdoc cref="ContentChannelItemAssociation.ChildContentChannelItemId" />
        [DataMember]
        public int ChildContentChannelItemId { get; set; }

        /// <inheritdoc cref="ContentChannelItemAssociation.Order" />
        [DataMember]
        public int Order { get; set; }

        /// <inheritdoc cref="ContentChannelItemAssociation.ContentChannelItem" />
        public ContentChannelItemCache ContentChannelItem => ContentChannelItemCache.Get( ContentChannelItemId );

        /// <inheritdoc cref="ContentChannelItemAssociation.ChildContentChannelItem" />
        public ContentChannelItemCache ChildContentChannelItem => ContentChannelItemCache.Get( ChildContentChannelItemId );

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Sets the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var contentChannelItemAssociation = entity as ContentChannelItemAssociation;
            if ( contentChannelItemAssociation == null )
            {
                return;
            }

            ContentChannelItemId = contentChannelItemAssociation.ContentChannelItemId;
            ChildContentChannelItemId = contentChannelItemAssociation.ChildContentChannelItemId;
            Order = contentChannelItemAssociation.Order;
        }

        #endregion Public Methods

        #region Static Methods

        /// <summary>
        /// Flushes any <see cref="ContentChannelItemAssociationCache"/> entries that are associated with the specified
        /// <see cref="Model.ContentChannelItem"/>.
        /// </summary>
        /// <param name="contentChannelItemId">The identifier of the <see cref="Model.ContentChannelItem"/>.</param>
        /// <param name="dbContext">
        /// The database context to use when retrieving the identifiers of the <see cref="ContentChannelItemAssociationCache"/>
        /// entries to flush.
        /// </param>
        /// <remarks>
        /// This will flush any entries where the specified <see cref="Model.ContentChannelItem"/> is either the parent
        /// or child in the association.
        /// </remarks>
        public static void FlushCachedAssociations( int contentChannelItemId, Rock.Data.DbContext dbContext )
        {
            var contentChannelItemAssociationIds = new ContentChannelItemAssociationService( ( RockContext ) dbContext )
                .Queryable()
                .Where( a =>
                    a.ContentChannelItemId == contentChannelItemId
                    || a.ChildContentChannelItemId == contentChannelItemId
                )
                .Select( a => a.Id )
                .ToList();

            foreach ( var contentChannelItemAssociationId in contentChannelItemAssociationIds )
            {
                FlushItem( contentChannelItemAssociationId );
            }
        }

        #endregion Static Methods

        #region ISecured

        /// <inheritdoc cref="ContentChannelItem.ParentAuthority" />
        public override ISecured ParentAuthority => ContentChannelItem ?? base.ParentAuthority;

        #endregion ISecured
    }
}

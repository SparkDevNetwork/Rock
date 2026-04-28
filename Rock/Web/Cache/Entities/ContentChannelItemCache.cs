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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache.Entities;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Content Channel Item Cache
    /// </summary>
    [Serializable]
    [DataContract]
    public class ContentChannelItemCache : ModelCache<ContentChannelItemCache, ContentChannelItem>
    {
        #region Properties

        private readonly object _obj = new object();

        /// <inheritdoc cref="ContentChannelItem.ContentChannelId" />
        [DataMember]
        public int ContentChannelId { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ContentChannelTypeId" />
        [DataMember]
        public int ContentChannelTypeId { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.Title" />
        [DataMember]
        public string Title { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.Content" />
        [DataMember]
        public string Content { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.Priority" />
        [DataMember]
        public int Priority { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.Status" />
        [DataMember]
        public ContentChannelItemStatus Status { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ApprovedByPersonAliasId" />
        [DataMember]
        public int? ApprovedByPersonAliasId { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ApprovedDateTime" />
        [DataMember]
        public DateTime? ApprovedDateTime { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.StartDateTime" />
        [DataMember]
        public DateTime StartDateTime { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ExpireDateTime" />
        [DataMember]
        public DateTime? ExpireDateTime { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.Permalink" />
        [DataMember]
        public string Permalink { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.Order" />
        [DataMember]
        public int Order { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ItemGlobalKey" />
        [DataMember]
        public string ItemGlobalKey { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.IsContentLibraryOwner" />
        [DataMember]
        public bool? IsContentLibraryOwner { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ContentLibrarySourceIdentifier" />
        [DataMember]
        public Guid? ContentLibrarySourceIdentifier { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ContentLibraryLicenseTypeValueId" />
        [DataMember]
        public int? ContentLibraryLicenseTypeValueId { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ContentLibraryContentTopicId" />
        [DataMember]
        public int? ContentLibraryContentTopicId { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ContentLibraryUploadedByPersonAliasId" />
        [DataMember]
        public int? ContentLibraryUploadedByPersonAliasId { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ContentLibraryUploadedDateTime" />
        [DataMember]
        public DateTime? ContentLibraryUploadedDateTime { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ExperienceLevel" />
        [DataMember]
        public ContentLibraryItemExperienceLevel? ExperienceLevel { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.AdditionalSettingsJson" />
        [DataMember]
        public string AdditionalSettingsJson { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.AllowsInteractiveBulkIndexing" />
        [DataMember]
        public bool AllowsInteractiveBulkIndexing { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.IsDownloadedFromContentLibrary"/>
        [DataMember]
        public bool IsDownloadedFromContentLibrary { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.IsUploadedToContentLibrary"/>
        [DataMember]
        public bool IsUploadedToContentLibrary { get; private set; }

        /// <inheritdoc cref="ContentChannelItem.ContentChannel" />
        public ContentChannelCache ContentChannel => ContentChannelCache.Get( ContentChannelId );

        /// <inheritdoc cref="ContentChannelItem.ContentChannelType" />
        public ContentChannelTypeCache ContentChannelType => ContentChannelTypeCache.Get( ContentChannelTypeId );

        /// <summary>
        /// Gets the content channel item slugs.
        /// </summary>
        public List<ContentChannelItemSlugCache> ContentChannelItemSlugs
        {
            get
            {
                var contentChannelItemSlugs = new List<ContentChannelItemSlugCache>();

                if ( _contentChannelItemSlugIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _contentChannelItemSlugIds == null )
                        {
                            using ( var rockContext = RockApp.Current.CreateRockContext() )
                            {
                                _contentChannelItemSlugIds = new ContentChannelItemSlugService( rockContext )
                                    .Queryable()
                                    .Where( s => s.ContentChannelItemId == Id )
                                    .Select( s => s.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                foreach ( var id in _contentChannelItemSlugIds )
                {
                    var contentChannelItemSlug = ContentChannelItemSlugCache.Get( id );
                    if ( contentChannelItemSlug != null )
                    {
                        contentChannelItemSlugs.Add( contentChannelItemSlug );
                    }
                }

                return contentChannelItemSlugs;
            }
        }

        private List<int> _contentChannelItemSlugIds = null;

        /// <inheritdoc cref="ContentChannelItem.PrimarySlug" />
        public string PrimarySlug
        {
            get
            {
                return ContentChannelItemSlugs
                    .OrderByDescending( s => s.IsPrimary )
                    .Select( s => s.Slug )
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the child content channel item associations.
        /// </summary>
        public List<ContentChannelItemAssociationCache> ChildContentChannelItemAssociations
        {
            get
            {
                var childContentChannelItemAssociations = new List<ContentChannelItemAssociationCache>();

                if ( _childItemAssociationIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _childItemAssociationIds == null )
                        {
                            using ( var rockContext = RockApp.Current.CreateRockContext() )
                            {
                                _childItemAssociationIds = new ContentChannelItemAssociationService( rockContext )
                                    .Queryable()
                                    .Where( a => a.ContentChannelItemId == Id )
                                    .OrderBy( a => a.Order )
                                    .Select( a => a.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                foreach ( var id in _childItemAssociationIds )
                {
                    var contentChannelItemAssociationCache = ContentChannelItemAssociationCache.Get( id );
                    if ( contentChannelItemAssociationCache != null )
                    {
                        childContentChannelItemAssociations.Add( contentChannelItemAssociationCache );
                    }
                }

                return childContentChannelItemAssociations;
            }
        }

        private List<int> _childItemAssociationIds = null;

        /// <summary>
        /// Gets the parent content channel item associations.
        /// </summary>
        public List<ContentChannelItemAssociationCache> ParentContentChannelItemAssociations
        {
            get
            {
                var parentContentChannelItemAssociations = new List<ContentChannelItemAssociationCache>();

                if ( _parentItemAssociationIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _parentItemAssociationIds == null )
                        {
                            using ( var rockContext = RockApp.Current.CreateRockContext() )
                            {
                                _parentItemAssociationIds = new ContentChannelItemAssociationService( rockContext )
                                    .Queryable()
                                    .Where( a => a.ChildContentChannelItemId == Id )
                                    .OrderBy( a => a.Order )
                                    .Select( a => a.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                foreach ( var id in _parentItemAssociationIds )
                {
                    var contentChannelItemAssociationCache = ContentChannelItemAssociationCache.Get( id );
                    if ( contentChannelItemAssociationCache != null )
                    {
                        parentContentChannelItemAssociations.Add( contentChannelItemAssociationCache );
                    }
                }

                return parentContentChannelItemAssociations;
            }
        }

        private List<int> _parentItemAssociationIds = null;

        /*
            3/31/2026 - JPH

            We've decided not to cache the item's EventItemOccurrences collection for this initial cache implementation,
            as the EventItemOccurrenceChannelItem object graph is quite large and would require several additional
            cache implementations to make it effective. If we find that we need to cache the EventItemOccurrences in
            the future, we can choose at that time to implement a caching strategy for them.

            Reason: Explain Incomplete Cache Object.
        */

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Sets the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var contentChannelItem = entity as ContentChannelItem;
            if ( contentChannelItem == null )
            {
                return;
            }

            ContentChannelId = contentChannelItem.ContentChannelId;
            ContentChannelTypeId = contentChannelItem.ContentChannelTypeId;
            Title = contentChannelItem.Title;
            Content = contentChannelItem.Content;
            Priority = contentChannelItem.Priority;
            Status = contentChannelItem.Status;
            ApprovedByPersonAliasId = contentChannelItem.ApprovedByPersonAliasId;
            ApprovedDateTime = contentChannelItem.ApprovedDateTime;
            StartDateTime = contentChannelItem.StartDateTime;
            ExpireDateTime = contentChannelItem.ExpireDateTime;
            Permalink = contentChannelItem.Permalink;
            Order = contentChannelItem.Order;
            ItemGlobalKey = contentChannelItem.ItemGlobalKey;
            IsContentLibraryOwner = contentChannelItem.IsContentLibraryOwner;
            ContentLibrarySourceIdentifier = contentChannelItem.ContentLibrarySourceIdentifier;
            ContentLibraryLicenseTypeValueId = contentChannelItem.ContentLibraryLicenseTypeValueId;
            ContentLibraryContentTopicId = contentChannelItem.ContentLibraryContentTopicId;
            ContentLibraryUploadedByPersonAliasId = contentChannelItem.ContentLibraryUploadedByPersonAliasId;
            ContentLibraryUploadedDateTime = contentChannelItem.ContentLibraryUploadedDateTime;
            ExperienceLevel = contentChannelItem.ExperienceLevel;
            AdditionalSettingsJson = contentChannelItem.AdditionalSettingsJson;
            AllowsInteractiveBulkIndexing = contentChannelItem.AllowsInteractiveBulkIndexing;
            IsDownloadedFromContentLibrary = contentChannelItem.IsDownloadedFromContentLibrary;
            IsUploadedToContentLibrary = contentChannelItem.IsUploadedToContentLibrary;

            _contentChannelItemSlugIds = null;
            _childItemAssociationIds = null;
            _parentItemAssociationIds = null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Title;
        }

        #endregion Public Methods

        #region Static Methods

        /// <summary>
        /// Flushes any <see cref="ContentChannelItemCache"/> entries that are associated with the specified
        /// <see cref="Model.ContentChannel"/>.
        /// </summary>
        /// <param name="contentChannelId">The identifier of the <see cref="Model.ContentChannel"/>.</param>
        /// <param name="dbContext">
        /// The database context to use when retrieving the identifiers of the <see cref="ContentChannelItemCache"/>
        /// entries to flush.
        /// </param>
        public static void FlushCachedItems( int contentChannelId, Rock.Data.DbContext dbContext )
        {
            var contentChannelItemIds = new ContentChannelItemService( ( RockContext ) dbContext )
                .Queryable()
                .Where( i => i.ContentChannelId == contentChannelId )
                .Select( i => i.Id )
                .ToList();

            foreach ( var contentChannelItemId in contentChannelItemIds )
            {
                FlushItem( contentChannelItemId );

                ContentChannelItemSlugCache.FlushCachedSlugs( contentChannelItemId, dbContext );
                ContentChannelItemAssociationCache.FlushCachedAssociations( contentChannelItemId, dbContext );
            }
        }

        #endregion Static Methods

        #region ISecured

        /// <inheritdoc cref="ContentChannelItem.ParentAuthority" />
        public override ISecured ParentAuthority => ContentChannel ?? base.ParentAuthority;

        #endregion ISecured
    }
}

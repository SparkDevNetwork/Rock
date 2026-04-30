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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Cms;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.AI.KnowledgeBaseFolderList;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.AI
{
    /// <summary>
    /// Displays the folders that belong to a knowledge base as a grid of cards.
    /// Selecting a card navigates to the folder detail page; the add affordance
    /// expands to a typed dropdown so the admin picks the source kind before
    /// the detail page renders the appropriate Source Key picker.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Knowledge Base Folder List" )]
    [Category( "AI" )]
    [Description( "Displays the folders that belong to a knowledge base." )]
    [IconCssClass( "ti ti-folders" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "The page that will show the knowledge base folder details.",
        IsRequired = true,
        Order = 0 )]

    #endregion Block Attributes

    [DefaultBlockRole( BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "C8E1A3D4-2F5B-4970-9D6E-8A1F4B7C0E25" )]
    [Rock.SystemGuid.BlockTypeGuid( "F0A7B2C3-9D4E-4B81-B5A2-3C6D7E8F1A04" )]
    public class KnowledgeBaseFolderList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string KnowledgeBaseId = "KnowledgeBaseId";
            public const string KnowledgeBaseFolderId = "KnowledgeBaseFolderId";
            public const string SourceEntityTypeId = "SourceEntityTypeId";
        }

        #endregion Keys

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var currentPerson = GetCurrentPerson();
            var knowledgeBaseKey = PageParameter( PageParameterKey.KnowledgeBaseId );

            var parentKnowledgeBase = ResolveParentKnowledgeBase( knowledgeBaseKey );

            if ( parentKnowledgeBase == null )
            {
                return new KnowledgeBaseFolderListInitializationBox
                {
                    ErrorMessage = $"The {KnowledgeBase.FriendlyTypeName} was not found."
                };
            }

            if ( !parentKnowledgeBase.IsAuthorized( Authorization.VIEW, currentPerson ) )
            {
                return new KnowledgeBaseFolderListInitializationBox
                {
                    ErrorMessage = "You are not authorized to view folders for this knowledge base."
                };
            }

            var summaries = LoadFolderSummaries( RockContext, parentKnowledgeBase.Id );

            var box = new KnowledgeBaseFolderListInitializationBox
            {
                KnowledgeBaseName = parentKnowledgeBase.Name,
                FolderSummaries = summaries,
                SourceTypes = GetSupportedSourceTypes(),
                IsAddEnabled = parentKnowledgeBase.IsAuthorized( Authorization.EDIT, currentPerson ),
                NavigationUrls = GetBoxNavigationUrls( parentKnowledgeBase.IdKey )
            };

            return box;
        }

        #endregion RockBlockType Implementation

        #region Private Methods

        /// <summary>
        /// Resolves the parent <see cref="KnowledgeBase"/> from the page
        /// parameter, accepting either an Id, IdKey, or Guid.
        /// </summary>
        /// <param name="key">The page parameter value identifying the parent knowledge base.</param>
        /// <returns>The cached parent knowledge base, or null if it cannot be resolved.</returns>
        private KnowledgeBaseCache ResolveParentKnowledgeBase( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // The page param is documented as KnowledgeBaseId but Rock blocks
            // routinely accept Id, IdKey, or Guid. Resolve by walking the
            // available cache lookups so all three keep working.
            var disablePredictableIds = PageCache.Layout.Site.DisablePredictableIds;

            if ( !disablePredictableIds && int.TryParse( key, out var parsedId ) )
            {
                var byId = KnowledgeBaseCache.Get( parsedId );
                if ( byId != null )
                {
                    return byId;
                }
            }

            if ( Guid.TryParse( key, out var parsedGuid ) )
            {
                var byGuid = KnowledgeBaseCache.Get( parsedGuid );
                if ( byGuid != null )
                {
                    return byGuid;
                }
            }

            return KnowledgeBaseCache.GetByIdKey( key );
        }

        /// <summary>
        /// Loads the folder summaries for the parent knowledge base, hydrating
        /// each with its document count and source-type label.
        /// </summary>
        /// <param name="rockContext">The Rock context to use for database queries.</param>
        /// <param name="knowledgeBaseId">The Id of the parent <see cref="KnowledgeBase"/>.</param>
        /// <returns>A list of <see cref="KnowledgeBaseFolderSummaryBag"/>.</returns>
        private List<KnowledgeBaseFolderSummaryBag> LoadFolderSummaries( RockContext rockContext, int knowledgeBaseId )
        {
            // Folders come from the folder cache so no database round-trip is
            // needed for the card metadata. The cache exposes everything the
            // summary needs (Id, Name, Description, SourceEntityTypeId).
            var folders = KnowledgeBaseFolderCache.All( rockContext )
                .Where( f => f.KnowledgeBaseId == knowledgeBaseId )
                .OrderBy( f => f.Name )
                .ToList();

            if ( !folders.Any() )
            {
                return new List<KnowledgeBaseFolderSummaryBag>();
            }

            // Documents are not cached (the set can be large), so document
            // counts still come from the service in a single grouped query.
            var folderIds = folders.Select( f => f.Id ).ToList();

            var documentCountsByFolderId = new KnowledgeBaseDocumentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( d => folderIds.Contains( d.KnowledgeBaseFolderId ) )
                .GroupBy( d => d.KnowledgeBaseFolderId )
                .Select( g => new { FolderId = g.Key, Count = g.Count() } )
                .ToDictionary( g => g.FolderId, g => g.Count );

            var summaries = folders
                .Select( f => new KnowledgeBaseFolderSummaryBag
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    SourceTypeName = GetSourceTypeName( f.SourceEntityTypeId ),
                    DocumentCount = documentCountsByFolderId.TryGetValue( f.Id, out var count ) ? count : 0
                } )
                .ToList();

            summaries.ForEach( s => s.TranslateIdToIdKey() );

            return summaries;
        }

        /// <summary>
        /// Resolves a friendly source-type label for a folder. Returns
        /// "Manual" when no source entity type is bound.
        /// </summary>
        /// <param name="sourceEntityTypeId">The folder's source entity type identifier, if any.</param>
        /// <returns>A display label for the source type.</returns>
        private string GetSourceTypeName( int? sourceEntityTypeId )
        {
            if ( !sourceEntityTypeId.HasValue )
            {
                return "Manual";
            }

            var entityType = EntityTypeCache.Get( sourceEntityTypeId.Value );

            return entityType?.FriendlyName ?? "Unknown";
        }

        /// <summary>
        /// Gets the hard-coded list of source types supported in v1. The list
        /// will move to a runtime registry once additional source types come
        /// online; for now Content Channel and Manual are sufficient.
        /// </summary>
        /// <returns>A list of <see cref="KnowledgeBaseFolderSourceTypeBag"/>.</returns>
        private List<KnowledgeBaseFolderSourceTypeBag> GetSupportedSourceTypes()
        {
            var sourceTypes = new List<KnowledgeBaseFolderSourceTypeBag>();

            var contentChannelEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.CONTENT_CHANNEL.AsGuid() );
            if ( contentChannelEntityType != null )
            {
                sourceTypes.Add( new KnowledgeBaseFolderSourceTypeBag
                {
                    Name = "Content Channel",
                    IconCssClass = "ti ti-news",
                    SourceEntityTypeId = contentChannelEntityType.Id
                } );
            }

            sourceTypes.Add( new KnowledgeBaseFolderSourceTypeBag
            {
                Name = "Manual",
                IconCssClass = "ti ti-pencil",
                SourceEntityTypeId = null
            } );

            return sourceTypes;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate. The
        /// Add URL embeds the parent knowledge base's IdKey so the detail
        /// page can scope its source-key picker correctly.
        /// </summary>
        /// <param name="knowledgeBaseIdKey">The parent knowledge base's IdKey.</param>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( string knowledgeBaseIdKey )
        {
            // Single DetailPage URL with the entity Id token left as
            // "((Key))" for the client to replace at navigation time. Click
            // a card → replace with the folder's IdKey; click Add → replace
            // with "0" so the detail block enters add-mode. Parent KB IdKey
            // is baked in so add-mode has the parent context to bootstrap
            // from. This mirrors the StepTypeList pattern.
            var detailUrl = this.GetLinkedPageUrl(
                AttributeKey.DetailPage,
                new Dictionary<string, string>
                {
                    [PageParameterKey.KnowledgeBaseFolderId] = "((Key))",
                    [PageParameterKey.KnowledgeBaseId] = knowledgeBaseIdKey
                } );

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = detailUrl
            };
        }

        #endregion Private Methods
    }
}

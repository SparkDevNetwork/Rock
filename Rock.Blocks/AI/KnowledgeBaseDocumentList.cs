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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Cms;
using Rock.Data;
using Rock.Enums.AI;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.AI.KnowledgeBaseDocumentList;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.AI
{
    /// <summary>
    /// Displays the documents that belong to a knowledge base folder.
    /// </summary>

    [DisplayName( "Knowledge Base Document List" )]
    [Category( "AI" )]
    [Description( "Displays the documents that belong to a knowledge base folder." )]
    [IconCssClass( "ti ti-files" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the knowledge base document details.",
        Key = AttributeKey.DetailPage,
        IsRequired = true,
        Order = 0 )]

    [DefaultBlockRole( BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "9D7F1A2B-4C8E-4F6A-B3D5-7E2C8F4A1E36" )]
    [Rock.SystemGuid.BlockTypeGuid( "2C5B8E0F-7A3D-49B2-86F4-1D9E3C8B5A47" )]
    [CustomizedGrid]
    public class KnowledgeBaseDocumentList : RockEntityListBlockType<KnowledgeBaseDocument>
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
            public const string KnowledgeBaseFolderId = "KnowledgeBaseFolderId";
            public const string KnowledgeBaseDocumentId = "KnowledgeBaseDocumentId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Cached resolved parent folder so security gating and
        /// query scoping share a single lookup.
        /// </summary>
        private KnowledgeBaseFolder _parentFolder;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The parent <see cref="KnowledgeBaseFolder"/> resolved from the
        /// page parameter. Returns null if the page parameter is missing or
        /// the folder cannot be found.
        /// </summary>
        private KnowledgeBaseFolder ParentFolder
        {
            get
            {
                if ( _parentFolder == null )
                {
                    var key = PageParameter( PageParameterKey.KnowledgeBaseFolderId );

                    if ( key.IsNotNullOrWhiteSpace() )
                    {
                        _parentFolder = new KnowledgeBaseFolderService( RockContext )
                            .Get( key, !PageCache.Layout.Site.DisablePredictableIds );
                    }
                }

                return _parentFolder;
            }
        }

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<KnowledgeBaseDocumentListOptionsBag>();
            var builder = GetGridBuilder();

            var parentFolder = ParentFolder;
            var parentKnowledgeBase = parentFolder != null ? KnowledgeBaseCache.Get( parentFolder.KnowledgeBaseId ) : null;
            var currentPerson = GetCurrentPerson();

            if ( parentFolder == null || parentKnowledgeBase == null )
            {
                box.ErrorMessage = $"The parent {KnowledgeBaseFolder.FriendlyTypeName} was not found.";
                return box;
            }

            if ( !parentKnowledgeBase.IsAuthorized( Authorization.VIEW, currentPerson ) )
            {
                box.ErrorMessage = "You are not authorized to view documents in this folder.";
                return box;
            }

            box.IsAddEnabled = parentKnowledgeBase.IsAuthorized( Authorization.EDIT, currentPerson );
            box.IsDeleteEnabled = parentKnowledgeBase.IsAuthorized( Authorization.EDIT, currentPerson );
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls( parentFolder.IdKey );
            box.Options = GetBoxOptions( parentFolder );
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <param name="parentFolder">The parent folder for context labeling.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private KnowledgeBaseDocumentListOptionsBag GetBoxOptions( KnowledgeBaseFolder parentFolder )
        {
            return new KnowledgeBaseDocumentListOptionsBag
            {
                FolderName = parentFolder?.Name
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// The detail URL embeds the parent folder's IdKey so add-mode
        /// navigations carry the folder context the document detail block
        /// needs to bootstrap a new document. Existing-document navigations
        /// also include it; the document detail block prefers the entity's
        /// own folder reference when one is available.
        /// </summary>
        /// <param name="parentFolderIdKey">The parent folder's IdKey.</param>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( string parentFolderIdKey )
        {
            var detailUrl = this.GetLinkedPageUrl(
                AttributeKey.DetailPage,
                new Dictionary<string, string>
                {
                    [PageParameterKey.KnowledgeBaseDocumentId] = "((Key))",
                    [PageParameterKey.KnowledgeBaseFolderId] = parentFolderIdKey
                } );

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = detailUrl
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<KnowledgeBaseDocument> GetListQueryable( RockContext rockContext )
        {
            var folderId = ParentFolder?.Id;

            if ( !folderId.HasValue )
            {
                return Enumerable.Empty<KnowledgeBaseDocument>().AsQueryable();
            }

            return base.GetListQueryable( rockContext )
                .Where( d => d.KnowledgeBaseFolderId == folderId.Value );
        }

        /// <inheritdoc/>
        protected override IQueryable<KnowledgeBaseDocument> GetOrderedListQueryable( IQueryable<KnowledgeBaseDocument> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( d => d.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<KnowledgeBaseDocument> GetGridBuilder()
        {
            return new GridBuilder<KnowledgeBaseDocument>()
                .WithBlock( this )
                .AddTextField( "idKey", d => d.IdKey )
                .AddTextField( "name", d => d.Name )
                .AddTextField( "sourceName", d => d.SourceName )
                .AddTextField( "sourceKey", d => d.SourceKey )
                .AddTextField( "indexStatus", d => d.IndexStatus.GetDisplayName() )
                .AddDateTimeField( "indexDateTime", d => d.IndexDateTime )
                .AddField( "isIndexDirty", d => d.IsIndexDirty );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new KnowledgeBaseDocumentService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{KnowledgeBaseDocument.FriendlyTypeName} not found." );
            }

            // Folders inherit security from the parent KnowledgeBase, so
            // delete authorization checks the KB's edit permission.
            var parentKnowledgeBase = KnowledgeBaseCache.Get( entity.KnowledgeBaseFolder.KnowledgeBaseId );

            if ( parentKnowledgeBase == null || !parentKnowledgeBase.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {KnowledgeBaseDocument.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}

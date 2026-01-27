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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.SignatureDocumentTemplateList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of signature document templates.
    /// </summary>

    [DisplayName( "Signature Document Template List" )]
    [Category( "Core" )]
    [Description( "Lists all the signature document templates and allows for managing them." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the signature document template details.",
        Key = AttributeKey.DetailPage )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "8fae9715-89f1-4faa-a35f-18cb55e269c0" )]
    [Rock.SystemGuid.BlockTypeGuid( "ffca1f50-e5fa-45b0-8d97-e2707e19bba7" )]
    [CustomizedGrid]
    public class SignatureDocumentTemplateList : RockListBlockType<SignatureDocumentTemplateList.SignatureDocumentTemplateData>
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

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SignatureDocumentTemplateListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private SignatureDocumentTemplateListOptionsBag GetBoxOptions()
        {
            var options = new SignatureDocumentTemplateListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "SignatureDocumentTemplateId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<SignatureDocumentTemplateData> GetListQueryable( RockContext rockContext )
        {
            var signatureDocumentQuery = new SignatureDocumentService( rockContext )
                .Queryable()
                .Include( sd => sd.SignatureDocumentTemplate );
            var signatureDocumentTemplateQuery = new SignatureDocumentTemplateService( rockContext )
                .Queryable()
                .Include( sdt => sdt.BinaryFileType );

            return signatureDocumentTemplateQuery.Select( s => new SignatureDocumentTemplateData
            {
                SignatureDocumentTemplate = s,
                BinaryFileTypeName = s.BinaryFileType.Name,
                Count = signatureDocumentQuery.Where( sd => sd.SignatureDocumentTemplateId == s.Id ).Count()
            } );
        }

        /// <inheritdoc/>
        protected override IQueryable<SignatureDocumentTemplateData> GetOrderedListQueryable( IQueryable<SignatureDocumentTemplateData> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( s => s.SignatureDocumentTemplate.Name );
        }

        /// <inheritdoc/>
        protected override List<SignatureDocumentTemplateData> GetListItems( IQueryable<SignatureDocumentTemplateData> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();
            var currentPerson = GetCurrentPerson();
            return items.Where( s => s.SignatureDocumentTemplate.IsAuthorized( Authorization.VIEW, currentPerson ) ).ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<SignatureDocumentTemplateData> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<SignatureDocumentTemplateData>
            {
                LavaObject = row => row.SignatureDocumentTemplate
            };

            return new GridBuilder<SignatureDocumentTemplateData>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", a => a.SignatureDocumentTemplate.IdKey )
                .AddTextField( "name", a => a.SignatureDocumentTemplate.Name )
                .AddTextField( "description", a => a.SignatureDocumentTemplate.Description )
                .AddTextField( "binaryFileType", a => a.BinaryFileTypeName )
                .AddTextField( "providerTemplateKey", a => a.SignatureDocumentTemplate.ProviderTemplateKey )
                .AddField( "documents", a => a.Count )
                .AddField( "isSecurityDisabled", a => !a.SignatureDocumentTemplate.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
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
            using ( var rockContext = new RockContext() )
            {
                var entityService = new SignatureDocumentTemplateService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{SignatureDocumentTemplate.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {SignatureDocumentTemplate.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The temporary data format to use when building the results for the
        /// grid.
        /// </summary>
        public class SignatureDocumentTemplateData
        {
            /// <summary>
            /// Gets or sets the whole SignatureDocumentTemplate object from the database.
            /// </summary>
            /// <value>
            /// The whole SignatureDocumentTemplate object from the database.
            /// </value>
            public SignatureDocumentTemplate SignatureDocumentTemplate { get; set; }

            /// <summary>
            /// Gets or sets the name of the binary file type.
            /// </summary>
            public string BinaryFileTypeName { get; set; }
            /// <summary>
            /// Gets or sets the number documents with this SignatureDocumentTemplate.
            /// </summary>
            /// <value>
            /// The number of documents with this SignatureDocumentTemplate.
            /// </value>
            public int Count { get; set; }
        }

        #endregion
    }
}

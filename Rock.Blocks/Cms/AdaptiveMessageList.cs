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
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.AdaptiveMessageList;
using Rock.Web.Cache;
using static Rock.Blocks.Cms.AdaptiveMessageList;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of adaptive messages.
    /// </summary>

    [DisplayName( "Adaptive Message List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of adaptive messages." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the adaptive message details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "9c7e8e9d-2af4-40e7-a4f9-307e114db918" )]
    [Rock.SystemGuid.BlockTypeGuid( "cba57502-8c9a-4414-b0d4-db0d57ef89bd" )]
    [CustomizedGrid]
    public class AdaptiveMessageList : RockListBlockType<AdaptiveMessageData>
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

        #region Fields

        /// <summary>
        /// The batch attributes that are configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new Lazy<List<AttributeCache>>( BuildGridAttributes );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AdaptiveMessageListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
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
        private AdaptiveMessageListOptionsBag GetBoxOptions()
        {
            var options = new AdaptiveMessageListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new AdaptiveMessage();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "AdaptiveMessageId", "((Key))" )
            };
        }

        /// <summary>
        /// Get a queryable for batches that is properly filtered.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <returns>A queryable for <see cref="FinancialBatch"/>.</returns>
        private IQueryable<AdaptiveMessage> GetAdaptiveMessageQueryable( RockContext rockContext )
        {
            var qry = new AdaptiveMessageService( rockContext )
                .Queryable();

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<AdaptiveMessageData> GetListQueryable( RockContext rockContext )
        {
            var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.ADAPTIVE_MESSAGES.AsGuid() );
            var interactionQry = new InteractionService( rockContext ).Queryable().Where( a => a.InteractionComponent.InteractionChannelId == interactionChannelId );
            return GetAdaptiveMessageQueryable( rockContext ).Select( b => new AdaptiveMessageData
            {
                AdaptiveMessage = b,
                Views = interactionQry.Where( a => a.InteractionComponent.EntityId == b.Id ).Count()
            } );
        }

        /// <inheritdoc/>
        protected override GridBuilder<AdaptiveMessageData> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<AdaptiveMessageData>
            {
                LavaObject = row => row.AdaptiveMessage
            };

            return new GridBuilder<AdaptiveMessageData>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", a => a.AdaptiveMessage.IdKey )
                .AddTextField( "name", a => a.AdaptiveMessage.Name )
                .AddTextField( "description", a => a.AdaptiveMessage.Description )
                .AddField( "isActive", a => a.AdaptiveMessage.IsActive )
                .AddTextField( "key", a => a.AdaptiveMessage.Key )
                .AddField( "categories", p => p.AdaptiveMessage.AdaptiveMessageCategories.Select( a => a.Category.Name ).ToList() )
                .AddField( "views", p => p.Views )
                .AddField( "adaptations", p => p.AdaptiveMessage.AdaptiveMessageAdaptations.Count() )
                .AddAttributeFieldsFrom( a => a.AdaptiveMessage, _gridAttributes.Value );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<AdaptiveMessage>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
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
                var entityService = new AdaptiveMessageService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{AdaptiveMessage.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {AdaptiveMessage.FriendlyTypeName}." );
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
        public class AdaptiveMessageData
        {
            /// <summary>
            /// Gets or sets the whole message object from the database.
            /// </summary>
            /// <value>
            /// The whole message object from the database.
            /// </value>
            public AdaptiveMessage AdaptiveMessage { get; set; }

            /// <summary>
            /// Gets or sets the number of transactions in this batch.
            /// </summary>
            /// <value>
            /// The number of transactions in this batch.
            /// </value>
            public int Views { get; set; }
        }

        #endregion
    }
}

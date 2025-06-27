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
using Rock.Data;
using Rock.Enums.AI;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Prayer.PrayerRequestList;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Prayer
{
    /// <summary>
    /// Displays a list of prayer requests.
    /// </summary>

    [DisplayName( "Prayer Request List" )]
    [Category( "Prayer" )]
    [Description( "Displays a list of prayer requests." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the prayer request details.",
        Key = AttributeKey.DetailPage )]

    [ContextAware( typeof( Rock.Model.Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "e8be562a-bb24-47a9-b3df-63cfb508f831" )]
    [Rock.SystemGuid.BlockTypeGuid( "e860f577-f30d-4197-87f0-c3dc6132f537" )]
    [CustomizedGrid]
    public class PrayerRequestList : RockEntityListBlockType<PrayerRequest>
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
            var box = new ListBlockBox<PrayerRequestListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = GetIsDeleteEnabled();
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
        private PrayerRequestListOptionsBag GetBoxOptions()
        {
            var options = new PrayerRequestListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Determines if the delete button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the delete button should be enabled.</returns>
        private bool GetIsDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "PrayerRequestId", "((Key))" );

            var personContext = GetContextEntity();
            if ( personContext != null )
            {
                qryParams.Add( "PersonId", personContext.Id.ToString() );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PrayerRequest> GetListQueryable( RockContext rockContext )
        {
            var qry = base.GetListQueryable( rockContext )
                .Include( a => a.Campus )
                .Include( a => a.Category );

            // Filter by person context if available
            var personContext = GetContextEntity();
            if ( personContext != null )
            {
                qry = qry.Where( p => p.RequestedByPersonAlias != null && p.RequestedByPersonAlias.PersonId == personContext.Id );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PrayerRequest> GetGridBuilder()
        {
            return new GridBuilder<PrayerRequest>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "fullName", a => a.FullName )
                .AddTextField( "campus", a => a.Campus?.Name )
                .AddTextField( "category", a => a.Category?.Name )
                .AddTextField( "text", a => a.Text )
                .AddDateTimeField( "enteredDateTime", a => a.EnteredDateTime )
                .AddField( "prayerCount", a => a.PrayerCount )
                .AddField( "flagCount", a => a.FlagCount )
                .AddField( "isApproved", a => a.IsApproved )
                .AddTextField( "moderationFlags", a => GetModerationFlagsText( a.ModerationFlags ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Converts a ModerationFlags bitmask to the text that will be displayed as a warning on the Grid.
        /// </summary>
        private string GetModerationFlagsText( ModerationFlags flags )
        {
            if ( flags == ModerationFlags.None )
            {
                return string.Empty;
            }

            var tooltipText = string.Empty;

            // Iterate through each defined flag and add its name if set.
            foreach ( ModerationFlags flag in Enum.GetValues( typeof( ModerationFlags ) ) )
            {
                if ( flag != ModerationFlags.None && flags.HasFlag( flag ) )
                {
                    tooltipText += GetTooltipText( flag );
                }
            }

            return tooltipText;
        }

        /// <summary>
        /// Get the tooltip text for a given ModerationFlag.
        /// </summary>
        /// <param name="flag">The given moderation flag</param>
        /// <returns>The Tooltip text</returns>
        private string GetTooltipText( ModerationFlags flag )
        {
            switch ( flag )
            {
                case ModerationFlags.Hate:
                    return "Flagged for hate. ";

                case ModerationFlags.Threat:
                    return "Flagged for threatening content. ";

                case ModerationFlags.SelfHarm:
                    return "Flagged for self-harm. ";

                case ModerationFlags.Sexual:
                    return "Flagged for sexual content. ";

                case ModerationFlags.SexualMinor:
                    return "Flagged for sexual content involving minors. ";

                case ModerationFlags.Violent:
                    return "Flagged for violent content. ";

                default:
                    return string.Empty;
            }
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
            var entityService = new PrayerRequestService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{PrayerRequest.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {PrayerRequest.FriendlyTypeName}." );
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

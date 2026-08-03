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
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Prayer.PrayerRequestList;
using Rock.ViewModels.Controls;
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
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the prayer request details.",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [IntegerField( "Expires After (days)",
        Description = "Number of days until the request will expire.",
        IsRequired = false,
        DefaultIntegerValue = 14,
        Key = AttributeKey.ExpireDays,
        Order = 1 )]

    [BooleanField( "Show Prayer Count",
        Description = "If enabled, the block will show the current prayer count for each request in the list.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowPrayerCount,
        Order = 2 )]

    [BooleanField( "Show 'Approved' column",
        Description = "If enabled, the Approved column will be shown with a Yes/No toggle button.",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowApprovedColumn,
        Order = 3 )]

    [BooleanField( "Show Grid Filter",
        Description = "If enabled, the grid filter will be visible.",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowGridFilter,
        Order = 4 )]

    [BooleanField( "Show Public Only",
        Description = "If enabled, it will limit the list only to the prayer requests that are public.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowPublicOnly,
        Order = 5 )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve prayer requests." )]

    [ContextAware( typeof( Rock.Model.Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "e8be562a-bb24-47a9-b3df-63cfb508f831" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "e860f577-f30d-4197-87f0-c3dc6132f537" )]
    [Rock.SystemGuid.BlockTypeGuid( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74" )]
    [CustomizedGrid]
    public class PrayerRequestList : RockEntityListBlockType<PrayerRequest>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ExpireDays = "ExpireDays";
            public const string ShowPrayerCount = "ShowPrayerCount";
            public const string ShowApprovedColumn = "ShowApprovedColumn";
            public const string ShowGridFilter = "ShowGridFilter";
            public const string ShowPublicOnly = "ShowPublicOnly";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterPublicOrPrivate = "filter-public-private";
            public const string FilterActive = "filter-active";
            public const string FilterUrgent = "filter-urgent";
            public const string FilterCommenting = "filter-commenting";
            public const string FilterShowExpiredRequests = "filter-show-expired-requests";
            public const string FilterDateRange = "filter-date-range";
        }

        #region Properties

        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        protected string FilterPublicOrPrivate => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterPublicOrPrivate );

        protected string FilterActive => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterActive );

        protected string FilterUrgent => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterUrgent );

        protected string FilterCommenting => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterCommenting );

        private bool FilterShowExpiredRequests => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterShowExpiredRequests )
            .AsBoolean();

        private SlidingDateRangeBag FilterDateRange => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull();

        #endregion Properties

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

            // The Approved column is only shown when the block setting is enabled AND
            // the current person is authorized to approve prayer requests.
            options.ShowIsApprovedColumn = GetAttributeValue( AttributeKey.ShowApprovedColumn ).AsBoolean()
                && IsPersonApproveAuthorized();
            options.IsCampusColumnVisible = CampusCache.All( false ).Count > 1;
            options.IsPrayerCountColumnVisible = GetAttributeValue( AttributeKey.ShowPrayerCount ).AsBoolean();
            options.IsGridFilterVisible = GetAttributeValue( AttributeKey.ShowGridFilter ).AsBooleanOrNull() ?? true;
            options.IsPublicOnly = GetAttributeValue( AttributeKey.ShowPublicOnly ).AsBoolean();

            // When the block is scoped to a specific person via context, every row will
            // belong to that same person, so hide the Name column to reduce clutter.
            // Only Person context collapses the column; other context types (e.g. Group)
            // still show different people per row and should keep the Name visible.
            options.IsNameColumnVisible = RequestContext.GetContextEntity<Person>() == null;

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
            IQueryable<PrayerRequest> qry = base.GetListQueryable( rockContext )
                .Include( a => a.Campus )
                .Include( a => a.Category );

            // Filter by person context if available
            var personContext = GetContextEntity();
            if ( personContext != null )
            {
                qry = qry.Where( p => p.RequestedByPersonAlias != null && p.RequestedByPersonAlias.PersonId == personContext.Id );
            }

            // If the block is configured to only show public prayer requests, enforce
            // that here regardless of the individual's public/private filter preference.
            if ( GetAttributeValue( AttributeKey.ShowPublicOnly ).AsBoolean() )
            {
                qry = qry.Where( p => p.IsPublic == true );
            }

            // Filter by IsPublic
            if ( !string.IsNullOrWhiteSpace( FilterPublicOrPrivate ) )
            {
                if ( FilterPublicOrPrivate.Equals( "Public", StringComparison.OrdinalIgnoreCase ) )
                {
                    qry = qry.Where( p => p.IsPublic == true );
                }
                else if ( FilterPublicOrPrivate.Equals( "Private", StringComparison.OrdinalIgnoreCase ) )
                {
                    qry = qry.Where( p => p.IsPublic == false );
                }
            }

            // Filter by IsActive
            if ( !string.IsNullOrWhiteSpace( FilterActive ) )
            {
                if ( FilterActive.Equals( "Active", StringComparison.OrdinalIgnoreCase ) )
                {
                    qry = qry.Where( p => p.IsActive == true );
                }
                else if ( FilterActive.Equals( "Inactive", StringComparison.OrdinalIgnoreCase ) )
                {
                    qry = qry.Where( p => p.IsActive == false );
                }
            }

            // Filter by IsUrgent
            if ( !string.IsNullOrWhiteSpace( FilterUrgent ) )
            {
                if ( FilterUrgent.Equals( "Urgent", StringComparison.OrdinalIgnoreCase ) )
                {
                    qry = qry.Where( p => p.IsUrgent == true );
                }
                else if ( FilterUrgent.Equals( "Non-Urgent", StringComparison.OrdinalIgnoreCase ) )
                {
                    qry = qry.Where( p => p.IsUrgent == false );
                }
            }

            // Filter by AllowComments
            if ( !string.IsNullOrWhiteSpace( FilterCommenting ) )
            {
                if ( FilterCommenting.Equals( "Allowed", StringComparison.OrdinalIgnoreCase ) )
                {
                    qry = qry.Where( p => p.AllowComments == true );
                }
                else if ( FilterCommenting.Equals( "Not Allowed", StringComparison.OrdinalIgnoreCase ) )
                {
                    qry = qry.Where( p => p.AllowComments == false );
                }
            }

            // Filter by the entered date range. This is always applied and defaults to the
            // last 3 months so the grid never materializes an unbounded number of rows (which
            // is what enabling 'Show Expired Requests' would otherwise do, since requests
            // accumulate over time). The individual can widen the range as needed.
            var defaultDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 3
            };

            var dateRange = FilterDateRange.Validate( defaultDateRange ).ActualDateRange;
            if ( dateRange.Start.HasValue )
            {
                qry = qry.Where( p => p.EnteredDateTime >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                qry = qry.Where( p => p.EnteredDateTime < dateRange.End.Value );
            }

            // If 'Show Expired Requests' is false, filter them out... they're included
            // by default. Compare against Today (midnight) rather than Now so a request
            // remains visible for the entire day it is scheduled to expire, matching the
            // long-standing webforms behavior.
            if ( !FilterShowExpiredRequests )
            {
                var today = RockDateTime.Today;
                qry = qry.Where( p => !p.ExpirationDate.HasValue || today <= p.ExpirationDate );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<PrayerRequest> GetOrderedListQueryable( IQueryable<PrayerRequest> queryable, RockContext rockContext )
        {
            return queryable
                .OrderByDescending( p => p.EnteredDateTime )
                .ThenBy( p => p.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<PrayerRequest> GetGridBuilder()
        {
            var builder = new GridBuilder<PrayerRequest>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "fullName", a => a.FullName )
                .AddTextField( "campus", a => a.Campus?.Name )
                .AddTextField( "category", a => a.Category?.Name )
                .AddTextField( "text", a => a.Text )
                .AddDateTimeField( "enteredDateTime", a => a.EnteredDateTime )
                // Coalesce the nullable counts to 0 so numeric column filters (e.g.
                // "Less Than 2") include requests that have never been prayed for or
                // flagged. Sending null causes those rows to be excluded from any
                // numeric comparison on the client.
                .AddField( "prayerCount", a => a.PrayerCount ?? 0 )
                .AddField( "flagCount", a => a.FlagCount ?? 0 )
                .AddTextField( "moderationFlags", a => GetModerationFlagsText( a.ModerationFlags ) )
                .AddAttributeFields( GetGridAttributes() );

            if ( IsPersonApproveAuthorized() )
            {
                builder.AddField( "isApproved", a => a.IsApproved );
            }

            return builder;
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

        /// <summary>
        /// Determines whether the current Person has either edit or administrative authorization for the block.
        /// </summary>
        /// <remarks>This method checks the current Person's permissions against the block's authorization
        /// settings  for the "Edit" and "Administrate" roles.</remarks>
        /// <returns><see langword="true"/> if the current Person is authorized with either edit or administrative permissions;
        /// otherwise, <see langword="false"/>.</returns>
        private bool IsPersonEditOrAdminAuthorized()
        {
            var currentPerson = RequestContext.CurrentPerson;
            var allowedAuthorizations = new[] { Authorization.EDIT, Authorization.ADMINISTRATE };

            if ( allowedAuthorizations.Any( auth => BlockCache.IsAuthorized( auth, currentPerson ) ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the current Person has authorization to approve prayer requests
        /// via the block's "Approve" security action.
        /// </summary>
        private bool IsPersonApproveAuthorized()
        {
            return BlockCache.IsAuthorized( Authorization.APPROVE, RequestContext.CurrentPerson );
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult UpdateApprovalStatus( string prayerRequestIdKey, bool isApproved )
        {
            var entityService = new PrayerRequestService( RockContext );
            var entity = entityService.Get( prayerRequestIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{PrayerRequest.FriendlyTypeName} not found." );
            }

            if ( !IsPersonApproveAuthorized() )
            {
                return ActionBadRequest( $"Not authorized to update approval status of {PrayerRequest.FriendlyTypeName}." );
            }

            entity.IsApproved = isApproved;

            // When a request is approved, capture who approved it and when, reset any
            // moderator flags that had accumulated, and extend the expiration date by
            // the number of days configured on the block. This mirrors the behavior of
            // the original webforms block and prevents freshly approved requests from
            // being hidden by the expired-requests filter. (GitHub issue #6950)
            if ( isApproved )
            {
                entity.ApprovedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                entity.ApprovedOnDateTime = RockDateTime.Now;

                if ( entity.FlagCount.HasValue && entity.FlagCount > 0 )
                {
                    entity.FlagCount = 0;
                }

                var expireDays = GetAttributeValue( AttributeKey.ExpireDays ).AsIntegerOrNull() ?? 14;
                entity.ExpirationDate = RockDateTime.Now.AddDays( expireDays );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

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

            if ( !IsPersonEditOrAdminAuthorized() )
            {
                return ActionBadRequest( $"Not authorized to delete {PrayerRequest.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Remove related notes (comments) before deleting the request itself.
            // Notes reference the prayer request polymorphically (by EntityTypeId and
            // EntityId), so there is no FK to catch orphans automatically. This mirrors
            // the webforms Delete behavior.
            DeleteAllRelatedNotes( entity, RockContext );

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes all comments/notes related to the given prayer request.
        /// </summary>
        /// <param name="prayerRequest">The prayer request whose notes should be removed.</param>
        /// <param name="rockContext">The Rock Context.</param>
        private void DeleteAllRelatedNotes( PrayerRequest prayerRequest, RockContext rockContext )
        {
            var prayerRequestEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.PRAYER_REQUEST.AsGuid() ).Id;
            var noteTypeIdsForPrayerRequest = EntityNoteTypesCache.Get()
                .EntityNoteTypes
                .First( a => a.EntityTypeId.Equals( prayerRequestEntityTypeId ) )
                .NoteTypeIds;
            var noteService = new NoteService( rockContext );
            var prayerRequestComments = noteService.Queryable()
                .Where( n => noteTypeIdsForPrayerRequest.Contains( n.NoteTypeId ) && n.EntityId == prayerRequest.Id );
            rockContext.BulkDelete( prayerRequestComments );
        }

        #endregion
    }
}

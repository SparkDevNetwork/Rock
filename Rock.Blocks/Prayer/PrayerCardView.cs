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
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Prayer.PrayerCardView;
using Rock.Web.Cache;

namespace Rock.Blocks.Prayer
{
    /// <summary>
    /// Provides an additional experience to pray using a card based view.
    /// </summary>
    [DisplayName( "Prayer Card View" )]
    [Category( "Prayer" )]
    [Description( "Provides an additional experience to pray using a card based view." )]
    [IconCssClass( "ti ti-grid-dots" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [TextField( "Prayed Button Text",
        Description = "The text to display inside the Prayed button.",
        Key = AttributeKey.PrayedButtonText,
        DefaultValue = "I Prayed",
        IsRequired = true,
        Order = 1 )]

    [CategoryField( "Category",
        Description = "A top level category. This controls which categories are shown when starting a prayer session.",
        Key = AttributeKey.Category,
        EntityTypeName = "Rock.Model.PrayerRequest",
        AllowMultiple = false,
        IsRequired = false,
        Category = AttributeCategory.Filtering,
        Order = 2 )]

    [BooleanField( "Public Only",
        Description = "If selected, all non-public prayer request will be excluded.",
        Key = AttributeKey.PublicOnly,
        DefaultBooleanValue = true,
        IsRequired = true,
        Category = AttributeCategory.Filtering,
        Order = 3 )]

    [BooleanField( "Enable Prayer Team Flagging",
        Description = "If enabled, members of the prayer team can flag a prayer request if they feel the request is inappropriate and needs review by an administrator.",
        Key = AttributeKey.EnablePrayerTeamFlagging,
        DefaultBooleanValue = false,
        Category = AttributeCategory.Flagging,
        Order = 4 )]

    [IntegerField( "Flag Limit",
        Description = "The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.",
        Key = AttributeKey.FlagLimit,
        DefaultIntegerValue = 1,
        IsRequired = false,
        Category = AttributeCategory.Flagging,
        Order = 5 )]

    [EnumField( "Order",
        Description = "The order that the requests should be displayed.",
        Key = AttributeKey.Order,
        EnumSourceType = typeof( PrayerRequestOrder ),
        DefaultEnumValue = ( int ) PrayerRequestOrder.LeastPrayedFor,
        IsRequired = true,
        Category = AttributeCategory.Filtering,
        Order = 6 )]

    [BooleanField( "Show Campus Filter",
        Description = "Shows or hides the campus filter.",
        Key = AttributeKey.ShowCampusFilter,
        DefaultBooleanValue = false,
        Category = AttributeCategory.Filtering,
        Order = 7 )]

    [DefinedValueField( "Campus Types",
        Description = "Allows selecting which campus types to filter campuses by.",
        Key = AttributeKey.CampusTypes,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        IsRequired = false,
        Category = AttributeCategory.Filtering,
        Order = 8 )]

    [DefinedValueField( "Campus Statuses",
        Description = "This allows selecting which campus statuses to filter campuses by.",
        Key = AttributeKey.CampusStatuses,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        IsRequired = false,
        Category = AttributeCategory.Filtering,
        Order = 9 )]

    [IntegerField( "Max Results",
        Description = "The maximum number of requests to display. Leave blank for all.",
        Key = AttributeKey.MaxResults,
        IsRequired = false,
        Category = AttributeCategory.Filtering,
        Order = 10 )]

    [WorkflowTypeField( "Prayed Workflow",
        Description = "The workflow type to launch when someone presses the Pray button. Prayer Request will be passed to the workflow as a generic \"Entity\" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: PrayerOfferedByPersonAliasGuid, PrayerOfferedByPerson.",
        Key = AttributeKey.PrayedWorkflow,
        AllowMultiple = false,
        IsRequired = false,
        Order = 11 )]

    [WorkflowTypeField( "Flagged Workflow",
        Description = "The workflow type to launch when someone presses the Flag button. Prayer Request will be passed to the workflow as a generic \"Entity\" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: FlaggedByPersonId.",
        Key = AttributeKey.FlaggedWorkflow,
        AllowMultiple = false,
        IsRequired = false,
        Category = AttributeCategory.Flagging,
        Order = 12 )]

    [BooleanField( "Load Last Prayed Collection",
        Description = "Loads an optional collection of last prayed times for the requests. This is available as a separate merge field in Lava.",
        Key = AttributeKey.LoadLastPrayedCollection,
        DefaultBooleanValue = false,
        Order = 13 )]

    [BooleanField( "Create Interactions for Prayers",
        Description = "If enabled then this block will record an Interaction whenever somebody prays for a prayer request.",
        Key = AttributeKey.CreateInteractionsForPrayers,
        DefaultBooleanValue = true,
        IsRequired = true,
        Order = 14 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "85FE88A7-0E8E-41E2-805E-1FA9E9BC2D73" )]
    [Rock.SystemGuid.BlockTypeGuid( "2B0B4AED-0D65-42B0-B1E6-20E904966986" )]
    public class PrayerCardView : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PrayedButtonText = "PrayedButtonText";
            public const string Category = "Category";
            public const string PublicOnly = "PublicOnly";
            public const string EnablePrayerTeamFlagging = "EnablePrayerTeamFlagging";
            public const string FlagLimit = "FlagLimit";
            public const string Order = "Order";
            public const string ShowCampusFilter = "ShowCampusFilter";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string MaxResults = "MaxResults";
            public const string PrayedWorkflow = "PrayedWorkflow";
            public const string FlaggedWorkflow = "FlaggedWorkflow";
            public const string LoadLastPrayedCollection = "LoadLastPrayedCollection";
            public const string CreateInteractionsForPrayers = "CreateInteractionsForPrayers";
        }

        private static class AttributeCategory
        {
            public const string Filtering = "Filtering";
            public const string Flagging = "Flagging";
        }

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string CategoryId = "CategoryId";
            public const string GroupGuid = "GroupGuid";
        }

        private static class PersonPreferenceKey
        {
            /// <summary>
            /// Same key and Id-based value the WebForms block used so saved selections survive the upgrade.
            /// </summary>
            public const string Campus = "selected-campus";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PrayerCardViewBag, PrayerCardViewOptionsBag>
            {
                Options = GetOptionsBag(),
                Bag = GetContentBag()
            };

            return box;
        }

        /// <summary>
        /// Builds the static configuration options from the block settings.
        /// </summary>
        /// <returns>The populated options bag.</returns>
        private PrayerCardViewOptionsBag GetOptionsBag()
        {
            return new PrayerCardViewOptionsBag
            {
                IsCampusFilterVisible = IsCampusFilterEnabled(),
                CampusTypeFilterGuids = GetCampusTypeGuids(),
                CampusStatusFilterGuids = GetCampusStatusGuids(),
                PrayedButtonText = GetAttributeValue( AttributeKey.PrayedButtonText ),
                IsPrayerTeamFlaggingEnabled = GetAttributeValue( AttributeKey.EnablePrayerTeamFlagging ).AsBoolean()
            };
        }

        /// <summary>
        /// Builds the bag that carries the prayer request cards and the person's
        /// current campus selection. Used by the initial load and again after
        /// the campus filter changes so both produce identical output.
        /// </summary>
        /// <returns>The populated content bag.</returns>
        private PrayerCardViewBag GetContentBag()
        {
            var eligibleCampuses = GetEligibleCampuses();
            var selectedCampus = GetSavedCampus( eligibleCampuses );
            var campusGuids = GetCampusFilterGuids( selectedCampus, eligibleCampuses );
            var prayerRequests = GetPrayerRequests( campusGuids );

            return new PrayerCardViewBag
            {
                PrayerRequests = prayerRequests.Select( BuildCardBag ).ToList(),
                SelectedCampus = selectedCampus?.ToListItemBag()
            };
        }

        /// <summary>
        /// Maps a prayer request to the display data a single card needs.
        /// </summary>
        /// <param name="prayerRequest">The prayer request to describe.</param>
        /// <returns>The populated card bag.</returns>
        private PrayerRequestCardBag BuildCardBag( PrayerRequest prayerRequest )
        {
            return new PrayerRequestCardBag
            {
                IdKey = prayerRequest.IdKey,
                FirstName = prayerRequest.FirstName,
                LastName = prayerRequest.LastName,
                Text = prayerRequest.Text,
                CategoryName = prayerRequest.CategoryId.HasValue ? CategoryCache.Get( prayerRequest.CategoryId.Value )?.Name : null
            };
        }

        /// <summary>
        /// Gets the prayer requests that match the block settings, page
        /// parameters and the supplied campus filter, ordered and limited in
        /// the database.
        /// </summary>
        /// <param name="campusGuids">The campuses to filter by, or <c>null</c> for no campus filter.</param>
        /// <returns>The prayer requests to display.</returns>
        private List<PrayerRequest> GetPrayerRequests( List<Guid> campusGuids )
        {
            var categoryGuid = GetCategoryGuid();
            var groupGuid = PageParameter( PageParameterKey.GroupGuid ).AsGuidOrNull();

            var qry = new PrayerRequestService( RockContext ).GetPrayerRequests( new PrayerRequestQueryOptions
            {
                IncludeEmptyCampus = true,
                IncludeNonPublic = !GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean(),
                Campuses = campusGuids,
                Categories = categoryGuid.HasValue ? new List<Guid> { categoryGuid.Value } : null,
                GroupGuids = groupGuid.HasValue ? new List<Guid> { groupGuid.Value } : null,
                IncludeGroupRequests = !groupGuid.HasValue
            } );

            var order = GetAttributeValue( AttributeKey.Order ).ConvertToEnum<PrayerRequestOrder>( PrayerRequestOrder.LeastPrayedFor );
            var orderedQry = qry.OrderBy( order );

            var maxResults = GetAttributeValue( AttributeKey.MaxResults ).AsIntegerOrNull();
            if ( maxResults.HasValue && maxResults.Value > 0 )
            {
                return orderedQry.Take( maxResults.Value ).ToList();
            }

            return orderedQry.ToList();
        }

        /// <summary>
        /// Gets the category to filter by: the configured block setting, or the
        /// CategoryId page parameter when no category is configured.
        /// </summary>
        /// <returns>The category unique identifier, or <c>null</c> for no category filter.</returns>
        private Guid? GetCategoryGuid()
        {
            var categoryGuid = GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
            if ( categoryGuid.HasValue )
            {
                return categoryGuid;
            }

            var category = CategoryCache.Get( PageParameter( PageParameterKey.CategoryId ), !PageCache.Layout.Site.DisablePredictableIds );

            return category?.Guid;
        }

        /// <summary>
        /// Gets the campus supplied by the CampusId page parameter, if any.
        /// When present it drives the filter and hides the campus picker.
        /// </summary>
        /// <returns>The campus, or <c>null</c> when the parameter is missing or invalid.</returns>
        private CampusCache GetPageParameterCampus()
        {
            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            return campusId.HasValue ? CampusCache.Get( campusId.Value ) : null;
        }

        /// <summary>
        /// Determines whether the campus filter is enabled: the block setting is
        /// on and no valid CampusId page parameter has already fixed the campus.
        /// </summary>
        /// <returns><c>true</c> if the campus filter should be used; otherwise <c>false</c>.</returns>
        private bool IsCampusFilterEnabled()
        {
            return GetAttributeValue( AttributeKey.ShowCampusFilter ).AsBoolean() && GetPageParameterCampus() == null;
        }

        /// <summary>
        /// Gets the campus type unique identifiers configured to limit the campus filter.
        /// </summary>
        /// <returns>The campus type unique identifiers.</returns>
        private List<Guid> GetCampusTypeGuids()
        {
            return GetAttributeValue( AttributeKey.CampusTypes ).SplitDelimitedValues().AsGuidList();
        }

        /// <summary>
        /// Gets the campus status unique identifiers configured to limit the campus filter.
        /// </summary>
        /// <returns>The campus status unique identifiers.</returns>
        private List<Guid> GetCampusStatusGuids()
        {
            return GetAttributeValue( AttributeKey.CampusStatuses ).SplitDelimitedValues().AsGuidList();
        }

        /// <summary>
        /// Gets the active campuses that pass the configured type and status
        /// filters. This is the same list the client-side campus picker shows,
        /// so filtering by "all campuses" uses exactly these campuses.
        /// </summary>
        /// <returns>The eligible campuses, or an empty list when the filter is not enabled.</returns>
        private List<CampusCache> GetEligibleCampuses()
        {
            if ( !IsCampusFilterEnabled() )
            {
                return new List<CampusCache>();
            }

            var campusTypeIds = GetDefinedValueIds( GetCampusTypeGuids() );
            var campusStatusIds = GetDefinedValueIds( GetCampusStatusGuids() );

            return CampusCache.All( false )
                .Where( c => !campusTypeIds.Any() || ( c.CampusTypeValueId.HasValue && campusTypeIds.Contains( c.CampusTypeValueId.Value ) ) )
                .Where( c => !campusStatusIds.Any() || ( c.CampusStatusValueId.HasValue && campusStatusIds.Contains( c.CampusStatusValueId.Value ) ) )
                .OrderBy( c => c.Order )
                .ToList();
        }

        /// <summary>
        /// Resolves defined value unique identifiers to their identifiers,
        /// skipping any that no longer exist.
        /// </summary>
        /// <param name="definedValueGuids">The defined value unique identifiers.</param>
        /// <returns>The matching defined value identifiers.</returns>
        private static List<int> GetDefinedValueIds( List<Guid> definedValueGuids )
        {
            return definedValueGuids
                .Select( g => DefinedValueCache.Get( g ) )
                .Where( dv => dv != null )
                .Select( dv => dv.Id )
                .ToList();
        }

        /// <summary>
        /// Gets the campus the person previously selected in the filter,
        /// provided it is still one of the eligible campuses.
        /// </summary>
        /// <param name="eligibleCampuses">The campuses the filter offers.</param>
        /// <returns>The saved campus, or <c>null</c> when none is saved or it is no longer eligible.</returns>
        private CampusCache GetSavedCampus( List<CampusCache> eligibleCampuses )
        {
            if ( !IsCampusPickerVisible( eligibleCampuses ) )
            {
                return null;
            }

            var savedCampusId = GetBlockPersonPreferences().GetValue( PersonPreferenceKey.Campus ).AsIntegerOrNull();
            if ( !savedCampusId.HasValue )
            {
                return null;
            }

            return eligibleCampuses.FirstOrDefault( c => c.Id == savedCampusId.Value );
        }

        /// <summary>
        /// Determines whether the campus picker is actually shown. The picker
        /// control hides itself unless more than one campus is available, so
        /// the server applies the same rule before honoring a saved selection.
        /// </summary>
        /// <param name="eligibleCampuses">The campuses the filter offers.</param>
        /// <returns><c>true</c> if the picker is shown; otherwise <c>false</c>.</returns>
        private bool IsCampusPickerVisible( List<CampusCache> eligibleCampuses )
        {
            return IsCampusFilterEnabled() && eligibleCampuses.Count > 1;
        }

        /// <summary>
        /// Determines which campuses the prayer request query should be limited
        /// to. A CampusId page parameter wins; otherwise the saved selection is
        /// used; otherwise every eligible campus is included so the type and
        /// status filters still apply. Requests without a campus are always
        /// included by the query options.
        /// </summary>
        /// <param name="selectedCampus">The person's saved campus, if any.</param>
        /// <param name="eligibleCampuses">The campuses the filter offers.</param>
        /// <returns>The campus unique identifiers to filter by, or <c>null</c> for no campus filter.</returns>
        private List<Guid> GetCampusFilterGuids( CampusCache selectedCampus, List<CampusCache> eligibleCampuses )
        {
            var pageParameterCampus = GetPageParameterCampus();
            if ( pageParameterCampus != null )
            {
                return new List<Guid> { pageParameterCampus.Guid };
            }

            if ( !IsCampusPickerVisible( eligibleCampuses ) )
            {
                return null;
            }

            if ( selectedCampus != null )
            {
                return new List<Guid> { selectedCampus.Guid };
            }

            return eligibleCampuses.Select( c => c.Guid ).ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Records that the current person prayed for the specified request.
        /// </summary>
        /// <param name="idKey">The identifier of the prayer request that was prayed for.</param>
        /// <returns>An empty 200-OK response.</returns>
        [BlockAction]
        public BlockActionResult PrayRequest( string idKey )
        {
            return ActionOk();
        }

        /// <summary>
        /// Flags the specified request as inappropriate for administrator review.
        /// </summary>
        /// <param name="idKey">The identifier of the prayer request to flag.</param>
        /// <returns>An empty 200-OK response.</returns>
        [BlockAction]
        public BlockActionResult FlagPrayerRequest( string idKey )
        {
            return ActionOk();
        }

        /// <summary>
        /// Saves the person's campus selection and returns the re-rendered cards.
        /// </summary>
        /// <param name="campusGuid">The unique identifier of the selected campus, or empty for all campuses.</param>
        /// <returns>The refreshed block data.</returns>
        [BlockAction]
        public BlockActionResult SelectCampus( string campusGuid )
        {
            return ActionOk( new PrayerCardViewBag
            {
                PrayerRequests = new List<PrayerRequestCardBag>(),
                SelectedCampus = null
            } );
        }

        #endregion Block Actions
    }
}

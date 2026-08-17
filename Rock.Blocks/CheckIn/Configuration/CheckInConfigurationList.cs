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
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// Displays a list of check-in configurations.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Check-in Configuration List" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Displays a list of check-in configurations." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Show Classic Label Settings",
        Key = AttributeKey.ShowClassicLabelSettings,
        Description = "Show the page link under Related Settings that allows the configuration of Classic Labels.",
        DefaultBooleanValue = true,
        Order = 0,
        IsRequired = false )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "C385BBCD-E0A1-4003-8CCE-487C6B845DED" )]
    [Rock.SystemGuid.BlockTypeGuid( "41233A39-404A-478F-A7FC-536B644E6728" )]
    public class CheckInConfigurationList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ShowClassicLabelSettings = "ShowClassicLabelSettings";
        }

        private static class NavigationUrlKey
        {
            // Per-config URLs:
            public const string AreasAndGroupsPage = "AreasAndGroupsPage";
            public const string ScheduleBuilderPage = "ScheduleBuilderPage";
            public const string ConfigurationSettingsPage = "ConfigurationSettingsPage";

            // Related settings URLs:
            public const string NamedLocationsPage = "NamedLocationsPage";
            public const string SchedulesPage = "SchedulesPage";

            public const string DevicesPage = "DevicesPage";
            public const string LabelsPage = "LabelsPage";
            public const string ClassicLabelsPage = "ClassicLabelsPage";
            public const string CloudPrintPage = "CloudPrintPage";

            public const string ClassicLabelMergeFields = "ClassicLabelMergeFields";
            public const string AbilityLevels = "AbilityLevels";
            public const string SearchType = "SearchType";

            // Public-Facing Docs URLs:
            public const string CheckInManual = "CheckInManual";
        }

        private static class PageParameterKey
        {
            public const string CheckInConfiguration = "CheckInConfiguration";
        }

        private static class PersonPreferenceKey
        {
            public const string SortBy = "sort-by";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The backing field for the <see cref="SortBy"/> property.
        /// </summary>
        private List<ListItemBag> _sortyByItems;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the list of "sort by" items the individual may select.
        /// </summary>
        private List<ListItemBag> SortByItems
        {
            get
            {
                if ( _sortyByItems == null )
                {
                    _sortyByItems = new List<ListItemBag>
                    {
                        SortBy.MostActivity,
                        SortBy.Alphabetical
                    };
                }

                return _sortyByItems;
            }
        }

        /// <summary>
        /// Gets the identifier of the check-in template group type purpose <see cref="DefinedValue"/>.
        /// </summary>
        private int CheckInTemplatePurposeValueId => DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() ) ?? 0;

        /// <summary>
        /// Gets whether the current person is authorized to administrate this block.
        /// </summary>
        public bool CanAdministrate => BlockCache.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() );

        /// <summary>
        /// Gets whether to show the page link under related settings that allows the configuration of classic labels.
        /// </summary>
        private bool ShowClassicLabelSettings => GetAttributeValue( AttributeKey.ShowClassicLabelSettings ).AsBoolean();

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets the current person's "sort by" preference.
        /// </summary>
        private string SortByPreference
        {
            get
            {
                var sortBy = BlockPersonPreferences
                    .GetValue( PersonPreferenceKey.SortBy );

                if ( sortBy.IsNotNullOrWhiteSpace() )
                {
                    return sortBy;
                }

                return SortBy.MostActivityValue;
            }
        }

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CheckInConfigurationListInitializationBox();

            if ( CheckInTemplatePurposeValueId == 0 )
            {
                box.ErrorMessage = "Unable to determine Check-in Template Group Type Purpose identifier";
            }

            box.SortByItems = SortByItems;
            box.SortBy = SortByPreference;
            box.ShowAddCheckInConfigurationButton = CanAdministrate;
            box.ShowClassicLabelSettings = GetAttributeValue( AttributeKey.ShowClassicLabelSettings ).AsBoolean();
            box.CheckInConfigurations = LoadCheckInConfigurations();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Gets the check-in configurations.
        /// </summary>
        /// <returns>An object containing information about the check-in configurations.</returns>
        [BlockAction]
        public BlockActionResult GetCheckInConfigurations()
        {
            var response = LoadCheckInConfigurations();

            return ActionOk( response );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Loads the check-in configurations from the database.
        /// </summary>
        /// <returns>A list of <see cref="CheckInConfigurationBag"/>s.</returns>
        private List<CheckInConfigurationBag> LoadCheckInConfigurations()
        {
            // Determine which check-in configurations this person is authorized to view before going to the database.
            var currentPerson = GetCurrentPerson();
            var authorizedCheckInConfigurationIds = GroupTypeCache.All()
                .Where( gt =>
                    gt.GroupTypePurposeValueId == CheckInTemplatePurposeValueId
                    && gt.IsAuthorized( Authorization.VIEW, currentPerson )
                )
                .Select( gt => gt.Id )
                .ToList();

            if ( !authorizedCheckInConfigurationIds.Any() )
            {
                return new List<CheckInConfigurationBag>();
            }

            var groupTypeService = new GroupTypeService( RockContext );

            List<CheckInConfigurationBag> checkInConfigs = null;

            if ( SortByPreference == SortBy.MostActivityValue )
            {
                var twentyEightDaysAgo = RockDateTime.Today.AddDays( -28 );
                var attendanceQry = new AttendanceService( RockContext ).Queryable();

                checkInConfigs = groupTypeService
                    .Queryable()
                    .Where( gt => authorizedCheckInConfigurationIds.Contains( gt.Id ) )
                    .OrderByDescending( gt => attendanceQry
                        .Count( a =>
                            a.DidAttend == true &&
                            a.StartDateTime >= twentyEightDaysAgo &&
                            a.Occurrence.RootGroupTypeId == gt.Id
                        )
                    )
                    .ThenBy( gt => gt.Name )
                    .Select( gt => new CheckInConfigurationBag
                    {
                        Id = gt.Id,
                        Name = gt.Name,
                        Description = gt.Description,
                        IconCssClass = gt.IconCssClass
                    } )
                    .ToList();
            }
            else
            {
                checkInConfigs = groupTypeService
                    .Queryable()
                    .Where( gt => authorizedCheckInConfigurationIds.Contains( gt.Id ) )
                    .OrderBy( gt => gt.Name )
                    .Select( gt => new CheckInConfigurationBag
                    {
                        Id = gt.Id,
                        Name = gt.Name,
                        Description = gt.Description,
                        IconCssClass = gt.IconCssClass
                    } )
                    .ToList();
            }

            checkInConfigs.ForEach( c => c.TranslateIdToIdKey() );

            return checkInConfigs;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var urls = new Dictionary<string, string>
            {
                // Per-config URLs:
                [NavigationUrlKey.AreasAndGroupsPage] = "/admin/checkin/configuration-areas-groups/((Key))",
                [NavigationUrlKey.ScheduleBuilderPage] = "/admin/checkin/configuration-schedule-builder/((Key))",
                [NavigationUrlKey.ConfigurationSettingsPage] = "/admin/checkin/configuration-settings/((Key))",

                // Related settings URLs:
                [NavigationUrlKey.NamedLocationsPage] = "/admin/checkin/named-locations",
                [NavigationUrlKey.SchedulesPage] = "/admin/checkin/schedules",

                [NavigationUrlKey.DevicesPage] = "/admin/checkin/devices",
                [NavigationUrlKey.LabelsPage] = "/admin/checkin/labels",
                [NavigationUrlKey.CloudPrintPage] = "/admin/checkin/cloud-print",

                [NavigationUrlKey.AbilityLevels] = "/admin/checkin/ability-levels",
                [NavigationUrlKey.SearchType] = "/admin/checkin/search-types",

                // Public-Facing Docs URLs:
                /*
                    5/27/2026 - JPH

                    This URL is a Page Short Link managed by the Spark Site. This way, if the actual URL changes in
                    the future, we can simply update the Short Link to point to the new URL without needing to update
                    the block's code and redeploy.

                    Reason: Mitigate risks with hard-coding a URL that may change in the future.
                */
                [NavigationUrlKey.CheckInManual] = "https://community.rockrms.com/app-check-in-configuration"
            };

            // Label Merge Fields are a Classic Labels concept (next-gen labels use their own field data sources), so
            // surface both links only when Classic label settings are enabled.
            if ( ShowClassicLabelSettings )
            {
                urls[NavigationUrlKey.ClassicLabelsPage] = "/admin/checkin/labels-classic";
                urls[NavigationUrlKey.ClassicLabelMergeFields] = "/admin/checkin/label-merge-fields";
            }

            return urls;
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent available sorting options.
        /// </summary>
        private class SortBy
        {
            public const string AlphabeticalValue = "alphabetical";
            public const string MostActivityValue = "most-activity";

            private static readonly ListItemBag _alphabetical = new ListItemBag { Text = "Alphabetical (Ascending)", Value = AlphabeticalValue };
            public static ListItemBag Alphabetical => _alphabetical;

            private static readonly ListItemBag _mostActivity = new ListItemBag { Text = "Most Activity (28 Days)", Value = MostActivityValue };
            public static ListItemBag MostActivity => _mostActivity;
        }

        #endregion Supporting Classes
    }
}

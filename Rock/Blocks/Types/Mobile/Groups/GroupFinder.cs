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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.ClientService.Core.Campus;
using Rock.ClientService.Core.Campus.Options;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using Regex = System.Text.RegularExpressions.Regex;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// Allows an individual to search for a group to join.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Finder" )]
    [Category( "Mobile > Groups" )]
    [Description( "Allows an individual to search for a group to join." )]
    [IconCssClass( "fa fa-search" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    /*
     * This can be changed to a multiple select once a field type
     * is designed to support multiple location types. The code has
     * been designed to handle multiple values separated by comma.
     * 
     * Daniel Hazelbaker 10/7/2021
     */
    [GroupLocationTypeField(
        "Location Types",
        Description = "The location types available to pick from when searching by location.",
        IsRequired = false,
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        Key = AttributeKey.LocationTypes,
        Order = 1 )]

    [BooleanField(
        "Hide Overcapacity Groups",
        Description = "Hides groups that have already reached their capacity limit.",
        IsRequired = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        DefaultBooleanValue = true,
        Key = AttributeKey.HideOvercapacityGroups,
        Order = 2 )]

    [BooleanField(
        "Show Results on Initial Page Load",
        Description = "Bypasses the filter and shows results immediately. Can also be set in query string with LoadResults=true.",
        IsRequired = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowResultsOnInitialPageLoad,
        Order = 3 )]

    [CodeEditorField(
        "Search Header",
        Description = "The XAML content to display above the search filters.",
        IsRequired = false,
        EditorMode = Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.SearchHeader,
        Order = 4 )]

    [BlockTemplateField(
        "Template",
        Description = "The Lava template to use to render the results.",
        IsRequired = true,
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUPS_GROUP_FINDER,
        DefaultValue = "CC117DBB-5C3C-4A32-8ABA-88A7493C7F70",
        Key = AttributeKey.Template,
        Order = 5 )]

    [IntegerField(
        "Max Results",
        Description = "The maximum number of results to show on the page.",
        IsRequired = true,
        DefaultIntegerValue = 25,
        Key = AttributeKey.MaxResults,
        Order = 6 )]

    [BooleanField(
        "Show Location Filter",
        Description = "Shows the location search filter and enables ordering results by distance.",
        IsRequired = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowLocationFilter,
        Order = 7 )]

    [BooleanField(
        "Show Campus Filter",
        Description = "Shows the campus search filter and enables filtering by campus.",
        IsRequired = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowCampusFilter,
        Order = 8 )]

    [DefinedValueField(
        "Campus Types",
        Description = "Specifies which campus types will be shown in the campus filter.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Key = AttributeKey.CampusTypes,
        Order = 9 )]

    [DefinedValueField(
        "Campus Statuses",
        Description = "Specifies which campus statuses will be shown in the campus filter.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Key = AttributeKey.CampusStatuses,
        Order = 10 )]

    [BooleanField(
        "Show Day of Week Filter",
        Description = "Shows the day of week filter and enables filtering to groups that meet on the selected day.",
        IsRequired = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowDayOfWeekFilter,
        Order = 11 )]

    [BooleanField(
        "Show Time Period Filter",
        Description = "Shows a filter that enables filtering based on morning, afternoon and evening.",
        IsRequired = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowTimePeriodFilter,
        Order = 12 )]

    [BooleanField(
        "Campus Context Enabled",
        Description = "Automatically sets the campus filter to the current campus context.",
        IsRequired = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        DefaultBooleanValue = false,
        Key = AttributeKey.CampusContextEnabled,
        Order = 13 )]

    [LinkedPage(
        "Detail Page",
        Description = "The page to link to when selecting a group.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 14 )]

    [EnumField(
        "Results Transition",
        Description = "The transition to use when going from filters to results and back.",
        IsRequired = true,
        EnumSourceType = typeof( GroupFinderTransition ),
        DefaultEnumValue = ( int ) GroupFinderTransition.Fade,
        Key = AttributeKey.ResultsTransition,
        Order = 15 )]

    // -- Custom Settings --

    [GroupTypesField(
        "Group Types",
        Description = "Specifies which group types are included in search results.",
        IsRequired = true,
        EnhancedSelection = true,
        Category = "customsetting",
        Key = AttributeKey.GroupTypes )]

    [TextField(
        "Group Types Location Type",
        Description = "The type of location each group type can use for distance calculations.",
        IsRequired = false,
        Category = "customsetting",
        Key = AttributeKey.GroupTypesLocationType )]

    [AttributeField( "Attribute Filters",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        IsRequired = false,
        AllowMultiple = true,
        Category = "customsetting",
        Key = AttributeKey.AttributeFilters )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_FINDER_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_FINDER )]
    public partial class GroupFinder : RockBlockType
    {
        #region Fields

        private static readonly Regex _newlineRegex = new Regex( @"[\r\n]+" );

        #endregion

        #region Block Attributes

        /// <summary>
        /// Attribute keys for the <see cref="GroupFinder"/> block.
        /// </summary>
        public static class AttributeKey
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public const string LocationTypes = "LocationTypes";

            public const string HideOvercapacityGroups = "HideOvercapacityGroups";

            public const string ShowResultsOnInitialPageLoad = "ShowResultsOnInitialPageLoad";

            public const string SearchHeader = "SearchHeader";

            public const string Template = "Template";

            public const string MaxResults = "MaxResults";

            public const string ShowLocationFilter = "ShowLocationFilter";

            public const string ShowCampusFilter = "ShowCampusFilter";

            public const string CampusTypes = "CampusTypes";

            public const string CampusStatuses = "CampusStatuses";

            public const string ShowDayOfWeekFilter = "ShowDayOfWeekFilter";

            public const string ShowTimePeriodFilter = "ShowTimePeriodFilter";

            public const string CampusContextEnabled = "CampusContextEnabled";

            public const string DetailPage = "DetailPage";

            public const string ResultsTransition = "ResultsTransition";

            public const string GroupTypes = "GroupTypes";

            public const string GroupTypesLocationType = "GroupTypesLocationType";

            public const string AttributeFilters = "AttributeFilters";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// Gets the group type guids.
        /// </summary>
        /// <value>
        /// The group type guids.
        /// </value>
        protected List<Guid> GroupTypeGuids => GetAttributeValue( AttributeKey.GroupTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the location type guids.
        /// </summary>
        /// <value>
        /// The location type guids.
        /// </value>
        protected List<Guid> LocationTypeGuids => GetAttributeValue( AttributeKey.LocationTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets a value indicating whether to hide groups that are over capacity already.
        /// </summary>
        /// <value>
        ///   <c>true</c> if groups already over capacity are hidden; otherwise, <c>false</c>.
        /// </value>
        protected bool HideOvercapacityGroups => GetAttributeValue( AttributeKey.HideOvercapacityGroups ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show the results on initial page load.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the results should be shown on initial page load; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowResultsOnInitialPageLoad => GetAttributeValue( AttributeKey.ShowResultsOnInitialPageLoad ).AsBoolean();

        /// <summary>
        /// Gets the search header content.
        /// </summary>
        /// <value>
        /// The search header content.
        /// </value>
        protected string SearchHeader => GetAttributeValue( AttributeKey.SearchHeader );

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.Template ) );

        /// <summary>
        /// Gets the maximum results to be returned.
        /// </summary>
        /// <value>
        /// The maximum results to be returned.
        /// </value>
        protected int MaxResults => GetAttributeValue( AttributeKey.MaxResults ).AsInteger();

        /// <summary>
        /// Gets a value indicating whether to show the location filter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the location filter should be shown; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowLocationFilter => GetAttributeValue( AttributeKey.ShowLocationFilter ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show the campus filter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if campus filter should be shown; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowCampusFilter => GetAttributeValue( AttributeKey.ShowCampusFilter ).AsBoolean();

        /// <summary>
        /// Gets the campus types to limit the campus picker to.
        /// </summary>
        /// <value>
        /// The campus types to limit the campus picker to.
        /// </value>
        protected List<Guid> CampusTypeGuids => GetAttributeValue( AttributeKey.CampusTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the campus statuses to limit the campus picker to.
        /// </summary>
        /// <value>
        /// The campus statuses to limit the campus picker to.
        /// </value>
        protected List<Guid> CampusStatusGuids => GetAttributeValue( AttributeKey.CampusStatuses ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets a value indicating whether to show the day of week filter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the day of week filter should be shown; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowDayOfWeekFilter => GetAttributeValue( AttributeKey.ShowDayOfWeekFilter ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show the time period filter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the period filter should be shown; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowTimePeriodFilter => GetAttributeValue( AttributeKey.ShowTimePeriodFilter ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to use campus context information.
        /// </summary>
        /// <value>
        ///   <c>true</c> if campus context information is used; otherwise, <c>false</c>.
        /// </value>
        protected bool CampusContextEnabled => GetAttributeValue( AttributeKey.CampusContextEnabled ).AsBoolean();

        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        protected Guid? DetailPageGuid => GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the results transition.
        /// </summary>
        /// <value>
        /// The results transition.
        /// </value>
        protected GroupFinderTransition ResultsTransition => GetAttributeValue( AttributeKey.ResultsTransition ).ConvertToEnum<GroupFinderTransition>( GroupFinderTransition.Fade );

        /// <summary>
        /// Gets the group types location type map.
        /// </summary>
        /// <value>
        /// The group types location type map.
        /// </value>
        protected Dictionary<int, int> GroupTypesLocationType => GetAttributeValue( AttributeKey.GroupTypesLocationType ).FromJsonOrNull<Dictionary<int, int>>() ?? new Dictionary<int, int>();

        /// <summary>
        /// Gets the group attribute unique identifiers enabled for this block.
        /// </summary>
        /// <value>
        /// The group attribute unique identifiers enabled for this block.
        /// </value>
        protected List<Guid> AttributeFiltersGuids => GetAttributeValue( AttributeKey.AttributeFilters ).SplitDelimitedValues().AsGuidList();

        #endregion

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            /// <summary>
            /// Will contain a true value if the results should be shown on
            /// initial page load.
            /// </summary>
            public const string LoadResults = "LoadResults";
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 3 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            using ( var rockContext = new RockContext() )
            {
                var attributes = AttributeFiltersGuids.Select( a => AttributeCache.Get( a ) )
                    .Where( a => a != null )
                    .Select( a => ToPublicEditableAttributeValue( a, a.DefaultValue ) )
                    .ToList();

                return new
                {
                    Campuses = GetValidCampuses( rockContext ),
                    SearchHeader,
                    ShowCampusFilter,
                    ShowDayOfWeekFilter,
                    ShowTimePeriodFilter,
                    ShowLocationFilter,
                    ResultsTransition,
                    Attributes = attributes
                };
            }
        }

        #endregion

        /// <summary>
        /// Converts to an attribute and value into a custom poco that will
        /// be transmitted to the shell.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value.</param>
        /// <returns>The editable attribute including the value.</returns>
        private PublicEditableAttributeValueViewModel ToPublicEditableAttributeValue( AttributeCache attribute, string value )
        {
            var attr = PublicAttributeHelper.GetPublicAttributeForEdit( attribute );

            return new PublicEditableAttributeValueViewModel
            {
                AttributeGuid = attr.AttributeGuid,
                Categories = attr.Categories,
                ConfigurationValues = attr.ConfigurationValues,
                Description = attr.Description,
                FieldTypeGuid = attr.FieldTypeGuid,
                IsRequired = attr.IsRequired,
                Key = attr.Key,
                Name = attr.Name,
                Order = attr.Order,
                Value = PublicAttributeHelper.GetPublicEditValue( attribute, value )
            };
        }

        #region Methods

        /// <summary>
        /// Determines if the results should be shown initially based on all
        /// configuration options.
        /// </summary>
        /// <returns><c>true</c> if the results screen should be shown initially.</returns>
        private bool ShouldShowInitialResults()
        {
            var showResults = RequestContext.GetPageParameter( PageParameterKey.LoadResults ).AsBooleanOrNull();

            return showResults ?? ShowResultsOnInitialPageLoad;
        }

        /// <summary>
        /// Gets the valid campuses that have been configured on the block settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A collection of list items.</returns>
        private List<ViewModels.Utility.ListItemBag> GetValidCampuses( RockContext rockContext )
        {
            var campusClientService = new CampusClientService( rockContext, RequestContext.CurrentPerson );

            // Bypass security because the admin has specified which campuses
            // they want to show up.
            campusClientService.EnableSecurity = false;

            return campusClientService.GetCampusesAsListItems( new CampusOptions
            {
                LimitCampusTypes = CampusTypeGuids,
                LimitCampusStatuses = CampusStatusGuids
            } );
        }

        /// <summary>
        /// Gets the valid group attributes defined for this block instance.
        /// </summary>
        /// <param name="rockContext">The rock database context.</param>
        /// <returns>A list of <see cref="AttributeCache"/> objects which can be used for filtering groups.</returns>
        private List<AttributeCache> GetValidGroupAttributes( RockContext rockContext )
        {
            return AttributeFiltersGuids
                .Select( g => AttributeCache.Get( g, rockContext ) )
                .Where( a => a != null )
                .ToList();
        }

        /// <summary>
        /// Gets the valid named locations in relation to the current person.
        /// </summary>
        /// <param name="rockContext">The rock database context.</param>
        /// <returns>A list of valid locations.</returns>
        private List<GroupFinderNamedLocation> GetValidNamedLocations( RockContext rockContext )
        {
            var locationTypeGuids = LocationTypeGuids;

            // If we don't have a person, then we have no named locations.
            if ( RequestContext.CurrentPerson == null )
            {
                return new List<GroupFinderNamedLocation>();
            }

            // Find all the locations in every family the person is a member of.
            var locations = RequestContext.CurrentPerson
                .GetFamilies( rockContext )
                .SelectMany( f => f.GroupLocations )
                .Where( l => locationTypeGuids.Contains( l.GroupLocationTypeValue.Guid )
                    && l.Location.GeoPoint != null )
                .Select( l => new
                {
                    l.Guid,
                    Type = l.GroupLocationTypeValue.Value,
                    l.Location
                } )
                .ToList();

            // Filter the list to only those that have a valid street address
            // and then build the list of named locations.
            return locations
                .Where( l => l.Location.GetFullStreetAddress().IsNotNullOrWhiteSpace() )
                .Select( l => new GroupFinderNamedLocation
                {
                    Guid = l.Guid,
                    Title = $"Your {l.Type} Address",
                    Description = _newlineRegex.Replace( l.Location.GetFullStreetAddress(), " " ),
                    Location = new GroupFinderLocation
                    {
                        Latitude = l.Location.Latitude.Value,
                        Longitude = l.Location.Longitude.Value
                    }
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the initial filter values to be used by the client.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The default filter information.</returns>
        private GroupFinderFilter GetInitialFilter( RockContext rockContext )
        {
            var validAttributes = GetValidGroupAttributes( rockContext );

            // Get all the page parameters and filter it down to just those that
            // match a valid group attribute and then build a list of those as
            // the initial attribute values.
            var attributeValues = RequestContext.GetPageParameters()
                .Select( p => new
                {
                    Attribute = validAttributes.FirstOrDefault( a => a.Key.Equals( p.Key, StringComparison.OrdinalIgnoreCase ) ),
                    Value = new List<string> { string.Empty, p.Value }
                } )
                .Where( a => a.Attribute != null )
                .ToDictionary( a => a.Attribute.Key, a => a.Value );

            
            return new GroupFinderFilter
            {
                CampusGuid = CampusContextEnabled ? RequestContext.GetContextEntity<Campus>()?.Guid : null,
                Attributes = attributeValues
            };
        }

        /// <summary>
        /// Gets the groups that match the filter options.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filter">The filter options.</param>
        /// <returns>A queryable of matching groups.</returns>
        private IQueryable<Group> GetGroups( RockContext rockContext, GroupFinderFilter filter )
        {
            var groupService = new GroupService( rockContext );
            var validAttributes = GetValidGroupAttributes( rockContext );

            if ( !GroupTypeGuids.Any() )
            {
                return groupService.Queryable().Where( g => false );
            }

            var groupTypeGuids = GroupTypeGuids;
            var daysOfWeek = filter.DayOfWeek.HasValue ? new List<DayOfWeek> { filter.DayOfWeek.Value } : null;
            var timePeriodsOfDay = filter.TimePeriodOfDay.HasValue ? new List<TimePeriodOfDay> { filter.TimePeriodOfDay.Value } : null;
            var campuses = filter.CampusGuid.HasValue ? new List<Guid> { filter.CampusGuid.Value } : null;
            var requiredSpotsAvailable = HideOvercapacityGroups ? ( int? ) 1 : null;
            var attributeFilters = filter.Attributes
                .Select( f => new
                {
                    Attribute = validAttributes.FirstOrDefault( a => a.Key == f.Key ),
                    f.Value
                } )
                .Where( a => a.Attribute != null && a.Value != null )
                .ToDictionary( a => a.Attribute, a => a.Value );

            // -- Everything below this line is common logic that can be moved to
            // the service layer.


            // Initial query only includes active and public groups.
            var groupQry = groupService.Queryable()
                .Include( g => g.GroupLocations.Select( l => l.Location ) )
                .Where( g => g.IsActive && g.IsPublic );

            // If any group types were specified then filter to only groups
            // that match one of the group types.
            if ( groupTypeGuids != null && groupTypeGuids.Any() )
            {
                groupQry = groupQry.Where( g => groupTypeGuids.Contains( g.GroupType.Guid ) );
            }

            // If any days of the week were specified then filter to only groups
            // that match one of those days of the week.
            if ( daysOfWeek != null && daysOfWeek.Any() )
            {
                groupQry = groupQry
                    .Where( g => g.Schedule.WeeklyDayOfWeek.HasValue
                        && daysOfWeek.Contains( g.Schedule.WeeklyDayOfWeek.Value ) );
            }

            // If any time periods were specified then filter to only groups
            // that match one of those time periods.
            if ( timePeriodsOfDay != null && timePeriodsOfDay.Any() )
            {
                groupQry = groupQry.WhereTimePeriodIsOneOf( timePeriodsOfDay, g => g.Schedule.WeeklyTimeOfDay.Value );
            }

            // If any campuses were specified then filter to only groups
            // that belong to one of those campuses.
            if ( campuses != null && campuses.Any() )
            {
                groupQry = groupQry.Where( g => campuses.Contains( g.Campus.Guid ) );
            }

            // If it has been requested to hide groups that are already over
            // capacity then do so.
            if ( requiredSpotsAvailable.HasValue )
            {
                // Filter by the overall group size.
                groupQry = groupQry.Where( g => !g.GroupCapacity.HasValue
                    || g.GroupType.GroupCapacityRule == GroupCapacityRule.None
                    || ( g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Count() + requiredSpotsAvailable.Value ) <= g.GroupCapacity );

                // Filter by the default role that new members would be placed into.
                groupQry = groupQry.Where( g => g.GroupType == null
                    || g.GroupType.GroupCapacityRule == GroupCapacityRule.None
                    || g.GroupType.DefaultGroupRole == null
                    || g.GroupType.DefaultGroupRole.MaxCount == null
                    || ( g.Members.Where( m => m.GroupRoleId == g.GroupType.DefaultGroupRoleId && m.GroupMemberStatus == GroupMemberStatus.Active ).Count() + requiredSpotsAvailable.Value ) <= g.GroupType.DefaultGroupRole.MaxCount );
            }

            // Filter query by any configured attribute filters
            if ( attributeFilters != null && attributeFilters.Any() )
            {
                var processedAttributes = new HashSet<string>();
                /*
                    07/01/2021 - MSB

                    This section of code creates an expression for each attribute in the search. The attributes from the same
                    Group Type get grouped and &&'d together. Then the grouped Expressions will get ||'d together so that results
                    will be returned across Group Types.

                    If we don't do this, when the Admin adds attributes from two different Group Types and then the user enters data
                    for both attributes they would get no results because Attribute A from Group Type A doesn't exists in Group Type B.
                    
                    Reason: Queries across Group Types
                */
                var filters = new Dictionary<string, Expression>();
                var parameterExpression = groupService.ParameterExpression;

                foreach ( var attributeFilter in attributeFilters )
                {
                    var attribute = attributeFilter.Key;
                    var values = attributeFilter.Value;
                    var queryKey = $"{attribute.EntityTypeQualifierColumn}_{attribute.EntityTypeQualifierValue}";

                    Expression leftExpression = null;
                    if ( filters.ContainsKey( queryKey ) )
                    {
                        leftExpression = filters[queryKey];
                    }

                    var expression = Rock.Utility.ExpressionHelper.BuildExpressionFromFieldType<Group>( values.ToList(), attribute, groupService, parameterExpression );
                    if ( expression != null )
                    {
                        if ( leftExpression == null )
                        {
                            filters[queryKey] = expression;
                        }
                        else
                        {
                            filters[queryKey] = Expression.And( leftExpression, expression );
                        }
                    }
                }

                // If we have a single filter group then just filter by the
                // expression. If we have more than one then OR each group
                // together and apply the resulting expression.
                if ( filters.Count == 1 )
                {
                    groupQry = groupQry.Where( parameterExpression, filters.FirstOrDefault().Value );
                }
                else if ( filters.Count > 1 )
                {
                    var keys = filters.Keys.ToList();
                    var expression = filters[keys[0]];

                    for ( var i = 1; i < filters.Count; i++ )
                    {
                        expression = Expression.Or( expression, filters[keys[i]] );
                    }

                    groupQry = groupQry.Where( parameterExpression, expression );
                }
            }

            return groupQry;
        }

        /// <summary>
        /// Get a dictionary of distances to each group. If a group has no
        /// locations then it will not be present in the dictionary. If a
        /// group has multiple locations then the closest location will be
        /// used.
        /// </summary>
        /// <param name="groups">The groups to be queried.</param>
        /// <param name="groupTypeLocation">The map of group types and the allowed location type to use.</param>
        /// <param name="latitude">The latitude of the reference point.</param>
        /// <param name="longitude">The longitude of the reference point.</param>
        /// <returns>
        /// A dictionary of group identifiers as the key and distance from the
        /// reference point as the values.
        /// </returns>
        private Dictionary<int, double> GetDistances( IEnumerable<Group> groups, Dictionary<int, int> groupTypeLocation, double latitude, double longitude )
        {
            var distances = new Dictionary<int, double>();

            foreach ( var group in groups )
            {
                var groupLocations = group.GroupLocations
                    .Where( gl => gl.Location.GeoPoint != null );

                var locationTypeId = groupTypeLocation.GetValueOrNull( group.GroupTypeId );

                if ( locationTypeId.HasValue )
                {
                    groupLocations = groupLocations.Where( gl => gl.GroupLocationTypeValueId == locationTypeId.Value );
                }

                foreach ( var groupLocation in groupLocations )
                {
                    var geoPoint = System.Data.Entity.Spatial.DbGeography.FromText( string.Format( "POINT({0} {1})", longitude, latitude ) );
                    double meters = groupLocation.Location.GeoPoint.Distance( geoPoint ) ?? 0.0D;
                    double miles = meters * Location.MilesPerMeter;

                    // If this group already has a distance calculated, see if this location is closer and if so, use it instead
                    if ( distances.ContainsKey( group.Id ) )
                    {
                        if ( miles < distances[group.Id] )
                        {
                            distances[group.Id] = miles;
                        }
                    }
                    else
                    {
                        distances.Add( group.Id, miles );
                    }
                }
            }

            return distances;
        }

        /// <summary>
        /// Gets all information needed and merges the data through the Lava
        /// template to get the final content to be displayed.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filter">The filter options.</param>
        /// <returns>The content to be displayed by the client.</returns>
        private string GetResultContent( RockContext rockContext, GroupFinderFilter filter )
        {
            var mergeFields = RequestContext.GetCommonMergeFields();
            var groups = GetGroups( rockContext, filter )
                .Include( g => g.GroupLocations.Select( gl => gl.Location ) )
                .ToList();

            // If they provided a location then attempt to filter and sort by
            // distance.
            if ( filter.Location?.Latitude != null && filter.Location?.Longitude != null )
            {
                var distances = GetDistances( groups, GroupTypesLocationType, filter.Location.Latitude.Value, filter.Location.Longitude.Value );

                // Only show groups with a known location, and sort those by distance.
                // Filtering is done to keep in parity with the Web version.
                groups = groups.Where( a => distances.ContainsKey( a.Id ) )
                    .OrderBy( a => distances[a.Id] )
                    .ThenBy( a => a.Name )
                    .ToList();

                mergeFields.AddOrReplace( "Distances", distances );
            }

            if ( MaxResults > 0 )
            {
                groups = groups.Take( MaxResults ).ToList();
            }

            mergeFields.AddOrReplace( "DetailPage", DetailPageGuid );
            mergeFields.AddOrReplace( "Groups", groups );

            return Template.ResolveMergeFields( mergeFields );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the initial data to use for the page at load time.
        /// </summary>
        /// <returns>The result of the action.</returns>
        [BlockAction]
        public BlockActionResult GetInitialData()
        {
            using ( var rockContext = new RockContext() )
            {
                var showResults = ShouldShowInitialResults();
                var validLocations = GetValidNamedLocations( rockContext );
                var filter = GetInitialFilter( rockContext );

                if ( validLocations.Any() )
                {
                    filter.Location = validLocations[0].Location;
                }

                return ActionOk( new
                {
                    ShowResults = showResults,
                    NamedLocations = validLocations,
                    Filter = filter,
                    ResultContent = showResults ? GetResultContent( rockContext, filter ) : null
                } );
            }
        }

        /// <summary>
        /// Gets the groups to be displayed for the given filtering options.
        /// </summary>
        /// <param name="filter">The filter options.</param>
        /// <returns>The content to display in the results view.</returns>
        [BlockAction]
        public BlockActionResult GetGroups( GroupFinderFilter filter )
        {
            using ( var rockContext = new RockContext() )
            {
                return ActionOk( new GetGroupsResult
                {
                    Content = GetResultContent( rockContext, filter )
                } );
            }
        }

        #endregion

        #region Block Action Classes

        /// <summary>
        /// Defines the filtering options to use with the Group Finder block.
        /// </summary>
        public class GroupFinderFilter
        {
            /// <summary>
            /// Gets or sets the campus unique identifier.
            /// </summary>
            /// <value>
            /// The campus unique identifier.
            /// </value>
            public Guid? CampusGuid { get; set; }

            /// <summary>
            /// Gets or sets the day of week.
            /// </summary>
            /// <value>
            /// The day of week.
            /// </value>
            public DayOfWeek? DayOfWeek { get; set; }

            /// <summary>
            /// Gets or sets the time period of day.
            /// </summary>
            /// <value>
            /// The time period of day.
            /// </value>
            public TimePeriodOfDay? TimePeriodOfDay { get; set; }

            /// <summary>
            /// Gets or sets the location to use for distance calculation.
            /// </summary>
            /// <value>
            /// The location to use for distance calculation.
            /// </value>
            public GroupFinderLocation Location { get; set; }

            /// <summary>
            /// Gets or sets the attribute values.
            /// </summary>
            /// <value>
            /// The attribute values.
            /// </value>
            public Dictionary<string, List<string>> Attributes { get; set; }
        }

        /// <summary>
        /// The result of the GetGroups action on the Group Finder block.
        /// </summary>
        public class GetGroupsResult
        {
            /// <summary>
            /// Gets or sets the content to be displayed.
            /// </summary>
            /// <value>
            /// The content to be displayed.
            /// </value>
            public string Content { get; set; }
        }

        /// <summary>
        /// The latitude and longitude of a location to use for distance
        /// calculations.
        /// </summary>
        public class GroupFinderLocation
        {
            /// <summary>
            /// Gets or sets the latitude.
            /// </summary>
            /// <value>
            /// The latitude.
            /// </value>
            public double? Latitude { get; set; }

            /// <summary>
            /// Gets or sets the longitude.
            /// </summary>
            /// <value>
            /// The longitude.
            /// </value>
            public double? Longitude { get; set; }
        }

        /// <summary>
        /// Defines a named location to display to the user so they can pick
        /// an address to use for distance calculations.
        /// </summary>
        public class GroupFinderNamedLocation
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the title of the named location.
            /// </summary>
            /// <value>
            /// The title of the named location.
            /// </value>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the descriptive text that describes this location.
            /// </summary>
            /// <value>
            /// The descriptive text that describes this location.
            /// </value>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the latitude and longitude of this location.
            /// </summary>
            /// <value>
            /// The latitude and longitude of this location.
            /// </value>
            public GroupFinderLocation Location { get; set; }
        }

        /// <summary>
        /// The type of transition to use in the Group Finder.
        /// </summary>
        public enum GroupFinderTransition
        {
            /// <summary>
            /// No transition is performed, hide and reveal will be instant.
            /// </summary>
            None = 0,

            /// <summary>
            /// The content will fade out to hide and fade in to reveal.
            /// </summary>
            Fade = 1,

            /// <summary>
            /// The content will flip with a flip view.
            /// </summary>
            Flip = 2,

            /// <summary>
            /// The content will slide up to hide and down to reveal.
            /// </summary>
            Slide = 3
        }

        #endregion

        /// <summary>
        /// Custom class to store the value along with the attribute. This is for
        /// backwards compatibility with Mobile Shell.
        /// </summary>
        private class PublicEditableAttributeValueViewModel : PublicAttributeBag
        {
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            public string Value { get; set; }
        }
    }
}

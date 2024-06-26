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

using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Reporting;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.SignUp.SignUpFinder;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Linq.Expressions;

namespace Rock.Blocks.Engagement.SignUp
{
    /// <summary>
    /// Block used for finding sign-up groups/projects.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="Rock.Blocks.IHasCustomActions" />

    [DisplayName( "Sign-Ups Finder" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Block used for finding sign-up groups/projects." )]
    [IconCssClass( "fa fa-clipboard-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    #region Layout / Initial Page Load

    [BooleanField( "Hide Overcapacity Projects",
        Key = AttributeKey.HideOvercapacityProjects,
        Description = "Determines if projects that are full should be shown.",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        IsRequired = false )]

    [BooleanField( "Load Results on Initial Page Load",
        Key = AttributeKey.LoadResultsOnInitialPageLoad,
        Description = "When enabled the project finder will load with all configured projects (no filters enabled).",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = true,
        IsRequired = false )]

    [CustomDropdownListField( "Display Project Filters As",
        Key = AttributeKey.DisplayProjectFiltersAs,
        Description = @"Determines if the ""Project Types"", ""Campus"", and ""Named Schedule"" project filters should be shown as checkboxes or multi-select dropdowns.",
        Category = AttributeCategory.CustomSetting,
        ListSource = "Checkboxes^Checkboxes,MultiSelectDropDown^Multi-Select Dropdown",
        DefaultValue = "Checkboxes",
        IsRequired = true )]

    [CustomDropdownListField( "Filter Columns",
        Key = AttributeKey.FilterColumns,
        Description = "The number of columns the filters should be displayed as.",
        Category = AttributeCategory.CustomSetting,
        ListSource = "1,2,3,4",
        DefaultValue = "1",
        IsRequired = true )]

    #endregion

    #region Project Filters

    [GroupTypesField( "Project Types",
        Key = AttributeKey.ProjectTypes,
        Description = "Select the sign-up project group types that should be considered for the search.",
        Category = AttributeCategory.CustomSetting,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP,
        IsRequired = true )]

    [TextField( "Project Type Filter Label",
        Key = AttributeKey.ProjectTypeFilterLabel,
        Description = "The label to use for the project type filter.",
        Category = AttributeCategory.CustomSetting,
        DefaultValue = "Project Type",
        IsRequired = true )]

    [AttributeField( "Display Attribute Filters",
        Key = AttributeKey.DisplayAttributeFilters,
        Description = "The group attributes that should be available for an individual to filter the results by.",
        Category = AttributeCategory.CustomSetting,
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        AllowMultiple = true,
        IsRequired = false )]

    #endregion

    #region Campus Filters

    [BooleanField( "Display Campus Filter",
        Key = AttributeKey.DisplayCampusFilter,
        Description = "Determines if the campus filter should be shown. If there is only one active campus to display then this filter will not be shown, even if enabled.",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        IsRequired = false )]

    [BooleanField( "Hide Campuses with no Sign-Up Opportunities",
        Key = AttributeKey.HideCampusesWithNoOpportunities,
        Description = @"Determines if campuses should be excluded from the filter list if they don't have any sign-up opportunities. This setting will be ignored if ""Display Campus Filter"" is disabled.",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        IsRequired = false )]

    [BooleanField( "Enable Campus Context",
        Key = AttributeKey.EnableCampusContext,
        Description = @"If enabled and the page has a campus context, its value will be used as a filter. If ""Display Campus Filter"" is disabled when this setting is enabled, the ""Campus Types"", ""Campus Statuses"" and ""Campuses"" settings will be ignored. However, if ""Display Campus Filter"" is enabled and the campus context is one of the allowed campuses, it will be pre-selected in the filter list.",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        IsRequired = false )]

    [DefinedValueField( "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = @"The types of campuses to include in the filter list if ""Display Campus Filter"" is enabled. If the filter is not displayed, these are the types the results should be automatically filtered by.",
        Category = AttributeCategory.CustomSetting,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        IsRequired = false )]

    [DefinedValueField( "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = @"The statuses of campuses to include in the filter list if ""Display Campus Filter"" is enabled. If the filter is not displayed, these are the statuses the results should be automatically filtered by.",
        Category = AttributeCategory.CustomSetting,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        IsRequired = false )]

    [CustomCheckboxListField( "Campuses",
        Key = AttributeKey.Campuses,
        Description = @"The specific campuses to include in the filter list if ""Display Campus Filter"" is enabled. If the filter is not displayed, these are the campuses the results should be automatically filtered by.",
        Category = AttributeCategory.CustomSetting,
        IsRequired = false )]

    #endregion

    #region Schedule Filters

    [BooleanField( "Display Named Schedule Filter",
        Key = AttributeKey.DisplayNamedScheduleFilter,
        Description = "When enabled a list of named schedules will be show as a filter.",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        IsRequired = false )]

    [TextField( "Named Schedule Filter Label",
        Key = AttributeKey.NamedScheduleFilterLabel,
        Description = "The label to use for the named schedule filter.",
        Category = AttributeCategory.CustomSetting,
        DefaultValue = "Schedules",
        IsRequired = true )]

    [CategoryField( "Root Schedule Category",
        Key = AttributeKey.RootScheduleCategory,
        Description = "When displaying the named schedule filter this will serve to filter which named schedules to show. Only direct descendants of this root schedule category will be displayed.",
        Category = AttributeCategory.CustomSetting,
        EntityTypeName = "Rock.Model.Schedule",
        IsRequired = false )]

    #endregion

    #region Location Filters

    [BooleanField( "Display Location Sort",
        Key = AttributeKey.DisplayLocationSort,
        Description = "Determines if the location sort field should be shown.",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = true,
        IsRequired = false )]

    [TextField( "Location Sort Label",
        Key = AttributeKey.LocationSortLabel,
        Description = "The label to use for the location sort filter.",
        Category = AttributeCategory.CustomSetting,
        DefaultValue = "Location (City, State or Zip Code)",
        IsRequired = true )]

    [BooleanField( "Display Location Range Filter",
        Key = AttributeKey.DisplayLocationRangeFilter,
        Description = "When enabled a filter will be shown to limit results to a specified number of miles from the location selected or their mailing address if logged in. If the Location Sort entry is not enabled to be shown and the individual is not logged in then this filter will not be shown, even if enabled, as we will not be able to honor the filter.",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = true,
        IsRequired = false )]

    #endregion

    #region Additional Filters

    [BooleanField( "Display Date Range",
        Key = AttributeKey.DisplayDateRange,
        Description = "When enabled, individuals would be able to filter the results by projects occurring inside the provided date range.",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = true,
        IsRequired = false )]

    [BooleanField( "Display Slots Available Filter",
        Key = AttributeKey.DisplaySlotsAvailableFilter,
        Description = @"When enabled allows the individual to find projects with ""at least"" or ""no more than"" the provided spots available.",
        Category = AttributeCategory.CustomSetting,
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = true,
        IsRequired = false )]

    #endregion

    #region Lava

    [CodeEditorField( "Results Lava Template",
        Key = AttributeKey.ResultsLavaTemplate,
        Description = "The Lava template to use to show the results of the search. Merge fields include: Projects. <span class='tip tip-lava'></span>",
        Category = AttributeCategory.CustomSetting,
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        DefaultValue = AttributeDefault.ResultsLavaTemplate,
        IsRequired = true )]

    [CodeEditorField( "Results Header Lava Template",
        Key = AttributeKey.ResultsHeaderLavaTemplate,
        Description = "The Lava Template to use to show the results header.",
        Category = AttributeCategory.CustomSetting,
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        DefaultValue = AttributeDefault.ResultsHeaderLavaTemplate,
        IsRequired = false )]

    #endregion

    #region Linked Pages

    [LinkedPage( "Project Detail Page",
        Key = AttributeKey.ProjectDetailPage,
        Description = "The page reference to pass to the Lava template for the details of the project.",
        Category = AttributeCategory.CustomSetting,
        IsRequired = true )]

    [LinkedPage( "Registration Page",
        Key = AttributeKey.RegistrationPage,
        Description = "The page reference to pass to the Lava template for the registration page.",
        Category = AttributeCategory.CustomSetting,
        IsRequired = true )]

    #endregion

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "BF09747C-786D-4979-BADF-2D0157F4CB21" )]
    [Rock.SystemGuid.BlockTypeGuid( "74A20402-00DF-4A87-98D1-B5A8920F1D32" )]
    public class SignUpFinder : RockBlockType, IHasCustomActions
    {
        #region Keys & Constants

        private static class AttributeKey
        {
            // Layout / Initial Page Load.
            public const string HideOvercapacityProjects = "HideOvercapacityProjects";
            public const string LoadResultsOnInitialPageLoad = "LoadResultsOnInitialPageLoad";
            public const string DisplayProjectFiltersAs = "DisplayProjectFiltersAs";
            public const string FilterColumns = "FilterColumns";

            // Project Filters.
            public const string ProjectTypes = "ProjectTypes";
            public const string ProjectTypeFilterLabel = "ProjectTypeFilterLabel";

            // Campus Filters.
            public const string DisplayCampusFilter = "DisplayCampusFilter";
            public const string HideCampusesWithNoOpportunities = "HideCampusesWithNoOpportunities";
            public const string EnableCampusContext = "EnableCampusContext";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string Campuses = "Campuses";

            // Schedule Filters.
            public const string DisplayNamedScheduleFilter = "DisplayNamedScheduleFilter";
            public const string NamedScheduleFilterLabel = "NamedScheduleFilterLabel";
            public const string RootScheduleCategory = "RootScheduleCategory";

            // Location Filters.
            public const string DisplayLocationSort = "DisplayLocationSort";
            public const string LocationSortLabel = "LocationSortLabel";
            public const string DisplayLocationRangeFilter = "DisplayLocationRangeFilter";

            // Additional Filters.
            public const string DisplayDateRange = "DisplayDateRange";
            public const string DisplaySlotsAvailableFilter = "DisplaySlotsAvailableFilter";
            public const string DisplayAttributeFilters = "DisplayAttributeFilters";

            // Lava.
            public const string ResultsLavaTemplate = "ResultsLavaTemplate";
            public const string ResultsHeaderLavaTemplate = "ResultsHeaderLavaTemplate";

            // Linked Pages.
            public const string ProjectDetailPage = "ProjectDetailPage";
            public const string RegistrationPage = "RegistrationPage";
        }

        private static class AttributeCategory
        {
            public const string CustomSetting = "CustomSetting";
        }

        private static class AttributeDefault
        {
            public const string ResultsLavaTemplate = @"{% assign projectCount = Projects | Size %}
{% if projectCount > 0 %}
    <div class=""row d-flex flex-wrap"">
        {% for project in Projects %}
            <div class=""col-xs-12 col-sm-6 col-md-4 mb-4"">
                <div class=""card h-100"">
                    <div class=""card-body"">
                        <h3 class=""card-title mt-0"">{{ project.Name }}</h3>
                        {% if project.ScheduleName and project.ScheduleName != empty %}
                            <p class=""card-subtitle text-muted mb-3"">{{ project.ScheduleName }}</p>
                        {% endif %}
                        <p class=""mb-2"">{{ project.FriendlySchedule }}</p>
                        <div class=""d-flex justify-content-between mb-3"">
                            {% if project.AvailableSpots != null %}
                                <span class=""badge badge-info"">Available Spots: {{ project.AvailableSpots }}</span>
                            {% else %}
                                &nbsp;
                            {% endif %}
                            {% if project.DistanceInMiles != null %}
                                <span class=""badge"">{{ project.DistanceInMiles | Format:'0.0' }} miles<span>
                            {% else %}
                                &nbsp;
                            {% endif %}
                        </div>
                        {% if project.MapCenter and project.MapCenter != empty %}
                            <div class=""mb-3"">
                                {[ googlestaticmap center:'{{ project.MapCenter }}' zoom:'15' ]}
                                {[ endgooglestaticmap ]}
                            </div>
                        {% endif %}
                        {% if project.Description and project.Description != empty %}
                            <p class=""card-text"">
                                {{ project.Description }}
                            </p>
                        {% endif %}
                    </div>
                    <div class=""card-footer bg-white border-0"">
                        {% if project.ShowRegisterButton %}
                            <a href=""{{ project.RegisterPageUrl }}"" class=""btn btn-primary btn-xs"">Register</a>
                        {% endif %}
                        <a href=""{{ project.ProjectDetailPageUrl }}"" class=""btn btn-link btn-xs"">Details</a>
                    </div>
                </div>
            </div>
        {% endfor %}
    </div>
{% else %}
    <div>
        No projects found.
    </div>
{% endif %}";

            public const string ResultsHeaderLavaTemplate = @"<h3>Results</h3>
<p>Below is a listing of the projects that match your search results.</p>
<hr>";
        }

        private static class FilterDisplayType
        {
            public const string Checkboxes = "Checkboxes";
            public const string MultiSelectDropDown = "MultiSelectDropDown";
        }

        private static class SlotsAvailableComparisonType
        {
            public const string AtLeast = "AtLeast";
            public const string NoMoreThan = "NoMoreThan";
        }

        private static class PageParameterKey
        {
            public const string ProjectId = "ProjectId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
        }

        #endregion

        #region Fields

        private List<ListItemBag> _projectTypeFilterItems;

        private CampusSettings _campusSettings;

        #endregion

        #region Properties

        public bool IsAuthenticated
        {
            get
            {
                return this.RequestContext.CurrentUser?.IsAuthenticated == true;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new SignUpFinderInitializationBox();

                SetBoxInitialState( box, rockContext );

                return box;
            }
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialState( SignUpFinderInitializationBox box, RockContext rockContext )
        {
            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes( rockContext );

            box.LoadResultsOnInitialPageLoad = GetAttributeValue( AttributeKey.LoadResultsOnInitialPageLoad ).AsBoolean();
            box.DisplayProjectFiltersAs = GetAttributeValue( AttributeKey.DisplayProjectFiltersAs );
            box.FilterColumns = GetAttributeValue( AttributeKey.FilterColumns ).ToIntSafe();
            box.ProjectTypes = GetShouldDisplayProjectTypeFilter() ? GetProjectTypeFilterItems() : new List<ListItemBag>();
            box.ProjectTypeFilterLabel = GetAttributeValue( AttributeKey.ProjectTypeFilterLabel );
            box.PageCampusContext = GetPageCampusContext().ToListItemBag();
            box.NamedSchedules = GetNamedScheduleFilterItems( rockContext );
            box.NamedScheduleFilterLabel = GetAttributeValue( AttributeKey.NamedScheduleFilterLabel );
            box.AttributesByProjectType = GetAttributeFilterItems( rockContext );
            box.DisplayDateRange = GetAttributeValue( AttributeKey.DisplayDateRange ).AsBoolean();
            box.DisplayLocationSort = GetAttributeValue( AttributeKey.DisplayLocationSort ).AsBoolean();
            box.LocationSortLabel = GetAttributeValue( AttributeKey.LocationSortLabel );
            box.DisplayLocationRangeFilter = GetShouldDisplayLocationRangeFilter();
            box.DisplaySlotsAvailableFilter = GetAttributeValue( AttributeKey.DisplaySlotsAvailableFilter ).AsBoolean();

            var campusSettings = GetCampusSettings();
            if ( !campusSettings.DisplayCampusFilter || campusSettings.AllowedCampuses.Count < 2 )
            {
                // We'll only send campuses to the UI if there are at least 2 for the individual to choose from.
                return;
            }

            box.Campuses = campusSettings.AllowedCampuses;

            if ( !campusSettings.HideCampusesWithNoOpportunities )
            {
                return;
            }

            // Perform a preliminary search using allowed campuses to hide those that don't have any opportunities.
            var selectedFilters = new SignUpFinderSelectedFiltersBag
            {
                Campuses = campusSettings.AllowedCampuses.Select( c => c.Value ).ToList()
            };

            var opportunities = GetOpportunities( rockContext, selectedFilters );
            var campusesWithOpportunities = campusSettings.AllowedCampuses.Where( c =>
                opportunities.Any( o => o.Project.Campus?.Guid.ToString() == c.Value )
            ).ToList();

            // Again, only send campuses to the UI if there are at least 2 for the individual to choose from.
            box.Campuses = campusesWithOpportunities.Count > 1 ? campusesWithOpportunities : null;
        }

        /// <summary>
        /// Gets whether the project type filter should be displayed.
        /// </summary>
        /// <returns>Whether the project type filter should be displayed.</returns>
        private bool GetShouldDisplayProjectTypeFilter()
        {
            return GetProjectTypeFilterItems().Count > 1;
        }

        /// <summary>
        /// Gets the sign-up project group types that should be presented as filter items for the search.
        /// </summary>
        /// <returns>The sign-up project group types that should be presented as filter items for the search.</returns>
        private List<ListItemBag> GetProjectTypeFilterItems()
        {
            if ( _projectTypeFilterItems == null )
            {
                _projectTypeFilterItems = GetAttributeValue( AttributeKey.ProjectTypes ).GroupTypeGuidsToListItemBagList();
            }

            return _projectTypeFilterItems;
        }

        /// <summary>
        /// Gets the <see cref="IEntity.Id"/>s for the supplied guid strings, using the specified function.
        /// </summary>
        /// <param name="entityGuidStrings">The <see cref="IEntity"/> guid string collection.</param>
        /// <param name="getEntityIdFunc">The function to use to get the <see cref="IEntity"/> IDs using the guid strings.</param>
        /// <param name="allowedListItems">
        /// The optional list of allowed items that will be used to refine the list of <paramref name="entityGuidStrings"/>.
        /// <para>
        /// This should be used to ensure that requests originating from the client don't pass more guid strings than they're authorized to pass.
        /// </para>
        /// </param>
        /// <returns>The <see cref="IEntity"/> IDs.</returns>
        private List<int> GetIds( IEnumerable<string> entityGuidStrings, Func<Guid, int?> getEntityIdFunc, List<ListItemBag> allowedListItems = null )
        {
            if ( entityGuidStrings?.Any() != true || getEntityIdFunc == null )
            {
                return new List<int>();
            }

            if ( allowedListItems?.Any() == true )
            {
                entityGuidStrings = entityGuidStrings.Where( a => allowedListItems.Any( i => i.Value == a ) );
            }

            return entityGuidStrings
                .AsGuidList()
                .Select( g => getEntityIdFunc( g ) )
                .AsIntegerList();
        }

        /// <summary>
        /// Gets the page campus context entity, if enabled, defined and allowed.
        /// </summary>
        /// <returns>The page campus context entity or <see langword="null"/> if not enabled, defined or allowed.</returns>
        private Campus GetPageCampusContext()
        {
            var campusSettings = GetCampusSettings();
            if ( !campusSettings.EnableCampusContext )
            {
                return null;
            }

            var pageContextCampus = this.RequestContext.GetContextEntity<Campus>();
            if ( pageContextCampus == null )
            {
                return null;
            }

            // If the campus filter is displayed, the campus context must be one of the allowed campuses
            // as dictated by the other campus block settings in order to use it for filtering.
            if ( campusSettings.DisplayCampusFilter && !campusSettings.AllowedCampuses.Any( c => c.Value == pageContextCampus.Guid.ToString() ) )
            {
                return null;
            }

            return pageContextCampus;
        }

        /// <summary>
        /// Gets the named schedules that should be presented as filter items for the search.
        /// </summary>
        /// <returns>The named schedules that should be presented as filter items for the search.</returns>
        private List<ListItemBag> GetNamedScheduleFilterItems( RockContext rockContext )
        {
            if ( !GetAttributeValue( AttributeKey.DisplayNamedScheduleFilter ).AsBoolean() )
            {
                return new List<ListItemBag>();
            }

            // ScheduleService is used instead of the NamedScheduleCache because we want ToFriendlyScheduleText to return
            // the condensed values instead of HTML formatted values when the Schedule has multiple dates.
            var query = new ScheduleService( rockContext ).Queryable();

            var rootScheduleCategoryGuid = GetAttributeValue( AttributeKey.RootScheduleCategory ).AsGuidOrNull();
            if ( rootScheduleCategoryGuid.HasValue )
            {
                var rootScheduleCategoryId = CategoryCache.GetId( rootScheduleCategoryGuid.Value );
                if ( rootScheduleCategoryId.HasValue )
                {
                    query = query.Where( a => a.CategoryId.HasValue && a.CategoryId.Value == rootScheduleCategoryId.Value && a.IsActive );
                }
            }

            return ToListItemBagList( query.ToList() );
        }

        /// <summary>
        /// Converts the schedules to a List of ListItemBags. Used instead of the ToListItemBagList() extension method because
        /// the extension method uses the FriendlyScheduleTex value of the Schedule which does not return the condensed value
        /// by default and as a result contains HTML string if the schedule has multiple dates.
        /// </summary>
        /// <param name="namedSchedules">The named schedules.</param>
        /// <returns></returns>
        private List<ListItemBag> ToListItemBagList( List<Schedule> namedSchedules )
        {
            var items = namedSchedules.Select( a => new ListItemBag()
            {
                Text = a.ToFriendlyScheduleText( true ),
                Value = a.Guid.ToString()
            } );

            return items.ToList();
        }

        /// <summary>
        /// Gets the display attribute filter guids that should be used to build attribute filter items for the search.
        /// </summary>
        /// <returns>The display attribute filter guids that should be used to build attribute filter items for the search.</returns>
        private List<Guid> GetDisplayAttributeFilterGuids()
        {
            return GetAttributeValue( AttributeKey.DisplayAttributeFilters )
                .SplitDelimitedValues( "," )
                .Select( a => a.AsGuidOrNull() )
                .Where( a => a.HasValue )
                .Select( a => a.Value )
                .ToList();
        }

        /// <summary>
        /// Gets the attributes that should be presented as filter items for the search, grouped by project type [guid].
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectedProjectTypeGuidStrings">The optional, selected project type guid strings.</param>
        /// <returns>The attributes that should be presented as filter items for the search, grouped by project type [guid].</returns>
        private Dictionary<string, Dictionary<string, PublicAttributeBag>> GetAttributeFilterItems( RockContext rockContext, IEnumerable<string> selectedProjectTypeGuidStrings = null )
        {
            // Because [Attribute].[Key]s aren't guaranteed unique across entities, we need to maintain a separate collection
            // of attribute filters per project [group] type.
            // Also, because attributes can be inherited by child group types, we'll add a given attribute (by it's guid) only
            // the first time we come across it.
            var attributeFilterItemsByGroupTypeId = new Dictionary<string, Dictionary<string, PublicAttributeBag>>();
            var attributeGuidsAlreadyAdded = new List<Guid>();

            var displayAttributeFilterGuids = GetDisplayAttributeFilterGuids();
            if ( !displayAttributeFilterGuids.Any() )
            {
                return attributeFilterItemsByGroupTypeId;
            }

            bool shouldAddAttribute( AttributeCache attributeCache )
            {
                if ( !displayAttributeFilterGuids.Contains( attributeCache.Guid ) )
                {
                    return false;
                }

                if ( attributeGuidsAlreadyAdded.Contains( attributeCache.Guid ) )
                {
                    return false;
                }

                attributeGuidsAlreadyAdded.Add( attributeCache.Guid );

                return true;
            }

            foreach ( var projectType in GetProjectTypeFilterItems() )
            {
                var projectTypeGuidString = projectType.Value;

                if ( selectedProjectTypeGuidStrings?.Any() == true
                    && !selectedProjectTypeGuidStrings.Any( a => a == projectTypeGuidString ) )
                {
                    // This project type was not selected by the individual performing the search; don't include its attributes.
                    continue;
                }

                var projectTypeGuid = projectType.Value.AsGuidOrNull();
                if ( !projectTypeGuid.HasValue )
                {
                    continue;
                }

                var projectTypeId = GroupTypeCache.GetId( projectTypeGuid.Value );
                if ( !projectTypeId.HasValue )
                {
                    continue;
                }

                var group = new Rock.Model.Group { GroupTypeId = projectTypeId.Value };
                group.LoadAttributes( rockContext );

                // Note that we're not enforcing security here, as the public-facing individual performing the search would most likely be restricted.
                var attributeFilterItems = group.GetPublicAttributesForEdit( this.RequestContext.CurrentPerson, enforceSecurity: false, attributeFilter: shouldAddAttribute );

                if ( attributeFilterItems.Any() )
                {
                    attributeFilterItemsByGroupTypeId.Add( projectTypeGuidString, attributeFilterItems );
                }
            }

            return attributeFilterItemsByGroupTypeId;
        }

        /// <summary>
        /// Gets whether the location range filter should be displayed.
        /// </summary>
        /// <returns>Whether the location range filter should be displayed.</returns>
        private bool GetShouldDisplayLocationRangeFilter()
        {
            // When enabled a filter will be show to limit results to a specified number of miles
            // from the location selected or their mailing address if logged in. If the Location Sort
            // entry is not enabled to be shown and the individual is not logged in then this filter
            // will not be shown, even if enabled, as we will not be able to honor the filter.
            if ( !GetAttributeValue( AttributeKey.DisplayLocationRangeFilter ).AsBoolean() )
            {
                return false;
            }

            var isLocationSortEnabled = GetAttributeValue( AttributeKey.DisplayLocationSort ).AsBoolean();

            return isLocationSortEnabled || this.IsAuthenticated;
        }

        /// <summary>
        /// Gets the campus settings to be used within the custom settings modal and public-facing search filters.
        /// </summary>
        /// <returns>The campus settings.</returns>
        private CampusSettings GetCampusSettings()
        {
            if ( _campusSettings == null )
            {
                // The available campuses are based on the currently-selected campus types and statuses, so get these first.
                var selectedCampusTypes = GetAttributeValue( AttributeKey.CampusTypes ).DefinedValueGuidsToListItemBagList();
                var selectedCampusStatuses = GetAttributeValue( AttributeKey.CampusStatuses ).DefinedValueGuidsToListItemBagList();

                var availableCampuses = GetCustomSettingsAvailableCampuses( selectedCampusTypes.Select( t => t.Value ), selectedCampusStatuses.Select( s => s.Value ) );

                // Ensure we don't try to filter against a campus that was previously saved, but no longer available.
                var selectedCampuses = GetAttributeValue( AttributeKey.Campuses ).ToListItemBagListUsingAvailableOptions( availableCampuses );

                // Double-check whether this Rock instance currently has multiple active Campuses.
                var hasMultipleCampuses = !CampusCache.SingleCampusId.HasValue;

                _campusSettings = new CampusSettings
                {
                    DisplayCampusFilter = hasMultipleCampuses && GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean(),
                    HideCampusesWithNoOpportunities = GetAttributeValue( AttributeKey.HideCampusesWithNoOpportunities ).AsBoolean(),
                    EnableCampusContext = GetAttributeValue( AttributeKey.EnableCampusContext ).AsBoolean(),
                    SelectedCampusTypes = selectedCampusTypes,
                    SelectedCampusStatuses = selectedCampusStatuses,
                    AvailableCampuses = availableCampuses,
                    SelectedCampuses = selectedCampuses
                };
            }

            return _campusSettings;
        }

        /// <summary>
        /// Gets the available sign-up project opportunities, based on the selected filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectedFilters">The selected filters.</param>
        /// <returns>A <see cref="Opportunity"/> list representing the available sign-up project opportunities.</returns>
        private List<Opportunity> GetOpportunities( RockContext rockContext, SignUpFinderSelectedFiltersBag selectedFilters )
        {
            if ( selectedFilters == null )
            {
                return new List<Opportunity>();
            }

            var signUpGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() ).ToIntSafe();

            // Get the active opportunities (GroupLocationSchedules).
            var qryGroupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl =>
                    gl.Group.IsActive
                    && ( gl.Group.GroupTypeId == signUpGroupTypeId || gl.Group.GroupType.InheritedGroupTypeId == signUpGroupTypeId )
                )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( glsc => glsc.ScheduleId == s.Id )
                } );

            // Filter by group types.
            var selectedProjectTypeGuidStrings = selectedFilters.ProjectTypes;
            var actualProjectTypeGuidStrings = GetProjectTypeFilterItems()
                .Where( p => selectedProjectTypeGuidStrings?.Any() != true || selectedProjectTypeGuidStrings.Contains( p.Value ) )
                .Select( p => p.Value );

            var groupTypeIds = GetIds( actualProjectTypeGuidStrings, GroupTypeCache.GetId );
            if ( groupTypeIds.Any() )
            {
                qryGroupLocationSchedules = qryGroupLocationSchedules
                    .Where( gls => groupTypeIds.Contains( gls.Group.GroupTypeId ) );
            }

            // Filter by campuses.
            var campusIds = new List<int>();
            var campusSettings = GetCampusSettings();

            if ( !campusSettings.DisplayCampusFilter )
            {
                if ( campusSettings.EnableCampusContext )
                {
                    // Campus context filtering overrules any other block settings campus filters in this
                    // case, even if there isn't actually a campus entity on the current page context.
                    var pageCampusContext = GetPageCampusContext();
                    if ( pageCampusContext != null )
                    {
                        campusIds.Add( pageCampusContext.Id );
                    }
                }
                else if ( campusSettings.AreCampusesFilteredByBlockSettings )
                {
                    campusIds = GetIds( campusSettings.AllowedCampuses.Select( c => c.Value ), CampusCache.GetId, campusSettings.AllowedCampuses );
                }
            }
            else
            {
                if ( selectedFilters.Campuses?.Any() == true )
                {
                    campusIds = GetIds( selectedFilters.Campuses, CampusCache.GetId, campusSettings.AllowedCampuses );
                }
                else if ( campusSettings.AreCampusesFilteredByBlockSettings )
                {
                    campusIds = GetIds( campusSettings.AllowedCampuses.Select( c => c.Value ), CampusCache.GetId, campusSettings.AllowedCampuses );
                }
            }

            if ( campusIds.Any() )
            {
                qryGroupLocationSchedules = qryGroupLocationSchedules
                    .Where( gls => gls.Group.CampusId.HasValue && campusIds.Contains( gls.Group.CampusId.Value ) );
            }
            else if ( campusSettings.AreCampusesFilteredByBlockSettings )
            {
                // If - for some reason - we don't have any campus IDs even though a block admin applied
                // behind-the-scenes campus filters, return no results.
                return new List<Opportunity>();
            }

            // Filter by named schedules.
            var namedScheduleIds = GetIds( selectedFilters.NamedSchedules, NamedScheduleCache.GetId, GetNamedScheduleFilterItems( rockContext ) );
            if ( namedScheduleIds.Any() )
            {
                qryGroupLocationSchedules = qryGroupLocationSchedules
                    .Where( gls => namedScheduleIds.Contains( gls.Schedule.Id ) );
            }

            // Filter by attributes.
            if ( selectedFilters.AttributeFiltersByProjectType?.Any( kvp => kvp.Value.Any() ) == true )
            {
                /*
                 * 2/1/2023 - JPH
                 * 
                 * Apply the selected attribute filters to the query, per project type.
                 * 
                 * Note that this filtering approach can lead to some unexpected results for the individuals administering and performing these searches.
                 * 
                 * --------
                 * Example:
                 * --------
                 *  1) Project Type A has a "Transportation is Provided" boolean attribute;
                 *  2) Project Type B doesn't have this attribute, and it has opportunities for which transportation is NOT provided;
                 *  3) Alisha Marble enables both of these project types for this block instance AND allows filtering against Project Type A's "Transportation is Provided" attribute;
                 *  4) A public-facing individual comes along and performs a search for projects where transportation is provided, but they unknowingly receive
                 *     a list of results that include projects from Project Type A (where transportation is provided) AND Project Type B (where transportation
                 *     is NOT provided). They might accidentally sign up for a project from Project Type B thinking that transportation will be provided for them.
                 * 
                 * This is not ideal, but not a scenario we're solving for in V1 of this feature; we might circle back in the future to improve this experience.
                 * 
                 * The challenge is that we cannot simply rule out the projects from Project Type B based on the presence of Project Type A's "Transportation is Provided"
                 * attribute; we can really only apply attribute filtering based on the project type to which a given attribute actually belongs, otherwise, we'd potentially
                 * never return any results to the individual in the above scenario (if we tried to apply attribute filters to project types to which they don't belong).
                 */
                var allowedProjectTypes = GetProjectTypeFilterItems();
                var allowedAttributeGuids = GetDisplayAttributeFilterGuids();

                var groupService = new GroupService( rockContext );
                var parameterExpression = groupService.ParameterExpression;
                var projectTypeExpressions = new List<Expression>();

                foreach ( var attrFiltersByProjectType in selectedFilters.AttributeFiltersByProjectType )
                {
                    var projectTypeGuidString = attrFiltersByProjectType.Key;
                    var attrFiltersDictionary = attrFiltersByProjectType.Value;

                    // Ensure this is an allowed project type.
                    var projectTypeIds = GetIds( new List<string> { projectTypeGuidString }, GroupTypeCache.GetId, allowedProjectTypes );
                    if ( projectTypeIds.Count != 1 )
                    {
                        // We didn't find a [GroupType].[Id] for this guid string; it must not be valid or allowed.
                        continue;
                    }

                    var projectTypeId = projectTypeIds.First();

                    Expression projectTypeExpression = null;

                    foreach ( var attrFilter in attrFiltersDictionary )
                    {
                        var attributeGuid = attrFilter.Key.AsGuidOrNull();
                        PublicComparisonValueBag filter = attrFilter.Value;

                        if ( !attributeGuid.HasValue || !allowedAttributeGuids.Contains( attributeGuid.Value ) )
                        {
                            // This attribute either isn't value or allowed.
                            continue;
                        }

                        var attributeCache = AttributeCache.Get( attributeGuid.Value );
                        var entityField = EntityHelper.GetEntityFieldForAttribute( attributeCache, false );

                        var values = new List<string>
                        {
                            filter.ComparisonType.ToString(),
                            PublicAttributeHelper.GetPrivateValue( attributeCache, filter.Value )
                        };

                        // Get the expression for this attribute filter.
                        var filterExpression = ExpressionHelper.GetAttributeExpression( groupService, parameterExpression, entityField, values );

                        // Combine it with expressions for any other selected filters tied to this project type.
                        projectTypeExpression = projectTypeExpression == null
                            ? filterExpression
                            : Expression.And( projectTypeExpression, filterExpression );
                    }

                    if ( projectTypeExpression != null )
                    {
                        projectTypeExpressions.Add( projectTypeExpression );
                    }
                }

                if ( projectTypeExpressions.Any() )
                {
                    var groupQry = groupService
                        .Queryable()
                        .AsNoTracking();

                    if ( projectTypeExpressions.Count == 1 )
                    {
                        groupQry = groupQry.Where( parameterExpression, projectTypeExpressions.First() );
                    }
                    else
                    {
                        var expression = projectTypeExpressions.First();

                        for ( int i = 1; i < projectTypeExpressions.Count; i++ )
                        {
                            expression = Expression.Or( expression, projectTypeExpressions[i] );
                        }

                        groupQry = groupQry.Where( parameterExpression, expression );
                    }

                    qryGroupLocationSchedules = qryGroupLocationSchedules
                        .Where( gls => groupQry.Any( g => gls.Group.Id == g.Id ) );
                }
            }

            // Filter by date range.
            DateTime fromDateTime = RockDateTime.Now;
            DateTime? toDateTime = null;

            if ( ( selectedFilters.StartDate.HasValue || selectedFilters.EndDate.HasValue )
                && GetAttributeValue( AttributeKey.DisplayDateRange ).AsBoolean() )
            {
                // This block shouldn't display past opportunities, since its goal is to get individuals to sign up.
                if ( selectedFilters.StartDate.HasValue && selectedFilters.StartDate.Value > fromDateTime )
                {
                    fromDateTime = selectedFilters.StartDate.Value.DateTime;
                }

                if ( selectedFilters.EndDate.HasValue )
                {
                    // Set this to the end of the selected day to perform a search fully-inclusive of the day the individual
                    // selected. Note also that we cannot apply this filter during the query phase; we need to wait until we
                    // materialize Schedule objects so we can compare this value to Schedule.NextStartDateTime, which is a
                    // runtime-calculated value. If we instead applied this filter to the Schedule.EffectiveEndDate, we could
                    // accidentally rule out opportunities for which the individual might otherwise be interested in signing up.
                    // We'll apply this filter value below.
                    toDateTime = selectedFilters.EndDate.Value.DateTime.EndOfDay();
                }
            }

            // Get just the date portion of the "from" date so we can compare it against the stored Schedules' EffectiveEndDates, which hold
            // only a date value (without the time component). Return any Schedules whose EffectiveEndDate:
            //  1) is not defined (this should never happen, but get them just in case), OR
            //  2) is greater than or equal to the "from" date being filtered against.
            // 
            // We'll do this to rule out any Schedules that have already ended, therefore making the initial results record set smaller,
            // since we still have to do additional Schedule-based filtering below: once we materialize the Schedule objects, we'll use their
            // runtime-calculated "Start[Date]Time" properties and methods to ensure we're only showing Schedules that actually qualify to
            // be shown, based on the DateTime filter criteria provided to this method (either RockDateTime.Now OR the "from" date selected
            // by the individual performing the search).
            DateTime fromDate = fromDateTime.Date;
            qryGroupLocationSchedules = qryGroupLocationSchedules
                .Where( gls => !gls.Schedule.EffectiveEndDate.HasValue || gls.Schedule.EffectiveEndDate >= fromDate );

            // Get all participant counts for all filtered opportunities; we'll hook them up to their respective opportunities below.
            var participantCounts = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gma =>
                    !gma.GroupMember.Person.IsDeceased
                    && qryGroupLocationSchedules.Any( gls =>
                        gls.Group.Id == gma.GroupMember.GroupId
                        && gls.Location.Id == gma.LocationId
                        && gls.Schedule.Id == gma.ScheduleId
                    )
                )
                .GroupBy( gma => new
                {
                    gma.GroupMember.GroupId,
                    gma.LocationId,
                    gma.ScheduleId
                } )
               .Select( g => new
               {
                   g.Key.GroupId,
                   g.Key.LocationId,
                   g.Key.ScheduleId,
                   Count = g.Count()
               } )
               .ToList();

            var opportunities = qryGroupLocationSchedules
                .ToList() // Execute the query; we have additional filtering that needs to happen once we materialize these objects.
                .Select( gls =>
                {
                    var participantCount = participantCounts.FirstOrDefault( c =>
                        c.GroupId == gls.Group.Id
                        && c.LocationId == gls.Location.Id
                        && c.ScheduleId == gls.Schedule.Id
                    )?.Count ?? 0;

                    return new Opportunity
                    {
                        Campus = gls.Group.Campus,
                        Project = gls.Group,
                        Location = gls.Location,
                        Schedule = gls.Schedule,
                        NextStartDateTime = gls.Schedule.NextStartDateTime,
                        ScheduleName = gls.Config?.ConfigurationName,
                        SlotsMin = gls.Config?.MinimumCapacity,
                        SlotsDesired = gls.Config?.DesiredCapacity,
                        SlotsMax = gls.Config?.MaximumCapacity,
                        ParticipantCount = participantCount,
                        GeoPoint = gls.Location.GeoPoint
                    };
                } );

            // Now that we have materialized Schedule objects in memory, let's further apply DateTime filtering using the Schedules' runtime-calculated
            // "NextStartDateTime" property values; only show Schedules that have current or upcoming start DateTimes.
            opportunities = opportunities
                .Where( o =>
                    o.NextStartDateTime.HasValue
                    && o.NextStartDateTime.Value >= fromDateTime
                    && (
                        !toDateTime.HasValue // The individual didn't select an end date.
                        || o.NextStartDateTime.Value < toDateTime.Value // The project's [next] start date time is less than the [end of the] end date they selected.
                    )
                );

            // Filter by slots available.
            if ( GetAttributeValue( AttributeKey.HideOvercapacityProjects ).AsBoolean() )
            {
                opportunities = opportunities.Where( o => o.SlotsAvailable > 0 );
            }

            if ( selectedFilters.SlotsAvailable.GetValueOrDefault() > 0
                && GetAttributeValue( AttributeKey.DisplaySlotsAvailableFilter ).AsBoolean()
                && new List<string> {
                    SlotsAvailableComparisonType.AtLeast,
                    SlotsAvailableComparisonType.NoMoreThan
                }.Contains( selectedFilters.SlotsAvailableComparisonType ) )
            {
                opportunities = opportunities
                    .Where( o =>
                    {
                        switch ( selectedFilters.SlotsAvailableComparisonType )
                        {
                            case SlotsAvailableComparisonType.AtLeast:
                                return o.SlotsAvailable >= selectedFilters.SlotsAvailable;
                            case SlotsAvailableComparisonType.NoMoreThan:
                                return o.SlotsAvailable <= selectedFilters.SlotsAvailable;
                            default:
                                return false;
                        }
                    } );
            }

            // Try to calculate the distance to each opportunity.
            // Perform this last, as it could be an expensive operation; refine the available opportunities above, first.
            var sortByProvidedLocation = !string.IsNullOrWhiteSpace( selectedFilters.LocationSort ) && GetAttributeValue( AttributeKey.DisplayLocationSort ).AsBoolean();
            var filterByProvidedRange = selectedFilters.LocationRange.GetValueOrDefault() > 0 && GetShouldDisplayLocationRangeFilter();

            var calculateDistances = this.IsAuthenticated
                || sortByProvidedLocation
                || filterByProvidedRange;

            // Go ahead and materialize this list so we don't enumerate through the above filters multiple times.
            var filteredOpportunities = opportunities.ToList();

            if ( calculateDistances )
            {
                // Display distance calculation failures to the individual only if they are actively searching with a location or range specified.
                // (But not if we're using a logged-in individual's home location behind the scenes to sort).
                var throwIfUnsuccessful = sortByProvidedLocation || filterByProvidedRange;

                CalculateDistances( rockContext, filteredOpportunities, selectedFilters.LocationSort, throwIfUnsuccessful );
            }

            // Filter by location range.
            if ( filterByProvidedRange )
            {
                filteredOpportunities = filteredOpportunities
                    .Where( o =>
                        o.DistanceInMiles.HasValue
                        && o.DistanceInMiles <= selectedFilters.LocationRange.Value
                    )
                    .ToList();
            }

            // Sort.
            List<Opportunity> sortedOpportunities = filteredOpportunities
                .OrderBy( o => o.DistanceInMiles.HasValue ? o.DistanceInMiles : double.MaxValue )
                .ThenBy( o => o.NextStartDateTime ?? DateTime.MaxValue )
                .ThenBy( o => o.ProjectName )
                .ThenByDescending( o => o.ParticipantCount )
                .ToList();

            return sortedOpportunities;
        }

        /// <summary>
        /// Gets the results HTML.
        /// </summary>
        /// <param name="opportunities">The opportunities.</param>
        /// <returns>The results HTML.</returns>
        private string GetResultsHtml( List<Opportunity> opportunities )
        {
            var lavaTemplate = GetAttributeValue( AttributeKey.ResultsLavaTemplate );
            var mergeFields = this.RequestContext.GetCommonMergeFields();

            var projects = opportunities.Select( o =>
            {
                var projectIdKey = o.Project.IdKey;
                var locationIdKey = o.Location.IdKey;
                var scheduleIdKey = o.Schedule.IdKey;

                var projectDetailPageUrl = GetLinkedPageUrl( AttributeKey.ProjectDetailPage, projectIdKey, locationIdKey, scheduleIdKey );
                var registrationPageUrl = GetLinkedPageUrl( AttributeKey.RegistrationPage, projectIdKey, locationIdKey, scheduleIdKey );

                return o.ToProject( projectDetailPageUrl, registrationPageUrl );
            } ).ToList();

            mergeFields.Add( "Projects", projects );

            return lavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the results header HTML.
        /// </summary>
        /// <returns>The results header HTML.</returns>
        private string GetResultsHeaderHtml()
        {
            var lavaTemplate = GetAttributeValue( AttributeKey.ResultsHeaderLavaTemplate );
            var mergeFields = this.RequestContext.GetCommonMergeFields();

            return lavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Calculates and populates the <see cref="Opportunity.DistanceInMiles"/> for each <see cref="Opportunity"/> in the collection, whose distance hasn't already been calculated.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="opportunities">The collection of <see cref="Opportunity"/>s for which to calculate the distance.</param>
        /// <param name="throwIfUnsuccessful">Whether an exception should be thrown (which will bubble up to the UI) if distances cannot be calculated.</param>
        private void CalculateDistances( RockContext rockContext, List<Opportunity> opportunities, string locationSort, bool throwIfUnsuccessful = false )
        {
            if ( !opportunities.Any( o => !o.DistanceInMiles.HasValue && o.GeoPoint != null ) )
            {
                return;
            }

            // We'll throw this exception if unable to determine the origin for the search, so the individual will
            // have some indication as to why their search isn't returning expected results.
            var friendlyException = new ApplicationException( "Unable to determine location." );

            // Determine the origin from which to calculate the distance.
            // If the locationSort is all numeric (zip code) we'll get the lat/long of the zip code and use that to
            // determine proximity, otherwise we’ll assume they provided a "city, state" which we'll use to get a
            // lat/long for. If they are logged in and have not provided a value we'll use their address that is
            // configured as their mapped location.
            DbGeography geoPointOrigin = null;
            if ( !string.IsNullOrWhiteSpace( locationSort ) )
            {
                try
                {
                    MapCoordinate mapCoordinate = null;
                    var locationService = new LocationService( rockContext );

                    if ( int.TryParse( locationSort, out int zipCode ) )
                    {
                        mapCoordinate = locationService.GetMapCoordinateFromPostalCode( locationSort );
                    }
                    else
                    {
                        var cityStateParts = locationSort.Split( ',' );
                        if ( cityStateParts.Length != 2 )
                        {
                            // Last check: maybe they entered "city state" format with no comma.
                            cityStateParts = locationSort.Split( ' ' );
                        }

                        if ( cityStateParts.Length == 2 )
                        {
                            var city = cityStateParts[0].Trim();
                            var state = cityStateParts[1].Trim();

                            if ( !string.IsNullOrWhiteSpace( city ) && !string.IsNullOrWhiteSpace( state ) )
                            {
                                mapCoordinate = locationService.GetMapCoordinateFromCityState( city, state );
                            }
                        }
                    }

                    if ( mapCoordinate != null )
                    {
                        geoPointOrigin = DbGeography.FromText( $"POINT({mapCoordinate.Longitude} {mapCoordinate.Latitude})" );
                    }
                }
                catch
                {
                    if ( throwIfUnsuccessful )
                    {
                        throw friendlyException;
                    }
                }
            }
            else if ( this.IsAuthenticated )
            {
                var person = this.RequestContext.CurrentPerson;
                if ( person != null )
                {
                    var homeLocation = person.GetHomeLocation();
                    geoPointOrigin = homeLocation?.GeoPoint;
                }
            }

            if ( geoPointOrigin == null )
            {
                if ( throwIfUnsuccessful )
                {
                    throw friendlyException;
                }

                return;
            }

            foreach ( var opportunity in opportunities.Where( o => !o.DistanceInMiles.HasValue && o.GeoPoint != null ) )
            {
                double meters = opportunity.GeoPoint.Distance( geoPointOrigin ) ?? 0.0D;
                double miles = meters * Location.MilesPerMeter;

                opportunity.DistanceInMiles = miles;
            }
        }

        /// <summary>
        /// Gets the available sign-up project group type guids for an individual to filter the results by, for the custom settings modal.
        /// </summary>
        /// <returns>A <see cref="Guid"/> list.</returns>
        private List<Guid> GetCustomSettingsProjectTypeGuids()
        {
            var groupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid();
            var groupTypeId = GroupTypeCache.GetId( groupTypeGuid );

            return GroupTypeCache
                .All()
                .Where( gt => gt.Guid.Equals( groupTypeGuid ) || gt.InheritedGroupTypeId.GetValueOrDefault() == groupTypeId )
                .Select( gt => gt.Guid )
                .ToList();
        }

        /// <summary>
        /// Gets the available group attributes for an individual to filter the results by, for the custom settings modal.
        /// </summary>
        /// <param name="selectedProjectTypeGuidStrings">The selected project type guid strings.</param>
        /// <param name="allowedGuids">
        /// The optional list of allowed guids that will be used to refine the list of <paramref name="selectedProjectTypeGuidStrings"/>.
        /// <para>
        /// This should be used to ensure that requests originating from the client don't pass more guids than they're authorized to pass.
        /// </para>
        /// </param>
        /// <returns>A <see cref="ListItemBag"/> list of the available group attributes for an individual to filter the results by.</returns>
        private List<ListItemBag> GetCustomSettingsDisplayAttributeFilters( IEnumerable<string> selectedProjectTypeGuidStrings, List<Guid> allowedGuids = null )
        {
            var filters = new List<ListItemBag>();

            if ( selectedProjectTypeGuidStrings?.Any() == true )
            {
                if ( allowedGuids?.Any() == true )
                {
                    selectedProjectTypeGuidStrings = selectedProjectTypeGuidStrings
                        .Where( a => allowedGuids.Any( g => g == a.AsGuidOrNull() ) );
                }

                // Attributes can be inherited from parent group types, so make sure we only add a given attribute once,
                // while noting to which group type(s) a given attribute belongs.
                var groupTypeNamesByAttributeGuid = new Dictionary<Guid, List<string>>();
                var attributeCachesByAttributeGuid = new Dictionary<Guid, AttributeCache>();

                foreach ( var projectTypeGuidString in selectedProjectTypeGuidStrings )
                {
                    var groupTypeCache = GroupTypeCache.Get( projectTypeGuidString );
                    if ( groupTypeCache == null )
                    {
                        continue;
                    }

                    var group = new Rock.Model.Group { GroupTypeId = groupTypeCache.Id };
                    group.LoadAttributes();

                    foreach ( var attribute in group.Attributes.Select( a => a.Value ) )
                    {
                        if ( !groupTypeNamesByAttributeGuid.TryGetValue( attribute.Guid, out List<string> groupTypeNames ) )
                        {
                            groupTypeNames = new List<string>();
                            groupTypeNamesByAttributeGuid.Add( attribute.Guid, groupTypeNames );
                            attributeCachesByAttributeGuid.TryAdd( attribute.Guid, attribute );
                        }

                        groupTypeNames.Add( groupTypeCache.Name );
                    }
                }

                foreach ( var kvp in groupTypeNamesByAttributeGuid )
                {
                    if ( !attributeCachesByAttributeGuid.TryGetValue( kvp.Key, out AttributeCache attributeCache ) )
                    {
                        continue;
                    }

                    filters.Add( new ListItemBag
                    {
                        Value = kvp.Key.ToString(),
                        Text = $"{attributeCache.Name} ({string.Join( ", ", kvp.Value.OrderBy( a => a ) )})"
                    } );
                }
            }

            return filters;
        }

        /// <summary>
        /// Gets the available campuses for an individual to filter the results by, for the custom settings modal.
        /// </summary>
        /// <param name="selectedCampusTypeGuidStrings">The selected campus type guid strings.</param>
        /// <param name="selectedCampusStatusGuidStrings">The selected campus status guid strings.</param>
        /// <returns>A <see cref="ListItemBag"/> list of the available campuses for an individual to filter the results by.</returns>
        private List<ListItemBag> GetCustomSettingsAvailableCampuses( IEnumerable<string> selectedCampusTypeGuidStrings, IEnumerable<string> selectedCampusStatusGuidStrings )
        {
            // Only get active campuses.
            var campusCaches = CampusCache.All( false );

            var selectedCampusTypeIds = ( selectedCampusTypeGuidStrings ?? new List<string>() )
                .AsGuidList()
                .Select( g => DefinedValueCache.GetId( g ) )
                .AsIntegerList();

            if ( selectedCampusTypeIds.Any() )
            {
                campusCaches = campusCaches
                    .Where( c => c.CampusTypeValueId.HasValue && selectedCampusTypeIds.Contains( c.CampusTypeValueId.Value ) )
                    .ToList();
            }

            var selectedCampusStatusIds = ( selectedCampusStatusGuidStrings ?? new List<string>() )
                .AsGuidList()
                .Select( g => DefinedValueCache.GetId( g ) )
                .AsIntegerList();

            if ( selectedCampusStatusIds.Any() )
            {
                campusCaches = campusCaches
                    .Where( c => c.CampusStatusValueId.HasValue && selectedCampusStatusIds.Contains( c.CampusStatusValueId.Value ) )
                    .ToList();
            }

            return campusCaches.ToListItemBagList();
        }

        /// <summary>
        /// Gets the linked page URL.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="projectIdKey">The project hashed identifier key.</param>
        /// <param name="locationIdKey">The location hashed identifier key.</param>
        /// <param name="scheduleIdKey">The schedule hashed identifier key.</param>
        /// <returns>The linked page URL or "#" if a given linked page attribute is not set.</returns>
        private string GetLinkedPageUrl( string attributeKey, string projectIdKey, string locationIdKey, string scheduleIdKey )
        {
            if ( string.IsNullOrWhiteSpace( GetAttributeValue( attributeKey ) ) )
            {
                return "#";
            }

            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.ProjectId, projectIdKey },
                { PageParameterKey.LocationId, locationIdKey },
                { PageParameterKey.ScheduleId, scheduleIdKey }
            };

            return this.GetLinkedPageUrl( attributeKey, queryParams );
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                return GetSecurityGrantToken();
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant()
                .ToToken();
        }

        #endregion

        #region IHasCustomActions

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "fa fa-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/Engagement/SignUp/signUpFinderCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the updated attributes that should be presented as filter items for the search.
        /// </summary>
        /// <param name="selectedProjectTypeGuidStrings">The selected project type guid strings.</param>
        /// <returns>The attributes that should be presented as filter items for the search, grouped by project type [guid].</returns>
        [BlockAction]
        public BlockActionResult GetUpdatedAttributes( IEnumerable<string> selectedProjectTypeGuidStrings )
        {
            using ( var rockContext = new RockContext() )
            {
                // Load block attributes, as we're going to double-check the provided guid strings against those available according to the settings.
                var block = new BlockService( rockContext ).Get( this.BlockId );
                block.LoadAttributes( rockContext );

                return ActionOk( GetAttributeFilterItems( rockContext, selectedProjectTypeGuidStrings ) );
            }
        }

        /// <summary>
        /// Gets the sign-up projects that should be displayed based on the provided filters.
        /// </summary>
        /// <param name="bag">The object containing the filter criteria.</param>
        /// <returns>The sign-up projects that should be displayed based on the provided filters.</returns>
        [BlockAction]
        public BlockActionResult GetFilteredProjects( SignUpFinderSelectedFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                // Load block attributes, as we're going to double-check that each type of filtering is allowed according to the settings.
                var block = new BlockService( rockContext ).Get( this.BlockId );
                block.LoadAttributes( rockContext );

                var opportunities = GetOpportunities( rockContext, bag );

                var results = new SignUpFinderResultsBag
                {
                    ResultsHtml = GetResultsHtml( opportunities ),
                    ResultsHeaderHtml = GetResultsHeaderHtml()
                };

                return ActionOk( results );
            }
        }

        /// <summary>
        /// Gets the values and all other required details that will be needed to display the custom settings modal.
        /// </summary>
        /// <returns>A box that contains the custom settings values and additional data.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit block settings." );
                }

                var campusSettings = GetCampusSettings();

                // The available attribute filters are based on the currently-selected group type(s), so get these first.
                var projectTypes = GetAttributeValue( AttributeKey.ProjectTypes ).GroupTypeGuidsToListItemBagList();

                var options = new SignUpFinderCustomSettingsOptionsBag
                {
                    AvailableProjectTypeGuids = GetCustomSettingsProjectTypeGuids(),
                    AvailableDisplayAttributeFilters = GetCustomSettingsDisplayAttributeFilters( projectTypes.Select( pt => pt.Value ) ),
                    AvailableCampuses = campusSettings.AvailableCampuses
                };

                // Ensure we don't try to preselect an attribute filter that was previously saved, but no longer relevant based on the currently-selected group type(s).
                var displayAttributeFilters = GetAttributeValue( AttributeKey.DisplayAttributeFilters )
                    .ToListItemBagListUsingAvailableOptions( options.AvailableDisplayAttributeFilters )
                    .Select( a => a.Value )
                    .ToList();

                var settings = new SignUpFinderCustomSettingsBag
                {
                    // Layout / Initial Page Load.
                    HideOvercapacityProjects = GetAttributeValue( AttributeKey.HideOvercapacityProjects ).AsBoolean(),
                    LoadResultsOnInitialPageLoad = GetAttributeValue( AttributeKey.LoadResultsOnInitialPageLoad ).AsBoolean(),
                    DisplayProjectFiltersAs = GetAttributeValue( AttributeKey.DisplayProjectFiltersAs ),
                    FilterColumns = GetAttributeValue( AttributeKey.FilterColumns ).ToIntSafe(),

                    // Project Filters.
                    ProjectTypes = projectTypes,
                    ProjectTypeFilterLabel = GetAttributeValue( AttributeKey.ProjectTypeFilterLabel ),
                    DisplayAttributeFilters = displayAttributeFilters,

                    // Campus Filters.
                    DisplayCampusFilterSettings = !CampusCache.SingleCampusId.HasValue, // Only display campus-related settings if there is more than one campus.
                    DisplayCampusFilter = campusSettings.DisplayCampusFilter,
                    EnableCampusContext = campusSettings.EnableCampusContext,
                    HideCampusesWithNoOpportunities = campusSettings.HideCampusesWithNoOpportunities,
                    CampusTypes = campusSettings.SelectedCampusTypes,
                    CampusStatuses = campusSettings.SelectedCampusStatuses,
                    Campuses = campusSettings.SelectedCampuses.Select( c => c.Value ).ToList(),

                    // Schedule Filters.
                    DisplayNamedScheduleFilter = GetAttributeValue( AttributeKey.DisplayNamedScheduleFilter ).AsBoolean(),
                    NamedScheduleFilterLabel = GetAttributeValue( AttributeKey.NamedScheduleFilterLabel ),
                    RootScheduleCategory = GetAttributeValue( AttributeKey.RootScheduleCategory ).CategoryGuidToListItemBag(),

                    // Location Filters.
                    DisplayLocationSort = GetAttributeValue( AttributeKey.DisplayLocationSort ).AsBoolean(),
                    LocationSortLabel = GetAttributeValue( AttributeKey.LocationSortLabel ),
                    DisplayLocationRangeFilter = GetAttributeValue( AttributeKey.DisplayLocationRangeFilter ).AsBoolean(),

                    // Additional Filters.
                    DisplayDateRange = GetAttributeValue( AttributeKey.DisplayDateRange ).AsBoolean(),
                    DisplaySlotsAvailableFilter = GetAttributeValue( AttributeKey.DisplaySlotsAvailableFilter ).AsBoolean(),

                    // Lava.
                    ResultsLavaTemplate = GetAttributeValue( AttributeKey.ResultsLavaTemplate ),
                    ResultsHeaderLavaTemplate = GetAttributeValue( AttributeKey.ResultsHeaderLavaTemplate ),

                    // Linked Pages.
                    ProjectDetailPage = GetAttributeValue( AttributeKey.ProjectDetailPage ).ToPageRouteValueBag(),
                    RegistrationPage = GetAttributeValue( AttributeKey.RegistrationPage ).ToPageRouteValueBag()
                };

                return ActionOk( new CustomSettingsBox<SignUpFinderCustomSettingsBag, SignUpFinderCustomSettingsOptionsBag>
                {
                    Settings = settings,
                    Options = options,
                    SecurityGrantToken = GetSecurityGrantToken()
                } );
            }
        }

        /// <summary>
        /// Gets the updated display attribute filters.
        /// </summary>
        /// <param name="selectedProjectTypeGuidStrings">The selected project type guid strings.</param>
        /// <returns>A <see cref="ListItemBag"/> list representing the updated display attribute filters.</returns>
        [BlockAction]
        public BlockActionResult GetUpdatedDisplayAttributeFilters( IEnumerable<string> selectedProjectTypeGuidStrings )
        {
            return ActionOk( GetCustomSettingsDisplayAttributeFilters( selectedProjectTypeGuidStrings, GetCustomSettingsProjectTypeGuids() ) );
        }

        /// <summary>
        /// Gets the updated available campuses.
        /// </summary>
        /// <param name="bag">The information needed to get the available campuses.</param>
        /// <returns>A <see cref="ListItemBag"/> list representing the updated available campuses.</returns>
        [BlockAction]
        public BlockActionResult GetUpdatedAvailableCampuses( GetAvailableCampusesBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest();
            }

            return ActionOk( GetCustomSettingsAvailableCampuses( bag.SelectedCampusTypes, bag.SelectedCampusStatuses ) );
        }

        /// <summary>
        /// Saves the updates to the custom setting values for this block, for the custom settings modal.
        /// </summary>
        /// <param name="box">The box that contains the setting values.</param>
        /// <returns>A response that indicates if the save was successful or not.</returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<SignUpFinderCustomSettingsBag, SignUpFinderCustomSettingsOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit block settings." );
                }

                var block = new BlockService( rockContext ).Get( this.BlockId );
                block.LoadAttributes( rockContext );

                #region Layout / Initial Page Load

                box.IfValidProperty( nameof( box.Settings.HideOvercapacityProjects ),
                    () => block.SetAttributeValue( AttributeKey.HideOvercapacityProjects, box.Settings.HideOvercapacityProjects.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.LoadResultsOnInitialPageLoad ),
                    () => block.SetAttributeValue( AttributeKey.LoadResultsOnInitialPageLoad, box.Settings.LoadResultsOnInitialPageLoad.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.DisplayProjectFiltersAs ),
                    () => block.SetAttributeValue( AttributeKey.DisplayProjectFiltersAs, box.Settings.DisplayProjectFiltersAs ) );

                box.IfValidProperty( nameof( box.Settings.FilterColumns ),
                    () => block.SetAttributeValue( AttributeKey.FilterColumns, box.Settings.FilterColumns ) );

                #endregion

                #region Project Filters

                box.IfValidProperty( nameof( box.Settings.ProjectTypes ),
                    () => block.SetAttributeValue( AttributeKey.ProjectTypes, box.Settings.ProjectTypes.ToCommaDelimitedValuesString() ) );

                box.IfValidProperty( nameof( box.Settings.ProjectTypeFilterLabel ),
                    () => block.SetAttributeValue( AttributeKey.ProjectTypeFilterLabel, box.Settings.ProjectTypeFilterLabel ) );

                box.IfValidProperty( nameof( box.Settings.DisplayAttributeFilters ),
                    () => block.SetAttributeValue( AttributeKey.DisplayAttributeFilters, box.Settings.DisplayAttributeFilters.AsDelimited( "," ) ) );

                #endregion

                #region Campus Filters

                box.IfValidProperty( nameof( box.Settings.DisplayCampusFilter ),
                    () => block.SetAttributeValue( AttributeKey.DisplayCampusFilter, box.Settings.DisplayCampusFilter.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.EnableCampusContext ),
                    () => block.SetAttributeValue( AttributeKey.EnableCampusContext, box.Settings.EnableCampusContext.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.HideCampusesWithNoOpportunities ),
                    () => block.SetAttributeValue( AttributeKey.HideCampusesWithNoOpportunities, box.Settings.HideCampusesWithNoOpportunities.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.CampusTypes ),
                    () => block.SetAttributeValue( AttributeKey.CampusTypes, box.Settings.CampusTypes.ToCommaDelimitedValuesString() ) );

                box.IfValidProperty( nameof( box.Settings.CampusStatuses ),
                    () => block.SetAttributeValue( AttributeKey.CampusStatuses, box.Settings.CampusStatuses.ToCommaDelimitedValuesString() ) );

                box.IfValidProperty( nameof( box.Settings.Campuses ),
                    () => block.SetAttributeValue( AttributeKey.Campuses, box.Settings.Campuses.AsDelimited( "," ) ) );

                #endregion

                #region Schedule Filters

                box.IfValidProperty( nameof( box.Settings.DisplayNamedScheduleFilter ),
                    () => block.SetAttributeValue( AttributeKey.DisplayNamedScheduleFilter, box.Settings.DisplayNamedScheduleFilter.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.NamedScheduleFilterLabel ),
                    () => block.SetAttributeValue( AttributeKey.NamedScheduleFilterLabel, box.Settings.NamedScheduleFilterLabel ) );

                box.IfValidProperty( nameof( box.Settings.RootScheduleCategory ),
                    () => block.SetAttributeValue( AttributeKey.RootScheduleCategory, box.Settings.RootScheduleCategory.ToValueString() ) );

                #endregion

                #region Location Filters

                box.IfValidProperty( nameof( box.Settings.DisplayLocationSort ),
                    () => block.SetAttributeValue( AttributeKey.DisplayLocationSort, box.Settings.DisplayLocationSort.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.LocationSortLabel ),
                    () => block.SetAttributeValue( AttributeKey.LocationSortLabel, box.Settings.LocationSortLabel ) );

                box.IfValidProperty( nameof( box.Settings.DisplayLocationRangeFilter ),
                    () => block.SetAttributeValue( AttributeKey.DisplayLocationRangeFilter, box.Settings.DisplayLocationRangeFilter.ToString() ) );

                #endregion

                #region Additional Filters

                box.IfValidProperty( nameof( box.Settings.DisplayDateRange ),
                    () => block.SetAttributeValue( AttributeKey.DisplayDateRange, box.Settings.DisplayDateRange.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.DisplaySlotsAvailableFilter ),
                    () => block.SetAttributeValue( AttributeKey.DisplaySlotsAvailableFilter, box.Settings.DisplaySlotsAvailableFilter.ToString() ) );

                #endregion

                #region Lava

                box.IfValidProperty( nameof( box.Settings.ResultsLavaTemplate ),
                    () => block.SetAttributeValue( AttributeKey.ResultsLavaTemplate, box.Settings.ResultsLavaTemplate ) );

                box.IfValidProperty( nameof( box.Settings.ResultsHeaderLavaTemplate ),
                    () => block.SetAttributeValue( AttributeKey.ResultsHeaderLavaTemplate, box.Settings.ResultsHeaderLavaTemplate ) );

                #endregion

                #region Linked Pages

                box.IfValidProperty( nameof( box.Settings.ProjectDetailPage ),
                    () => block.SetAttributeValue( AttributeKey.ProjectDetailPage, box.Settings.ProjectDetailPage.ToCommaDelimitedPageRouteValues() ) );

                box.IfValidProperty( nameof( box.Settings.RegistrationPage ),
                    () => block.SetAttributeValue( AttributeKey.RegistrationPage, box.Settings.RegistrationPage.ToCommaDelimitedPageRouteValues() ) );

                #endregion

                block.SaveAttributeValues( rockContext );

                return ActionOk();
            }
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// This POCO will hold campus-related settings to be used within the custom settings
        /// modal in an administrative setting, as well as the public-facing search filters.
        /// </summary>
        private class CampusSettings
        {
            /// <summary>
            /// Gets or sets whether the campus filter should be shown.
            /// </summary>
            public bool DisplayCampusFilter { get; set; }

            /// <summary>
            /// Gets or sets whether to hide campuses with no opportunities.
            /// </summary>
            public bool HideCampusesWithNoOpportunities { get; set; }

            /// <summary>
            /// Gets or sets whether to enable the page campus context.
            /// </summary>
            public bool EnableCampusContext { get; set; }

            /// <summary>
            /// Gets or sets the selected campus types.
            /// </summary>
            public List<ListItemBag> SelectedCampusTypes { get; set; }

            /// <summary>
            /// Gets or sets the selected campus statuses.
            /// </summary>
            public List<ListItemBag> SelectedCampusStatuses { get; set; }

            /// <summary>
            /// Gets or sets the available campuses.
            /// </summary>
            public List<ListItemBag> AvailableCampuses { get; set; }

            /// <summary>
            /// Gets or sets the selected campuses.
            /// </summary>
            public List<ListItemBag> SelectedCampuses { get; set; }

            /// <summary>
            /// Gets the allowed campuses based on the current campus settings.
            /// <para>
            /// This will never return null.
            /// </para>
            /// </summary>
            public List<ListItemBag> AllowedCampuses
            {
                get
                {
                    if ( !this.DisplayCampusFilter && this.EnableCampusContext )
                    {
                        return new List<ListItemBag>();
                    }

                    if ( this.SelectedCampuses?.Any() == true )
                    {
                        return this.SelectedCampuses;
                    }

                    return this.AvailableCampuses ?? new List<ListItemBag>();
                }
            }

            /// <summary>
            /// Gets whether a block admin applied any behind-the-scenes campus filters.
            /// </summary>
            public bool AreCampusesFilteredByBlockSettings
            {
                get
                {
                    return this.SelectedCampusTypes?.Any() == true
                        || this.SelectedCampusStatuses?.Any() == true
                        || this.SelectedCampuses?.Any() == true;
                }
            }
        }

        /// <summary>
        /// This POCO will be used to hold project opportunity instances (GroupLocationSchedules), against which we'll perform filtering.
        /// <para>
        /// It will also provide convenience properties for the final, Lava class, <see cref="SignUpFinder.Project"/> to easily pick from.
        /// </para>
        /// </summary>
        private class Opportunity
        {
            public Campus Campus { get; set; }

            public Rock.Model.Group Project { get; set; }

            public Location Location { get; set; }

            public Schedule Schedule { get; set; }

            public DateTime? NextStartDateTime { get; set; }

            public string ScheduleName { get; set; }

            public int? SlotsMin { get; set; }

            public int? SlotsDesired { get; set; }

            public int? SlotsMax { get; set; }

            public int ParticipantCount { get; set; }

            public DbGeography GeoPoint { get; set; }

            public double? DistanceInMiles { get; set; }

            public string ProjectName
            {
                get
                {
                    return this.Project?.Name;
                }
            }

            public string Description
            {
                get
                {
                    return this.Project?.Description;
                }
            }

            public bool ScheduleHasFutureStartDateTime
            {
                get
                {
                    return this.Schedule != null
                        && this.Schedule.NextStartDateTime.HasValue
                        && this.Schedule.NextStartDateTime.Value >= RockDateTime.Now;
                }
            }

            public string FriendlySchedule
            {
                get
                {
                    if ( !this.ScheduleHasFutureStartDateTime )
                    {
                        return "No upcoming occurrences.";
                    }

                    var friendlySchedule = this.NextStartDateTime.Value.ToString( "dddd, MMM d h:mm tt" );

                    if ( this.NextStartDateTime.Value.Year != RockDateTime.Now.Year )
                    {
                        friendlySchedule = $"{friendlySchedule} ({this.NextStartDateTime.Value.Year})";
                    }

                    return friendlySchedule;
                }
            }

            public int SlotsAvailable
            {
                get
                {
                    if ( !this.ScheduleHasFutureStartDateTime )
                    {
                        return 0;
                    }

                    // This more complex approach uses a dynamic/floating minuend:
                    // 1) If the max value is defined, use that;
                    // 2) Else, if the desired value is defined, use that;
                    // 3) Else, if the min value is defined, use that;
                    // 4) Else, use int.MaxValue (there is no limit to the slots available).
                    //var minuend = this.SlotsMax.GetValueOrDefault() > 0
                    //    ? this.SlotsMax.Value
                    //    : this.SlotsDesired.GetValueOrDefault() > 0
                    //        ? this.SlotsDesired.Value
                    //        : this.SlotsMin.GetValueOrDefault() > 0
                    //            ? this.SlotsMin.Value
                    //            : int.MaxValue;

                    // Simple approach:
                    // 1) If the max value is defined, subtract participant count from that;
                    // 2) Otherwise, use int.MaxValue (there is no limit to the slots available).
                    var available = int.MaxValue;
                    if ( this.SlotsMax.GetValueOrDefault() > 0 )
                    {
                        available = this.SlotsMax.Value - this.ParticipantCount;
                    }

                    return available < 0 ? 0 : available;
                }
            }

            /// <summary>
            /// Converts an <see cref="Opportunity"/> to a <see cref="SignUpFinder.Project"/> for display within the lava results template.
            /// </summary>
            /// <param name="projectDetailPageUrl">The project detail page URL for this <see cref="SignUpFinder.Project"/>.</param>
            /// <param name="registrationPageUrl">The registration page URL for this <see cref="SignUpFinder.Project"/>.</param>
            /// <returns>a <see cref="SignUpFinder.Project"/> instance for display within the lava results template.</returns>
            public Project ToProject( string projectDetailPageUrl, string registrationPageUrl )
            {
                int? availableSpots = null;
                if ( this.SlotsAvailable != int.MaxValue )
                {
                    availableSpots = this.SlotsAvailable;
                }

                var showRegisterButton = this.ScheduleHasFutureStartDateTime
                    &&
                    (
                        !availableSpots.HasValue
                        || availableSpots.Value > 0
                    );

                string mapCenter = null;
                if ( this.Location.Latitude.HasValue && this.Location.Longitude.HasValue )
                {
                    mapCenter = $"{this.Location.Latitude.Value},{this.Location.Longitude.Value}";
                }
                else
                {
                    var streetAddress = this.Location.GetFullStreetAddress();
                    if ( !string.IsNullOrWhiteSpace( streetAddress ) )
                    {
                        mapCenter = streetAddress;
                    }
                }

                return new Project
                {
                    Name = this.ProjectName,
                    Description = this.Description,
                    ScheduleName = this.ScheduleName,
                    FriendlySchedule = this.FriendlySchedule,
                    AvailableSpots = availableSpots,
                    ShowRegisterButton = showRegisterButton,
                    DistanceInMiles = this.DistanceInMiles,
                    MapCenter = mapCenter,
                    ProjectDetailPageUrl = projectDetailPageUrl,
                    RegisterPageUrl = registrationPageUrl,
                    GroupId = this.Project.Id,
                    LocationId = this.Location.Id,
                    ScheduleId = this.Schedule.Id
                };
            }
        }

        /// <summary>
        /// This POCO will be passed to the results Lava template, one instance for each project opportunity (GroupLocationSchedule).
        /// </summary>
        /// <seealso cref="Rock.Utility.RockDynamic" />
        private class Project : RockDynamic
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public string ScheduleName { get; set; }

            public string FriendlySchedule { get; set; }

            public int? AvailableSpots { get; set; }

            public bool ShowRegisterButton { get; set; }

            public double? DistanceInMiles { get; set; }

            public string MapCenter { get; set; }

            public string ProjectDetailPageUrl { get; set; }

            public string RegisterPageUrl { get; set; }

            public int CampusId { get; set; }

            public int GroupId { get; set; }

            public int LocationId { get; set; }

            public int ScheduleId { get; set; }
        }

        #endregion
    }
}

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

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn
{
    /// <summary>
    /// Provides a way to manually enter attendance for a large group of people in an efficient manner.
    /// </summary>
    [DisplayName( "Rapid Attendance Entry" )]
    [Category( "Check-in" )]
    [Description( "Provides a way to manually enter attendance for a large group of people in an efficient manner." )]
    [IconCssClass( "ti ti-users" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #region General

    [LinkedPage(
        "Add Family Page",
        Key = AttributeKey.AddFamilyPage,
        Description = "The page used to add new families.",
        Category = AttributeCategory.General,
        Order = 0,
        IsRequired = false )]

    [LinkedPage(
        "Attendance List Page",
        Key = AttributeKey.AttendanceListPage,
        Description = "The page where attendance records are displayed.",
        Category = AttributeCategory.General,
        Order = 1,
        IsRequired = false )]

    #endregion General

    #region Attendance

    [BooleanField(
        "Enable Attendance",
        Key = AttributeKey.EnableAttendance,
        Description = "Enables the attendance setup screen at the start of each session. Attendance can then be taken for family members.",
        Category = AttributeCategory.Attendance,
        DefaultBooleanValue = true,
        Order = 0,
        IsRequired = false )]

    [GroupField(
        "Parent Group",
        Key = AttributeKey.ParentGroup,
        Description = "Limits the group picker to children of this group.",
        Category = AttributeCategory.Attendance,
        Order = 1,
        IsRequired = false )]

    [GroupField(
        "Attendance Group",
        Key = AttributeKey.AttendanceGroup,
        Description = "Locks the block to a specific group. Only schedule and date are configurable at session start.",
        Category = AttributeCategory.Attendance,
        Order = 2,
        IsRequired = false )]

    [BooleanField(
        "Show Can Check-In Relationships",
        Key = AttributeKey.ShowCanCheckInRelationships,
        Description = @"Includes people linked via a ""Can check-in"" known relationship in the attendance list.",
        Category = AttributeCategory.Attendance,
        DefaultBooleanValue = true,
        Order = 3,
        IsRequired = false )]

    [IntegerField(
        "Minimum Attendance Age",
        Key = AttributeKey.AttendanceAgeLimit,
        Description = "Family members below this age cannot be marked as attended. Set as zero to allow attendance for individuals of any age.",
        Category = AttributeCategory.Attendance,
        DefaultIntegerValue = 0,
        Order = 4,
        IsRequired = true )]

    [BooleanField(
        "Show Campus Filter",
        Key = AttributeKey.ShowCampus,
        Description = "When visible, the campus picker filters available group locations by campus.",
        Category = AttributeCategory.Attendance,
        DefaultBooleanValue = true,
        Order = 5,
        IsRequired = true )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "Limits the campus picker to campuses of the selected type(s).",
        Category = AttributeCategory.Attendance,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 6,
        IsRequired = false )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "Limits the campus picker to campuses with the selected status(es).",
        Category = AttributeCategory.Attendance,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 7,
        IsRequired = false )]

    #endregion Attendance

    #region Family

    [AttributeField(
        "Family Attributes",
        Key = AttributeKey.FamilyAttributes,
        Description = "Attributes shown on the Edit Family panel.",
        Category = AttributeCategory.Family,
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        AllowMultiple = true,
        Order = 0,
        IsRequired = false )]

    [CodeEditorField(
        "Family Header Template",
        Key = AttributeKey.FamilyHeaderLavaTemplate,
        Description = "Lava template rendered as the family header above the contact entry area.",
        Category = AttributeCategory.Family,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        DefaultValue = AttributeDefault.FamilyHeaderLavaTemplate,
        Order = 1,
        IsRequired = true )]

    #endregion Family

    #region Individual

    [CodeEditorField(
        "Individual Header Template",
        Key = AttributeKey.IndividualHeaderLavaTemplate,
        Description = "Lava template for the personal summary displayed when viewing an individual.",
        Category = AttributeCategory.Individual,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        DefaultValue = AttributeDefault.IndividualHeaderLavaTemplate,
        Order = 0,
        IsRequired = true )]

    [DefinedValueField(
        "Adult Phone Types",
        Key = AttributeKey.AdultPhoneTypes,
        Description = "Phone number types shown and editable when editing an adult.",
        Category = AttributeCategory.Individual,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        AllowMultiple = true,
        Order = 1,
        IsRequired = false )]

    [AttributeField(
        "Adult Person Attributes",
        Key = AttributeKey.AdultPersonAttributes,
        Description = "Person attributes shown on the edit panel for adults.",
        Category = AttributeCategory.Individual,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        AllowMultiple = true,
        Order = 2,
        IsRequired = false )]

    [BooleanField(
        "Adult Communication Preference",
        Key = AttributeKey.ShowCommunicationPreference,
        Description = "Shows the communication preference field (Email or SMS) when editing an adult.",
        Category = AttributeCategory.Individual,
        DefaultBooleanValue = true,
        Order = 3,
        IsRequired = false )]

    [DefinedValueField(
        "Child Phone Types",
        Key = AttributeKey.ChildPhoneTypes,
        Description = "Phone number types shown and editable when editing a child.",
        Category = AttributeCategory.Individual,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        AllowMultiple = true,
        Order = 4,
        IsRequired = false )]

    [AttributeField(
        "Child Person Attributes",
        Key = AttributeKey.ChildPersonAttributes,
        Description = "Person attributes shown on the edit panel for children.",
        Category = AttributeCategory.Individual,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        AllowMultiple = true,
        Order = 5,
        IsRequired = false )]

    [BooleanField(
        "Allow Child Email Edit",
        Key = AttributeKey.ChildAllowEmailEdit,
        Description = "Makes the email field visible and editable when editing a child.",
        Category = AttributeCategory.Individual,
        DefaultBooleanValue = true,
        Order = 6,
        IsRequired = false )]

    [CustomDropdownListField(
        "Race",
        Key = AttributeKey.RaceOption,
        Description = "Controls whether the race field appears on the edit panel and whether a value is required.",
        Category = AttributeCategory.Individual,
        ListSource = ListSource.HideOptionalRequired,
        DefaultValue = "Hide",
        Order = 7,
        IsRequired = false )]

    [CustomDropdownListField(
        "Ethnicity",
        Key = AttributeKey.EthnicityOption,
        Description = "Controls whether the ethnicity field appears on the edit panel and whether a value is required.",
        Category = AttributeCategory.Individual,
        ListSource = ListSource.HideOptionalRequired,
        DefaultValue = "Hide",
        Order = 8,
        IsRequired = false )]

    #endregion Individual

    #region Workflow

    [TextField(
        "Workflow List Title",
        Key = AttributeKey.WorkflowListTitle,
        Description = "The label displayed above the workflow checkbox list.",
        Category = AttributeCategory.Workflow,
        DefaultValue = "Interested In",
        Order = 0,
        IsRequired = false )]

    [WorkflowTypeField(
        "Workflow Types",
        Key = AttributeKey.WorkflowTypes,
        Description = "Workflows shown as checkboxes. Selected workflows are launched for the person when the form is saved.",
        Category = AttributeCategory.Workflow,
        AllowMultiple = true,
        Order = 1,
        IsRequired = false )]

    #endregion Workflow

    #region Connections

    [TextField(
        "Connection Opportunities List Title",
        Key = AttributeKey.ConnectionOpportunitiesListTitle,
        Description = "The label displayed above the Connection Opportunities checkbox list.",
        Category = AttributeCategory.Connections,
        DefaultValue = "Connection Opportunities",
        Order = 0,
        IsRequired = false )]

    [ConnectionTypeField(
        "Connection Type",
        Key = AttributeKey.ConnectionType,
        Description = "Connection opportunities from the configured type are shown as checkboxes. If no type is configured, the section is hidden.",
        Category = AttributeCategory.Connections,
        Order = 1,
        IsRequired = false )]

    #endregion Connections

    #region Notes

    [NoteTypeField(
        "Note Types",
        Key = AttributeKey.NoteTypes,
        Description = "Note types available in the Note section. When only one type is configured, the type dropdown is hidden.",
        Category = AttributeCategory.Notes,
        EntityTypeName = "Rock.Model.Person",
        AllowMultiple = true,
        Order = 0,
        IsRequired = false )]

    #endregion Notes

    #region Prayer

    [BooleanField(
        "Enable Prayer Requests",
        Key = AttributeKey.EnablePrayerRequestEntry,
        Description = "Shows the prayer request section on each person's entry panel.",
        Category = AttributeCategory.Prayer,
        DefaultBooleanValue = true,
        Order = 0,
        IsRequired = false )]

    [BooleanField(
        "Urgent Flag",
        Key = AttributeKey.ShowUrgentFlag,
        Description = "Shows the Urgent checkbox on the prayer request form, allowing a request to be flagged as urgent.",
        Category = AttributeCategory.Prayer,
        DefaultBooleanValue = true,
        Order = 1,
        IsRequired = false )]

    [BooleanField(
        "Public Flag",
        Key = AttributeKey.ShowPublicFlag,
        Description = "Shows the Public checkbox, allowing a request to be flagged for display on the public website.",
        Category = AttributeCategory.Prayer,
        DefaultBooleanValue = true,
        Order = 2,
        IsRequired = false )]

    [IntegerField(
        "Expiration (Days)",
        Key = AttributeKey.ExpiresAfter,
        Description = "The number of days before a prayer request expires.",
        Category = AttributeCategory.Prayer,
        DefaultIntegerValue = 14,
        Order = 3,
        IsRequired = true )]

    [CategoryField(
        "Default Category",
        Key = AttributeKey.DefaultCategory,
        Description = "The category applied to new prayer requests.",
        Category = AttributeCategory.Prayer,
        EntityTypeName = "Rock.Model.PrayerRequest",
        Order = 4,
        IsRequired = false )]

    [BooleanField(
        "Default to Public",
        Key = AttributeKey.DisplayToPublic,
        Description = "Sets new prayer requests as public by default.",
        Category = AttributeCategory.Prayer,
        DefaultBooleanValue = true,
        Order = 5,
        IsRequired = false )]

    [BooleanField(
        "Allow Comments by Default",
        Key = AttributeKey.DefaultAllowComments,
        Description = "Whether new prayer requests allow comments during a prayer session.",
        Category = AttributeCategory.Prayer,
        Order = 6,
        IsRequired = false )]

    [BooleanField(
        "Enable Category Selection",
        Key = AttributeKey.EnableCategorySelection,
        Description = "Shows the category picker on the prayer request form. When hidden, the default category is applied automatically.",
        Category = AttributeCategory.Prayer,
        DefaultBooleanValue = true,
        Order = 7,
        IsRequired = false )]

    #endregion Prayer

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "09BCACC3-F821-4AAF-9843-8CDE982C318A" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "31AC7A04-AD88-4F51-9847-4D75A2B72C32" )]
    [Rock.SystemGuid.BlockTypeGuid( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4" )]
    public class RapidAttendanceEntry : RockBlockType
    {
        #region Keys & Constants

        private static class AttributeKey
        {
            // General
            public const string AddFamilyPage = "AddFamilyPage";
            public const string AttendanceListPage = "AttendanceListPage";

            // Attendance
            public const string EnableAttendance = "EnableAttendance";
            public const string ParentGroup = "ParentGroup";
            public const string AttendanceGroup = "AttendanceGroup";
            public const string ShowCanCheckInRelationships = "ShowCanCheckInRelationships";
            public const string AttendanceAgeLimit = "AttendanceAgeLimit";
            public const string ShowCampus = "ShowCampus";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";

            // Family
            public const string FamilyAttributes = "FamilyAttributes";
            public const string FamilyHeaderLavaTemplate = "FamilyHeaderLavaTemplate";

            // Individual
            public const string IndividualHeaderLavaTemplate = "IndividualHeaderLavaTemplate";
            public const string AdultPhoneTypes = "AdultPhoneTypes";
            public const string AdultPersonAttributes = "AdultPersonAttributes";
            public const string ShowCommunicationPreference = "ShowCommunicationPreference";
            public const string ChildPhoneTypes = "ChildPhoneTypes";
            public const string ChildPersonAttributes = "ChildPersonAttributes";
            public const string ChildAllowEmailEdit = "ChildAllowEmailEdit";
            public const string RaceOption = "RaceOption";
            public const string EthnicityOption = "EthnicityOption";

            // Workflow
            public const string WorkflowListTitle = "WorkflowListTitle";
            public const string WorkflowTypes = "WorkflowTypes";

            // Connections
            public const string ConnectionOpportunitiesListTitle = "ConnectionOpportunitiesListTitle";
            public const string ConnectionType = "ConnectionType";

            // Notes
            public const string NoteTypes = "NoteTypes";

            // Prayer
            public const string EnablePrayerRequestEntry = "EnablePrayerRequestEntry";
            public const string ShowUrgentFlag = "ShowUrgentFlag";
            public const string ShowPublicFlag = "ShowPublicFlag";
            public const string ExpiresAfter = "ExpiresAfter";
            public const string DefaultCategory = "DefaultCategory";
            public const string DisplayToPublic = "DisplayToPublic";
            public const string DefaultAllowComments = "DefaultAllowComments";
            public const string EnableCategorySelection = "CategorySelection";
        }

        private static class AttributeCategory
        {
            public const string General = "";
            public const string Attendance = "Attendance";
            public const string Family = "Family";
            public const string Individual = "Individual";
            public const string Workflow = "Workflow";
            public const string Connections = "Connections";
            public const string Notes = "Notes";
            public const string Prayer = "Prayer";
        }

        private static class AttributeDefault
        {
            public const string FamilyHeaderLavaTemplate = @"
<h4 class='margin-t-none'>{{ Family.Name }}</h4>
{% if Family.GroupLocations != null -%}
    {% assign groupLocations = Family.GroupLocations -%}
    {% assign locationCount = groupLocations | Size -%}
    {% if locationCount > 0 -%}
        {% for groupLocation in groupLocations -%}
            {% if groupLocation.GroupLocationTypeValue.Value == 'Home' and groupLocation.Location.FormattedHtmlAddress != null and groupLocation.Location.FormattedHtmlAddress != '' -%}
                <div class='rapid-attendance-entry-home-address'>{{ groupLocation.Location.FormattedHtmlAddress }}</div>
            {%- endif %}
        {%- endfor %}
    {%- endif %}
{%- endif %}";

            public const string IndividualHeaderLavaTemplate = @"
<div class='row'>
    <div class='col-md-6 rapid-attendance-entry-person-details'>
        <div class='d-flex align-items-center margin-b-sm'>
            <h5 class='margin-t-none margin-b-none'>{{ Person.FullName }}</h5>
            {% if Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == 'Inactive' -%}
                <span class='label label-danger margin-l-sm' title='{{ Person.RecordStatusReasonValue.Value }}' data-toggle='tooltip'>{{ Person.RecordStatusValue.Value }}</span>
            {%- elseif Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == 'Pending' -%}
                <span class='label label-warning margin-l-sm' title='{{ Person.RecordStatusReasonValue.Value }}' data-toggle='tooltip'>{{ Person.RecordStatusValue.Value }}</span>
            {%- endif %}
        </div>
        {% if Person.Age != null and Person.Age != '' -%}
            {{ Person.Age }} yrs old ({{ Person.BirthDate | Date:'sd' }})<br>
        {%- endif -%}
        {% if Person.Email != '' -%}
            <a href='mailto:{{ Person.Email }}'>{{ Person.Email }}</a>
        {%- endif -%}
    </div>
    <div class='col-md-6 rapid-attendance-entry-phone-numbers'>
        {% for phone in Person.PhoneNumbers -%}
            {% if phone.NumberTypeValue.IsActive == true -%}
                {% if phone.IsUnlisted != true -%}
                    <a href='tel:{{ phone.NumberFormatted }}'>{{ phone.NumberFormatted }}</a>
                {%- else -%}
                    Unlisted
                {%- endif %}
                <small>({{ phone.NumberTypeValue.Value }})</small><br>
            {%- endif %}
        {%- endfor %}
    </div>
</div>";
        }

        private static class PageParameterKey
        {
            // Inbound
            public const string PersonId = "PersonId";

            // Outbound
            public const string GroupId = "GroupId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
            public const string AttendanceDate = "AttendanceDate";
        }

        private static class PersonPreferenceKey
        {
            public const string Campus = "campus";
            public const string Group = "group";
            public const string Location = "location";
            public const string Schedule = "schedule";
            public const string AttendanceDate = "attendance-date";
        }

        private static class NavigationUrlKey
        {
            public const string AddFamilyPage = "AddFamilyPage";
        }

        private static class ListSource
        {
            public const string HideOptionalRequired = "Hide,Optional,Required";
        }

        #endregion Keys & Constants

        #region Properties

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets the campus type defined value unique identifiers that limit which campuses the campus picker offers.
        /// </summary>
        private List<Guid> CampusTypeFilter => GetAttributeValue( AttributeKey.CampusTypes )
            .SplitDelimitedValues( true )
            .AsGuidList();

        /// <summary>
        /// Gets the campus status defined value unique identifiers that limit which campuses the campus picker offers.
        /// </summary>
        private List<Guid> CampusStatusFilter => GetAttributeValue( AttributeKey.CampusStatuses )
            .SplitDelimitedValues( true )
            .AsGuidList();

        /// <summary>
        /// Gets the unique identifier of the campus remembered from the individual's last session, or null when none
        /// was saved. The preference holds the campus picker's selection serialized as ListItemBag JSON.
        /// </summary>
        private Guid? CampusPreferenceGuid => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.Campus )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the unique identifier of the group remembered from the individual's last session, or null when none
        /// was saved. The preference holds the group selection serialized as ListItemBag JSON.
        /// </summary>
        private Guid? GroupPreferenceGuid => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.Group )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the unique identifier of the named location remembered from the individual's last session, or null
        /// when none was saved.
        /// </summary>
        private Guid? LocationPreferenceGuid => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.Location )
            .AsGuidOrNull();

        /// <summary>
        /// Gets the unique identifier of the schedule remembered from the individual's last session, or null when
        /// none was saved.
        /// </summary>
        private Guid? SchedulePreferenceGuid => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.Schedule )
            .AsGuidOrNull();

        /// <summary>
        /// Gets the attendance date remembered from the individual's last session, or null when none was saved.
        /// </summary>
        private DateTime? AttendanceDatePreference => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.AttendanceDate )
            .AsDateTime();

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            ClearInvalidCampusPreference();

            var box = new RapidAttendanceEntryInitializationBox
            {
                IsAttendanceEnabled = GetAttributeValue( AttributeKey.EnableAttendance ).AsBoolean(),
                IsCampusPickerVisible = GetAttributeValue( AttributeKey.ShowCampus ).AsBoolean(),
                CampusTypeFilter = CampusTypeFilter,
                CampusStatusFilter = CampusStatusFilter,
                AttendanceGroup = GetAttendanceGroupBag(),
                GroupItems = GetGroupItems(),
                ActiveSession = GetResumedSession(),
                MinimumAttendanceAge = GetAttributeValue( AttributeKey.AttendanceAgeLimit ).AsInteger(),
                WorkflowListTitle = GetAttributeValue( AttributeKey.WorkflowListTitle ),
                WorkflowItems = GetWorkflowItems(),
                NoteTypeItems = GetNoteTypeItems(),
                ConnectionOpportunitiesListTitle = GetAttributeValue( AttributeKey.ConnectionOpportunitiesListTitle ),
                PrayerOptions = GetPrayerOptions(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Gets the locations available for the specified group, optionally limited to locations at the specified
        /// campus.
        /// </summary>
        /// <param name="groupGuid">The unique identifier of the selected group.</param>
        /// <param name="campusGuid">The unique identifier of the selected campus, or null for all campuses.</param>
        /// <returns>
        /// The matching locations keyed by location unique identifier, and whether the group has only one location
        /// overall.
        /// </returns>
        [BlockAction]
        public BlockActionResult GetLocations( Guid groupGuid, Guid? campusGuid )
        {
            var group = GroupCache.Get( groupGuid );

            if ( group == null )
            {
                return ActionNotFound( "The selected group no longer exists." );
            }

            int? campusId = null;

            if ( campusGuid.HasValue )
            {
                campusId = CampusCache.GetId( campusGuid.Value );
            }

            // Only the group's id list is queried; the group locations and their locations resolve through the cache.
            var groupLocationIds = new GroupLocationService( RockContext )
                .Queryable()
                .Where( gl => gl.GroupId == group.Id )
                .Select( gl => gl.Id )
                .ToList();

            // The campus filter must run in memory: a location's CampusId is derived from its ancestor locations,
            // not a mapped column.
            var locationItems = GroupLocationCache.GetMany( groupLocationIds, RockContext )
                .Select( gl => gl.Location )
                .Where( l => l != null && ( !campusId.HasValue || l.CampusId == campusId.Value ) )
                .DistinctBy( l => l.Id )
                .OrderBy( l => l.Name )
                .Select( l => new ListItemBag
                {
                    Value = l.Guid.ToString(),
                    Text = l.Name
                } )
                .ToList();

            return ActionOk( new RapidAttendanceEntryLocationsBag
            {
                Items = locationItems,
                TotalLocationCount = groupLocationIds.Count
            } );
        }

        /// <summary>
        /// Gets the active schedules available for the specified group and location.
        /// </summary>
        /// <param name="groupGuid">The unique identifier of the selected group.</param>
        /// <param name="locationGuid">The unique identifier of the selected location.</param>
        /// <returns>The matching schedules as list items keyed by schedule unique identifier.</returns>
        [BlockAction]
        public BlockActionResult GetSchedules( Guid groupGuid, Guid locationGuid )
        {
            var group = GroupCache.Get( groupGuid );
            var location = NamedLocationCache.Get( locationGuid );

            if ( group == null || location == null )
            {
                return ActionNotFound( "The selected group or location no longer exists." );
            }

            var activeSchedules = GroupLocationCache.AllForLocationId( location.Id, RockContext )
                .Where( gl => gl.GroupId == group.Id )
                .SelectMany( gl => gl.Schedules )
                .Where( s => s.IsActive )
                .DistinctBy( s => s.Id )
                .ToList();

            var scheduleItems = activeSchedules
                .Select( s => new ListItemBag
                {
                    Value = s.Guid.ToString(),
                    Text = s.Name
                } )
                .ToList();

            // The group's own schedule is offered alongside the group location schedules.
            var groupSchedule = group.Schedule;

            if ( groupSchedule != null && groupSchedule.IsActive && !activeSchedules.Any( s => s.Id == groupSchedule.Id ) )
            {
                scheduleItems.Add( new ListItemBag
                {
                    Value = groupSchedule.Guid.ToString(),
                    Text = groupSchedule.Name
                } );
            }

            return ActionOk( scheduleItems.OrderBy( s => s.Text ).ToList() );
        }

        /// <summary>
        /// Validates the selected attendance settings and starts the entry session.
        /// </summary>
        /// <param name="groupGuid">The unique identifier of the selected group.</param>
        /// <param name="locationGuid">The unique identifier of the selected location.</param>
        /// <param name="scheduleGuid">The unique identifier of the selected schedule.</param>
        /// <param name="attendanceDate">The date attendance will be recorded for.</param>
        /// <returns>The validated session the entry screen operates under.</returns>
        [BlockAction]
        public BlockActionResult StartSession( Guid groupGuid, Guid locationGuid, Guid scheduleGuid, DateTime attendanceDate )
        {
            var session = GetValidatedSession( groupGuid, locationGuid, scheduleGuid, attendanceDate );

            if ( session == null )
            {
                return ActionBadRequest( "The selected group, location, and schedule are no longer a valid combination. Review the selections and try again." );
            }

            return ActionOk( session );
        }

        /// <summary>
        /// Searches for people by name and returns the matches with the family context shown on the search sidebar's
        /// result cards.
        /// </summary>
        /// <param name="searchTerm">The full or partial name to search for.</param>
        /// <param name="session">
        /// The active session, used to flag results already attended for its occurrence. Null when attendance is not
        /// being taken.
        /// </param>
        /// <returns>Up to 25 matches in search rank order.</returns>
        [BlockAction]
        public BlockActionResult SearchPeople( string searchTerm, RapidAttendanceEntrySessionBag session )
        {
            if ( searchTerm.IsNullOrWhiteSpace() )
            {
                return ActionOk( new List<RapidAttendanceEntrySearchResultBag>() );
            }

            var inactiveRecordStatusId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;
            var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME ).Id;
            var mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

            // A campus label only means something when more than one exists.
            var isCampusNameShown = CampusCache.All( false ).Count > 1;

            var people = new PersonService( RockContext )
                .GetByFullNameOrdered( searchTerm.Trim(), false, false, false, out _ )
                .Where( p => p.PrimaryFamilyId.HasValue )
                .Take( 25 )
                .ToList();

            if ( !people.Any() )
            {
                return ActionOk( new List<RapidAttendanceEntrySearchResultBag>() );
            }

            var personIds = people.Select( p => p.Id ).ToList();
            var familyIds = people.Select( p => p.PrimaryFamilyId.Value ).Distinct().ToList();

            // The session's occurrence flags which results are already attended (the saved snapshot shown on the cards).
            HashSet<int> attendedPersonIds = null;

            if ( session != null )
            {
                var sessionGroup = GroupCache.Get( session.GroupGuid );
                var sessionLocation = NamedLocationCache.Get( session.LocationGuid );
                var sessionSchedule = NamedScheduleCache.Get( session.ScheduleGuid );

                if ( sessionGroup != null && sessionLocation != null && sessionSchedule != null )
                {
                    attendedPersonIds = new HashSet<int>( GetAttendedPersonIds( sessionGroup.Id, sessionLocation.Id, sessionSchedule.Id, session.AttendanceDate.Date ) );
                }
            }

            // Batch the per-card data by family and person so the loop below never touches the database.
            var familiesById = new GroupService( RockContext )
                .Queryable()
                .Where( g => familyIds.Contains( g.Id ) )
                .Select( g => new { g.Id, g.Guid, g.Name, g.CampusId } )
                .ToDictionary( g => g.Id );

            var homeAddressHtmlByFamilyId = new GroupLocationService( RockContext )
                .Queryable()
                .Where( gl => familyIds.Contains( gl.GroupId ) && gl.GroupLocationTypeValueId == homeLocationTypeId )
                .Select( gl => new { gl.GroupId, gl.Location } )
                .ToList()
                .GroupBy( gl => gl.GroupId )
                .ToDictionary( g => g.Key, g => g.First().Location?.FormattedHtmlAddress );

            var mobilePhoneByPersonId = new PhoneNumberService( RockContext )
                .Queryable()
                .Where( ph => personIds.Contains( ph.PersonId ) && ph.NumberTypeValueId == mobilePhoneTypeId )
                .ToList()
                .GroupBy( ph => ph.PersonId )
                .ToDictionary( g => g.Key, g => g.First().NumberFormatted );

            // Family member nicknames by family for the card list: non-deceased members ordered by role, birth date, then gender.
            var familyMemberNamesByFamilyId = new GroupMemberService( RockContext )
                .Queryable( "Person", true )
                .Where( m => familyIds.Contains( m.GroupId ) && !m.Person.IsDeceased )
                .OrderBy( m => m.GroupRole.Order )
                .ThenBy( m => m.Person.BirthDate ?? DateTime.MinValue )
                .ThenByDescending( m => m.Person.Gender )
                .Select( m => new { m.GroupId, m.Person.NickName } )
                .ToList()
                .GroupBy( m => m.GroupId )
                .ToDictionary( g => g.Key, g => g.Select( m => m.NickName ).ToList() );

            var results = new List<RapidAttendanceEntrySearchResultBag>();

            foreach ( var person in people )
            {
                var familyId = person.PrimaryFamilyId.Value;

                if ( !familiesById.TryGetValue( familyId, out var family ) )
                {
                    continue;
                }

                familyMemberNamesByFamilyId.TryGetValue( familyId, out var familyMemberNames );
                homeAddressHtmlByFamilyId.TryGetValue( familyId, out var addressHtml );
                mobilePhoneByPersonId.TryGetValue( person.Id, out var mobilePhone );

                results.Add( new RapidAttendanceEntrySearchResultBag
                {
                    PersonGuid = person.Guid,
                    FamilyGuid = family.Guid,
                    Name = person.NickName + " " + person.LastName,
                    Age = person.Age,
                    FamilyName = family.Name,
                    FamilyMemberNames = ( familyMemberNames ?? new List<string>() ).AsDelimited( ", ", " & " ),
                    CampusName = isCampusNameShown && family.CampusId.HasValue ? CampusCache.Get( family.CampusId.Value )?.Name : null,
                    Email = person.Email,
                    MobilePhone = mobilePhone,
                    AddressHtml = addressHtml,
                    IsActive = person.RecordStatusValueId != inactiveRecordStatusId,
                    IsAttended = attendedPersonIds != null && attendedPersonIds.Contains( person.Id )
                } );
            }

            return ActionOk( results );
        }

        /// <summary>
        /// Gets the family shown in the main entry pane: the rendered family header, the family members, and, when
        /// a session is supplied, the can check-in guests and current attendance state.
        /// </summary>
        /// <param name="familyGuid">The unique identifier of the family group.</param>
        /// <param name="session">The active session, or null when attendance is not being taken.</param>
        /// <returns>The family.</returns>
        [BlockAction]
        public BlockActionResult GetFamily( Guid familyGuid, RapidAttendanceEntrySessionBag session )
        {
            var family = new GroupService( RockContext ).Get( familyGuid );
            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;

            if ( family == null || family.GroupTypeId != familyGroupTypeId )
            {
                return ActionNotFound( "The family no longer exists." );
            }

            // includeSelf bypasses the excluded-person filter, so 0 stands in for it and every member is returned.
            var members = new PersonService( RockContext )
                .GetFamilyMembers( family, 0, includeSelf: true )
                .Select( m => m.Person )
                .ToList();

            List<int> attendedPersonIds = null;
            int? sessionCampusId = null;

            if ( session != null )
            {
                var group = GroupCache.Get( session.GroupGuid );
                var location = NamedLocationCache.Get( session.LocationGuid );
                var schedule = NamedScheduleCache.Get( session.ScheduleGuid );

                if ( group != null && location != null && schedule != null )
                {
                    attendedPersonIds = GetAttendedPersonIds( group.Id, location.Id, schedule.Id, session.AttendanceDate.Date );
                }

                sessionCampusId = group?.CampusId ?? location?.CampusId;
            }

            List<Person> guests = null;

            if ( session != null && GetAttributeValue( AttributeKey.ShowCanCheckInRelationships ).AsBoolean() )
            {
                guests = GetCanCheckInGuests( members );
            }

            var minimumAttendanceAge = GetAttributeValue( AttributeKey.AttendanceAgeLimit ).AsInteger();
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Family", family );

            // Offer opportunities for the session campus, falling back to the family's, matching the campus a saved
            // request would receive.
            var connectionOpportunityItems = GetConnectionOpportunityItems( sessionCampusId ?? family.CampusId );

            return ActionOk( new RapidAttendanceEntryFamilyBag
            {
                FamilyGuid = family.Guid,
                FamilyName = family.Name,
                HeaderHtml = GetAttributeValue( AttributeKey.FamilyHeaderLavaTemplate ).ResolveMergeFields( mergeFields ),
                Members = members.Select( p => GetPersonBag( p, minimumAttendanceAge, attendedPersonIds ) ).ToList(),
                CanCheckInGuests = guests?.Select( p => GetPersonBag( p, minimumAttendanceAge, attendedPersonIds ) ).ToList(),
                AttendanceCount = attendedPersonIds?.Count,
                ConnectionOpportunityItems = connectionOpportunityItems
            } );
        }

        /// <summary>
        /// Gets the editable family information shown in the Edit Family modal.
        /// </summary>
        /// <param name="familyGuid">The unique identifier of the family to edit.</param>
        /// <returns>The family's home address, location flags, and configured family attributes.</returns>
        [BlockAction]
        public BlockActionResult GetFamilyForEdit( Guid familyGuid )
        {
            var family = new GroupService( RockContext ).Get( familyGuid );
            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;

            if ( family == null || family.GroupTypeId != familyGroupTypeId )
            {
                return ActionNotFound( "The family no longer exists." );
            }

            var bag = new RapidAttendanceEntryEditFamilyBag
            {
                FamilyGuid = family.Guid,
                IsMailingLocation = true,
                IsPhysicalLocation = true
            };

            var homeAddressTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            var homeLocation = new GroupLocationService( RockContext )
                .Queryable()
                .FirstOrDefault( gl =>
                    gl.GroupId == family.Id
                    && gl.GroupLocationTypeValueId == homeAddressTypeId
                );

            if ( homeLocation?.Location != null )
            {
                bag.Address = new AddressControlBag
                {
                    Street1 = homeLocation.Location.Street1,
                    Street2 = homeLocation.Location.Street2,
                    City = homeLocation.Location.City,
                    State = homeLocation.Location.State,
                    PostalCode = homeLocation.Location.PostalCode,
                    Country = homeLocation.Location.Country
                };
                bag.IsMailingLocation = homeLocation.IsMailingLocation;
                bag.IsPhysicalLocation = homeLocation.IsMappedLocation;
                bag.AddressFormatted = homeLocation.Location.FormattedHtmlAddress;
            }

            var familyAttributeGuids = GetAttributeValue( AttributeKey.FamilyAttributes )
                .SplitDelimitedValues()
                .AsGuidList();

            family.LoadAttributes( RockContext );

            bag.Attributes = family
                .GetPublicAttributesForEdit(
                    RequestContext.CurrentPerson,
                    enforceSecurity: true,
                    attributeFilter: a => familyAttributeGuids.Contains( a.Guid )
                );

            bag.AttributeValues = family
                .GetPublicAttributeValuesForEdit(
                    RequestContext.CurrentPerson,
                    enforceSecurity: true,
                    attributeFilter: a => familyAttributeGuids.Contains( a.Guid )
                );

            return ActionOk( bag );
        }

        /// <summary>
        /// Saves the family's home address, location flags, and family attributes from the Edit Family modal.
        /// </summary>
        /// <param name="bag">The edited family information.</param>
        /// <returns>An empty result on success.</returns>
        [BlockAction]
        public BlockActionResult SaveFamily( RapidAttendanceEntryEditFamilyBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "No family information was supplied." );
            }

            var family = new GroupService( RockContext ).Get( bag.FamilyGuid );
            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;

            if ( family == null || family.GroupTypeId != familyGroupTypeId )
            {
                return ActionNotFound( "The family no longer exists." );
            }

            var groupLocationService = new GroupLocationService( RockContext );
            var homeAddressTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            var homeLocation = groupLocationService
                .Queryable()
                .FirstOrDefault( gl =>
                    gl.GroupId == family.Id
                    && gl.GroupLocationTypeValueId == homeAddressTypeId
                );

            var hasAddress = bag.Address != null && bag.Address.Street1.IsNotNullOrWhiteSpace();

            if ( homeLocation != null && !hasAddress )
            {
                // The address was cleared, so remove it from the family.
                groupLocationService.Delete( homeLocation );
                RockContext.SaveChanges();
            }
            else if ( hasAddress )
            {
                if ( homeLocation == null )
                {
                    homeLocation = new GroupLocation
                    {
                        GroupLocationTypeValueId = homeAddressTypeId,
                        GroupId = family.Id
                    };
                    groupLocationService.Add( homeLocation );
                }
                else if ( bag.PreviousAddress != null && bag.PreviousAddress.Street1.IsNotNullOrWhiteSpace() )
                {
                    // The family moved, so preserve the prior address as a Previous Address.
                    var previousAddressTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() )?.Id;

                    if ( previousAddressTypeId.HasValue )
                    {
                        groupLocationService.Add( new GroupLocation
                        {
                            GroupLocationTypeValueId = previousAddressTypeId,
                            GroupId = family.Id,
                            Location = new Location
                            {
                                Street1 = bag.PreviousAddress.Street1,
                                Street2 = bag.PreviousAddress.Street2,
                                City = bag.PreviousAddress.City,
                                State = bag.PreviousAddress.State,
                                PostalCode = bag.PreviousAddress.PostalCode,
                                Country = bag.PreviousAddress.Country
                            }
                        } );
                    }
                }

                homeLocation.IsMailingLocation = bag.IsMailingLocation;
                homeLocation.IsMappedLocation = bag.IsPhysicalLocation;
                homeLocation.Location = new LocationService( RockContext ).Get(
                    bag.Address.Street1,
                    bag.Address.Street2,
                    bag.Address.City,
                    bag.Address.State,
                    bag.Address.PostalCode,
                    bag.Address.Country,
                    group: family,
                    verifyLocation: true
                );

                // Only one location can be the mapped location, so clear it on the family's others.
                if ( homeLocation.IsMappedLocation )
                {
                    var otherLocations = groupLocationService
                        .Queryable()
                        .Where( gl =>
                            gl.GroupId == family.Id
                            && gl.Id != homeLocation.Id
                        )
                        .ToList();

                    foreach ( var otherLocation in otherLocations )
                    {
                        otherLocation.IsMappedLocation = false;
                    }
                }

                RockContext.SaveChanges();
            }

            if ( bag.AttributeValues != null )
            {
                family.LoadAttributes( RockContext );
                family.SetPublicAttributeValues( bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                family.SaveAttributeValues( RockContext );
            }

            return ActionOk();
        }

        /// <summary>
        /// Gets the detail shown for the individual selected on the entry screen.
        /// </summary>
        /// <param name="personGuid">The unique identifier of the selected person.</param>
        /// <returns>The rendered personal summary.</returns>
        [BlockAction]
        public BlockActionResult GetPerson( Guid personGuid )
        {
            var person = new PersonService( RockContext ).Get( personGuid );

            if ( person == null )
            {
                return ActionNotFound( "The person no longer exists." );
            }

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Person", person );

            return ActionOk( new RapidAttendanceEntryPersonDetailBag
            {
                HeaderHtml = GetAttributeValue( AttributeKey.IndividualHeaderLavaTemplate ).ResolveMergeFields( mergeFields )
            } );
        }

        /// <summary>
        /// Gets the editable individual and the role-dependent configuration shown in the Add Person and Edit Person
        /// modals.
        /// </summary>
        /// <param name="personGuid">The unique identifier of the person to edit, or null to add a new family member.</param>
        /// <param name="familyGuid">The unique identifier of the family the person belongs to or is being added to.</param>
        /// <returns>The editable person and the configuration the modal needs to render.</returns>
        [BlockAction]
        public BlockActionResult GetPersonForEdit( Guid? personGuid, Guid familyGuid )
        {
            var family = new GroupService( RockContext ).Get( familyGuid );
            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;

            if ( family == null || family.GroupTypeId != familyGroupTypeId )
            {
                return ActionNotFound( "The family no longer exists." );
            }

            var adultRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var isAdd = !personGuid.HasValue || personGuid.Value == Guid.Empty;
            var roleGuid = adultRoleGuid;
            Person person;

            if ( isAdd )
            {
                person = new Person();
            }
            else
            {
                person = new PersonService( RockContext ).Get( personGuid.Value );

                if ( person == null )
                {
                    return ActionNotFound( "The person no longer exists." );
                }

                var memberRoleGuid = family.Members
                    .Where( m => m.PersonId == person.Id )
                    .Select( m => ( Guid? ) m.GroupRole.Guid )
                    .FirstOrDefault();
                roleGuid = memberRoleGuid ?? adultRoleGuid;
            }

            person.LoadAttributes( RockContext );

            var adultAttributeGuids = GetAttributeValue( AttributeKey.AdultPersonAttributes ).SplitDelimitedValues().AsGuidList();
            var childAttributeGuids = GetAttributeValue( AttributeKey.ChildPersonAttributes ).SplitDelimitedValues().AsGuidList();
            var allAttributeGuids = adultAttributeGuids.Concat( childAttributeGuids ).Distinct().ToList();

            var options = new RapidAttendanceEntryEditPersonOptionsBag
            {
                IsAdd = isAdd,
                AdultRoleGuid = adultRoleGuid,
                RoleItems = GetFamilyRoleItems(),
                AdultPhoneTypes = GetPhoneTypeItems( AttributeKey.AdultPhoneTypes ),
                ChildPhoneTypes = GetPhoneTypeItems( AttributeKey.ChildPhoneTypes ),
                MobilePhoneTypeGuid = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.Guid,
                IsCommunicationPreferenceShown = GetAttributeValue( AttributeKey.ShowCommunicationPreference ).AsBoolean(),
                IsChildEmailEditAllowed = GetAttributeValue( AttributeKey.ChildAllowEmailEdit ).AsBoolean(),
                RaceVisibility = GetAttributeValue( AttributeKey.RaceOption ),
                EthnicityVisibility = GetAttributeValue( AttributeKey.EthnicityOption ),
                AdultAttributes = person.GetPublicAttributesForEdit( RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => adultAttributeGuids.Contains( a.Guid ) ),
                ChildAttributes = person.GetPublicAttributesForEdit( RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => childAttributeGuids.Contains( a.Guid ) ),
                Person = new RapidAttendanceEntryEditPersonBag
                {
                    PersonGuid = isAdd ? ( Guid? ) null : person.Guid,
                    FamilyGuid = family.Guid,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Suffix = DefinedValueCache.Get( person.SuffixValueId ?? 0 ).ToListItemBag(),
                    Gender = person.Gender,
                    BirthDate = new DatePartsPickerValueBag
                    {
                        Year = person.BirthYear ?? 0,
                        Month = person.BirthMonth ?? 0,
                        Day = person.BirthDay ?? 0
                    },
                    RoleGuid = roleGuid,
                    MaritalStatus = DefinedValueCache.Get( person.MaritalStatusValueId ?? 0 ).ToListItemBag(),
                    Grade = person.GradeOffset.HasValue
                        ? new ListItemBag { Text = person.GradeFormatted, Value = person.GradeOffset.Value.ToString() }
                        : null,
                    Race = DefinedValueCache.Get( person.RaceValueId ?? 0 ).ToListItemBag(),
                    Ethnicity = DefinedValueCache.Get( person.EthnicityValueId ?? 0 ).ToListItemBag(),
                    Email = person.Email,
                    IsEmailActive = person.IsEmailActive,
                    CommunicationPreference = ( Enums.Communication.CommunicationType ) person.CommunicationPreference,
                    PhoneNumbers = GetPhoneNumberBags( person ),
                    AttributeValues = person.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => allAttributeGuids.Contains( a.Guid ) )
                }
            };

            if ( isAdd )
            {
                options.FamilyName = family.Name;

                var homeAddressTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
                var homeLocation = new GroupLocationService( RockContext )
                    .Queryable()
                    .FirstOrDefault( gl =>
                        gl.GroupId == family.Id
                        && gl.GroupLocationTypeValueId == homeAddressTypeId
                    );

                options.FamilyAddressFormatted = homeLocation?.Location?.FormattedHtmlAddress;
            }

            return ActionOk( options );
        }

        /// <summary>
        /// Adds a new family member or saves changes to an existing one from the Add Person and Edit Person modals.
        /// </summary>
        /// <param name="bag">The edited individual.</param>
        /// <returns>The unique identifier of the saved person.</returns>
        [BlockAction]
        public BlockActionResult SavePerson( RapidAttendanceEntryEditPersonBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "No individual information was supplied." );
            }

            var family = new GroupService( RockContext ).Get( bag.FamilyGuid );
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

            if ( family == null || family.GroupTypeId != familyGroupType.Id )
            {
                return ActionNotFound( "The family no longer exists." );
            }

            var role = familyGroupType.Roles.FirstOrDefault( r => r.Guid == bag.RoleGuid );

            if ( role == null )
            {
                return ActionBadRequest( "Select a role for the individual." );
            }

            var isAdult = role.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var isEmailShown = isAdult || GetAttributeValue( AttributeKey.ChildAllowEmailEdit ).AsBoolean();

            // Communication preference only matters when a phone field is shown (SMS needs an enterable messaging
            // number). With no phone fields shown it is hidden and left unchanged, so it is neither applied nor
            // validated below; this keeps a person already set to SMS from blocking the save.
            var arePhoneNumbersShown = GetPhoneTypeItems( isAdult ? AttributeKey.AdultPhoneTypes : AttributeKey.ChildPhoneTypes ).Any();
            var isCommunicationPreferenceShown = isAdult
                && GetAttributeValue( AttributeKey.ShowCommunicationPreference ).AsBoolean()
                && arePhoneNumbersShown;

            var personService = new PersonService( RockContext );
            var groupMemberService = new GroupMemberService( RockContext );

            Person person = null;
            string validationMessage = null;

            var saved = RockContext.WrapTransactionIf( () =>
            {
                if ( !bag.PersonGuid.HasValue || bag.PersonGuid.Value == Guid.Empty )
                {
                    var newPerson = new Person
                    {
                        FirstName = bag.FirstName,
                        LastName = bag.LastName,
                        RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() )?.Id
                    };

                    // New adults give as part of the family and inherit the head of household's record status.
                    if ( isAdult )
                    {
                        newPerson.GivingGroupId = family.Id;
                    }

                    var headOfHousehold = GroupServiceExtensions.HeadOfHousehold( family.Members.AsQueryable() );

                    if ( headOfHousehold?.RecordStatusValueId != null )
                    {
                        newPerson.RecordStatusValueId = headOfHousehold.RecordStatusValueId;
                    }

                    groupMemberService.Add( new GroupMember
                    {
                        Person = newPerson,
                        Group = family,
                        GroupId = family.Id,
                        GroupRoleId = role.Id
                    } );

                    RockContext.SaveChanges();
                    person = newPerson;
                }
                else
                {
                    person = personService.Get( bag.PersonGuid.Value );

                    if ( person == null )
                    {
                        validationMessage = "The person no longer exists.";
                        return false;
                    }

                    var groupMember = groupMemberService.Queryable()
                        .FirstOrDefault( m => m.PersonId == person.Id && m.GroupId == family.Id );

                    if ( groupMember != null )
                    {
                        groupMember.GroupRoleId = role.Id;
                    }
                }

                person.FirstName = bag.FirstName;
                person.LastName = bag.LastName;
                person.SuffixValueId = GetDefinedValueId( bag.Suffix?.Value.AsGuidOrNull() );
                person.Gender = bag.Gender;
                person.RaceValueId = GetDefinedValueId( bag.Race?.Value.AsGuidOrNull() );
                person.EthnicityValueId = GetDefinedValueId( bag.Ethnicity?.Value.AsGuidOrNull() );

                ApplyBirthDate( person, bag.BirthDate );

                if ( isAdult )
                {
                    person.MaritalStatusValueId = GetDefinedValueId( bag.MaritalStatus?.Value.AsGuidOrNull() );
                }
                else
                {
                    person.GradeOffset = bag.Grade?.Value.AsIntegerOrNull();
                }

                if ( isEmailShown )
                {
                    person.Email = bag.Email?.Trim();
                    person.IsEmailActive = bag.IsEmailActive;
                }

                if ( isCommunicationPreferenceShown )
                {
                    person.CommunicationPreference = ( Model.CommunicationType ) bag.CommunicationPreference;
                }

                var keptPhoneTypeIds = ApplyPhoneNumbers( person, bag.PhoneNumbers, isAdult );
                personService.RemoveEmptyAndDuplicatePhoneNumbers( person, keptPhoneTypeIds, RockContext );

                // An individual can only prefer SMS when a number they can edit has messaging enabled.
                if ( isCommunicationPreferenceShown
                    && bag.CommunicationPreference == Enums.Communication.CommunicationType.SMS
                    && !person.PhoneNumbers.Any( pn => pn.IsMessagingEnabled ) )
                {
                    validationMessage = "A phone number with SMS enabled is required when Communication Preference is set to SMS.";
                    return false;
                }

                RockContext.SaveChanges();

                person.LoadAttributes( RockContext );
                person.SetPublicAttributeValues( FilterAttributeValues( bag.AttributeValues, isAdult ), RequestContext.CurrentPerson, enforceSecurity: true );
                person.SaveAttributeValues( RockContext );

                return true;
            } );

            if ( !saved )
            {
                return ActionBadRequest( validationMessage ?? "Unable to save the individual." );
            }

            return ActionOk( person.Guid );
        }

        /// <summary>
        /// Saves everything the operator entered: the attendance roster and each individual's prayer request, note,
        /// workflows, and connection requests.
        /// </summary>
        /// <param name="bag">The entries to save.</param>
        /// <returns>The updated attendance count for the session's occurrence.</returns>
        [BlockAction]
        public BlockActionResult Save( RapidAttendanceEntrySaveBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "No entries were supplied." );
            }

            GroupCache group = null;
            NamedLocationCache location = null;
            NamedScheduleCache schedule = null;
            RapidAttendanceEntrySessionBag session = null;

            if ( GetAttributeValue( AttributeKey.EnableAttendance ).AsBoolean() && bag.Session != null )
            {
                session = GetValidatedSession( bag.Session.GroupGuid, bag.Session.LocationGuid, bag.Session.ScheduleGuid, bag.Session.AttendanceDate );

                if ( session == null )
                {
                    return ActionBadRequest( "The session is no longer valid. Return to setup and start a new session." );
                }

                group = GroupCache.Get( session.GroupGuid );
                location = NamedLocationCache.Get( session.LocationGuid );
                schedule = NamedScheduleCache.Get( session.ScheduleGuid );
            }

            // Resolve every person referenced by the roster and the per-person inputs in one query.
            var personGuids = ( bag.Attendances?.Select( a => a.PersonGuid ) ?? Enumerable.Empty<Guid>() )
                .Concat( bag.PersonInputs?.Select( pi => pi.PersonGuid ) ?? Enumerable.Empty<Guid>() )
                .Distinct()
                .ToList();

            /*
                6/15/26 - JPH

                PrimaryCampus is intentionally not eager-loaded: GetCampus() reads it only on the campus fallback
                (group has no campus), so an include would burden every save for a rare case and still leave a lazy
                load, since GetCampus() then falls back to the family's campus.

                Reason: Eager-loading PrimaryCampus costs every save to help a rare fallback it can't fully fix.
            */
            var peopleByGuid = new PersonService( RockContext )
                .Queryable( "Aliases" )
                .Where( p => personGuids.Contains( p.Guid ) )
                .ToDictionary( p => p.Guid );

            if ( session != null && bag.Attendances != null )
            {
                SaveAttendances( bag.Attendances, peopleByGuid, group, location, schedule, session.AttendanceDate );
            }

            foreach ( var personInput in bag.PersonInputs ?? new List<RapidAttendanceEntryPersonInputBag>() )
            {
                if ( !peopleByGuid.TryGetValue( personInput.PersonGuid, out var person ) )
                {
                    continue;
                }

                AddPrayerRequest( person, personInput.PrayerRequest, group );
                AddPersonNote( person, personInput.Note );
                LaunchWorkflows( person, personInput.WorkflowTypeGuids, group, location, schedule, session?.AttendanceDate );
                CreateConnectionRequests( person, personInput.ConnectionOpportunityGuids, group );
            }

            RockContext.SaveChanges();

            int? attendanceCount = null;

            if ( session != null )
            {
                // Kiosks read this cache for live location counts.
                Rock.CheckIn.KioskLocationAttendance.Remove( location.Id );

                attendanceCount = GetAttendedPersonIds( group.Id, location.Id, schedule.Id, session.AttendanceDate ).Count;
            }

            return ActionOk( new RapidAttendanceEntrySaveResponseBag
            {
                AttendanceCount = attendanceCount
            } );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the group the block is locked to by the Attendance Group block setting, or null when the setting is
        /// not configured.
        /// </summary>
        /// <returns>The locked group as a list item, or null.</returns>
        private ListItemBag GetAttendanceGroupBag()
        {
            var attendanceGroupGuid = GetAttributeValue( AttributeKey.AttendanceGroup ).AsGuidOrNull();

            if ( !attendanceGroupGuid.HasValue )
            {
                return null;
            }

            var attendanceGroup = GroupCache.Get( attendanceGroupGuid.Value );

            if ( attendanceGroup == null )
            {
                return null;
            }

            return new ListItemBag
            {
                Value = attendanceGroup.Guid.ToString(),
                Text = attendanceGroup.Name
            };
        }

        /// <summary>
        /// Gets the selectable groups: the active children of the configured Parent Group. Returns null when the
        /// block is locked to an Attendance Group or no Parent Group is configured, since no group list applies in
        /// either mode.
        /// </summary>
        /// <returns>The selectable groups as list items, or null.</returns>
        private List<ListItemBag> GetGroupItems()
        {
            if ( GetAttributeValue( AttributeKey.AttendanceGroup ).AsGuidOrNull().HasValue )
            {
                return null;
            }

            var parentGroupGuid = GetAttributeValue( AttributeKey.ParentGroup ).AsGuidOrNull();

            if ( !parentGroupGuid.HasValue )
            {
                return null;
            }

            var parentGroup = GroupCache.Get( parentGroupGuid.Value );

            if ( parentGroup == null )
            {
                return null;
            }

            return new GroupService( RockContext )
                .Queryable()
                .Where( g => g.ParentGroupId == parentGroup.Id && g.IsActive )
                .OrderBy( g => g.Order )
                .ThenBy( g => g.Name )
                .Select( g => new { g.Guid, g.Name } )
                .ToList()
                .Select( g => new ListItemBag
                {
                    Value = g.Guid.ToString(),
                    Text = g.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Clears the remembered campus preference when the campus no longer exists, is inactive, or is excluded by
        /// the Campus Types and Campus Statuses settings. The campus picker keeps the current selection in its list
        /// even when filtered out, so a stale preference must not be restored as the selection.
        /// </summary>
        private void ClearInvalidCampusPreference()
        {
            var campusGuid = CampusPreferenceGuid;

            if ( !campusGuid.HasValue )
            {
                return;
            }

            var campus = CampusCache.Get( campusGuid.Value );
            var isOffered = campus != null && campus.IsActive == true;

            if ( isOffered && CampusTypeFilter.Any() )
            {
                var campusTypeIds = CampusTypeFilter
                    .Select( DefinedValueCache.GetId )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList();

                isOffered = campus.CampusTypeValueId.HasValue && campusTypeIds.Contains( campus.CampusTypeValueId.Value );
            }

            if ( isOffered && CampusStatusFilter.Any() )
            {
                var campusStatusIds = CampusStatusFilter
                    .Select( DefinedValueCache.GetId )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList();

                isOffered = campus.CampusStatusValueId.HasValue && campusStatusIds.Contains( campus.CampusStatusValueId.Value );
            }

            if ( !isOffered )
            {
                BlockPersonPreferences.SetValue( PersonPreferenceKey.Campus, string.Empty );
                BlockPersonPreferences.Save();
            }
        }

        /// <summary>
        /// Gets the session remembered from the individual's last visit when it still validates and its attendance
        /// date is today; otherwise null so the individual confirms the setup before entry begins.
        /// </summary>
        /// <returns>The validated session, or null.</returns>
        private RapidAttendanceEntrySessionBag GetResumedSession()
        {
            if ( !GetAttributeValue( AttributeKey.EnableAttendance ).AsBoolean() )
            {
                return null;
            }

            if ( AttendanceDatePreference?.Date != RockDateTime.Today )
            {
                return null;
            }

            return GetValidatedSession( GroupPreferenceGuid, LocationPreferenceGuid, SchedulePreferenceGuid, AttendanceDatePreference );
        }

        /// <summary>
        /// Gets the session the supplied selections describe when they are complete and still valid: the schedule is
        /// active, the location belongs to the group, and the group satisfies the Attendance Group or Parent Group
        /// constraint. Returns null otherwise.
        /// </summary>
        /// <param name="groupGuid">The unique identifier of the selected group.</param>
        /// <param name="locationGuid">The unique identifier of the selected location.</param>
        /// <param name="scheduleGuid">The unique identifier of the selected schedule.</param>
        /// <param name="attendanceDate">The date attendance will be recorded for.</param>
        /// <returns>The validated session, or null.</returns>
        private RapidAttendanceEntrySessionBag GetValidatedSession( Guid? groupGuid, Guid? locationGuid, Guid? scheduleGuid, DateTime? attendanceDate )
        {
            if ( !groupGuid.HasValue || !locationGuid.HasValue || !scheduleGuid.HasValue || !attendanceDate.HasValue )
            {
                return null;
            }

            var group = GroupCache.Get( groupGuid.Value );
            var location = NamedLocationCache.Get( locationGuid.Value );
            var schedule = NamedScheduleCache.Get( scheduleGuid.Value );

            if ( group == null || location == null || schedule == null || !schedule.IsActive )
            {
                return null;
            }

            if ( !IsGroupAllowed( group ) )
            {
                return null;
            }

            var groupLocation = GroupLocationCache
                .AllForLocationId( location.Id, RockContext )
                .FirstOrDefault( gl => gl.GroupId == group.Id );

            if ( groupLocation == null )
            {
                return null;
            }

            return new RapidAttendanceEntrySessionBag
            {
                GroupGuid = group.Guid,
                GroupName = group.Name,
                LocationGuid = location.Guid,
                LocationName = location.Name,
                ScheduleGuid = schedule.Guid,
                ScheduleName = schedule.Name,
                AttendanceDate = attendanceDate.Value.Date,
                AttendanceCount = GetAttendedPersonIds( group.Id, location.Id, schedule.Id, attendanceDate.Value.Date ).Count,
                AttendanceListUrl = GetAttendanceListUrl( group, groupLocation, schedule, attendanceDate.Value.Date )
            };
        }

        /// <summary>
        /// Determines whether the group satisfies the block's group constraint: the Attendance Group when one is
        /// configured, otherwise an active child of the Parent Group when one is configured, otherwise any group.
        /// </summary>
        /// <param name="group">The group to check.</param>
        /// <returns>True when the group may be used for the session.</returns>
        private bool IsGroupAllowed( GroupCache group )
        {
            var attendanceGroupGuid = GetAttributeValue( AttributeKey.AttendanceGroup ).AsGuidOrNull();

            if ( attendanceGroupGuid.HasValue )
            {
                return group.Guid == attendanceGroupGuid.Value;
            }

            var parentGroupGuid = GetAttributeValue( AttributeKey.ParentGroup ).AsGuidOrNull();

            if ( parentGroupGuid.HasValue )
            {
                var parentGroup = GroupCache.Get( parentGroupGuid.Value );

                return parentGroup != null
                    && group.ParentGroupId == parentGroup.Id
                    && group.IsActive;
            }

            return true;
        }

        /// <summary>
        /// Gets the URLs the client navigates to from the block.
        /// </summary>
        /// <returns>The URLs keyed by the NavigationUrlKey constants.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.AddFamilyPage] = this.GetLinkedPageUrl( AttributeKey.AddFamilyPage )
            };
        }

        /// <summary>
        /// Gets the workflows offered as checkboxes on each individual's entry panel, from the Workflow Types block
        /// setting, in workflow type order. Returns null when none are configured.
        /// </summary>
        /// <returns>The workflows as list items keyed by workflow type unique identifier, or null.</returns>
        private List<ListItemBag> GetWorkflowItems()
        {
            var items = GetAttributeValue( AttributeKey.WorkflowTypes )
                .SplitDelimitedValues()
                .AsGuidList()
                .Select( WorkflowTypeCache.Get )
                .Where( t => t != null )
                .OrderBy( t => t.Order )
                .ToListItemBagList();

            return items.Any() ? items : null;
        }

        /// <summary>
        /// Gets the note types available in the Note section, from the Note Types block setting, in note type
        /// order. Returns null when none are configured.
        /// </summary>
        /// <returns>The note types as list items keyed by note type unique identifier, or null.</returns>
        private List<ListItemBag> GetNoteTypeItems()
        {
            var items = GetAttributeValue( AttributeKey.NoteTypes )
                .SplitDelimitedValues()
                .AsGuidList()
                .Select( NoteTypeCache.Get )
                .Where( t => t != null )
                .OrderBy( t => t.Order )
                .ToListItemBagList();

            return items.Any() ? items : null;
        }

        /// <summary>
        /// Gets the active connection opportunities of the configured Connection Type that the operator is
        /// authorized to see and that are available for the given campus, as the entry panel's checkbox list.
        /// Returns null when no type is configured, the operator cannot view the type, or nothing qualifies.
        /// </summary>
        /// <param name="campusId">The campus the opportunities must be available for; an opportunity with no campus
        /// restriction always qualifies. Null offers only unrestricted opportunities.</param>
        /// <returns>The opportunities as list items keyed by opportunity unique identifier, or null.</returns>
        private List<ListItemBag> GetConnectionOpportunityItems( int? campusId )
        {
            var connectionTypeGuid = GetAttributeValue( AttributeKey.ConnectionType ).AsGuidOrNull();

            if ( !connectionTypeGuid.HasValue )
            {
                return null;
            }

            var connectionType = ConnectionTypeCache.Get( connectionTypeGuid.Value );
            var currentPerson = RequestContext.CurrentPerson;

            // Hide the section when the operator cannot view the connection type.
            if ( connectionType == null
                || !( connectionType.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson )
                    || connectionType.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson ) ) )
            {
                return null;
            }

            var opportunities = new ConnectionOpportunityService( RockContext )
                .Queryable( "ConnectionType,ConnectionOpportunityCampuses" )
                .Where( o => o.ConnectionTypeId == connectionType.Id && o.IsActive )
                .OrderBy( o => o.Order )
                .ThenBy( o => o.Name )
                .ToList();

            var items = opportunities
                .Where( o => o.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson )
                    || o.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson ) )
                .Where( o => !o.ConnectionOpportunityCampuses.Any()
                    || ( campusId.HasValue && o.ConnectionOpportunityCampuses.Any( oc => oc.CampusId == campusId.Value ) ) )
                .ToListItemBagList();

            return items.Any() ? items : null;
        }

        /// <summary>
        /// Gets the configuration for the prayer request section. Returns null when prayer request entry is
        /// disabled.
        /// </summary>
        /// <returns>The prayer request options, or null.</returns>
        private RapidAttendanceEntryPrayerOptionsBag GetPrayerOptions()
        {
            if ( !GetAttributeValue( AttributeKey.EnablePrayerRequestEntry ).AsBoolean() )
            {
                return null;
            }

            ListItemBag defaultCategory = null;
            var defaultCategoryGuid = GetAttributeValue( AttributeKey.DefaultCategory ).AsGuidOrNull();

            if ( defaultCategoryGuid.HasValue )
            {
                var category = CategoryCache.Get( defaultCategoryGuid.Value );

                if ( category != null )
                {
                    defaultCategory = new ListItemBag
                    {
                        Value = category.Guid.ToString(),
                        Text = category.Name
                    };
                }
            }

            return new RapidAttendanceEntryPrayerOptionsBag
            {
                IsUrgentFlagShown = GetAttributeValue( AttributeKey.ShowUrgentFlag ).AsBoolean(),
                IsPublicFlagShown = GetAttributeValue( AttributeKey.ShowPublicFlag ).AsBoolean(),
                IsPublicByDefault = GetAttributeValue( AttributeKey.DisplayToPublic ).AsBoolean(),
                IsCategoryPickerShown = GetAttributeValue( AttributeKey.EnableCategorySelection ).AsBoolean(),
                DefaultCategory = defaultCategory
            };
        }

        /// <summary>
        /// Gets the people outside the family who have a "Can check-in" known relationship with any of the supplied
        /// family members, ordered by name. They may be listed for attendance alongside the family.
        /// </summary>
        /// <param name="members">The family members whose related people are found.</param>
        /// <returns>The related people.</returns>
        private List<Person> GetCanCheckInGuests( List<Person> members )
        {
            var knownRelationshipsType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            var ownerRole = knownRelationshipsType?.Roles
                .FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() );
            var canCheckInRole = knownRelationshipsType?.Roles
                .FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid() );

            if ( ownerRole == null || canCheckInRole == null )
            {
                return new List<Person>();
            }

            var groupMemberService = new GroupMemberService( RockContext );
            var memberPersonIds = members.Select( p => p.Id ).ToList();

            var memberRelationshipGroupIds = groupMemberService.Queryable()
                .Where( m => m.GroupRoleId == ownerRole.Id && memberPersonIds.Contains( m.PersonId ) )
                .Select( m => m.GroupId );

            // Family members can hold the relationship to each other; they are already listed.
            return groupMemberService.Queryable()
                .Where( m => m.GroupRoleId == canCheckInRole.Id
                    && memberRelationshipGroupIds.Contains( m.GroupId )
                    && !m.Person.IsDeceased
                    && !memberPersonIds.Contains( m.PersonId ) )
                .Select( m => m.Person )
                .Distinct()
                .OrderBy( p => p.LastName )
                .ThenBy( p => p.NickName )
                .ToList();
        }

        /// <summary>
        /// Gets the identifiers of every person attended for the occurrence the supplied values describe.
        /// </summary>
        /// <param name="groupId">The identifier of the occurrence's group.</param>
        /// <param name="locationId">The identifier of the occurrence's location.</param>
        /// <param name="scheduleId">The identifier of the occurrence's schedule.</param>
        /// <param name="attendanceDate">The occurrence date.</param>
        /// <returns>The attended person identifiers.</returns>
        private List<int> GetAttendedPersonIds( int groupId, int locationId, int scheduleId, DateTime attendanceDate )
        {
            return new AttendanceService( RockContext )
                .Queryable()
                .Where( a => a.DidAttend == true
                    && a.Occurrence.GroupId == groupId
                    && a.Occurrence.OccurrenceDate == attendanceDate
                    && a.Occurrence.LocationId == locationId
                    && a.Occurrence.ScheduleId == scheduleId )
                .Select( a => a.PersonAlias.PersonId )
                .ToList();
        }

        /// <summary>
        /// Builds the entry screen bag for the supplied person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="minimumAttendanceAge">The age below which a person cannot be marked as attended.</param>
        /// <param name="attendedPersonIds">
        /// The attended person identifiers for the session's occurrence, or null when attendance is not being taken.
        /// </param>
        /// <returns>The person bag.</returns>
        private RapidAttendanceEntryPersonBag GetPersonBag( Person person, int minimumAttendanceAge, List<int> attendedPersonIds )
        {
            return new RapidAttendanceEntryPersonBag
            {
                PersonGuid = person.Guid,
                NickName = person.NickName,
                FullName = person.FullName,
                PhotoUrl = person.PhotoUrl,
                Age = person.Age,
                IsBelowMinimumAge = person.Age.HasValue && person.Age.Value < minimumAttendanceAge,
                DidAttend = attendedPersonIds != null && attendedPersonIds.Contains( person.Id )
            };
        }

        /// <summary>
        /// Builds the URL of the Attendance List page for the session's occurrence. Returns null when no page is
        /// configured.
        /// </summary>
        /// <param name="group">The session's group.</param>
        /// <param name="groupLocation">The group location joining the session's group and location.</param>
        /// <param name="schedule">The session's schedule.</param>
        /// <param name="attendanceDate">The session's attendance date.</param>
        /// <returns>The page URL, or null.</returns>
        private string GetAttendanceListUrl( GroupCache group, GroupLocationCache groupLocation, NamedScheduleCache schedule, DateTime attendanceDate )
        {
            if ( GetAttributeValue( AttributeKey.AttendanceListPage ).IsNullOrWhiteSpace() )
            {
                return null;
            }

            // LocationId carries the GroupLocation identifier; configured attendance list pages expect it.
            return this.GetLinkedPageUrl( AttributeKey.AttendanceListPage, new Dictionary<string, string>
            {
                [PageParameterKey.GroupId] = group.IdKey,
                [PageParameterKey.LocationId] = groupLocation.IdKey,
                [PageParameterKey.ScheduleId] = schedule.IdKey,
                [PageParameterKey.AttendanceDate] = attendanceDate.ToShortDateString()
            } );
        }

        /// <summary>
        /// Adds and removes attendance records so the session's occurrence matches the submitted roster.
        /// </summary>
        /// <param name="attendances">The submitted roster.</param>
        /// <param name="peopleByGuid">The people referenced by the roster, keyed by unique identifier.</param>
        /// <param name="group">The session's group.</param>
        /// <param name="location">The session's location.</param>
        /// <param name="schedule">The session's schedule.</param>
        /// <param name="attendanceDate">The session's attendance date.</param>
        private void SaveAttendances( List<RapidAttendanceEntryAttendanceBag> attendances, Dictionary<Guid, Person> peopleByGuid, GroupCache group, NamedLocationCache location, NamedScheduleCache schedule, DateTime attendanceDate )
        {
            var attendanceService = new AttendanceService( RockContext );
            var occurrenceService = new AttendanceOccurrenceService( RockContext );
            var campusId = group.CampusId ?? location.CampusId;

            // Find the occurrence (without its attendee list), creating it only if someone is marked attended.
            var occurrence = occurrenceService.Queryable()
                .FirstOrDefault( o =>
                    o.OccurrenceDate == attendanceDate.Date
                    && o.GroupId == group.Id
                    && o.LocationId == location.Id
                    && o.ScheduleId == schedule.Id
                );

            if ( occurrence == null && attendances.Any( a => a.DidAttend ) )
            {
                occurrence = occurrenceService.GetOrAdd( attendanceDate.Date, group.Id, location.Id, schedule.Id );
            }

            if ( occurrence == null )
            {
                return;
            }

            // One family at a time: load only this roster's existing attendance, not the whole attendee list.
            var rosterAliasIds = attendances
                .Where( a => peopleByGuid.ContainsKey( a.PersonGuid ) )
                .SelectMany( a => peopleByGuid[a.PersonGuid].Aliases.Select( pa => pa.Id ) )
                .Distinct()
                .ToList();

            var existingAttendances = attendanceService.Queryable()
                .Where( a =>
                    a.OccurrenceId == occurrence.Id
                    && a.PersonAliasId.HasValue
                    && rosterAliasIds.Contains( a.PersonAliasId.Value )
                )
                .ToList();

            foreach ( var attendanceBag in attendances )
            {
                if ( !peopleByGuid.TryGetValue( attendanceBag.PersonGuid, out var person ) || !person.PrimaryAliasId.HasValue )
                {
                    continue;
                }

                // Match an existing record on any of the person's aliases.
                var personAliasIds = person.Aliases.Select( a => a.Id ).ToList();
                var attendance = existingAttendances.FirstOrDefault( a => a.PersonAliasId.HasValue && personAliasIds.Contains( a.PersonAliasId.Value ) );

                if ( attendanceBag.DidAttend )
                {
                    if ( attendance == null )
                    {
                        attendance = new Attendance { Occurrence = occurrence, OccurrenceId = occurrence.Id, PersonAliasId = person.PrimaryAliasId.Value };
                        attendanceService.Add( attendance );
                    }

                    if ( campusId.HasValue )
                    {
                        attendance.CampusId = campusId.Value;
                    }

                    attendance.StartDateTime = attendanceDate;
                    attendance.DidAttend = true;
                }
                else if ( attendance != null )
                {
                    attendanceService.Delete( attendance );
                }
            }

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the prayer request entered for the person, applying the configured defaults.
        /// </summary>
        /// <param name="person">The person the request is for.</param>
        /// <param name="prayerRequestBag">The entered request, or null when none was entered.</param>
        /// <param name="group">The session's group, whose campus the request prefers, or null.</param>
        private void AddPrayerRequest( Person person, RapidAttendanceEntryPrayerRequestBag prayerRequestBag, GroupCache group )
        {
            if ( prayerRequestBag == null || prayerRequestBag.Text.IsNullOrWhiteSpace() )
            {
                return;
            }

            if ( !GetAttributeValue( AttributeKey.EnablePrayerRequestEntry ).AsBoolean() )
            {
                return;
            }

            var prayerRequest = new PrayerRequest
            {
                IsActive = true,
                IsApproved = true,
                AllowComments = GetAttributeValue( AttributeKey.DefaultAllowComments ).AsBoolean(),
                EnteredDateTime = RockDateTime.Now,
                ApprovedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId,
                ApprovedOnDateTime = RockDateTime.Now,
                ExpirationDate = RockDateTime.Now.AddDays( GetAttributeValue( AttributeKey.ExpiresAfter ).AsIntegerOrNull() ?? 14 ),
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                RequestedByPersonAliasId = person.PrimaryAliasId,
                CampusId = group?.CampusId ?? person.GetCampus()?.Id,
                Text = prayerRequestBag.Text
            };

            // The entered category only applies while category selection is enabled; the default fills any gap.
            var categoryGuid = GetAttributeValue( AttributeKey.EnableCategorySelection ).AsBoolean()
                ? prayerRequestBag.CategoryGuid
                : null;
            categoryGuid = categoryGuid ?? GetAttributeValue( AttributeKey.DefaultCategory ).AsGuidOrNull();

            if ( categoryGuid.HasValue )
            {
                prayerRequest.CategoryId = CategoryCache.Get( categoryGuid.Value )?.Id;
            }

            prayerRequest.IsUrgent = GetAttributeValue( AttributeKey.ShowUrgentFlag ).AsBoolean() && prayerRequestBag.IsUrgent;
            prayerRequest.IsPublic = GetAttributeValue( AttributeKey.ShowPublicFlag ).AsBoolean()
                ? prayerRequestBag.IsPublic
                : GetAttributeValue( AttributeKey.DisplayToPublic ).AsBoolean();

            new PrayerRequestService( RockContext ).Add( prayerRequest );
        }

        /// <summary>
        /// Creates the note entered for the person. The note type must be one of the configured Note Types.
        /// </summary>
        /// <param name="person">The person the note is for.</param>
        /// <param name="noteBag">The entered note, or null when none was entered.</param>
        private void AddPersonNote( Person person, RapidAttendanceEntryNoteBag noteBag )
        {
            if ( noteBag == null || noteBag.Text.IsNullOrWhiteSpace() || !noteBag.NoteTypeGuid.HasValue )
            {
                return;
            }

            var isConfiguredNoteType = GetAttributeValue( AttributeKey.NoteTypes )
                .SplitDelimitedValues()
                .AsGuidList()
                .Contains( noteBag.NoteTypeGuid.Value );
            var noteType = isConfiguredNoteType ? NoteTypeCache.Get( noteBag.NoteTypeGuid.Value ) : null;

            if ( noteType == null )
            {
                return;
            }

            var note = new Note
            {
                EntityId = person.Id,
                NoteTypeId = noteType.Id,
                IsAlert = false,
                IsPrivateNote = false,
                Text = noteBag.Text
            };

            new NoteService( RockContext ).Add( note );
        }

        /// <summary>
        /// Launches the checked workflows for the person, passing the session's group, schedule, location, and date
        /// when attendance is being taken. Workflow types must be among the configured Workflow Types.
        /// </summary>
        /// <param name="person">The person the workflows are launched for.</param>
        /// <param name="workflowTypeGuids">The unique identifiers of the checked workflow types.</param>
        /// <param name="group">The session's group, or null.</param>
        /// <param name="location">The session's location, or null.</param>
        /// <param name="schedule">The session's schedule, or null.</param>
        /// <param name="attendanceDate">The session's attendance date, or null.</param>
        private void LaunchWorkflows( Person person, List<Guid> workflowTypeGuids, GroupCache group, NamedLocationCache location, NamedScheduleCache schedule, DateTime? attendanceDate )
        {
            if ( workflowTypeGuids == null || !workflowTypeGuids.Any() )
            {
                return;
            }

            var configuredWorkflowTypeGuids = GetAttributeValue( AttributeKey.WorkflowTypes ).SplitDelimitedValues().AsGuidList();

            /*
                6/15/26 - JPH

                Process runs configurable workflow actions against the person we pass it, and an action may modify
                that person. We share the block's RockContext (which loaded the person) so any such change saves
                normally.

                Reason: A workflow action may modify the passed person; keep Process on the context that loaded it.
            */
            var workflowService = new WorkflowService( RockContext );

            foreach ( var workflowTypeGuid in workflowTypeGuids.Where( configuredWorkflowTypeGuids.Contains ) )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );

                if ( workflowType == null )
                {
                    continue;
                }

                var workflow = Rock.Model.Workflow.Activate( workflowType, person.FullName );

                if ( group != null && location != null && schedule != null && attendanceDate.HasValue )
                {
                    workflow.SetAttributeValue( "DateSelected", attendanceDate.Value.ToString( "o" ) );
                    workflow.SetAttributeValue( "Group", group.Guid.ToString() );
                    workflow.SetAttributeValue( "Schedule", schedule.Guid.ToString() );
                    workflow.SetAttributeValue( "Location", location.Guid.ToString() );
                }

                // Surface processing failures; an unprocessed workflow otherwise leaves no trace.
                try
                {
                    var didProcess = workflowService.Process( workflow, person, out var workflowErrors );
                    var hasErrors = workflowErrors != null && workflowErrors.Any();

                    if ( !didProcess || hasErrors )
                    {
                        Logger.LogWarning(
                            "Rapid Attendance Entry failed to launch workflow type {WorkflowTypeName} for {PersonName}: {WorkflowErrors}",
                            workflowType.Name,
                            person.FullName,
                            hasErrors ? workflowErrors.AsDelimited( "; " ) : "no error details were returned" );
                    }
                }
                catch ( Exception ex )
                {
                    Logger.LogError( ex, "Rapid Attendance Entry failed to launch workflow type {WorkflowTypeName} for {PersonName}.", workflowType.Name, person.FullName );
                }
            }
        }

        /// <summary>
        /// Creates a connection request for each checked opportunity with the person as the requestor. Opportunities
        /// must be active and belong to the configured Connection Type. Each request gets the type's initial status
        /// (the first active status when the type enforces sequential status, otherwise its default status), the
        /// session's campus (falling back to the person's), and the opportunity's default connector.
        /// </summary>
        /// <param name="person">The person the requests are for.</param>
        /// <param name="opportunityGuids">The unique identifiers of the checked opportunities.</param>
        /// <param name="group">The session's group, whose campus the request prefers, or null.</param>
        private void CreateConnectionRequests( Person person, List<Guid> opportunityGuids, GroupCache group )
        {
            if ( opportunityGuids == null || !opportunityGuids.Any() || !person.PrimaryAliasId.HasValue )
            {
                return;
            }

            var connectionTypeGuid = GetAttributeValue( AttributeKey.ConnectionType ).AsGuidOrNull();
            var connectionType = connectionTypeGuid.HasValue ? ConnectionTypeCache.Get( connectionTypeGuid.Value ) : null;

            if ( connectionType == null )
            {
                return;
            }

            var opportunities = new ConnectionOpportunityService( RockContext )
                .Queryable()
                .Where( o => o.ConnectionTypeId == connectionType.Id
                    && o.IsActive
                    && opportunityGuids.Contains( o.Guid ) )
                .ToList();

            if ( !opportunities.Any() )
            {
                return;
            }

            // A sequential-status type starts at its first active status, ignoring the default flag; other types
            // use their default status (falling back to the first active).
            var activeStatuses = connectionType.OrderedStatuses.Where( s => s.IsActive ).ToList();
            var initialStatus = connectionType.IsSequentialStatusEnforced
                ? activeStatuses.FirstOrDefault()
                : activeStatuses.FirstOrDefault( s => s.IsDefault ) ?? activeStatuses.FirstOrDefault();

            if ( initialStatus == null )
            {
                return;
            }

            var connectionRequestService = new ConnectionRequestService( RockContext );
            var campusId = group?.CampusId ?? person.GetCampus()?.Id;

            foreach ( var opportunity in opportunities )
            {
                connectionRequestService.Add( new ConnectionRequest
                {
                    PersonAliasId = person.PrimaryAliasId.Value,
                    ConnectionOpportunityId = opportunity.Id,
                    ConnectionTypeId = opportunity.ConnectionTypeId,
                    ConnectionState = ConnectionState.Active,
                    ConnectionStatusId = initialStatus.Id,
                    CampusId = campusId,
                    ConnectorPersonAliasId = opportunity.GetDefaultConnectorPersonAliasId( campusId )
                } );
            }
        }

        /// <summary>
        /// Gets the adult and child family roles offered as the Role radio options, adult first.
        /// </summary>
        /// <returns>The role list items.</returns>
        private List<ListItemBag> GetFamilyRoleItems()
        {
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

            return familyGroupType.Roles
                .Where( r => r.Guid == adultGuid || r.Guid == childGuid )
                .OrderBy( r => r.Guid == adultGuid ? 0 : 1 )
                .ToListItemBagList();
        }

        /// <summary>
        /// Gets the configured phone number types for the given setting as list items, in the configured order.
        /// </summary>
        /// <param name="attributeKey">The Adult Phone Types or Child Phone Types setting key.</param>
        /// <returns>The phone type list items.</returns>
        private List<ListItemBag> GetPhoneTypeItems( string attributeKey )
        {
            return GetAttributeValue( attributeKey )
                .SplitDelimitedValues()
                .AsGuidList()
                .Select( g => DefinedValueCache.Get( g ) )
                .Where( dv => dv != null )
                .ToListItemBagList();
        }

        /// <summary>
        /// Builds the person's existing phone numbers, sent so the modal can populate either role's phone rows.
        /// </summary>
        /// <param name="person">The person whose numbers to read.</param>
        /// <returns>The person's phone numbers.</returns>
        private List<RapidAttendanceEntryPhoneNumberBag> GetPhoneNumberBags( Person person )
        {
            if ( person.PhoneNumbers == null )
            {
                return new List<RapidAttendanceEntryPhoneNumberBag>();
            }

            return person.PhoneNumbers
                .Where( pn => pn.NumberTypeValueId.HasValue )
                .Select( pn => new RapidAttendanceEntryPhoneNumberBag
                {
                    PhoneTypeGuid = DefinedValueCache.Get( pn.NumberTypeValueId.Value )?.Guid ?? Guid.Empty,
                    CountryCode = pn.CountryCode,
                    Number = pn.Number,
                    IsMessagingEnabled = pn.IsMessagingEnabled,
                    IsUnlisted = pn.IsUnlisted
                } )
                .Where( bag => bag.PhoneTypeGuid != Guid.Empty )
                .ToList();
        }

        /// <summary>
        /// Resolves a defined value's identifier from its unique identifier, or null.
        /// </summary>
        /// <param name="definedValueGuid">The defined value unique identifier, or null.</param>
        /// <returns>The defined value's identifier, or null.</returns>
        private int? GetDefinedValueId( Guid? definedValueGuid )
        {
            return definedValueGuid.HasValue ? DefinedValueCache.Get( definedValueGuid.Value )?.Id : null;
        }

        /// <summary>
        /// Applies the edited birth date to the person. A future date is rolled back by whole centuries, matching
        /// a two-digit year entered as the current century.
        /// </summary>
        /// <param name="person">The person to update.</param>
        /// <param name="birthDate">The entered month, day, and year parts.</param>
        private void ApplyBirthDate( Person person, DatePartsPickerValueBag birthDate )
        {
            if ( birthDate == null || birthDate.Month <= 0 || birthDate.Day <= 0 )
            {
                person.SetBirthDate( null );
                return;
            }

            if ( birthDate.Year <= 0 )
            {
                person.BirthYear = null;
                person.BirthMonth = birthDate.Month;
                person.BirthDay = birthDate.Day;
                return;
            }

            var date = new DateTime( birthDate.Year, birthDate.Month, birthDate.Day );

            while ( date > RockDateTime.Today )
            {
                date = date.AddYears( -100 );
            }

            person.BirthYear = date.Year;
            person.BirthMonth = date.Month;
            person.BirthDay = date.Day;
        }

        /// <summary>
        /// Applies the edited phone rows to the person and returns the phone type identifiers to keep. Only one
        /// number may have messaging enabled. Cleared rows for the active role's types are dropped while numbers
        /// of types this role does not show are preserved.
        /// </summary>
        /// <param name="person">The person to update.</param>
        /// <param name="phoneNumbers">The edited phone rows.</param>
        /// <param name="isAdult">Whether the adult phone types apply, rather than the child phone types.</param>
        /// <returns>The phone type identifiers to keep.</returns>
        private List<int> ApplyPhoneNumbers( Person person, List<RapidAttendanceEntryPhoneNumberBag> phoneNumbers, bool isAdult )
        {
            var shownTypeIds = GetAttributeValue( isAdult ? AttributeKey.AdultPhoneTypes : AttributeKey.ChildPhoneTypes )
                .SplitDelimitedValues()
                .AsGuidList()
                .Select( g => DefinedValueCache.Get( g )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var filledTypeIds = new List<int>();
            var isSmsTaken = false;

            foreach ( var phoneBag in phoneNumbers ?? new List<RapidAttendanceEntryPhoneNumberBag>() )
            {
                var phoneTypeId = DefinedValueCache.Get( phoneBag.PhoneTypeGuid )?.Id;
                var cleanNumber = PhoneNumber.CleanNumber( phoneBag.Number );

                if ( !phoneTypeId.HasValue || cleanNumber.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneTypeId.Value );

                if ( phoneNumber == null )
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = phoneTypeId.Value };
                    person.PhoneNumbers.Add( phoneNumber );
                }

                phoneNumber.CountryCode = PhoneNumber.CleanNumber( phoneBag.CountryCode );
                phoneNumber.Number = cleanNumber;
                phoneNumber.IsUnlisted = phoneBag.IsUnlisted;
                phoneNumber.IsMessagingEnabled = phoneBag.IsMessagingEnabled && !isSmsTaken;
                isSmsTaken = isSmsTaken || phoneNumber.IsMessagingEnabled;

                filledTypeIds.Add( phoneTypeId.Value );
            }

            // Keep the numbers just entered plus any whose type this role does not show, so a cleared shown number
            // is removed while hidden-type numbers survive.
            var preservedTypeIds = person.PhoneNumbers
                .Where( pn => pn.NumberTypeValueId.HasValue && !shownTypeIds.Contains( pn.NumberTypeValueId.Value ) )
                .Select( pn => pn.NumberTypeValueId.Value );

            return filledTypeIds.Concat( preservedTypeIds ).Distinct().ToList();
        }

        /// <summary>
        /// Filters the submitted attribute values to the active role's configured person attributes, so a role's
        /// values are not written by the other role's save.
        /// </summary>
        /// <param name="attributeValues">The submitted attribute values, keyed by attribute key.</param>
        /// <param name="isAdult">Whether the adult attributes apply, rather than the child attributes.</param>
        /// <returns>The values whose keys belong to the active role's attributes.</returns>
        private Dictionary<string, string> FilterAttributeValues( Dictionary<string, string> attributeValues, bool isAdult )
        {
            if ( attributeValues == null )
            {
                return new Dictionary<string, string>();
            }

            var roleAttributeKeys = new HashSet<string>(
                GetAttributeValue( isAdult ? AttributeKey.AdultPersonAttributes : AttributeKey.ChildPersonAttributes )
                    .SplitDelimitedValues()
                    .AsGuidList()
                    .Select( g => AttributeCache.Get( g )?.Key )
                    .Where( k => k.IsNotNullOrWhiteSpace() ) );

            return attributeValues
                .Where( kvp => roleAttributeKeys.Contains( kvp.Key ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
        }

        #endregion Private Methods
    }
}

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
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Blocks.Group.GroupAttendanceDetail;
using Rock.Logging;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Security;
using Rock.ViewModels.Blocks.Group.GroupAttendanceDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Group
{
    [DisplayName( "Group Attendance Detail" )]
    [Category( "Group" )]
    [Description( "Lists the group members for a specific occurrence date time and allows selecting if they attended or not." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Allow Add",
        Category = AttributeCategory.None,
        DefaultBooleanValue = true,
        Description = "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?",
        IsRequired = false,
        Key = AttributeKey.AllowAdd,
        Order = 0 )]

    [BooleanField(
        "Allow Adding Person",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Should block support adding new people as attendees?",
        IsRequired = false,
        Key = AttributeKey.AllowAddingPerson,
        Order = 1 )]

    [CustomDropdownListField(
        "Add Person As",
        Category = AttributeCategory.None,
        DefaultValue = "Attendee",
        Description = "'Attendee' will only add the person to attendance. 'Group Member' will add them to the group with the default group role.",
        IsRequired = true,
        Key = AttributeKey.AddPersonAs,
        ListSource = "Attendee,Group Member",
        Order = 2 )]

    [LinkedPage(
        "Group Member Add Page",
        Category = AttributeCategory.Pages,
        Description = "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.",
        IsRequired = false,
        Key = AttributeKey.GroupMemberAddPage,
        Order = 3 )]

    [BooleanField(
        "Allow Campus Filter",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Should block add an option to allow filtering people and attendance counts by campus?",
        IsRequired = false,
        Key = AttributeKey.AllowCampusFilter,
        Order = 4 )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 5 )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "This setting filters the list of campuses by statuses that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 6 )]

    [MergeTemplateField(
        "Attendance Roster Template",
        Category = AttributeCategory.None,
        IsRequired = false,
        Key = AttributeKey.AttendanceRosterTemplate,
        Order = 7 )]

    [CodeEditorField(
        "List Item Details Template",
        Category = AttributeCategory.None,
        DefaultValue = DefaultListItemDetailsTemplate,
        Description = "An optional lava template to appear next to each person in the list.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        Key = AttributeKey.ListItemDetailsTemplate,
        Order = 8 )]

    [BooleanField(
        "Restrict Future Occurrence Date",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Should user be prevented from selecting a future Occurrence date?",
        IsRequired = false,
        Key = AttributeKey.RestrictFutureOccurrenceDate,
        Order = 9 )]

    [BooleanField(
        "Show Notes",
        Category = AttributeCategory.None,
        DefaultBooleanValue = true,
        Description = "Should the notes field be displayed?",
        IsRequired = false,
        Key = AttributeKey.ShowNotes,
        Order = 10 )]

    [TextField(
        "Attendance Note Label",
        Category = AttributeCategory.Labels,
        DefaultValue = "Notes",
        Description = "The text to use to describe the notes",
        IsRequired = true,
        Key = AttributeKey.AttendanceNoteLabel,
        Order = 11 )]

    [DefinedValueField(
        "Configured Attendance Types",
        AllowMultiple = true,
        Category = AttributeCategory.None,
        DefaultValue = "",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CHECK_IN_ATTENDANCE_TYPES,
        Description = "The Attendance types that an occurrence can have. If no or one Attendance type is selected, then none will be shown.",
        IsRequired = false,
        Key = AttributeKey.AttendanceOccurrenceTypes,
        Order = 12 )]

    [TextField(
        "Attendance Type Label",
        Category = AttributeCategory.Labels,
        DefaultValue = "Attendance Location",
        Description = "The label that will be shown for the attendance types section.",
        IsRequired = false,
        Key = AttributeKey.AttendanceOccurrenceTypesLabel,
        Order = 13 )]

    [BooleanField(
        "Disable Long-List",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Will disable the long-list feature which groups individuals by the first character of their last name. When enabled, this only shows when there are more than 50 individuals on the list.",
        IsRequired = false,
        Key = AttributeKey.DisableLongList,
        Order = 14 )]

    [BooleanField(
        "Disable Did Not Meet",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Allows for hiding the flag that the group did not meet.",
        IsRequired = false,
        Key = AttributeKey.DisableDidNotMeet,
        Order = 15 )]

    [BooleanField(
        "Hide Back Button",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Will hide the back button from the bottom of the block.",
        IsRequired = false,
        Key = AttributeKey.HideBackButton,
        Order = 16 )]

    [EnumField(
        "Date Selection Mode",
        Category = AttributeCategory.None,
        DefaultEnumValue = ( int ) DateSelectionModeSpecifier.DatePicker,
        Description = "'Date Picker' individual can pick any date. 'Current Date' locked to the current date. 'Pick From Schedule' drop down of dates from the schedule. This will need to be updated based on the location.",
        EnumSourceType = typeof( DateSelectionModeSpecifier ),
        IsRequired = true,
        Key = AttributeKey.DateSelectionMode,
        Order = 17 )]

    [IntegerField(
        "Number of Previous Days To Show",
        Category = AttributeCategory.None,
        DefaultIntegerValue = 14,
        Description = "When the 'Pick From Schedule' option is used, this setting will control how many days back appear in the drop down list to choose from.",
        IsRequired = false,
        Key = AttributeKey.NumberOfPreviousDaysToShow,
        Order = 18 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "64ECB2E0-218F-4EB4-8691-7DC94A767037" )]
    [Rock.SystemGuid.BlockTypeGuid( "308DBA32-F656-418E-A019-9D18235027C1" )]
    public class GroupAttendanceDetail : RockBlockType
    {
        #region Attribute Values

        private const string DefaultListItemDetailsTemplate = @"{% comment %}
  This is the lava template for each attendance item in the GroupAttendanceDetail block
   Available Lava Fields:

   + Person (the person on the attendance record)
   + Attended (whether or not the attendance record is marked DidAttend = true)
   + GroupMembers (the member records for the Person)
   + Roles (the member role name(s) for the Person separated by ', ')
{% endcomment %}
<div class=""d-flex align-items-center h-100"" style=""gap: 8px;"">
    <img src=""{{ Person.PhotoUrl }}"" style=""border-radius: 48px; width: 48px; height: 48px"" />
    <div class=""checkbox-card-data"">
        {% assign activeGroupMembershipCount = GroupMembers | Where:'GroupMemberStatus','Active' | Size %}
        {% if activeGroupMembershipCount == 0 %}<span class=""label label-info align-self-end"">{{ GroupMembers | Select:'GroupMemberStatus' | Distinct | Join:', ' }}</span>{% endif %}
        <div>
        <strong>{{ Person.LastName }}, {{ Person.NickName }}</strong>
        {% if Roles %}<div class=""text-sm text-muted"">{{ Roles }}</div>{% endif %}
        </div>
        {% if activeGroupMembershipCount == 0 %}<span class=""label label-info invisible"">&nbsp;</span>{% endif %}
    </div>
</div>";

        #endregion

        #region Attribute Categories

        private static class AttributeCategory
        {
            public const string None = "";

            public const string CommunicationTemplates = "Communication Templates";

            public const string Labels = "Labels";

            public const string Pages = "Pages";
        }

        #endregion

        #region Keys

        /// <summary>
        /// Keys for attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string AllowAdd = "AllowAdd";
            public const string AllowAddingPerson = "AllowAddingPerson";
            public const string AddPersonAs = "AddPersonAs";
            public const string GroupMemberAddPage = "GroupMemberAddPage";
            public const string AllowCampusFilter = "AllowCampusFilter";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string AttendanceRosterTemplate = "AttendanceRosterTemplate";
            public const string ListItemDetailsTemplate = "ListItemDetailsTemplate";
            public const string RestrictFutureOccurrenceDate = "RestrictFutureOccurrenceDate";
            public const string ShowNotes = "ShowNotes";
            public const string AttendanceNoteLabel = "AttendanceNoteLabel";
            public const string AttendanceOccurrenceTypes = "AttendanceTypes";
            public const string AttendanceOccurrenceTypesLabel = "AttendanceTypeLabel";
            public const string DisableLongList = "DisableLongList";
            public const string DisableDidNotMeet = "DisableDidNotMeet";
            public const string HideBackButton = "HideBackButton";
            public const string DateSelectionMode = "DateSelectionMode";
            public const string NumberOfPreviousDaysToShow = "NumberOfPreviousDaysToShow";
        }

        /// <summary>
        /// Keys for page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupName = "GroupName";
            public const string GroupTypeIds = "GroupTypeIds";
            public const string OccurrenceId = "OccurrenceId";
            public const string Occurrence = "Occurrence";
            public const string Date = "Date";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
            public const string ReturnUrl = "ReturnUrl";
            public const string EntitySetId = "EntitySetId";
        }

        /// <summary>
        /// Keys for user preferences.
        /// </summary>
        private static class PersonPreferenceKeys
        {
            public const string Campus = "campus";
        }

        /// <summary>
        /// Keys for merge fields.
        /// </summary>
        private static class MergeFieldKeys
        {
            public const string AttendanceDate = "AttendanceDate";

            public const string AttendanceNoteLabel = "AttendanceNoteLabel";

            public const string AttendanceOccurrence = "AttendanceOccurrence";

            public const string Attended = "Attended";

            public const string Group = "Group";

            [Obsolete( "Use 'GroupMembers' merge field instead.", false )]
            [RockObsolete( "1.15.2" )]
            public const string GroupMember = "GroupMember";

            [Obsolete( "Use 'Roles' merge field instead.", false )]
            [RockObsolete( "1.15.2" )]
            public const string GroupRoleName = "GroupRoleName";

            public const string Person = "Person";

            public const string GroupMembers = "GroupMembers";

            public const string Roles = "Roles";
        }

        #endregion

        #region Block Settings

        /// <summary>
        /// Should block restrict adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?
        /// </summary>
        /// <value>
        ///   <c>true</c> if using new attendance dates is restricted; otherwise, <c>false</c>.
        /// </value>
        private bool IsNewAttendanceDateAdditionRestricted => !GetAttributeValue( AttributeKey.AllowAdd ).AsBoolean();

        /// <summary>
        /// Gets the attendance note label.
        /// </summary>
        private string AttendanceNoteLabel => GetAttributeValue( AttributeKey.AttendanceNoteLabel );

        /// <summary>
        /// Should block support adding new people as attendees?
        /// </summary>
        /// <value>
        ///   <c>true</c> if new attendee addition is allowed; otherwise, <c>false</c>.
        /// </value>
        private bool IsNewAttendeeAdditionAllowed => GetAttributeValue( AttributeKey.AllowAddingPerson ).AsBoolean();

        /// <summary>
        /// 'Attendee' will only add the person to attendance. 'Group Member' will add them to the group with the default group role.
        /// </summary>
        private string AddPersonAs => GetAttributeValue( AttributeKey.AddPersonAs );

        /// <summary>
        /// Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.
        /// </summary>
        private string AddGroupMemberPage => GetAttributeValue( AttributeKey.GroupMemberAddPage );

        /// <summary>
        /// Should block add an option to allow filtering people and attendance counts by campus?
        /// </summary>
        /// <value>
        ///   <c>true</c> if filtering by campus is allowed; otherwise, <c>false</c>.
        /// </value>
        private bool IsCampusFilteringAllowed => GetAttributeValue( AttributeKey.AllowCampusFilter ).AsBoolean();

        /// <summary>
        /// Campus type defined value guids that limit which campuses are included in the list of available campuses in the campus picker.
        /// </summary>
        /// <value>
        /// The campus type filter.
        /// </value>
        private List<Guid> CampusTypeFilter => GetAttributeValue( AttributeKey.CampusTypes ).SplitDelimitedValues( true ).AsGuidList();

        /// <summary>
        /// Campus status defined value guids that limit which campuses are included in the list of available campuses in the campus picker.
        /// </summary>
        /// <value>
        /// The campus status filter.
        /// </value>
        private List<Guid> CampusStatusFilter => GetAttributeValue( AttributeKey.CampusStatuses ).SplitDelimitedValues( true ).AsGuidList();

        /// <summary>
        /// Gets the attendance roster template unique identifier.
        /// </summary>
        private Guid AttendanceRosterTemplateGuid => GetAttributeValue( AttributeKey.AttendanceRosterTemplate ).AsGuid();

        /// <summary>
        /// An optional lava template to appear next to each person in the list.
        /// </summary>
        private string ListItemDetailsTemplate => GetAttributeValue( AttributeKey.ListItemDetailsTemplate );

        /// <summary>
        /// Should user be restricted from selecting a future Occurrence date?
        /// </summary>
        /// <value>
        ///   <c>true</c> if future occurrence date selection is restricted; otherwise, <c>false</c>.
        /// </value>
        private bool IsFutureOccurrenceDateSelectionRestricted => GetAttributeValue( AttributeKey.RestrictFutureOccurrenceDate ).AsBoolean();

        /// <summary>
        /// Should the notes field be hidden?
        /// </summary>
        /// <value>
        ///   <c>true</c> if the notes section is hidden; otherwise, <c>false</c>.
        /// </value>
        private bool IsNotesSectionHidden => !GetAttributeValue( AttributeKey.ShowNotes ).AsBoolean();

        /// <summary>
        /// Gets the notes section label.
        /// </summary>
        private string NotesSectionLabel => GetAttributeValue( AttributeKey.AttendanceNoteLabel );

        /// <summary>
        /// The Attendance types that an occurrence can have.
        /// <para>If no or one Attendance type is selected, then none will be shown.</para>
        /// </summary>
        private List<string> AttendanceOccurrenceTypes => GetAttributeValues( AttributeKey.AttendanceOccurrenceTypes );

        /// <summary>
        /// The Attendance type values that an occurrence can have.
        /// <para>If no or one Attendance type is selected, then none will be shown.</para>
        /// </summary>
        private List<DefinedValueCache> AttendanceOccurrenceTypeValues => AttendanceOccurrenceTypes.Select( attendanceOccurrenceType => DefinedValueCache.Get( attendanceOccurrenceType ) ).ToList();

        /// <summary>
        /// The label that will be shown for the attendance types section.
        /// </summary>
        private string AttendanceOccurrenceTypesLabel => GetAttributeValue( AttributeKey.AttendanceOccurrenceTypesLabel );

        /// <summary>
        /// Will disable the long-list feature which groups individuals by the first character of their last name.
        /// <para>When enabled, this only shows when there are more than 50 individuals on the list.</para>
        /// </summary>
        private bool IsLongListDisabled => GetAttributeValue( AttributeKey.DisableLongList ).AsBoolean();

        /// <summary>
        /// Allows for hiding the flag that the group did not meet.
        /// </summary>
        private bool IsDidNotMeetDisabled => GetAttributeValue( AttributeKey.DisableDidNotMeet ).AsBoolean();

        /// <summary>
        /// Will hide the back button from the bottom of the block.
        /// </summary>
        private bool IsBackButtonHidden => GetAttributeValue( AttributeKey.HideBackButton ).AsBoolean();

        /// <summary>
        /// Allows the block date selection mode to be configurable.
        /// </summary>
        private DateSelectionModeSpecifier DateSelectionMode => GetAttributeValue( AttributeKey.DateSelectionMode ).ConvertToEnum<DateSelectionModeSpecifier>();

        /// <summary>
        /// When the 'Pick From Schedule' option is used, this setting will control how many days back appear in the drop down list to choose from.
        /// </summary>
        private int NumberOfPreviousDaysToShow => GetAttributeValue( AttributeKey.NumberOfPreviousDaysToShow ).AsInteger();

        #endregion

        #region Page Parameters

        /// <summary>
        /// Gets the group identifier page parameter or null if missing.
        /// </summary>
        private string GroupIdPageParameter => PageParameter( PageParameterKey.GroupId );

        /// <summary>
        /// Gets the group type identifiers page parameter or null if missing.
        /// </summary>
        private string GroupTypeIdsPageParameter => PageParameter( PageParameterKey.GroupTypeIds );

        /// <summary>
        /// Gets the Occurrence ID page parameter or null if missing.
        /// </summary>
        private int? OccurrenceIdPageParameter => PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the Date page parameter or null if missing.
        /// </summary>
        private DateTime? DatePageParameter => PageParameter( PageParameterKey.Date ).AsDateTime();

        /// <summary>
        /// Gets the Occurrence page parameter or null if missing.
        /// </summary>
        private DateTime? OccurrencePageParameter => PageParameter( PageParameterKey.Occurrence ).AsDateTime();

        /// <summary>
        /// Gets the Location ID page parameter or null if missing.
        /// </summary>
        private int? LocationIdPageParameter => PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the Schedule ID page parameter.
        /// </summary>
        private int? ScheduleIdPageParameter => PageParameter( PageParameterKey.ScheduleId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the entity set identifier page parameter.
        /// </summary>
        private string EntitySetIdPageParameter => PageParameter( PageParameterKey.EntitySetId );

        #endregion

        #region IRockObsidianBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( clientService.GetGroupIfAuthorized() );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters, withTracking: true );

                return GetInitializationBox( rockContext, occurrenceData );
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the data needed to display an Attendance record in the Group Attendance Detail block.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        /// <returns>The data needed to display an Attendance record in the Group Attendance Detail block.</returns>
        [BlockAction( "GetAttendance" )]
        public BlockActionResult GetAttendance( GroupAttendanceDetailGetAttendanceRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceQuery = new AttendanceService( rockContext )
                    .Queryable()
                    .Where( a => a.Guid == bag.AttendanceGuid );
                var attendanceInfo = attendanceQuery
                    .Select( a => new
                    {
                        a.Occurrence.GroupId
                    } )
                    .FirstOrDefault();

                if ( attendanceInfo == null )
                {
                    return ActionBadRequest( "Attendance not found." );
                }

                if ( !attendanceInfo.GroupId.HasValue )
                {
                    return ActionBadRequest( "Group not found." );
                }

                var group = new GroupService( rockContext ).Get( attendanceInfo.GroupId.Value );

                if ( !group.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.Forbidden );
                }

                // The attendee may not be a member of the group.
                var groupMembersQuery = new GroupMemberService( rockContext )
                    .Queryable()
                    .Where( m => m.GroupId == group.Id );
                var personAliasAttendanceBagDto = attendanceQuery
                    .Select( a => new PersonAliasAttendanceBagDto
                    {
                        DidAttend = a.DidAttend,
                        GroupMembers = groupMembersQuery.Where( groupMember => groupMember.PersonId == a.PersonAlias.PersonId ).ToList(),
                        Roles = groupMembersQuery.Where( groupMember => groupMember.PersonId == a.PersonAlias.PersonId ).Select( groupMember => groupMember.GroupRole.Name ).ToList(),
                        Person = a.PersonAlias.Person,
                        PersonAliasId = a.PersonAlias.Id,
                        PrimaryCampusGuid = a.PersonAlias.Person.PrimaryCampusId.HasValue ? a.PersonAlias.Person.PrimaryCampus.Guid : ( Guid? ) null
                    } )
                    .FirstOrDefault();

                var attendanceBag = GetAttendanceBag( personAliasAttendanceBagDto );

                return ActionOk( attendanceBag );
            }
        }

        /// <summary>
        /// Gets the current (read-only) occurrence data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The current occurrence data.</returns>
        private OccurrenceData GetOccurrenceData( RockContext rockContext )
        {
            var client = GetOccurrenceDataClientService( rockContext );
            // Use the default search parameters so we only print the persisted AttendanceOccurrence.
            var searchParameters = client.GetAttendanceOccurrenceSearchParameters( client.GetGroupIfAuthorized() );
            return client.GetOccurrenceData( searchParameters );
        }

        /// <summary>
        /// Downloads the AttendanceOccurrence roster.
        /// </summary>
        [BlockAction( "PrintRoster" )]
        public BlockActionResult PrintRoster( GroupAttendanceDetailPrintRosterRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters(
                    clientService.GetGroupIfAuthorized(),
                    bag.AttendanceOccurrenceGuid,
                    search =>
                    {
                        // Override specific search parameters if specific values were passed in.
                        if ( bag.AttendanceOccurrenceDate.HasValue )
                        {
                            search.AttendanceOccurrenceDate = bag.AttendanceOccurrenceDate.Value.Date;
                        }

                        if ( bag.LocationGuid.HasValue )
                        {
                            search.LocationId = new LocationService( rockContext ).GetId( bag.LocationGuid.Value );
                        }

                        if ( bag.ScheduleGuid.HasValue )
                        {
                            search.ScheduleId = new ScheduleService( rockContext ).GetId( bag.ScheduleGuid.Value );
                        }
                    } );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                var attendances = GetAttendanceBags( rockContext, occurrenceData );

                var mergeObjects = new Dictionary<int, object>();

                if ( attendances.Any() )
                {
                    var personGuids = attendances.Select( a => a.PersonGuid ).ToList();
                    var personList = new PersonService( rockContext )
                        .GetByGuids( personGuids )
                        .OrderBy( a => a.LastName )
                        .ThenBy( a => a.NickName )
                        .ToList();
                    foreach ( var person in personList )
                    {
                        mergeObjects.TryAdd( person.Id, person );
                    }
                }

                var mergeFields = this.RequestContext.GetCommonMergeFields();
                mergeFields.Add( MergeFieldKeys.Group, occurrenceData.Group );
                mergeFields.Add( MergeFieldKeys.AttendanceDate, occurrenceData.AttendanceOccurrence.OccurrenceDate );

                var mergeTemplate = new MergeTemplateService( rockContext ).Get( this.AttendanceRosterTemplateGuid );

                if ( mergeTemplate == null )
                {
                    RockLogger.Log.Error( RockLogDomains.Group, new Exception( "Error printing Attendance Roster: No merge template selected. Please configure an 'Attendance Roster Template' in the block settings." ) );
                    return ActionBadRequest( "Unable to print Attendance Roster: No merge template selected. Please configure an 'Attendance Roster Template' in the block settings." );
                }

                var mergeTemplateType = mergeTemplate.GetMergeTemplateType();

                if ( mergeTemplateType == null )
                {
                    RockLogger.Log.Error( RockLogDomains.Group, new Exception( "Error printing Attendance Roster: Unable to determine Merge Template Type from the 'Attendance Roster Template' in the block settings." ) );
                    return ActionBadRequest( $"Error printing Attendance Roster: Unable to determine Merge Template Type from the 'Attendance Roster Template' in the block settings." );
                }

                var mergeObjectList = mergeObjects.Select( a => a.Value ).ToList();

                var outputBinaryFileDoc = mergeTemplateType.CreateDocument( mergeTemplate, mergeObjectList, mergeFields );

                if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
                {
                    if ( mergeTemplateType.Exceptions.Count == 1 )
                    {
                        RockLogger.Log.Error( RockLogDomains.Group, mergeTemplateType.Exceptions[0] );
                    }
                    else if ( mergeTemplateType.Exceptions.Count > 50 )
                    {
                        RockLogger.Log.Error( RockLogDomains.Group, new AggregateException( $"Exceptions merging template {mergeTemplate.Name}. See InnerExceptions for top 50.", mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
                    }
                    else
                    {
                        RockLogger.Log.Error( RockLogDomains.Group, new AggregateException( $"Exceptions merging template {mergeTemplate.Name}. See InnerExceptions", mergeTemplateType.Exceptions.ToList() ) );
                    }
                }

                return ActionOk( new GroupAttendanceDetailPrintRosterResponseBag
                {
                    RedirectUrl = $"{this.RequestContext.RootUrlPath}/GetFile.ashx?guid={outputBinaryFileDoc.Guid}&attachment=true"
                } );
            }
        }

        /// <summary>
        /// Adds a Person as an Attendance record to an AttendanceOccurrence.
        /// <para>Optionally adds the Person as a GroupMember to the associated Group.</para>
        /// </summary>
        /// <param name="bag">The request bag.</param>
        /// <returns>The data needed to display the new Attendance record in the Group Attendance Detail block.</returns>
        [BlockAction( "AddPerson" )]
        public BlockActionResult AddPerson( GroupAttendanceDetailAddPersonRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( clientService.GetGroupIfAuthorized(), searchParameterOverrides: s =>
                {
                    s.AttendanceOccurrenceGuid = bag.AttendanceOccurrenceGuid.IsEmpty() ? null : ( Guid? )bag.AttendanceOccurrenceGuid;
                } );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters, withTracking: true );

                GroupMember groupMember = null;

                if ( occurrenceData.AttendanceOccurrence?.DidNotOccur == true )
                {
                    // This should not be able to happen as the Add Attendee/Group Member control should be hidden when DidNotOccur == true.
                    return ActionBadRequest( "Unable to add person when the meeting did not occur." );
                }

                if ( string.Equals( this.AddPersonAs, "Group Member", StringComparison.OrdinalIgnoreCase ) )
                {
                    groupMember = clientService.AddPersonAsGroupMember( occurrenceData, bag );

                    if ( groupMember == null )
                    {
                        return ActionBadRequest( occurrenceData.ErrorMessage );
                    }
                }

                var personAlias = new PersonAliasService( rockContext )
                    .Queryable()
                    .Include( a => a.Person )
                    .Include( a => a.Person.PrimaryCampus )
                    .Where( a => a.Guid == bag.PersonAliasGuid )
                    .FirstOrDefault();

                if ( personAlias == null )
                {
                    return ActionBadRequest( "Person not found." );
                }

                var campusGuid = personAlias.Person?.PrimaryCampusId.HasValue == true ? personAlias.Person.PrimaryCampus.Guid : ( Guid? ) null;

                if ( !clientService.UpdateAttendance( occurrenceData, new GroupAttendanceDetailMarkAttendanceRequestBag
                {
                    AttendanceOccurrenceGuid = bag.AttendanceOccurrenceGuid,
                    DidAttend = true,
                    PersonAliasId = personAlias.Id,
                } ) )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                };

                // Save GroupMember and Attendance changes.
                rockContext.SaveChanges();

                var personGroupMembers = new GroupMemberService( rockContext )
                    .Queryable( "GroupRole" )
                    .Where( gm => gm.GroupId == occurrenceData.AttendanceOccurrence.GroupId && personAlias.PersonId == gm.PersonId )
                    .ToList();

                var attendanceBag = GetAttendanceBag( new PersonAliasAttendanceBagDto
                {
                    DidAttend = true,
                    GroupMembers = personGroupMembers,
                    Roles = personGroupMembers.Select( gm => gm.GroupRole.Name ).ToList(),
                    Person = personAlias.Person,
                    PersonAliasId = personAlias.Id,
                    PrimaryCampusGuid = campusGuid
                } );

                return ActionOk( new GroupAttendanceDetailAddPersonResponseBag
                {
                    Attendance = attendanceBag
                } );
            }
        }

        /// <summary>
        /// Gets a new AttendanceOccurrence record (without saving) and returns the data needed to display it in the Group Attendance Detail block.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        /// <returns>The data needed to display the AttendanceOccurrence in the Group Attendance Detail block.</returns>
        [BlockAction( "Get" )]
        public BlockActionResult Get( GroupAttendanceDetailGetOrCreateRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters(
                    clientService.GetGroupIfAuthorized(),
                    bag.AttendanceOccurrenceGuid,
                    searchParameterOverrides: ( search ) =>
                    {
                        // Override specific search parameters if specific values were passed in.
                        if ( bag.AttendanceOccurrenceDate.HasValue )
                        {
                            search.AttendanceOccurrenceDate = bag.AttendanceOccurrenceDate.Value.Date;
                        }

                        if ( bag.LocationGuid.HasValue )
                        {
                            search.LocationId = new LocationService( rockContext ).GetId( bag.LocationGuid.Value );
                        }

                        if ( bag.ScheduleGuid.HasValue )
                        {
                            search.ScheduleId = new ScheduleService( rockContext ).GetId( bag.ScheduleGuid.Value );
                        }
                    } );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

                var box = GetInitializationBox( rockContext, occurrenceData );

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Gets or creates a new AttendanceOccurrence record and returns the data needed to display it in the Group Attendance Detail block.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        /// <returns>The data needed to display the AttendanceOccurrence in the Group Attendance Detail block.</returns>
        [BlockAction( "GetOrCreate" )]
        public BlockActionResult GetOrCreate( GroupAttendanceDetailGetOrCreateRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters(
                    clientService.GetGroupIfAuthorized(),
                    bag.AttendanceOccurrenceGuid,
                    searchParameterOverrides: ( search ) =>
                    {
                        // Override specific search parameters if specific values were passed in.
                        if (bag.AttendanceOccurrenceDate.HasValue)
                        {
                            search.AttendanceOccurrenceDate = bag.AttendanceOccurrenceDate.Value.Date;
                        }

                        if ( bag.LocationGuid.HasValue )
                        {
                            search.LocationId = new LocationService( rockContext ).GetId( bag.LocationGuid.Value );
                        }

                        if ( bag.ScheduleGuid.HasValue )
                        {
                            search.ScheduleId = new ScheduleService( rockContext ).GetId( bag.ScheduleGuid.Value );
                        }
                    } );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters, withTracking: true );

                if ( !occurrenceData.IsValid || !occurrenceData.AttendanceOccurrence.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage ?? occurrenceData.AttendanceOccurrence.ValidationResults?.Select( v => v.ErrorMessage ).FirstOrDefault() );
                }

                var result = clientService.Save( occurrenceData, bag );

                if ( !result )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                var box = GetInitializationBox( rockContext, occurrenceData );

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Subscribes to the real-time AttendanceOccurrence channels.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="groupGuid">The Group unique identifier.</param>
        [BlockAction( "SubscribeToRealTime" )]
        public async Task<BlockActionResult> SubscribeToRealTime( string connectionId, Guid groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( groupGuid );

                // Authorize the current user.
                if ( group == null )
                {
                    return ActionNotFound( "Group not found." );
                }

                if ( !group.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.Forbidden );
                }

                var topicChannels = RealTimeHelper.GetTopicContext<IEntityUpdated>().Channels;

                await topicChannels.AddToChannelAsync( connectionId, EntityUpdatedTopic.GetAttendanceChannelForGroup( groupGuid ) );
                await topicChannels.AddToChannelAsync( connectionId, EntityUpdatedTopic.GetAttendanceOccurrenceChannelForGroup( groupGuid ) );

                return ActionOk();
            }
        }

        /// <summary>
        /// Updates a single <see cref="Attendance.DidAttend"/> value.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction( "MarkAttendance" )]
        public BlockActionResult MarkAttendance( GroupAttendanceDetailMarkAttendanceRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( clientService.GetGroupIfAuthorized(), bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters, withTracking: true );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                var attendance = occurrenceData.AttendanceOccurrence.Attendees.FirstOrDefault( a => a.PersonAliasId == bag.PersonAliasId );

                if ( attendance == null )
                {
                    var personAliasId = bag.PersonAliasId;

                    DateTime startDateTime;

                    if ( occurrenceData.AttendanceOccurrence.Schedule != null
                        && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() )
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add(
                            occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay );
                    }
                    else
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate;
                    }

                    occurrenceData.AttendanceOccurrence.Attendees.Add( CreateAttendanceInstance( personAliasId, occurrenceData.Campus?.Id, startDateTime, bag.DidAttend ) );
                }
                else
                {
                    attendance.DidAttend = bag.DidAttend;
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Updates a single <see cref="AttendanceOccurrence.DidNotOccur"/> value.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction( "UpdateDidNotOccur" )]
        public BlockActionResult UpdateDidNotOccur( GroupAttendanceDetailUpdateDidNotOccurRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( clientService.GetGroupIfAuthorized(), bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters, withTracking: true );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                occurrenceData.AttendanceOccurrence.DidNotOccur = bag.DidNotOccur;

                // Save the AttendanceOccurrence first so we can send out a real-time update before updating attendees.
                rockContext.SaveChanges();

                if ( bag.DidNotOccur )
                {
                    // Clear the DidAttend flags if the occurrence did not occur.
                    foreach ( var attendee in occurrenceData.AttendanceOccurrence.Attendees )
                    {
                        attendee.DidAttend = null;
                    }
                }
                else
                {
                    var campusId = occurrenceData.Campus?.Id;
                    var existingAttendances = occurrenceData.AttendanceOccurrence.Attendees.ToList();

                    // Add or update attendances with DidAttend = false.
                    foreach ( var attendee in GetAttendanceBags( rockContext, occurrenceData ) )
                    {
                        var existingAttendance = existingAttendances.FirstOrDefault( a => a.PersonAliasId == attendee.PersonAliasId );

                        if ( existingAttendance != null )
                        {
                            existingAttendance.DidAttend = false;
                        }
                        else
                        {
                            var attendance = CreateAttendanceInstance(
                                attendee.PersonAliasId,
                                campusId,
                                occurrenceData.AttendanceOccurrence.Schedule != null && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() ? occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add( occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay ) : occurrenceData.AttendanceOccurrence.OccurrenceDate,
                                false );

                            if ( !attendance.IsValid )
                            {
                                occurrenceData.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                                return ActionBadRequest( occurrenceData.ErrorMessage );
                            }

                            occurrenceData.AttendanceOccurrence.Attendees.Add( attendance );
                        }
                    }
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Updates a single <see cref="AttendanceOccurrence.Notes"/> value.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction( "UpdateNotes" )]
        public BlockActionResult UpdateNotes( GroupAttendanceDetailUpdateNotesRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( clientService.GetGroupIfAuthorized(), bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters, withTracking: true );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                occurrenceData.AttendanceOccurrence.Notes = bag.Notes;
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Updates a single <see cref="AttendanceOccurrence.AttendanceTypeValueId"/> value.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction( "UpdateAttendanceOccurrenceType" )]
        public BlockActionResult UpdateAttendanceOccurrenceType( GroupAttendanceDetailUpdateAttendanceOccurrenceTypeRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( clientService.GetGroupIfAuthorized(), bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters, withTracking: true );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                if ( !this.AttendanceOccurrenceTypeValues.Any( v => v.Guid == bag.AttendanceOccurrenceTypeGuid ) )
                {
                    return ActionBadRequest( "The selected attendance type is not allowed." );
                }

                occurrenceData.AttendanceOccurrence.AttendanceTypeValueId = DefinedValueCache.GetId( bag.AttendanceOccurrenceTypeGuid );

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Gets the group locations that can be displayed in the group location picker.
        /// </summary>
        /// <param name="bag">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the group locations.</returns>
        [BlockAction( "GetGroupLocations" )]
        public BlockActionResult GetGroupLocations( GroupAttendanceDetailGetGroupLocationsRequestBag bag )
        {
            if ( !bag.GroupGuid.HasValue )
            {
                return ActionNotFound();
            }
            using ( var rockContext = new RockContext() )
            {
                var list = GetGroupLocations( rockContext, bag.GroupGuid.Value );

                return ActionOk( list );
            }
        }

        /// <summary>
        /// Gets the group location schedules that can be displayed in the group location schedule picker.
        /// </summary>
        /// <param name="bag">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the group location schedules.</returns>
        [BlockAction( "GetGroupLocationSchedules" )]
        public BlockActionResult GetGroupLocationSchedules( GroupAttendanceDetailGetGroupLocationSchedulesRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var schedules = GetGroupLocationSchedules( rockContext, bag.GroupGuid, bag.LocationGuid, bag.Date );

                if ( !schedules.Any() )
                {
                    return ActionNotFound();
                }

                return ActionOk( schedules );
            }
        }

        /// <summary>
        /// Gets the group location schedule dates that can be displayed in the group location schedule date picker.
        /// </summary>
        /// <param name="bag">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the group location schedule dates.</returns>
        [BlockAction( "GetGroupLocationScheduleDates" )]
        public BlockActionResult GetGroupLocationScheduleDates( GroupAttendanceDetailGetGroupLocationScheduleDatesRequestBag bag )
        {
            if ( !bag.GroupGuid.HasValue || !bag.LocationGuid.HasValue )
            {
                return ActionNotFound();
            }

            using ( var rockContext = new RockContext() )
            {
                var list = GetGroupLocationScheduleDateBags( rockContext, bag );

                if ( !list.Any() )
                {
                    return ActionNotFound();
                }

                return ActionOk( list );
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the group locations.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <returns>Group locations.</returns>
        private List<ListItemBag> GetGroupLocations( RockContext rockContext, Guid groupGuid )
        {
            var locations = new GroupService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.Guid == groupGuid && g.GroupLocations.Any() )
                    .SelectMany( g => g.GroupLocations )
                    .Where( gl => gl.Location != null )
                    .Select( gl => gl.Location )
                    .Where( l => l.Name != null && !string.IsNullOrEmpty( l.Name.Trim() ) )
                    .Select( l => new LocationDto
                    {
                        Id = l.Id,
                        Guid = l.Guid,
                        Name = l.Name,
                        ParentLocationId = l.ParentLocationId,
                        ParentLocationGuid = l.ParentLocationId.HasValue ? l.ParentLocation.Guid : ( Guid? ) null
                    } )
                    .ToList();

            if ( !locations.Any() )
            {
                return new List<ListItemBag>();
            }

            var locationPaths = new Dictionary<Guid, string>();
            var locationValues = new Dictionary<Guid, string>();

            var locationService = new LocationService( rockContext );

            foreach ( var location in locations )
            {
                // Get location path
                var parentLocationPath = string.Empty;
                if ( location.ParentLocationId.HasValue && location.ParentLocationGuid.HasValue )
                {
                    var parentLocationGuid = location.ParentLocationGuid.Value;

                    if ( !locationPaths.ContainsKey( parentLocationGuid ) )
                    {
                        locationPaths.Add( parentLocationGuid, locationService.GetPath( location.ParentLocationId.Value ) );
                    }

                    parentLocationPath = locationPaths[parentLocationGuid];
                }

                if ( !locationValues.ContainsKey( location.Guid ) )
                {
                    locationValues.Add( location.Guid, new List<string> { parentLocationPath, location.Name }.AsDelimited( " > " ) );
                }
            }

            return locationValues.Select( kvp => new ListItemBag
            {
                Value = kvp.Key.ToString(),
                Text = kvp.Value
            } ).ToList();
        }

        /// <summary>
        /// Gets the group location schedule dates that can be displayed in the group location schedule date picker.
        /// </summary>
        /// <param name="bag">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the group location schedule dates.</returns>
        private List<ListItemBag> GetGroupLocationScheduleDateBags( RockContext rockContext, GroupAttendanceDetailGetGroupLocationScheduleDatesRequestBag bag )
        {
            if ( !bag.GroupGuid.HasValue || !bag.LocationGuid.HasValue )
            {
                return new List<ListItemBag>();
            }

            var groupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl => gl.Group.Guid == bag.GroupGuid.Value )
                .Where( gl => gl.Location.Guid == bag.LocationGuid.Value )
                .Where( gl => gl.Schedules.Any() )
                .SelectMany( gl => gl.Schedules )
                .OrderBy( s => s.Name )
                .Distinct()
                .ToList();

            if ( !groupLocationSchedules.Any() )
            {
                return new List<ListItemBag>();
            }

            var scheduleDates = new List<(DateTime Date, Guid ScheduleGuid, string FormattedValue)>();

            var now = RockDateTime.Now;
            DateTime startDate;
            DateTime endDate;

            if ( bag.OccurrenceDate.HasValue )
            {
                startDate = bag.OccurrenceDate.Value.Date;
                endDate = startDate.AddDays( 1 );
            }
            else if ( bag.NumberOfPreviousDaysToShow.HasValue )
            {
                // Get schedules between N days ago and now.
                startDate = now.AddDays( -bag.NumberOfPreviousDaysToShow.Value );
                endDate = now;
            }
            else
            {
                // By default, get schedules between 1 month ago and now.
                startDate = now.AddMonths( -1 );
                endDate = now;
            }

            foreach ( var groupLocationSchedule in groupLocationSchedules )
            {
                var startTimes = groupLocationSchedule.GetScheduledStartTimes( startDate, endDate );

                foreach ( var startTime in startTimes )
                {
                    scheduleDates.Add( (startTime, groupLocationSchedule.Guid, startTime.ToString( "dddd, MMMM d, yyyy - h:mmtt" )) );
                }
            }

            return scheduleDates
                .OrderByDescending( s => s.Date )
                .Select( s => new ListItemBag
                {
                    Value = $"{s.Date:s}|{s.ScheduleGuid}",
                    Text = s.FormattedValue
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the group location schedules that can be displayed in the group location schedule picker.
        /// </summary>
        /// <param name="bag">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the group location schedules.</returns>
        private List<ListItemBag> GetGroupLocationSchedules( RockContext rockContext, Guid? groupGuid, Guid? locationGuid, DateTimeOffset? date )
        {
            if ( !groupGuid.HasValue )
            {
                return new List<ListItemBag>();
            }

            var groupLocationsQuery = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( gl => gl.Location )
                .Include( gl => gl.Schedules )
                .Where( gl => gl.Group.Guid == groupGuid.Value )
                .Where( gl => gl.Schedules.Any() );

            if ( locationGuid.HasValue )
            {
                groupLocationsQuery = groupLocationsQuery.Where( gl => gl.Location.Guid == locationGuid.Value );
            }

            var groupLocationSchedulesQuery = groupLocationsQuery
                .SelectMany( gl => gl.Schedules )
                .OrderBy( s => s.Name )
                .Distinct();

            var schedules = new Dictionary<Guid, string>();

            if ( date.HasValue )
            {
                // Only include schedules that apply to a specific date.
                var groupLocationSchedules = groupLocationSchedulesQuery.ToList();
                foreach ( var schedule in groupLocationSchedules )
                {
                    var startTimes = schedule.GetScheduledStartTimes( date.Value.Date, date.Value.Date.AddDays( 1 ) );
                    if ( startTimes.Any() )
                    {
                        schedules.TryAdd( schedule.Guid, schedule.Name );
                    }
                }
            }
            else
            {
                var groupLocationSchedules = groupLocationSchedulesQuery
                    .Select( s => new ScheduleDto
                    {
                        ScheduleGuid = s.Guid,
                        ScheduleName = s.Name
                    } )
                    .ToList();

                foreach ( var groupLocationSchedule in groupLocationSchedules )
                {
                    schedules.TryAdd( groupLocationSchedule.ScheduleGuid, groupLocationSchedule.ScheduleName );
                }
            }

            if ( !schedules.Any() )
            {
                return new List<ListItemBag>();
            }

            return schedules
                .Select( kvp => new ListItemBag
                {
                    Value = kvp.Key.ToString(),
                    Text = kvp.Value
                } )
                .ToList();
        }

        /// <summary>
        /// Gets an initialization box.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <returns>An initialization box.</returns>
        private GroupAttendanceDetailInitializationBox GetInitializationBox( RockContext rockContext, OccurrenceData occurrenceData )
        {
            var occurrence = occurrenceData.AttendanceOccurrence;
            var group = occurrenceData.Group;

            if ( !occurrenceData.IsValid )
            {
                return SetErrorData( occurrenceData, new GroupAttendanceDetailInitializationBox() );
            }

            var box = new GroupAttendanceDetailInitializationBox
            {
                AddGroupMemberPageUrl = GetAddGroupMemberPageUrl( occurrenceData ),
                AddPersonAs = this.AddPersonAs,
                AttendanceOccurrenceGuid = occurrenceData.IsNewOccurrence ? ( Guid? ) null : occurrenceData.AttendanceOccurrence.Guid,
                AttendanceOccurrenceId = occurrenceData.IsNewOccurrence ? ( int? ) null : occurrenceData.AttendanceOccurrence.Id,
                BackPageUrl = !this.IsBackButtonHidden ? GetBackPageUrl( occurrenceData ) : null,
                GroupGuid = occurrenceData.Group.Guid,
                GroupMembersSectionLabel = group.GroupType.GroupMemberTerm.Pluralize(),
                GroupName = occurrenceData.Group.Name,
                IsBackButtonHidden = this.IsBackButtonHidden,
                IsCampusFilteringAllowed = this.IsCampusFilteringAllowed,
                IsDateIncludedInPickFromSchedule = this.DateSelectionMode != DateSelectionModeSpecifier.PickFromSchedule,
                IsDidNotMeetDisabled = this.IsDidNotMeetDisabled,
                IsLocationRequired = occurrenceData.Group.GroupType.GroupAttendanceRequiresLocation,
                IsRosterDownloadShown = !this.AttendanceRosterTemplateGuid.IsEmpty(),
                IsScheduleRequired = occurrenceData.Group.GroupType.GroupAttendanceRequiresSchedule,

                // Enforce this on the client if possible. Either way, it should be enforced on the server.
                IsFutureOccurrenceDateSelectionRestricted = this.IsFutureOccurrenceDateSelectionRestricted,

                IsLongListDisabled = this.IsLongListDisabled,
                IsNewAttendanceDateAdditionRestricted = this.IsNewAttendanceDateAdditionRestricted,
                IsNotesSectionHidden = this.IsNotesSectionHidden,
                NotesSectionLabel = this.NotesSectionLabel,
                NumberOfPreviousDaysToShow = this.NumberOfPreviousDaysToShow,
                CampusStatusFilter = this.CampusStatusFilter,
                CampusTypeFilter = this.CampusTypeFilter,
            };

            var groupGuid = group.Guid;
            var groupLocations = new Lazy<List<ListItemBag>>( () => GetGroupLocations( new RockContext(), groupGuid ) );
            var groupLocationSchedules = new Lazy<List<ListItemBag>>( () => GetGroupLocationSchedules( new RockContext(), groupGuid, occurrence.Location?.Guid, occurrence.OccurrenceDate ) );
            var groupLocationScheduleDates = new Lazy<List<ListItemBag>>( () => GetGroupLocationScheduleDateBags(
                new RockContext(),
                new GroupAttendanceDetailGetGroupLocationScheduleDatesRequestBag
                {
                    GroupGuid = groupGuid,
                    LocationGuid = occurrence.Location?.Guid,
                    NumberOfPreviousDaysToShow = this.NumberOfPreviousDaysToShow,
                    OccurrenceDate = this.DateSelectionMode != DateSelectionModeSpecifier.PickFromSchedule ? occurrence.OccurrenceDate : ( DateTimeOffset? )null
                } ) );

            if ( occurrenceData.IsSpecificOccurrence )
            {
                box.AttendanceOccurrenceDate = occurrenceData.AttendanceOccurrence.OccurrenceDate.Date;
                box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.Readonly;

                if ( occurrence.Location != null )
                {
                    box.LocationGuid = occurrence.Location.Guid;
                    box.LocationSelectionMode = GroupAttendanceDetailLocationSelectionMode.Readonly;
                    box.LocationLabel = new LocationService( rockContext ).GetPath( occurrence.Location.Id );
                }
                else
                {
                    box.LocationSelectionMode = GroupAttendanceDetailLocationSelectionMode.None;
                }

                if ( occurrence.Schedule != null )
                {
                    box.ScheduleGuid = occurrence.Schedule.Guid;
                    box.ScheduleSelectionMode = GroupAttendanceDetailScheduleSelectionMode.Readonly;
                    box.ScheduleLabel = occurrence.Schedule.ToString();
                }
                else
                {
                    box.ScheduleSelectionMode = GroupAttendanceDetailScheduleSelectionMode.None;
                }
            }
            // The individual is not looking at a specific occurrence, so let them choose a date, location, and schedule.
            else
            {
                box.AttendanceOccurrenceDate = occurrenceData.AttendanceOccurrence.OccurrenceDate.Date;
                switch ( this.DateSelectionMode )
                {
                    case DateSelectionModeSpecifier.DatePicker:
                        // If there are no date query parameters then show a date picker.
                        box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.DatePicker;
                        break;
                    case DateSelectionModeSpecifier.CurrentDate:
                        if ( occurrenceData.AttendanceOccurrence.Location != null && groupLocationScheduleDates.Value.Any() )
                        {
                            // There is a location, and schedules for the group, location, and current date,
                            // so show a scheduled date picker.
                            box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.ScheduledDatePicker;
                        }
                        else
                        {
                            box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.Readonly;
                        }
                        break;
                    case DateSelectionModeSpecifier.PickFromSchedule:
                        if ( occurrence.Location != null && groupLocationScheduleDates.Value.Any() )
                        {
                            // There is a location, and schedules for the group, location, and current date,
                            // so show a scheduled date picker.
                            box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.ScheduledDatePicker;
                        }
                        else if ( groupLocations.Value.Any() )
                        {
                            // If are locations to choose from, then hide the date picker.
                            box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.None;
                        }
                        else
                        {
                            // If there are no locations to choose from, then display the date picker.
                            box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.DatePicker;
                        } 
                        break;
                }

                if ( groupLocations.Value.Any() )
                {
                    box.LocationSelectionMode = GroupAttendanceDetailLocationSelectionMode.GroupLocationPicker;
                }
                else
                {
                    box.LocationSelectionMode = GroupAttendanceDetailLocationSelectionMode.None;
                }

                if ( occurrence.Location != null )
                {
                    box.LocationGuid = occurrence.Location.Guid;
                    box.LocationLabel = new LocationService( rockContext ).GetPath( occurrence.Location.Id );

                    // If the occurrence has a location and there are no locations to choose from, then fallback to a readonly location.
                    if ( !groupLocations.Value.Any() )
                    {
                        box.LocationSelectionMode = GroupAttendanceDetailLocationSelectionMode.Readonly;
                    }
                }

                if ( groupLocationSchedules.Value.Any() )
                {
                    box.ScheduleSelectionMode = GroupAttendanceDetailScheduleSelectionMode.GroupLocationSchedulePicker;
                }
                else
                {
                    box.ScheduleSelectionMode = GroupAttendanceDetailScheduleSelectionMode.None;
                }

                if ( occurrence.Schedule != null )
                {
                    box.ScheduleGuid = occurrence.Schedule.Guid;
                    box.ScheduleLabel = occurrence.Schedule.ToString();

                    // If the occurrence has a schedule and there are no schedules to choose from, then fallback to a readonly schedule.
                    if ( !groupLocationSchedules.Value.Any() )
                    {
                        box.ScheduleSelectionMode = GroupAttendanceDetailScheduleSelectionMode.Readonly;
                    }
                }
            }

            // Set the allowed occurrence types.
            var allowedAttendanceTypeValues = this.AttendanceOccurrenceTypeValues;

            if ( allowedAttendanceTypeValues.Any() )
            {
                box.AttendanceOccurrenceTypes = allowedAttendanceTypeValues.ToListItemBagList();
                box.AttendanceOccurrenceTypesSectionLabel = this.AttendanceOccurrenceTypesLabel;
                box.IsAttendanceOccurrenceTypesSectionShown = box.AttendanceOccurrenceTypes.Count > 1;
                if ( box.AttendanceOccurrenceTypes.Count == 1 )
                {
                    box.SelectedAttendanceOccurrenceTypeValue = box.AttendanceOccurrenceTypes.First().Value;
                }
                else
                {
                    var attendanceOccurrenceTypeValue = allowedAttendanceTypeValues.Where( a => a.Id == occurrenceData.AttendanceOccurrence?.AttendanceTypeValueId ).Select( a => a.Guid.ToString() ).FirstOrDefault();
                    box.SelectedAttendanceOccurrenceTypeValue = box.AttendanceOccurrenceTypes
                        .FirstOrDefault( attendanceOccurrenceType => attendanceOccurrenceType.Value == attendanceOccurrenceTypeValue )?.Value;
                }
            }

            var allowAddPerson = this.IsNewAttendeeAdditionAllowed;

            if ( this.AddGroupMemberPage.IsNotNullOrWhiteSpace() )
            {
                box.IsNewAttendeeAdditionAllowed = allowAddPerson && box.AddPersonAs == "Attendee";
            }
            else
            {
                box.IsNewAttendeeAdditionAllowed = allowAddPerson;
            }

            box.IsAttendanceOccurrenceTypesSectionShown = allowedAttendanceTypeValues.Count > 1;
            box.AttendanceOccurrenceTypesSectionLabel = this.AttendanceOccurrenceTypesLabel;
            box.AttendanceOccurrenceTypes = allowedAttendanceTypeValues
                .Select( attendenceOccurrenceType => new ListItemBag
                {
                    Text = attendenceOccurrenceType.Value,
                    Value = attendenceOccurrenceType.Guid.ToString(),
                } )
                .ToList();
            if ( box.AttendanceOccurrenceTypes.Count == 1 )
            {
                box.SelectedAttendanceOccurrenceTypeValue = box.AttendanceOccurrenceTypes.First().Value;
            }
            else
            {
                var attendanceOccurrenceTypeValue = allowedAttendanceTypeValues.Where( a => a.Id == occurrenceData.AttendanceOccurrence?.AttendanceTypeValueId ).Select( a => a.Guid.ToString() ).FirstOrDefault();
                box.SelectedAttendanceOccurrenceTypeValue = box.AttendanceOccurrenceTypes
                    .FirstOrDefault( attendanceOccurrenceType => attendanceOccurrenceType.Value == attendanceOccurrenceTypeValue )?.Value;
            }

            if ( occurrenceData.AttendanceOccurrence.Id > 0 )
            {
                box.Notes = occurrenceData.AttendanceOccurrence.Notes;
                box.IsDidNotMeetChecked = occurrenceData.AttendanceOccurrence.DidNotOccur ?? false;
            }

            box.Attendances = GetAttendanceBags( rockContext, occurrenceData );

            return box;
        }

        /// <summary>
        /// Gets the "Add Group Member" page URL.
        /// </summary>
        private string GetAddGroupMemberPageUrl( OccurrenceData occurrenceData )
        {
            if ( this.AddGroupMemberPage.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.GroupId, occurrenceData.Group.Id.ToString() },
                { PageParameterKey.GroupName, occurrenceData.Group.Name },
                { PageParameterKey.ReturnUrl, this.RequestContext.RequestUri.AbsoluteUri }
            };

            return this.GetLinkedPageUrl( AttributeKey.GroupMemberAddPage, queryParams );
        }

        /// <summary>
        /// Gets the "Back" page URL.
        /// </summary>
        private string GetBackPageUrl( OccurrenceData occurrenceData )
        {
            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.GroupId, occurrenceData.Group.Id.ToString() }
            };

            var returnUrl = this.PageParameter( PageParameterKey.ReturnUrl );

            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                queryParams.Add( PageParameterKey.ReturnUrl, returnUrl );
            }

            var groupTypeIds = this.GroupTypeIdsPageParameter;

            if ( groupTypeIds.IsNotNullOrWhiteSpace() )
            {
                queryParams.Add( PageParameterKey.GroupTypeIds, groupTypeIds );
            }

            return this.GetParentPageUrl( queryParams );
        }

        /// <summary>
        /// Gets the client service for reading the occurrence data.
        /// </summary>
        private OccurrenceDataClientService GetOccurrenceDataClientService( RockContext rockContext )
        {
            return new OccurrenceDataClientService( this, rockContext );
        }

        /// <summary>
        /// Sets the error data on the box from the occurrence data.
        /// </summary>
        private GroupAttendanceDetailInitializationBox SetErrorData( OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            var errors = new List<string>();

            if (occurrenceData.ErrorMessage.IsNotNullOrWhiteSpace())
            {
                errors.Add( occurrenceData.ErrorMessage );
            }

            if ( occurrenceData.AttendanceOccurrence?.ValidationResults?.Any() == true )
            {
                errors.AddRange( occurrenceData.AttendanceOccurrence.ValidationResults.Select( a => a.ErrorMessage ) );
            }

            box.ErrorMessage = string.Join( "<br>", errors );
            box.IsAuthorizedGroupNotFoundError = occurrenceData.IsAuthorizedGroupNotFoundError;
            box.IsNoAttendanceOccurrencesError = occurrenceData.IsNoAttendanceOccurrencesError;

            return box;
        }

        /// <summary>
        /// Gets the list of bags containing the information to render the attendances in the Group Attendance Detail block.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <returns>The list of bags containing the information to render the attendances in the Group Attendance Detail block.</returns>
        private List<GroupAttendanceDetailAttendanceBag> GetAttendanceBags( RockContext rockContext, OccurrenceData occurrenceData )
        {
            // Get the query to get this group's members.
            var groupMembersQuery = new GroupMemberService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( m => m.GroupId == occurrenceData.Group.Id );

            // Get the query to get primary person aliases.
            var primaryAliasQuery = new PersonAliasService( rockContext ).GetPrimaryAliasQuery();

            IQueryable<PersonAliasAttendanceBagDto> existingAttendeesQuery = null;
            if ( occurrenceData.AttendanceOccurrence.Id > 0 )
            {
                // Get the query for people who did or didn't attend this occurrence.
                // These may or may not be group members.
                existingAttendeesQuery = new AttendanceService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( a =>
                        a.OccurrenceId == occurrenceData.AttendanceOccurrence.Id
                        && a.DidAttend.HasValue
                        && a.PersonAliasId.HasValue )
                    .Select( a => new PersonAliasAttendanceBagDto
                    {
                        DidAttend = a.DidAttend,
                        // We should include all group members regardless of GroupMemberStatus
                        // since these attendees already exist on the occurrence.
                        GroupMembers = groupMembersQuery.Where( m => m.PersonId == a.PersonAlias.Person.Id ).ToList(),
                        Person = a.PersonAlias.Person,
                        PersonAliasId = a.PersonAlias.Id,
                        PrimaryCampusGuid = a.PersonAlias.Person.PrimaryCampusId.HasValue ? a.PersonAlias.Person.PrimaryCampus.Guid : ( Guid? ) null,
                        Roles = groupMembersQuery.Where( m => m.PersonId == a.PersonAlias.Person.Id ).Select( v => v.GroupRole.Name ).ToList()
                    } );
            }

            IQueryable<PersonAliasAttendanceBagDto> prospectiveAttendeesQuery = null;
            var entitySetId = this.EntitySetIdPageParameter.AsIntegerOrNull();
            var entitySetGuid = this.EntitySetIdPageParameter.AsGuidOrNull();
            if ( entitySetId.HasValue || entitySetGuid.HasValue )
            {
                // Get the prospective attendees from a Person EntitySet.
                // These may or may not be group members.
                var entitySetService = new EntitySetService( rockContext );
                IQueryable<Person> personEntitySetQuery;

                if ( entitySetId.HasValue )
                {
                    personEntitySetQuery = entitySetService.GetEntityQuery<Person>( entitySetId.Value );
                }
                else
                {
                    personEntitySetQuery = entitySetService.GetEntityQuery<Person>( entitySetGuid.Value );
                }

                prospectiveAttendeesQuery = personEntitySetQuery
                    .AsNoTracking()
                    .Select( p => new PersonAliasAttendanceBagDto
                    {
                        DidAttend = null,
                        GroupMembers = groupMembersQuery.Where( m => m.PersonId == p.Id ).ToList(),
                        Person = p,
                        PersonAliasId = primaryAliasQuery.Where( a => a.PersonId == p.Id ).Select( a => a.Id ).FirstOrDefault(),
                        PrimaryCampusGuid = p.PrimaryCampusId.HasValue ? p.PrimaryCampus.Guid : ( Guid? ) null,
                        Roles = groupMembersQuery.Where( m => m.PersonId == p.Id ).Select( v => v.GroupRole.Name ).ToList()
                    } );
            }
            else
            {
                // Get the prospective attendees from the current group members.
                // Inactive group members should be excluded from the prospective attendees result.
                prospectiveAttendeesQuery = primaryAliasQuery
                    .Select( p => new PersonAliasAttendanceBagDto
                    {
                        DidAttend = null,
                        GroupMembers = groupMembersQuery.Where( gm => gm.PersonId == p.PersonId && gm.GroupMemberStatus != GroupMemberStatus.Inactive ).ToList(),
                        Person = p.Person,
                        PersonAliasId = p.Id,
                        PrimaryCampusGuid = p.Person.PrimaryCampusId.HasValue ? p.Person.PrimaryCampus.Guid : ( Guid? ) null,
                        Roles = groupMembersQuery.Where( gm => gm.PersonId == p.PersonId && gm.GroupMemberStatus != GroupMemberStatus.Inactive ).Select( v => v.GroupRole.Name ).ToList()
                    } )
                    .Where( p => p.GroupMembers.Any() );
            }

            if (existingAttendeesQuery == null && prospectiveAttendeesQuery == null )
            {
                // This is a fallback in case the queries above aren't set for some reason.
                return new List<GroupAttendanceDetailAttendanceBag>();
            }

            IEnumerable<PersonAliasAttendanceBagDto> allAttendees;
            if ( existingAttendeesQuery == null )
            {
                allAttendees = prospectiveAttendeesQuery.ToList();
            }
            else if ( prospectiveAttendeesQuery == null )
            {
                allAttendees = existingAttendeesQuery.ToList();
            }
            else
            {
                // Get the prospective attendees query that excludes existing attendees.
                var prospectiveNewAttendeesQuery = prospectiveAttendeesQuery
                    .Where( newAttendee => !existingAttendeesQuery
                        .Select( existingattendee => existingattendee.PersonAliasId )
                        .Contains( newAttendee.PersonAliasId ) );

                // Execute the existing and prospective attendees queries individually and combine the attendees.
                allAttendees = existingAttendeesQuery.ToList().Concat( prospectiveNewAttendeesQuery.ToList() );
            }

            // The attendees query may contain multiple records for the same person
            // if they have multiple group roles (like leader and member).
            // Return a list of attendances, one per person, and include their groups and roles.
            return allAttendees
                .Select( p => GetAttendanceBag( p ) )
                .ToList();
        }

        /// <summary>
        /// Gets the bag containing the information needed to display an Attendance record in the Group Attendance Detail block.
        /// </summary>
        /// <param name="attendanceData">The Attendance record data.</param>
        /// <returns>The bag containing the information needed to display an Attendance record in the Group Attendance Detail block.</returns>
        private GroupAttendanceDetailAttendanceBag GetAttendanceBag( PersonAliasAttendanceBagDto attendanceData )
        {
            var mergeFields = this.RequestContext.GetCommonMergeFields();
            mergeFields.Add( MergeFieldKeys.Person, attendanceData.Person );
            mergeFields.Add( MergeFieldKeys.Attended, attendanceData.DidAttend );
#pragma warning disable CS0618 // Type or member is obsolete
            mergeFields.Add( MergeFieldKeys.GroupMember, attendanceData.GroupMembers?.FirstOrDefault() );
            mergeFields.Add( MergeFieldKeys.GroupRoleName, string.Join( ", ", attendanceData.Roles?.Distinct() ?? Enumerable.Empty<string>() ) );
#pragma warning restore CS0618 // Type or member is obsolete
            mergeFields.Add( MergeFieldKeys.GroupMembers, attendanceData.GroupMembers );
            mergeFields.Add( MergeFieldKeys.Roles, string.Join( ", ", attendanceData.Roles?.Distinct() ?? Enumerable.Empty<string>() ) );

            var itemTemplate = this.ListItemDetailsTemplate.ResolveMergeFields( mergeFields );

            return new GroupAttendanceDetailAttendanceBag
            {
                PersonGuid = attendanceData.Person.Guid,
                PersonAliasId = attendanceData.PersonAliasId,
                NickName = attendanceData.Person.NickName,
                LastName = attendanceData.Person.LastName,
                DidAttend = attendanceData.DidAttend,
                CampusGuid = attendanceData.PrimaryCampusGuid,
                ItemTemplate = itemTemplate
            };
        }

        /// <summary>
        /// Creates a new <see cref="Attendance"/> instance without adding to the data context.
        /// </summary>
        /// <param name="personAliasId">The PersonAlias ID.</param>
        /// <param name="campusId">The Campus ID.</param>
        /// <param name="startDateTime">The start DateTime.</param>
        /// <param name="didAttend">Whether or not the Person attended.</param>
        /// <returns>A new <see cref="Attendance"/> instance.</returns>
        private static Attendance CreateAttendanceInstance( int? personAliasId, int? campusId, DateTime startDateTime, bool? didAttend )
        {
            return new Attendance
            {
                PersonAliasId = personAliasId,
                CampusId = campusId,
                StartDateTime = startDateTime,
                DidAttend = didAttend
            };
        }

        /// <summary>
        /// Gets the Campus ID from the AttendanceOccurrence Location.
        /// <para>If not present, then try the Group's Campus.</para>
        /// <para>Finally, if not set there, get the Campus from the Campus filter, if present in the request.</para>
        /// </summary>
        private int? GetAttendanceCampusId( int? occurrenceLocationCampusId, int? groupLocationCampusId )
        {
            return occurrenceLocationCampusId ?? groupLocationCampusId;
        }

        #endregion

        #region Helper Classes and Enums

        /// <summary>
        /// Used for gathering <see cref="Attendance"/> data to create an instance of <see cref="GroupAttendanceDetailAttendanceBag"/>.
        /// </summary>
        private class PersonAliasAttendanceBagDto
        {
            public bool? DidAttend { get; set; }

            public Person Person { get; set; }

            public Guid? PrimaryCampusGuid { get; set; }

            public List<GroupMember> GroupMembers { get; set; }

            public int PersonAliasId { get; set; }

            public List<string> Roles { get; set; }
        }

        /// <summary>
        /// A helper for retrieving and saving <see cref="AttendanceOccurrence"/> data for the Group Attendance Detail block.
        /// </summary>
        private class OccurrenceDataClientService
        {
            private readonly GroupAttendanceDetail _block;
            private readonly RockContext _rockContext;
            private readonly AttendanceOccurrenceService _attendanceOccurrenceService;
            private readonly AttendanceService _attendanceService;
            private readonly GroupService _groupService;
            private readonly GroupMemberService _groupMemberService;
            private readonly GroupTypeRoleService _groupTypeRoleService;
            private readonly LocationService _locationService;
            private readonly PersonService _personService;
            private readonly PersonAliasService _personAliasService;
            private readonly ScheduleService _scheduleService;

            internal OccurrenceDataClientService( GroupAttendanceDetail block, RockContext rockContext )
            {
                _block = block ?? throw new ArgumentNullException( nameof( block ) );
                _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
                _attendanceService = new AttendanceService( rockContext );
                _attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                _groupService = new GroupService( rockContext );
                _groupMemberService = new GroupMemberService( rockContext );
                _groupTypeRoleService = new GroupTypeRoleService( rockContext );
                _locationService = new LocationService( rockContext );
                _personAliasService = new PersonAliasService( rockContext );
                _personService = new PersonService( rockContext );
                _scheduleService = new ScheduleService( rockContext );
            }

            private bool ShouldAddPersonAsGroupMember => string.Equals( _block.AddPersonAs, "Group Member", StringComparison.OrdinalIgnoreCase );

            /// <summary>
            /// Adds the person as group member without saving.
            /// </summary>
            /// <param name="person">The person.</param>
            /// <param name="rockContext">The rock context.</param>
            internal GroupMember AddPersonAsGroupMember( OccurrenceData occurrenceData, GroupAttendanceDetailAddPersonRequestBag bag )
            {
                var group = occurrenceData.Group;

                if ( group == null )
                {
                    occurrenceData.ErrorMessage = "Group not found.";
                    return null;
                }

                var person = _personAliasService.GetPerson( bag.PersonAliasGuid );

                if ( person == null )
                {
                    occurrenceData.ErrorMessage = "Person not found.";
                    return null;
                }

                // Check to see if the person is already a member of the group/role.
                var existingGroupMember = _groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( group.Id, person.Id, group.GroupType.DefaultGroupRoleId ?? 0 );

                if ( existingGroupMember != null )
                {
                    return existingGroupMember;
                }

                var role = _groupTypeRoleService.Get( group.GroupType.DefaultGroupRoleId ?? 0 );

                var groupMember = new GroupMember
                {
                    Id = 0,
                    GroupId = group.Id,
                    PersonId = person.Id,
                    GroupRoleId = role.Id,
                    GroupMemberStatus = GroupMemberStatus.Active
                };

                _groupMemberService.Add( groupMember );

                return groupMember;
            }

            /// <summary>
            /// Marks attendance without saving.
            /// </summary>
            /// <param name="occurrenceData">The occurrence data.</param>
            /// <param name="bag">The bag.</param>
            /// <returns></returns>
            internal bool UpdateAttendance( OccurrenceData occurrenceData, GroupAttendanceDetailMarkAttendanceRequestBag bag )
            {
                if ( !occurrenceData.IsValid )
                {
                    return false;
                }

                var attendance = occurrenceData.AttendanceOccurrence.Attendees.FirstOrDefault( a => a.PersonAliasId == bag.PersonAliasId );

                if ( attendance == null )
                {
                    var personAliasId = bag.PersonAliasId;

                    DateTime startDateTime;

                    if ( occurrenceData.AttendanceOccurrence.Schedule != null
                        && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() )
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add(
                            occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay );
                    }
                    else
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate;
                    }

                    occurrenceData.AttendanceOccurrence.Attendees.Add( CreateAttendanceInstance( personAliasId, occurrenceData.Campus?.Id, startDateTime, bag.DidAttend ) );
                }
                else
                {
                    attendance.DidAttend = bag.DidAttend;
                }

                return true;
            }

            internal OccurrenceData GetOccurrenceData( AttendanceOccurrenceSearchParameters searchParameters, bool withTracking = false )
            {
                var occurrenceData = new OccurrenceData()
                {
                    Group = searchParameters.Group
                };

                if ( occurrenceData.IsAuthorizedGroupNotFoundError )
                {
                    // Short-circuit if the authorized Group was not found.
                    return occurrenceData;
                }

                // Set the AttendanceOccurrence.
                SetAttendanceOccurrence( occurrenceData, searchParameters, withTracking );

                if ( occurrenceData.IsNoAttendanceOccurrencesError )
                {
                    // Short-circuit if the AttendanceOccurrence was not found.
                    return occurrenceData;
                }

                // Set the Campus.
                occurrenceData.Campus = GetOccurrenceCampus( occurrenceData.Group, occurrenceData.AttendanceOccurrence );

                return occurrenceData;
            }

            /// <summary>
            /// Gets the attendance occurrence search parameters.
            /// </summary>
            /// <param name="group">The group.</param>
            /// <param name="searchParameterOverrides">The search parameter overrides. These are only applied if new attendance is allowed.</param>
            /// <returns></returns>
            internal AttendanceOccurrenceSearchParameters GetAttendanceOccurrenceSearchParameters( Model.Group group, Guid? attendanceOccurrenceGuid = null, Action<AttendanceOccurrenceSearchParameters> searchParameterOverrides = null )
            {
                // Get defaults.
                var occurrenceDataSearchParameters = new AttendanceOccurrenceSearchParameters
                {
                    AttendanceOccurrenceDate = ( _block.DatePageParameter ?? _block.OccurrencePageParameter ),
                    AttendanceOccurrenceGuid = attendanceOccurrenceGuid,
                    AttendanceOccurrenceId = _block.OccurrenceIdPageParameter,
                    Group = group,
                    LocationId = _block.LocationIdPageParameter,
                    ScheduleId = _block.ScheduleIdPageParameter ?? group?.ScheduleId,
                };

                occurrenceDataSearchParameters.IsSpecificSearch =
                    ( _block.DatePageParameter ?? _block.OccurrencePageParameter ).HasValue
                    || _block.LocationIdPageParameter.HasValue
                    || _block.ScheduleIdPageParameter.HasValue
                    || attendanceOccurrenceGuid.HasValue
                    || _block.OccurrenceIdPageParameter.HasValue;

                // If overrides are allowed, then use the overrides.
                if ( searchParameterOverrides != null && !_block.IsNewAttendanceDateAdditionRestricted )
                {
                    // Apply search parameter overrides.
                    searchParameterOverrides( occurrenceDataSearchParameters );
                }

                return occurrenceDataSearchParameters;
            }

            internal bool Save( OccurrenceData occurrenceData, GroupAttendanceDetailGetOrCreateRequestBag bag )
            {
                if ( occurrenceData.IsValid != true )
                {
                    return false;
                }

                if ( occurrenceData.IsReadOnly )
                {
                    // If an occurrence already exists but not by the supplied OccurrenceID,
                    // then return an error to the client.
                    occurrenceData.ErrorMessage = "An occurrence already exists for this group for the selected date, location, and schedule that you've selected. Please return to the list and select that occurrence to update it's attendance.";
                    return false;
                }

                if ( occurrenceData.IsNewOccurrence )
                {
                    if ( occurrenceData.AttendanceOccurrence.OccurrenceDate.IsFuture() && _block.IsFutureOccurrenceDateSelectionRestricted )
                    {
                        occurrenceData.ErrorMessage = "Future dates are not allowed";
                        return false;
                    }

                    _attendanceOccurrenceService.Add( occurrenceData.AttendanceOccurrence );
                }
                else
                {
                    _attendanceOccurrenceService.Attach( occurrenceData.AttendanceOccurrence );
                }

                if ( !_block.IsNotesSectionHidden && bag.UpdatedNotes != null )
                {
                    occurrenceData.AttendanceOccurrence.Notes = bag.UpdatedNotes;
                }

                if ( bag.UpdatedDidNotOccur.HasValue )
                {
                    occurrenceData.AttendanceOccurrence.DidNotOccur = bag.UpdatedDidNotOccur;
                }

                // Set the attendance type.
                if ( bag.UpdatedAttendanceOccurrenceTypeGuid.HasValue )
                {
                    var allowedAttendanceTypes = _block.AttendanceOccurrenceTypeValues;
                    var attendanceTypeDefinedValue = allowedAttendanceTypes.FirstOrDefault( a => a.Guid == bag.UpdatedAttendanceOccurrenceTypeGuid.Value );
                    occurrenceData.AttendanceOccurrence.AttendanceTypeValueId = attendanceTypeDefinedValue?.Id;
                }

                // Add new attendee/group member.
                if ( bag.AddedPersonAliasGuid.HasValue )
                {
                    GroupMember groupMember = null;
                    if ( this.ShouldAddPersonAsGroupMember )
                    {
                        groupMember = AddPersonAsGroupMember( occurrenceData, new GroupAttendanceDetailAddPersonRequestBag
                        {
                            PersonAliasGuid = bag.AddedPersonAliasGuid.Value
                        } );
                    }

                    if ( bag.UpdatedAttendances == null )
                    {
                        bag.UpdatedAttendances = new List<GroupAttendanceDetailMarkAttendanceRequestBag>();
                    }

                    // Update if an attendance record already exists.
                    var personAliasId = _personAliasService.GetId( bag.AddedPersonAliasGuid.Value );
                    var existingAttendance = bag.UpdatedAttendances.FirstOrDefault( a => a.PersonAliasId == personAliasId );

                    if ( existingAttendance != null )
                    {
                        existingAttendance.DidAttend = true;
                    }
                    else if ( personAliasId.HasValue )
                    {
                        bag.UpdatedAttendances.Add( new GroupAttendanceDetailMarkAttendanceRequestBag
                        {
                            DidAttend = true,
                            PersonAliasId = personAliasId.Value,
                        } );
                    }
                }

                // Update the attendees.
                var existingAttendees = occurrenceData.AttendanceOccurrence.Attendees.ToList();

                var campusId = new Lazy<int?>( () =>
                {
                    var occurrenceLocationCampusId = _locationService.GetCampusIdForLocation( occurrenceData.AttendanceOccurrence.LocationId );
                    return _block.GetAttendanceCampusId( occurrenceLocationCampusId, occurrenceData.Group.CampusId );
                } );

                if ( bag.UpdatedDidNotOccur == true )
                {
                    // If did not meet was selected and this was a manually entered occurrence (not based on a schedule/location)
                    // then just delete all the attendance records instead of tracking a 'did not meet' value
                    if ( !occurrenceData.AttendanceOccurrence.ScheduleId.HasValue )
                    {
                        foreach ( var attendance in existingAttendees )
                        {
                            _attendanceService.Delete( attendance );
                        }
                    }
                    // If the occurrence is based on a schedule and there are attendees,
                    // then set the did not meet flags on existing attendees.
                    else if ( existingAttendees.Any() )
                    {
                        foreach ( var attendance in existingAttendees )
                        {
                            attendance.DidAttend = null;
                        }
                    }
                    // If the occurrence is based on a schedule and there are no attendees,
                    // then add new attendees with did not meet flags set.
                    else if ( bag.UpdatedAttendances?.Any() == true )
                    {
                        foreach ( var attendee in bag.UpdatedAttendances )
                        {
                            var attendance = CreateAttendanceInstance(
                                attendee.PersonAliasId,
                                campusId.Value,
                                occurrenceData.AttendanceOccurrence.Schedule != null && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() ? occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add( occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay ) : occurrenceData.AttendanceOccurrence.OccurrenceDate,
                                false );

                            if ( !attendance.IsValid )
                            {
                                occurrenceData.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                                return false;
                            }

                            occurrenceData.AttendanceOccurrence.Attendees.Add( attendance );
                        }
                    }
                }
                else if ( bag.UpdatedAttendances?.Any() == true )
                {
                    // Validate the AttendanceOccurrence before updating the Attendance records.
                    if ( !occurrenceData.AttendanceOccurrence.IsValid )
                    {
                        occurrenceData.ErrorMessage = occurrenceData.AttendanceOccurrence.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                        return false;
                    }

                    DateTime startDateTime;

                    if ( occurrenceData.AttendanceOccurrence.Schedule != null
                        && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() )
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add(
                            occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay );
                    }
                    else
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate;
                    }

                    foreach ( var attendee in bag.UpdatedAttendances )
                    {
                        var attendance = existingAttendees
                            .Where( a => a.PersonAliasId == attendee.PersonAliasId )
                            .FirstOrDefault();

                        if ( attendance == null )
                        {
                            attendance = CreateAttendanceInstance(
                                attendee.PersonAliasId,
                                campusId.Value,
                                startDateTime,
                                attendee.DidAttend ?? false );

                            // Check that the attendance record is valid
                            if ( !attendance.IsValid )
                            {
                                occurrenceData.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                                return false;
                            }

                            occurrenceData.AttendanceOccurrence.Attendees.Add( attendance );
                        }
                        else if ( attendee.DidAttend.HasValue )
                        {
                            // Otherwise, only record that they attended -- don't change their attendance startDateTime.
                            attendance.DidAttend = attendee.DidAttend.Value;
                        }
                    }
                }

                _rockContext.SaveChanges();

                return true;
            }

            /// <summary>
            /// Gets the occurrence campus.
            /// </summary>
            /// <param name="group">The group.</param>
            /// <param name="occurrence">The occurrence.</param>
            /// <returns>The occurrence campus.</returns>
            private CampusCache GetOccurrenceCampus( Model.Group group, AttendanceOccurrence occurrence )
            {
                var campusId = _locationService.GetCampusIdForLocation( occurrence.LocationId ) ?? group.CampusId;

                if ( campusId.HasValue )
                {
                    return CampusCache.Get( campusId.Value );
                }

                return null;
            }

            /// <summary>
            /// Gets the occurrence items.
            /// </summary>
            private void SetAttendanceOccurrence( OccurrenceData occurrenceData, AttendanceOccurrenceSearchParameters attendanceOccurrenceSearchParameters, bool withTracking = false )
            {
                AttendanceOccurrence attendanceOccurrence = null;
                occurrenceData.AttendanceOccurrence = null;

                var baseQuery = _attendanceOccurrenceService
                    .AsNoFilter()
                    .Include( a => a.Schedule )
                    .Include( a => a.Location )
                    .Include( a => a.Attendees );

                // Check if an occurrence guid was specified, and if so, query for it.
                if ( attendanceOccurrenceSearchParameters.AttendanceOccurrenceGuid.HasValue )
                {
                    var query = baseQuery.Where( o => o.Guid == attendanceOccurrenceSearchParameters.AttendanceOccurrenceGuid );

                    if ( !withTracking )
                    {
                        query = query.AsNoTracking();
                    }

                    attendanceOccurrence = query.FirstOrDefault();

                    // If we have a valid occurrence return it now (the date, location, schedule cannot be changed for an existing occurrence).
                    if ( attendanceOccurrence != null )
                    {
                        occurrenceData.IsSpecificOccurrence = attendanceOccurrenceSearchParameters.IsSpecificSearch;
                        occurrenceData.AttendanceOccurrence = attendanceOccurrence;
                        return;
                    }
                }

                // Check if an occurrence ID was specified on the query string, and if so, query for it.
                if ( attendanceOccurrenceSearchParameters.AttendanceOccurrenceId.HasValue && attendanceOccurrenceSearchParameters.AttendanceOccurrenceId.Value > 0 )
                {
                    var query = baseQuery.Where( o => o.Id == attendanceOccurrenceSearchParameters.AttendanceOccurrenceId.Value );

                    if ( !withTracking )
                    {
                        query = query.AsNoTracking();
                    }

                    attendanceOccurrence = query.FirstOrDefault();

                    // If we have a valid occurrence return it now (the date, location, schedule cannot be changed for an existing occurrence).
                    if ( attendanceOccurrence != null )
                    {
                        occurrenceData.IsSpecificOccurrence = attendanceOccurrenceSearchParameters.IsSpecificSearch;
                        occurrenceData.AttendanceOccurrence = attendanceOccurrence;
                        return;
                    }
                }

                var occurrenceDate = attendanceOccurrenceSearchParameters.AttendanceOccurrenceDate ?? RockDateTime.Today;
                var locationId = attendanceOccurrenceSearchParameters.LocationId;
                var scheduleId = attendanceOccurrenceSearchParameters.ScheduleId;
                var group = attendanceOccurrenceSearchParameters.Group;

                // If no specific occurrenceId was specified, try to find a matching occurrence from Date, GroupId, Location, ScheduleId.
                var occurrenceQuery = baseQuery
                    .Where( o => o.OccurrenceDate == occurrenceDate )
                    .Where( o => o.GroupId.HasValue && o.GroupId.Value == group.Id );

                occurrenceQuery = locationId.HasValue ?
                    occurrenceQuery.Where( o => o.LocationId.HasValue && o.LocationId.Value == locationId.Value ) :
                    occurrenceQuery.Where( o => !o.LocationId.HasValue );

                occurrenceQuery = scheduleId.HasValue ?
                    occurrenceQuery.Where( o => o.ScheduleId.HasValue && o.ScheduleId.Value == scheduleId.Value ) :
                    occurrenceQuery.Where( o => !o.ScheduleId.HasValue );

                if ( !withTracking )
                {
                    occurrenceQuery = occurrenceQuery.AsNoTracking();
                }

                attendanceOccurrence = occurrenceQuery.FirstOrDefault();

                if ( attendanceOccurrence != null )
                {
                    occurrenceData.IsSpecificOccurrence = attendanceOccurrenceSearchParameters.IsSpecificSearch;
                    occurrenceData.AttendanceOccurrence = attendanceOccurrence;
                    return;
                }

                // If an occurrence date was included, but no occurrence was found with that date, and new
                // occurrences can be added, then create a new one.
                if ( !this._block.IsNewAttendanceDateAdditionRestricted )
                {
                    var locationQuery = _locationService
                        .AsNoFilter()
                        .Where( l => l.Id == locationId );

                    if ( !withTracking )
                    {
                        locationQuery = locationQuery.AsNoTracking();
                    }

                    var location = locationQuery.FirstOrDefault();

                    var scheduleQuery = _scheduleService
                        .AsNoFilter()
                        .Where( s => s.Id == scheduleId );

                    if ( !withTracking )
                    {
                        scheduleQuery = scheduleQuery.AsNoTracking();
                    }

                    var schedule = scheduleQuery.FirstOrDefault();

                    // Create a new occurrence record and return it
                    attendanceOccurrence = new AttendanceOccurrence
                    {
                        GroupId = group.Id,
                        OccurrenceDate = occurrenceDate,
                        LocationId = location?.Id,
                        Location = location,
                        ScheduleId = schedule?.Id,
                        Schedule = schedule
                    };

                    if ( withTracking )
                    {
                        _attendanceOccurrenceService.Add( attendanceOccurrence );
                    }

                    occurrenceData.IsSpecificOccurrence = attendanceOccurrenceSearchParameters.IsSpecificSearch;
                    occurrenceData.AttendanceOccurrence = attendanceOccurrence;
                    return;
                }
            }

            /// <summary>
            /// Gets the group by GroupId page parameter if the current person is authorized to manage group members or edit the group; otherwise, null is returned.
            /// </summary>
            /// <param name="rockContext">The rock context.</param>
            /// <returns>The group associated with the GroupId page parameter if the current person is authorized; otherwise, <c>null</c>.</returns>
            internal Model.Group GetGroupIfAuthorized( bool withTracking = false )
            {
                var query = _groupService
                        .AsNoFilter()
                        .Include( g => g.GroupType )
                        .Include( g => g.Schedule );

                var groupId = this._block.GroupIdPageParameter.AsIntegerOrNull();
                var groupGuid = this._block.GroupIdPageParameter.AsGuidOrNull();

                if ( groupId.HasValue )
                {
                    query = query.Where( g => g.Id == groupId.Value );
                }
                else if ( groupGuid.HasValue )
                {
                    query = query.Where( g => g.Guid == groupGuid.Value );
                }
                else
                {
                    // The GroupId page parameter is not an integer ID
                    // nor a guid ID so return null.
                    return null;
                }

                if ( !withTracking )
                {
                    query = query.AsNoTracking();
                }

                var group = query.FirstOrDefault();

                if ( group == null )
                {
                    return null;
                }

                var currentPerson = this._block.GetCurrentPerson();

                if ( !group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson ) && !group.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return null;
                }

                return group;
            }
        }

        /// <summary>
        /// Represents <see cref="AttendanceOccurrence"/> data in a shape that is useful for this Group Attendance Detail block.
        /// </summary>
        private class OccurrenceData
        {
            public AttendanceOccurrence AttendanceOccurrence { get; internal set; }

            public CampusCache Campus { get; internal set; }
            
            public string ErrorMessage { get; set; }

            public Model.Group Group { get; internal set; }

            public bool IsValid => ErrorMessage.IsNullOrWhiteSpace()
                && !IsAuthorizedGroupNotFoundError
                && !IsNoAttendanceOccurrencesError
                && Group != null
                && AttendanceOccurrence != null;

            public bool IsAuthorizedGroupNotFoundError => Group == null;

            public bool IsNewOccurrence => AttendanceOccurrence?.Id == 0;

            public bool IsSpecificOccurrence { get; set; }

            public bool IsNoAttendanceOccurrencesError => !IsAuthorizedGroupNotFoundError && AttendanceOccurrence == null;

            public bool IsReadOnly { get; internal set; }
        }

        /// <summary>
        /// Represents search parameters for finding an existing <see cref="AttendanceOccurrence"/>.
        /// </summary>
        private class AttendanceOccurrenceSearchParameters
        {
            /// <summary>
            /// Gets or sets the attendance occurrence unique identifier.
            /// <para>If set to an existing Attendance Occurrence Guid, then no other search parameters are necessary.</para>
            /// </summary>
            public Guid? AttendanceOccurrenceGuid { get; internal set; }

            /// <summary>
            /// Gets or sets the attendance occurrence identifier.
            /// <para>If set to an existing Attendance Occurrence ID, then no other search parameters are necessary.</para>
            /// </summary>
            public int? AttendanceOccurrenceId { get; internal set; }

            public Model.Group Group { get; set; }

            public DateTime? AttendanceOccurrenceDate { get; set; }

            public int? LocationId { get; set; }

            public int? ScheduleId { get; set; }

            /// <summary>
            /// If true, then the individual is trying to load a specific occurrence via page parameters by id, guid, date, location, and/or schedule.
            /// </summary>
            public bool IsSpecificSearch { get; internal set; }
        }

        /// <summary>
        /// Allows the block to be configured to select the date for the attendance in several ways.
        /// </summary>
        private enum DateSelectionModeSpecifier
        {
            /// <summary>
            /// Individual can pick any date.
            /// </summary>
            DatePicker = 1,

            /// <summary>
            /// Locked to the current date.
            /// </summary>
            CurrentDate = 2,

            /// <summary>
            /// Drop down of dates from the schedule. This will need to be updated based on the location.
            /// </summary>
            PickFromSchedule = 3
        }

        private class GroupMemberDto
        {
            public GroupMember GroupMember { get; internal set; }
            public string GroupRoleName { get; internal set; }
        }

        private class LocationDto
        {
            public int Id { get; internal set; }
            public Guid Guid { get; internal set; }
            public string Name { get; internal set; }
            public int? ParentLocationId { get; internal set; }
            public Guid? ParentLocationGuid { get; internal set; }
        }

        private class ScheduleDto
        {
            public Guid ScheduleGuid { get; internal set; }
            public string ScheduleName { get; internal set; }
        }

        #endregion
    }
}

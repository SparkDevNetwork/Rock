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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Blocks.Groups.GroupAttendanceDetail;
using Rock.Logging;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Security;
using Rock.ViewModels.Blocks.Groups.GroupAttendanceDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Groups
{
    [DisplayName( "Group Attendance Detail" )]
    [Category( "Obsidian > Groups" )]
    [Description( "Lists the group members for a specific occurrence date time and allows selecting if they attended or not." )]

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

    [WorkflowTypeField(
        "Workflow",
        AllowMultiple = false,
        Category = AttributeCategory.None,
        Description = "An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.",
        IsRequired = false,
        Key = AttributeKey.Workflow,
        Order = 5 )]

    [MergeTemplateField(
        "Attendance Roster Template",
        Category = AttributeCategory.None,
        IsRequired = false,
        Key = AttributeKey.AttendanceRosterTemplate,
        Order = 6 )]

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
        Order = 7 )]

    [BooleanField(
        "Restrict Future Occurrence Date",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Should user be prevented from selecting a future Occurrence date?",
        IsRequired = false,
        Key = AttributeKey.RestrictFutureOccurrenceDate,
        Order = 8 )]

    [BooleanField(
        "Show Notes",
        Category = AttributeCategory.None,
        DefaultBooleanValue = true,
        Description = "Should the notes field be displayed?",
        IsRequired = false,
        Key = AttributeKey.ShowNotes,
        Order = 9 )]

    [TextField(
        "Attendance Note Label",
        Category = AttributeCategory.Labels,
        DefaultValue = "Notes",
        Description = "The text to use to describe the notes",
        IsRequired = true,
        Key = AttributeKey.AttendanceNoteLabel,
        Order = 10 )]

    [DefinedValueField(
        "Configured Attendance Types",
        AllowMultiple = true,
        Category = AttributeCategory.None,
        DefaultValue = "",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CHECK_IN_ATTENDANCE_TYPES,
        Description = "The Attendance types that an occurrence can have. If no or one Attendance type is selected, then none will be shown.",
        IsRequired = false,
        Key = AttributeKey.AttendanceOccurrenceTypes,
        Order = 11 )]

    [TextField(
        "Attendance Type Label",
        Category = AttributeCategory.Labels,
        DefaultValue = "Attendance Location",
        Description = "The label that will be shown for the attendance types section.",
        IsRequired = false,
        Key = AttributeKey.AttendanceOccurrenceTypesLabel,
        Order = 12 )]

    [BooleanField(
        "Disable Long-List",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Will disable the long-list feature which groups individuals by the first character of their last name. When enabled, this only shows when there are more than 50 individuals on the list.",
        IsRequired = false,
        Key = AttributeKey.DisableLongList,
        Order = 13 )]

    [BooleanField(
        "Disable Did Not Meet",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Allows for hiding the flag that the group did not meet.",
        IsRequired = false,
        Key = AttributeKey.DisableDidNotMeet,
        Order = 14 )]

    [BooleanField(
        "Hide Back Button",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Will hide the back button from the bottom of the block.",
        IsRequired = false,
        Key = AttributeKey.HideBackButton,
        Order = 15 )]

    [EnumField(
        "Date Selection Mode",
        Category = AttributeCategory.None,
        DefaultEnumValue = ( int )DateSelectionModeSpecifier.DatePicker,
        Description = "'Date Picker' individual can pick any date. 'Current Date' locked to the current date. 'Pick From Schedule' drop down of dates from the schedule. This will need to be updated based on the location.",
        EnumSourceType = typeof( DateSelectionModeSpecifier ),
        IsRequired = true,
        Key = AttributeKey.DateSelectionMode,
        Order = 16 )]

    [IntegerField(
        "Number of Previous Days To Show",
        Category = AttributeCategory.None,
        DefaultIntegerValue = 14,
        Description = "When the 'Pick From Schedule' option is used, this setting will control how many days back appear in the drop down list to choose from.",
        IsRequired = false,
        Key = AttributeKey.NumberOfPreviousDaysToShow,
        Order = 17 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "64ECB2E0-218F-4EB4-8691-7DC94A767037" )]
    [Rock.SystemGuid.BlockTypeGuid( "308DBA32-F656-418E-A019-9D18235027C1" )]
    public class GroupAttendanceDetail : RockObsidianBlockType
    {
        #region Attribute Values

        private const string DefaultListItemDetailsTemplate = @"<div style=""display: flex; align-items: center; gap: 8px; padding: 12px;"">
    <img width=""80px"" height=""80px"" src=""{{ Person.PhotoUrl }}"" style=""border-radius: 80px; width: 80px; height: 80px"" />
    <div>
        <strong>{{ Person.LastName }}, {{ Person.NickName }}</strong>
        {% if GroupRoleName %}<div>{{ GroupRoleName }}</div>{% endif %}
        {% if GroupMember.GroupMemberStatus and GroupMember.GroupMemberStatus != 'Active' %}<span class=""label label-info"" style=""position: absolute; right: 10px; top: 10px;"">{{ GroupMember.GroupMemberStatus }}</span>{% endif %}
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
            public const string Workflow = "Workflow";
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
        private static class UserPreferenceKeys
        {
            public const string AreGroupAttendanceAttendeesSortedByFirstName = "Attendance_List_Sorting_Toggle";
            public const string Campus = "Campus";
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

            public const string GroupMember = "GroupMember";

            public const string GroupRoleName = "GroupRoleName";

            public const string Person = "Person";
        }

        #endregion

        #region Properties

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
        /// An optional workflow type to launch whenever attendance is saved.
        /// <para>The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.</para>
        /// </summary>
        private Guid? WorkflowGuid => GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();

        /// <summary>
        /// An optional workflow type to launch whenever attendance is saved.
        /// <para>The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.</para>
        /// </summary>
        /// <value>
        /// The workflow cache.
        /// </value>
        private WorkflowTypeCache WorkflowType
        {
            get
            {
                var guid = this.WorkflowGuid;

                if ( guid.HasValue )
                {
                    return WorkflowTypeCache.Get( guid.Value );
                }

                return null;
            }
        }

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
        private int? GroupIdPageParameter => PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();

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
        private int? EntitySetIdPageParameter => PageParameter( PageParameterKey.EntitySetId ).AsIntegerOrNull();

        #endregion

        #region User Preferences

        /// <summary>
        /// The Campus ID filter.
        /// </summary>
        private int? CampusIdBlockUserPreference
        {
            get
            {
                return GetCurrentUserPreferenceForBlock( UserPreferenceKeys.Campus ).AsIntegerOrNull();
            }
            set
            {
                SetCurrentUserPreferenceForBlock( UserPreferenceKeys.Campus, value.ToString() );
            }
        }

        #endregion

        #endregion

        #region IRockObsidianBlockType Implementation

        /// <inheritdoc/>
        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceDataClientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = occurrenceDataClientService.GetAttendanceOccurrenceSearchParameters( campusIdOverride: this.CampusIdBlockUserPreference );
                var occurrenceData = occurrenceDataClientService.GetOccurrenceData( searchParameters, asNoTracking: false );
                var box = new GroupAttendanceDetailInitializationBox();

                if ( !occurrenceData.IsValid )
                {
                    SetErrorData( occurrenceData, box );
                    return box;
                }

                if ( occurrenceData.IsValid && occurrenceData.IsNewOccurrence && occurrenceData.AttendanceOccurrence.IsValid )
                {
                    var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                    attendanceOccurrenceService.Add( occurrenceData.AttendanceOccurrence );

                    // Add the AttendanceOccurrence if it is new and valid.
                    rockContext.SaveChanges();
                } 

                SetInitializationBox( rockContext, occurrenceData, box );

                return box;
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
                var attendanceInfo = new AttendanceService( rockContext )
                    .Queryable()
                    .Where( a =>  a.Guid == bag.AttendanceGuid )
                    .Select( a => new
                    {
                        GroupId = a.Occurrence.GroupId,
                        Person = a.PersonAlias.Person,
                        DidAttend = a.DidAttend,
                        PersonAliasId = a.PersonAliasId
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

                if (!group.IsAuthorized(Authorization.VIEW, GetCurrentPerson() ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.Forbidden );
                }

                // The attendee may not be a member of the group.
                var groupMemberDto = new GroupMemberService( rockContext )
                    .Queryable()
                    .Where( m =>
                        m.GroupId == group.Id
                        && m.GroupId == group.Id
                        && m.PersonId == attendanceInfo.Person.Id )
                    .Select( m => new
                    {
                        GroupMember = m,
                        GroupRoleName = m.GroupRole != null ? m.GroupRole.Name : null,
                    } )
                    .FirstOrDefault();

                var attendanceBag = GetAttendanceBag( new AttendanceData
                {
                    DidAttend = attendanceInfo.DidAttend ?? false,
                    GroupMember = groupMemberDto?.GroupMember,
                    GroupRoleName = groupMemberDto?.GroupRoleName,
                    Person = attendanceInfo.Person,
                    PersonAliasId = attendanceInfo.PersonAliasId,
                    PrimaryCampusGuid = attendanceInfo.Person.PrimaryCampusId.HasValue ? attendanceInfo.Person.PrimaryCampus.Guid : ( Guid? ) null
                } );

                return ActionOk( attendanceBag );
            }
        }

        /// <summary>
        /// Downloads the AttendanceOccurrence roster.
        /// </summary>
        [BlockAction( "PrintRoster" )]
        public BlockActionResult PrintRoster()
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );

                // Use the default search parameters so we only print the persisted AttendanceOccurrence.
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters();
                var occurrenceData = clientService.GetOccurrenceData( searchParameters, asNoTracking: true );

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
                        mergeObjects.AddOrIgnore( person.Id, person );
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

                // Set the name of the output doc.
                outputBinaryFileDoc = new BinaryFileService( rockContext ).Get( outputBinaryFileDoc.Id );
                outputBinaryFileDoc.FileName = occurrenceData.Group.Name + " Attendance Roster" + Path.GetExtension( outputBinaryFileDoc.FileName ?? string.Empty ) ?? ".docx";
                rockContext.SaveChanges();

                if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
                {
                    if ( mergeTemplateType.Exceptions.Count == 1 )
                    {
                        RockLogger.Log.Error(RockLogDomains.Group, mergeTemplateType.Exceptions[0] );
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
                    RedirectUrl = $"{this.RequestContext.RootUrlPath}/GetFile.ashx?Guid={outputBinaryFileDoc.Guid}&attachment=true"
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
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( attendanceOccurrenceGuidOverride: bag.AttendanceOccurrenceGuid );
                var occurrenceData = new OccurrenceData();

                if ( !clientService.TrySetGroup( occurrenceData, searchParameters, asNoTracking: true ) )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                var person = new PersonAliasService( rockContext ).GetPerson( bag.PersonAliasGuid );

                if ( person == null )
                {
                    return ActionBadRequest( "Person not found." );
                }

                var addPersonAs = this.AddPersonAs;

                GroupMember groupMember = null;

                if ( !addPersonAs.IsNullOrWhiteSpace() && addPersonAs == "Group Member" )
                {
                    groupMember = AddPersonAsGroupMemberWithoutSaving( occurrenceData.Group, person, rockContext );
                }

                var campusGuid = person.PrimaryCampusId.HasValue ? person.PrimaryCampus.Guid : ( Guid? ) null;

                MarkAttendance( new GroupAttendanceDetailMarkAttendanceRequestBag
                {
                    AttendanceOccurrenceGuid = bag.AttendanceOccurrenceGuid,
                    CampusGuid = campusGuid,
                    DidAttend = true,
                    PersonGuid = person.Guid,
                } );

                var attendanceBag = GetAttendanceBag( new AttendanceData
                {
                    DidAttend = true,
                    GroupMember = groupMember,
                    GroupRoleName = groupMember?.GroupRole != null ? groupMember.GroupRole.Name : null,
                    Person = person,
                    PersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasQuery().Where( a => a.PersonId == person.Id ).Select( a => a.Id ).FirstOrDefault(),
                    PrimaryCampusGuid =  campusGuid
                } );

                return ActionOk( new GroupAttendanceDetailAddPersonResponseBag
                {
                    Attendance = attendanceBag
                } );
            }
        }

        /// <summary>
        /// Gets or creates a new AttendanceOccurrence record and returns the data needed to display it in the Group Attendance Detail block.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        /// <returns>The data needed to display the AttendanceOccurrence in the Group Attendance Detail block.</returns>
        [BlockAction("GetOrCreate")]
        public BlockActionResult GetOrCreate( GroupAttendanceDetailGetOrCreateRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( bag );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                if ( occurrenceData.IsNewOccurrence )
                {
                    var result = clientService.Save( occurrenceData, bag );

                    if ( !result )
                    {
                        return ActionBadRequest( occurrenceData.ErrorMessage );
                    }

                    rockContext.SaveChanges();
                }
                else
                {
                    // If this is an existing AttendanceOccurrence, and if Location and Schedule were passed in, and if Location and Schedule are different than the AttendanceOccurrence,
                    // then update its Location and Schedule.
                    var isUpdateNeeded = false;

                    if ( searchParameters.LocationId.HasValue && occurrenceData.AttendanceOccurrence.LocationId != searchParameters.LocationId )
                    {
                        occurrenceData.AttendanceOccurrence.LocationId = searchParameters.LocationId;
                        isUpdateNeeded = true;
                    }

                    if ( searchParameters.ScheduleId.HasValue && occurrenceData.AttendanceOccurrence.ScheduleId != searchParameters.ScheduleId )
                    {
                        occurrenceData.AttendanceOccurrence.ScheduleId = searchParameters.ScheduleId;
                        isUpdateNeeded = true;
                    }

                    if ( isUpdateNeeded )
                    {
                        rockContext.SaveChanges();
                    }
                }

                var box = new GroupAttendanceDetailInitializationBox();

                SetInitializationBox( rockContext, occurrenceData, box );

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
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( attendanceOccurrenceGuidOverride: bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                var attendance = occurrenceData.AttendanceOccurrence.Attendees.FirstOrDefault( a => a.PersonAlias?.Person?.Guid == bag.PersonGuid );

                if ( attendance == null )
                {
                    var personAliasId = new PersonService( rockContext ).Get( bag.PersonGuid )?.PrimaryAliasId;
                    var occurrenceLocationCampusId = new LocationService( rockContext ).GetCampusIdForLocation( occurrenceData.AttendanceOccurrence.LocationId );
                    var campusId = GetAttendanceCampusId( occurrenceLocationCampusId, occurrenceData.Group.CampusId, bag.CampusGuid );

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

                    occurrenceData.AttendanceOccurrence.Attendees.Add( CreateAttendanceInstance( personAliasId, campusId, startDateTime, bag.DidAttend ) );
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
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( attendanceOccurrenceGuidOverride: bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

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
                    var occurrenceLocationCampusId = new LocationService( rockContext ).GetCampusIdForLocation( occurrenceData.AttendanceOccurrence.LocationId );
                    var campusId = GetAttendanceCampusId( occurrenceLocationCampusId, occurrenceData.Group.CampusId, bag.CampusGuid );

                    foreach ( var attendee in GetAttendanceBags( rockContext, occurrenceData ).Where( a => a.PersonAliasId.HasValue ) )
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
            using (  var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( attendanceOccurrenceGuidOverride: bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

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
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( attendanceOccurrenceGuidOverride: bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

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
                var locations = new GroupService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.Guid == bag.GroupGuid.Value && g.GroupLocations.Any() )
                    .SelectMany( g => g.GroupLocations )
                    .Where( gl => gl.Location != null )
                    .Select( gl => gl.Location )
                    .Where( l => l.Name != null && !string.IsNullOrEmpty( l.Name.Trim() ) )
                    .Select( l => new
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
                    return ActionNotFound();
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

                var list = locationValues.Select( kvp => new ListItemBag
                {
                    Value = kvp.Key.ToString(),
                    Text = kvp.Value
                } );

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
            if ( !bag.GroupGuid.HasValue || !bag.LocationGuid.HasValue )
            {
                return ActionNotFound();
            }

            using ( var rockContext = new RockContext() )
            {
                var groupLocationSchedulesQuery = new GroupLocationService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( gl => gl.Location )
                    .Include( gl => gl.Schedules )
                    .Where( gl => gl.Group.Guid == bag.GroupGuid.Value )
                    .Where( gl => gl.Location.Guid == bag.LocationGuid.Value )
                    .Where( gl => gl.Schedules.Any() )
                    .SelectMany( gl => gl.Schedules )
                    .OrderBy( s => s.Name )
                    .Distinct();

                var schedules = new Dictionary<Guid, string>();

                if ( bag.Date.HasValue )
                {
                    // Only include schedules that apply to a specific date.
                    var groupLocationSchedules = groupLocationSchedulesQuery.ToList();
                    foreach ( var schedule in groupLocationSchedules )
                    {
                        var startTimes = schedule.GetScheduledStartTimes( bag.Date.Value.Date, bag.Date.Value.Date.AddDays( 1 ) );
                        if ( startTimes.Any() )
                        {
                            schedules.AddOrIgnore( schedule.Guid, schedule.Name );
                        }
                    }
                }
                else
                {
                    var groupLocationSchedules = groupLocationSchedulesQuery
                        .Select( s => new
                        {
                            ScheduleGuid = s.Guid,
                            ScheduleName = s.Name
                        } )
                        .ToList();

                    foreach ( var groupLocationSchedule in groupLocationSchedules )
                    {
                        schedules.AddOrIgnore( groupLocationSchedule.ScheduleGuid, groupLocationSchedule.ScheduleName );
                    }
                }

                if ( !schedules.Any() )
                {
                    return ActionNotFound();
                }

                var list = schedules.Select( kvp => new ListItemBag
                {
                    Value = kvp.Key.ToString(),
                    Text = kvp.Value
                } );

                return ActionOk( list );
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
                    return ActionNotFound();
                }

                var scheduleDates = new List<(DateTime Date, Guid ScheduleGuid, string FormattedValue)>();

                var now = RockDateTime.Now;
                DateTime startDate;

                if ( bag.NumberOfPreviousDaysToShow.HasValue )
                {
                    // Start N days ago.
                    startDate = now.AddDays( -bag.NumberOfPreviousDaysToShow.Value );
                }
                else
                {
                    // Default to 1 month ago.
                    startDate = now.AddMonths( -1 );
                }
                var endDate = now;

                foreach ( var groupLocationSchedule in groupLocationSchedules )
                {
                    var startTimes = groupLocationSchedule.GetScheduledStartTimes( startDate, endDate );

                    foreach ( var startTime in startTimes )
                    {
                        scheduleDates.Add( (startTime, groupLocationSchedule.Guid, startTime.ToString( "dddd MMMM d, yyyy - h:mmtt" )) );
                    }
                }

                var list = scheduleDates
                    .OrderByDescending( s => s.Date )
                    .Select( s => new ListItemBag
                    {
                        Value = $"{s.Date.ToString( "s" )}|{s.ScheduleGuid}",
                        Text = s.FormattedValue
                    } );

                return ActionOk( list );
            }
        }

        #endregion

        #region Private Methods

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
            var returnUrl = this.PageParameter( PageParameterKey.ReturnUrl );

            if ( returnUrl.IsNullOrWhiteSpace() )
            {
                returnUrl = this.RequestContext.RequestUri.AbsoluteUri;
            }

            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.GroupId, occurrenceData.Group.Id.ToString() },
                { PageParameterKey.ReturnUrl, returnUrl }
            };

            var groupTypeIds = this.GroupTypeIdsPageParameter;
            if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
            {
                queryParams.Add( PageParameterKey.GroupTypeIds, groupTypeIds );
            }

            return this.GetParentPageUrl( queryParams );
        }

        /// <summary>
        /// Gets a user preference for the current user.
        /// </summary>
        /// <param name="key">The user preference key.</param>
        /// <returns>The user preference.</returns>
        private string GetCurrentUserPreference( string key )
        {
            return PersonService.GetUserPreference( this.GetCurrentPerson(), key );
        }

        /// <summary>
        /// Gets a user preference for the current user and block instance.
        /// </summary>
        /// <param name="key">The user preference key that will be converted to a block user preference key.</param>
        /// <returns>The user preference.</returns>
        private string GetCurrentUserPreferenceForBlock( string key )
        {
            return GetCurrentUserPreference( GetUserPreferenceKeyForBlock( key ) );
        }

        /// <summary>
        /// Gets the client service for reading the occurrence data.
        /// </summary>
        private OccurrenceDataClientService GetOccurrenceDataClientService( RockContext rockContext )
        {
            return new OccurrenceDataClientService(
                this,
                new GroupService( rockContext ),
                new AttendanceService( rockContext ),
                new AttendanceOccurrenceService( rockContext ),
                new LocationService( rockContext ),
                new ScheduleService( rockContext ) );
        }

        /// <summary>
        /// Gets the user preference key for this block instance.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The block user preference key.</returns>
        private string GetUserPreferenceKeyForBlock( string key )
        {
            return $"{PersonService.GetBlockUserPreferenceKeyPrefix( this.BlockId )}{key}";
        }

        /// <summary>
        /// Sets a user preference for the current user.
        /// </summary>
        /// <param name="key">The user preference key.</param>
        /// <param name="value">The user preference value.</param>
        private void SetCurrentUserPreference( string key, string value )
        {
            PersonService.SaveUserPreference( this.GetCurrentPerson(), key, value );
        }

        /// <summary>
        /// Sets a user preference for the current user and block instance.
        /// </summary>
        /// <param name="key">The user preference key that will be converted to a block user preference key.</param>
        /// <param name="value">The user preference value.</param>
        private void SetCurrentUserPreferenceForBlock( string key, string value )
        {
            SetCurrentUserPreference( GetUserPreferenceKeyForBlock( key ), value );
        }

        /// <summary>
        /// Sets the error data on the box from the occurrence data.
        /// </summary>
        private void SetErrorData( OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            box.ErrorMessage = occurrenceData.ErrorMessage;
            box.IsGroupNotFoundError = occurrenceData.IsGroupNotFoundError;
            box.IsNotAuthorizedError = occurrenceData.IsNotAuthorizedError;
            box.IsNoAttendanceOccurrencesError = occurrenceData.IsNoAttendanceOccurrencesError;

            box.IsConfigError = box.ErrorMessage.IsNotNullOrWhiteSpace()
                || box.IsGroupNotFoundError
                || box.IsNotAuthorizedError
                || box.IsNoAttendanceOccurrencesError;
        }

        /// <summary>
        /// Gets the initialization box.
        /// </summary>
        private GroupAttendanceDetailInitializationBox SetInitializationBox( RockContext rockContext, OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            box.CampusName = occurrenceData.Campus?.Name;
            box.CampusGuid = occurrenceData.Campus?.Guid;
            box.GroupGuid = occurrenceData.Group.Guid;
            box.GroupName = occurrenceData.Group.Name;
            box.IsCampusFilteringAllowed = this.IsCampusFilteringAllowed;
            box.IsFutureOccurrenceDateSelectionRestricted = this.IsFutureOccurrenceDateSelectionRestricted;
            box.IsNewAttendanceDateAdditionRestricted = this.IsNewAttendanceDateAdditionRestricted;
            box.IsNewAttendeeAdditionAllowed = this.IsNewAttendeeAdditionAllowed;
            box.IsNotesSectionHidden = this.IsNotesSectionHidden;
            box.NotesSectionLabel = this.NotesSectionLabel;
            box.AddPersonAs = this.AddPersonAs;
            box.AttendanceOccurrenceId = occurrenceData.IsNewOccurrence ? ( int? ) null : occurrenceData.AttendanceOccurrence.Id;
            box.AttendanceOccurrenceGuid = occurrenceData.IsNewOccurrence ? (Guid?)null : occurrenceData.AttendanceOccurrence.Guid;
            box.IsLongListDisabled = this.IsLongListDisabled;
            box.IsDidNotMeetDisabled = this.IsDidNotMeetDisabled;
            box.IsBackButtonHidden = this.IsBackButtonHidden;
            box.NumberOfPreviousDaysToShow = this.NumberOfPreviousDaysToShow;
            box.Notes = occurrenceData.AttendanceOccurrence.Notes;

            box.BackPageUrl = GetBackPageUrl( occurrenceData );
            box.AddGroupMemberPageUrl = GetAddGroupMemberPageUrl( occurrenceData );
            SetOccurrenceDateOptions( occurrenceData, box );
            SetLocationOptions( rockContext, occurrenceData, box );
            SetScheduleOptions( occurrenceData, box );
            box.GroupMembersSectionLabel = occurrenceData.Group.GroupType.GroupMemberTerm.Pluralize();

            var allowedAttendanceTypeValues = this.AttendanceOccurrenceTypeValues;

            if ( allowedAttendanceTypeValues.Any() )
            {
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
        /// Gets the list of bags containing the information to render the attendances in the Group Attendance Detail block.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <returns>The list of bags containing the information to render the attendances in the Group Attendance Detail block.</returns>
        private List<GroupAttendanceDetailAttendanceBag> GetAttendanceBags( RockContext rockContext, OccurrenceData occurrenceData )
        {
            // Get the group members query.
            var groupMembersQuery = new GroupMemberService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( m =>
                    m.GroupId == occurrenceData.Group.Id
                    && m.GroupMemberStatus != GroupMemberStatus.Inactive );
            var groupRoleQuery = new GroupTypeRoleService( rockContext ).Queryable().AsNoTracking();
            var primaryAliasQuery = new PersonAliasService( rockContext ).GetPrimaryAliasQuery();

            // Get the query for people who attended.
            IQueryable<AttendanceData> attendedPeopleQuery = null;
            if ( occurrenceData.AttendanceOccurrence.Id > 0 )
            {
                // These may or may not be group members.
                attendedPeopleQuery = new AttendanceService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( a =>
                        a.OccurrenceId == occurrenceData.AttendanceOccurrence.Id
                        && a.DidAttend.HasValue
                        && a.DidAttend.Value
                        && a.PersonAliasId.HasValue )
                    .Select( a => new
                    {
                        Attendance = a,
                        GroupMember = groupMembersQuery.FirstOrDefault( m => m.PersonId == a.PersonAlias.Person.Id )
                    } )
                    .Select( a => new AttendanceData
                    {
                        DidAttend = true,
                        GroupMember = a.GroupMember,
                        GroupRoleName = a.GroupMember != null && a.GroupMember.GroupRole != null ? a.GroupMember.GroupRole.Name : null,
                        Person = a.Attendance.PersonAlias.Person,
                        PersonAliasId = a.Attendance.PersonAlias.Id,
                        PrimaryCampusGuid = a.Attendance.PersonAlias.Person.PrimaryCampusId.HasValue ? a.Attendance.PersonAlias.Person.PrimaryCampus.Guid : ( Guid? ) null,
                    } );
            }

            // Get the people who did not attend from a Person EntitySet (if specified) or from the unattended group members.
            IQueryable<AttendanceData> unattendedPeopleQuery = null;
            var entitySetId = this.EntitySetIdPageParameter;
            if ( entitySetId.HasValue )
            {
                // These may or may not be group members.
                var entitySetService = new EntitySetService( rockContext );
                unattendedPeopleQuery = entitySetService
                    .GetEntityQuery<Person>( entitySetId.Value )
                    .AsNoTracking()
                    .Select( p => new
                    {
                        Person = p,
                        GroupMember = groupMembersQuery.FirstOrDefault( m => m.PersonId == p.Id ),
                    } )
                    .Select( p => new AttendanceData
                    {
                        DidAttend = false,
                        GroupMember = p.GroupMember,
                        GroupRoleName = p.GroupMember != null && p.GroupMember.GroupRole != null ? p.GroupMember.GroupRole.Name : null,
                        Person = p.Person,
                        PersonAliasId = primaryAliasQuery.Where( a => a.PersonId == p.Person.Id ).Select( a => a.Id ).FirstOrDefault(),
                        PrimaryCampusGuid = p.Person.PrimaryCampusId.HasValue ? p.Person.PrimaryCampus.Guid : ( Guid? ) null,
                    } );
            }
            else
            {
                unattendedPeopleQuery = groupMembersQuery
                    .Select( m => new AttendanceData
                    {
                        DidAttend = false,
                        GroupMember = m,
                        GroupRoleName = m.GroupRole != null ? m.GroupRole.Name : null,
                        Person = m.Person,
                        PersonAliasId = primaryAliasQuery.Where( a => a.PersonId == m.PersonId ).Select( a => a.Id ).FirstOrDefault(),
                        PrimaryCampusGuid = m.Person.PrimaryCampusId.HasValue ? m.Person.PrimaryCampus.Guid : ( Guid? ) null
                    } );
            }

            var lavaItemTemplate = this.ListItemDetailsTemplate;

            // Union the attended and unattended people and project the results to a roster attendance bag.
            
            if (attendedPeopleQuery == null && unattendedPeopleQuery == null )
            {
                return new List<GroupAttendanceDetailAttendanceBag>();
            }

            IQueryable<AttendanceData> query;
            if ( attendedPeopleQuery == null )
            {
                query = unattendedPeopleQuery;
            }
            else if ( unattendedPeopleQuery == null )
            {
                query = attendedPeopleQuery;
            }
            else
            {
                var distinctUnattendedPeople = unattendedPeopleQuery.Where( u => !attendedPeopleQuery.Any( a => a.PersonAliasId == u.PersonAliasId ) );
                query = attendedPeopleQuery.Union( distinctUnattendedPeople );
            }

            return query.ToList()
                .Select( p => GetAttendanceBag( p ) )
                .ToList();
        }

        /// <summary>
        /// Gets the bag containing the information needed to display an Attendance record in the Group Attendance Detail block.
        /// </summary>
        /// <param name="attendanceData">The Attendance record data.</param>
        /// <returns>The bag containing the information needed to display an Attendance record in the Group Attendance Detail block.</returns>
        private GroupAttendanceDetailAttendanceBag GetAttendanceBag( AttendanceData attendanceData )
        {
            var mergeFields = this.RequestContext.GetCommonMergeFields();
            mergeFields.Add( MergeFieldKeys.Person, attendanceData.Person );
            mergeFields.Add( MergeFieldKeys.Attended, attendanceData.DidAttend );
            mergeFields.Add( MergeFieldKeys.GroupMember, attendanceData.GroupMember );
            mergeFields.Add( MergeFieldKeys.GroupRoleName, attendanceData.GroupRoleName );

            var itemTemplate = this.ListItemDetailsTemplate.ResolveMergeFields( mergeFields );

            return new GroupAttendanceDetailAttendanceBag
            {
                PersonGuid = attendanceData.Person.Guid,
                NickName = attendanceData.Person.NickName,
                LastName = attendanceData.Person.LastName,
                DidAttend = attendanceData.DidAttend,
                CampusGuid = attendanceData.PrimaryCampusGuid,
                ItemTemplate = itemTemplate
            };
        }

        /// <summary>
        /// Sets the initialization options for the Attendance For date.
        /// </summary>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <param name="box">The initialization box.</param>
        private void SetOccurrenceDateOptions( OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            // Set occurrence date options.
            box.AttendanceOccurrenceDate = occurrenceData.AttendanceOccurrence.OccurrenceDate.Date;

            if ( occurrenceData.IsNewOccurrence && occurrenceData.AttendanceOccurrence.OccurrenceDate != this.DatePageParameter )
            {
                switch ( this.DateSelectionMode )
                {
                    case DateSelectionModeSpecifier.DatePicker:
                        box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.DatePicker;
                        break;
                    case DateSelectionModeSpecifier.CurrentDate:
                        box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.Readonly;
                        box.AttendanceOccurrenceDate = RockDateTime.Today;
                        break;
                    case DateSelectionModeSpecifier.PickFromSchedule:
                        box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.ScheduledDatePicker;
                        break;
                }
            }
            else
            {
                box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.Readonly;
            } 
        }

        /// <summary>
        /// Sets the initialization options for the location picker.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <param name="box">The initialization box.</param>
        private void SetLocationOptions( RockContext rockContext, OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            // Set location options.
            if ( occurrenceData.AttendanceOccurrence.LocationId.HasValue )
            {
                box.LocationGuid = occurrenceData.AttendanceOccurrence.Location.Guid;
                box.LocationSelectionMode = GroupAttendanceDetailLocationSelectionMode.Readonly;
                box.LocationLabel = new LocationService( rockContext ).GetPath( occurrenceData.AttendanceOccurrence.LocationId.Value );
            }
            else
            {
                box.LocationGuid = null;
                box.LocationSelectionMode = GroupAttendanceDetailLocationSelectionMode.GroupLocationPicker;
                box.LocationLabel = null;
            }
        }

        /// <summary>
        /// Sets the initialization options for the schedule picker.
        /// </summary>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <param name="box">The initialization box.</param>
        private void SetScheduleOptions( OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            if ( occurrenceData.AttendanceOccurrence.Location != null && occurrenceData.AttendanceOccurrence.Schedule != null )
            {
                box.ScheduleGuid = occurrenceData.AttendanceOccurrence.Schedule.Guid;
                box.ScheduleSelectionMode = GroupAttendanceDetailScheduleSelectionMode.Specific;
                box.ScheduleLabel = occurrenceData.AttendanceOccurrence.Schedule.Name;
            }
            else
            {
                box.ScheduleGuid = null;
                box.ScheduleSelectionMode = GroupAttendanceDetailScheduleSelectionMode.GroupLocationSchedulePicker;
                box.ScheduleLabel = null;
            }
        }

        /// <summary>
        /// Adds the person as group member but does not save changes.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private GroupMember AddPersonAsGroupMemberWithoutSaving( Group group, Person person, RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );
            var role = new GroupTypeRoleService( rockContext ).Get( group.GroupType.DefaultGroupRoleId ?? 0 );

            var groupMember = new GroupMember
            {
                Id = 0,
                GroupId = group.Id
            };

            // Check to see if the person is already a member of the group/role.
            var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( group.Id, person.Id, group.GroupType.DefaultGroupRoleId ?? 0 );

            if ( existingGroupMember != null )
            {
                return existingGroupMember;
            }

            groupMember.PersonId = person.Id;
            groupMember.GroupRoleId = role.Id;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            if ( groupMember.Id.Equals( 0 ) )
            {
                groupMemberService.Add( groupMember );
                rockContext.SaveChanges();
            }

            return groupMember;
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
        private int? GetAttendanceCampusId( int? occurrenceLocationCampusId, int? groupLocationCampusId, Guid? campusGuidOverride )
        {
            var campusId = occurrenceLocationCampusId ?? groupLocationCampusId;

            if ( !campusId.HasValue && this.IsCampusFilteringAllowed && campusGuidOverride.HasValue )
            {
                campusId = CampusCache.GetId( campusGuidOverride.Value );
            }

            return campusId;
        }

        #endregion

        #region Helper Classes and Enums

        /// <summary>
        /// Used for gathering <see cref="Attendance"/> data to create an instance of <see cref="GroupAttendanceDetailAttendanceBag"/>.
        /// </summary>
        private class AttendanceData
        {
            internal bool DidAttend { get; set; }

            internal Person Person { get; set; }

            internal Guid? PrimaryCampusGuid { get; set; }

            internal GroupMember GroupMember { get; set; }

            internal int? PersonAliasId { get; set; }

            internal string GroupRoleName { get; set; }
        }

        /// <summary>
        /// A helper for retrieving and saving <see cref="AttendanceOccurrence"/> data for the Group Attendance Detail block.
        /// </summary>
        private class OccurrenceDataClientService
        {
            private readonly AttendanceOccurrenceService _attendanceOccurrenceService;
            private readonly AttendanceService _attendanceService;
            private readonly GroupAttendanceDetail _block;
            private readonly GroupService _groupService;
            private readonly LocationService _locationService;
            private readonly ScheduleService _scheduleService;

            internal OccurrenceDataClientService( GroupAttendanceDetail block, GroupService groupService, AttendanceService attendanceService, AttendanceOccurrenceService attendanceOccurrenceService, LocationService locationService, ScheduleService scheduleService )
            {
                _block = block ?? throw new ArgumentNullException( nameof( block ) );
                _groupService = groupService ?? throw new ArgumentNullException( nameof( groupService ) );
                _attendanceService = attendanceService ?? throw new ArgumentNullException( nameof( attendanceService ) );
                _attendanceOccurrenceService = attendanceOccurrenceService ?? throw new ArgumentNullException( nameof( attendanceOccurrenceService ) );
                _locationService = locationService ?? throw new ArgumentNullException( nameof( locationService ) );
                _scheduleService = scheduleService ?? throw new ArgumentNullException( nameof( scheduleService ) );
            }

            internal OccurrenceData GetOccurrenceData( AttendanceOccurrenceSearchParameters occurrenceDataSearchParameters = null, bool asNoTracking = false )
            {
                var occurrenceData = new OccurrenceData();

                if ( occurrenceDataSearchParameters == null )
                {
                    occurrenceDataSearchParameters = GetAttendanceOccurrenceSearchParameters();
                }

                if ( !TrySetGroup( occurrenceData, occurrenceDataSearchParameters, asNoTracking ) )
                {
                    return occurrenceData;
                }

                if ( !TrySetAttendanceOccurrence( occurrenceData, occurrenceDataSearchParameters, asNoTracking ) )
                {
                    return occurrenceData;
                }

                occurrenceData.Campus = GetCampus( occurrenceDataSearchParameters );

                return occurrenceData;
            }

            internal AttendanceOccurrenceSearchParameters GetAttendanceOccurrenceSearchParameters( Guid? attendanceOccurrenceGuidOverride = null, DateTime? attendanceOccurrenceDateOverride = null, Guid? locationGuidOverride = null, Guid? scheduleGuidOverride = null, int? campusIdOverride = null )
            {
                // Get defaults.
                var occurrenceDataSearchParameters = new AttendanceOccurrenceSearchParameters
                {
                    AttendanceOccurrenceDate = _block.DatePageParameter ?? _block.OccurrencePageParameter,
                    AttendanceOccurrenceId = _block.OccurrenceIdPageParameter,
                    GroupId = _block.GroupIdPageParameter,
                    LocationId = _block.LocationIdPageParameter,
                    ScheduleId = _block.ScheduleIdPageParameter,
                };

                // If AttendanceOccurrenceId is set, then assume a specific AttendanceOccurrence is being requested.
                if ( attendanceOccurrenceGuidOverride.HasValue )
                {
                    occurrenceDataSearchParameters.AttendanceOccurrenceId = _attendanceOccurrenceService.GetId( attendanceOccurrenceGuidOverride.Value );
                }

                // If Group ID page parameter not found, then use the Group Schedule.
                if ( !occurrenceDataSearchParameters.ScheduleId.HasValue && occurrenceDataSearchParameters.GroupId.HasValue )
                {
                    occurrenceDataSearchParameters.ScheduleId = GetGroup( occurrenceDataSearchParameters.GroupId.Value, asNoTracking: true )?.ScheduleId;
                }

                // If overrides are allowed, then use the overrides.
                if ( !_block.IsNewAttendanceDateAdditionRestricted )
                {
                    if ( locationGuidOverride.HasValue )
                    {
                        occurrenceDataSearchParameters.LocationId = _locationService.GetId( locationGuidOverride.Value );
                    }

                    if ( attendanceOccurrenceDateOverride.HasValue )
                    {
                        occurrenceDataSearchParameters.AttendanceOccurrenceDate = attendanceOccurrenceDateOverride.Value.Date;
                    }

                    if ( scheduleGuidOverride.HasValue )
                    {
                        occurrenceDataSearchParameters.ScheduleId = _scheduleService.GetId( scheduleGuidOverride.Value );
                    }

                    if ( _block.IsCampusFilteringAllowed )
                    {
                        // Set the search parameter.
                        occurrenceDataSearchParameters.CampusId = campusIdOverride;

                        // Update the user preference.
                        var campusIdUserPreference = _block.CampusIdBlockUserPreference;
                        if ( campusIdUserPreference != campusIdOverride )
                        {
                            _block.CampusIdBlockUserPreference = campusIdOverride;
                        }
                    }
                }

                return occurrenceDataSearchParameters;
            }

            internal AttendanceOccurrenceSearchParameters GetAttendanceOccurrenceSearchParameters( GroupAttendanceDetailGetOrCreateRequestBag bag )
            {
                return GetAttendanceOccurrenceSearchParameters( bag.AttendanceOccurrenceGuid, bag.AttendanceOccurrenceDate?.Date, bag.LocationGuid, bag.ScheduleGuid, bag.CampusGuid.HasValue ? CampusCache.GetId( bag.CampusGuid.Value ) : null );
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
                    _attendanceOccurrenceService.Add( occurrenceData.AttendanceOccurrence );
                }
                else
                {
                    _attendanceOccurrenceService.Attach( occurrenceData.AttendanceOccurrence );
                }

                occurrenceData.AttendanceOccurrence.Notes = !_block.IsNotesSectionHidden ? bag.Notes : string.Empty;
                occurrenceData.AttendanceOccurrence.DidNotOccur = bag.DidNotOccur;

                // Set the attendance type.
                if ( bag.AttendanceTypeGuid.HasValue )
                {
                    var allowedAttendanceTypes = _block.AttendanceOccurrenceTypeValues;
                    var attendanceTypeDefinedValue = allowedAttendanceTypes.FirstOrDefault( a => a.Guid == bag.AttendanceTypeGuid.Value );
                    occurrenceData.AttendanceOccurrence.AttendanceTypeValueId = attendanceTypeDefinedValue?.Id;
                }

                // Update the attendees.
                var existingAttendees = occurrenceData.AttendanceOccurrence.Attendees.ToList();

                var campusId = new Lazy<int?>( () =>
                {
                    var occurrenceLocationCampusId = _locationService.GetCampusIdForLocation( occurrenceData.AttendanceOccurrence.LocationId );
                    return _block.GetAttendanceCampusId( occurrenceLocationCampusId, occurrenceData.Group.CampusId, bag.CampusGuid );
                } );

                if ( bag.DidNotOccur )
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
                    else
                    {
                        foreach ( var attendee in bag.Attendees.Where( a => a.PersonAliasId.HasValue ) )
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
                else
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

                    foreach ( var attendee in bag.Attendees )
                    {
                        var attendance = existingAttendees
                            .Where( a => a.PersonAliasId == attendee.PersonAliasId )
                            .FirstOrDefault();

                        if ( attendance == null )
                        {
                            if ( attendee.PersonAliasId.HasValue )
                            {
                                attendance = CreateAttendanceInstance(
                                    attendee.PersonAliasId,
                                    campusId.Value,
                                    startDateTime,
                                    attendee.DidAttend );

                                // Check that the attendance record is valid
                                if ( !attendance.IsValid )
                                {
                                    occurrenceData.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                                    return false;
                                }

                                occurrenceData.AttendanceOccurrence.Attendees.Add( attendance );
                            }
                        }
                        else
                        {
                            // Otherwise, only record that they attended -- don't change their attendance startDateTime.
                            attendance.DidAttend = attendee.DidAttend;
                        }
                    }
                }

                return true;
            }

            internal bool TrySetGroup( OccurrenceData occurrenceData, AttendanceOccurrenceSearchParameters attendanceOccurrenceSearchParameters, bool asNoTracking )
            {
                // Get Group.
                if ( attendanceOccurrenceSearchParameters.GroupId.HasValue )
                {
                    occurrenceData.Group = GetGroup( attendanceOccurrenceSearchParameters.GroupId.Value, asNoTracking );
                }
                else
                {
                    occurrenceData.Group = null;
                    occurrenceData.ErrorMessage = "Group ID was not provided.";
                    return false;
                }

                if ( occurrenceData.Group == null )
                {
                    occurrenceData.IsGroupNotFoundError = true;
                    return false;
                }

                // Authorize Group.
                var currentPerson = _block.GetCurrentPerson();

                if ( !occurrenceData.Group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson )
                    && !occurrenceData.Group.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    occurrenceData.IsNotAuthorizedError = true;
                    return false;
                }

                return true;
            }

            internal bool TrySetAttendanceOccurrence( OccurrenceData occurrenceData, AttendanceOccurrenceSearchParameters occurrenceDataSearchParameters, bool asNoTracking = false )
            {
                (var isReadOnly, var attendanceOccurrence) = GetExistingAttendanceOccurrence( occurrenceDataSearchParameters, asNoTracking );

                occurrenceData.IsReadOnly = isReadOnly;
                occurrenceData.AttendanceOccurrence = attendanceOccurrence;

                if ( occurrenceData.AttendanceOccurrence == null )
                {
                    if ( _block.IsNewAttendanceDateAdditionRestricted )
                    {
                        occurrenceData.IsNoAttendanceOccurrencesError = true;
                        return false;
                    }

                    occurrenceData.AttendanceOccurrence = GetNewAttendanceOccurrence( occurrenceDataSearchParameters );
                }

                return occurrenceData.AttendanceOccurrence != null;
            }

            private CampusCache GetCampus( AttendanceOccurrenceSearchParameters occurrenceDataSearchParameters )
            {
                var campusId = occurrenceDataSearchParameters.CampusId;

                if ( campusId.HasValue )
                {
                    return CampusCache.Get( campusId.Value );
                }

                return null;
            }

            private Group GetGroup( int groupId, bool asNoTracking )
            {
                // Get Group.
                var query = _groupService
                    .AsNoFilter()
                    .Include( g => g.GroupType )
                    .Include( g => g.Schedule )
                    .Where( g => g.Id == groupId );

                if ( asNoTracking )
                {
                    query = query.AsNoTracking();
                }

                return query.FirstOrDefault();
            }

            private (bool IsReadOnly, AttendanceOccurrence AttendanceOccurrence) GetExistingAttendanceOccurrence( AttendanceOccurrenceSearchParameters occurrenceDataSearchParameters, bool asNoTracking )
            {
                // Try to set the AttendanceOccurrence from Attendance Occurrence ID.
                if ( occurrenceDataSearchParameters.AttendanceOccurrenceId.HasValue && occurrenceDataSearchParameters.AttendanceOccurrenceId.Value > 0 )
                {
                    var query = _attendanceOccurrenceService
                        .AsNoFilter()
                        .Include( a => a.Schedule )
                        .Include( a => a.Location )
                        .Include( a => a.Attendees )
                        .Where( a => a.Id == occurrenceDataSearchParameters.AttendanceOccurrenceId.Value );

                    if ( asNoTracking )
                    {
                        query = query.AsNoTracking();
                    }

                    var attendanceOccurrence = query.FirstOrDefault();

                    if ( attendanceOccurrence != null )
                    {
                        return (false, attendanceOccurrence);
                    }
                }

                // If no specific Attendance Occurrence ID was specified, try to find a matching occurrence from Date, GroupId, Location, ScheduleId.
                if ( occurrenceDataSearchParameters.AttendanceOccurrenceDate.HasValue )
                {
                    var attendanceOccurrence = _attendanceOccurrenceService.Get( occurrenceDataSearchParameters.AttendanceOccurrenceDate.Value.Date, occurrenceDataSearchParameters.GroupId, occurrenceDataSearchParameters.LocationId, occurrenceDataSearchParameters.ScheduleId, "Location,Schedule" );
                    return ( attendanceOccurrence != null ? true : false, attendanceOccurrence );
                }

                return ( false, null );
            }

            private AttendanceOccurrence GetNewAttendanceOccurrence( AttendanceOccurrenceSearchParameters attendanceOccurrenceSearchParameters )
            {
                var attendanceOccurrence = new AttendanceOccurrence
                {
                    GroupId = attendanceOccurrenceSearchParameters.GroupId,
                    LocationId = attendanceOccurrenceSearchParameters.LocationId,
                    OccurrenceDate = attendanceOccurrenceSearchParameters.AttendanceOccurrenceDate ?? RockDateTime.Today,
                    ScheduleId = attendanceOccurrenceSearchParameters.ScheduleId
                };

                if ( attendanceOccurrenceSearchParameters.LocationId.HasValue )
                {
                    attendanceOccurrence.Location = _locationService.Get( attendanceOccurrenceSearchParameters.LocationId.Value );
                }

                if ( attendanceOccurrenceSearchParameters.ScheduleId.HasValue )
                {
                    attendanceOccurrence.Schedule = _scheduleService.Get( attendanceOccurrenceSearchParameters.ScheduleId.Value );
                }

                return attendanceOccurrence;
            }
        }

        /// <summary>
        /// Represents <see cref="AttendanceOccurrence"/> data in a shape that is useful for this Group Attendance Detail block.
        /// </summary>
        private class OccurrenceData
        {
            public bool IsValid => ErrorMessage.IsNullOrWhiteSpace() && !IsGroupNotFoundError && !IsNoAttendanceOccurrencesError && !IsNotAuthorizedError && Group != null && AttendanceOccurrence != null;

            public bool IsNewOccurrence => AttendanceOccurrence?.Id == 0;

            public string ErrorMessage { get; set; }

            public Group Group { get; internal set; }

            public AttendanceOccurrence AttendanceOccurrence { get; internal set; }

            public CampusCache Campus { get; internal set; }

            public bool IsGroupNotFoundError { get; internal set; }

            public bool IsNotAuthorizedError { get; internal set; }

            public bool IsNoAttendanceOccurrencesError { get; internal set; }
            public bool IsReadOnly { get; internal set; }
        }

        /// <summary>
        /// Represents search parameters for finding an existing <see cref="AttendanceOccurrence"/>.
        /// </summary>
        private class AttendanceOccurrenceSearchParameters
        {
            /// <summary>
            /// Gets or sets the attendance occurrence identifier.
            /// <para>If set to an existing Attendance Occurrence ID, then no other search parameters are necessary.</para>
            /// </summary>
            public int? AttendanceOccurrenceId { get; internal set; }

            public int? GroupId { get; set; }

            public DateTime? AttendanceOccurrenceDate { get; set; }

            public int? LocationId { get; set; }

            public int? ScheduleId { get; set; }

            public int? CampusId { get; set; }
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

        #endregion
    }
}

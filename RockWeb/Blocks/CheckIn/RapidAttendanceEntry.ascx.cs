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
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName( "Rapid Attendance Entry" )]
    [Category( "Check-in" )]
    [Description( "Provides a way to manually enter attendance for a large group of people in an efficient manner." )]

    #region Block Attributes

    [LinkedPage( "Add Family Page",
        Key = AttributeKey.AddFamilyPage,
        Description = "Page used for adding new families.",
        IsRequired = false,
        Order = 0 )]
    [LinkedPage( "Attendance List Page",
        Key = AttributeKey.AttendanceListPage,
        Description = "Page used to show the attendance list.",
        IsRequired = false,
        Order = 1 )]

    #region Attendance Block Attribute Settings
    [BooleanField(
        "Enable Attendance",
        Key = AttributeKey.EnableAttendance,
        Description = "If enabled, allows the individual to select a group, schedule, and attendance date at the start of the session and take attendance for family members.",
        Category = "Attendance",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 1 )]
    [GroupField(
        "Parent Group",
        Key = AttributeKey.ParentGroup,
        Description = "If set, contrains the group picker to only list groups that are under this group.",
        Category = "Attendance",
        IsRequired = false,
        Order = 2 )]
    [GroupField(
        "Attendance Group",
        Key = AttributeKey.AttendanceGroup,
        Description = "If selected will lock the block to the selected group. The individual would then only be able to select Schedule and Date when configuring.",
        Category = "Attendance",
        IsRequired = false,
        Order = 3 )]
    [BooleanField(
        "Show Can Check-In Relationships",
        Key = AttributeKey.ShowCanCheckInRelationships,
        Description = "If enabled, people who have a 'Can check-in' relationship will be shown.",
        Category = "Attendance",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 4 )]
    [IntegerField(
        "Attendance Age Limit",
        Key = AttributeKey.AttendanceAgeLimit,
        Description = "Individuals under this age will not be allowed to be marked as attended.",
        Category = "Attendance",
        IsRequired = true,
        Order = 5 )]
    #endregion Attendance Block Attribute Settings
    #region Family Block Attribute Settings
    [AttributeField(
        "Family Attributes",
        Key = AttributeKey.FamilyAttributes,
        Description = "The family attributes to display when editing a family.",
        Category = "Family",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        IsRequired = false,
        AllowMultiple = true,
        Order = 1 )]
    [CodeEditorField(
        "Header Lava Template",
        Key = AttributeKey.FamilyHeaderLavaTemplate,
        DefaultValue = FamilyHeaderLavaTemplateDefaultValue,
        Description = "Lava for the family header at the top of the page.",
        Category = "Family",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        Order = 2,
        IsRequired = true )]
    #endregion Family Block Attribute Settings
    #region Individual Block Attribute Settings
    [CodeEditorField(
        "Header Lava Template",
        Key = AttributeKey.IndividualHeaderLavaTemplate,
        DefaultValue = IndividualHeaderLavaTemplateDefaultValue,
        Description = "Lava template for the contents of the personal detail when viewing.",
        Category = "Individual",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        Order = 1,
        IsRequired = true )]
    [DefinedValueField(
        "Adult Phone Types",
        Key = AttributeKey.AdultPhoneTypes,
        Description = "The types of phone numbers to display / edit.",
        AllowMultiple = true,
        Category = "Individual",
        Order = 2,
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE )]
    [AttributeField(
        "Adult Person Attributes",
        Key = AttributeKey.AdultPersonAttributes,
        Description = "The attributes to display when editing a person that is an adult.",
        Category = "Individual",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        Order = 3,
        AllowMultiple = true,
        IsRequired = false )]
    [BooleanField(
        "Show Communication Preference(Adults)",
        Key = AttributeKey.ShowCommunicationPreference,
        Description = "Shows the communication preference and allow it to be edited.",
        Category = "Individual",
        DefaultBooleanValue = true,
        IsRequired = false,
        Order = 4 )]
    [DefinedValueField(
        "Child Phone Types",
        AllowMultiple = true,
        Key = AttributeKey.ChildPhoneTypes,
        Description = "The types of phone numbers to display / edit.",
        Category = "Individual",
        Order = 5,
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE )]
    [AttributeField(
        "Child Person Attributes",
        Key = AttributeKey.ChildPersonAttributes,
        Description = "The attributes to display when editing a person that is a child.",
        Category = "Individual",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        IsRequired = false,
        AllowMultiple = true,
        Order = 6 )]
    [BooleanField(
        "Child Allow Email Edit",
        Key = AttributeKey.ChildAllowEmailEdit,
        Description = "If enabled, a child's email address will be visible/editable.",
        Category = "Individual",
        DefaultBooleanValue = true,
        IsRequired = false,
        Order = 7 )]
    #endregion Individual Block Attribute Settings
    #region Prayer Block Attribute Settings
    [BooleanField(
        "Enable Prayer Request Entry",
        Key = AttributeKey.EnablePrayerRequestEntry,
        Description = "If enabled, will show a section for entering a prayer request for the person.",
        Category = "Prayer",
        DefaultBooleanValue = true,
        Order = 1 )]
    [BooleanField(
        "Enabled Urgent Flag",
        Key = AttributeKey.ShowUrgentFlag,
        Description = "If enabled, the request can be flagged as urgent by checking a checkbox.",
        Category = "Prayer",
        DefaultBooleanValue = true,
        Order = 2 )]
    [BooleanField(
        "Show Public Flag",
        Key = AttributeKey.ShowPublicFlag,
        Description = "If enabled, a checkbox will be shown displayed on the public website.",
        Category = "Prayer",
        DefaultBooleanValue = true,
        Order = 3 )]
    [IntegerField(
        "Expires After (Days)",
        Key = AttributeKey.ExpiresAfter,
        DefaultIntegerValue = 14,
        Description = "Number of days until the request will expire.",
        Category = "Prayer",
        IsRequired = true,
        Order = 4 )]
    [CategoryField(
        "Default Category",
        entityTypeName: "Rock.Model.PrayerRequest",
        Description = "The default category to use for all the new prayer requests.",
        Category = "Prayer",
        IsRequired = false,
        Key = AttributeKey.DefaultCategory,
        Order = 5 )]
    [BooleanField(
        "Display To Public",
        Description = "If enabled, all prayers will be set to public by default.",
        Category = "Prayer",
        DefaultBooleanValue = true,
        Key = AttributeKey.DisplayToPublic,
        Order = 6 )]
    [BooleanField(
        "Default Allow Comments",
        Description = "Controls whether or not prayer requests are flagged to allow comments during prayer session.",
        Category = "Prayer",
        IsRequired = false,
        Key = AttributeKey.DefaultAllowComments,
        Order = 7 )]
    [BooleanField(
        "Enable Category Selection",
        Description = "If enabled, it will allow the individual to choose/change the selected category for the prayer request.  If not enabled, the category selection will not be shown and the default category will be used instead.",
        Category = "Prayer",
        DefaultBooleanValue = true,
        Key = AttributeKey.EnableCategorySelection,
        Order = 8 )]
    #endregion Prayer Block Attribute Settings
    #region Workflows Block Attribute Settings
    [TextField(
        "Workflow List Title",
        Key = AttributeKey.WorkflowListTitle,
        DefaultValue = "I'm interested in",
        Description = "The text to show above the workflow list. (For example: I'm Interested in.)",
        Category = "Workflow",
        Order = 0,
        IsRequired = false )]
    [WorkflowTypeField(
        "Workflow Types",
        AllowMultiple = true,
        Key = AttributeKey.WorkflowTypes,
        Description = "A list of workflows to display as a checkbox that can be selected and fired on behalf of the selected person on save.",
        Category = "Workflow",
        IsRequired = false,
        Order = 1 )]
    #endregion Workflows Block Attribute Settings
    #region Notes Block Attribute Settings
    [NoteTypeField(
        "Note Types",
        allowMultiple: true,
        entityTypeName: "Rock.Model.Person",
        Key = AttributeKey.NoteTypes,
        Description = "The type of notes available to select on the form.",
        Category = "Notes",
        IsRequired = false,
        Order = 1 )]
    #endregion Notes Block Attribute Settings
    #endregion Block Attributes
    public partial class RapidAttendanceEntry : RockBlock
    {
        #region Fields

        private const string ROCK_RAPIDATTENDANCEENTRY = "rock_rapidattendanceentry";
        private const string GROUP_ID = "group_id";
        private const string SCHEDULE_ID = "schedule_id";
        private const string LOCATION_ID = "location_id";
        private const string ATTENDANCE_DATE = "attendance_date";
        private const string ATTENDANCE_SETTING = "AttendanceSetting";
        private const string SEARCH_RESULTS_JSON = "SearchResultsJSON";
        private const string SELECTED_GROUP_ID = "SelectedGroupId";
        private const string SELECTED_PERSON_ID = "SelectedPersonId";

        private AttendanceSetting _attendanceSettingState;
        private List<SearchResult> _searchResultsState;
        private int? _selectedGroupId;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [attendance enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attendance enabled]; otherwise, <c>false</c>.
        /// </value>
        protected bool IsAttendanceEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the selected person identifier.
        /// </summary>
        /// <value>
        /// The selected person identifier.
        /// </value>
        protected int? SelectedPersonId { get; set; }

        #endregion Properties

        #region PageParameterKeys

        /// <summary>
        /// A defined list of page parameter keys used by this block.
        /// </summary>
        protected static class PageParameterKey
        {
            /// <summary>
            /// The ULR encoded key for a person
            /// </summary>
            public const string PersonId = "PersonId";
        }

        #endregion PageParameterKeys

        #region Attribute Default values

        private const string FamilyHeaderLavaTemplateDefaultValue = @"
<h4 class='margin-t-none'>{{ Family.Name }}</h4>
	{% if Family.GroupLocations != null %}
	{% assign groupLocations = Family.GroupLocations %}
	{% assign locationCount = groupLocations | Size %}
	    {% if locationCount > 0  %}
		{% for groupLocation in groupLocations %}
		{% if groupLocation.GroupLocationTypeValue.Value == 'Home' and groupLocation.Location.FormattedHtmlAddress != null and groupLocation.Location.FormattedHtmlAddress != ''  %}
		{{ groupLocation.Location.FormattedHtmlAddress }}
		{% endif %}
		{% endfor %}
		{% endif %}
	{% endif %}";

        private const string IndividualHeaderLavaTemplateDefaultValue = @"
<div class='row margin-v-lg'>
    <div class='col-md-6'>
        {%- if Person.Age != null and Person.Age != '' -%}
        {{ Person.Age }} yrs old ({{ Person.BirthDate | Date:'M/d/yyyy' }}) <br />
        {%- endif -%}
        {%- if Person.Email != '' -%}
		<a href='mailto:{{ Person.Email }}'>{{ Person.Email }}</a>
        {%- endif -%}
		{% if Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == 'Inactive' -%}
		    <br/>
		    <span class='label label-danger' title='{{ Person.RecordStatusReasonValue.Value }}' data-toggle='tooltip'>{{ Person.RecordStatusValue.Value }}</span>
		    {% elseif Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == 'Pending' -%}
		    <span class='label label-warning' title='{{ Person.RecordStatusReasonValue.Value }}' data-toggle='tooltip'>{{ Person.RecordStatusValue.Value }}</span>
        {% endif -%}
    </div>
    <div class='col-md-6'>
        {% for phone in Person.PhoneNumbers %}
        {% if phone.IsUnlisted != true %}<a href='tel:{{ phone.NumberFormatted}}'>{{ phone.NumberFormatted }}</a>{% else %}Unlisted{% endif %}  <small>({{ phone.NumberTypeValue.Value }})</small><br />
		{% endfor %}
    </div>
</div>
";

        #endregion Attribute Default values

        #region Atrribute Keys

        /// <summary>
        /// A defined list of attribute keys used by this block.
        /// </summary>
        protected static class AttributeKey
        {
            public const string AddFamilyPage = "AddFamilyPage";
            public const string AttendanceListPage = "AttendanceListPage";
            public const string EnableAttendance = "EnableAttendance";
            public const string ParentGroup = "ParentGroup";
            public const string AttendanceAgeLimit = "AttendanceAgeLimit";
            public const string AttendanceGroup = "AttendanceGroup";
            public const string ShowCanCheckInRelationships = "ShowCanCheckInRelationships";
            public const string FamilyAttributes = "FamilyAttributes";
            public const string FamilyHeaderLavaTemplate = "FamilyHeaderLavaTemplate";
            public const string IndividualHeaderLavaTemplate = "IndividualHeaderLavaTemplate";
            public const string AdultPhoneTypes = "AdultPhoneTypes";
            public const string AdultPersonAttributes = "AdultPersonAttributes";
            public const string ShowCommunicationPreference = "ShowCommunicationPreference";
            public const string ChildPhoneTypes = "ChildPhoneTypes";
            public const string ChildPersonAttributes = "ChildPersonAttributes";
            public const string ChildAllowEmailEdit = "ChildAllowEmailEdit";
            public const string EnablePrayerRequestEntry = "EnablePrayerRequestEntry";
            public const string ShowUrgentFlag = "ShowUrgentFlag";
            public const string ShowPublicFlag = "ShowPublicFlag";
            public const string ExpiresAfter = "ExpiresAfter";
            public const string WorkflowListTitle = "WorkflowListTitle";
            public const string WorkflowTypes = "WorkflowTypes";
            public const string NoteTypes = "NoteTypes";
            public const string DefaultCategory = "DefaultCategory";
            public const string DisplayToPublic = "DisplayToPublic";
            public const string DefaultAllowComments = "DefaultAllowComments";
            public const string EnableCategorySelection = "CategorySelection";
        }

        #endregion Atrribute Keys

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _selectedGroupId = ViewState[SELECTED_GROUP_ID] as int?;
            SelectedPersonId = ViewState[SELECTED_PERSON_ID] as int?;
            _attendanceSettingState = ViewState[ATTENDANCE_SETTING] as AttendanceSetting;
            _searchResultsState = ( this.ViewState[SEARCH_RESULTS_JSON] as string ).FromJsonOrNull<List<SearchResult>>() ?? new List<SearchResult>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbAddFamily.Visible = GetAttributeValue( AttributeKey.AddFamilyPage ).IsNotNullOrWhiteSpace();
            dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;

            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            rblRole.DataSource = familyGroupType.Roles.OrderBy( r => r.Order ).ToList();
            rblRole.DataBind();
            rblRole.Visible = true;
            rblRole.Required = true;

            string clearAttendanceScript = string.Format( "$('#{0}').val('false');", hfAttendanceDirty.ClientID );
            lbSaveAttendance.OnClientClick = clearAttendanceScript;

            string clearPersonScript = string.Format( "$('#{0}').val('false');", hfPersonDirty.ClientID );
            bbtnSaveContactItems.OnClientClick = clearPersonScript;

            IsAttendanceEnabled = GetAttributeValue( AttributeKey.EnableAttendance ).AsBoolean();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ATTENDANCE_SETTING] = _attendanceSettingState;
            ViewState[SEARCH_RESULTS_JSON] = _searchResultsState.ToJson();
            ViewState[SELECTED_PERSON_ID] = SelectedPersonId;
            ViewState[SELECTED_GROUP_ID] = _selectedGroupId;
            return base.SaveViewState();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        #region Setting Events

        /// <summary>
        /// Handles the user selecting an item of the ddlGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateLocations();
            UpdateSchedules();
        }

        /// <summary>
        /// Handles the user selecting an item of the ddlLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateSchedules();
        }

        /// <summary>
        /// Handles the Click event of the lbStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbStart_Click( object sender, EventArgs e )
        {
            var groupId = GetSelectedGroupId();
            var scheduleId = ddlSchedule.SelectedValueAsInt();
            var locationId = ddlLocation.SelectedValueAsInt();
            var attedanceDate = dpAttendanceDate.SelectedDate;
            if ( groupId.HasValue && scheduleId.HasValue && locationId.HasValue && attedanceDate.HasValue )
            {
                _attendanceSettingState = new AttendanceSetting();
                _attendanceSettingState.GroupId = groupId.Value;
                _attendanceSettingState.ScheduleId = scheduleId.Value;
                _attendanceSettingState.LocationId = locationId.Value;
                _attendanceSettingState.AttendanceDate = attedanceDate.Value;
                CreateRapidAttendanceCookie( _attendanceSettingState );
                ShowMainPanel( SelectedPersonId );
            }
        }
        #endregion Setting Events

        #region Main Panel Events

        /// <summary>
        /// Handles the Click event of the lbSetting control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSetting_Click( object sender, EventArgs e )
        {
            ShowAttendanceSetting( _attendanceSettingState );
        }

        /// <summary>
        /// Handles the Click event of the btnGo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGo_Click( object sender, EventArgs e )
        {
            SelectedPersonId = null;
            _selectedGroupId = null;
            GetSearchResults();
            var selectedSearchResult = _searchResultsState.FirstOrDefault();
            if ( selectedSearchResult != null )
            {
                SelectedPersonId = selectedSearchResult.Id;
                _selectedGroupId = selectedSearchResult.FamilyId;
                hfPersonGuid.Value = selectedSearchResult.Guid.ToString();
            }
            BindMainPanel();
        }

        /// <summary>
        /// Handles the Click event of the lbAddFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddFamily_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "ReturnUrl", Server.UrlEncode( Request.RawUrl ) );
            NavigateToLinkedPage( AttributeKey.AddFamilyPage, queryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbEditFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEditFamily_Click( object sender, EventArgs e )
        {
            pnlEditFamily.Visible = true;
            pnlMainEntry.Visible = false;
            ShowFamilyActionButton( false );
            lPreviousAddress.Text = string.Empty;
            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( _selectedGroupId.Value );
            var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            var familyGroupType = GroupTypeCache.Get( familyGroupTypeGuid );
            var addressTypeDv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            var familyAddress = new GroupLocationService( rockContext ).Queryable()
                                .Where( l => l.Group.GroupTypeId == familyGroupType.Id
                                        && l.GroupLocationTypeValueId == addressTypeDv.Id
                                        && l.Group.Members.Any( m => m.PersonId == SelectedPersonId ) )
                                .FirstOrDefault();

            if ( familyAddress != null )
            {
                acAddress.SetValues( familyAddress.Location );
                cbIsMailingAddress.Checked = familyAddress.IsMailingLocation;
                cbIsPhysicalAddress.Checked = familyAddress.IsMappedLocation;
            }
            else
            {
                acAddress.SetValues( null );
                cbIsMailingAddress.Checked = false;
                cbIsPhysicalAddress.Checked = false;
            }

            List<Guid> familyAttributeGuidList = GetAttributeValue( AttributeKey.FamilyAttributes ).SplitDelimitedValues().AsGuidList();
            var attributeList = familyAttributeGuidList.Select( a => AttributeCache.Get( a ) ).ToList();
            avcFamilyAttributes.IncludedAttributes = attributeList.ToArray();
            avcFamilyAttributes.NumberOfColumns = 2;
            avcFamilyAttributes.ShowCategoryLabel = false;
            avcFamilyAttributes.AddEditControls( group );
        }

        /// <summary>
        /// Handles the Click event of the lbAddMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddMember_Click( object sender, EventArgs e )
        {
            ShowEditPersonDetails( Guid.Empty );
        }

        /// <summary>
        /// Handles the Click event of the lbSaveAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveAttendance_Click( object sender, EventArgs e )
        {
            if ( _attendanceSettingState == null )
            {
                return;
            }

            hfAttendanceDirty.Value = "false";
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            var group = new GroupService( rockContext ).Get( _attendanceSettingState.GroupId );
            var groupLocation = new GroupLocationService( rockContext ).Get( _attendanceSettingState.LocationId );
            var personService = new PersonService( rockContext );

            for ( int i = 0; i < rcbAttendance.Items.Count; i++ )
            {
                var personId = rcbAttendance.Items[i].Value.AsInteger();
                var person = personService.Get( personId );

                if ( rcbAttendance.Items[i].Selected )
                {
                    var attendance = attendanceService.AddOrUpdate( person.PrimaryAliasId.Value, _attendanceSettingState.AttendanceDate, group.Id, groupLocation.LocationId, _attendanceSettingState.ScheduleId, group.CampusId );
                }
                else
                {
                    var attendance = attendanceService.Get( _attendanceSettingState.AttendanceDate, groupLocation.LocationId, _attendanceSettingState.ScheduleId, group.Id, person.Id );
                    if ( attendance != null )
                    {
                        attendanceService.Delete( attendance );
                    }
                }
            }

            rockContext.SaveChanges();

            //
            // Flush the attendance cache.
            //
            Rock.CheckIn.KioskLocationAttendance.Remove( groupLocation.LocationId );

            ShowMainPanel( SelectedPersonId );
        }

        /// <summary>
        /// Handles the Click event of the bbtnSaveContactItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnSaveContactItems_Click( object sender, EventArgs e )
        {
            hfPersonDirty.Value = "false";

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( hfPersonGuid.Value.AsGuid() );

            if ( GetAttributeValue( AttributeKey.EnablePrayerRequestEntry ).AsBoolean() && tbPrayerRequest.Text.IsNotNullOrWhiteSpace() )
            {
                PrayerRequest prayerRequest = new PrayerRequest { Id = 0, IsActive = true, IsApproved = true, AllowComments = GetAttributeValue( AttributeKey.DefaultAllowComments ).AsBoolean() };

                prayerRequest.EnteredDateTime = RockDateTime.Now;

                prayerRequest.ApprovedByPersonAliasId = CurrentPersonAliasId;
                prayerRequest.ApprovedOnDateTime = RockDateTime.Now;
                var expireDays = Convert.ToDouble( GetAttributeValue( AttributeKey.ExpiresAfter ) );
                prayerRequest.ExpirationDate = RockDateTime.Now.AddDays( expireDays );

                Category category = null;
                int? categoryId = cpPrayerCategory.SelectedValueAsInt();
                Guid defaultCategoryGuid = GetAttributeValue( AttributeKey.DefaultCategory ).AsGuid();
                if ( categoryId == null && !defaultCategoryGuid.IsEmpty() )
                {
                    category = new CategoryService( rockContext ).Get( defaultCategoryGuid );
                    categoryId = category.Id;
                }
                else if ( categoryId.HasValue )
                {
                    category = new CategoryService( rockContext ).Get( categoryId.Value );
                }

                if ( categoryId.HasValue )
                {
                    prayerRequest.CategoryId = categoryId;
                    prayerRequest.Category = category;
                }
                prayerRequest.FirstName = person.FirstName;
                prayerRequest.LastName = person.LastName;
                prayerRequest.Email = person.Email;
                prayerRequest.RequestedByPersonAliasId = person.PrimaryAliasId;

                int? campusId = null;
                if ( _attendanceSettingState != null )
                {
                    var group = new GroupService( rockContext ).Get( _attendanceSettingState.GroupId );
                    if ( group != null && group.CampusId.HasValue )
                    {
                        campusId = group.CampusId;
                    }
                }
                if ( !campusId.HasValue )
                {
                    var campus = person.GetCampus();
                    if ( campus != null )
                    {
                        campusId = campus.Id;
                    }
                }

                prayerRequest.CampusId = campusId;
                prayerRequest.Text = tbPrayerRequest.Text;

                if ( GetAttributeValue( AttributeKey.ShowUrgentFlag ).AsBoolean() )
                {
                    prayerRequest.IsUrgent = cbIsUrgent.Checked;
                }
                else
                {
                    prayerRequest.IsUrgent = false;
                }

                if ( GetAttributeValue( AttributeKey.ShowPublicFlag ).AsBoolean() )
                {
                    prayerRequest.IsPublic = cbIsPublic.Checked;
                }
                else
                {
                    prayerRequest.IsPublic = GetAttributeValue( AttributeKey.DisplayToPublic ).AsBoolean();
                }

                PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
                prayerRequestService.Add( prayerRequest );
                rockContext.SaveChanges();
            }

            if ( rcwNotes.Visible )
            {
                NoteService noteService = new NoteService( rockContext );

                Note note = new Note();
                note.EntityId = person.Id;
                note.IsAlert = false;
                note.IsPrivateNote = false;
                note.Text = tbNote.Text;

                var noteTypeId = ddlNoteType.SelectedValueAsId();
                if ( noteTypeId.HasValue )
                {
                    note.NoteTypeId = noteTypeId.Value;
                }

                noteService.Add( note );
                rockContext.SaveChanges();
            }

            if ( rcbWorkFlowTypes.Visible && rcbWorkFlowTypes.SelectedValues.Any() )
            {
                var workflowService = new WorkflowService( rockContext );
                Group group = null;
                Schedule schedule = null;
                Location location = null;
                if ( _attendanceSettingState != null )
                {
                    group = new GroupService( rockContext ).Get( _attendanceSettingState.GroupId );
                    schedule = new ScheduleService( rockContext ).Get( _attendanceSettingState.ScheduleId );
                    location = new LocationService( rockContext ).Get( _attendanceSettingState.LocationId );
                }
                var personWorkflows = rcbWorkFlowTypes.SelectedValues.AsGuidList();
                foreach ( var workflowType in personWorkflows )
                {
                    if ( group != null && schedule != null && location != null )
                    {
                        LaunchWorkflows( workflowService, workflowType, person.FullName, person, group.Guid, schedule.Guid, location.Guid );
                    }
                    else
                    {
                        LaunchWorkflows( workflowService, workflowType, person.FullName, person );
                    }
                }
            }
        }


        /// <summary>
        /// Handles the user selecting an item of the ddlNoteType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlNoteType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var noteTypeId = ddlNoteType.SelectedValueAsId();
            tbNote.Enabled = noteTypeId.HasValue;
            if ( !noteTypeId.HasValue )
            {
                tbNote.Text = string.Empty;
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptResults_ItemCommand( object Sender, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Display" )
            {
                var personId = e.CommandArgument.ToString().AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var searchResult = _searchResultsState.FirstOrDefault( a => a.Id == personId );
                    if ( searchResult != null )
                    {
                        SelectedPersonId = personId.Value;
                        _selectedGroupId = searchResult.FamilyId;
                        hfPersonGuid.Value = searchResult.Guid.ToString();
                        BindMainPanel();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPersons_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var person = e.Item.DataItem as Person;
                if ( person != null )
                {
                    // very similar code in EditFamily.ascx.cs
                    HtmlControl divPersonImage = e.Item.FindControl( "divPersonImage" ) as HtmlControl;
                    if ( divPersonImage != null )
                    {
                        divPersonImage.Style.Add( "background-image", @String.Format( @"url({0})", Person.GetPersonPhotoUrl( person ) + "&width=65" ) );
                        divPersonImage.Style.Add( "background-size", "cover" );
                        divPersonImage.Style.Add( "background-position", "50%" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptPersons_ItemCommand( object Sender, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Display" )
            {
                var personGuid = e.CommandArgument.ToString().AsGuidOrNull();
                var searchResult = _searchResultsState.FirstOrDefault( a => a.Id == SelectedPersonId.Value );
                if ( personGuid.HasValue && searchResult != null )
                {
                    var person = searchResult.FamilyMembers.FirstOrDefault( a => a.Guid == personGuid.Value );
                    if ( person != null )
                    {
                        hfPersonGuid.Value = person.Guid.ToString();
                        ShowMainEntryPersonDetail( searchResult );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEditPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEditPerson_Click( object sender, EventArgs e )
        {
            ShowEditPersonDetails( hfPersonGuid.Value.AsGuid() );
        }

        #endregion Main Entry Events

        #region Edit Family Events

        /// <summary>
        /// Handles the Click event of the lbMoved control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMoved_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
            {
                hfStreet1.Value = acAddress.Street1;
                hfStreet2.Value = acAddress.Street2;
                hfCity.Value = acAddress.City;
                hfState.Value = acAddress.State;
                hfPostalCode.Value = acAddress.PostalCode;
                hfCountry.Value = acAddress.Country;

                Location currentAddress = new Location();
                acAddress.Required = true;
                acAddress.GetValues( currentAddress );
                lPreviousAddress.Text = string.Format( "<strong>Previous Address</strong><br />{0}", currentAddress.FormattedHtmlAddress );

                acAddress.Street1 = string.Empty;
                acAddress.Street2 = string.Empty;
                acAddress.PostalCode = string.Empty;
                acAddress.City = string.Empty;

                cbIsMailingAddress.Checked = true;
                cbIsPhysicalAddress.Checked = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSaveFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveFamily_Click( object sender, EventArgs e )
        {
            pnlEditFamily.Visible = false;
            pnlMainEntry.Visible = true;

            var rockContext = new RockContext();

            if ( !_selectedGroupId.HasValue )
            {
                return;
            }

            var group = new GroupService( rockContext ).Get( _selectedGroupId.Value );

            // invalid situation.
            if ( group == null )
            {
                return;
            }

            Guid familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            var familyGroup = new GroupService( rockContext ).Get( _selectedGroupId.Value );
            if ( familyGroup != null )
            {
                var groupLocationService = new GroupLocationService( rockContext );
                var dvHomeAddressType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
                var familyAddress = groupLocationService.Queryable().Where( l => l.GroupId == familyGroup.Id && l.GroupLocationTypeValueId == dvHomeAddressType.Id ).FirstOrDefault();
                if ( familyAddress != null && string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                {
                    // delete the current address
                    groupLocationService.Delete( familyAddress );
                    rockContext.SaveChanges();
                }
                else
                {
                    if ( !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                    {
                        if ( familyAddress == null )
                        {
                            familyAddress = new GroupLocation();
                            groupLocationService.Add( familyAddress );
                            familyAddress.GroupLocationTypeValueId = dvHomeAddressType.Id;
                            familyAddress.GroupId = familyGroup.Id;
                            familyAddress.IsMailingLocation = true;
                            familyAddress.IsMappedLocation = true;
                        }
                        else if ( hfStreet1.Value != string.Empty )
                        {
                            // user clicked move so create a previous address
                            var previousAddress = new GroupLocation();
                            groupLocationService.Add( previousAddress );

                            var previousAddressValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                            if ( previousAddressValue != null )
                            {
                                previousAddress.GroupLocationTypeValueId = previousAddressValue.Id;
                                previousAddress.GroupId = familyGroup.Id;

                                Location previousAddressLocation = new Location();
                                previousAddressLocation.Street1 = hfStreet1.Value;
                                previousAddressLocation.Street2 = hfStreet2.Value;
                                previousAddressLocation.City = hfCity.Value;
                                previousAddressLocation.State = hfState.Value;
                                previousAddressLocation.PostalCode = hfPostalCode.Value;
                                previousAddressLocation.Country = hfCountry.Value;

                                previousAddress.Location = previousAddressLocation;
                            }
                        }

                        familyAddress.IsMailingLocation = cbIsMailingAddress.Checked;
                        familyAddress.IsMappedLocation = cbIsPhysicalAddress.Checked;

                        var loc = new Location();
                        acAddress.GetValues( loc );

                        familyAddress.Location = new LocationService( rockContext ).Get(
                            loc.Street1, loc.Street2, loc.City, loc.State, loc.PostalCode, loc.Country, familyGroup, true );

                        // since there can only be one mapped location, set the other locations to not mapped
                        if ( familyAddress.IsMappedLocation )
                        {
                            var groupLocations = groupLocationService.Queryable()
                                .Where( l => l.GroupId == familyGroup.Id && l.Id != familyAddress.Id ).ToList();

                            foreach ( var groupLocation in groupLocations )
                            {
                                groupLocation.IsMappedLocation = false;
                            }
                        }

                        rockContext.SaveChanges();
                    }
                }

                familyGroup.LoadAttributes();
                avcFamilyAttributes.GetEditValues( familyGroup );
                familyGroup.SaveAttributeValues();

                foreach ( var searchResult in _searchResultsState.Where( a => a.FamilyId == familyGroup.Id ) )
                {
                    searchResult.Address = familyAddress.Location;
                }
            }

            BindMainPanel();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            BindMainPanel();
        }
        #endregion Edit Family Events

        #region Edit Members Events


        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedId = rblRole.SelectedValueAsId();

            var rockContext = new RockContext();
            Person person = new Person();
            var personGuid = hfPersonGuid.Value.AsGuid();
            if ( personGuid != Guid.Empty )
            {
                person = new PersonService( rockContext ).Get( personGuid );
            }
            GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( rockContext );
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var groupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            if ( selectedId.HasValue )
            {

                bool isAdult = groupTypeRoleService.Queryable().Where( gr =>
                               gr.GroupType.Guid == groupTypeGuid &&
                               gr.Guid == adultGuid &&
                               gr.Id == selectedId ).Any();

                BindPersonDetailByRole( person, isAdult );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSaveMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveMember_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personGuid = hfPersonGuid.Value.AsGuid();

            // invalid situation/tampering; return and report nothing.
            if ( !_selectedGroupId.HasValue )
            {
                return;
            }

            var group = new GroupService( rockContext ).Get( _selectedGroupId.Value );

            // invalid situation; return and report nothing.
            if ( group == null )
            {
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                var personService = new PersonService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );

                if ( personGuid == Guid.Empty )
                {
                    var groupMember = new GroupMember() { Person = new Person(), Group = group, GroupId = group.Id };
                    groupMember.Person.FirstName = tbFirstName.Text;
                    groupMember.Person.LastName = tbLastName.Text;
                    groupMember.Person.SuffixValueId = dvpSuffix.SelectedValueAsId();

                    var role = group.GroupType.Roles.Where( r => r.Id == ( rblRole.SelectedValueAsInt() ?? 0 ) ).FirstOrDefault();
                    if ( role != null )
                    {
                        groupMember.GroupRole = role;
                        groupMember.GroupRoleId = role.Id;
                    }

                    var headOfHousehold = GroupServiceExtensions.HeadOfHousehold( group.Members.AsQueryable() );
                    if ( headOfHousehold != null )
                    {
                        DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( headOfHousehold.RecordStatusValueId ?? 0 );
                        if ( dvcRecordStatus != null )
                        {
                            groupMember.Person.RecordStatusValueId = dvcRecordStatus.Id;
                        }
                    }

                    if ( groupMember.GroupRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                    {
                        groupMember.Person.GivingGroupId = group.Id;
                    }

                    groupMember.Person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                    if ( pnlEmail.Visible )
                    {
                        groupMember.Person.Email = tbEmail.Text.Trim();
                        groupMember.Person.IsEmailActive = cbIsEmailActive.Checked;
                        if ( rblCommunicationPreference.Visible )
                        {
                            groupMember.Person.CommunicationPreference = rblCommunicationPreference.SelectedValueAsEnum<CommunicationType>();
                        }
                    }

                    groupMemberService.Add( groupMember );
                    rockContext.SaveChanges();
                    personGuid = groupMember.Person.Guid;
                    hfPersonGuid.Value = groupMember.Person.Guid.ToString();
                }

                var person = personService.Get( personGuid );
                if ( person != null )
                {
                    var groupMember = groupMemberService.Queryable( "Person", true ).Where( m =>
                                    m.PersonId == person.Id &&
                                    m.GroupId == group.Id )
                                    .FirstOrDefault();
                    person.FirstName = tbFirstName.Text;
                    person.LastName = tbLastName.Text;
                    person.SuffixValueId = dvpSuffix.SelectedValueAsInt();
                    person.Gender = rblGender.SelectedValue.ConvertToEnum<Gender>();
                    var birthMonth = person.BirthMonth;
                    var birthDay = person.BirthDay;
                    var birthYear = person.BirthYear;

                    var birthday = bpBirthDay.SelectedDate;
                    if ( birthday.HasValue )
                    {
                        // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                        var today = RockDateTime.Today;
                        while ( birthday.Value.CompareTo( today ) > 0 )
                        {
                            birthday = birthday.Value.AddYears( -100 );
                        }

                        person.BirthMonth = birthday.Value.Month;
                        person.BirthDay = birthday.Value.Day;
                        if ( birthday.Value.Year != DateTime.MinValue.Year )
                        {
                            person.BirthYear = birthday.Value.Year;
                        }
                        else
                        {
                            person.BirthYear = null;
                        }
                    }
                    else
                    {
                        person.SetBirthDate( null );
                    }

                    if ( gpGradePicker.Visible )
                    {
                        if ( gpGradePicker.SelectedGradeValue != null )
                        {
                            person.GradeOffset = gpGradePicker.SelectedGradeValue.Value.AsIntegerOrNull();
                        }
                        else
                        {
                            person.GradeOffset = null;
                        }
                    }

                    if ( dvpMaritalStatus.Visible )
                    {
                        person.MaritalStatusValueId = dvpMaritalStatus.SelectedValueAsInt();
                    }

                    bool isAdult = false;
                    var role = group.GroupType.Roles.Where( r => r.Id == ( rblRole.SelectedValueAsInt() ?? 0 ) ).FirstOrDefault();
                    if ( role.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                    {
                        isAdult = true;
                    }

                    if ( groupMember != null )
                    {
                        groupMember.GroupRoleId = role.Id;
                    }

                    var selectedPhoneTypeGuids = ( isAdult ? GetAttributeValue( AttributeKey.AdultPhoneTypes ) : GetAttributeValue( AttributeKey.ChildPhoneTypes ) ).Split( ',' ).AsGuidList();
                    var phoneNumberTypeIds = new List<int>();

                    bool smsSelected = false;

                    foreach ( RepeaterItem item in rContactInfo.Items )
                    {
                        HiddenField hfPhoneType = item.FindControl( "hfPhoneType" ) as HiddenField;
                        PhoneNumberBox pnbPhone = item.FindControl( "pnbPhone" ) as PhoneNumberBox;
                        CheckBox cbUnlisted = item.FindControl( "cbUnlisted" ) as CheckBox;
                        CheckBox cbSms = item.FindControl( "cbSms" ) as CheckBox;

                        if ( hfPhoneType != null &&
                            pnbPhone != null &&
                            cbSms != null &&
                            cbUnlisted != null )
                        {
                            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
                            {
                                int phoneNumberTypeId;
                                if ( int.TryParse( hfPhoneType.Value, out phoneNumberTypeId ) )
                                {
                                    var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberTypeId );
                                    string oldPhoneNumber = string.Empty;
                                    if ( phoneNumber == null )
                                    {
                                        phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberTypeId };
                                        person.PhoneNumbers.Add( phoneNumber );
                                    }
                                    else
                                    {
                                        oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                                    }

                                    phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                                    phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );

                                    // Only allow one number to have SMS selected
                                    if ( smsSelected )
                                    {
                                        phoneNumber.IsMessagingEnabled = false;
                                    }
                                    else
                                    {
                                        phoneNumber.IsMessagingEnabled = cbSms.Checked;
                                        smsSelected = cbSms.Checked;
                                    }

                                    phoneNumber.IsUnlisted = cbUnlisted.Checked;
                                    phoneNumberTypeIds.Add( phoneNumberTypeId );
                                }
                            }
                        }
                    }

                    // Remove any blank numbers
                    var phoneNumberService = new PhoneNumberService( rockContext );
                    foreach ( var phoneNumber in person.PhoneNumbers
                        .Where( n => n.NumberTypeValueId.HasValue && !phoneNumberTypeIds.Contains( n.NumberTypeValueId.Value ) && selectedPhoneTypeGuids.Contains( n.NumberTypeValue.Guid ) )
                        .ToList() )
                    {
                        person.PhoneNumbers.Remove( phoneNumber );
                        phoneNumberService.Delete( phoneNumber );
                    }

                    person.LoadAttributes();
                    avcPersonAttributes.GetEditValues( person );
                    rockContext.SaveChanges();
                }
            } );

            ShowMainPanel( SelectedPersonId );

        }

        #endregion Edit Members Events

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Creates the rapid attendance cookie.
        /// </summary>
        private void CreateRapidAttendanceCookie( AttendanceSetting attendanceSetting )
        {
            HttpCookie httpcookie = new HttpCookie( ROCK_RAPIDATTENDANCEENTRY );
            httpcookie.Expires = RockDateTime.Now.AddMinutes( 480 );
            httpcookie.Values.Add( GROUP_ID, attendanceSetting.GroupId.ToString() );
            httpcookie.Values.Add( LOCATION_ID, attendanceSetting.LocationId.ToString() );
            httpcookie.Values.Add( SCHEDULE_ID, attendanceSetting.ScheduleId.ToString() );
            httpcookie.Values.Add( ATTENDANCE_DATE, attendanceSetting.AttendanceDate.ToString() );
            Response.Cookies.Add( httpcookie );
        }

        /// <summary>
        /// Creates the rapid attendance cookie.
        /// </summary>
        private AttendanceSetting GetRapidAttendanceCookie()
        {
            HttpCookie rapidAttendanceEntryCookie = Request.Cookies[ROCK_RAPIDATTENDANCEENTRY];
            if ( rapidAttendanceEntryCookie != null )
            {
                AttendanceSetting attendanceSetting = new AttendanceSetting();
                attendanceSetting.GroupId = rapidAttendanceEntryCookie.Values[GROUP_ID].AsInteger();
                attendanceSetting.LocationId = rapidAttendanceEntryCookie.Values[LOCATION_ID].AsInteger();
                attendanceSetting.ScheduleId = rapidAttendanceEntryCookie.Values[SCHEDULE_ID].AsInteger();
                attendanceSetting.AttendanceDate = rapidAttendanceEntryCookie.Values[ATTENDANCE_DATE].AsDateTime() ?? RockDateTime.Now;
                return attendanceSetting;
            }
            return null;
        }

        /// <summary>
        /// Update the locations drop down to match the valid locations for the currently
        /// selected group.
        /// </summary>
        private void UpdateLocations( int? locationId = null )
        {
            pnlLocationPicker.Visible = false;
            ddlLocation.Items.Clear();
            ddlLocation.Items.Add( new ListItem() );

            int? groupId = GetSelectedGroupId();

            if ( groupId.HasValue )
            {
                var group = new GroupService( new RockContext() ).Get( groupId.Value );
                var groupLocations = group.GroupLocations;

                //
                // Add all the locations to the drop down list.
                //
                foreach ( var groupLocation in groupLocations )
                {
                    ddlLocation.Items.Add( new ListItem( groupLocation.Location.Name, groupLocation.Id.ToString() ) );
                }

                //
                // If there is only one location then select it, otherwise show the picker and let the user select.
                //
                if ( groupLocations.Count == 1 )
                {
                    ddlLocation.SelectedIndex = 1;
                }
                else
                {
                    pnlLocationPicker.Visible = true;
                }

                if ( pnlLocationPicker.Visible )
                {
                    ddlLocation.SetValue( locationId );
                }
            }
        }

        /// <summary>
        /// Get the selected group identifier.
        /// </summary>
        private int? GetSelectedGroupId()
        {
            int? groupId = null;
            if ( gpGroups.Visible )
            {
                groupId = gpGroups.GroupId;
            }
            else
            {
                groupId = ddlGroup.SelectedValueAsInt();
            }

            return groupId;
        }

        /// <summary>
        /// Update the schedules drop down to match the valid schedules for the currently
        /// selected group locations.
        /// </summary>
        private void UpdateSchedules( int? scheduleId = null )
        {
            pnlSchedulePicker.Visible = false;
            ddlSchedule.Items.Clear();
            ddlSchedule.Items.Add( new ListItem() );
            int? groupId = GetSelectedGroupId();

            if ( !string.IsNullOrWhiteSpace( ddlLocation.SelectedValue ) && groupId.HasValue )
            {
                var rockContext = new RockContext();

                var group = new GroupService( rockContext ).Get( groupId.Value );
                var groupLocation = new GroupLocationService( rockContext ).Get( ddlLocation.SelectedValue.AsInteger() );
                var schedules = groupLocation.Schedules.ToList();

                // TODO: Should keep?
                if ( group.Schedule != null )
                {
                    schedules.Add( group.Schedule );
                }

                //
                // Add all the schedules to the drop down list.
                //
                foreach ( var schedule in schedules )
                {
                    ddlSchedule.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
                }

                //
                // If there is only one schedule then select it, otherwise show the picker and let the user select.
                //
                if ( schedules.Count == 1 )
                {
                    ddlSchedule.SelectedIndex = 1;
                }
                else
                {
                    pnlSchedulePicker.Visible = true;
                }

                if ( pnlSchedulePicker.Visible )
                {
                    ddlSchedule.SetValue( scheduleId );
                }
            }
        }

        /// <summary>
        /// Show all details. This also clears any existing selections the user may have made.
        /// </summary>
        private void ShowDetails()
        {
            if ( IsAttendanceEnabled )
            {
                if ( _attendanceSettingState == null )
                {
                    _attendanceSettingState = GetRapidAttendanceCookie();
                }
            }

            if ( IsAttendanceEnabled && _attendanceSettingState == null )
            {
                ShowAttendanceSetting();
            }
            else
            {
                ShowMainPanel( PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull() );
            }
        }

        /// <summary>
        /// Shows the attendance setting.
        /// </summary>
        private void ShowAttendanceSetting( AttendanceSetting attendanceSetting = null )
        {
            pnlStart.Visible = true;
            pnlMainPanel.Visible = false;

            var rockContext = new RockContext();
            pnlGroupPicker.Visible = false;
            ddlGroup.Visible = false;
            gpGroups.Visible = false;

            if ( GetAttributeValue( AttributeKey.AttendanceGroup ).AsGuid() != default( Guid ) )
            {
                ddlGroup.Items.Clear();
                var attendanceGroupGuid = GetAttributeValue( AttributeKey.AttendanceGroup ).AsGuid();
                var group = new GroupService( rockContext ).Get( attendanceGroupGuid );
                ddlGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                ddlGroup.Visible = true;
                ddlGroup.SelectedIndex = 0;
            }
            else
            {

                if ( GetAttributeValue( AttributeKey.ParentGroup ).AsGuid() != default( Guid ) )
                {

                    var parentGroupGuid = GetAttributeValue( AttributeKey.ParentGroup ).AsGuid();
                    var groups = new GroupService( rockContext )
                                .Queryable()
                                .Where( a => a.ParentGroup.Guid == parentGroupGuid && a.IsActive )
                                .OrderBy( g => g.Order )
                                .ToList();
                    if ( groups.Count != 1 )
                    {
                        pnlGroupPicker.Visible = true;
                        ddlGroup.Items.Add( new ListItem() );
                    }

                    foreach ( var group in groups )
                    {
                        ddlGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                    }

                    if ( groups.Count == 1 )
                    {
                        ddlGroup.SelectedIndex = 0;
                    }

                    ddlGroup.Visible = true;
                }
                else
                {
                    pnlGroupPicker.Visible = true;
                    gpGroups.Visible = true;
                }
            }

            if ( attendanceSetting != null )
            {
                dpAttendanceDate.SelectedDate = attendanceSetting.AttendanceDate;
                if ( pnlGroupPicker.Visible )
                {
                    if ( ddlGroup.Visible )
                    {
                        ddlGroup.SetValue( attendanceSetting.GroupId );
                    }
                    else
                    {
                        gpGroups.SetValue( attendanceSetting.GroupId );
                    }
                }

                UpdateLocations( attendanceSetting.LocationId );
                UpdateSchedules( attendanceSetting.ScheduleId );
            }
        }

        /// <summary>
        /// Shows the main entry panel.
        /// </summary>
        private void ShowMainPanel( int? selectedPersonId = null )
        {
            pnlStart.Visible = false;
            pnlMainPanel.Visible = true;

            var enableAttendance = GetAttributeValue( AttributeKey.EnableAttendance ).AsBoolean();
            if ( enableAttendance && _attendanceSettingState != null )
            {
                var rockContext = new RockContext();
                var attendanceService = new AttendanceService( rockContext );
                IEnumerable<Attendance> attendance = new List<Attendance>();

                var groupLocation = new GroupLocationService( rockContext ).Get( _attendanceSettingState.LocationId );
                var schedule = new ScheduleService( rockContext ).Get( _attendanceSettingState.ScheduleId );

                attendance = attendanceService.Queryable()
                    .Where( a =>
                        a.DidAttend == true &&
                        a.Occurrence.GroupId == _attendanceSettingState.GroupId &&
                        a.Occurrence.OccurrenceDate == _attendanceSettingState.AttendanceDate &&
                        a.Occurrence.LocationId == groupLocation.LocationId &&
                        a.Occurrence.ScheduleId == _attendanceSettingState.ScheduleId );

                hlAttendance.Text = string.Format( "{0} - {1} - {2}", groupLocation.Group.Name, schedule.Name, _attendanceSettingState.AttendanceDate.ToShortDateString() );
                if ( GetAttributeValue( AttributeKey.AttendanceListPage ).IsNotNullOrWhiteSpace() )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "GroupId", _attendanceSettingState.GroupId.ToString() );
                    qryParams.Add( "LocationId", _attendanceSettingState.LocationId.ToString() );
                    qryParams.Add( "ScheduleId", _attendanceSettingState.ScheduleId.ToString() );
                    qryParams.Add( "AttendanceDate", _attendanceSettingState.AttendanceDate.ToShortDateString() );
                    string url = LinkedPageUrl( AttributeKey.AttendanceListPage, qryParams );
                    hlCurrentCount.Text = string.Format( "<a href='{0}'>Count: {1}</a>", url, attendance.Count() );
                }
                else
                {
                    hlCurrentCount.Text = string.Format( "Count: {0}", attendance.Count() );
                }
            }

            GetSearchResults( selectedPersonId );
            BindMainPanel();
        }

        /// <summary>
        /// Get the search results
        /// </summary>
        private void GetSearchResults( int? selectedPersonId = null )
        {
            bool reversed = false;
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var inactiveRecordStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );
            var homeGroupLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );

            List<int> personIds = null;
            IEnumerable<Person> persons;
            if ( selectedPersonId.HasValue && tbSearch.Text.IsNullOrWhiteSpace() )
            {
                var person = personService.Get( selectedPersonId.Value );
                personIds = personService.GetFamilyMembers( person.PrimaryFamily, person.Id, true ).Where( a => !a.Person.IsDeceased ).Select( a => a.PersonId ).ToList();
                persons = new List<Person>() { person };
            }
            else
            {
                persons = personService
                    .GetByFullNameOrdered( tbSearch.Text.Trim(), false, false, false, out reversed )
                    .Where( a => a.PrimaryFamilyId.HasValue )
                    .AsEnumerable();
            }

            List<int> attendedPersonIds = new List<int>();
            if ( IsAttendanceEnabled )
            {
                attendedPersonIds = GetAttendedPersonIds( rockContext, personIds );
            }

            bool includeCampusName = false;
            if ( CampusCache.All( false ).Count > 1 )
            {
                includeCampusName = true;
            }

            _searchResultsState = new List<SearchResult>();
            foreach ( var person in persons )
            {
                if ( person.PrimaryFamily != null )
                {
                    SearchResult searchResult = new SearchResult()
                    {
                        Id = person.Id,
                        Guid = person.Guid,
                        Name = person.NickName + " " + person.LastName,
                        IsActive = person.RecordStatusValueId != inactiveRecordStatus.Id,
                        Email = person.Email,
                        FamilyName = person.PrimaryFamily.Name,
                        FamilyId = person.PrimaryFamilyId.Value,
                        Age = person.Age
                    };

                    if ( includeCampusName && person.PrimaryFamily.Campus != null )
                    {
                        searchResult.CampusName = person.PrimaryFamily.Campus.Name;
                    }

                    searchResult.FamilyMembers = personService
                                            .GetFamilyMembers( person.PrimaryFamily, person.Id, true )
                                            .Where( a => !a.Person.IsDeceased )
                                            .Select( a => a.Person )
                                            .ToList();
                    var mobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    if ( mobilePhone != null )
                    {
                        searchResult.Mobile = mobilePhone.NumberFormatted;
                    }
                    var location = person.PrimaryFamily.GroupLocations.Where( a => a.GroupLocationTypeValueId == homeGroupLocationType.Id ).FirstOrDefault();
                    if ( location != null )
                    {
                        searchResult.Address = location.Location;
                    }

                    if ( IsAttendanceEnabled )
                    {
                        var familyPersonIds = searchResult.FamilyMembers.Select( b => b.Id ).ToList();
                        searchResult.IsAttended = attendedPersonIds.Any( a => familyPersonIds.Contains( a ) );
                    }

                    _searchResultsState.Add( searchResult );
                }
            }

            if ( selectedPersonId.HasValue && _searchResultsState.Any( a => a.Id == selectedPersonId.Value ) )
            {
                var searchResult = _searchResultsState.First( a => a.Id == selectedPersonId.Value );
                SelectedPersonId = searchResult.Id;
                _selectedGroupId = searchResult.FamilyId;
                if ( !searchResult.FamilyMembers.Any( a => a.Guid == hfPersonGuid.Value.AsGuid() ) )
                {
                    hfPersonGuid.Value = searchResult.Guid.ToString();
                }
            }
        }

        /// <summary>
        /// Get the attended person identifiers.
        /// </summary>
        private List<int> GetAttendedPersonIds( RockContext rockContext = null, List<int> personIds = null )
        {
            rockContext = rockContext ?? new RockContext();
            var groupLocation = new GroupLocationService( rockContext ).Get( _attendanceSettingState.LocationId );
            var attendanceService = new AttendanceService( rockContext );
            var attendanceQry = attendanceService.Queryable()
                            .Where( a =>
                                a.DidAttend == true &&
                                a.Occurrence.GroupId == _attendanceSettingState.GroupId &&
                                a.Occurrence.OccurrenceDate == _attendanceSettingState.AttendanceDate &&
                                a.Occurrence.LocationId == groupLocation.LocationId &&
                                a.Occurrence.ScheduleId == _attendanceSettingState.ScheduleId );
            if ( personIds != null && personIds.Count > 0 )
            {
                attendanceQry = attendanceQry.Where( a => personIds.Contains( a.PersonAlias.PersonId ) );
            }

            return attendanceQry
                .Select( a => a.PersonAlias.PersonId )
                .ToList();
        }

        /// <summary>
        /// Binds the controls in main panel.
        /// </summary>
        private void BindMainPanel()
        {
            hfAttendanceDirty.Value = "false";
            rptResults.DataSource = _searchResultsState;
            rptResults.DataBind();

            BindMainEntryPanel();
        }

        /// <summary>
        /// Binds the controls in main entry panel.
        /// </summary>
        private void BindMainEntryPanel()
        {
            pnlEditMember.Visible = false;
            pnlEditFamily.Visible = false;
            pnlMainEntry.Visible = SelectedPersonId.HasValue;
            ShowFamilyActionButton( SelectedPersonId.HasValue );
            lFamilyDetail.Text = string.Empty;
            if ( SelectedPersonId.HasValue )
            {
                var searchResult = _searchResultsState.FirstOrDefault( a => a.Id == SelectedPersonId );
                if ( searchResult != null )
                {
                    var group = new GroupService( new RockContext() ).Get( searchResult.FamilyId );

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Family", group );

                    lFamilyDetail.Text = GetAttributeValue( AttributeKey.FamilyHeaderLavaTemplate ).ResolveMergeFields( mergeFields );
                    //lAddress.Text = searchResult.Address != null ? searchResult.Address.FormattedHtmlAddress : "";

                    bool enableAttendance = GetAttributeValue( AttributeKey.EnableAttendance ).AsBoolean() && _attendanceSettingState != null;
                    rcwAttendance.Visible = enableAttendance;
                    List<Person> guests = new List<Person>();
                    if ( enableAttendance )
                    {
                        var personIds = searchResult.FamilyMembers.Select( a => a.Id ).ToList();
                        if ( GetAttributeValue( AttributeKey.ShowCanCheckInRelationships ).AsBoolean() )
                        {
                            Guid knownRelationshipGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
                            Guid knownRelationshipOwner = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER );
                            Guid knownRelationshipCanCheckin = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN );

                            RockContext rockContext = new RockContext();
                            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                            PersonService personService = new PersonService( rockContext );

                            var familyMembersKnownRelationshipGroups = new GroupMemberService( rockContext ).Queryable()
                                                    .Where( g => g.Group.GroupType.Guid == knownRelationshipGuid
                                                                    && g.GroupRole.Guid == knownRelationshipOwner
                                                                    && personIds.Contains( g.PersonId ) )
                                                    .Select( m => m.GroupId );
                            guests = groupMemberService.Queryable()
                                                    .Where( g => g.GroupRole.Guid == knownRelationshipCanCheckin
                                                                    && familyMembersKnownRelationshipGroups.Contains( g.GroupId ) && !g.Person.IsDeceased )
                                                    .Select( g => g.Person )
                                                    .Distinct().ToList();

                            if ( guests.Any() )
                            {
                                personIds.AddRange( guests.Select( a => a.Id ).ToList() );
                            }
                        }

                        var attendedPersonIds = GetAttendedPersonIds( personIds: personIds );
                        rcbAttendance.Items.Clear();
                        foreach ( var member in searchResult.FamilyMembers )
                        {
                            bool enabled = true;
                            string text = member.NickName;
                            if ( member.Age.HasValue )
                            {
                                if ( member.Age.Value < GetAttributeValue( AttributeKey.AttendanceAgeLimit ).AsInteger() )
                                {
                                    enabled = false;
                                }
                                text += string.Format( " ({0})", member.Age );
                            }
                            rcbAttendance.Items.Add( new ListItem( text, member.Id.ToString(), enabled ) );
                        }

                        foreach ( var member in guests )
                        {
                            bool enabled = true;
                            string text = member.NickName;
                            if ( member.Age.HasValue )
                            {
                                if ( member.Age.Value < GetAttributeValue( AttributeKey.AttendanceAgeLimit ).AsInteger() )
                                {
                                    enabled = false;
                                }
                                text += string.Format( " ({0})", member.Age );
                            }
                            rcbAttendance.Items.Add( new ListItem( text, member.Id.ToString(), enabled ) );
                        }
                        rcbAttendance.SetValues( attendedPersonIds );
                    }

                    ShowMainEntryPersonDetail( searchResult );
                }
            }
        }

        #region Main Entry Person Methods

        /// <summary>
        /// Binds the person detail to main entry person section.
        /// </summary>
        private void ShowMainEntryPersonDetail( SearchResult searchResult )
        {
            hfPersonDirty.Value = "false";
            rptPersons.DataSource = searchResult.FamilyMembers;
            rptPersons.DataBind();

            var selectedPersonGuid = hfPersonGuid.Value.AsGuid();
            var person = searchResult.FamilyMembers.FirstOrDefault( a => a.Guid == selectedPersonGuid );
            var personMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            personMergeFields.Add( "Person", person );
            lPersonDetail.Text = GetAttributeValue( AttributeKey.IndividualHeaderLavaTemplate ).ResolveMergeFields( personMergeFields );

            var workflowTypeGuids = GetAttributeValue( AttributeKey.WorkflowTypes ).SplitDelimitedValues().AsGuidList();
            rcbWorkFlowTypes.Visible = workflowTypeGuids.Any();
            if ( workflowTypeGuids.Count > 0 )
            {
                rcbWorkFlowTypes.Label = GetAttributeValue( AttributeKey.WorkflowListTitle );
                var workFlowTypes = workflowTypeGuids.Select( a => WorkflowTypeCache.Get( a ) ).OrderBy( a => a.Order );
                rcbWorkFlowTypes.Items.Clear();
                foreach ( var workflowType in workFlowTypes )
                {
                    rcbWorkFlowTypes.Items.Add( new ListItem( workflowType.Name, workflowType.Guid.ToString() ) );
                }
            }

            var configuredNoteTypeGuids = GetAttributeValue( AttributeKey.NoteTypes ).SplitDelimitedValues().AsGuidList();
            rcwNotes.Visible = configuredNoteTypeGuids.Any();
            if ( configuredNoteTypeGuids.Any() )
            {
                var noteTypes = configuredNoteTypeGuids.Select( a => NoteTypeCache.Get( a ) ).OrderBy( a => a.Order ).ToList();
                if ( rcwNotes.Visible )
                {
                    ddlNoteType.DataSource = noteTypes;
                    ddlNoteType.DataTextField = "Name";
                    ddlNoteType.DataValueField = "Id";
                    ddlNoteType.DataBind();

                    ddlNoteType.Visible = noteTypes.Count > 1;
                    if ( noteTypes.Count > 1 )
                    {
                        ddlNoteType.Items.Insert( 0, new ListItem() );
                    }
                }

                var noteTypeId = ddlNoteType.SelectedValueAsId();
                tbNote.Enabled = noteTypeId.HasValue;
                tbNote.Text = string.Empty;
            }

            pnlPrayerRequest.Visible = GetAttributeValue( AttributeKey.EnablePrayerRequestEntry ).AsBoolean();
            if ( pnlPrayerRequest.Visible )
            {
                pnlIsPublic.Visible = GetAttributeValue( AttributeKey.ShowPublicFlag ).AsBoolean();
                if ( pnlIsPublic.Visible )
                {
                    cbIsPublic.Checked = GetAttributeValue( AttributeKey.DisplayToPublic ).AsBoolean();
                }

                pnlIsUrgent.Visible = GetAttributeValue( AttributeKey.ShowUrgentFlag ).AsBoolean();
                if ( pnlIsUrgent.Visible )
                {
                    cbIsUrgent.Checked = false;
                }

                tbPrayerRequest.Text = string.Empty;

                cpPrayerCategory.Visible = GetAttributeValue( AttributeKey.EnableCategorySelection ).AsBoolean();
                if ( cpPrayerCategory.Visible )
                {
                    // set the default category
                    if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.DefaultCategory ) ) )
                    {
                        Guid defaultCategoryGuid = GetAttributeValue( AttributeKey.DefaultCategory ).AsGuid();
                        var defaultCategoryId = CategoryCache.Get( defaultCategoryGuid ).Id;
                        cpPrayerCategory.SetValue( defaultCategoryId );
                    }
                }
            }

            bbtnSaveContactItems.Visible = pnlPrayerRequest.Visible || rcbWorkFlowTypes.Visible || rcwNotes.Visible;
        }

        /// <summary>
        /// Launch the workflows.
        /// </summary>
        private void LaunchWorkflows( WorkflowService workflowService, Guid workflowTypeGuid, string name, object entity,
                                        Guid? groupGuid = null, Guid? scheduleGuid = null, Guid? locationGuid = null )
        {
            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );
            if ( workflowType != null )
            {
                var workflow = Workflow.Activate( workflowType, name );
                if ( groupGuid.HasValue && scheduleGuid.HasValue && locationGuid.HasValue && _attendanceSettingState != null )
                {
                    workflow.SetAttributeValue( "DateSelected", _attendanceSettingState.AttendanceDate.ToString( "o" ) );
                    workflow.SetAttributeValue( "Group", groupGuid );
                    workflow.SetAttributeValue( "Schedule", scheduleGuid );
                    workflow.SetAttributeValue( "Location", locationGuid );
                }

                List<string> workflowErrors;
                workflowService.Process( workflow, entity, out workflowErrors );
            }
        }

        #endregion Main Entry Person Methods

        #region Edit Member Methods

        /// <summary>
        /// Shows the edit person details.
        /// </summary>
        /// <param name="personGuid">The person's global unique identifier.</param>
        private void ShowEditPersonDetails( Guid personGuid )
        {
            pnlEditMember.Visible = true;
            pnlMainEntry.Visible = false;
            ShowFamilyActionButton( false );

            RockContext rockContext = new RockContext();

            GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( rockContext );
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var groupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            bool isChild = false;
            var adultRole = groupTypeRoleService.Queryable().Where( gr =>
                           gr.GroupType.Guid == groupTypeGuid &&
                           gr.Guid == adultGuid ).FirstOrDefault();
            if ( adultRole != null )
            {
                rblRole.SelectedValue = adultRole.Id.ToString();
            }

            var family = new GroupService( rockContext ).Get( _selectedGroupId.Value );
            if ( family != null )
            {
                hfPersonGuid.Value = personGuid.ToString();
                var person = new Person();
                if ( personGuid != Guid.Empty )
                {
                    person = new PersonService( rockContext ).Get( personGuid );
                    var groupRole = family.Members.Where( gm => gm.PersonId == person.Id ).Select( a => a.GroupRole ).FirstOrDefault();
                    if ( groupRole != null )
                    {
                        rblRole.SelectedValue = groupRole.Id.ToString();
                        if ( groupRole != null && adultRole != null && groupRole.Id != adultRole.Id )
                        {
                            isChild = true;
                        }
                    }
                }

                if ( person != null )
                {
                    tbFirstName.Text = person.FirstName;
                    tbLastName.Text = person.LastName;
                    dvpSuffix.SetValue( person.SuffixValueId );
                    bpBirthDay.SelectedDate = person.BirthDate;
                    rblGender.SelectedValue = person.Gender.ConvertToString();
                }

                BindPersonDetailByRole( person, !isChild );
            }
        }

        /// <summary>
        /// Shows the family action button.
        /// </summary>
        private void ShowFamilyActionButton( bool isVisible )
        {
            lbEditFamily.Visible = isVisible;
            lbAddMember.Visible = isVisible;
        }

        /// <summary>
        /// Bind the phone numbers to the repeater.
        /// </summary>
        private void BindPhoneNumbers( bool isAdult, Person person = null )
        {
            if ( person == null )
                person = new Person();

            var phoneNumbers = new List<PhoneNumber>();
            var phoneNumberTypes = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
            var mobilePhoneType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );
            var selectedPhoneTypeGuids = ( isAdult ? GetAttributeValue( AttributeKey.AdultPhoneTypes ) : GetAttributeValue( AttributeKey.ChildPhoneTypes ) ).Split( ',' ).AsGuidList();

            if ( phoneNumberTypes.DefinedValues.Where( pnt => selectedPhoneTypeGuids.Contains( pnt.Guid ) ).Any() )
            {
                foreach ( var phoneNumberType in phoneNumberTypes.DefinedValues.Where( pnt => selectedPhoneTypeGuids.Contains( pnt.Guid ) ) )
                {
                    var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberType.Id );
                    if ( phoneNumber == null )
                    {
                        var numberType = new DefinedValue();
                        numberType.Id = phoneNumberType.Id;
                        numberType.Value = phoneNumberType.Value;
                        numberType.Guid = phoneNumberType.Guid;
                        phoneNumber = new PhoneNumber { NumberTypeValueId = numberType.Id, NumberTypeValue = numberType };
                        phoneNumber.IsMessagingEnabled = mobilePhoneType != null && phoneNumberType.Id == mobilePhoneType.Id;
                    }
                    else
                    {
                        // Update number format, just in case it wasn't saved correctly
                        phoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number );
                    }

                    phoneNumbers.Add( phoneNumber );
                }

                rContactInfo.DataSource = phoneNumbers;
                rContactInfo.DataBind();
            }
        }

        /// <summary>
        /// Bind the person detail on the basis of role.
        /// </summary>
        private void BindPersonDetailByRole( Person person, bool isAdult )
        {
            gpGradePicker.Visible = !isAdult;
            dvpMaritalStatus.Visible = isAdult;
            if ( isAdult )
            {
                dvpMaritalStatus.SetValue( person.MaritalStatusValueId );
                pnlEmail.Visible = true;
                tbEmail.Text = person.Email;
                cbIsEmailActive.Checked = person.IsEmailActive;
                rblCommunicationPreference.Visible = GetAttributeValue( AttributeKey.ShowCommunicationPreference ).AsBoolean();
                if ( rblCommunicationPreference.Visible )
                {
                    rblCommunicationPreference.SetValue( person.CommunicationPreference == CommunicationType.SMS ? "2" : "1" );
                }
            }
            else
            {
                if ( person.GradeOffset.HasValue )
                {
                    gpGradePicker.SetValue( person.GradeOffset );
                }
                else
                {
                    gpGradePicker.SelectedValue = null;
                }
                pnlEmail.Visible = GetAttributeValue( AttributeKey.ChildAllowEmailEdit ).AsBoolean();
                rblCommunicationPreference.Visible = false;
            }

            BindPhoneNumbers( isAdult, person );
            var attributeList = GetPersonAttributeGuids( isAdult );
            avcPersonAttributes.IncludedAttributes = attributeList.ToArray();
            avcPersonAttributes.NumberOfColumns = 2;
            avcPersonAttributes.ShowCategoryLabel = false;
            avcPersonAttributes.AddEditControls( person );
            lPersonAttributeTitle.Text = string.Format( "{0} Person Attributes", isAdult ? "Adult" : "Child" );
            pnlPersonAttributes.Visible = attributeList.Count > 0;

            string smsScript = @"
    $('.js-sms-number').click(function () {
        if ($(this).is(':checked')) {
            $('.js-sms-number').not($(this)).prop('checked', false);
        }
    });
";
            ScriptManager.RegisterStartupScript( rContactInfo, rContactInfo.GetType(), "sms-number-" + BlockId.ToString(), smsScript, true );
        }

        /// <summary>
        /// Gets the person attribute Guids.
        /// </summary>
        /// <returns></returns>
        private List<AttributeCache> GetPersonAttributeGuids( bool isAdult )
        {
            List<Guid> attributeGuidList = new List<Guid>();

            if ( isAdult )
            {
                attributeGuidList = GetAttributeValue( AttributeKey.AdultPersonAttributes ).SplitDelimitedValues().AsGuidList();
            }
            else
            {
                attributeGuidList = GetAttributeValue( AttributeKey.ChildPersonAttributes ).SplitDelimitedValues().AsGuidList();
            }

            return attributeGuidList.Select( a => AttributeCache.Get( a ) ).ToList();
        }

        #endregion Edit Member Methods

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        ///
        /// </summary>
        [Serializable]
        public class AttendanceSetting
        {
            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets or sets the location identifier.
            /// </summary>
            /// <value>
            /// The location identifier.
            /// </value>
            public int LocationId { get; set; }

            /// <summary>
            /// Gets or sets the schedule identifier.
            /// </summary>
            /// <value>
            /// The schedule identifier.
            /// </value>
            public int ScheduleId { get; set; }

            /// <summary>
            /// Gets or sets the attendance date.
            /// </summary>
            /// <value>
            /// The attendance date.
            /// </value>
            public DateTime AttendanceDate { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class SearchResult
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the person guid identifier.
            /// </summary>
            /// <value>
            /// The person guid identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the age.
            /// </summary>
            /// <value>
            /// The age.
            /// </value>
            public int? Age { get; set; }

            /// <summary>
            /// Gets or sets the family group identifier.
            /// </summary>
            /// <value>
            /// The family group identifier.
            /// </value>
            public int FamilyId { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the campus name.
            /// </summary>
            /// <value>
            /// The campus name.
            /// </value>
            public string CampusName { get; set; }

            /// <summary>
            /// Gets or sets the family member names.
            /// </summary>
            /// <value>
            /// The family member names.
            /// </value>
            public string FamilyMemberNames { get { return FamilyMembers.Count > 1 ? FamilyMembers.Select( a => a.NickName ).ToList().AsDelimited( ", ", " & " ) : ""; } }

            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            /// <value>
            /// The address.
            /// </value>
            public Location Address { get; set; }

            /// <summary>
            /// Gets or sets the mobile.
            /// </summary>
            /// <value>
            /// The mobile.
            /// </value>
            public string Mobile { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [active].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [active]; otherwise, <c>false</c>.
            /// </value>
            public bool IsActive { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [attended].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [attended]; otherwise, <c>false</c>.
            /// </value>
            public bool IsAttended { get; set; }

            /// <summary>
            /// Gets or sets the family members.
            /// </summary>
            /// <value>
            /// The family members.
            /// </value>
            public List<Person> FamilyMembers { get; set; }

            /// <summary>
            /// Gets or sets the family name.
            /// </summary>
            /// <value>
            /// The family name.
            /// </value>
            public string FamilyName { get; set; }
        }

        # endregion Supporting Classes
    }
}
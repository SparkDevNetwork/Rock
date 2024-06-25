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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using static Rock.Blocks.Types.Mobile.Prayer.PrayerRequestDetails;

namespace RockWeb.Plugins.org_lakepointe.Groups
{
    [DisplayName( "Group Attendance Detail 2" )]
    [Category( "LPC > Groups" )]
    [Description( "Lists the group members for a specific occurrence datetime and allows selecting if they attended or not. Group Leader Toolbox V2." )]

    [BooleanField( "Allow Add", "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?", true, "", 0 )]
    [BooleanField( "Allow Adding Person", "Should block support adding new people as attendees?", false, "", 1 )]
    [CustomDropdownListField( "Add Person As", "'Attendee' will only add the person to attendance. 'Group Member' will add them to the group with the default group role.", "Attendee,Group Member", true, "Attendee", "", 2 )]
    [LinkedPage( "Group Member Add Page", "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.", false, "", "", 3 )]
    [BooleanField( "Allow Campus Filter", "Should block add an option to allow filtering people and attendance counts by campus?", false, "", 4 )]
    [WorkflowTypeField( "Workflow", "An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.", false, false, "", "", 5 )]
    [MergeTemplateField( "Attendance Roster Template", "", false, "", "", 6 )]
    [CodeEditorField( "Lava Template", "An optional lava template to appear next to each person in the list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, "", "", 7 )]
    [BooleanField( "Restrict Future Occurrence Date", "Should user be prevented from selecting a future Occurrence date?", false, "", 8 )]
    [BooleanField( "Show Notes", "Should the notes field be displayed?", true, "", 9 )]
    [TextField( "Attendance Note Label", "The text to use to describe the notes", true, "Notes", "", 10 )]
    [EnumsField( "Send Summary Email To", "", typeof( SendSummaryEmailType ), false, "", "", 11 )]
    [SystemCommunicationField( "Attendance Email", "The System Email to use to send the attendance", false, Rock.SystemGuid.SystemCommunication.ATTENDANCE_NOTIFICATION, "", 12, "AttendanceEmailTemplate" )]
    [BooleanField( "Allow Sorting", "Should the block allow sorting the Member's list by First Name or Last Name?", true, "", 13 )]
    [BooleanField("Display Headcount", "Should the headcount field be displayed on the block", true, "", 14, "DisplayHeadcount")]
    [WorkflowTypeField( AttributeKeys.NotesWorkflow,
        Description = "An optional workflow to start when notes are included in an attendance report. The Attendance Occurrence will be set as the workflow 'Entity' attribute when processing is started.", 
        AllowMultiple = false,
        IsRequired = false,
        Category = AttributeCategories.OnSaveBehavior,
        Order = 15 )]

    public partial class GroupAttendanceDetail2 : RockBlock
    {
        #region Block Settings

        /// <summary>
        /// The block setting attribute keys for the PrayerRequestDetails block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The workflow
            /// </summary>
            public const string NotesWorkflow = "NotesWorkflow";
        }

        /// <summary>
        /// Gets the workflow.
        /// </summary>
        /// <value>
        /// The workflow.
        /// </value>
        protected Guid? NotesWorkflow => GetAttributeValue( AttributeKeys.NotesWorkflow ).AsGuidOrNull();

        #endregion
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        private enum SendSummaryEmailType
        {
            /// <summary>
            /// Group Leaders
            /// </summary>
            GroupLeaders = 1,

            /// <summary>
            /// All Group Members (note: all active group members)
            /// </summary>
            AllGroupMembers = 2,

            /// <summary>
            /// Parent Group Leaders
            /// </summary>
            ParentGroupLeaders = 3,

            /// <summary>
            /// Individual Entering Attendance
            /// </summary>
            IndividualEnteringAttendance = 4,

            /// <summary>
            /// Group Administrator
            /// </summary>
            GroupAdministrator = 5,

            /// <summary>
            /// Group Pastor
            /// </summary>
            GroupPastor = 6
        }

        protected FeatureSet _featureSet;

        #endregion

        #region Properties
        private bool AllowMemberEdit
        {
            get
            {
                if ( _group == null )
                {
                    return false;
                }
                if ( _group.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson )
                    || _group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    return true;
                }
                else
                {
                    return _featureSet.EditGroupMembers;
                }
            }
        }
        #endregion

        #region Private Variables
        private RockContext _rockContext = null;
        private Group _group = null;
        private bool _canManageMembers = false;
        private bool _allowAdd = false;
        private bool _allowCampusFilter = false;
        private AttendanceOccurrence _occurrence = null;
        private List<GroupAttendanceAttendee> _attendees;
        //private int? _anonymousCount = new int?( 0 );
		private const string TOGGLE_SETTING = "Attendance_List_Sorting_Toggle";
		
        #endregion

        #region Control Methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _attendees = ViewState["Attendees"] as List<GroupAttendanceAttendee>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RegisterScript();

            _rockContext = new RockContext();

            int groupId = PageParameter( "GroupId" ).AsInteger();
            _group = new GroupService( _rockContext )
                .Queryable( "GroupType,Schedule" ).AsNoTracking()
                .FirstOrDefault( g => g.Id == groupId );

            _featureSet = new FeatureSet( GetFeatureSetStrings() );

            if ( AllowMemberEdit )
            {
                lHeading.Text = _group.Name + " Attendance";
                _canManageMembers = true;
            }

            dpOccurrenceDate.AllowFutureDateSelection = !GetAttributeValue( "RestrictFutureOccurrenceDate" ).AsBoolean();
            _allowAdd = GetAttributeValue( "AllowAdd" ).AsBoolean();

            _allowCampusFilter = GetAttributeValue( "AllowCampusFilter" ).AsBoolean();
            bddlCampus.Visible = _allowCampusFilter;
            if ( _allowCampusFilter )
            {
                bddlCampus.DataSource = CampusCache.All();
                bddlCampus.DataBind();
                bddlCampus.Items.Insert( 0, new ListItem( "All Campuses", "0" ) );
            }

            dtNotes.Label = GetAttributeValue( "AttendanceNoteLabel" );
            dtNotes.Visible = GetAttributeValue( "ShowNotes" ).AsBooleanOrNull() ?? true;
			tglSort.Visible = GetAttributeValue( "AllowSorting" ).AsBooleanOrNull() ?? true;

            pnlHeadcount.Visible = GetAttributeValue("DisplayHeadcount").AsBooleanOrNull() ?? true;
            //_allowAnonymousAttendance = GetAttributeValue( "AllowAnonymousAttendance" ).AsBooleanOrNull() ?? true;
            //nudAnonymous.Visible = _allowAnonymousAttendance;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                tglSort.Checked = GetUserPreference( TOGGLE_SETTING ).AsBoolean( true );
            }

            if ( !_canManageMembers )
            {
                nbNotice.Heading = "Sorry";
                nbNotice.Text = "<p>You're not authorized to update the attendance for the selected group.</p>";
                nbNotice.NotificationBoxType = NotificationBoxType.Danger;
                nbNotice.Visible = true;
                pnlDetails.Visible = false;
            }
            else
            {
                _occurrence = GetOccurrence();
                if ( !Page.IsPostBack )
                {
                    if ( _allowCampusFilter )
                    {
                        var campus = CampusCache.Get( GetBlockUserPreference( "Campus" ).AsInteger() );
                        if ( campus != null )
                        {
                            bddlCampus.Title = campus.Name;
                            bddlCampus.SetValue( campus.Id );
                        }
                    }

                    BindLocations();
                    ShowDetails();
                }
                else
                {
                    if ( _attendees != null )
                    {
                        foreach ( RepeaterItem item in rptMembers.Items )
                        {
                            var hfMember = item.FindControl( "hfMember" ) as HiddenField;
                            var cbMember = item.FindControl( "cbMember" ) as CheckBox;

                            if ( hfMember != null && cbMember != null )
                            {
                                int personId = hfMember.ValueAsInt();

                                var attendance = _attendees.Where( a => a.PersonId == personId ).FirstOrDefault();
                                if ( attendance != null )
                                {
                                    attendance.Attended = cbMember.Checked;
                                }
                            }
                        }
                    }
                }
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
            ViewState["Attendees"] = _attendees;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( _group != null && _occurrence != null )
            {
                if ( SaveAttendance() )
                {
                    EmailAttendanceSummary();
                    CreateFollowupTask();

                    var qryParams = new Dictionary<string, string> { { "GroupId", _group.Id.ToString() } };

                    var groupTypeIds = PageParameter( "GroupTypeIds" );
                    if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
                    {
                        qryParams.Add( "GroupTypeIds", groupTypeIds );
                    }

                    NavigateToParentPage( qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( _group != null )
            {
                var qryParams = new Dictionary<string, string> { { "GroupId", _group.Id.ToString() } };

                var groupTypeIds = PageParameter( "GroupTypeIds" );
                if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
                {
                    qryParams.Add( "GroupTypeIds", groupTypeIds );
                }

                NavigateToParentPage( qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbPrintAttendanceRoster control.
        /// NOTE: lbPrintAttendanceRoster is a full postback since we are returning a download of the roster
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrintAttendanceRoster_Click( object sender, EventArgs e )
        {
            nbPrintRosterWarning.Visible = false;
            var rockContext = new RockContext();

            Dictionary<int, object> mergeObjectsDictionary = new Dictionary<int, object>();
            if ( _attendees != null )
            {
                var personIdList = _attendees.Select( a => a.PersonId ).ToList();
                var personList = new PersonService( rockContext ).GetByIds( personIdList );
                foreach ( var person in personList.OrderBy( a => a.LastName ).ThenBy( a => a.NickName ) )
                {
                    mergeObjectsDictionary.AddOrIgnore( person.Id, person );
                }
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Group", this._group );
            mergeFields.Add( "AttendanceDate", this._occurrence.OccurrenceDate );

            var mergeTemplate = new MergeTemplateService( rockContext ).Get( this.GetAttributeValue( "AttendanceRosterTemplate" ).AsGuid() );

            if ( mergeTemplate == null )
            {
                this.LogException( new Exception( "Error printing Attendance Roster: No merge template selected. Please configure an 'Attendance Roster Template' in the block settings." ) );
                nbPrintRosterWarning.Visible = true;
                nbPrintRosterWarning.Text = "Unable to print Attendance Roster: No merge template selected. Please configure an 'Attendance Roster Template' in the block settings.";
                return;
            }

            MergeTemplateType mergeTemplateType = mergeTemplate.GetMergeTemplateType();
            if ( mergeTemplateType == null )
            {
                this.LogException( new Exception( "Error printing Attendance Roster: Unable to determine Merge Template Type from the 'Attendance Roster Template' in the block settings." ) );
                nbPrintRosterWarning.Visible = true;
                nbPrintRosterWarning.Text = "Error printing Attendance Roster: Unable to determine Merge Template Type from the 'Attendance Roster Template' in the block settings.";
                return;
            }

            BinaryFile outputBinaryFileDoc = null;

            var mergeObjectList = mergeObjectsDictionary.Select( a => a.Value ).ToList();

            outputBinaryFileDoc = mergeTemplateType.CreateDocument( mergeTemplate, mergeObjectList, mergeFields );

            // Set the name of the output doc
            outputBinaryFileDoc = new BinaryFileService( rockContext ).Get( outputBinaryFileDoc.Id );
            outputBinaryFileDoc.FileName = _group.Name + " Attendance Roster" + Path.GetExtension( outputBinaryFileDoc.FileName ?? string.Empty ) ?? ".docx";
            rockContext.SaveChanges();

            if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
            {
                if ( mergeTemplateType.Exceptions.Count == 1 )
                {
                    this.LogException( mergeTemplateType.Exceptions[0] );
                }
                else if ( mergeTemplateType.Exceptions.Count > 50 )
                {
                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions for top 50.", mergeTemplate.Name ), mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
                }
                else
                {
                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions", mergeTemplate.Name ), mergeTemplateType.Exceptions.ToList() ) );
                }
            }

            var uri = new UriBuilder( outputBinaryFileDoc.Url );
            var qry = System.Web.HttpUtility.ParseQueryString( uri.Query );
            qry["attachment"] = true.ToTrueFalse();
            uri.Query = qry.ToString();
            Response.Redirect( uri.ToString(), false );
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindSchedules( ddlLocation.SelectedValueAsInt() );
        }

        /// <summary>
        /// Handles the SelectionChanged event of the bddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlCampus_SelectionChanged( object sender, EventArgs e )
        {
            SetBlockUserPreference( "Campus", bddlCampus.SelectedValue );
            var campus = CampusCache.Get( bddlCampus.SelectedValueAsInt() ?? 0 );
            bddlCampus.Title = campus != null ? campus.Name : "All Campuses";
            BindAttendees();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {
            string template = GetAttributeValue( "LavaTemplate" );

            if ( ppAddPerson.PersonId.HasValue )
            {
                if ( !_attendees.Any( a => a.PersonId == ppAddPerson.PersonId.Value ) )
                {
                    var rockContext = new RockContext();
                    var person = new PersonService( rockContext ).Get( ppAddPerson.PersonId.Value );
                    if ( person != null )
                    {
                        string addPersonAs = GetAttributeValue( "AddPersonAs" );
                        if ( !addPersonAs.IsNullOrWhiteSpace() && addPersonAs == "Group Member" )
                        {
                            AddPersonAsGroupMember( person, rockContext );
                        }

                        var attendee = new GroupAttendanceAttendee();
                        attendee.PersonId = person.Id;
                        attendee.NickName = person.NickName;
                        attendee.LastName = person.LastName;
                        attendee.Attended = true;
                        attendee.CampusIds = person.GetCampusIds();

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Person", person );
                        mergeFields.Add( "Attended", true );
                        attendee.MergedTemplate = template.ResolveMergeFields( mergeFields );
                        _attendees.Add( attendee );
                        BindAttendees();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvPendingMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvPendingMembers_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            if ( _group != null && e.CommandName == "Add" )
            {
                if ( SaveAttendance() ) // so we don't lose anything they've already checked
                {
                    int personId = e.CommandArgument.ToString().AsInteger();

                    using ( var rockContext = new RockContext() )
                    {
                        var groupMemberService = new GroupMemberService( rockContext );

                        foreach ( var groupMember in groupMemberService.GetByGroupIdAndPersonId( _group.Id, personId ) )
                        {
                            if ( groupMember.GroupMemberStatus == GroupMemberStatus.Pending )
                            {
                                if ( groupMember.GroupRole.Name == "Prospect" ) // convert pending prospects to active guests
                                {
                                    var guestRole = groupMember.Group.GroupType.Roles.Where( r => r.Name == "Guest" ).FirstOrDefault();
                                    if ( guestRole != null )
                                    {
                                        // check to see if they're already in a Guest role
                                        var isGuest = groupMemberService.Queryable()
                                            .Where( m => m.GroupId == _group.Id && m.PersonId == personId && m.GroupRoleId == guestRole.Id )
                                            .Any();
                                        if ( isGuest )
                                        {
                                            // they're already a guest. inactivate this prospect role and let further iterations of this loop
                                            // activate the guest role.
                                            groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                                        }
                                        else
                                        {
                                            // they're not a guest, so convert this prospect record to guest
                                            groupMember.GroupRoleId = guestRole.Id;
                                            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                        }
                                    }
                                    else  // defensive programming. We shouldn't hit this as it would mean no Guest role is defined.
                                    {
                                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                    }
                                }
                                else // Primary path ... roles other than Prospect 
                                {
                                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                }
                            }
                        }

                        rockContext.SaveChanges();
                    }

                    ShowDetails();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddMember_Click( object sender, EventArgs e )
        {
            var personAddPage = GetAttributeValue( "GroupMemberAddPage" );

            if ( !personAddPage.IsNullOrWhiteSpace() )
            {
                // Redirect to the add page provided
                if ( _group != null && _occurrence != null )
                {
                    if ( SaveAttendance() )
                    {
                        var queryParams = new Dictionary<string, string>();
                        queryParams.Add( "GroupId", _group.Id.ToString() );
                        queryParams.Add( "GroupName", _group.Name );
                        queryParams.Add( "ReturnUrl", Request.QueryString["returnUrl"] ?? Server.UrlEncode( Request.RawUrl ) );
                        NavigateToLinkedPage( "GroupMemberAddPage", queryParams );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the DataBinding event of the cbMember control.
        /// Set the Full Name Display of the cbMember check box
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbMember_DataBinding( object sender, EventArgs e )
        {
            var checkBox = sender as RockCheckBox;
            var parent = checkBox.Parent as ListViewDataItem;
            var data = parent.DataItem as GroupAttendanceAttendee;
            string displayName = string.Empty;

            if ( data != null )
            {
               
                if ( tglSort.Visible && tglSort.Checked )
                {
                    displayName = data.LastName + ", " + data.NickName;
                }
                else
                {
                    displayName = data.NickName + " " + data.LastName;
                }
                if (displayName.IsNullOrWhiteSpace() || displayName.Trim().Equals(","))
                {
                    displayName = string.Concat("Nameless Person", data.MobileNumber.IsNotNullOrWhiteSpace()  ? string.Format(" - {0}", data.MobileNumber) : String.Empty); 
                }

                checkBox.Text = string.Format( "{0} {1}", data.MergedTemplate, displayName );
            }
        }

        protected void rptMembers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Header || e.Item.ItemType == ListItemType.Footer)
            {
                return;
            }

            var attendeeItem = e.Item.DataItem as GroupAttendanceAttendee;

            var hfMember = e.Item.FindControl("hfMember") as HiddenField;
            var lMemberName = e.Item.FindControl("lMemberName") as Literal;
            var cbMember = e.Item.FindControl("cbMember") as CheckBox;
            var lMemberImage = e.Item.FindControl("lMemberImage") as Literal;


            hfMember.Value = attendeeItem.PersonId.ToString();
            lMemberName.Text = attendeeItem.FullName;
            cbMember.Checked = attendeeItem.Attended;

            var person = new PersonService(_rockContext).Get(attendeeItem.PersonId);
            lMemberImage.Text = Person.GetPersonPhotoImageTag(person, altText: attendeeItem.FullName, className: "img-circle member-attendance-photo");
            

        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglSort UI control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglSort_CheckedChanged( object sender, EventArgs e )
        {
            SetUserPreference( TOGGLE_SETTING, tglSort.Checked.ToString() );
            BindAttendees();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Adds the person as group member.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddPersonAsGroupMember( Person person, RockContext rockContext )
        {
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            GroupTypeRole role = new GroupTypeRoleService( rockContext ).Get( _group.GroupType.DefaultGroupRoleId ?? 0 );

            var groupMember = new GroupMember { Id = 0 };
            groupMember.GroupId = _group.Id;

            // check to see if the person is already a member of the group/role
            var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( _group.Id, person.Id, _group.GroupType.DefaultGroupRoleId ?? 0 );

            if ( existingGroupMember != null )
            {
                return;
            }

            groupMember.PersonId = person.Id;
            groupMember.GroupRoleId = role.Id;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            if ( groupMember.Id.Equals( 0 ) )
            {
                groupMemberService.Add( groupMember );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the occurrence items.
        /// </summary>
        private AttendanceOccurrence GetOccurrence()
        {
            AttendanceOccurrence occurrence = null;

            var occurrenceService = new AttendanceOccurrenceService( _rockContext );

            // Check to see if a occurrence id was specified on the query string, and if so, query for it
            int? occurrenceId = PageParameter( "OccurrenceId" ).AsIntegerOrNull();
            if ( occurrenceId.HasValue && occurrenceId.Value > 0 )
            {
                occurrence = occurrenceService.Get( occurrenceId.Value );

                // If we have a valid occurrence return it now (the date,location,schedule cannot be changed for an existing occurrence)
                if ( occurrence != null )
                    return occurrence;
            }

            // Set occurrence values from query string
            var occurrenceDate = PageParameter( "Date" ).AsDateTime() ?? PageParameter( "Occurrence" ).AsDateTime();
            var locationId = PageParameter( "LocationId" ).AsIntegerOrNull();
            var scheduleId = PageParameter( "ScheduleId" ).AsIntegerOrNull();

            if ( scheduleId == null )
            {
                // if no specific schedule was specified in the URL, use the group's scheduleId 
                scheduleId = _group.ScheduleId;
            }

            // If this is a postback, check to see if date/location/schedule were updated
            if ( Page.IsPostBack && _allowAdd )
            {
                if ( dpOccurrenceDate.Visible && dpOccurrenceDate.SelectedDate.HasValue )
                {
                    occurrenceDate = dpOccurrenceDate.SelectedDate.Value;
                }

                if ( ddlLocation.Visible && ddlLocation.SelectedValueAsInt().HasValue )
                {
                    locationId = ddlLocation.SelectedValueAsInt().Value;
                }

                if ( ddlSchedule.Visible && ddlSchedule.SelectedValueAsInt().HasValue )
                {
                    scheduleId = ddlSchedule.SelectedValueAsInt().Value;
                }
            }

            if ( occurrence == null && occurrenceDate.HasValue )
            {
                // if no specific occurrenceId was specified, try to find a matching occurrence from Date, GroupId, Location, ScheduleId
                occurrence = occurrenceService.Get( occurrenceDate.Value.Date, _group.Id, locationId, scheduleId );
            }

            // If an occurrence date was included, but no occurrence was found with that date, and new 
            // occurrences can be added, create a new one
            if ( occurrence == null && _allowAdd )
            {
                // Create a new occurrence record and return it
                return new AttendanceOccurrence
                {
                    Group = _group,
                    GroupId = _group.Id,
                    OccurrenceDate = occurrenceDate ?? RockDateTime.Today,
                    LocationId = locationId,
                    ScheduleId = scheduleId,
                };
            }

            return occurrence;

        }

        /// <summary>
        /// Binds the locations.
        /// </summary>
        private void BindLocations()
        {
            var locations = new Dictionary<int, string> { { 0, string.Empty } };

            if ( _group != null )
            {
                var locationPaths = new Dictionary<int, string>();
                var locationService = new LocationService( _rockContext );

                foreach ( var location in _group.GroupLocations
                    .Where( l =>
                        l.Location.Name != null &&
                        l.Location.Name != string.Empty )
                    .Select( l => l.Location ) )
                {
                    // Get location path
                    string parentLocationPath = string.Empty;
                    if ( location.ParentLocationId.HasValue )
                    {
                        var locId = location.ParentLocationId.Value;
                        if ( !locationPaths.ContainsKey( locId ) )
                        {
                            locationPaths.Add( locId, locationService.GetPath( locId ) );
                        }
                        parentLocationPath = locationPaths[locId];
                    }

                    if ( !locations.ContainsKey( location.Id ) )
                    {
                        locations.Add( location.Id, new List<string> { parentLocationPath, location.Name }.AsDelimited( " > " ) );
                    }
                }
            }

            if ( locations.Any() )
            {
                ddlLocation.DataSource = locations;
                ddlLocation.DataBind();
            }
        }

        /// <summary>
        /// Binds the schedules.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        private void BindSchedules( int? locationId )
        {
            var schedules = new Dictionary<int, string> { { 0, string.Empty } };

            if ( _group != null && locationId.HasValue )
            {
                _group.GroupLocations
                    .Where( l => l.LocationId == locationId.Value )
                    .SelectMany( l => l.Schedules )
                    .OrderBy( s => s.Name )
                    .ToList()
                    .ForEach( s => schedules.AddOrIgnore( s.Id, s.Name ) );
            }

            if ( schedules.Any() )
            {
                ddlSchedule.DataSource = schedules;
                ddlSchedule.DataBind();
            }

            ddlSchedule.Visible = ddlSchedule.Items.Count > 1;
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void ShowDetails()
        {
            if ( _occurrence == null )
            {
                nbNotice.Heading = "No Occurrences";
                nbNotice.Text = "<p>There are currently not any active occurrences for selected group to take attendance for.</p>";
                nbNotice.NotificationBoxType = NotificationBoxType.Warning;
                nbNotice.Visible = true;

                pnlDetails.Visible = false;
            }
            else
            {
                nbNotice.Visible = false;

                if ( PageParameter( "OccurrenceId" ).AsIntegerOrNull().HasValue )
                {
                    lOccurrenceDate.Visible = true;
                    lOccurrenceDate.Text = _occurrence.OccurrenceDate.ToShortDateString();
                    dpOccurrenceDate.Visible = false;

                    if ( _occurrence.LocationId.HasValue )
                    {
                        lLocation.Visible = true;
                        lLocation.Text = new LocationService( _rockContext ).GetPath( _occurrence.LocationId.Value );
                    }
                    else
                    {
                        lLocation.Visible = false;
                    }
                    ddlLocation.Visible = false;

                    lSchedule.Visible = _occurrence.Schedule != null && _occurrence.Schedule.Name.IsNotNullOrWhiteSpace();
                    lSchedule.Text = _occurrence.Schedule != null ? _occurrence.Schedule.Name : string.Empty;
                    ddlSchedule.Visible = false;
                }
                else
                {
                    lOccurrenceDate.Visible = false;
                    dpOccurrenceDate.Visible = true;
                    dpOccurrenceDate.SelectedDate = _occurrence.OccurrenceDate;

                    int? locationId = PageParameter( "LocationId" ).AsIntegerOrNull();
                    if ( locationId.HasValue )
                    {
                        lLocation.Visible = true;
                        lLocation.Text = new LocationService( _rockContext ).GetPath( locationId.Value );
                        ddlLocation.Visible = false;

                        Schedule schedule = null;
                        int? scheduleId = PageParameter( "ScheduleId" ).AsIntegerOrNull();
                        if ( scheduleId.HasValue )
                        {
                            schedule = new ScheduleService( _rockContext ).Get( scheduleId.Value );
                        }

                        if ( schedule != null )
                        {
                            lSchedule.Visible = true;
                            lSchedule.Text = schedule.Name;
                            ddlSchedule.Visible = false;
                        }
                        else
                        {
                            BindSchedules( locationId.Value );
                            lSchedule.Visible = false;
                            ddlSchedule.Visible = ddlSchedule.Items.Count > 1;
                        }
                    }
                    else
                    {
                        lLocation.Visible = false;
                        ddlLocation.Visible = ddlLocation.Items.Count > 1;

                        lSchedule.Visible = false;
                        ddlSchedule.Visible = ddlSchedule.Items.Count > 1;
                    }
                }

                lMembers.Text = _group.GroupType.GroupMemberTerm.Pluralize();
                lPendingMembers.Text = "Pending " + lMembers.Text;

                List<int> attendedIds = new List<int>();
                int? anonymousCount = null;
                // Load the attendance for the selected occurrence
                if ( _occurrence.Id > 0 )
                {
                    dtNotes.Text = _occurrence.Notes;

                    cbDidNotMeet.Checked = _occurrence.DidNotOccur ?? false;

                    // Get the list of people who attended
                    attendedIds = new AttendanceService( _rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.OccurrenceId == _occurrence.Id &&
                            a.DidAttend.HasValue &&
                            a.DidAttend.Value &&
                            a.PersonAlias != null )
                        .Select( a => a.PersonAlias.PersonId )
                        .Distinct()
                        .ToList();

                    anonymousCount = new AttendanceOccurrenceService(_rockContext).Queryable().AsNoTracking()
                        .Where(o => o.Id == _occurrence.Id)
                        .Select(o => o.AnonymousAttendanceCount)
                        .FirstOrDefault();
                }

                var allowAddPerson = GetAttributeValue( "AllowAddingPerson" ).AsBoolean();
                var addPersonAs = GetAttributeValue( "AddPersonAs" );
                ppAddPerson.PersonName = string.Format( "Add New {0}", addPersonAs );
                if ( !GetAttributeValue( "GroupMemberAddPage" ).IsNullOrWhiteSpace() )
                {
                    lbAddMember.Visible = allowAddPerson;
                    ppAddPerson.Visible = allowAddPerson && addPersonAs == "Attendee";
                }
                else
                {
                    ppAddPerson.Visible = allowAddPerson;
                }

                // Get the group members
                var groupMemberService = new GroupMemberService( _rockContext );

                // Add any existing active members not on that list
                var unattendedIds = groupMemberService
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.GroupId == _group.Id &&
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        !attendedIds.Contains( m.PersonId ) )
                    .Select( m => m.PersonId )
                    .ToList();

                string template = GetAttributeValue( "LavaTemplate" );
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

                var mobilePhoneDefinedValue = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), _rockContext);
                // Bind the attendance roster
                _attendees = new PersonService( _rockContext )
                    .Queryable(new Rock.Model.PersonService.PersonQueryOptions() { IncludeNameless = true }).Include("PhoneNumbers").AsNoTracking()
                    .Where( p => attendedIds.Contains( p.Id ) || unattendedIds.Contains( p.Id ) )
                    .ToList()
                    .Select( p => new GroupAttendanceAttendee()
                    {
                        PersonId = p.Id,
                        NickName = p.NickName,
                        LastName = p.LastName,
                        Attended = attendedIds.Contains( p.Id ),
                        CampusIds = p.GetCampusIds(),
                        MobileNumber = p.PhoneNumbers.Where(pn => pn.NumberTypeValueId == mobilePhoneDefinedValue.Id).Select(pn => pn.NumberFormatted).FirstOrDefault(),
                        MergedTemplate = template.ResolveMergeFields( mergeFields.Union( new Dictionary<string, object>() { { "Person", p } } ).ToDictionary( x => x.Key, x => x.Value ) )
                    } )
                    .ToList();

                BindAttendees();

                // Bind the pending members
                var pendingMembers = groupMemberService
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.GroupId == _group.Id &&
                        m.GroupMemberStatus == GroupMemberStatus.Pending )
                    .OrderBy( m => m.Person.LastName )
                    .ThenBy( m => m.Person.NickName )
                    .Select( m => new
                    {
                        Id = m.PersonId,
                        FullName = m.Person.NickName + " " + m.Person.LastName
                    } )
                    .ToList();

                pnlPendingMembers.Visible = pendingMembers.Any();
                lvPendingMembers.DataSource = pendingMembers;
                lvPendingMembers.DataBind();

                // Update the headcount
                //nudAnonymous.Value = _anonymousCount ?? 0;

                if (anonymousCount.HasValue)
                {
                    tbHeadCount.IntegerValue = anonymousCount;
                }
            }
        }

        /// <summary>
        /// Binds the attendees to the list.
        /// </summary>
        private void BindAttendees()
        {
            var campusAttendees = _attendees;
            if ( _allowCampusFilter )
            {
                var campus = CampusCache.Get( bddlCampus.SelectedValueAsInt() ?? 0 );
                if ( campus != null )
                {
                    campusAttendees = _attendees.Where( a => a.CampusIds.Contains( campus.Id ) ).ToList();
                }
            }

            int attendanceCount = campusAttendees.Where(a => a.Attended).Count();// + (_anonymousCount ?? 0);
            lDidAttendCount.Visible = attendanceCount > 0;
            lDidAttendCount.Text = attendanceCount.ToString( "N0" );

            if ( tglSort.Visible && tglSort.Checked )
            {
                rptMembers.DataSource = campusAttendees.OrderBy( a => a.LastName ).ThenBy( a => a.NickName ).ToList();
            }
            else
            {
                rptMembers.DataSource = campusAttendees.OrderBy( a => a.NickName ).ThenBy( a => a.LastName ).ToList();
            }

            rptMembers.DataBind();

            ppAddPerson.PersonId = Rock.Constants.None.Id;
            ppAddPerson.PersonName = string.Format( "Add New {0}", GetAttributeValue( "AddPersonAs" ) );
        }

        protected void RegisterScript()
        {
            string script = string.Format( @"

    Sys.Application.add_load(function () {{

        if ($('#{0}').is(':checked')) {{
            $('div.js-roster').hide();
        }}

        $('#{0}').click(function () {{
            if ($(this).is(':checked')) {{
                $('div.js-roster').hide('fast');
            }} else {{
                $('div.js-roster').show('fast');
            }}
        }});

        $('.js-add-member').click(function ( e ) {{
            e.preventDefault();
            var $a = $(this);
            var memberName = $(this).parent().find('span').html();
            Rock.dialogs.confirm('Add ' + memberName + ' to your group?', function (result) {{
                if (result) {{
                    window.location = $a.prop('href');                    
                }}
            }});
        }});

    }});

", cbDidNotMeet.ClientID );

            ScriptManager.RegisterStartupScript( cbDidNotMeet, cbDidNotMeet.GetType(), "group-attendance-detail", script, true );
        }

        /// <summary>
        /// Method to save attendance for use in two separate areas.
        /// </summary>
        protected bool SaveAttendance()
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var locationService = new LocationService( rockContext );

                AttendanceOccurrence occurrence = null;

                if ( _occurrence.Id != 0 )
                {
                    occurrence = occurrenceService.Get( _occurrence.Id );
                }

                if ( occurrence == null )
                {
                    var existingOccurrence = occurrenceService.Get( _occurrence.OccurrenceDate, _group.Id, _occurrence.LocationId, _occurrence.ScheduleId );
                    if ( existingOccurrence != null )
                    {
                        nbNotice.Heading = "Occurrence Already Exists";
                        nbNotice.Text = "<p>An occurrence already exists for this group for the selected date, location, and schedule that you've selected. Please return to the list and select that occurrence to update it's attendance.</p>";
                        nbNotice.NotificationBoxType = NotificationBoxType.Danger;
                        nbNotice.Visible = true;

                        return false;
                    }
                    else
                    {
                        occurrence = new AttendanceOccurrence();
                        occurrence.GroupId = _occurrence.GroupId;
                        occurrence.LocationId = _occurrence.LocationId;
                        occurrence.ScheduleId = _occurrence.ScheduleId;
                        occurrence.OccurrenceDate = _occurrence.OccurrenceDate;
                        occurrenceService.Add( occurrence );
                    }
                }

                occurrence.Notes = GetAttributeValue( "ShowNotes" ).AsBoolean() ? dtNotes.Text : string.Empty;
                occurrence.DidNotOccur = cbDidNotMeet.Checked;

                var existingAttendees = occurrence.Attendees.ToList();

                // If did not meet was selected and this was a manually entered occurrence (not based on a schedule/location)
                // then just delete all the attendance records instead of tracking a 'did not meet' value
                if ( cbDidNotMeet.Checked && !_occurrence.ScheduleId.HasValue )
                {
                    foreach ( var attendance in existingAttendees )
                    {
                        attendanceService.Delete( attendance );
                    }
                }
                else
                {
                    int? campusId = locationService.GetCampusIdForLocation( _occurrence.LocationId ) ?? _group.CampusId;
                    if ( !campusId.HasValue && _allowCampusFilter )
                    {
                        var campus = CampusCache.Get( bddlCampus.SelectedValueAsInt() ?? 0 );
                        if ( campus != null )
                        {
                            campusId = campus.Id;
                        }
                    }

                    if ( cbDidNotMeet.Checked )
                    {
                        // If the occurrence is based on a schedule, set the did not meet flags
                        foreach ( var attendance in existingAttendees )
                        {
                            attendance.DidAttend = null;
                        }
                    }
                    else
                    {
                        _occurrence.Schedule = _occurrence.Schedule == null && _occurrence.ScheduleId.HasValue ? new ScheduleService( rockContext ).Get( _occurrence.ScheduleId.Value ) : _occurrence.Schedule;

                        cvAttendance.IsValid = _occurrence.IsValid;
                        if ( !cvAttendance.IsValid )
                        {
                            cvAttendance.ErrorMessage = _occurrence.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                            return false;
                        }

                        foreach ( var attendee in _attendees )
                        {
                            var attendance = existingAttendees
                                .Where( a => a.PersonAlias.PersonId == attendee.PersonId )
                                .FirstOrDefault();

                            if ( attendance == null )
                            {
                                int? personAliasId = personAliasService.GetPrimaryAliasId( attendee.PersonId );
                                if ( personAliasId.HasValue )
                                {
                                    attendance = new Attendance();
                                    attendance.PersonAliasId = personAliasId;
                                    attendance.CampusId = campusId;
                                    attendance.StartDateTime = _occurrence.Schedule != null && _occurrence.Schedule.HasSchedule() ? _occurrence.OccurrenceDate.Date.Add( _occurrence.Schedule.StartTimeOfDay ) : _occurrence.OccurrenceDate;
                                    attendance.DidAttend = attendee.Attended;

                                    // Check that the attendance record is valid
                                    cvAttendance.IsValid = attendance.IsValid;
                                    if ( !cvAttendance.IsValid )
                                    {
                                        cvAttendance.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                                        return false;
                                    }

                                    occurrence.Attendees.Add( attendance );
                                }
                            }
                            else
                            {
                                // Otherwise, only record that they attended -- don't change their attendance startDateTime 
                                attendance.DidAttend = attendee.Attended;
                            }
                        }
                        if (GetAttributeValue("DisplayHeadcount").AsBooleanOrNull() ?? true)
                        {
                            occurrence.AnonymousAttendanceCount = tbHeadCount.IntegerValue;
                        }
                    }
                }

                rockContext.SaveChanges();                

                if ( occurrence.LocationId.HasValue )
                {
                    Rock.CheckIn.KioskLocationAttendance.Remove( occurrence.LocationId.Value );
                }


                Guid? workflowTypeGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
                if ( workflowTypeGuid.HasValue )
                {
                    var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        try
                        {
                            var workflow = Workflow.Activate( workflowType, _group.Name );

                            workflow.SetAttributeValue( "StartDateTime", _occurrence.OccurrenceDate.ToString( "o" ) );
                            workflow.SetAttributeValue( "Schedule", _group.Schedule.Guid.ToString() );

                            List<string> workflowErrors;
                            new WorkflowService( rockContext ).Process( workflow, _group, out workflowErrors );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex, this.Context );
                        }
                    }
                }

                _occurrence.Id = occurrence.Id;
            }

            return true;
        }

        /// <summary>
        /// Method to create followup task for staff
        /// </summary>
        private void CreateFollowupTask()
        {
            // SNS 20240522
            // This may seem counter-intuitive but if a GroupAdministrator is defined, this is probably a student group and we don't want to try to find a pastor in a great-grandparent group
            // (because it probably doesn't exist).
            // We include this exclusion here so the same block with the same configuration can be used on a single group leader toolbox page to address both adult groups and student groups.
            if ( _group.GroupType.ShowAdministrator && _group.GroupAdministratorPersonAliasId.HasValue && _group.GroupAdministratorPersonAlias.Person.Email.IsNotNullOrWhiteSpace() )
            {
                return;
            }

            if ( _occurrence.Notes.IsNullOrWhiteSpace() )
            {
                return;
            }

            if ( !NotesWorkflow.HasValue )
            {
                return;
            }

            try
            {
                var workflowType = WorkflowTypeCache.Get( NotesWorkflow.Value );
                if ( workflowType == null || !( workflowType.IsActive ?? true ) )
                {
                    return;
                }

                var notificationOptions = GetAttributeValue( "SendSummaryEmailTo" ).SplitDelimitedValues().Select( a => a.ConvertToEnumOrNull<SendSummaryEmailType>() ).ToList();
                foreach ( var notificationOption in notificationOptions )
                {
                    if ( !notificationOption.HasValue )
                    {
                        continue;
                    }

                    switch ( notificationOption )
                    {
                        case SendSummaryEmailType.GroupPastor:
                            try
                            {
                                using ( var rockContext = new RockContext() )
                                {
                                    // Create workflow for followup and assign to group pastor
                                    var occurrence = new AttendanceOccurrenceService( rockContext ).Get( _occurrence.Id );
                                    var workflow = Workflow.Activate( workflowType, _group.Name );
                                    new WorkflowService( rockContext ).Process( workflow, _occurrence, out var workflowErrors );
                                }
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( ex, Context );
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
            }
        }

        /// <summary>
        /// Method to email attendance summary.
        /// </summary>
        private void EmailAttendanceSummary()
        {
            try
            {
                var rockContext = new RockContext();
                var occurrence = new AttendanceOccurrenceService( rockContext ).Get( _occurrence.Id );
                var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeObjects.Add( "Group", _group );
                mergeObjects.Add( "AttendanceOccurrence", occurrence );
                mergeObjects.Add( "AttendanceNoteLabel", GetAttributeValue( "AttendanceNoteLabel" ) );

                List<Person> recipients = new List<Person>();

                var notificationOptions = GetAttributeValue( "SendSummaryEmailTo" ).SplitDelimitedValues().Select( a => a.ConvertToEnumOrNull<SendSummaryEmailType>() ).ToList();
                foreach ( var notificationOption in notificationOptions )
                {
                    if ( !notificationOption.HasValue )
                    {
                        continue;
                    }

                    switch ( notificationOption )
                    {
                        case SendSummaryEmailType.GroupLeaders:
                            var leaders = new GroupMemberService( _rockContext )
                                .Queryable( "Person" )
                                .AsNoTracking()
                                .Where( m => m.GroupId == _group.Id )
                                .Where( m => m.IsArchived == false )
                                .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive )
                                .Where( m => m.GroupRole.IsLeader );

                            recipients.AddRange( leaders.Where( a => !string.IsNullOrEmpty( a.Person.Email ) ).Select( a => a.Person ) );
                            break;
                        case SendSummaryEmailType.AllGroupMembers:
                            var allGroupMembers = new GroupMemberService( _rockContext )
                                .Queryable( "Person" )
                                .AsNoTracking()
                                .Where( m => m.GroupId == _group.Id )
                                .Where( m => m.IsArchived == false )
                                .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive );

                            recipients.AddRange( allGroupMembers.Where( a => !string.IsNullOrEmpty( a.Person.Email ) ).Select( a => a.Person ) );
                            break;

                        case SendSummaryEmailType.GroupAdministrator:
                            if ( _group.GroupType.ShowAdministrator && _group.GroupAdministratorPersonAliasId.HasValue && _group.GroupAdministratorPersonAlias.Person.Email.IsNotNullOrWhiteSpace() )
                            {
                                recipients.Add( _group.GroupAdministratorPersonAlias.Person );
                            }
                            break;
                        case SendSummaryEmailType.ParentGroupLeaders:
                                if ( _group.ParentGroupId.HasValue )
                                {
                                    var parentLeaders = new GroupMemberService( _rockContext )
                                    .Queryable( "Person" )
                                    .AsNoTracking()
                                    .Where( m => m.GroupId == _group.ParentGroupId.Value )
                                    .Where( m => m.IsArchived == false )
                                    .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive )
                                    .Where( m => m.GroupRole.IsLeader );

                                    recipients.AddRange( parentLeaders.Where( a => !string.IsNullOrEmpty( a.Person.Email ) ).Select( a => a.Person ) );
                                }

                            break;
                        case SendSummaryEmailType.IndividualEnteringAttendance:
                            if ( !string.IsNullOrEmpty( this.CurrentPerson.Email ) )
                            {
                                recipients.Add( this.CurrentPerson );
                            }
                            break;
                        case SendSummaryEmailType.GroupPastor:
                            // Don't send the email to Group Pastors. They'll get a task assigned instead and that will send its own email.
                            break;
                        default:
                            break;
                    }
                }

                foreach ( var recipient in recipients )
                {
                    var emailMessage = new RockEmailMessage( GetAttributeValue( "AttendanceEmailTemplate" ).AsGuid() );
                    emailMessage.AddRecipient( new RockEmailMessageRecipient( recipient, mergeObjects ) );
                    emailMessage.CreateCommunicationRecord = false;
                    emailMessage.Send();
                }
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
            }
        }

        private string GetFeatureSetStrings()
        {
            List<string> result = new List<string>();

            var context = new RockContext();
            var groupMemberService = new GroupMemberService( context );

            // get features that might be enabled because the current person is a member of this group
            foreach ( var gm in groupMemberService.Queryable().AsNoTracking().Where( gm => gm.GroupId == _group.Id && gm.PersonId == CurrentPerson.Id ) )
            {
                var role = gm.GroupRole;
                role.LoadAttributes();
                result.Add( role.AttributeValues["GroupLeaderToolboxFeatureSet"]?.Value ?? string.Empty );
            }

            // get features that might be enabled because the current person is a member of a coach group
            foreach ( var gm in groupMemberService.Queryable().AsNoTracking().Where( gm => gm.GroupId == _group.ParentGroupId && gm.PersonId == CurrentPerson.Id ) )
            {
                var role = gm.GroupRole;
                role.LoadAttributes();
                result.Add( role.AttributeValues["GroupLeaderToolboxFeatureSet"]?.Value ?? string.Empty );
            }

            // get features that might be enabled because the current person is a member of a captain group
            foreach ( var gm in groupMemberService.Queryable().AsNoTracking().Where( gm => gm.GroupId == _group.ParentGroup.ParentGroupId && gm.PersonId == CurrentPerson.Id ) )
            {
                var role = gm.GroupRole;
                role.LoadAttributes();
                result.Add( role.AttributeValues["GroupLeaderToolboxFeatureSet"]?.Value ?? string.Empty );
            }

            return result.JoinStrings( "," );
        }

        #endregion

        #region Helper Classes

        [Serializable]
        public class GroupAttendanceAttendee
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the name of the nick.
            /// </summary>
            /// <value>
            /// The name of the nick.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName
            {
                get { return NickName + " " + LastName; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="GroupAttendanceAttendee"/> is attended.
            /// </summary>
            /// <value>
            ///   <c>true</c> if attended; otherwise, <c>false</c>.
            /// </value>
            public bool Attended { get; set; }

            /// <summary>
            /// Gets or sets the campus ids that a person's families belong to.
            /// </summary>
            /// <value>
            /// The campus ids.
            /// </value>
            public List<int> CampusIds { get; set; }

            public string MobileNumber { get; set; }

            public string MergedTemplate { get; set; }
        }

        protected class FeatureSet
        {
            public bool AddGuestButton;
            public bool SchedulerButton;
            public bool Headcount;
            public bool RequestUpdates;
            public bool DisplayParentInfo;
            public bool ForwardAttendanceNotes;
            public bool AddSendEmailToCoachButton;
            public bool GroupLeaderToolbox;
            public bool Roster;
            public bool Attendance;
            public bool LeaderNewsPortal;
            public bool Curriculum;
            public bool EditGroupMembers;
            public bool GroupCommunication;

            public FeatureSet( string features )
            {
                AddGuestButton = features.Contains( "6abaaa35-3e8d-4386-a755-4fb8c31beef8" );
                SchedulerButton = features.Contains( "2b7997c0-e98b-40eb-9f6c-d9375bac79f1" );
                Headcount = features.Contains( "a70d029b-90c0-4e00-bcd3-ea3dec2dca7b" );
                RequestUpdates = features.Contains( "8b91f1cd-dcf6-4d04-aeb7-cbcff5b48134" );
                DisplayParentInfo = features.Contains( "bb45f157-ce0e-48e1-8946-21eb3c9c7a85" );
                ForwardAttendanceNotes = features.Contains( "31dcc393-d2e2-48b0-8fd8-481bcdfe6230" );
                AddSendEmailToCoachButton = features.Contains( "59608021-6cef-4934-9547-d7c71c2a0666" );
                GroupLeaderToolbox = features.Contains( "bf778fb4-6613-4565-8a45-371cb9a7c172" );
                Roster = features.Contains( "5c3e2545-ee21-43b4-a7c9-f3cc4eca018b" );
                Attendance = features.Contains( "30ced7e4-03ca-47d0-8aa7-98f4c64e0c2d" );
                LeaderNewsPortal = features.Contains( "e93c8e1d-afe2-486c-bd10-74dfe47593f5" );
                Curriculum = features.Contains( "63e6ea73-7e54-4bf6-a060-100f028b19a1" );
                EditGroupMembers = features.Contains( "1f54b1cc-b94c-429b-b927-1e542d8c7fe3" );
                GroupCommunication = features.Contains( "4f092398-a348-4974-bc70-517da060b7cd" );
            }
        }

        #endregion
    }
}
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
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Member List" )]
    [Category( "Groups" )]
    [Description( "Lists all the members of the given group." )]

    [GroupField( "Group", "Either pick a specific group or choose <none> to have group be determined by the groupId page parameter", false )]
    [LinkedPage( "Detail Page" )]
    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, "", "", 2, "PersonProfilePage" )]
    [LinkedPage( "Registration Page", "Page used for viewing the registration(s) associated with a particular group member", false, "", "", 3 )]
    [LinkedPage( "Data View Detail Page", "Page used to view data views that are used with the group member sync.", false, order: 3 )]
    [BooleanField( "Show Campus Filter", "Setting to show/hide campus filter.", true, order: 4 )]
    [BooleanField( "Show First/Last Attendance", "If the group allows attendance, should the first and last attendance date be displayed for each group member?", false, "", 5, SHOW_FIRST_LAST_ATTENDANCE_KEY )]
    [BooleanField( "Show Date Added", "Should the date that person was added to the group be displayed for each group member?", false, "", 6, SHOW_DATE_ADDED_KEY )]
    [BooleanField( "Show Note Column", "Should the note be displayed as a separate grid column (instead of displaying a note icon under person's name)?", false, "", 7 )]
    public partial class GroupMemberList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        private const string SHOW_FIRST_LAST_ATTENDANCE_KEY = "ShowAttendance";
        private const string SHOW_DATE_ADDED_KEY = "ShowDateAdded";
        private const string DATE_ADDED_FILTER_KEY = "Date Added";

        #region Private Variables

        private DefinedValueCache _inactiveStatus = null;
        private Group _group = null;
        private GroupTypeCache _groupTypeCache = null;
        private bool _canView = false;
        private Dictionary<int, List<GroupMemberRegistrationItem>> _groupMembersWithRegistrations = new Dictionary<int, List<GroupMemberRegistrationItem>>();
        private int? _homePhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
        private int? _cellPhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

        private class GroupMemberRegistrationItem
        {
            public int RegistrationId { get; set; }
            public string RegistrationName { get; set; }
        }

        private HashSet<int> _groupMembersWithGroupMemberHistory = null;

        // cache the DeleteField and ColumnIndex since it could get called many times in GridRowDataBound
        private DeleteField _deleteField = null;
        private int? _deleteFieldColumnIndex = null;

        // cache these fields since they could get called many times in GridRowDataBound
        private RockLiteralField _fullNameField = null;
        private RockLiteralField _nameWithHtmlField = null;
        private RockLiteralField _connectionStatusField = null;
        private RockLiteralField _maritalStatusField = null;
        private RockLiteralField _registrationField = null;
        private RockLiteralField _firstAttendedField = null;
        private RockLiteralField _lastAttendedField = null;
        private RockLiteralField _exportHomePhoneField = null;
        private RockLiteralField _exportCellPhoneField = null;
        private RockLiteralField _exportHomeAddressField = null;
        private RockLiteralField _exportLatitudeField = null;
        private RockLiteralField _exportLongitude = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( ViewState["AvailableAttributeIds"] != null )
            {
                AvailableAttributes = ( ViewState["AvailableAttributeIds"] as int[] ).Select( a => AttributeCache.Get( a ) ).ToList();
            }

            AddDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += GroupMemberList_BlockUpdated;

            // show hide campus filter
            cpCampusFilter.Visible = GetAttributeValue( "ShowCampusFilter" ).AsBoolean();

            /// add lazyload js so that person-link-popover javascript works (see GroupMemberList.ascx)
            RockPage.AddScriptLink( "~/Scripts/jquery.lazyload.min.js" );

            // if this block has a specific GroupId set, use that, otherwise, determine it from the PageParameters
            Guid groupGuid = GetAttributeValue( "Group" ).AsGuid();
            int groupId = 0;

            if ( groupGuid == Guid.Empty )
            {
                groupId = PageParameter( "GroupId" ).AsInteger();
            }

            if ( !( groupId == 0 && groupGuid == Guid.Empty ) )
            {
                string key = string.Format( "Group:{0}", groupId );
                _group = RockPage.GetSharedItem( key ) as Group;
                if ( _group == null )
                {
                    _group = new GroupService( new RockContext() ).Queryable( "GroupType.Roles" )
                        .Where( g => g.Id == groupId || g.Guid == groupGuid )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, _group );
                }

                if ( _group != null )
                {
                    _groupTypeCache = GroupTypeCache.Get( _group.GroupTypeId );
                }

                if ( _group != null && _group.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    _canView = true;

                    rFilter.UserPreferenceKeyPrefix = string.Format( "{0}-", groupId );
                    rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                    gGroupMembers.DataKeyNames = new string[] { "Id" };
                    gGroupMembers.PersonIdField = "PersonId";
                    gGroupMembers.GetRecipientMergeFields += gGroupMembers_GetRecipientMergeFields;
                    gGroupMembers.Actions.AddClick += gGroupMembers_AddClick;
                    gGroupMembers.GridRebind += gGroupMembers_GridRebind;
                    gGroupMembers.RowItemText = _groupTypeCache.GroupTerm + " " + _groupTypeCache.GroupMemberTerm;
                    gGroupMembers.ExportFilename = _group.Name;
                    gGroupMembers.ExportSource = ExcelExportSource.DataSource;

                    // we'll have custom javascript (see GroupMemberList.ascx ) do this instead
                    gGroupMembers.ShowConfirmDeleteDialog = false;

                    // make sure they have Auth to edit the block OR edit to the Group
                    bool canEditBlock = IsUserAuthorized( Authorization.EDIT ) || _group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) || _group.IsAuthorized( Authorization.MANAGE_MEMBERS, this.CurrentPerson );
                    gGroupMembers.Actions.ShowAdd = canEditBlock;
                    gGroupMembers.IsDeleteEnabled = canEditBlock;

                    // If all of the roles in a group are sync'd then don't show the add button
                    gGroupMembers.Actions.ShowAdd = _groupTypeCache.Roles
                        .Where( r => !_group.GroupSyncs.Select( s => s.GroupTypeRoleId )
                        .Contains( r.Id ) )
                        .Any();
                }
            }

            // Show the sync icon if group member sync is set up for this group.
            if ( _group != null && _group.GroupSyncs != null && _group.GroupSyncs.Count() > 0 )
            {
                string syncedRolesHtml = string.Empty;
                var dataViewDetailPage = GetAttributeValue( "DataViewDetailPage" );

                if ( !string.IsNullOrWhiteSpace( dataViewDetailPage ) )
                {
                    syncedRolesHtml = string.Join( "<br>", _group.GroupSyncs.Select( s => string.Format( "<small><a href='{0}'>{1}</a> as {2}</small>", LinkedPageUrl( "DataViewDetailPage", new Dictionary<string, string>() { { "DataViewId", s.SyncDataViewId.ToString() } } ), s.SyncDataView.Name, s.GroupTypeRole.Name ) ).ToArray() );
                }
                else
                {
                    syncedRolesHtml = string.Join( "<br>", _group.GroupSyncs.Select( s => string.Format( "<small><i class='text-info'>{0}</i> as {1}</small>", s.SyncDataView.Name, s.GroupTypeRole.Name ) ).ToArray() );
                }

                spanSyncLink.Attributes.Add( "data-content", syncedRolesHtml );
                spanSyncLink.Visible = true;
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the GroupMemberList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void GroupMemberList_BlockUpdated( object sender, EventArgs e )
        {
            SetFilter();
            BindGroupMembersGrid();
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
                pnlContent.Visible = _canView;
                if ( _canView )
                {
                    SetFilter();
                    BindGroupMembersGrid();
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
            ViewState["AvailableAttributeIds"] = AvailableAttributes == null? null :  AvailableAttributes.Select( a => a.Id ).ToArray();
            return base.SaveViewState();
        }

        #endregion

        #region GroupMembers Grid

        private readonly string _photoFormat = "<div class=\"photo-icon photo-round photo-round-xs pull-left margin-r-sm js-person-popover\" personid=\"{0}\" data-original=\"{1}&w=50\" style=\"background-image: url( '{2}' ); background-size: cover; background-repeat: no-repeat;\"></div>";

        private bool _isExporting = false;
        private bool _showAttendance = false;
        private bool _hasGroupRequirements = false;
        private HashSet<int> _groupMemberIdsThatLackGroupRequirements = new HashSet<int>();
        private bool _showDateAdded = false;
        private bool _showNoteColumn = false;

        private bool _showPersonsThatHaventSigned = false;
        private HashSet<int> _personIdsThatHaveSigned = new HashSet<int>();
        private Dictionary<int, Location> _personIdHomeLocationLookup = null;

        // dictionary of PhoneNumbers' FormattedNumber by PersonId and NumberTypeValueId
        private Dictionary<int, Dictionary<int, string>> _personIdPhoneNumberTypePhoneNumberLookup = null;

        private Dictionary<int, DateRange> _personIdAttendanceFirstLastLookup = null;

        // cache a hash of GroupTypeRoleIds that have GroupSync enabled for the group type (GridRowDataBound uses this)
        private HashSet<int> _groupTypeRoleIdsWithGroupSync = null;

        /// <summary>
        /// Handles the RowCreated event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gGroupMembers_RowCreated( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            GroupMember groupMember = e.Row.DataItem as GroupMember;
            if ( groupMember != null )
            {
                // We already have the Group fetched, so set the Group here so it doesn't have to be loaded from the database for each row (this helps when loading attributes)
                groupMember.Group = _group;
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gGroupMembers_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            GroupMember groupMember = e.Row.DataItem as GroupMember;
            if ( groupMember == null )
            {
                return;
            }

            int groupMemberId = groupMember.Id;

            if ( _groupMembersWithRegistrations.ContainsKey( groupMemberId ) )
            {
                e.Row.AddCssClass( "js-has-registration" );

                var lRegistration = e.Row.FindControl( _registrationField.ID ) as Literal;
                if ( lRegistration != null )
                {
                    var regLinks = new List<string>();

                    foreach ( var reg in _groupMembersWithRegistrations[groupMemberId] )
                    {
                        regLinks.Add(
                            string.Format(
                                "<a href='{0}'>{1}</a>",
                                LinkedPageUrl( "RegistrationPage", new Dictionary<string, string> { { "RegistrationId", reg.RegistrationId.ToString() } } ),
                                reg.RegistrationName ) );
                    }

                    lRegistration.Text = regLinks.AsDelimited( "<br/>" );
                }
            }

            var lFullName = e.Row.FindControl( _fullNameField.ID ) as Literal;
            if ( lFullName != null )
            {
                lFullName.Text = groupMember.Person.FullNameReversed;
            }

            var lMaritalStatusValue = e.Row.FindControl( _maritalStatusField.ID ) as Literal;
            if ( lMaritalStatusValue != null )
            {
                lMaritalStatusValue.Text = DefinedValueCache.GetValue( groupMember.Person.MaritalStatusValueId );
            }

            var lConnectionStatusValue = e.Row.FindControl( _connectionStatusField.ID ) as Literal;
            if ( lConnectionStatusValue != null )
            {
                lConnectionStatusValue.Text = DefinedValueCache.GetValue( groupMember.Person.ConnectionStatusValueId );
            }

            if ( _isExporting )
            {
                var personPhoneNumbers = _personIdPhoneNumberTypePhoneNumberLookup.GetValueOrNull( groupMember.PersonId );

                if ( personPhoneNumbers != null )
                {
                    var lExportHomePhone = e.Row.FindControl( _exportHomePhoneField.ID ) as Literal;
                    var lExportCellPhone = e.Row.FindControl( _exportCellPhoneField.ID ) as Literal;

                    if ( _homePhoneTypeId.HasValue )
                    {
                        lExportHomePhone.Text = personPhoneNumbers.GetValueOrNull( _homePhoneTypeId.Value );
                    }

                    if ( _cellPhoneTypeId.HasValue )
                    {
                        lExportCellPhone.Text = personPhoneNumbers.GetValueOrNull( _cellPhoneTypeId.Value );
                    }
                }

                var homeLocation = _personIdHomeLocationLookup.GetValueOrNull( groupMember.PersonId );
                if ( homeLocation != null )
                {
                    var lExportHomeAddress = e.Row.FindControl( _exportHomeAddressField.ID ) as Literal;
                    var lExportLatitude = e.Row.FindControl( _exportLatitudeField.ID ) as Literal;
                    var lExportLongitude = e.Row.FindControl( _exportLongitude.ID ) as Literal;

                    lExportHomeAddress.Text = homeLocation.FormattedAddress;
                    lExportLatitude.Text = homeLocation.Latitude.ToString();
                    lExportLongitude.Text = homeLocation.Longitude.ToString();
                }
            }

            var lNameWithHtml = e.Row.FindControl( _nameWithHtmlField.ID ) as Literal;
            if ( lNameWithHtml != null )
            {
                StringBuilder sbNameHtml = new StringBuilder();
                sbNameHtml.AppendFormat( _photoFormat, groupMember.PersonId, groupMember.Person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) );
                sbNameHtml.Append( groupMember.Person.FullName );
                if ( groupMember.Person.TopSignalColor.IsNotNullOrWhiteSpace() )
                {
                    sbNameHtml.Append( groupMember.Person.GetSignalMarkup() );
                }

                if ( _hasGroupRequirements )
                {
                    if ( _groupMemberIdsThatLackGroupRequirements.Contains( groupMember.Id ) )
                    {
                        sbNameHtml.Append( " <i class='fa fa-exclamation-triangle text-warning'></i>" );
                    }
                }

                if ( _showNoteColumn && groupMember.Note.IsNotNullOrWhiteSpace() )
                {
                    sbNameHtml.Append( " <span class='js-group-member-note' data-toggle='tooltip' data-placement='top' title='" + groupMember.Note.EncodeHtml() + "'><i class='fa fa-file-text-o text-info'></i></span>" );
                }

                // If there is a required signed document that member has not signed, show an icon in the grid
                if ( _showPersonsThatHaventSigned && !_personIdsThatHaveSigned.Contains( groupMember.PersonId ) )
                {
                    sbNameHtml.Append( " <i class='fa fa-pencil-square-o text-danger'></i>" );
                }

                lNameWithHtml.Text = sbNameHtml.ToString();
            }

            if ( groupMember.Person.IsDeceased )
            {
                e.Row.AddCssClass( "is-deceased" );
            }

            if ( _deleteField != null && _deleteField.Visible )
            {
                LinkButton deleteButton = null;
                HtmlGenericControl buttonIcon = null;

                if ( !_deleteFieldColumnIndex.HasValue )
                {
                    _deleteFieldColumnIndex = gGroupMembers.GetColumnIndex( gGroupMembers.Columns.OfType<DeleteField>().First() );
                }

                if ( _deleteFieldColumnIndex.HasValue && _deleteFieldColumnIndex > -1 )
                {
                    deleteButton = e.Row.Cells[_deleteFieldColumnIndex.Value].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                }

                if ( deleteButton != null )
                {
                    buttonIcon = deleteButton.ControlsOfTypeRecursive<HtmlGenericControl>().FirstOrDefault();
                }

                if ( buttonIcon != null )
                {
                    if ( _groupTypeRoleIdsWithGroupSync.Contains( groupMember.GroupRoleId ) )
                    {
                        deleteButton.Enabled = false;
                        buttonIcon.Attributes["class"] = "fa fa-exchange";
                        var groupTypeRole = _groupTypeCache.Roles.FirstOrDefault( a => a.Id == groupMember.GroupRoleId );
                        deleteButton.ToolTip = string.Format( "Managed by group sync for role \"{0}\".", groupTypeRole );
                    }
                    else if ( _groupTypeCache.EnableGroupHistory == true && _groupMembersWithGroupMemberHistory.Contains( groupMember.Id ) )
                    {
                        buttonIcon.Attributes["class"] = "fa fa-archive";
                        deleteButton.AddCssClass( "btn-danger" );
                        deleteButton.ToolTip = "Archive";
                        e.Row.AddCssClass( "js-has-grouphistory" );
                    }
                }
            }

            if ( _inactiveStatus != null && groupMember.Person.RecordStatusValueId == _inactiveStatus.Id )
            {
                e.Row.AddCssClass( "is-inactive-person" );
            }

            if ( _inactiveStatus != null && groupMember.GroupMemberStatus == GroupMemberStatus.Inactive )
            {
                e.Row.AddCssClass( "is-inactive" );
            }

            if ( _showAttendance )
            {
                var lFirstAttended = e.Row.FindControl( _firstAttendedField.ID ) as Literal;
                var lLastAttended = e.Row.FindControl( _lastAttendedField.ID ) as Literal;

                var attendanceFirstLastRecord = _personIdAttendanceFirstLastLookup.GetValueOrNull( groupMember.PersonId );
                if ( attendanceFirstLastRecord != null )
                {
                    lFirstAttended.Text = attendanceFirstLastRecord.Start.ToString();
                    lLastAttended.Text = attendanceFirstLastRecord.End.ToString();
                }
            }
        }

        /// <summary>
        /// Handles the GetRecipientMergeFields event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GetRecipientMergeFieldsEventArgs"/> instance containing the event data.</param>
        protected void gGroupMembers_GetRecipientMergeFields( object sender, GetRecipientMergeFieldsEventArgs e )
        {
            GroupMember groupMemberRow = e.DataItem as GroupMember;

            if ( groupMemberRow == null )
            {
                return;
            }

            var groupMember = new GroupMemberService( new RockContext() ).Get( groupMemberRow.Id );
            groupMember.LoadAttributes();

            var mergefields = e.MergeValues;
            e.MergeValues.Add( "GroupRole", groupMemberRow.GroupRole );
            e.MergeValues.Add( "GroupMemberStatus", groupMemberRow.GroupMemberStatus.ConvertToString() );
            e.MergeValues.Add( "GroupName", groupMember.Group.Name );

            dynamic dynamicAttributeCarrier = new RockDynamic();
            foreach ( var attributeKeyValue in groupMember.AttributeValues )
            {
                dynamicAttributeCarrier[attributeKeyValue.Key] = attributeKeyValue.Value.Value;
            }

            e.MergeValues.Add( "GroupMemberAttributes", dynamicAttributeCarrier );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "First Name", "First Name", tbFirstName.Text );
            rFilter.SaveUserPreference( "Last Name", "Last Name", tbLastName.Text );
            rFilter.SaveUserPreference( "Role", "Role", cblRole.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( "Status", "Status", cblGroupMemberStatus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( "Campus", "Campus", cpCampusFilter.SelectedCampusId.ToString() );
            rFilter.SaveUserPreference( "Gender", "Gender", cblGenderFilter.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( "Registration", "Registration", ddlRegistration.SelectedValue );
            rFilter.SaveUserPreference( "Signed Document", "Signed Document", ddlSignedDocument.SelectedValue );
            rFilter.SaveUserPreference( DATE_ADDED_FILTER_KEY, DATE_ADDED_FILTER_KEY, drpDateAdded.DelimitedValues );

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            rFilter.SaveUserPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch
                        {
                            // intentionally ignore
                        }
                    }
                    else
                    {
                        // no filter control, so clear out the user preference
                        rFilter.SaveUserPreference( attribute.Key, attribute.Name, null );
                    }
                }
            }

            BindGroupMembersGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => a.Key == e.Key );
                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch
                    {
                        // intentionally ignore
                    }
                }
            }

            if ( e.Key == "First Name" )
            {
                return;
            }
            else if ( e.Key == "Last Name" )
            {
                return;
            }
            else if ( e.Key == "Role" )
            {
                e.Value = ResolveValues( e.Value, cblRole );
            }
            else if ( e.Key == "Status" )
            {
                e.Value = ResolveValues( e.Value, cblGroupMemberStatus );
            }
            else if ( e.Key == "Gender" )
            {
                e.Value = ResolveValues( e.Value, cblGenderFilter );
            }
            else if ( e.Key == "Campus" )
            {
                var campusId = e.Value.AsIntegerOrNull();
                if ( campusId.HasValue )
                {
                    var campusCache = CampusCache.Get( campusId.Value );
                    if ( campusCache != null )
                    {
                        e.Value = campusCache.Name;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
            else if ( e.Key == "Registration" )
            {
                var instanceId = e.Value.AsIntegerOrNull();
                if ( instanceId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var instance = new RegistrationInstanceService( rockContext ).Get( instanceId.Value );
                        if ( instance != null )
                        {
                            e.Value = instance.ToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }
                    }
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
            else if ( e.Key == "Signed Document" )
            {
                return;
            }
            else if ( e.Key == DATE_ADDED_FILTER_KEY )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            SetFilter();
        }

        /// <summary>
        /// Handles the Click event of the delete/archive button in the grid
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteOrArchiveGroupMember_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            GroupMemberHistoricalService groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );
            GroupMember groupMember = groupMemberService.Get( e.RowKeyId );
            if ( groupMember != null )
            {
                bool archive = false;
                if ( _groupTypeCache.EnableGroupHistory == true && groupMemberHistoricalService.Queryable().Any( a => a.GroupMemberId == groupMember.Id ) )
                {
                    // if the group has GroupHistory enabled, and this group member has group member history snapshots, they were prompted to Archive
                    archive = true;
                }
                else
                {
                    string errorMessage;
                    if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }
                }

                int groupId = groupMember.GroupId;

                if ( archive )
                {
                    // NOTE: Delete will AutoArchive, but since we know that we need to archive, we can call .Archive directly
                    groupMemberService.Archive( groupMember, this.CurrentPersonAliasId, true );
                }
                else
                {
                    groupMemberService.Delete( groupMember, true );
                }

                rockContext.SaveChanges();
            }

            BindGroupMembersGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupMembers_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "GroupMemberId", 0, "GroupId", _group.Id );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupMembers_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "GroupMemberId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs" /> instance containing the event data.</param>
        protected void gGroupMembers_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGroupMembersGrid( e.IsExporting, e.IsCommunication );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            if ( _group != null )
            {
                cblRole.DataSource = _group.GroupType.Roles.OrderBy( a => a.Order ).ToList();
                cblRole.DataBind();

                using ( var rockContext = new RockContext() )
                {
                    ddlRegistration.DataSource = new RegistrationInstanceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( i => i.Linkages.Any( l => l.GroupId == _group.Id ) )
                        .OrderByDescending( i => i.StartDateTime )
                        .Select( i => new { i.Id, i.Name } )
                        .ToList();
                    ddlRegistration.DataBind();
                    ddlRegistration.Items.Insert( 0, new ListItem() );
                }
            }

            cblGroupMemberStatus.BindToEnum<GroupMemberStatus>();
            cpCampusFilter.Campuses = CampusCache.All();

            BindAttributes();
            AddDynamicControls();

            tbFirstName.Text = rFilter.GetUserPreference( "First Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Last Name" );
            cpCampusFilter.SelectedCampusId = rFilter.GetUserPreference( "Campus" ).AsIntegerOrNull();

            string genderValue = rFilter.GetUserPreference( "Gender" );
            if ( !string.IsNullOrWhiteSpace( genderValue ) )
            {
                cblGenderFilter.SetValues( genderValue.Split( ';' ).ToList() );
            }
            else
            {
                cblGenderFilter.ClearSelection();
            }

            string roleValue = rFilter.GetUserPreference( "Role" );
            if ( !string.IsNullOrWhiteSpace( roleValue ) )
            {
                cblRole.SetValues( roleValue.Split( ';' ).ToList() );
            }

            string statusValue = rFilter.GetUserPreference( "Status" );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cblGroupMemberStatus.SetValues( statusValue.Split( ';' ).ToList() );
            }

            ddlRegistration.SetValue( rFilter.GetUserPreference( "Registration" ) );
            ddlRegistration.Visible = ddlRegistration.Items.Count > 1;

            ddlSignedDocument.SetValue( rFilter.GetUserPreference( "Signed Document" ) );
            ddlSignedDocument.Visible = _group.RequiredSignatureDocumentTemplateId.HasValue;

            drpDateAdded.DelimitedValues = rFilter.GetUserPreference( DATE_ADDED_FILTER_KEY );
            drpDateAdded.Visible = GetAttributeValue( SHOW_DATE_ADDED_KEY ).AsBoolean();
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();
            if ( _group != null )
            {
                var rockContext = new RockContext();
                int entityTypeId = new GroupMember().TypeId;
                string groupQualifier = _group.Id.ToString();
                string groupTypeQualifier = _group.GroupTypeId.ToString();
                foreach ( var attribute in new AttributeService( rockContext ).GetByEntityTypeQualifier( entityTypeId, "GroupId", groupQualifier, true )
                    .Where( a => a.IsGridColumn )
                    .OrderByDescending( a => a.EntityTypeQualifierColumn )
                    .ThenBy( a => a.Order )
                    .ThenBy( a => a.Name ).ToAttributeCacheList() )
                {
                    if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        AvailableAttributes.Add( attribute );
                    }
                }

                foreach ( var inheritedGridColumnAttribute in ( new GroupMember() { GroupId = _group.Id } ).GetInheritedAttributes( rockContext ).Where( a => a.IsGridColumn == true && a.IsActive == true ).ToList() )
                {
                    if ( inheritedGridColumnAttribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        AvailableAttributes.Add( inheritedGridColumnAttribute );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Clear dynamic controls so we can re-add them
            RemoveAttributeAndButtonColumns();

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = ( IRockControl ) control;
                            rockControl.Label = attribute.Name;
                            rockControl.Help = attribute.Description;
                            phAttributeFilters.Controls.Add( control );
                        }
                        else
                        {
                            var wrapper = new RockControlWrapper();
                            wrapper.ID = control.ID + "_wrapper";
                            wrapper.Label = attribute.Name;
                            wrapper.Controls.Add( control );
                            phAttributeFilters.Controls.Add( wrapper );
                        }

                        string savedValue = rFilter.GetUserPreference( attribute.Key );
                        if ( !string.IsNullOrWhiteSpace( savedValue ) )
                        {
                            try
                            {
                                var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                            }
                            catch
                            {
                                // intentionally ignore
                            }
                        }
                    }

                    bool columnExists = gGroupMembers.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;
                        boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;

                        gGroupMembers.Columns.Add( boundField );
                    }
                }
            }

            AddRowButtonsToEnd();
        }

        private void RemoveAttributeAndButtonColumns()
        {
            // Remove added button columns
            DataControlField buttonColumn = gGroupMembers.Columns.OfType<DeleteField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gGroupMembers.Columns.Remove( buttonColumn );
            }

            buttonColumn = gGroupMembers.Columns.OfType<HyperLinkField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gGroupMembers.Columns.Remove( buttonColumn );
            }

            buttonColumn = gGroupMembers.Columns.OfType<LinkButtonField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gGroupMembers.Columns.Remove( buttonColumn );
            }

            // Remove attribute columns
            foreach ( var column in gGroupMembers.Columns.OfType<AttributeField>().ToList() )
            {
                gGroupMembers.Columns.Remove( column );
            }
        }

        private void AddRowButtonsToEnd()
        {
            // Add Place Elsewhere column if the group or group type has any Place Elsewhere member triggers
            if ( _group != null && _group.GroupType != null )
            {
                if ( _group.GetGroupMemberWorkflowTriggers().Where( a => a.TriggerType == GroupMemberWorkflowTriggerType.MemberPlacedElsewhere ).Any() )
                {
                    AddPlaceElsewhereColumn();
                }
            }

            // Add Link to Profile Page Column
            if ( !string.IsNullOrEmpty( GetAttributeValue( "PersonProfilePage" ) ) )
            {
                AddPersonProfileLinkColumn();
            }

            // Add delete column
            _deleteField = new DeleteField();
            _deleteField.Click += DeleteOrArchiveGroupMember_Click;
            gGroupMembers.Columns.Add( _deleteField );
        }

        /// <summary>
        /// Adds the column with a link to profile page.
        /// </summary>
        private void AddPersonProfileLinkColumn()
        {
            HyperLinkField hlPersonProfileLink = new HyperLinkField();
            hlPersonProfileLink.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            hlPersonProfileLink.HeaderStyle.CssClass = "grid-columncommand";
            hlPersonProfileLink.ItemStyle.CssClass = "grid-columncommand";
            hlPersonProfileLink.DataNavigateUrlFields = new string[1] { "PersonId" };
            hlPersonProfileLink.DataNavigateUrlFormatString = LinkedPageUrl( "PersonProfilePage", new Dictionary<string, string> { { "PersonId", "###" } } ).Replace( "###", "{0}" );
            hlPersonProfileLink.DataTextFormatString = "<div class='btn btn-default btn-sm'><i class='fa fa-user'></i></div>";
            hlPersonProfileLink.DataTextField = "PersonId";
            gGroupMembers.Columns.Add( hlPersonProfileLink );
        }

        /// <summary>
        /// Adds the Place Elsewhere column
        /// </summary>
        private void AddPlaceElsewhereColumn()
        {
            LinkButtonField btnPlaceElsewhere = new LinkButtonField();
            btnPlaceElsewhere.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            btnPlaceElsewhere.HeaderStyle.CssClass = "grid-columncommand";
            btnPlaceElsewhere.ItemStyle.CssClass = "grid-columncommand";
            btnPlaceElsewhere.Text = "<i class='fa fa-share'></i>";
            btnPlaceElsewhere.CssClass = "btn btn-default btn-sm";
            btnPlaceElsewhere.ToolTip = "Place Elsewhere";
            btnPlaceElsewhere.Click += btnPlaceElsewhere_Click;

            gGroupMembers.Columns.Add( btnPlaceElsewhere );
        }

        /// <summary>
        /// Handles the Click event of the btnPlaceElsewhere control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void btnPlaceElsewhere_Click( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();

            var groupMemberPerson = new GroupMemberService( rockContext ).GetPerson( e.RowKeyId );
            if ( groupMemberPerson != null )
            {
                hfPlaceElsewhereGroupMemberId.Value = e.RowKeyId.ToString();
                lPlaceElsewhereGroupMemberName.Text = groupMemberPerson.ToString();
                BindPlaceElsewhereTriggerButtons( true );

                mdPlaceElsewhere.Visible = true;
                mdPlaceElsewhere.Show();
            }
        }

        /// <summary>
        /// Binds the place elsewhere trigger buttons.
        /// </summary>
        /// <param name="setDefault">if set to <c>true</c> [set default].</param>
        private void BindPlaceElsewhereTriggerButtons( bool setDefault )
        {
            var sortedTriggerList = _group.GetGroupMemberWorkflowTriggers().Where( a => a.TriggerType == GroupMemberWorkflowTriggerType.MemberPlacedElsewhere ).ToList();

            if ( setDefault )
            {
                var defaultTrigger = sortedTriggerList.FirstOrDefault();
                hfPlaceElsewhereTriggerId.Value = defaultTrigger != null ? defaultTrigger.Id.ToString() : null;
            }

            // if only one trigger, just show the name of it (don't show the button list)
            if ( sortedTriggerList.Count == 1 )
            {
                rcwSelectMemberTrigger.Visible = false;

                lWorkflowTriggerName.Visible = true;
                lWorkflowTriggerName.Text = sortedTriggerList[0].Name;
            }
            else
            {
                lWorkflowTriggerName.Visible = false;

                rcwSelectMemberTrigger.Visible = true;
                rptSelectMemberTrigger.DataSource = sortedTriggerList.OrderBy( a => a.WorkflowName );
                rptSelectMemberTrigger.DataBind();
            }

            var selectedTrigger = sortedTriggerList.Where( a => a.Id == hfPlaceElsewhereTriggerId.Value.AsInteger() ).FirstOrDefault();
            if ( selectedTrigger != null )
            {
                var qualifierParts = ( selectedTrigger.TypeQualifier ?? string.Empty ).Split( new char[] { '|' } );
                bool showNote = qualifierParts.Length > 5 ? qualifierParts[5].AsBoolean() : false;
                bool requireNote = qualifierParts.Length > 6 ? qualifierParts[6].AsBoolean() : false;
                tbPlaceElsewhereNote.Visible = showNote || requireNote;
                tbPlaceElsewhereNote.Required = requireNote;
                lWorkflowName.Text = selectedTrigger.WorkflowType.ToString();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMemberTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMemberTrigger_Click( object sender, EventArgs e )
        {
            var btnMemberTrigger = sender as LinkButton;
            if ( btnMemberTrigger != null )
            {
                hfPlaceElsewhereTriggerId.Value = btnMemberTrigger.CommandArgument;
                BindPlaceElsewhereTriggerButtons( false );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptSelectMemberTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptSelectMemberTrigger_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var btnMemberTrigger = e.Item.FindControl( "btnMemberTrigger" ) as LinkButton;
            var trigger = e.Item.DataItem as GroupMemberWorkflowTrigger;
            if ( trigger != null && trigger.Id == hfPlaceElsewhereTriggerId.Value.AsInteger() )
            {
                btnMemberTrigger.AddCssClass( "active" );
            }
            else
            {
                btnMemberTrigger.RemoveCssClass( "active" );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdPlaceElsewhere control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdPlaceElsewhere_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var groupMember = groupMemberService.Get( hfPlaceElsewhereGroupMemberId.Value.AsInteger() );
                if ( groupMember != null )
                {
                    string errorMessage;
                    if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                    {
                        nbPlaceElsewhereWarning.Text = errorMessage;
                        return;
                    }

                    var trigger = _group.GetGroupMemberWorkflowTriggers().FirstOrDefault( a => a.Id == hfPlaceElsewhereTriggerId.Value.AsInteger() );
                    if ( trigger != null )
                    {
                        // create a transaction for the selected trigger
                        var transaction = new Rock.Transactions.GroupMemberPlacedElsewhereTransaction( groupMember, tbPlaceElsewhereNote.Text, trigger );

                        // Un-link any registrant records that point to this group member.
                        foreach ( var registrant in new RegistrationRegistrantService( rockContext ).Queryable()
                            .Where( r => r.GroupMemberId == groupMember.Id ) )
                        {
                            registrant.GroupMemberId = null;
                        }

                        // delete the group member from the current group
                        groupMemberService.Delete( groupMember );

                        rockContext.SaveChanges();

                        // queue up the transaction
                        Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                    }
                }

                mdPlaceElsewhere.Hide();
                mdPlaceElsewhere.Visible = false;
                BindGroupMembersGrid();
            }
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGroupMembersGrid( bool isExporting = false, bool isCommunication = false )
        {
            _isExporting = isExporting;
            if ( _group == null )
            {
                pnlGroupMembers.Visible = false;
                return;
            }

            if ( !_groupTypeCache.Roles.Any() )
            {
                nbRoleWarning.Text = string.Format(
                       "{0} cannot be added to this {1} because the '{2}' group type does not have any roles defined.",
                       _groupTypeCache.GroupMemberTerm.Pluralize(),
                       _groupTypeCache.GroupTerm,
                       _groupTypeCache.Name );

                nbRoleWarning.Visible = true;
                rFilter.Visible = false;
                gGroupMembers.Visible = false;
            }

            int groupId = _group.Id;
            pnlGroupMembers.Visible = true;
            nbRoleWarning.Visible = false;
            rFilter.Visible = true;
            gGroupMembers.Visible = true;

            lHeading.Text = string.Format( "{0} {1}", _groupTypeCache.GroupTerm, _groupTypeCache.GroupMemberTerm.Pluralize() );

            _fullNameField = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lExportFullName" ).FirstOrDefault();
            _nameWithHtmlField = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lNameWithHtml" ).FirstOrDefault();
            _registrationField = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lRegistration" ).FirstOrDefault();
            _firstAttendedField = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lFirstAttended" ).FirstOrDefault();
            _lastAttendedField = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lLastAttended" ).FirstOrDefault();
            _exportHomePhoneField = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lExportHomePhone" ).FirstOrDefault();
            _exportCellPhoneField = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lExportCellPhone" ).FirstOrDefault();
            _exportHomeAddressField = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lExportHomeAddress" ).FirstOrDefault();
            _exportLatitudeField = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lExportLatitude" ).FirstOrDefault();
            _exportLongitude = gGroupMembers.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lExportLongitude" ).FirstOrDefault();

            _groupTypeRoleIdsWithGroupSync = new HashSet<int>( _group.GroupSyncs.Select( a => a.GroupTypeRoleId ).ToList() );

            var rockContext = new RockContext();

            if ( _group != null &&
                _group.RequiredSignatureDocumentTemplateId.HasValue )
            {
                _showPersonsThatHaventSigned = true;
                _personIdsThatHaveSigned = new HashSet<int>( new SignatureDocumentService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( d =>
                        d.SignatureDocumentTemplateId == _group.RequiredSignatureDocumentTemplateId.Value &&
                        d.Status == SignatureDocumentStatus.Signed &&
                        d.BinaryFileId.HasValue &&
                        d.AppliesToPersonAlias != null )
                    .OrderByDescending( d => d.LastStatusDate )
                    .Select( d => d.AppliesToPersonAlias.PersonId )
                    .Distinct()
                    .ToList() );
            }
            else
            {
                _personIdsThatHaveSigned = new HashSet<int>();
                _showPersonsThatHaventSigned = false;
            }

            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            var qry = groupMemberService.Queryable( true )
                .Include( a => a.GroupRole )
                .Include( a => a.Person )
                .AsNoTracking()
                .Where( m => m.GroupId == _group.Id );

            if ( isCommunication )
            {
                qry = qry.Where( a => a.GroupMemberStatus != GroupMemberStatus.Inactive );
            }

            // Filter by First Name
            string firstName = tbFirstName.Text;
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                qry = qry.Where( m =>
                    m.Person.FirstName.StartsWith( firstName ) ||
                    m.Person.NickName.StartsWith( firstName ) );
            }

            // Filter by Last Name
            string lastName = tbLastName.Text;
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                qry = qry.Where( m => m.Person.LastName.StartsWith( lastName ) );
            }

            // Filter by role
            var validGroupTypeRoles = _groupTypeCache.Roles.Select( r => r.Id ).ToList();
            var roles = new List<int>();
            foreach ( var roleId in cblRole.SelectedValues.AsIntegerList() )
            {
                if ( validGroupTypeRoles.Contains( roleId ) )
                {
                    roles.Add( roleId );
                }
            }

            if ( roles.Any() )
            {
                qry = qry.Where( m => roles.Contains( m.GroupRoleId ) );
            }

            // Filter by Group Member Status
            var statuses = new List<GroupMemberStatus>();
            foreach ( string status in cblGroupMemberStatus.SelectedValues )
            {
                if ( !string.IsNullOrWhiteSpace( status ) )
                {
                    statuses.Add( status.ConvertToEnum<GroupMemberStatus>() );
                }
            }

            if ( statuses.Any() )
            {
                qry = qry.Where( m => statuses.Contains( m.GroupMemberStatus ) );
            }

            var genders = new List<Gender>();
            foreach ( var item in cblGenderFilter.SelectedValues )
            {
                var gender = item.ConvertToEnum<Gender>();
                genders.Add( gender );
            }

            if ( genders.Any() )
            {
                qry = qry.Where( m => genders.Contains( m.Person.Gender ) );
            }

            // Filter by Campus
            if ( cpCampusFilter.SelectedCampusId.HasValue )
            {
                int familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                int campusId = cpCampusFilter.SelectedCampusId.Value;
                var qryFamilyMembersForCampus = new GroupMemberService( rockContext ).Queryable().Where( a => a.Group.GroupTypeId == familyGroupTypeId && a.Group.CampusId == campusId );
                qry = qry.Where( a => qryFamilyMembersForCampus.Any( f => f.PersonId == a.PersonId ) );
            }

            // Filter by Registration
            var instanceId = ddlRegistration.SelectedValueAsInt();
            if ( instanceId.HasValue )
            {
                var registrants = new RegistrationRegistrantService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( r =>
                        r.Registration != null &&
                        r.Registration.RegistrationInstanceId == instanceId.Value &&
                        r.PersonAlias != null )
                    .Select( r => r.PersonAlias.PersonId );

                qry = qry.Where( m => registrants.Contains( m.PersonId ) );
            }

            // Filter by signed documents
            if ( _personIdsThatHaveSigned != null )
            {
                if ( ddlSignedDocument.SelectedValue.AsBooleanOrNull() == true )
                {
                    qry = qry.Where( m => _personIdsThatHaveSigned.Contains( m.PersonId ) );
                }
                else if ( ddlSignedDocument.SelectedValue.AsBooleanOrNull() == false )
                {
                    qry = qry.Where( m => !_personIdsThatHaveSigned.Contains( m.PersonId ) );
                }
            }

            // Filter by date added range
            var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateAdded.DelimitedValues );

            if ( dateRange.Start.HasValue )
            {
                qry = qry.Where( m =>
                    m.DateTimeAdded.HasValue &&
                    m.DateTimeAdded.Value >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                var end = dateRange.End.Value.AddHours( 23 ).AddMinutes( 59 ).AddSeconds( 59 );
                qry = qry.Where( m =>
                    m.DateTimeAdded.HasValue &&
                    m.DateTimeAdded.Value < end );
            }

            // Filter query by any configured attribute filters
            if ( AvailableAttributes != null && AvailableAttributes.Any() )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, groupMemberService, Rock.Reporting.FilterMode.SimpleFilter );
                }
            }

            _inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );

            _hasGroupRequirements = new GroupRequirementService( rockContext ).Queryable().Where( a => ( a.GroupId.HasValue && a.GroupId == _group.Id ) || ( a.GroupTypeId.HasValue && a.GroupTypeId == _group.GroupTypeId ) ).Any();

            // If there are group requirements that that member doesn't meet, show an icon in the grid
            bool includeWarnings = false;
            _groupMemberIdsThatLackGroupRequirements = new HashSet<int>( new GroupService( rockContext ).GroupMembersNotMeetingRequirements( _group, includeWarnings ).Select( a => a.Key.Id ).ToList().Distinct() );

            gGroupMembers.EntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() ).Id;

            if ( isExporting )
            {
                var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;
                var personFamily = new GroupMemberService( rockContext ).Queryable()
                    .Where( a => qry.Any( m => m.PersonId == a.PersonId ) )
                    .Where( m => m.Group.GroupTypeId == familyGroupTypeId )
                    .Select( m => new
                    {
                        m.PersonId,
                        m.Group
                    } );

                // preload all phonenumbers for the person in the qry in one query so that we don't have to fetch them individually
                _personIdPhoneNumberTypePhoneNumberLookup = new PhoneNumberService( rockContext ).Queryable()
                    .Where( n => personFamily.Any( x => x.PersonId == n.PersonId ) && n.NumberTypeValueId.HasValue )
                    .GroupBy( a => new { a.PersonId, a.NumberTypeValueId } )
                    .Select( a => new
                    {
                        a.Key.PersonId,
                        a.Key.NumberTypeValueId,
                        NumberFormatted = a.Select( x => x.NumberFormatted ).FirstOrDefault()
                    } )
                    .GroupBy( a => a.PersonId )
                    .ToDictionary( k => k.Key, v => v.ToDictionary( xk => xk.NumberTypeValueId.Value, xv => xv.NumberFormatted ) );

                // preload all mapped home locations for the person in the qry in one query so that we don't have to fetch them individually
                Guid? homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
                if ( homeAddressGuid.HasValue )
                {
                    var homeAddressDv = DefinedValueCache.Get( homeAddressGuid.Value );
                    if ( homeAddressDv != null )
                    {
                        var locationsQry = personFamily
                            .Select( pf => new
                            {
                                HomeLocation = pf.Group.GroupLocations.Where( l => l.GroupLocationTypeValueId == homeAddressDv.Id && l.IsMappedLocation ).Select( l => l.Location ).FirstOrDefault(),
                                GroupOrder = pf.Group.Order,
                                PersonId = pf.PersonId
                            } );

                        _personIdHomeLocationLookup = locationsQry.GroupBy( a => a.PersonId ).ToDictionary( k => k.Key, v => v.OrderBy( a => a.GroupOrder ).Select( x => x.HomeLocation ).FirstOrDefault() );
                    }
                }
            }

            SortProperty sortProperty = gGroupMembers.SortProperty;
            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.GroupRole.Order ).ThenBy( a => a.Person.LastName ).ThenBy( a => a.Person.FirstName );
            }

            var groupMemberIdQuery = qry.Select( m => m.Id );

            _groupMembersWithGroupMemberHistory = new HashSet<int>( new GroupMemberHistoricalService( rockContext ).Queryable().Where( a => a.GroupId == groupId ).Select( a => a.GroupMemberId ).ToList() );

            // Get all the group members with any associated registrations
            _groupMembersWithRegistrations = new RegistrationRegistrantService( rockContext )
                .Queryable().AsNoTracking()
                .Where( r =>
                    r.Registration != null &&
                    r.Registration.RegistrationInstance != null &&
                    r.GroupMemberId.HasValue &&
                    groupMemberIdQuery.Contains( r.GroupMemberId.Value ) )
                .ToList()
                .GroupBy( r => r.GroupMemberId.Value )
                .Select( g => new
                {
                    GroupMemberId = g.Key,
                    Registrations = g.ToList()
                        .Select( r => new
                        {
                            Id = r.Registration.Id,
                            Name = r.Registration.RegistrationInstance.Name
                        } ).Distinct()
                        .Select( r => new GroupMemberRegistrationItem { RegistrationId = r.Id, RegistrationName = r.Name } ).ToList()
                } )
                .ToDictionary( r => r.GroupMemberId, r => r.Registrations );

            if ( _registrationField != null )
            {
                _registrationField.Visible = _groupMembersWithRegistrations.Any();
            }

            _connectionStatusField = gGroupMembers.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lConnectionStatusValue" );
            if ( _connectionStatusField != null )
            {
                _connectionStatusField.Visible = _groupTypeCache.ShowConnectionStatus;
            }

            _maritalStatusField = gGroupMembers.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lMaritalStatusValue" );
            if ( _maritalStatusField != null )
            {
                _maritalStatusField.Visible = _groupTypeCache.ShowMaritalStatus;
            }

            _personIdAttendanceFirstLastLookup = new Dictionary<int, DateRange>();
            _showAttendance = GetAttributeValue( SHOW_FIRST_LAST_ATTENDANCE_KEY ).AsBoolean() && _groupTypeCache.TakesAttendance;
            _firstAttendedField.Visible = _showAttendance;
            _lastAttendedField.Visible = _showAttendance;
            if ( _showAttendance )
            {
                foreach ( var attendance in new AttendanceService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.Occurrence.GroupId.HasValue && a.Occurrence.GroupId.Value == _group.Id &&
                        a.DidAttend.HasValue && a.DidAttend.Value )
                    .GroupBy( a => a.PersonAlias.PersonId )
                    .Select( g => new
                    {
                        PersonId = g.Key,
                        FirstAttended = g.Min( a => a.StartDateTime ),
                        LastAttended = g.Max( a => a.StartDateTime )
                    } )
                    .ToList() )
                {
                    _personIdAttendanceFirstLastLookup.Add( attendance.PersonId, new DateRange( attendance.FirstAttended, attendance.LastAttended ) );
                }
            }

            _showDateAdded = GetAttributeValue( "ShowDateAdded" ).AsBoolean();
            gGroupMembers.ColumnsOfType<DateField>().First( a => a.DataField == "DateTimeAdded" ).Visible = _showDateAdded;

            _showNoteColumn = GetAttributeValue( "ShowNoteColumn" ).AsBoolean();
            gGroupMembers.ColumnsOfType<RockBoundField>().First( a => a.DataField == "Note" ).Visible = _showNoteColumn;

            gGroupMembers.SetLinqDataSource( qry );
            gGroupMembers.DataBind();
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string ResolveValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}
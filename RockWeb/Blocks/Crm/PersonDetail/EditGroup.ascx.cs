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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile block the main information about a person 
    /// </summary>
    [DisplayName( "Edit Group" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to edit a group that person belongs to." )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        name: "Default Connection Status",
        description: "The connection status that should be set by default",
        required: false,
        allowMultiple: false,
        defaultValue: Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR,
        order: 0,
        key: "DefaultConnectionStatus" )]
    [BooleanField( "Require Campus",
        description: "Determines if a campus is required.",
        defaultValue: true,
        order: 1,
        key: "RequireCampus" )]
    [BooleanField( "Require Birthdate",
        description: "Determines if a birthdate should be required.",
        defaultValue: false,
        order: 2,
        key: "RequireBirthdate" )]
    [BooleanField( "Hide Title",
        description: "Should Title field be hidden when entering new people?.",
        defaultValue: false,
        order: 3,
        key: "HideTitle" )]
    [BooleanField( "Hide Suffix",
        description: "Should Suffix field be hidden when entering new people?.",
        defaultValue: false,
        order: 4,
        key: "HideSuffix" )]
    [BooleanField( "Hide Grade",
        description: "Should Grade field be hidden when entering new people?.",
        defaultValue: false,
        order: 5,
        key: "HideGrade" )]
    [BooleanField( "Show Age",
        description: "Should Age of Family Members be displayed?.",
        defaultValue: false,
        order: 6,
        key: "ShowAge" )]
    [BooleanField( "Show County",
        description: "Should County be displayed when editing an address?",
        defaultValue: false,
        order: 7,
        key: "ShowCounty" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        name: "New Person Phone",
        description: "The Phone Type to prompt for when adding a new person to family (if any).",
        required: false,
        allowMultiple: false,
        order: 8,
        key: "NewPersonPhone" )]
    [BooleanField( "New Person Email",
        description: "Should an Email field be displayed when adding a new person to the family?",
        defaultValue: false,
        order: 9,
        key: "NewPersonEmail" )]
    public partial class EditGroup : PersonBlock
    {
        private GroupTypeCache _groupType = null;
        private bool _isFamilyGroupType = false;
        private Group _group = null;
        private bool _canEdit = false;
        private bool _showAge = false;
        private bool _showEmail = false;
        private bool _showCounty = false;
        private DefinedValueCache _showPhoneType = null;

        protected string basePersonUrl { get; set; }
        protected string GroupTypeName { get; set; }

        private List<GroupMemberInfo> GroupMembers
        {
            get { return ViewState["GroupMembers"] as List<GroupMemberInfo>; }
            set { ViewState["GroupMembers"] = value; }
        }

        private List<GroupAddressInfo> GroupAddresses
        {
            get { return ViewState["GroupAddresses"] as List<GroupAddressInfo>; }
            set { ViewState["GroupAddresses"] = value; }
        }

        private bool HasDeceasedMembers
        {
            get { return ViewState["HasDeceasedMembers"] as bool? ?? false; }
            set { ViewState["HasDeceasedMembers"] = value; }
        }

        private string DefaultState
        {
            get
            {
                string state = ViewState["DefaultState"] as string;
                if ( state == null )
                {
                    string orgLocGuid = GlobalAttributesCache.Value( "OrganizationAddress" );
                    if ( !string.IsNullOrWhiteSpace( orgLocGuid ) )
                    {
                        Guid locGuid = Guid.Empty;
                        if ( Guid.TryParse( orgLocGuid, out locGuid ) )
                        {
                            var location = new Rock.Model.LocationService( new RockContext() ).Get( locGuid );
                            if ( location != null )
                            {
                                state = location.State;
                                ViewState["DefaultState"] = state;
                            }
                        }
                    }
                }

                return state;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _showAge = GetAttributeValue( "ShowAge" ).AsBoolean();

            var rockContext = new RockContext();

            int groupId = int.MinValue;
            if ( int.TryParse( PageParameter( "GroupId" ), out groupId ) )
            {
                _group = new GroupService( rockContext ).Get( groupId );
            }

            if ( _group == null )
            {
                nbInvalidGroup.Text = "Sorry, but the specified group was not found.";
                nbInvalidGroup.NotificationBoxType = NotificationBoxType.Danger;
                nbInvalidGroup.Visible = true;

                _group = null;
                pnlEditGroup.Visible = false;
                return;
            }
            else
            {
                _groupType = GroupTypeCache.Get( _group.GroupTypeId );

                rblNewPersonRole.DataSource = _groupType.Roles.OrderBy( r => r.Order ).ToList();
                rblNewPersonRole.DataBind();

                if ( _groupType != null )
                {
                    _isFamilyGroupType = _groupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                }
                else
                {
                    _groupType = GroupTypeCache.GetFamilyGroupType();
                    _isFamilyGroupType = true;
                }

                GroupTypeName = _groupType.Name;
                tbGroupName.Label = _groupType.Name + " Name";
                confirmExit.ConfirmationMessage = string.Format( "Changes have been made to this {0} that have not yet been saved.", _groupType.Name.ToLower() );
                cbRemoveOtherGroups.Text = string.Format( "Remove person from other {0}.", _groupType.Name.ToLower().Pluralize() );

                var homeLocType = _groupType.LocationTypeValues.FirstOrDefault( v => v.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ) );
                var prevLocType = _groupType.LocationTypeValues.FirstOrDefault( v => v.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() ) );
                lbMoved.Visible = ( homeLocType != null && prevLocType != null );

                basePersonUrl = ResolveUrl( "~/Person/" );
                btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", _groupType.Name );
            }

            _canEdit = IsUserAuthorized( Authorization.EDIT );

            var campusi = CampusCache.All();
            cpCampus.Campuses = campusi;
            cpCampus.Visible = campusi.Any();

            if ( _isFamilyGroupType )
            {
                cpCampus.Required = GetAttributeValue( "RequireCampus" ).AsBoolean( true );

                dvpRecordStatus.Visible = true;
                dvpRecordStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid() ).Id;
                dvpReason.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() ).Id;
            }
            else
            {
                cpCampus.Required = false;
                dvpRecordStatus.Visible = false;
            }

            dvpGroupStatus.DefinedTypeId = _groupType.GroupStatusDefinedTypeId;
            if ( _groupType.GroupStatusDefinedType != null )
            {
                dvpGroupStatus.Label = _groupType.GroupStatusDefinedType.ToString();
            }

            dvpGroupStatus.Visible = _groupType.GroupStatusDefinedTypeId.HasValue;

            dvpNewPersonTitle.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ).Id;
            dvpNewPersonSuffix.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ).Id;
            dvpNewPersonMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
            dvpNewPersonConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Id;

            lvMembers.DataKeyNames = new string[] { "Index" };
            lvMembers.ItemDataBound += lvMembers_ItemDataBound;
            lvMembers.ItemCommand += lvMembers_ItemCommand;

            modalAddPerson.SaveButtonText = "Save";
            modalAddPerson.SaveClick += modalAddPerson_SaveClick;
            modalAddPerson.OnCancelScript = string.Format( "$('#{0}').val('');", hfActiveTab.ClientID );

            gLocations.DataKeyNames = new string[] { "Id" };
            gLocations.RowDataBound += gLocations_RowDataBound;
            gLocations.RowEditing += gLocations_RowEditing;
            gLocations.RowUpdating += gLocations_RowUpdating;
            gLocations.RowCancelingEdit += gLocations_RowCancelingEdit;
            gLocations.Actions.ShowAdd = _canEdit;
            gLocations.Actions.ShowAdd = true;
            gLocations.Actions.AddClick += gLocations_Add;
            gLocations.IsDeleteEnabled = _canEdit;
            gLocations.GridRebind += gLocations_GridRebind;

            rblNewPersonGender.Items.Clear();
            rblNewPersonGender.Items.Add( new ListItem( Gender.Male.ConvertToString(), Gender.Male.ConvertToInt().ToString() ) );
            rblNewPersonGender.Items.Add( new ListItem( Gender.Female.ConvertToString(), Gender.Female.ConvertToInt().ToString() ) );
            rblNewPersonGender.Items.Add( new ListItem( Gender.Unknown.ConvertToString(), Gender.Unknown.ConvertToInt().ToString() ) );

            btnSave.Visible = _canEdit;

            // Save and Cancel should not confirm exit
            btnSave.OnClientClick = string.Format( "javascript:$('#{0}').val('');return true;", confirmExit.ClientID );
            btnCancel.OnClientClick = string.Format( "javascript:$('#{0}').val('');return true;", confirmExit.ClientID );

            _showEmail = GetAttributeValue( "NewPersonEmail" ).AsBoolean();
            _showPhoneType = DefinedValueCache.Get( GetAttributeValue( "NewPersonPhone" ).AsGuid() );
            _showCounty = GetAttributeValue( "ShowCounty" ).AsBoolean();
            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbAddPerson.Visible = false;

            if ( Page.IsPostBack )
            {
                foreach ( var item in lvMembers.Items )
                {
                    var rblRole = item.FindControl( "rblRole" ) as RadioButtonList;
                    if ( rblRole != null )
                    {
                        int? roleId = rblRole.SelectedValueAsInt();
                        if ( roleId.HasValue )
                        {
                            var role = _groupType.Roles.Where( r => r.Id == roleId.Value ).FirstOrDefault();
                            if ( role != null )
                            {
                                int index = (int)lvMembers.DataKeys[item.DataItemIndex]["Index"];
                                if ( GroupMembers != null )
                                {
                                    var groupMember = GroupMembers.Where( m => m.Index == index ).FirstOrDefault();
                                    if ( groupMember != null )
                                    {
                                        groupMember.RoleGuid = role.Guid;
                                        groupMember.RoleName = role.Name;
                                        groupMember.IsLeader = role.IsLeader;
                                    }
                                }
                            }
                        }
                    }
                }

                if ( !string.IsNullOrWhiteSpace( hfActiveTab.Value ) )
                {
                    SetActiveTab();
                    modalAddPerson.Show();
                }

                BuildAttributes( false );
            }
            else
            {
                if ( _group != null )
                {
                    tbGroupName.Text = _group.Name;

                    // add banner text
                    if ( _isFamilyGroupType && !_group.Name.ToLower().EndsWith( " family" ) )
                    {
                        lBanner.Text = ( _group.Name + " Family" ).FormatAsHtmlTitle();
                    }
                    else
                    {
                        lBanner.Text = _group.Name.FormatAsHtmlTitle();
                    }

                    cpCampus.SelectedCampusId = _group.CampusId;

                    if ( _isFamilyGroupType )
                    {
                        // If all group members have the same record status, display that value
                        if ( _group.Members.Select( m => m.Person.RecordStatusValueId ).Distinct().Count() == 1 )
                        {
                            dvpRecordStatus.SetValue( _group.Members.Select( m => m.Person.RecordStatusValueId ).FirstOrDefault() );
                        }
                        else
                        {
                            dvpRecordStatus.Warning = String.Format( "{0} members have different record statuses", _groupType.Name );
                        }

                        // If all group members have the same inactive reason, set that value
                        if ( _group.Members.Select( m => m.Person.RecordStatusReasonValueId ).Distinct().Count() == 1 )
                        {
                            dvpReason.SetValue( _group.Members.Select( m => m.Person.RecordStatusReasonValueId ).FirstOrDefault() );
                        }
                        else
                        {
                            if ( String.IsNullOrWhiteSpace( dvpRecordStatus.Warning ) )
                            {
                                dvpRecordStatus.Warning = String.Format( "{0} members have different record status reasons", _groupType.Name );
                            }
                            else
                            {
                                dvpRecordStatus.Warning += " and record status reasons";
                            }
                        }

                        // Does the family have any deceased members?
                        var inactiveStatus = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
                        if ( _group.Members.Where( m => m.Person.RecordStatusValueId == inactiveStatus ).Any() )
                        {
                            HasDeceasedMembers = true;
                        }
                    }

                    dvpGroupStatus.SetValue( _group.StatusValueId );

                    // Get all the group members
                    GroupMembers = new List<GroupMemberInfo>();
                    foreach ( var groupMember in _group.Members )
                    {
                        GroupMembers.Add( new GroupMemberInfo( groupMember, true ) );
                    }

                    // Figure out which ones are in another group
                    var groupMemberPersonIds = GroupMembers.Select( m => m.PersonId ).ToList();
                    var otherGroupPersonIds = new GroupMemberService( new RockContext() ).Queryable()
                        .Where( m =>
                            groupMemberPersonIds.Contains( m.PersonId ) &&
                            m.Group.GroupTypeId == _groupType.Id &&
                            m.GroupId != _group.Id )
                        .Select( m => m.PersonId )
                        .Distinct();
                    GroupMembers
                        .Where( m => otherGroupPersonIds.Contains( m.PersonId ) )
                        .ToList()
                        .ForEach( m => m.IsInOtherGroups = true );

                    BindMembers();

                    GroupAddresses = new List<GroupAddressInfo>();
                    foreach ( var groupLocation in _group.GroupLocations
                        .Where( l => l.GroupLocationTypeValue != null )
                        .OrderBy( l => l.GroupLocationTypeValue.Order ) )
                    {
                        GroupAddresses.Add( new GroupAddressInfo( groupLocation, _showCounty ) );
                    }
                    foreach ( var groupLocation in _group.GroupLocations
                        .Where( l => l.GroupLocationTypeValue == null ) )
                    {
                        GroupAddresses.Add( new GroupAddressInfo( groupLocation, _showCounty ) );
                    }

                    BindLocations();

                    BuildAttributes( true );

                    string roleLimitWarnings;
                    nbRoleLimitWarning.Visible = _group.GetGroupTypeRoleLimitWarnings( out roleLimitWarnings );
                    nbRoleLimitWarning.Text = roleLimitWarnings;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( modalAddPerson.Visible )
            {
                string script = string.Format( @"

    $('#{0}').on('click', function () {{

        // if Save was clicked, set the fields that should be validated based on what tab they are on
        if ($('#{9}').val() == 'Existing') {{
            enableRequiredField( '{1}', true )
            enableRequiredField( '{2}_rfv', false );
            enableRequiredField( '{3}_rfv', false );
            enableRequiredField( '{4}', false );
            enableRequiredField( '{5}', false );
            enableRequiredField( '{6}_rfv', false );
            enableRequiredField( '{10}_rfv', false );
        }} else {{
            enableRequiredField('{1}', false)
            enableRequiredField('{2}_rfv', true);
            enableRequiredField('{3}_rfv', true);
            enableRequiredField('{4}', true);
            enableRequiredField('{5}', true);
            enableRequiredField('{6}_rfv', true);
            enableRequiredField('{10}_rfv', true);
        }}

        // update the scrollbar since our validation box could show
        setTimeout( function ()
        {{
            Rock.dialogs.updateModalScrollBar( '{7}' );
        }});

    }})

    $('a[data-toggle=""pill""]').on('shown.bs.tab', function (e) {{

        var tabHref = $( e.target ).attr( 'href' );
        if ( tabHref == '#{8}' )
        {{
            $( '#{9}' ).val( 'Existing' );
        }} else {{
            $( '#{9}' ).val( 'New' );
        }}

        // if the validation error summary is shown, hide it when they switch tabs
        $( '#{7}' ).hide();
    }});
",
                    modalAddPerson.ServerSaveLink.ClientID,                         // {0}
                    ppPerson.RequiredFieldValidator.ClientID,                       // {1}
                    tbNewPersonFirstName.ClientID,                                  // {2}
                    tbNewPersonLastName.ClientID,                                   // {3}
                    rblNewPersonRole.RequiredFieldValidator.ClientID,               // {4}
                    rblNewPersonGender.RequiredFieldValidator.ClientID,             // {5}
                    dvpNewPersonConnectionStatus.ClientID,                          // {6}
                    valSummaryAddPerson.ClientID,                                   // {7}
                    divExistingPerson.ClientID,                                     // {8}
                    hfActiveTab.ClientID,                                           // {9}
                    dpNewPersonBirthDate.ClientID                                   // {10}
                );

                ScriptManager.RegisterStartupScript( modalAddPerson, modalAddPerson.GetType(), "modaldialog-validation", script, true );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // Currently we need to go through the whole page cycle to get all of the data.
            NavigateToCurrentPageReference();
        }

        #region Events

        /// <summary>
        /// Handles the TextChanged event of the tbGroupName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tbGroupName_TextChanged( object sender, EventArgs e )
        {
            confirmExit.Enabled = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            confirmExit.Enabled = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRecordStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlRecordStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            var inactiveStatus = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
            if ( HasDeceasedMembers && dvpRecordStatus.SelectedValueAsInt() != inactiveStatus )
            {
                dvpRecordStatus.Warning = "Note: the status of deceased people will not be changed.";
            }

            dvpReason.Visible = dvpRecordStatus.SelectedValueAsInt() == inactiveStatus;
            confirmExit.Enabled = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlReason control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlReason_SelectedIndexChanged( object sender, EventArgs e )
        {
            confirmExit.Enabled = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the dvpGroupStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void dvpGroupStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            confirmExit.Enabled = true;
        }

        #region Group Member List Events

        /// <summary>
        /// Handles the ItemDataBound event of the lvMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void lvMembers_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                var groupMember = e.Item.DataItem as GroupMemberInfo;
                if ( groupMember != null )
                {
                    // very similar code in EditFamily.ascx.cs
                    HtmlControl divPersonImage = e.Item.FindControl( "divPersonImage" ) as HtmlControl;
                    if ( divPersonImage != null )
                    {
                        divPersonImage.Style.Add( "background-image", @String.Format( @"url({0})", Person.GetPersonPhotoUrl( groupMember.PersonId, groupMember.PhotoId, groupMember.Age, groupMember.Gender, groupMember.RecordTypeValueGuid, groupMember.AgeClassification ) + "&width=65" ) );
                    }

                    var rblRole = e.Item.FindControl( "rblRole" ) as RadioButtonList;
                    if ( rblRole != null )
                    {
                        var groupRoles = _groupType.Roles.OrderBy( r => r.Order ).ToList();
                        rblRole.DataSource = groupRoles;
                        rblRole.DataBind();

                        var role = groupRoles.Where( r => r.Guid.Equals( groupMember.RoleGuid ) ).FirstOrDefault();
                        if ( role != null )
                        {
                            rblRole.SelectedValue = role.Id.ToString();
                        }
                    }

                    int members = GroupMembers.Where( m => !m.Removed ).Count();

                    var lbNewGroup = e.Item.FindControl( "lbNewGroup" ) as LinkButton;
                    if ( lbNewGroup != null )
                    {
                        lbNewGroup.ToolTip = string.Format( "Move to New {0}", _groupType.Name );
                        lbNewGroup.Visible = _canEdit && groupMember.ExistingGroupMember && members > 1;
                    }

                    var lbRemoveMember = e.Item.FindControl( "lbRemoveMember" ) as LinkButton;
                    if ( lbRemoveMember != null )
                    {
                        lbRemoveMember.ToolTip = string.Format( "Remove from {0}", _groupType.Name );
                        lbRemoveMember.Visible = _canEdit && ( !groupMember.ExistingGroupMember || groupMember.IsInOtherGroups ) && members > 1;
                    }

                    var lFamilyMemberAge = e.Item.FindControl( "lFamilyMemberAge" ) as Literal;
                    if ( lFamilyMemberAge != null )
                    {
                        lFamilyMemberAge.Text = ( _showAge && groupMember.Age.HasValue ) ? string.Format( " ({0})", groupMember.Age ) : string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvMembers_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            int index = (int)lvMembers.DataKeys[e.Item.DataItemIndex]["Index"];
            var groupMemberInfo = GroupMembers.Where( m => m.Index == index ).FirstOrDefault();
            if ( groupMemberInfo != null )
            {
                if ( e.CommandName == "Move" )
                {
                    groupMemberInfo.Removed = true;
                }
                else if ( e.CommandName == "Remove" )
                {
                    if ( groupMemberInfo.ExistingGroupMember )
                    {
                        groupMemberInfo.Removed = true;
                    }
                    else
                    {
                        GroupMembers.RemoveAt( index );
                    }
                }

                confirmExit.Enabled = true;

                BindMembers();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPerson_Click( object sender, EventArgs e )
        {
            tbNewPersonFirstName.Required = true;
            tbNewPersonLastName.Required = true;
            rblNewPersonRole.Required = true;
            rblNewPersonGender.Required = true;
            dvpNewPersonConnectionStatus.Required = true;
            var connectionStatusGuid = GetAttributeValue( "DefaultConnectionStatus" ).AsGuidOrNull();
            if ( connectionStatusGuid.HasValue )
            {
                var defaultConnectionStatus = DefinedValueCache.Get( connectionStatusGuid.Value );
                if ( defaultConnectionStatus != null )
                {
                    dvpNewPersonConnectionStatus.SetValue( defaultConnectionStatus.Id );
                }
            }

            hfActiveTab.Value = "New";
            SetActiveTab();

            ppPerson.SetValue( null );

            dvpNewPersonTitle.SelectedIndex = 0;
            dvpNewPersonTitle.Visible = !GetAttributeValue( "HideTitle" ).AsBoolean();

            tbNewPersonFirstName.Text = string.Empty;

            // default the last name of the new family member to the lastname of the existing adults in the family (if all the adults have the same last name)
            var lastNames = GroupMembers.Where( a => a.RoleGuid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Select( a => a.LastName ).Distinct().ToList();
            if ( lastNames.Count == 1 )
            {
                tbNewPersonLastName.Text = lastNames[0];
            }
            else
            {
                tbNewPersonLastName.Text = string.Empty;
            }

            dvpNewPersonSuffix.SelectedIndex = 0;
            dvpNewPersonSuffix.Visible = !GetAttributeValue( "HideSuffix" ).AsBoolean();

            foreach ( ListItem li in rblNewPersonRole.Items )
            {
                li.Selected = false;
            }

            dvpNewPersonMaritalStatus.SelectedIndex = 0;
            foreach ( ListItem li in rblNewPersonGender.Items )
            {
                li.Selected = false;
            }

            dpNewPersonBirthDate.SelectedDate = null;
            dpNewPersonBirthDate.Required = GetAttributeValue( "RequireBirthdate" ).AsBoolean( true );

            ddlGradePicker.SelectedIndex = 0;
            ddlGradePicker.Visible = !GetAttributeValue( "HideGrade" ).AsBoolean();

            tbNewPersonEmail.Visible = _showEmail;
            pnNewPersonPhoneNumber.Visible = _showPhoneType != null;
            pnNewPersonPhoneNumber.Label = _showPhoneType != null ? _showPhoneType.Value : "";

            modalAddPerson.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void modalAddPerson_SaveClick( object sender, EventArgs e )
        {
            var validationMessages = new List<string>();

            bool isValid = true;
            if ( hfActiveTab.Value == "Existing" )
            {
            }
            else
            {
                DateTime? birthdate = dpNewPersonBirthDate.SelectedDate;
                if ( !dpNewPersonBirthDate.IsValid )
                {
                    isValid = false;
                }
                else if ( dpNewPersonBirthDate.IsValid && !birthdate.HasValue && GetAttributeValue( "RequireBirthdate" ).AsBoolean() )
                {
                    validationMessages.Add( "Birthdate is Required." );
                    isValid = false;
                }
            }

            if ( !isValid )
            {
                if ( validationMessages.Any() )
                {
                    nbAddPerson.Text = "<ul><li>" + validationMessages.AsDelimited( "</li><li>" ) + "</li></lu>";
                    nbAddPerson.Visible = true;
                }

                return;
            }

            if ( hfActiveTab.Value == "Existing" )
            {
                if ( ppPerson.PersonId.HasValue )
                {
                    var existingGroupMember = GroupMembers.Where( m => m.PersonId == ppPerson.PersonId.Value ).FirstOrDefault();
                    if ( existingGroupMember != null )
                    {
                        existingGroupMember.Removed = false;
                    }
                    else
                    {
                        var rockContext = new RockContext();
                        var person = new PersonService( rockContext ).Get( ppPerson.PersonId.Value );
                        if ( person != null )
                        {
                            var groupMember = new GroupMemberInfo();
                            groupMember.SetValuesFromPerson( person );

                            var groupRoleIds = _groupType.Roles.Select( r => r.Id ).ToList();

                            var existingGroupMembers = new GroupMemberService( rockContext ).Queryable( "GroupRole" )
                                .Where( m => m.PersonId == person.Id && groupRoleIds.Contains( m.GroupRoleId ) )
                                .OrderBy( m => m.GroupRole.Order )
                                .ToList();

                            var existingRole = existingGroupMembers.Select( m => m.GroupRole ).FirstOrDefault();
                            if ( existingRole != null )
                            {
                                groupMember.RoleGuid = existingRole.Guid;
                                groupMember.RoleName = existingRole.Name;
                                groupMember.IsLeader = existingRole.IsLeader;
                            }

                            groupMember.ExistingGroupMember = existingGroupMembers.Any( r => r.GroupId == _group.Id );
                            groupMember.RemoveFromOtherGroups = cbRemoveOtherGroups.Checked;

                            GroupMembers.Add( groupMember );
                        }
                    }
                }
            }
            else
            {
                var groupMember = new GroupMemberInfo();
                groupMember.TitleValueId = dvpNewPersonTitle.SelectedValueAsId();
                groupMember.FirstName = tbNewPersonFirstName.Text;
                groupMember.NickName = tbNewPersonFirstName.Text;
                groupMember.LastName = tbNewPersonLastName.Text;
                groupMember.SuffixValueId = dvpNewPersonSuffix.SelectedValueAsId();
                groupMember.Gender = rblNewPersonGender.SelectedValueAsEnum<Gender>();
                groupMember.MaritalStatusValueId = dvpNewPersonMaritalStatus.SelectedValueAsInt();

                if ( _showEmail )
                {
                    groupMember.Email = tbNewPersonEmail.Text;
                }

                groupMember.PhoneNumbers = new List<GroupMemberPhoneInfo>();
                if ( _showPhoneType != null && !string.IsNullOrWhiteSpace( pnNewPersonPhoneNumber.Text) )
                {
                    var pn = new GroupMemberPhoneInfo();
                    pn.PhoneTypeId = _showPhoneType.Id;
                    pn.Number = PhoneNumber.CleanNumber( pnNewPersonPhoneNumber.Text );
                    groupMember.PhoneNumbers.Add( pn );
                }

                DateTime? birthdate = dpNewPersonBirthDate.SelectedDate;

                groupMember.BirthDate = birthdate;
                groupMember.GradeOffset = ddlGradePicker.SelectedValueAsInt();
                groupMember.ConnectionStatusValueId = dvpNewPersonConnectionStatus.SelectedValueAsId();
                var role = _groupType.Roles.Where( r => r.Id == ( rblNewPersonRole.SelectedValueAsInt() ?? 0 ) ).FirstOrDefault();
                if ( role != null )
                {
                    groupMember.RoleGuid = role.Guid;
                    groupMember.RoleName = role.Name;
                    groupMember.IsLeader = role.IsLeader;
                }

                GroupMembers.Add( groupMember );
            }

            ppPerson.Required = false;
            tbNewPersonFirstName.Required = false;
            tbNewPersonLastName.Required = false;
            rblNewPersonRole.Required = false;
            rblNewPersonGender.Required = false;
            dvpNewPersonConnectionStatus.Required = false;

            confirmExit.Enabled = true;

            hfActiveTab.Value = string.Empty;

            modalAddPerson.Hide();

            BindMembers();

        }

        #endregion

        #region Location Events

        /// <summary>
        /// Handles the Click event of the lbMoved control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMoved_Click( object sender, EventArgs e )
        {
            var homeLocType = _groupType.LocationTypeValues.FirstOrDefault( v => v.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ) );
            var prevLocType = _groupType.LocationTypeValues.FirstOrDefault( v => v.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() ) );

            if ( homeLocType != null && prevLocType != null )
            {
                bool setLocation = false;

                foreach ( var groupAddress in GroupAddresses )
                {
                    if ( groupAddress.LocationTypeId == homeLocType.Id )
                    {
                        if ( groupAddress.IsLocation )
                        {
                            groupAddress.IsLocation = false;
                            setLocation = true;
                        }

                        groupAddress.IsMailing = false;
                        groupAddress.LocationTypeId = prevLocType.Id;
                        groupAddress.LocationTypeName = prevLocType.Value;
                    }
                }

                GroupAddresses.Add( new GroupAddressInfo
                {
                    LocationTypeId = homeLocType.Id,
                    LocationTypeName = homeLocType.Value,
                    LocationIsDirty = true,
                    State = DefaultState,
                    IsMailing = true,
                    IsLocation = setLocation,
                    ShowCounty = _showCounty
                } );

                gLocations.EditIndex = GroupAddresses.Count - 1;

                confirmExit.Enabled = true;

                BindLocations();
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                if ( ( e.Row.RowState & DataControlRowState.Edit ) == DataControlRowState.Edit )
                {
                    GroupAddressInfo groupAddress = e.Row.DataItem as GroupAddressInfo;
                    if ( groupAddress != null )
                    {
                        var ddlLocType = e.Row.FindControl( "ddlLocType" ) as DropDownList;
                        if ( ddlLocType != null )
                        {
                            ddlLocType.DataSource = _groupType.LocationTypeValues.OrderBy( v => v.Order ).ToList();
                            ddlLocType.DataBind();
                            ddlLocType.SetValue( groupAddress.LocationTypeId );
                        }

                        var acAddress = e.Row.FindControl( "acAddress" ) as AddressControl;
                        if ( acAddress != null )
                        {
                            acAddress.UseStateAbbreviation = true;
                            acAddress.UseCountryAbbreviation = false;
                            acAddress.ShowCounty = _showCounty;
                            acAddress.Country = groupAddress.Country;
                            acAddress.Street1 = groupAddress.Street1;
                            acAddress.Street2 = groupAddress.Street2;
                            acAddress.City = groupAddress.City;
                            acAddress.State = groupAddress.State;
                            acAddress.PostalCode = groupAddress.PostalCode;
                            acAddress.County = groupAddress.County;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowEditing event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        protected void gLocations_RowEditing( object sender, GridViewEditEventArgs e )
        {
            gLocations.EditIndex = e.NewEditIndex;
            BindLocations();
        }

        /// <summary>
        /// Handles the Add event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLocations_Add( object sender, EventArgs e )
        {
            GroupAddresses.Add( new GroupAddressInfo { State = DefaultState, IsMailing = true, ShowCounty = _showCounty } );
            gLocations.EditIndex = GroupAddresses.Count - 1;

            BindLocations();
        }

        /// <summary>
        /// Handles the RowUpdating event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewUpdateEventArgs" /> instance containing the event data.</param>
        protected void gLocations_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {
            var groupAddress = GroupAddresses[e.RowIndex];
            if ( groupAddress != null )
            {
                // was added
                if ( groupAddress.Id < 0 )
                {
                    groupAddress.Id = 0;
                }

                var row = gLocations.Rows[e.RowIndex];
                DropDownList ddlLocType = row.FindControl( "ddlLocType" ) as DropDownList;
                AddressControl acAddress = row.FindControl( "acAddress" ) as AddressControl;
                CheckBox cbMailing = row.FindControl( "cbMailing" ) as CheckBox;
                CheckBox cbLocation = row.FindControl( "cbLocation" ) as CheckBox;

                groupAddress.LocationTypeId = ddlLocType.SelectedValueAsInt() ?? 0;
                groupAddress.LocationTypeName = ddlLocType.SelectedItem != null ? ddlLocType.SelectedItem.Text : string.Empty;
                groupAddress.Street1 = acAddress.Street1;
                groupAddress.Street2 = acAddress.Street2;
                groupAddress.City = acAddress.City;
                groupAddress.State = acAddress.State;
                groupAddress.PostalCode = acAddress.PostalCode;
                groupAddress.Country = acAddress.Country;
                groupAddress.County = acAddress.County;
                groupAddress.IsMailing = cbMailing.Checked;

                // If setting this location to be a map location, unselect all the other locations
                if ( !groupAddress.IsLocation && cbLocation.Checked )
                {
                    foreach ( var otherAddress in GroupAddresses )
                    {
                        otherAddress.IsLocation = false;
                    }
                }

                groupAddress.IsLocation = cbLocation.Checked;
                groupAddress.LocationIsDirty = true;
            }

            gLocations.EditIndex = -1;

            confirmExit.Enabled = true;

            BindLocations();
        }

        /// <summary>
        /// Handles the RowCancelingEdit event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCancelEditEventArgs"/> instance containing the event data.</param>
        protected void gLocations_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            var groupAddress = GroupAddresses[e.RowIndex];

            if ( groupAddress != null && groupAddress.Id < 0 )
            {
                // was added
                GroupAddresses.RemoveAt( e.RowIndex );
            }

            gLocations.EditIndex = -1;

            BindLocations();
        }

        /// <summary>
        /// Handles the RowDelete event of the gLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocation_RowDelete( object sender, RowEventArgs e )
        {
            GroupAddresses.RemoveAt( e.RowIndex );

            confirmExit.Enabled = true;

            gLocations.EditIndex = -1;

            BindLocations();
        }

        /// <summary>
        /// Handles the GridRebind event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLocations_GridRebind( object sender, EventArgs e )
        {
            BindLocations();
        }

        #endregion

        #region Action Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            // if a Location is getting edited, validate and save it
            if ( gLocations.EditIndex >= 0 )
            {
                var row = gLocations.Rows[gLocations.EditIndex];
                AddressControl acAddress = row.FindControl( "acAddress" ) as AddressControl;
                if ( acAddress.IsValid )
                {
                    gLocations_RowUpdating( sender, new GridViewUpdateEventArgs( gLocations.EditIndex ) );
                }
                else
                {
                    // acAddress will render an error message
                    return;
                }
            }

            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                return;
            }

            // confirmation was disabled by btnSave on client-side.  So if returning without a redirect,
            // it should be enabled.  If returning with a redirect, the control won't be updated to reflect
            // confirmation being enabled, so it's ok to enable it here
            confirmExit.Enabled = true;

            if ( Page.IsValid )
            {
                confirmExit.Enabled = true;

                var rockContext = new RockContext();
                
                try
                {
                    rockContext.WrapTransaction( () =>
	                {
	                    var groupService = new GroupService( rockContext );
	                    var groupMemberService = new GroupMemberService( rockContext );
	                    var personService = new PersonService( rockContext );
	                    var historyService = new HistoryService( rockContext );
	
	                    // SAVE GROUP
	                    _group = groupService.Get( _group.Id );
	
	                    _group.Name = tbGroupName.Text;
                        _group.CampusId = cpCampus.SelectedValueAsInt();
                        _group.StatusValueId = dvpGroupStatus.SelectedValueAsId();
	
	                    rockContext.SaveChanges();
	
	                    // SAVE GROUP MEMBERS
	                    var recordStatusInactiveId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
	                    var reasonStatusReasonDeceasedId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED ) ).Id;
	                    int? recordStatusValueID = dvpRecordStatus.SelectedValueAsInt();
	                    int? reasonValueId = dvpReason.SelectedValueAsInt();
	                    var newGroups = new List<Group>();
	
	                    foreach ( var groupMemberInfo in GroupMembers )
	                    {
	                        var role = _groupType.Roles.Where( r => r.Guid.Equals( groupMemberInfo.RoleGuid ) ).FirstOrDefault();
	                        if ( role == null )
	                        {
	                            role = _groupType.Roles.FirstOrDefault();
	                        }
	
	                        bool isAdult = role != null && role.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );
	
	                        // People added to group (new or from other group )
	                        if ( !groupMemberInfo.ExistingGroupMember )
	                        {
	                            Person person = null;
	                            if ( groupMemberInfo.PersonId == -1 )
	                            {
	                                person = new Person();
	
	                                person.TitleValueId = groupMemberInfo.TitleValueId;
	                                person.FirstName = groupMemberInfo.FirstName;
	                                person.NickName = groupMemberInfo.NickName;
	                                person.LastName = groupMemberInfo.LastName;
	                                person.SuffixValueId = groupMemberInfo.SuffixValueId;
	                                person.Gender = groupMemberInfo.Gender;
	
	                                DateTime? birthdate = groupMemberInfo.BirthDate;
	                                if ( birthdate.HasValue )
	                                {
	                                    // If setting a future birthdate, subtract a century until birthdate is not greater than today.
	                                    var today = RockDateTime.Today;
	                                    while ( birthdate.Value.CompareTo( today ) > 0 )
	                                    {
	                                        birthdate = birthdate.Value.AddYears( -100 );
	                                    }
	                                }
	
	                                person.SetBirthDate( birthdate );
	
	                                person.MaritalStatusValueId = groupMemberInfo.MaritalStatusValueId;
	                                person.GradeOffset = groupMemberInfo.GradeOffset;
	                                person.ConnectionStatusValueId = groupMemberInfo.ConnectionStatusValueId;
	                                person.Email = groupMemberInfo.Email;
	                                if ( groupMemberInfo.PhoneNumbers != null && groupMemberInfo.PhoneNumbers.Any() )
	                                {
	                                    foreach( var pnInfo in groupMemberInfo.PhoneNumbers )
	                                    {
	                                        var phoneNumber = new PhoneNumber();
	                                        phoneNumber.NumberTypeValueId = pnInfo.PhoneTypeId;
	                                        phoneNumber.Number = pnInfo.Number;
	                                        person.PhoneNumbers.Add( phoneNumber );
	                                    }
	                                }
	                                if ( isAdult )
	                                {
	                                    person.GivingGroupId = _group.Id;
	                                }
	
	                                person.IsEmailActive = true;
	                                person.EmailPreference = EmailPreference.EmailAllowed;
	                                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
	                            }
	                            else
	                            {
	                                person = personService.Get( groupMemberInfo.PersonId );
	                            }
	
	                            if ( person == null )
	                            {
	                                // shouldn't happen
	                                return;
	                            }

                                if ( _isFamilyGroupType && recordStatusValueID.HasValue && recordStatusValueID.Value > 0 && ( person.RecordStatusReasonValueId ?? 0 ) != reasonStatusReasonDeceasedId )
                                {
                                    if ( recordStatusValueID.HasValue && recordStatusValueID.Value != 0 )
                                    {
                                        person.RecordStatusValueId = recordStatusValueID.Value;
                                    }

                                    if ( reasonValueId.HasValue && reasonValueId.Value != 0 )
                                    {
                                        person.RecordStatusReasonValueId = reasonValueId.Value;
                                    }
                                }
	
	                            PersonService.AddPersonToGroup( person, person.Id == 0, _group.Id, role.Id, rockContext );
	                            groupMemberInfo.PersonId = person.Id;
	                        }
	                        else
	                        {
	                            // existing group members
	                            var groupMember = groupMemberService.Queryable( "Person", true ).Where( m =>
	                                m.PersonId == groupMemberInfo.PersonId &&
	                                m.Group.GroupTypeId == _groupType.Id &&
	                                m.GroupId == _group.Id ).FirstOrDefault();
	
	                            if ( groupMember != null )
	                            {
	                                if ( groupMemberInfo.Removed )
	                                {
	                                    if ( !groupMemberInfo.IsInOtherGroups )
	                                    {
	                                        // Family member was removed and should be created in their own new family
	                                        var newGroup = new Group();
	                                        newGroup.Name = groupMemberInfo.LastName + " " + _groupType.Name;
	                                        newGroup.GroupTypeId = _groupType.Id;
	                                        newGroup.CampusId = _group.CampusId;
	                                        groupService.Add( newGroup );
	                                        rockContext.SaveChanges();
	
	                                        // If person's previous giving group was this family, set it to their new family id
	                                        if ( _isFamilyGroupType && groupMember.Person.GivingGroup != null && groupMember.Person.GivingGroupId == _group.Id )
	                                        {
	                                            groupMember.Person.GivingGroupId = newGroup.Id;
	                                        }
	
	                                        groupMember.Group = newGroup;
	                                        
	                                        // If this person is 18 or older, create them as an Adult in their new group
	                                        if ((groupMember.Person.Age ?? 0) >= 18)
	                                        {
	                                            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
	                                            groupMember.GroupRoleId = familyGroupType.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
	                                        }
	
	                                        rockContext.SaveChanges();

	                                        newGroups.Add( newGroup );

	                                    }
	                                    else
	                                    {
	                                        groupMemberService.Delete( groupMember );
	                                        rockContext.SaveChanges();
	                                    }
	                                }
	                                else
	                                {
	                                    // Existing member was not removed
	                                    if ( role != null )
	                                    {
	                                        groupMember.GroupRoleId = role.Id;

                                            // Only change a person's record status if they were not previously deceased (#1887).
                                            if ( _isFamilyGroupType && recordStatusValueID.HasValue && recordStatusValueID.Value > 0 && ( groupMember.Person.RecordStatusReasonValueId ?? 0 ) != reasonStatusReasonDeceasedId )
                                            {
                                                if ( recordStatusValueID.HasValue && recordStatusValueID.Value != 0 )
                                                {
                                                    groupMember.Person.RecordStatusValueId = recordStatusValueID.Value;
                                                }

                                                if ( reasonValueId.HasValue && reasonValueId.Value != 0 )
                                                {
                                                    groupMember.Person.RecordStatusReasonValueId = reasonValueId.Value;
                                                }
                                            }

                                            if ( !groupMember.IsValidGroupMember( rockContext ) )
                                            {
                                                throw new GroupMemberValidationException( groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" ) );
                                            }

	                                        rockContext.SaveChanges();
	                                    }
	                                }
	                            }
	                        }
	
	                        // Remove anyone that was moved from another family
	                        if ( groupMemberInfo.RemoveFromOtherGroups )
	                        {
	                            PersonService.RemovePersonFromOtherFamilies( _group.Id, groupMemberInfo.PersonId, rockContext );
	                        }
	                    }
	
	                    // Now check if family group should be marked inactive or active
	                    if ( _isFamilyGroupType )
	                    {
	                        // Are there any members of the family who are NOT inactive?
	                        // If not, mark the whole family inactive.
	                        if ( !_group.Members.Where( m => m.Person.RecordStatusValueId != recordStatusInactiveId ).Any() )
	                        {
	                            _group.IsActive = false;
	                        }
	                        else
	                        {
	                            _group.IsActive = true;
	                        }
	                    }
	
	                    // SAVE LOCATIONS
	                    var groupLocationService = new GroupLocationService( rockContext );
	
	                    // delete any group locations that were removed
	                    var remainingLocationIds = GroupAddresses.Where( a => a.Id > 0 ).Select( a => a.Id ).ToList();
	                    foreach ( var removedLocation in groupLocationService.Queryable( "GroupLocationTypeValue,Location" )
	                        .Where( l => l.GroupId == _group.Id &&
	                            !remainingLocationIds.Contains( l.Id ) ) )
	                    {
	                        groupLocationService.Delete( removedLocation );
	                    }
	
	                    rockContext.SaveChanges();
	
	                    foreach ( var groupAddressInfo in GroupAddresses.Where( a => a.Id >= 0 ) )
	                    {
	                        Location updatedAddress = null;
	                        if ( groupAddressInfo.LocationIsDirty )
	                        {
	                            updatedAddress = new LocationService( rockContext ).Get( groupAddressInfo.Street1, groupAddressInfo.Street2, groupAddressInfo.City, groupAddressInfo.State, groupAddressInfo.PostalCode, groupAddressInfo.Country );
                                if( _showCounty )
                                {
                                    updatedAddress.County = groupAddressInfo.County;
                                }
                            }
	
	                        GroupLocation groupLocation = null;
	                        if ( groupAddressInfo.Id > 0 )
	                        {
	                            groupLocation = groupLocationService.Get( groupAddressInfo.Id );
	                        }
	
	                        if ( groupLocation == null )
	                        {
	                            groupLocation = new GroupLocation();
	                            groupLocation.GroupId = _group.Id;
	                            groupLocationService.Add( groupLocation );
	                        }
	
	                        groupLocation.GroupLocationTypeValueId = groupAddressInfo.LocationTypeId;
	                        groupLocation.IsMailingLocation = groupAddressInfo.IsMailing;
	                        groupLocation.IsMappedLocation = groupAddressInfo.IsLocation;
	
	                        if ( updatedAddress != null )
	                        {
	                            groupLocation.Location = updatedAddress;
	                        }
	
	                        rockContext.SaveChanges();
	
	                        // Add the same locations to any new families created by removing an existing family member
	                        if ( newGroups.Any() )
	                        {
	                            // reload grouplocation for access to child properties
	                            groupLocation = groupLocationService.Get( groupLocation.Id );
	                            foreach ( var newGroup in newGroups )
	                            {
	                                var newGroupLocation = new GroupLocation();
	                                newGroupLocation.GroupId = newGroup.Id;
	                                newGroupLocation.LocationId = groupLocation.LocationId;
	                                newGroupLocation.GroupLocationTypeValueId = groupLocation.GroupLocationTypeValueId;
	                                newGroupLocation.IsMailingLocation = groupLocation.IsMailingLocation;
	                                newGroupLocation.IsMappedLocation = groupLocation.IsMappedLocation;
	                                groupLocationService.Add( newGroupLocation );
	                            }
	
	                            rockContext.SaveChanges();
	                        }
	                    }
	
	                    _group.LoadAttributes();

                        Dictionary<string, AttributeValueCache> originalGroupAttributes = new Dictionary<string, AttributeValueCache>();
                        foreach ( var attribute in _group.AttributeValues )
                        {
                            originalGroupAttributes.Add( attribute.Key, attribute.Value );
                        }

                        Rock.Attribute.Helper.GetEditValues( phGroupAttributes, _group );

	                    _group.SaveAttributeValues( rockContext );
	
	                    Response.Redirect( string.Format( "~/Person/{0}", Person.Id ), false );
	
	                } );
				}
                catch ( GroupMemberValidationException gmvex )
                {
                    cvGroupMember.IsValid = false;
                    cvGroupMember.ErrorMessage = gmvex.Message;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            Response.Redirect( string.Format( "~/Person/{0}", Person.Id ), false );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var groupId = _group.Id;
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMembers = groupMemberService.GetByGroupId( groupId, true );

            if ( !_isFamilyGroupType || groupMembers.Count() == 1 )
            {
                var groupMember = groupMembers.FirstOrDefault();

                // If the person's giving group id is this group, change their giving group id to null
                if ( groupMember.Person.GivingGroupId == groupMember.GroupId )
                {
                    var personService = new PersonService( rockContext );
                    var person = personService.Get( groupMember.PersonId );
                    if ( person != null )
                    {
                        person.GivingGroupId = null;
                        rockContext.SaveChanges();
                    }
                }

                groupMemberService.Delete( groupMember );
                rockContext.SaveChanges();
            }

            var groupService = new GroupService( rockContext );

            // get the family that we want to delete (if it has no members )
            var group = groupService.Queryable()
                .Where( g =>
                    g.Id == groupId &&
                    !g.Members.Any() )
                .FirstOrDefault();

            if ( group != null )
            {
                groupService.Delete( group );
                rockContext.SaveChanges();
            }

            Response.Redirect( string.Format( "~/Person/{0}", Person.Id ), false );
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitializeValues()
        {

        }

        /// <summary>
        /// Sets the active tab.
        /// </summary>
        private void SetActiveTab()
        {
            if ( hfActiveTab.Value == "Existing" )
            {
                liNewPerson.RemoveCssClass( "active" );
                divNewPerson.RemoveCssClass( "active" );
                liExistingPerson.AddCssClass( "active" );
                divExistingPerson.AddCssClass( "active" );
            }
            else
            {
                liNewPerson.AddCssClass( "active" );
                divNewPerson.AddCssClass( "active" );
                liExistingPerson.RemoveCssClass( "active" );
                divExistingPerson.RemoveCssClass( "active" );
            }
        }

        /// <summary>
        /// Binds the members.
        /// </summary>
        private void BindMembers()
        {
            int i = 0;
            GroupMembers.ForEach( m => m.Index = i++ );

            // Do not allow deleting of person's family group unless they are in multiple families, and they are the only one in this family
            if ( _isFamilyGroupType )
            {
                btnDelete.Visible = false;
                if ( GroupMembers.Count <= 1 )
                {
                    var groupMemberInfo = GroupMembers.FirstOrDefault();
                    if ( groupMemberInfo != null )
                    {
                        if ( groupMemberInfo.IsInOtherGroups )
                        {
                            // person is only person in the current group, and they are also in at least one other group, so let them delete this group
                            btnDelete.Visible = true;
                        }
                    }
                    else
                    {
                        // somehow there are no people in this group at all, so let them delete this group
                        btnDelete.Visible = true;
                    }
                }
            }
            else
            {
                btnDelete.Visible = true;
            }

            lvMembers.DataSource = GetMembersOrdered();
            lvMembers.DataBind();
        }

        /// <summary>
        /// Binds the locations.
        /// </summary>
        private void BindLocations()
        {
            int homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;

            // If there are not any addresses with a Map Location, set the first home location to be a mapped location
            if ( !GroupAddresses.Any( l => l.IsLocation == true ) )
            {
                var firstHomeAddress = GroupAddresses.Where( l => l.LocationTypeId == homeLocationTypeId ).FirstOrDefault();
                if ( firstHomeAddress != null )
                {
                    firstHomeAddress.IsLocation = true;
                }
            }

            gLocations.DataSource = GroupAddresses;
            gLocations.DataBind();
        }

        private void BuildAttributes( bool setValues )
        {
            if ( _group != null )
            {
                phGroupAttributes.Controls.Clear();
                _group.LoadAttributes();

                var attributes = _group.GetAuthorizedAttributes( Authorization.EDIT, CurrentPerson );
                if ( attributes.Any() )
                {
                    pnlAttributes.Visible = true;
                    Helper.AddEditControls( string.Empty, attributes.OrderBy( a => a.Value.Order ).Select( a => a.Key ).ToList(),
                        _group, phGroupAttributes, BlockValidationGroup, setValues, new List<string>(), 3);
                }
                else
                {
                    pnlAttributes.Visible = false;
                }
            }
        }

        /// <summary>
        /// Gets the members ordered.
        /// </summary>
        /// <returns></returns>
        private List<GroupMemberInfo> GetMembersOrdered()
        {
            var orderedMembers = new List<GroupMemberInfo>();

            if ( _isFamilyGroupType )
            {
                // Add adult males
                orderedMembers.AddRange( GroupMembers
                    .Where( m =>
                        !m.Removed &&
                        m.RoleGuid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                        m.Gender == Gender.Male )
                    .OrderByDescending( m => m.Age ) );

                // Add adult females
                orderedMembers.AddRange( GroupMembers
                    .Where( m =>
                        !m.Removed &&
                        m.RoleGuid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                        m.Gender != Gender.Male )
                    .OrderByDescending( m => m.Age ) );

                // Add non-adults
                orderedMembers.AddRange( GroupMembers
                    .Where( m =>
                        !m.Removed &&
                        !m.RoleGuid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                    .OrderByDescending( m => m.Age ) );
            }
            else
            {
                orderedMembers.AddRange( GroupMembers
                    .Where( m =>
                        !m.Removed &&
                        m.IsLeader )
                    .OrderBy( m => m.LastName )
                    .ThenBy( m => m.NickName ) );

                orderedMembers.AddRange( GroupMembers
                    .Where( m =>
                        !m.Removed &&
                        !m.IsLeader )
                    .OrderBy( m => m.LastName )
                    .ThenBy( m => m.NickName ) );
            }
            return orderedMembers;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class GroupMemberInfo
    {
        public int Index { get; set; }

        public int PersonId { get; set; }

        public bool ExistingGroupMember { get; set; }  // Is this person part of the original group 

        public bool Removed { get; set; } // Was an existing person removed from the group (to their own group)

        public bool RemoveFromOtherGroups { get; set; } // When adding an existing person, should they be removed from other groups of same type

        public bool IsInOtherGroups { get; set; } // This person is also a member of another group

        public int? TitleValueId { get; set; }

        public string FirstName { get; set; }

        public string NickName { get; set; }

        public string LastName { get; set; }

        public int? SuffixValueId { get; set; }

        public int? RecordTypeValueId { get; private set; }

        public Guid? RecordTypeValueGuid
        {
            get
            {
                DefinedValueCache recordTypeValue = null;
                if ( RecordTypeValueId != null )
                {
                    recordTypeValue = DefinedValueCache.Get( RecordTypeValueId.Value );
                }

                if ( recordTypeValue != null )
                {
                    return recordTypeValue.Guid;
                }
                else
                {
                    return null;
                }
            }
        }

        public AgeClassification AgeClassification { get; private set; }

        public Gender Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public int? GradeOffset { get; set; }

        public Guid RoleGuid { get; set; }

        public bool IsLeader { get; set; }

        public string RoleName { get; set; }

        public int? MaritalStatusValueId { get; set; }

        public int? PhotoId { get; set; }

        public int? ConnectionStatusValueId { get; set; }

        public string Email { get; set; }

        public List<GroupMemberPhoneInfo> PhoneNumbers { get; set; }

        public int? Age
        {
            get
            {
                if ( BirthDate.HasValue )
                {
                    return BirthDate.Age();
                }

                return null;
            }
        }

        public GroupMemberInfo( GroupMember groupMember, bool existingGroupMember )
        {
            if ( groupMember != null )
            {
                SetValuesFromPerson( groupMember.Person );

                if ( groupMember.GroupRole != null )
                {
                    RoleGuid = groupMember.GroupRole.Guid;
                    RoleName = groupMember.GroupRole.Name;
                    IsLeader = groupMember.GroupRole.IsLeader;
                }
            }

            ExistingGroupMember = existingGroupMember;
            Removed = false;
        }

        public GroupMemberInfo()
        {
            PersonId = -1;
            ExistingGroupMember = false;
            Removed = false;
            RemoveFromOtherGroups = false;
        }

        public void SetValuesFromPerson( Person person )
        {
            if ( person != null )
            {
                PersonId = person.Id;
                TitleValueId = person.TitleValueId;
                FirstName = person.FirstName;
                NickName = person.NickName;
                LastName = person.LastName;
                SuffixValueId = person.SuffixValueId;
                RecordTypeValueId = person.RecordTypeValueId;
                AgeClassification = person.AgeClassification;
                Gender = person.Gender;
                BirthDate = person.BirthDate;
                GradeOffset = person.GradeOffset;
                MaritalStatusValueId = person.MaritalStatusValueId;
                PhotoId = person.PhotoId;
                ConnectionStatusValueId = person.ConnectionStatusValueId;
                Email = person.Email;
                PhoneNumbers = new List<GroupMemberPhoneInfo>();
                foreach( var pn in person.PhoneNumbers )
                {
                    var phoneNumberInfo = new GroupMemberPhoneInfo();
                    phoneNumberInfo.Id = pn.Id;
                    phoneNumberInfo.PhoneTypeId = pn.NumberTypeValueId;
                    phoneNumberInfo.Number = pn.Number;
                    PhoneNumbers.Add( phoneNumberInfo );
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class GroupMemberPhoneInfo
    {
        public int Id { get; set; }
        public int? PhoneTypeId { get; set; }
        public string Number { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class GroupAddressInfo
    {
        public int Id { get; set; }

        public int LocationTypeId { get; set; }

        public string LocationTypeName { get; set; }

        public int LocationId { get; set; }

        public bool LocationIsDirty { get; set; }

        public string Street1 { get; set; }

        public string Street2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string County { get; set; }

        public bool IsMailing { get; set; }

        public bool IsLocation { get; set; }

        public bool ShowCounty { get; set; }

        public GroupAddressInfo( GroupLocation groupLocation, bool showCounty )
        {
            LocationIsDirty = false;
            if ( groupLocation != null )
            {
                Id = groupLocation.Id;

                if ( groupLocation.GroupLocationTypeValue != null )
                {
                    LocationTypeId = groupLocation.GroupLocationTypeValue.Id;
                    LocationTypeName = groupLocation.GroupLocationTypeValue.Value;
                }

                if ( groupLocation.Location != null )
                {
                    LocationId = groupLocation.Location.Id;
                    Street1 = groupLocation.Location.Street1;
                    Street2 = groupLocation.Location.Street2;
                    City = groupLocation.Location.City;
                    State = groupLocation.Location.State;
                    PostalCode = groupLocation.Location.PostalCode;
                    Country = groupLocation.Location.Country;
                    County = groupLocation.Location.County;
                }

                IsMailing = groupLocation.IsMailingLocation;
                IsLocation = groupLocation.IsMappedLocation;
                ShowCounty = showCounty;
            }
        }

        public GroupAddressInfo()
        {
            Id = -1; // Adding
            LocationIsDirty = true;

            string orgLocGuid = GlobalAttributesCache.Value( "OrganizationAddress" );
        }

        public string FormattedAddress
        {
            get
            {
                string result = string.Empty;
                if ( ShowCounty )
                {
                    result = string.Format(
                        "{0}{1}{2}{3}, {4} {5}",
                        this.Street1 + Environment.NewLine,
                        this.Street2 + Environment.NewLine,
                        this.City,
                        this.County.IsNotNullOrWhiteSpace() ? ", " + this.County : "",
                        this.State,
                        this.PostalCode
                        ).ReplaceWhileExists( "  ", " " );

                    // If the block is configured to display county, do not reformat the address using the country's address format as it does not account for county
                }
                else
                {
                    var countryValue = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES ) ).GetDefinedValueFromValue( this.Country );
                    if ( countryValue != null )
                    {
                        string format = countryValue.GetAttributeValue( "AddressFormat" );
                        if ( !string.IsNullOrWhiteSpace( format ) )
                        {
                            var mergeFields = new Dictionary<string, object>();
                            mergeFields.Add( "Street1", Street1 );
                            mergeFields.Add( "Street2", Street2 );
                            mergeFields.Add( "City", City );
                            mergeFields.Add( "State", State );
                            mergeFields.Add( "PostalCode", PostalCode );
                            mergeFields.Add( "Country", countryValue.Description );

                            result = format.ResolveMergeFields( mergeFields );
                        }
                    }
                }

                while ( result.Contains( Environment.NewLine + Environment.NewLine ) )
                {
                    result = result.Replace( Environment.NewLine + Environment.NewLine, Environment.NewLine );
                }

                while ( result.Contains( "\x0A\x0A" ) )
                {
                    result = result.Replace( "\x0A\x0A", "\x0A" );
                }

                if ( string.IsNullOrWhiteSpace( result.Replace( ",", string.Empty ) ) )
                {
                    return string.Empty;
                }

                return result.ConvertCrLfToHtmlBr();
            }
        }
    }
}

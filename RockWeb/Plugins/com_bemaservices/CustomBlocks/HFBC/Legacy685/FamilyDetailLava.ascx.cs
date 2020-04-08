// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_hfbc.Legacy685
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Family Detail Lava" )]
    [Category( "org_hfbc > Legacy 685" )]
    [Description( "Block used to view members of a Legacy 685 family" )]

    [LinkedPage( "Communication Page", "The communication page to use for sending emails to the group members.", true, "", "", 4 )]
    [AttributeCategoryField( "Category", "The Attribute Category to edit attributes from", false, "Rock.Model.Group", true, "", "", 0 )]

    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupDetail.lava' %}", "", 8 )]
    [BooleanField( "Enable Location Edit", "Enables changing locations when editing a group.", false, "", 9 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 10 )]
    public partial class FamilyDetailLava : Rock.Web.UI.RockBlock
    {
        #region Fields
        private List<GroupAddressInfo> GroupAddresses
        {
            get { return ViewState["GroupAddresses"] as List<GroupAddressInfo>; }
            set { ViewState["GroupAddresses"] = value; }
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

        // used for private variables
        private int _groupId = 0;
        private const string MEMBER_LOCATION_TAB_TITLE = "Member Location";
        private const string OTHER_LOCATION_TAB_TITLE = "Other Location";

        private readonly List<string> _tabs = new List<string> { MEMBER_LOCATION_TAB_TITLE, OTHER_LOCATION_TAB_TITLE };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editing group.
        /// used for public / protected properties
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is editing group; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditingGroup
        {
            get
            {
                return ViewState["IsEditingGroup"] as bool? ?? false;
            }

            set
            {
                ViewState["IsEditingGroup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editing group member.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is editing group member; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditingGroupMember
        {
            get
            {
                return ViewState["IsEditingGroupMember"] as bool? ?? false;
            }

            set
            {
                ViewState["IsEditingGroupMember"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current group member identifier.
        /// </summary>
        /// <value>
        /// The current group member identifier.
        /// </value>
        public int CurrentGroupMemberId
        {
            get
            {
                int groupMemberId = 0;
                int.TryParse( ViewState["CurrentGroupMemberId"].ToString(), out groupMemberId );
                return groupMemberId;
            }

            set
            {
                ViewState["CurrentGroupMemberId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the location type tab.
        /// </summary>
        /// <value>
        /// The location type tab.
        /// </value>
        private string LocationTypeTab
        {
            get
            {
                object currentProperty = ViewState["LocationTypeTab"];
                return currentProperty != null ? currentProperty.ToString() : MEMBER_LOCATION_TAB_TITLE;
            }

            set
            {
                ViewState["LocationTypeTab"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gLocations.DataKeyNames = new string[] { "Id" };
            gLocations.RowDataBound += gLocations_RowDataBound;
            gLocations.RowEditing += gLocations_RowEditing;
            gLocations.RowUpdating += gLocations_RowUpdating;
            gLocations.RowCancelingEdit += gLocations_RowCancelingEdit;
            gLocations.Actions.ShowAdd = true;
            gLocations.Actions.AddClick += gLocations_Add;
            gLocations.IsDeleteEnabled = false;
            gLocations.GridRebind += gLocations_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // get the group id
            if ( GetGroupId().HasValue )
            {
                _groupId = GetGroupId().Value;
            }

            var campusi = CampusCache.All();
            cpCampus.Campuses = campusi;
            cpCampus.Visible = campusi.Any();
            cpCampus.Required = true;

            ddlRecordStatus.Visible = true;
            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid() ), true );
            ddlReason.BindToDefinedType( DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() ), true );

        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( IsEditingGroup == true )
            {
                Group group = new GroupService( new RockContext() ).Get( _groupId );
                group.LoadAttributes();

                phAttributes.Controls.Clear();
                string categoryGuid = GetAttributeValue( "Category" );
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( categoryGuid, out guid ) )
                {
                    var category = CategoryCache.Get( guid );
                    if ( category != null )
                    {
                        List<string> excludeList = new List<string>();
                        var allowedAttributeIds = new AttributeService( new RockContext() ).GetByCategoryId( category.Id ).Select( a => a.Id ).ToList();
                        excludeList.AddRange( group.Attributes.Where( a => !allowedAttributeIds.Contains( a.Value.Id ) ).Select( a => a.Value.Name ) );
                        Rock.Attribute.Helper.AddEditControls( group, phAttributes, false, BlockValidationGroup, excludeList, false );

                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RouteAction();

            // add a navigate event to cature when someone presses the back button
            var sm = ScriptManager.GetCurrent( Page );
            sm.EnableSecureHistoryState = false;
            sm.Navigate += sm_Navigate;
        }

        /// <summary>
        /// Handles the Navigate event of the sm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        public void sm_Navigate( object sender, HistoryEventArgs e )
        {
            // show the view mode
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = true;
            DisplayViewGroup();
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RouteAction();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveGroup_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );

            Group group = groupService.Get( _groupId );

            if ( group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                var groupChanges = new List<string>();

                // set attributes
                group.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, group );

                int? campusId = cpCampus.SelectedValueAsInt();
                if ( group.CampusId != campusId )
                {
                    /*
                    History.EvaluateChange(
                        groupChanges,
                        "Campus",
                        group.CampusId.HasValue ? CampusCache.Read( group.CampusId.Value ).Name : string.Empty,
                        campusId.HasValue ? CampusCache.Read( campusId.Value ).Name : string.Empty );
                    */

                    group.CampusId = campusId;
                }

                // SAVE GROUP MEMBERS
                int? recordStatusValueID = ddlRecordStatus.SelectedValueAsInt();
                int? reasonValueId = ddlReason.SelectedValueAsInt();

                foreach ( var groupMember in group.Members )
                {
                    var memberChanges = new List<string>();
                    var demographicChanges = new List<string>();

                    if ( groupMember != null )
                    {
                        if ( recordStatusValueID > 0 )
                        {
                            //History.EvaluateChange( demographicChanges, "Record Status", DefinedValueCache.GetName( groupMember.Person.RecordStatusValueId ), DefinedValueCache.GetName( recordStatusValueID ) );
                            groupMember.Person.RecordStatusValueId = recordStatusValueID;

                            //History.EvaluateChange( demographicChanges, "Record Status Reason", DefinedValueCache.GetName( groupMember.Person.RecordStatusReasonValueId ), DefinedValueCache.GetName( reasonValueId ) );
                            groupMember.Person.RecordStatusReasonValueId = reasonValueId;
                        }
                    }

                    //HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), groupMember.PersonId, demographicChanges );
                }

                // SAVE LOCATIONS
                var groupLocationService = new GroupLocationService( rockContext );

                // delete any group locations that were removed
                var remainingLocationIds = GroupAddresses.Where( a => a.Id > 0 ).Select( a => a.Id ).ToList();
                foreach ( var removedLocation in groupLocationService.Queryable( "GroupLocationTypeValue,Location" )
                    .Where( l => l.GroupId == group.Id &&
                        !remainingLocationIds.Contains( l.Id ) ) )
                {
                    /*
                    History.EvaluateChange( groupChanges,
                        ( removedLocation.GroupLocationTypeValue != null ? removedLocation.GroupLocationTypeValue.Value : "Unknown" ) + " Location",
                        removedLocation.Location.ToString(), string.Empty );
                    */
                    groupLocationService.Delete( removedLocation );

                }

                // Saving Group Name
                group.Name = tbGroupName.Text;

                rockContext.SaveChanges();

                foreach ( var groupAddressInfo in GroupAddresses.Where( a => a.Id >= 0 ) )
                {
                    Location updatedAddress = null;
                    if ( groupAddressInfo.LocationIsDirty )
                    {
                        updatedAddress = new LocationService( rockContext ).Get( groupAddressInfo.Street1, groupAddressInfo.Street2, groupAddressInfo.City, groupAddressInfo.State, groupAddressInfo.PostalCode, groupAddressInfo.Country );
                    }

                    GroupLocation groupLocation = null;
                    if ( groupAddressInfo.Id > 0 )
                    {
                        groupLocation = groupLocationService.Get( groupAddressInfo.Id );
                    }

                    if ( groupLocation == null )
                    {
                        groupLocation = new GroupLocation();
                        groupLocation.GroupId = group.Id;
                        groupLocationService.Add( groupLocation );
                    }

                    /*
                    History.EvaluateChange(
                        groupChanges,
                        "Location Type",
                        groupLocation.GroupLocationTypeValueId.HasValue ? DefinedValueCache.Read( groupLocation.GroupLocationTypeValueId.Value ).Value : string.Empty,
                        groupAddressInfo.LocationTypeName );
                    */
                    groupLocation.GroupLocationTypeValueId = groupAddressInfo.LocationTypeId;

                    /*History.EvaluateChange(
                        groupChanges,
                        groupAddressInfo.LocationTypeName + " Is Mailing",
                        groupLocation.IsMailingLocation.ToString(),
                        groupAddressInfo.IsMailing.ToString() );
                    */
                    groupLocation.IsMailingLocation = groupAddressInfo.IsMailing;

                    /*
                    History.EvaluateChange(
                        groupChanges,
                        groupAddressInfo.LocationTypeName + " Is Map Location",
                        groupLocation.IsMappedLocation.ToString(),
                        groupAddressInfo.IsLocation.ToString() );
                    */
                    groupLocation.IsMappedLocation = groupAddressInfo.IsLocation;

                    if ( updatedAddress != null )
                    {
                        /*
                        History.EvaluateChange(
                            groupChanges,
                            groupAddressInfo.LocationTypeName + " Location",
                            groupLocation.Location != null ? groupLocation.Location.ToString() : string.Empty,
                            updatedAddress.ToString() );
                        */
                        groupLocation.Location = updatedAddress;
                    }

                    rockContext.SaveChanges();
                }

                rockContext.SaveChanges();
                group.SaveAttributeValues( rockContext );

                foreach ( var fm in group.Members )
                {
                    /*
                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                        fm.PersonId,
                        groupChanges,
                        group.Name,
                        typeof( Group ),
                        group.Id );
                    */
                }
            }

            this.IsEditingGroup = false;

            // reload the group info
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = true;

            var queryString = new Dictionary<string, string>();
            if ( PageParameter( "GroupId" ).AsIntegerOrNull().HasValue )
            {
                queryString.Add( "GroupId", PageParameter( "GroupId" ) );
            }

            if ( PageParameter( "PersonId" ).AsIntegerOrNull().HasValue )
            {
                queryString.Add( "PersonId", PageParameter( "PersonId" ) );
            }

            NavigateToCurrentPage( queryString );
        }

        /// <summary>
        /// Handles the Click event of the lbCancelGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelGroup_Click( object sender, EventArgs e )
        {
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = true;
            this.IsEditingGroup = false;

            var queryString = new Dictionary<string, string>();
            if ( PageParameter( "GroupId" ).AsIntegerOrNull().HasValue )
            {
                queryString.Add( "GroupId", PageParameter( "GroupId" ) );
            }

            if ( PageParameter( "PersonId" ).AsIntegerOrNull().HasValue )
            {
                queryString.Add( "PersonId", PageParameter( "PersonId" ) );
            }
            NavigateToCurrentPage( queryString );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRecordStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlRecordStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlReason.Visible = ddlRecordStatus.SelectedValueAsInt() == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
        }

        #region Location Events

        /// <summary>
        /// Handles the Click event of the lbMoved control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMoved_Click( object sender, EventArgs e )
        {
            var groupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var homeLocType = groupType.LocationTypeValues.FirstOrDefault( v => v.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ) );
            var prevLocType = groupType.LocationTypeValues.FirstOrDefault( v => v.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() ) );

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
                    IsLocation = setLocation
                } );

                gLocations.EditIndex = GroupAddresses.Count - 1;

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
                            var groupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

                            ddlLocType.DataSource = groupType.LocationTypeValues.OrderBy( v => v.Order ).ToList();
                            ddlLocType.DataBind();
                            ddlLocType.SetValue( groupAddress.LocationTypeId );
                        }

                        var acAddress = e.Row.FindControl( "acAddress" ) as AddressControl;
                        if ( acAddress != null )
                        {
                            acAddress.UseStateAbbreviation = true;
                            acAddress.UseCountryAbbreviation = false;
                            acAddress.Country = groupAddress.Country;
                            acAddress.Street1 = groupAddress.Street1;
                            acAddress.Street2 = groupAddress.Street2;
                            acAddress.City = groupAddress.City;
                            acAddress.State = groupAddress.State;
                            acAddress.PostalCode = groupAddress.PostalCode;
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
            GroupAddresses.Add( new GroupAddressInfo { State = DefaultState, IsMailing = true } );
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
                groupAddress.IsMailing = cbMailing.Checked;

                // If setting this location to be a map location, unselect all the other loctions
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

        #endregion
        #endregion

        #region Methods

        private int? GetGroupId( RockContext rockContext = null )
        {
            int? groupId = null;

            groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
            if ( !groupId.HasValue )
            {
                var personId = PageParameter( "PersonId" ).AsIntegerOrNull();

                if ( personId != null )
                {
                    if ( rockContext == null )
                    {
                        rockContext = new RockContext();
                    }

                    var person = new PersonService( rockContext ).Get( personId.Value );
                    if ( person != null )
                    {
                        groupId = person.GetFamily().Id;
                    }
                }
            }

            return groupId;
        }

        /// <summary>
        /// Route the request to the correct panel
        /// </summary>
        private void RouteAction()
        {
            var sm = ScriptManager.GetCurrent( Page );

            if ( Request.Form["__EVENTARGUMENT"] != null )
            {
                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];

                    int argument = 0;
                    int.TryParse( parameters, out argument );

                    switch ( action )
                    {
                        case "EditGroup":
                            pnlGroupEdit.Visible = true;
                            pnlGroupView.Visible = false;
                            DisplayEditGroup();
                            sm.AddHistoryPoint( "Action", "EditGroup" );
                            break;

                        case "FollowButton":
                            FollowPreferredContact();
                            pnlGroupEdit.Visible = false;
                            pnlGroupView.Visible = true;
                            DisplayViewGroup();
                            break;
                    }
                }
            }
            else
            {
                pnlGroupEdit.Visible = false;
                pnlGroupView.Visible = true;
                DisplayViewGroup();
            }
        }

        private void FollowPreferredContact()
        {
            // Get the personAlias entity type
            var personAliasEntityType = EntityTypeCache.Get( typeof( Rock.Model.PersonAlias ) );

            // If we have a valid current person and items were selected
            if ( personAliasEntityType != null && CurrentPersonAliasId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupId = GetGroupId( rockContext );
                    if ( groupId != null )
                    {
                        var group = new GroupService( rockContext ).Get( groupId.Value );
                        if ( group != null )
                        {
                            Person preferredContact = GetPreferredContact( group );
                            if ( preferredContact != null )
                            {
                                // Find any of the selected person alias ids that current person is already following
                                var followingService = new FollowingService( rockContext );
                                var alreadyFollowing = followingService
                                    .Queryable()
                                    .Where( f =>
                                        f.EntityTypeId == personAliasEntityType.Id &&
                                        f.PersonAliasId == CurrentPersonAliasId.Value &&
                                        f.EntityId == preferredContact.PrimaryAliasId )
                                    .Distinct();

                                if ( alreadyFollowing.Any() )
                                {
                                    followingService.DeleteRange( alreadyFollowing );
                                }
                                else
                                {
                                    // Add a following record
                                    var following = new Following();
                                    following.EntityTypeId = personAliasEntityType.Id;
                                    following.EntityId = preferredContact.PrimaryAliasId.Value;
                                    following.PersonAliasId = CurrentPersonAliasId.Value;
                                    followingService.Add( following );
                                }

                                rockContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        //// Group Methods

        /// <summary>
        /// Displays the view group  using a lava template
        /// </summary>
        private void DisplayViewGroup()
        {
            if ( _groupId > 0 )
            {
                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );

                bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();

                var qry = groupService
                    .Queryable( "GroupLocations,Members,Members.Person,Members.Person.PhoneNumbers,GroupType" )
                    .Where( g => g.Id == _groupId );

                if ( !enableDebug )
                {
                    qry = qry.AsNoTracking();
                }

                var group = qry.FirstOrDefault();

                // order group members by name
                if ( group != null )
                {
                    group.Members = group.Members.OrderBy( m => m.Person.LastName ).ThenBy( m => m.Person.FirstName ).ToList();
                }

                Person preferredContact = GetPreferredContact( group );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "Group", group );
                mergeFields.Add( "PreferredContact", preferredContact );

                if ( preferredContact != null )
                {
                    var followingEntityType = EntityTypeCache.Get( preferredContact.PrimaryAlias.GetType() );
                    if ( this.CurrentPerson != null && this.CurrentPerson.PrimaryAliasId.HasValue )
                    {

                        var personAliasService = new PersonAliasService( rockContext );
                        var followingService = new FollowingService( rockContext );

                        var followingQry = followingService.Queryable()
                            .Where( f =>
                                f.EntityTypeId == followingEntityType.Id &&
                                f.PersonAlias.PersonId == this.CurrentPersonId );

                        followingQry = followingQry.Where( f => f.EntityId == preferredContact.PrimaryAlias.Id );

                        mergeFields.Add( "Following", followingQry.Any() );
                    }
                }


                // add linked pages
                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( "CommunicationPage", LinkedPageRoute( "CommunicationPage" ) );

                // add collection of allowed security actions
                Dictionary<string, object> securityActions = new Dictionary<string, object>();
                securityActions.Add( "View", group != null && group.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                securityActions.Add( "Edit", group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) );
                securityActions.Add( "Administrate", group != null && group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) );
                mergeFields.Add( "AllowedActions", securityActions );

                Dictionary<string, object> currentPageProperties = new Dictionary<string, object>();
                currentPageProperties.Add( "Id", RockPage.PageId );
                currentPageProperties.Add( "Path", Request.Path );
                mergeFields.Add( "CurrentPage", currentPageProperties );

                string template = GetAttributeValue( "LavaTemplate" );

                // show debug info
                if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
                {
                    string postbackCommands = @"<h5>Available Postback Commands</h5>
                                                <ul>
                                                    <li><strong>EditGroup:</strong> Shows a panel for modifing group info. Expects a group id. <code>{{ Group.Id | Postback:'EditGroup' }}</code></li>
                                                    <li><strong>AddGroupMember:</strong> Shows a panel for adding group info. Does not require input. <code>{{ '' | Postback:'AddGroupMember' }}</code></li>
                                                    <li><strong>EditGroupMember:</strong> Shows a panel for modifing group info. Expects a group member id. <code>{{ member.Id | Postback:'EditGroupMember' }}</code></li>
                                                    <li><strong>DeleteGroupMember:</strong> Deletes a group member. Expects a group member id. <code>{{ member.Id | Postback:'DeleteGroupMember' }}</code></li>
                                                    <li><strong>SendCommunication:</strong> Sends a communication to all group members on behalf of the Current User. This will redirect them to the communication page where they can author their email. <code>{{ '' | Postback:'SendCommunication' }}</code></li>
                                                </ul>";

                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo( null, string.Empty, postbackCommands );
                }

                lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );
            }
            else
            {
                lContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }
        }

        private static Person GetPreferredContact( Group group )
        {
            Person preferredContact = null;
            if ( group != null )
            {
                group.Members = group.Members.OrderBy( m => m.Person.LastName ).ThenBy( m => m.Person.FirstName ).ToList();

                var adultMemberList = group.Members.Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active && gm.GroupRoleId == 3 ).ToList();
                foreach ( var member in adultMemberList )
                {
                    var person = member.Person;
                    person.LoadAttributes();
                    var isPreferredContact = person.GetAttributeValue( "IsPreferredContact" ).AsBoolean();
                    if ( isPreferredContact )
                    {
                        preferredContact = person;
                        break;
                    }
                }

                if ( preferredContact == null )
                {
                    preferredContact = adultMemberList.Where( gm => gm.Person.Gender == Gender.Female ).Select( gm => gm.Person ).FirstOrDefault();
                }

                if ( preferredContact == null )
                {
                    preferredContact = adultMemberList.Select( gm => gm.Person ).FirstOrDefault();
                }
            }


            return preferredContact;
        }

        /// <summary>
        /// Displays the edit group panel.
        /// </summary>
        private void DisplayEditGroup()
        {
            this.IsEditingGroup = true;

            if ( _groupId != -1 )
            {
                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );

                var qry = groupService
                        .Queryable( "GroupLocations,GroupType,Schedule" )
                        .Where( g => g.Id == _groupId );

                var group = qry.FirstOrDefault();

                if ( group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    lEditTitle.Text = string.Format( "Edit {0}", group.Name );
                    tbGroupName.Text = group.Name;
                    cpCampus.SelectedCampusId = group.CampusId;


                    // If all group members have the same record status, display that value
                    if ( group.Members.Select( m => m.Person.RecordStatusValueId ).Distinct().Count() == 1 )
                    {
                        ddlRecordStatus.SetValue( group.Members.Select( m => m.Person.RecordStatusValueId ).FirstOrDefault() );
                    }
                    else
                    {
                        ddlRecordStatus.Warning = "Family members have different record statuses";
                    }

                    // If all group members have the same inactive reason, set that value
                    if ( group.Members.Select( m => m.Person.RecordStatusReasonValueId ).Distinct().Count() == 1 )
                    {
                        ddlReason.SetValue( group.Members.Select( m => m.Person.RecordStatusReasonValueId ).FirstOrDefault() );
                    }
                    else
                    {
                        if ( String.IsNullOrWhiteSpace( ddlRecordStatus.Warning ) )
                        {
                            ddlRecordStatus.Warning = "Family members have different record status reasons";
                        }
                        else
                        {
                            ddlRecordStatus.Warning += " and record status reasons";
                        }
                    }


                    group.LoadAttributes();
                    phAttributes.Controls.Clear();

                    string categoryGuid = GetAttributeValue( "Category" );
                    Guid guid = Guid.Empty;
                    if ( Guid.TryParse( categoryGuid, out guid ) )
                    {
                        var category = CategoryCache.Get( guid );
                        if ( category != null )
                        {
                            List<string> excludeList = new List<string>();
                            var allowedAttributeIds = new AttributeService( rockContext ).GetByCategoryId( category.Id ).Select( a => a.Id ).ToList();
                            excludeList.AddRange( group.Attributes.Where( a => !allowedAttributeIds.Contains( a.Value.Id ) ).Select( a => a.Value.Name ) );
                            Rock.Attribute.Helper.AddEditControls( group, phAttributes, true, BlockValidationGroup, excludeList, false );

                        }
                    }

                    GroupAddresses = new List<GroupAddressInfo>();
                    foreach ( var groupLocation in group.GroupLocations
                        .Where( l => l.GroupLocationTypeValue != null )
                        .OrderBy( l => l.GroupLocationTypeValue.Order ) )
                    {
                        GroupAddresses.Add( new GroupAddressInfo( groupLocation ) );
                    }
                    foreach ( var groupLocation in group.GroupLocations
                        .Where( l => l.GroupLocationTypeValue == null ) )
                    {
                        GroupAddresses.Add( new GroupAddressInfo( groupLocation ) );
                    }

                    BindLocations();
                }
                else
                {
                    lContent.Text = "<div class='alert alert-warning'>You do not have permission to edit this group.</div>";
                }
            }
            else
            {
                lContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }
        }

        #endregion

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

            public bool IsMailing { get; set; }

            public bool IsLocation { get; set; }

            public GroupAddressInfo( GroupLocation groupLocation )
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
                    }

                    IsMailing = groupLocation.IsMailingLocation;
                    IsLocation = groupLocation.IsMappedLocation;
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
                    string result = string.Format(
                        "{0} {1} {2}, {3} {4}",
                        this.Street1,
                        this.Street2,
                        this.City,
                        this.State,
                        this.PostalCode ).ReplaceWhileExists( "  ", " " );

                    var countryValue = Rock.Web.Cache.DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                        .DefinedValues
                        .Where( v => v.Value.Equals( this.Country, StringComparison.OrdinalIgnoreCase ) )
                        .FirstOrDefault();
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
}
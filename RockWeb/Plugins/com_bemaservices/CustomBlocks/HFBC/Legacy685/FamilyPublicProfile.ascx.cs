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
    [DisplayName( "Family Public Profile" )]
    [Category( "org_hfbc > Legacy 685" )]
    [Description( "Block used to view details of a Legacy 685 family" )]

    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupDetail.lava' %}", "", 8 )]
    [BooleanField( "Use Current Person", "Whether the block should use the current person to generate family members" )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "GroupTypeId", Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Family Attributes", "The family attributes that should be displayed / edited.", false, true, order: 6 )]
    [BooleanField( "Enable Location Edit", "Enables changing locations when editing a group.", false, "", 9 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 10 )]
    public partial class FamilyPublicProfile : Rock.Web.UI.RockBlock
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

                List<Guid> familyAttributeGuidList = GetAttributeValue( "FamilyAttributes" ).SplitDelimitedValues().AsGuidList();
                if ( familyAttributeGuidList.Any() )
                {
                    phAttributes.Controls.Clear();
                    group.LoadAttributes();
                    var excludedAttributeList = group.Attributes.Where( a => !familyAttributeGuidList.Contains( a.Value.Guid ) ).Select( a => a.Value.Key ).ToList();
                    if ( group.Attributes != null && group.Attributes.Any() && familyAttributeGuidList.Any() )
                    {
                        phAttributes.Visible = true;
                        Rock.Attribute.Helper.AddEditControls( group, phAttributes, false, BlockValidationGroup, excludedAttributeList, false, 2 );
                    }
                    else
                    {
                        phAttributes.Visible = false;
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
                    History.EvaluateChange(
                        groupChanges,
                        "Campus",
                        group.CampusId.HasValue ? CampusCache.Read( group.CampusId.Value ).Name : string.Empty,
                        campusId.HasValue ? CampusCache.Read( campusId.Value ).Name : string.Empty );

                    group.CampusId = campusId;
                }

                // SAVE LOCATIONS
                Guid? addressTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
                if ( addressTypeGuid.HasValue )
                {
                    var groupLocationService = new GroupLocationService( rockContext );

                    var dvHomeAddressType = DefinedValueCache.Read( addressTypeGuid.Value );
                    var familyAddress = groupLocationService.Queryable().Where( l => l.GroupId == group.Id && l.GroupLocationTypeValueId == dvHomeAddressType.Id ).FirstOrDefault();
                    if ( familyAddress != null && string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                    {
                        // delete the current address
                        History.EvaluateChange( groupChanges, familyAddress.GroupLocationTypeValue.Value + " Location", familyAddress.Location.ToString(), string.Empty );
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
                                familyAddress.GroupId = group.Id;
                                familyAddress.IsMailingLocation = true;
                                familyAddress.IsMappedLocation = true;
                            }
                            else if ( hfStreet1.Value != string.Empty )
                            {
                                // user clicked move so create a previous address
                                var previousAddress = new GroupLocation();
                                groupLocationService.Add( previousAddress );

                                var previousAddressValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                                if ( previousAddressValue != null )
                                {
                                    previousAddress.GroupLocationTypeValueId = previousAddressValue.Id;
                                    previousAddress.GroupId = group.Id;

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

                            var updatedHomeAddress = new Location();
                            acAddress.GetValues( updatedHomeAddress );

                            History.EvaluateChange( groupChanges, dvHomeAddressType.Value + " Location", familyAddress.Location != null ? familyAddress.Location.ToString() : string.Empty, updatedHomeAddress.ToString() );

                            familyAddress.Location = updatedHomeAddress;
                            rockContext.SaveChanges();
                        }
                    }
                }

                rockContext.SaveChanges();
                group.SaveAttributeValues( rockContext );

                foreach ( var fm in group.Members )
                {
                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                        fm.PersonId,
                        groupChanges,
                        group.Name,
                        typeof( Group ),
                        group.Id );
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

        #region Location Events

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

        #endregion
        #endregion

        #region Methods

        private int? GetGroupId( RockContext rockContext = null )
        {
            int? groupId = null;

            groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
            if ( !groupId.HasValue )
            {
                if ( GetAttributeValue( "UseCurrentPerson" ).AsBoolean() )
                {
                    if ( CurrentPersonId.HasValue )
                    {
                        if ( rockContext == null )
                        {
                            rockContext = new RockContext();
                        }

                        var person = new PersonService( rockContext ).Get( CurrentPersonId.Value );
                        if ( person != null )
                        {
                            groupId = person.GetFamily().Id;
                        }
                    }

                }
                else
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

                        case "SendCommunication":
                            SendCommunication();
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

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "Group", group );
                mergeFields.Add( "GroupId", _groupId);

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
                    cpCampus.SelectedCampusId = group.CampusId;


                    List<Guid> familyAttributeGuidList = GetAttributeValue( "FamilyAttributes" ).SplitDelimitedValues().AsGuidList();
                    if ( familyAttributeGuidList.Any() )
                    {
                        phAttributes.Controls.Clear();
                        group.LoadAttributes();
                        var excludedAttributeList = group.Attributes.Where( a => !familyAttributeGuidList.Contains( a.Value.Guid ) ).Select( a => a.Value.Key ).ToList();
                        if ( group.Attributes != null && group.Attributes.Any() && familyAttributeGuidList.Any() )
                        {
                            phAttributes.Visible = true;
                            Rock.Attribute.Helper.AddEditControls( group, phAttributes, true, BlockValidationGroup, excludedAttributeList, false, 2 );
                        }
                        else
                        {
                            phAttributes.Visible = false;
                        }
                    }

                    Guid? locationTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
                    if ( locationTypeGuid.HasValue )
                    {
                        var addressTypeDv = DefinedValueCache.Read( locationTypeGuid.Value );

                        // if address type is home enable the move and is mailing/physical
                        if ( addressTypeDv.Guid == Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )
                        {
                            lbMoved.Visible = true;
                            cbIsMailingAddress.Visible = true;
                            cbIsPhysicalAddress.Visible = true;
                        }
                        else
                        {
                            lbMoved.Visible = false;
                            cbIsMailingAddress.Visible = false;
                            cbIsPhysicalAddress.Visible = false;
                        }

                        lAddressTitle.Text = addressTypeDv.Value + " Address";

                        var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuidOrNull();

                        if ( familyGroupTypeGuid.HasValue )
                        {
                            var familyGroupType = GroupTypeCache.Read( familyGroupTypeGuid.Value );

                            var familyAddress = new GroupLocationService( rockContext ).Queryable()
                                                .Where( l => l.Group.GroupTypeId == familyGroupType.Id
                                                     && l.GroupLocationTypeValueId == addressTypeDv.Id
                                                     && l.Group.Id == group.Id )
                                                .FirstOrDefault();
                            if ( familyAddress != null )
                            {
                                acAddress.SetValues( familyAddress.Location );

                                cbIsMailingAddress.Checked = familyAddress.IsMailingLocation;
                                cbIsPhysicalAddress.Checked = familyAddress.IsMappedLocation;
                            }
                        }
                    }
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

        /// <summary>
        /// Sends the communication.
        /// </summary>
        private void SendCommunication()
        {
            // create communication
            if ( this.CurrentPerson != null && _groupId != -1 && !string.IsNullOrWhiteSpace( GetAttributeValue( "CommunicationPage" ) ) )
            {
                var rockContext = new RockContext();
                var service = new Rock.Model.CommunicationService( rockContext );
                var communication = new Rock.Model.Communication();
                communication.IsBulkCommunication = false;
                communication.Status = Rock.Model.CommunicationStatus.Transient;

                communication.SenderPersonAliasId = this.CurrentPersonAliasId;

                service.Add( communication );

                var personAliasIds = new GroupMemberService( rockContext ).Queryable()
                                    .Where( m => m.GroupId == _groupId && m.GroupMemberStatus != GroupMemberStatus.Inactive )
                                    .ToList()
                                    .Select( m => m.Person.PrimaryAliasId )
                                    .ToList();

                // Get the primary aliases
                foreach ( int personAlias in personAliasIds )
                {
                    var recipient = new Rock.Model.CommunicationRecipient();
                    recipient.PersonAliasId = personAlias;
                    communication.Recipients.Add( recipient );
                }

                rockContext.SaveChanges();

                Dictionary<string, string> queryParameters = new Dictionary<string, string>();
                queryParameters.Add( "CommunicationId", communication.Id.ToString() );

                NavigateToLinkedPage( "CommunicationPage", queryParameters );
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

                    var countryValue = Rock.Web.Cache.DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
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
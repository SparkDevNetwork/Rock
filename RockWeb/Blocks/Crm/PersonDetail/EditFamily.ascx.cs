//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile blockthe main information about a peron 
    /// </summary>
    public partial class EditFamily : PersonBlock
    {
        private Group _family = null;
        private bool _canEdit = false;
        private DefinedTypeCache addressTypes;
        private List<GroupRole> familyRoles = new List<GroupRole>();
        protected string basePersonUrl;

        private List<FamilyMember> FamilyMembers
        {
            get { return ViewState["FamilyMembers"] as List<FamilyMember>; }
            set { ViewState["FamilyMembers"] = value; }
        }

        private List<FamilyAddress> FamilyAddresses
        {
            get { return ViewState["FamilyAddresses"] as List<FamilyAddress>; }
            set { ViewState["FamilyAddresses"] = value; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            basePersonUrl = ResolveUrl( "~/Person/" );

            int familyId = int.MinValue;
            if ( int.TryParse( PageParameter( "FamilyId" ), out familyId ) )
            {
                _family = new GroupService().Get( familyId );
                if ( _family != null && string.Compare( _family.GroupType.Guid.ToString(), Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, true ) != 0 )
                {
                    nbNotice.Heading = "Invalid Family";
                    nbNotice.Text = "Sorry, but the group selected is not a Family group";
                    nbNotice.NotificationBoxType = NotificationBoxType.Error;
                    nbNotice.Visible = true;

                    _family = null;
                    return;
                }
                else
                {
                    familyRoles = _family.GroupType.Roles.OrderBy( r => r.SortOrder ).ToList();
                    rblNewPersonRole.DataSource = familyRoles;
                    rblNewPersonRole.DataBind();
                }
            }

            _canEdit = IsUserAuthorized( "Edit" );

            addressTypes = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.LOCATION_LOCATION_TYPE ) );
            var campusi = new CampusService().Queryable().OrderBy( a => a.Name ).ToList();
            cpCampus.Campuses = campusi;
            cpCampus.Visible = campusi.Any();

            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ) );
            ddlReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ), true );

            lvMembers.DataKeyNames = new string[] { "Index" };
            lvMembers.ItemDataBound += lvMembers_ItemDataBound;
            lvMembers.ItemCommand += lvMembers_ItemCommand;

            modalAddPerson.SaveButtonText = "Ok";
            modalAddPerson.SaveClick += modalAddPerson_SaveClick;

            gLocations.DataKeyNames = new string[] { "id" };
            gLocations.RowDataBound += gLocations_RowDataBound;
            gLocations.RowEditing += gLocations_RowEditing;
            gLocations.RowUpdating += gLocations_RowUpdating;
            gLocations.RowCancelingEdit += gLocations_RowCancelingEdit;
            gLocations.Actions.ShowAdd = _canEdit;
            gLocations.Actions.ShowAdd = true;
            gLocations.Actions.AddClick += gLocations_Add;
            gLocations.IsDeleteEnabled = _canEdit;
            gLocations.GridRebind += gLocations_GridRebind;

            ddlNewPersonGender.BindToEnum( typeof( Gender ) );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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
                            var role = familyRoles.Where( r => r.Id == roleId.Value ).FirstOrDefault();
                            if ( role != null )
                            {
                                int index = (int)lvMembers.DataKeys[item.DataItemIndex]["Index"];
                                var familyMember = FamilyMembers.Where( m => m.Index == index ).FirstOrDefault();
                                if ( familyMember != null )
                                {
                                    familyMember.RoleGuid = role.Guid;
                                    familyMember.RoleName = role.Name;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if ( _family != null )
                {

                    tbFamilyName.Text = _family.Name;
                    cpCampus.SelectedCampusId = _family.CampusId;

                    FamilyMembers = new List<FamilyMember>();
                    foreach ( var familyMember in _family.Members )
                    {
                        FamilyMembers.Add( new FamilyMember( familyMember, true ) );
                    }
                    BindMembers();

                    FamilyAddresses = new List<FamilyAddress>();
                    foreach ( var groupLocation in _family.GroupLocations )
                    {
                        FamilyAddresses.Add( new FamilyAddress( groupLocation ) );
                    }
                    BindLocations();

                }
            }
        }

        #region Events

        /// <summary>
        /// Handles the TextChanged event of the tbFamilyName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tbFamilyName_TextChanged( object sender, EventArgs e )
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
            ddlReason.Visible = ( ddlRecordStatus.SelectedValueAsInt() == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id );
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


        #region Family Member List Events

        /// <summary>
        /// Handles the ItemDataBound event of the lvMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void lvMembers_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                var familyMember = e.Item.DataItem as FamilyMember;
                if ( familyMember != null)
                {
                    System.Web.UI.WebControls.Image imgPerson = e.Item.FindControl( "imgPerson" ) as System.Web.UI.WebControls.Image;
                    if ( imgPerson != null )
                    {
                        imgPerson.Visible = familyMember.PhotoId.HasValue;
                        if ( familyMember.PhotoId.HasValue )
                        {
                            imgPerson.ImageUrl = string.Format( "~/GetImage.ashx?id={0}", familyMember.PhotoId );
                        }
                    }

                    var rblRole = e.Item.FindControl( "rblRole" ) as RadioButtonList;
                    if ( rblRole != null )
                    {
                        rblRole.DataSource = familyRoles;
                        rblRole.DataBind();

                        var role = familyRoles.Where( r => r.Guid.Equals(familyMember.RoleGuid)).FirstOrDefault();
                        if ( role != null )
                        {
                            rblRole.SelectedValue = role.Id.ToString();
                        }
                    }

                    int members = FamilyMembers.Where( m => !m.Removed ).Count();

                    var lbNewFamily = e.Item.FindControl( "lbNewFamily" ) as LinkButton;
                    if ( lbNewFamily != null )
                    {
                        lbNewFamily.Visible = familyMember.ExistingFamilyMember && members > 1;
                    }

                    var lbRemoveMember = e.Item.FindControl( "lbRemoveMember" ) as LinkButton;
                    if ( lbRemoveMember != null )
                    {
                        lbRemoveMember.Visible = !familyMember.ExistingFamilyMember && members > 1;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        void lvMembers_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            int index = (int)lvMembers.DataKeys[e.Item.DataItemIndex]["Index"];
            var familyMember = FamilyMembers.Where( m => m.Index == index ).FirstOrDefault();
            if ( familyMember != null )
            {
                if (e.CommandName == "Move")
                {
                    familyMember.Removed = true;
                }
                else if (e.CommandName == "Remove")
                {
                    FamilyMembers.RemoveAt( index );
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
            hfActiveTab.Value = "Existing";
            modalAddPerson.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void modalAddPerson_SaveClick( object sender, EventArgs e )
        {
            if ( hfActiveTab.Value == "Existing" )
            {
                int? personId = ppExistingPerson.PersonId;
                if (personId.HasValue)
                {
                    using ( new UnitOfWorkScope() )
                    {
                        var person = new PersonService().Get( personId.Value );
                        if ( person != null )
                        {
                            var familyMember = new FamilyMember();
                            familyMember.SetValuesFromPerson( person );

                            var familyRoleIds = familyRoles.Select( r => r.Id).ToList();

                            var existingFamilyRoles = new GroupMemberService().Queryable("GroupRole")
                                .Where( m => m.PersonId == person.Id && familyRoleIds.Contains( m.GroupRoleId ) )
                                .OrderBy( m => m.GroupRole.SortOrder)
                                .ToList();

                            var existingRole = existingFamilyRoles.Select( m => m.GroupRole).FirstOrDefault();
                            if (existingRole != null)
                            {
                                familyMember.RoleGuid = existingRole.Guid;
                                familyMember.RoleName = existingRole.Name;
                            }

                            familyMember.ExistingFamilyMember = existingFamilyRoles.Any( r => r.GroupId == _family.Id);
                            familyMember.RemoveFromOtherFamilies = cbRemoveOtherFamilies.Checked;

                            FamilyMembers.Add( familyMember );
                        }
                    }
                }
            }
            else
            {
                var familyMember = new FamilyMember();
                familyMember.FirstName = tbNewPersonFirstName.Text;
                familyMember.LastName = tbNewPersonLastName.Text;
                familyMember.Gender = ddlNewPersonGender.SelectedValueAsEnum<Gender>();
                familyMember.BirthDate = dpNewPersonBirthDate.SelectedDate;
                var role = familyRoles.Where( r => r.Id == ( rblNewPersonRole.SelectedValueAsInt() ?? 0 ) ).FirstOrDefault();
                if ( role != null )
                {
                    familyMember.RoleGuid = role.Guid;
                    familyMember.RoleName = role.Name;
                }
                FamilyMembers.Add( familyMember );
            }

            tbNewPersonFirstName.Required = false;
            tbNewPersonLastName.Required = false;

            confirmExit.Enabled = true;

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
            var homeLocType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_HOME ) );
            var prevLocType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_PREVIOUS ) );

            if ( homeLocType != null && prevLocType != null )
            {
                foreach ( var familyAddress in FamilyAddresses )
                {
                    if ( familyAddress.LocationTypeId == homeLocType.Id )
                    {
                        familyAddress.LocationTypeId = prevLocType.Id;
                        familyAddress.LocationTypeName = prevLocType.Name;
                    }
                }

                FamilyAddresses.Add( new FamilyAddress { LocationTypeId = homeLocType.Id, LocationTypeName = homeLocType.Name, LocationIsDirty=true } );

                gLocations.EditIndex = FamilyAddresses.Count - 1;

                confirmExit.Enabled = true;

                BindLocations();
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gLocations_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                if ( ( e.Row.RowState & DataControlRowState.Edit ) == DataControlRowState.Edit )
                {
                    FamilyAddress familyAddress = e.Row.DataItem as FamilyAddress;
                    var ddlLocType = e.Row.FindControl( "ddlLocType" ) as DropDownList;
                    if ( familyAddress != null && ddlLocType != null )
                    {
                        ddlLocType.BindToDefinedType( addressTypes );
                        ddlLocType.SelectedValue = familyAddress.LocationTypeId.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowEditing event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        void gLocations_RowEditing( object sender, GridViewEditEventArgs e )
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
            FamilyAddresses.Add( new FamilyAddress() );
            gLocations.EditIndex = FamilyAddresses.Count - 1;

            BindLocations();
        }

        /// <summary>
        /// Handles the RowUpdating event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewUpdateEventArgs" /> instance containing the event data.</param>
        void gLocations_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {
            var familyAddress = FamilyAddresses[e.RowIndex];
            if ( familyAddress != null)
            {
                if ( familyAddress.Id < 0 ) // was added
                {
                    familyAddress.Id = 0;
                }

                var row = gLocations.Rows[e.RowIndex];
                DropDownList ddlLocType = row.FindControl( "ddlLocType" ) as DropDownList;
                TextBox tbStreet1 = row.FindControl( "tbStreet1" ) as TextBox;
                TextBox tbStreet2 = row.FindControl( "tbStreet2" ) as TextBox;
                TextBox tbCity = row.FindControl( "tbCity" ) as TextBox;
                DropDownList ddlState = row.FindControl( "ddlState" ) as DropDownList;
                TextBox tbZip = row.FindControl( "tbZip" ) as TextBox;
                CheckBox cbMailing = row.FindControl("cbMailing") as CheckBox;
                CheckBox cbLocation = row.FindControl("cbLocation") as CheckBox;

                familyAddress.LocationTypeId = ddlLocType.SelectedValueAsInt() ?? 0;
                familyAddress.LocationTypeName = ddlLocType.SelectedItem != null ? ddlLocType.SelectedItem.Text : string.Empty;
                familyAddress.Street1 = tbStreet1.Text;
                familyAddress.Street2 = tbStreet2.Text;
                familyAddress.City = tbCity.Text;
                familyAddress.State = ddlState.SelectedValue;
                familyAddress.Zip = tbZip.Text;
                familyAddress.IsMailing = cbMailing.Checked;
                familyAddress.IsLocation = cbLocation.Checked;
                familyAddress.LocationIsDirty = true;
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
        void gLocations_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {
            var familyAddress = FamilyAddresses[e.RowIndex];
            if ( familyAddress != null && familyAddress.Id < 0 ) // was added
            {
                FamilyAddresses.RemoveAt( e.RowIndex );
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
            FamilyAddresses.RemoveAt( e.RowIndex );

            confirmExit.Enabled = true;

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
            // confirmation was disabled by btnSave on client-side.  So if returning without a redirect,
            // it should be enabled.  If returning with a redirect, the control won't be updated to reflect
            // confirmation being enabled, so it's ok to enable it here
            confirmExit.Enabled = true;

            if ( Page.IsValid )
            {
                confirmExit.Enabled = true;

                using ( new UnitOfWorkScope() )
                {
                    var familyService = new GroupService();
                    var familyMemberService = new GroupMemberService();
                    var personService = new PersonService();

                    // SAVE FAMILY
                    _family = familyService.Get( _family.Id );
                    _family.Name = tbFamilyName.Text;
                    _family.CampusId = cpCampus.SelectedValueAsInt();

                    var familyGroupTypeId = _family.GroupTypeId;

                    familyService.Save( _family, CurrentPersonId );

                    // SAVE FAMILY MEMBERS
                    int? recordStatusValueID = ddlRecordStatus.SelectedValueAsInt();
                    int? reasonValueId = ddlReason.SelectedValueAsInt();
                    var newFamilies = new List<Group>();

                    foreach ( var familyMember in FamilyMembers )
                    {
                        var role = familyRoles.Where( r => r.Guid.Equals( familyMember.RoleGuid ) ).FirstOrDefault();
                        if ( role == null )
                        {
                            role = familyRoles.FirstOrDefault();
                        }

                        // People added to family (new or from other family)
                        if ( !familyMember.ExistingFamilyMember )
                        {
                            var groupMember = new GroupMember();

                            if ( familyMember.Id == -1 )
                            {
                                // added new person
                                groupMember.Person = new Person();
                                groupMember.Person.GivenName = familyMember.FirstName;
                                groupMember.Person.LastName = familyMember.LastName;
                                groupMember.Person.Gender = familyMember.Gender;
                                groupMember.Person.BirthDate = familyMember.BirthDate;
                            }
                            else
                            {
                                // added from other family
                                groupMember.Person = personService.Get( familyMember.Id );
                            }

                            groupMember.Person.RecordStatusValueId = recordStatusValueID;
                            groupMember.Person.RecordStatusReasonValueId = reasonValueId;

                            groupMember.GroupId = _family.Id;
                            if ( role != null )
                            {
                                groupMember.GroupRoleId = role.Id;
                            }

                            if ( groupMember.Person != null )
                            {
                                familyMemberService.Add( groupMember, CurrentPersonId );
                                familyMemberService.Save( groupMember, CurrentPersonId );
                            }
                        }
                        else
                        {
                            // existing family members
                            var groupMember = familyMemberService.Queryable().Where( m =>
                                m.PersonId == familyMember.Id &&
                                m.Group.GroupTypeId == familyGroupTypeId &&
                                m.GroupId == _family.Id ).FirstOrDefault();
                            if ( groupMember != null )
                            {
                                if ( familyMember.Removed )
                                {
                                    // Family member was removed and should be created in their own new family
                                    var newFamily = new Group();
                                    newFamily.Name = familyMember.LastName + " Family";
                                    newFamily.GroupTypeId = familyGroupTypeId;
                                    newFamily.CampusId = _family.CampusId;
                                    familyService.Add( newFamily, CurrentPersonId );
                                    familyService.Save( newFamily, CurrentPersonId );

                                    groupMember.Group = newFamily;
                                    familyMemberService.Save( groupMember, CurrentPersonId );

                                    newFamilies.Add( newFamily );
                                }
                                else
                                {
                                    // Existing member was not remvoved
                                    if ( role != null )
                                    {
                                        groupMember.GroupRoleId = role.Id;
                                        groupMember.Person.RecordStatusValueId = recordStatusValueID;
                                        groupMember.Person.RecordStatusReasonValueId = reasonValueId;
                                        familyMemberService.Save( groupMember, CurrentPersonId );
                                    }
                                }
                            }
                        }

                        // Remove anyone that was moved from another family
                        if ( familyMember.RemoveFromOtherFamilies )
                        {
                            var otherFamilies = familyMemberService.Queryable()
                                .Where( m =>
                                    m.PersonId == familyMember.Id &&
                                    m.Group.GroupTypeId == familyGroupTypeId &&
                                    m.GroupId != _family.Id )
                                .ToList();

                            foreach ( var otherFamilyMember in otherFamilies )
                            {
                                var fm = familyMemberService.Get( otherFamilyMember.Id );
                                familyMemberService.Delete( fm, CurrentPersonId );
                                familyMemberService.Save( fm, CurrentPersonId );

                                var f = familyService.Queryable()
                                    .Where( g =>
                                        g.Id == otherFamilyMember.GroupId &&
                                        !g.Members.Any() )
                                    .FirstOrDefault();

                                if ( f != null )
                                {
                                    familyService.Delete( f, CurrentPersonId );
                                    familyService.Save( f, CurrentPersonId );
                                }
                            }
                        }
                    }

                    // SAVE LOCATIONS
                    var groupLocationService = new GroupLocationService();

                    // delete any group locations that were removed
                    var remainingLocationIds = FamilyAddresses.Where( a => a.Id > 0 ).Select( a => a.Id ).ToList();
                    foreach ( var removedLocation in groupLocationService.Queryable()
                        .Where( l => l.GroupId == _family.Id &&
                            !remainingLocationIds.Contains( l.Id ) ) )
                    {
                        groupLocationService.Delete( removedLocation, CurrentPersonId );
                        groupLocationService.Save( removedLocation, CurrentPersonId );
                    }

                    foreach ( var familyAddress in FamilyAddresses )
                    {
                        Location updatedAddress = null;
                        if (familyAddress.LocationIsDirty)
                        {
                            updatedAddress  = new LocationService().Get(
                                familyAddress.Street1, familyAddress.Street2, familyAddress.City,
                                familyAddress.State, familyAddress.Zip );
                        }

                        GroupLocation groupLocation = null;
                        if ( familyAddress.Id > 0 )
                        {
                            groupLocation = groupLocationService.Get( familyAddress.Id );
                        }
                        if ( groupLocation == null )
                        {
                            groupLocation = new GroupLocation();
                            groupLocation.GroupId = _family.Id;
                            groupLocationService.Add( groupLocation, CurrentPersonId );
                        }

                        groupLocation.GroupLocationTypeValueId = familyAddress.LocationTypeId;
                        groupLocation.IsMailing = familyAddress.IsMailing;
                        groupLocation.IsLocation = familyAddress.IsLocation;
                        if ( updatedAddress != null )
                        {
                            groupLocation.Location = updatedAddress;
                        }

                        groupLocationService.Save( groupLocation, CurrentPersonId );


                        // Add the same locations to any new families created by removing an existing family member
                        if ( newFamilies.Any() )
                        {
                            //reload grouplocation for access to child properties
                            groupLocation = groupLocationService.Get( groupLocation.Id );
                            foreach ( var newFamily in newFamilies )
                            {
                                var newFamilyLocation = new GroupLocation();
                                newFamilyLocation.GroupId = newFamily.Id;
                                newFamilyLocation.LocationId = groupLocation.LocationId;
                                newFamilyLocation.GroupLocationTypeValueId = groupLocation.GroupLocationTypeValueId;
                                newFamilyLocation.IsMailing = groupLocation.IsMailing;
                                newFamilyLocation.IsLocation = groupLocation.IsLocation;
                                groupLocationService.Add( newFamilyLocation, CurrentPersonId );
                                groupLocationService.Save( newFamilyLocation, CurrentPersonId );
                            }
                        }
                    }

                    _family = familyService.Get( _family.Id );
                    if ( _family.Members.Any( m => m.PersonId == Person.Id ) )
                    {
                        Response.Redirect( string.Format( "~/Person/{0}", Person.Id ), false );
                    }
                    else
                    {
                        var fm = _family.Members
                            .Where( m =>
                                m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                                m.Person.Gender == Gender.Male )
                            .OrderByDescending( m => m.Person.Age )
                            .FirstOrDefault();
                        if ( fm == null )
                        {
                            fm = _family.Members
                                .Where( m =>
                                    m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                                .OrderByDescending( m => m.Person.Age )
                                .FirstOrDefault();
                        }
                        if ( fm == null )
                        {
                            fm = _family.Members
                                .OrderByDescending( m => m.Person.Age )
                                .FirstOrDefault();
                        }
                        if ( fm != null )
                        {
                            Response.Redirect( string.Format( "~/Person/{0}", fm.PersonId ), false );
                        }
                        else
                        {
                            Response.Redirect( "~", false );
                        }
                    }

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

        #endregion

        #endregion

        #region Private Methods

        private void BindMembers()
        {
            int i = 0;
            FamilyMembers.ForEach( m => m.Index = i++ );

            lvMembers.DataSource = GetMembersOrdered();
            lvMembers.DataBind();
        }

        private void BindLocations()
        {
            gLocations.DataSource = FamilyAddresses;
            gLocations.DataBind();
        }

        private List<FamilyMember> GetMembersOrdered()
        {
            var orderedMembers = new List<FamilyMember>();
                        
            // Add adult males
            orderedMembers.AddRange(FamilyMembers
                .Where( m => 
                    !m.Removed &&
                    m.RoleGuid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                    m.Gender == Gender.Male)
                .OrderByDescending( m => m.Age));
                        
            // Add adult females
            orderedMembers.AddRange( FamilyMembers
                .Where( m => 
                    !m.Removed &&
                    m.RoleGuid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                    m.Gender != Gender.Male)
                .OrderByDescending( m => m.Age));

            // Add non-adults
            orderedMembers.AddRange( FamilyMembers
                .Where( m => 
                    !m.Removed &&
                    !m.RoleGuid.Equals(new Guid(Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT)))
                .OrderByDescending( m => m.Age));

            return orderedMembers;
        }

        #endregion

}

    [Serializable]
    class FamilyMember
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public bool ExistingFamilyMember { get; set; }  // Is this person part of the original family 
        public bool Removed { get; set; } // Was an existing person removed from the family (to their own family)
        public bool RemoveFromOtherFamilies { get; set; } // When adding an existing person, should they be removed from other families
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public Guid RoleGuid { get; set; }
        public string RoleName { get; set; }
        public int? PhotoId { get; set; }

        public int? Age
        {
            get 
            {
                if (BirthDate.HasValue)
                {
                    return BirthDate.Age();
                }

                return null;
            }
        }

        public FamilyMember (GroupMember familyMember, bool existingFamilyMember)
        {
            if (familyMember != null)
            {
                SetValuesFromPerson( familyMember.Person );

                if ( familyMember.GroupRole != null )
                {
                    RoleGuid = familyMember.GroupRole.Guid;
                    RoleName = familyMember.GroupRole.Name;
                }
            }

            ExistingFamilyMember = existingFamilyMember;
            Removed = false;
        }

        public FamilyMember()
        {
            Id = -1;
            ExistingFamilyMember = false;
            Removed = false;
            RemoveFromOtherFamilies = false;
        }

        public void SetValuesFromPerson( Person person )
        {
            if ( person != null )
            {
                Id = person.Id;
                FirstName = person.GivenName;
                LastName = person.LastName;
                Gender = person.Gender;
                BirthDate = person.BirthDate;
                PhotoId = person.PhotoId;
            }
        }
    }

    [Serializable]
    class FamilyAddress
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
        public string Zip { get; set; }
        public bool IsMailing { get; set; }
        public bool IsLocation { get; set; }

        public FamilyAddress(GroupLocation groupLocation)
        {
            LocationIsDirty = false;
            if ( groupLocation != null )
            {
                Id = groupLocation.Id;

                if ( groupLocation.LocationTypeValue != null )
                {
                    LocationTypeId = groupLocation.LocationTypeValue.Id;
                    LocationTypeName = groupLocation.LocationTypeValue.Name;
                }

                if ( groupLocation.Location != null )
                {
                    LocationId = groupLocation.Location.Id;
                    Street1 = groupLocation.Location.Street1;
                    Street2 = groupLocation.Location.Street2;
                    City = groupLocation.Location.City;
                    State = groupLocation.Location.State;
                    Zip = groupLocation.Location.Zip;
                }

                IsMailing = groupLocation.IsMailing;
                IsLocation = groupLocation.IsLocation;
            }
        }

        public FamilyAddress()
        {
            Id = -1; // Adding
            LocationIsDirty = true;
        }
    }
}
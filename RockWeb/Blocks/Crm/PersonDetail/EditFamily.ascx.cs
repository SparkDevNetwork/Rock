﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
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
using Rock.Security;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile block the main information about a peron 
    /// </summary>
    [DisplayName( "Edit Family" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to edit a family." )]

    public partial class EditFamily : PersonBlock
    {
        private Group _family = null;
        private bool _canEdit = false;
        private List<DefinedValue> addressTypes = new List<DefinedValue>();
        private List<GroupTypeRole> familyRoles = new List<GroupTypeRole>();
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

        private string DefaultState
        {
            get
            {
                string state = ViewState["DefaultState"] as string;
                if ( state == null )
                {
                    string orgLocGuid = GlobalAttributesCache.Read().GetValue( "OrganizationAddress" );
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

            basePersonUrl = ResolveUrl( "~/Person/" );

            var rockContext = new RockContext();

            int familyId = int.MinValue;
            if ( int.TryParse( PageParameter( "FamilyId" ), out familyId ) )
            {
                _family = new GroupService( rockContext ).Get( familyId );
                if ( _family != null && string.Compare( _family.GroupType.Guid.ToString(), Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, true ) != 0 )
                {
                    nbNotice.Heading = "Invalid Family";
                    nbNotice.Text = "Sorry, but the group selected is not a Family group";
                    nbNotice.NotificationBoxType = NotificationBoxType.Danger;
                    nbNotice.Visible = true;

                    _family = null;
                    return;
                }
                else
                {
                    familyRoles = _family.GroupType.Roles.OrderBy( r => r.Order ).ToList();
                    rblNewPersonRole.DataSource = familyRoles;
                    rblNewPersonRole.DataBind();

                    addressTypes = _family.GroupType.LocationTypes.Select( l => l.LocationTypeValue ).OrderBy( v => v.Order ).ToList();
                }
            }

            _canEdit = IsUserAuthorized( Authorization.EDIT );

            var campusi = CampusCache.All( rockContext );
            cpCampus.Campuses = campusi;
            cpCampus.Visible = campusi.Any();

            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ), true );
            ddlReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ), true );
            rblNewPersonConnectionStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ) );

            lvMembers.DataKeyNames = new string[] { "Index" };
            lvMembers.ItemDataBound += lvMembers_ItemDataBound;
            lvMembers.ItemCommand += lvMembers_ItemCommand;

            modalAddPerson.SaveButtonText = "Save";
            modalAddPerson.SaveClick += modalAddPerson_SaveClick;
            modalAddPerson.OnCancelScript = string.Format( "$('#{0}').val('');", hfActiveTab.ClientID );

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

            ddlNewPersonGender.BindToEnum<Gender>();

            // Save and Cancel should not confirm exit
            btnSave.OnClientClick = string.Format( "javascript:$('#{0}').val('');return true;", confirmExit.ClientID );
            btnCancel.OnClientClick = string.Format( "javascript:$('#{0}').val('');return true;", confirmExit.ClientID );
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

                if ( !string.IsNullOrWhiteSpace( hfActiveTab.Value ) )
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

                    modalAddPerson.Show();
                }

            }
            else
            {
                if ( _family != null )
                {
                    tbFamilyName.Text = _family.Name;

                    // add banner text
                    if ( _family.Name.ToLower().EndsWith( " family" ) )
                    {
                        lBanner.Text = _family.Name.FormatAsHtmlTitle();
                    }
                    else
                    {
                        lBanner.Text = ( _family.Name + " Family" ).FormatAsHtmlTitle();
                    }


                    cpCampus.SelectedCampusId = _family.CampusId;

                    // If all family members have the same record status, display that value
                    if ( _family.Members.Select( m => m.Person.RecordStatusValueId ).Distinct().Count() == 1 )
                    {
                        ddlRecordStatus.SetValue( _family.Members.Select( m => m.Person.RecordStatusValueId ).FirstOrDefault() );
                    }

                    // If all family members have the same inactive reason, set that value
                    if ( _family.Members.Select( m => m.Person.RecordStatusReasonValueId ).Distinct().Count() == 1 )
                    {
                        ddlReason.SetValue( _family.Members.Select( m => m.Person.RecordStatusReasonValueId ).FirstOrDefault() );
                    }

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
                if ( familyMember != null )
                {

                    // very similar code in EditFamily.ascx.cs
                    HtmlControl divPersonImage = e.Item.FindControl( "divPersonImage" ) as HtmlControl;
                    if ( divPersonImage != null )
                    {
                        divPersonImage.Style.Add( "background-image", @String.Format( @"url({0})", Person.GetPhotoUrl( familyMember.PhotoId, familyMember.Age, familyMember.Gender ) + "&width=65" ) );
                        divPersonImage.Style.Add( "background-size", "cover" );
                        divPersonImage.Style.Add( "background-position", "50%" );
                    }

                    var rblRole = e.Item.FindControl( "rblRole" ) as RadioButtonList;
                    if ( rblRole != null )
                    {
                        rblRole.DataSource = familyRoles;
                        rblRole.DataBind();

                        var role = familyRoles.Where( r => r.Guid.Equals( familyMember.RoleGuid ) ).FirstOrDefault();
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
                if ( e.CommandName == "Move" )
                {
                    familyMember.Removed = true;
                }
                else if ( e.CommandName == "Remove" )
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
            hfActiveTab.Value = "New";

            ppPerson.SetValue( null );

            tbNewPersonFirstName.Text = string.Empty;

            // default the last name of the new family member to the lastname of the existing adults in the family (if all the adults have the same last name)
            var lastNames = FamilyMembers.Where( a => a.RoleGuid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Select( a => a.LastName ).Distinct().ToList();
            if ( lastNames.Count == 1 )
            {
                tbNewPersonLastName.Text = lastNames[0];
            }
            else
            {
                tbNewPersonLastName.Text = string.Empty;
            }


            ddlNewPersonGender.SelectedIndex = 0;
            dpNewPersonBirthDate.SelectedDate = null;
            rblNewPersonRole.SelectedIndex = 0;

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
                if ( ppPerson.PersonId.HasValue )
                {
                    var existingfamilyMember = FamilyMembers.Where( m => m.Id == ppPerson.PersonId.Value ).FirstOrDefault();
                    if ( existingfamilyMember != null )
                    {
                        existingfamilyMember.Removed = false;
                    }
                    else
                    {
                        var rockContext = new RockContext();
                        var person = new PersonService( rockContext ).Get( ppPerson.PersonId.Value );
                        if ( person != null )
                        {
                            var familyMember = new FamilyMember();
                            familyMember.SetValuesFromPerson( person );

                            var familyRoleIds = familyRoles.Select( r => r.Id ).ToList();

                            var existingFamilyRoles = new GroupMemberService( rockContext ).Queryable( "GroupRole" )
                                .Where( m => m.PersonId == person.Id && familyRoleIds.Contains( m.GroupRoleId ) )
                                .OrderBy( m => m.GroupRole.Order )
                                .ToList();

                            var existingRole = existingFamilyRoles.Select( m => m.GroupRole ).FirstOrDefault();
                            if ( existingRole != null )
                            {
                                familyMember.RoleGuid = existingRole.Guid;
                                familyMember.RoleName = existingRole.Name;
                            }

                            familyMember.ExistingFamilyMember = existingFamilyRoles.Any( r => r.GroupId == _family.Id );
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
                familyMember.NickName = tbNewPersonFirstName.Text;
                familyMember.LastName = tbNewPersonLastName.Text;
                familyMember.Gender = ddlNewPersonGender.SelectedValueAsEnum<Gender>();
                familyMember.BirthDate = dpNewPersonBirthDate.SelectedDate;

                familyMember.ConnectionStatusValueId = rblNewPersonConnectionStatus.SelectedValue.AsIntegerOrNull();
                var role = familyRoles.Where( r => r.Id == ( rblNewPersonRole.SelectedValueAsInt() ?? 0 ) ).FirstOrDefault();
                if ( role != null )
                {
                    familyMember.RoleGuid = role.Guid;
                    familyMember.RoleName = role.Name;
                }

                FamilyMembers.Add( familyMember );
            }

            ppPerson.Required = false;
            tbNewPersonFirstName.Required = false;
            tbNewPersonLastName.Required = false;

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
            var homeLocType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME ) );
            var prevLocType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS ) );

            if ( homeLocType != null && prevLocType != null )
            {
                bool setLocation = false;

                foreach ( var familyAddress in FamilyAddresses )
                {
                    if ( familyAddress.LocationTypeId == homeLocType.Id )
                    {
                        if ( familyAddress.IsLocation )
                        {
                            familyAddress.IsLocation = false;
                            setLocation = true;
                        }
                        familyAddress.IsMailing = false;
                        familyAddress.LocationTypeId = prevLocType.Id;
                        familyAddress.LocationTypeName = prevLocType.Value;
                    }
                }

                FamilyAddresses.Add( new FamilyAddress
                {
                    LocationTypeId = homeLocType.Id,
                    LocationTypeName = homeLocType.Value,
                    LocationIsDirty = true,
                    State = DefaultState,
                    IsMailing = true,
                    IsLocation = setLocation
                } );

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
                    if ( familyAddress != null )
                    {
                        var ddlLocType = e.Row.FindControl( "ddlLocType" ) as DropDownList;
                        if ( ddlLocType != null )
                        {
                            ddlLocType.DataSource = addressTypes;
                            ddlLocType.DataBind();
                            ddlLocType.SelectedValue = familyAddress.LocationTypeId.ToString();
                        }

                        var acAddress = e.Row.FindControl( "acAddress" ) as AddressControl;
                        if ( acAddress != null )
                        {
                            acAddress.UseStateAbbreviation = true;
                            acAddress.UseCountryAbbreviation = false;
                            acAddress.Country = familyAddress.Country;
                            acAddress.Street1 = familyAddress.Street1;
                            acAddress.Street2 = familyAddress.Street2;
                            acAddress.City = familyAddress.City;
                            acAddress.State = familyAddress.State;
                            acAddress.PostalCode = familyAddress.PostalCode;
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
            FamilyAddresses.Add( new FamilyAddress { State = DefaultState, IsMailing = true } );
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
            if ( familyAddress != null )
            {
                if ( familyAddress.Id < 0 ) // was added
                {
                    familyAddress.Id = 0;
                }

                var row = gLocations.Rows[e.RowIndex];
                DropDownList ddlLocType = row.FindControl( "ddlLocType" ) as DropDownList;
                AddressControl acAddress = row.FindControl( "acAddress" ) as AddressControl;
                CheckBox cbMailing = row.FindControl( "cbMailing" ) as CheckBox;
                CheckBox cbLocation = row.FindControl( "cbLocation" ) as CheckBox;

                familyAddress.LocationTypeId = ddlLocType.SelectedValueAsInt() ?? 0;
                familyAddress.LocationTypeName = ddlLocType.SelectedItem != null ? ddlLocType.SelectedItem.Text : string.Empty;
                familyAddress.Street1 = acAddress.Street1;
                familyAddress.Street2 = acAddress.Street2;
                familyAddress.City = acAddress.City;
                familyAddress.State = acAddress.State;
                familyAddress.PostalCode = acAddress.PostalCode;
                familyAddress.Country = acAddress.Country;
                familyAddress.IsMailing = cbMailing.Checked;

                // If setting this location to be a map location, unselect all the other loctions
                if ( !familyAddress.IsLocation && cbLocation.Checked )
                {
                    foreach ( var otherAddress in FamilyAddresses )
                    {
                        otherAddress.IsLocation = false;
                    }
                }

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

                var rockContext = new RockContext();
                rockContext.WrapTransaction( () =>
                {
                    var familyService = new GroupService( rockContext );
                    var familyMemberService = new GroupMemberService( rockContext );
                    var personService = new PersonService( rockContext );
                    var historyService = new HistoryService( rockContext );

                    var familyChanges = new List<string>();

                    // SAVE FAMILY
                    _family = familyService.Get( _family.Id );

                    History.EvaluateChange( familyChanges, "Family Name", _family.Name, tbFamilyName.Text );
                    _family.Name = tbFamilyName.Text;

                    int? campusId = cpCampus.SelectedValueAsInt();
                    if ( _family.CampusId != campusId )
                    {
                        History.EvaluateChange( familyChanges, "Campus",
                            _family.CampusId.HasValue ? CampusCache.Read( _family.CampusId.Value ).Name : string.Empty,
                            campusId.HasValue ? CampusCache.Read( campusId.Value ).Name : string.Empty );
                        _family.CampusId = campusId;
                    }

                    var familyGroupTypeId = _family.GroupTypeId;

                    rockContext.SaveChanges();

                    // SAVE FAMILY MEMBERS
                    int? recordStatusValueID = ddlRecordStatus.SelectedValueAsInt();
                    int? reasonValueId = ddlReason.SelectedValueAsInt();
                    var newFamilies = new List<Group>();

                    foreach ( var familyMember in FamilyMembers )
                    {
                        var memberChanges = new List<string>();
                        var demographicChanges = new List<string>();

                        var role = familyRoles.Where( r => r.Guid.Equals( familyMember.RoleGuid ) ).FirstOrDefault();
                        if ( role == null )
                        {
                            role = familyRoles.FirstOrDefault();
                        }

                        bool isChild = role != null && role.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) );

                        // People added to family (new or from other family)
                        if ( !familyMember.ExistingFamilyMember )
                        {
                            var groupMember = new GroupMember();

                            if ( familyMember.Id == -1 )
                            {
                                // added new person
                                demographicChanges.Add( "Created" );

                                var person = new Person();
                                person.FirstName = familyMember.FirstName;
                                person.NickName = familyMember.NickName;
                                History.EvaluateChange( demographicChanges, "First Name", string.Empty, person.FirstName );

                                person.LastName = familyMember.LastName;
                                History.EvaluateChange( demographicChanges, "Last Name", string.Empty, person.LastName );

                                person.Gender = familyMember.Gender;
                                History.EvaluateChange( demographicChanges, "Gender", null, person.Gender );

                                person.BirthDate = familyMember.BirthDate;
                                History.EvaluateChange( demographicChanges, "Birth Date", null, person.BirthDate );

                                person.ConnectionStatusValueId = familyMember.ConnectionStatusValueId;
                                History.EvaluateChange( demographicChanges, "Connection Status", string.Empty, person.ConnectionStatusValueId.HasValue ? DefinedValueCache.GetName( person.ConnectionStatusValueId ) : string.Empty );

                                person.IsEmailActive = false;
                                History.EvaluateChange( demographicChanges, "Email Active", false.ToString(), ( person.IsEmailActive ?? false ).ToString() );

                                person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                                History.EvaluateChange( demographicChanges, "Record Type", string.Empty, person.RecordTypeValueId.HasValue ? DefinedValueCache.GetName( person.RecordTypeValueId.Value ) : string.Empty );

                                person.MaritalStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_UNKNOWN.AsGuid() ).Id;
                                History.EvaluateChange( demographicChanges, "Marital Status", string.Empty, person.MaritalStatusValueId.HasValue ? DefinedValueCache.GetName( person.MaritalStatusValueId ) : string.Empty );

                                if ( !isChild )
                                {
                                    person.GivingGroupId = _family.Id;
                                    History.EvaluateChange( demographicChanges, "Giving Group", string.Empty, _family.Name );
                                }

                                person.EmailPreference = EmailPreference.EmailAllowed;

                                groupMember.Person = person;
                            }
                            else
                            {
                                // added from other family
                                groupMember.Person = personService.Get( familyMember.Id );
                            }

                            if ( recordStatusValueID > 0 )
                            {
                                History.EvaluateChange( demographicChanges, "Record Status", DefinedValueCache.GetName( groupMember.Person.RecordStatusValueId ), DefinedValueCache.GetName( recordStatusValueID ) );
                                groupMember.Person.RecordStatusValueId = recordStatusValueID;

                                History.EvaluateChange( demographicChanges, "Record Status Reason", DefinedValueCache.GetName( groupMember.Person.RecordStatusReasonValueId ), DefinedValueCache.GetName( reasonValueId ) );
                                groupMember.Person.RecordStatusReasonValueId = reasonValueId;
                            }

                            groupMember.GroupId = _family.Id;
                            if ( role != null )
                            {
                                History.EvaluateChange( memberChanges, "Role", string.Empty, role.Name );
                                groupMember.GroupRoleId = role.Id;
                            }

                            if ( groupMember.Person != null )
                            {
                                familyMemberService.Add( groupMember );
                                rockContext.SaveChanges();

                                // Every person should have an alias record with same id.  If it's missing, create it
                                if ( !groupMember.Person.Aliases.Any( a => a.AliasPersonId == groupMember.Person.Id ) )
                                {
                                    var groupMemberPerson = personService.Get( groupMember.Person.Id );
                                    if ( groupMemberPerson != null )
                                    {
                                        groupMemberPerson.Aliases.Add( new PersonAlias { AliasPersonId = groupMemberPerson.Id, AliasPersonGuid = groupMemberPerson.Guid } );
                                        rockContext.SaveChanges();
                                    }
                                }

                                familyMember.Id = groupMember.Person.Id;
                            }

                        }
                        else
                        {
                            // existing family members
                            var groupMember = familyMemberService.Queryable( "Person" ).Where( m =>
                                m.PersonId == familyMember.Id &&
                                m.Group.GroupTypeId == familyGroupTypeId &&
                                m.GroupId == _family.Id ).FirstOrDefault();

                            if ( groupMember != null )
                            {
                                if ( familyMember.Removed )
                                {
                                    var newFamilyChanges = new List<string>();

                                    // Family member was removed and should be created in their own new family
                                    var newFamily = new Group();
                                    newFamily.Name = familyMember.LastName + " Family";
                                    History.EvaluateChange( newFamilyChanges, "Family", string.Empty, newFamily.Name );

                                    newFamily.GroupTypeId = familyGroupTypeId;

                                    if ( _family.CampusId.HasValue )
                                    {
                                        History.EvaluateChange( newFamilyChanges, "Campus", string.Empty, CampusCache.Read( _family.CampusId.Value ).Name );
                                    }
                                    newFamily.CampusId = _family.CampusId;

                                    familyService.Add( newFamily );
                                    rockContext.SaveChanges();

                                    // If person's previous giving group was this family, set it to their new family id
                                    if ( groupMember.Person.GivingGroup != null && groupMember.Person.GivingGroupId == _family.Id )
                                    {
                                        History.EvaluateChange( demographicChanges, "Giving Group", groupMember.Person.GivingGroup.Name, _family.Name );
                                        groupMember.Person.GivingGroupId = newFamily.Id;
                                    }

                                    groupMember.Group = newFamily;
                                    rockContext.SaveChanges();

                                    var newMemberChanges = new List<string>();
                                    History.EvaluateChange( newMemberChanges, "Role", string.Empty, groupMember.GroupRole.Name );

                                    HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                        groupMember.Person.Id, newFamilyChanges, newFamily.Name, typeof( Group ), newFamily.Id );

                                    newFamilies.Add( newFamily );

                                    History.EvaluateChange( memberChanges, "Role", groupMember.GroupRole.Name, string.Empty );
                                }
                                else
                                {
                                    // Existing member was not remvoved
                                    if ( role != null )
                                    {
                                        History.EvaluateChange( memberChanges, "Role",
                                            groupMember.GroupRole != null ? groupMember.GroupRole.Name : string.Empty, role.Name );
                                        groupMember.GroupRoleId = role.Id;

                                        if ( recordStatusValueID > 0 )
                                        {
                                            History.EvaluateChange( demographicChanges, "Record Status", DefinedValueCache.GetName( groupMember.Person.RecordStatusValueId ), DefinedValueCache.GetName( recordStatusValueID ) );
                                            groupMember.Person.RecordStatusValueId = recordStatusValueID;

                                            History.EvaluateChange( demographicChanges, "Record Status Reason", DefinedValueCache.GetName( groupMember.Person.RecordStatusReasonValueId ), DefinedValueCache.GetName( reasonValueId ) );
                                            groupMember.Person.RecordStatusReasonValueId = reasonValueId;
                                        }

                                        rockContext.SaveChanges();
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

                                // If the person's giving group id was the family they are being removed from, update it to this new family's id
                                if ( fm.Person.GivingGroupId == fm.GroupId )
                                {
                                    var person = personService.Get( fm.PersonId );

                                    History.EvaluateChange( demographicChanges, "Giving Group", person.GivingGroup.Name, _family.Name );
                                    person.GivingGroupId = _family.Id;

                                    rockContext.SaveChanges();
                                }

                                var oldMemberChanges = new List<string>();
                                History.EvaluateChange( oldMemberChanges, "Role", fm.GroupRole.Name, string.Empty );
                                History.EvaluateChange( oldMemberChanges, "Family", fm.Group.Name, string.Empty );
                                HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                    fm.Person.Id, oldMemberChanges, fm.Group.Name, typeof( Group ), fm.Group.Id );

                                familyMemberService.Delete( fm );
                                rockContext.SaveChanges();

                                var f = familyService.Queryable()
                                    .Where( g =>
                                        g.Id == otherFamilyMember.GroupId &&
                                        !g.Members.Any() )
                                    .FirstOrDefault();
                                if ( f != null )
                                {
                                    familyService.Delete( f );
                                    rockContext.SaveChanges();
                                }

                            }
                        }

                        HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                            familyMember.Id, demographicChanges );

                        HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                            familyMember.Id, memberChanges, _family.Name, typeof( Group ), _family.Id );
                    }

                    // SAVE LOCATIONS
                    var groupLocationService = new GroupLocationService( rockContext );

                    // delete any group locations that were removed
                    var remainingLocationIds = FamilyAddresses.Where( a => a.Id > 0 ).Select( a => a.Id ).ToList();
                    foreach ( var removedLocation in groupLocationService.Queryable( "GroupLocationTypeValue,Location" )
                        .Where( l => l.GroupId == _family.Id &&
                            !remainingLocationIds.Contains( l.Id ) ) )
                    {
                        History.EvaluateChange( familyChanges, removedLocation.GroupLocationTypeValue.Value + " Location",
                            removedLocation.Location.ToString(), string.Empty );
                        groupLocationService.Delete( removedLocation );
                    }
                    rockContext.SaveChanges();

                    foreach ( var familyAddress in FamilyAddresses.Where( a => a.Id >= 0 ) )
                    {
                        Location updatedAddress = null;
                        if ( familyAddress.LocationIsDirty )
                        {
                            updatedAddress = new LocationService( rockContext ).Get(
                                familyAddress.Street1, familyAddress.Street2, familyAddress.City,
                                familyAddress.State, familyAddress.PostalCode, familyAddress.Country );
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
                            groupLocationService.Add( groupLocation );
                        }

                        History.EvaluateChange( familyChanges, "Location Type",
                            groupLocation.GroupLocationTypeValueId.HasValue ? DefinedValueCache.Read( groupLocation.GroupLocationTypeValueId.Value ).Value : string.Empty,
                            familyAddress.LocationTypeName );
                        groupLocation.GroupLocationTypeValueId = familyAddress.LocationTypeId;

                        History.EvaluateChange( familyChanges, familyAddress.LocationTypeName + " Is Mailing",
                            groupLocation.IsMailingLocation.ToString(), familyAddress.IsMailing.ToString() );
                        groupLocation.IsMailingLocation = familyAddress.IsMailing;

                        History.EvaluateChange( familyChanges, familyAddress.LocationTypeName + " Is Map Location",
                            groupLocation.IsMappedLocation.ToString(), familyAddress.IsLocation.ToString() );
                        groupLocation.IsMappedLocation = familyAddress.IsLocation;

                        if ( updatedAddress != null )
                        {
                            History.EvaluateChange( familyChanges, familyAddress.LocationTypeName + " Location",
                                groupLocation.Location != null ? groupLocation.Location.ToString() : "", updatedAddress.ToString() );
                            groupLocation.Location = updatedAddress;
                        }

                        rockContext.SaveChanges();

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
                                newFamilyLocation.IsMailingLocation = groupLocation.IsMailingLocation;
                                newFamilyLocation.IsMappedLocation = groupLocation.IsMappedLocation;
                                groupLocationService.Add( newFamilyLocation );
                            }

                            rockContext.SaveChanges();
                        }
                    }

                    foreach ( var fm in _family.Members )
                    {
                        HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                            fm.PersonId, familyChanges, _family.Name, typeof( Group ), _family.Id );
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

                } );
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
            int homeLocationTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;

            // If there are not any addresses with a Map Location, set the first home location to be a mapped location
            if ( !FamilyAddresses.Any( l => l.IsLocation == true ) )
            {
                var firstHomeAddress = FamilyAddresses.Where( l => l.LocationTypeId == homeLocationTypeId ).FirstOrDefault();
                if ( firstHomeAddress != null )
                {
                    firstHomeAddress.IsLocation = true;
                }
            }

            gLocations.DataSource = FamilyAddresses;
            gLocations.DataBind();
        }

        private List<FamilyMember> GetMembersOrdered()
        {
            var orderedMembers = new List<FamilyMember>();

            // Add adult males
            orderedMembers.AddRange( FamilyMembers
                .Where( m =>
                    !m.Removed &&
                    m.RoleGuid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                    m.Gender == Gender.Male )
                .OrderByDescending( m => m.Age ) );

            // Add adult females
            orderedMembers.AddRange( FamilyMembers
                .Where( m =>
                    !m.Removed &&
                    m.RoleGuid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                    m.Gender != Gender.Male )
                .OrderByDescending( m => m.Age ) );

            // Add non-adults
            orderedMembers.AddRange( FamilyMembers
                .Where( m =>
                    !m.Removed &&
                    !m.RoleGuid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                .OrderByDescending( m => m.Age ) );

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
        public string NickName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public Guid RoleGuid { get; set; }
        public string RoleName { get; set; }
        public int? PhotoId { get; set; }
        public int? ConnectionStatusValueId { get; set; }

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

        public FamilyMember( GroupMember familyMember, bool existingFamilyMember )
        {
            if ( familyMember != null )
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
                FirstName = person.FirstName;
                NickName = person.NickName;
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
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsMailing { get; set; }
        public bool IsLocation { get; set; }

        public FamilyAddress( GroupLocation groupLocation )
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

        public FamilyAddress()
        {
            Id = -1; // Adding
            LocationIsDirty = true;

            string orgLocGuid = GlobalAttributesCache.Read().GetValue( "OrganizationAddress" );
        }

        public string FormattedAddress
        {
            get
            {
                string result = string.Format( "{0} {1} {2}, {3} {4}",
                    this.Street1, this.Street2, this.City, this.State, this.PostalCode ).ReplaceWhileExists( "  ", " " );

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
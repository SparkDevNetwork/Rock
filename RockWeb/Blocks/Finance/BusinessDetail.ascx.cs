// <copyright>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given business." )]
    public partial class BusinessDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "businessId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "businessId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Events

        protected void lbSave_Click( object sender, EventArgs e )
        {
            using ( new UnitOfWorkScope() )
            {
                RockTransactionScope.WrapTransaction( () =>
                {
                    var businessService = new PersonService();
                    Person business = null;
                    var businessId = int.Parse( hfBusinessId.Value );
                    if ( businessId == 0 )
                    {
                        business = new Person();
                    }
                    else
                    {
                        business = businessService.Get( businessId );
                    }

                    business.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                    business.RecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                    business.FirstName = tbBusinessName.Text;
                    business.Email = tbEmailAddress.Text;
                    business.IsEmailActive = true;

                    var phoneNumber = business.PhoneNumbers.FirstOrDefault();
                    if ( phoneNumber != null )
                    {
                        // The phone number already exists
                        var phoneNumberService = new PhoneNumberService();
                        business.PhoneNumbers.Remove(phoneNumber);
                        phoneNumberService.Delete(phoneNumber, CurrentPersonAlias);
                    }
                    else
                    {
                        // Need a new phone number
                        phoneNumber = new PhoneNumber();
                    }

                    phoneNumber.Number = PhoneNumber.CleanNumber( tbPhone.Text );
                    business.PhoneNumbers.Add(phoneNumber);
                } );
            }









            //var groupService = new GroupService();
            //var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            //if ( familyGroupType != null )
            //{
            //    Group businessGroup = null;
            //    if ( business.GivingGroupId != null )
            //    {
            //        businessGroup = groupService.Get( (int)business.GivingGroupId );
            //    }
            //    else
            //    {
            //        businessGroup = new Group();
            //    }
            //    businessGroup.GroupTypeId = familyGroupType.Id;
            //    businessGroup.Name = tbBusinessName.Text + " Business";
            //    businessGroup.CampusId = 1;   // **** This should be set to the value of a dropdown.
            //    GroupMember groupMember = null;
            //    if ( businessGroup.Members.Count > 0 )
            //    {
            //        groupMember = businessGroup.Members.FirstOrDefault();
            //    }
            //    else
            //    {
            //        groupMember = new GroupMember();
            //    }
            //    groupMember.Person = business;
            //    //groupMember.GroupRoleId = new GroupTypeRoleService( groupService.RockContext ).Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            //    groupMember.GroupRoleId = new GroupTypeRoleService().Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            //    if ( businessId == 0 )
            //    {
            //        businessGroup.Members.Add( groupMember );
            //        groupService.Add( businessGroup, CurrentPersonAlias );
            //    }
            //    groupService.Save( businessGroup, CurrentPersonAlias );
            //    //var personService = new PersonService( groupService.RockContext );
            //    //business = businessService.Get( groupMember.PersonId );
            //    if ( business != null )
            //    {
            //        if ( !business.Aliases.Any( a => a.AliasPersonId == business.Id ) )
            //        {
            //            business.Aliases.Add( new PersonAlias { AliasPersonId = business.Id, AliasPersonGuid = business.Guid } );
            //        }

            //        business.GivingGroupId = businessGroup.Id;
            //        if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( tbPhone.Text ) ) )
            //        {
            //            var phoneNumber = new PhoneNumber();
            //            phoneNumber.Number = PhoneNumber.CleanNumber( tbPhone.Text );
            //            if ( businessId != 0 )
            //            {
            //                var phoneNumberChanged = business.PhoneNumbers.FirstOrDefault().Number != phoneNumber.Number;
            //                if ( phoneNumberChanged )
            //                {
            //                    var oldPhoneNumber = new PhoneNumber();
            //                    oldPhoneNumber.Number = business.PhoneNumbers.FirstOrDefault().Number;
            //                    business.PhoneNumbers.Remove( oldPhoneNumber );
            //                }
            //            }
            //            business.PhoneNumbers.Add( phoneNumber );
            //        }
            //        businessService.Save( business, CurrentPersonAlias );
            //    }

            //    // Save the address
            //    if ( businessId > 0 )
            //    {
            //        var locationService = new LocationService();
            //        var businessAddress = locationService.Get( businessGroup.GroupLocations.Where( a => a.GroupId == businessGroup.Id ).FirstOrDefault().Location.Id );
            //        var groupLocation = new GroupLocation();
            //        groupLocation.Location = businessAddress;
            //        businessGroup.GroupLocations.Remove( groupLocation );
            //    }

            //    groupService.AddNewFamilyAddress( businessGroup, GetAttributeValue( "LocationType" ),
            //        tbStreet1.Text, tbStreet2.Text, tbCity.Text, ddlState.SelectedValue, tbZipCode.Text, CurrentPersonAlias );
            //}

            NavigateToParentPage();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( hfBusinessId.ValueAsInt() != 0 )
            {
                var savedBusiness = new PersonService().Get( hfBusinessId.ValueAsInt() );
                ShowSummary( savedBusiness );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var business = new PersonService().Get( int.Parse( hfBusinessId.Value ) );
            ShowEditDetails( business );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            pnlDetails.Visible = false;

            if ( !itemKey.Equals( "businessId" ) )
            {
                return;
            }

            bool editAllowed = true;

            Person business = null;     // A business is a person

            if ( !itemKeyValue.Equals( 0 ) )
            {
                business = new PersonService().Get( itemKeyValue );
                editAllowed = business.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
            else
            {
                business = new Person { Id = 0 };
            }

            if ( business == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfBusinessId.Value = business.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Person.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowSummary( business );
            }
            else
            {
                if ( business.Id > 0 )
                {
                    ShowSummary( business );
                }
                else
                {
                    ShowEditDetails( business );
                }
            }
        }

        private void ShowSummary( Person business )
        {
            SetEditMode( false );
            hfBusinessId.SetValue( business.Id );
            lTitle.Text = "View Business".FormatAsHtmlTitle();

            var groupService = new GroupService();
            var businessGroup = groupService.Get( (int)business.GivingGroupId );
            var locationService = new LocationService();
            var businessAddress = locationService.Get( businessGroup.GroupLocations.Where( a => a.GroupId == businessGroup.Id ).FirstOrDefault().Location.Id );
            var phoneNumberService = new PhoneNumberService();
            var businessPhone = phoneNumberService.GetByPersonId( business.Id ).FirstOrDefault();

            lDetailsLeft.Text = new DescriptionList()
                .Add( "Title", business.FirstName )
                .Add( "Address Line 1", businessAddress.Street1 )
                .Add( "Address Line 2", businessAddress.Street2 )
                .Add( "City", businessAddress.City )
                .Add( "State", businessAddress.State )
                .Add( "Zip Code", businessAddress.Zip )
                .Html;

            lDetailsRight.Text = new DescriptionList()
                .Add( "Phone Number", businessPhone.NumberFormatted )
                .Add( "Email Address", business.Email )
                .Html;
        }

        private void ShowEditDetails( Person business )
        {
            if ( business.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( business.FullName ).FormatAsHtmlTitle();
                var groupService = new GroupService();
                var businessGroup = groupService.Get( (int)business.GivingGroupId );
                var locationService = new LocationService();
                var businessAddress = locationService.Get( businessGroup.GroupLocations.Where( a => a.GroupId == businessGroup.Id ).FirstOrDefault().Location.Id );
                var phoneNumberService = new PhoneNumberService();
                var businessPhone = phoneNumberService.GetByPersonId( business.Id ).FirstOrDefault();

                tbBusinessName.Text = business.FirstName;
                tbStreet1.Text = businessAddress.Street1;
                tbStreet2.Text = businessAddress.Street2;
                tbCity.Text = businessAddress.City;
                ddlState.SelectedValue = businessAddress.State;
                tbZipCode.Text = businessAddress.Zip;
                tbPhone.Text = businessPhone.NumberFormatted;
                tbEmailAddress.Text = business.Email;
            }
            else
            {
                lTitle.Text = ActionTitle.Add( "Business" ).FormatAsHtmlTitle();
            }

            SetEditMode( true );
        }

        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        #endregion
}
}
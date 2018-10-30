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

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Convert Business" )]
    [Category( "Finance" )]
    [Description( "Allows you to convert a Person record into a Business and vice-versa." )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The default connection status to use when converting a business to a person.", false, order: 0 )]
    public partial class ConvertBusiness : RockBlock
    {
        #region Base Method Overrides

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
            }
        }

        #endregion

        #region Core Methods

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            pnlToBusiness.Visible = false;
            pnlToPerson.Visible = false;
            ppSource.SetValue( null );
        }

        /// <summary>
        /// Handles the SelectPerson event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppSource_SelectPerson( object sender, EventArgs e )
        {
            nbSuccess.Text = string.Empty;
            nbWarning.Text = string.Empty;
            nbError.Text = string.Empty;
            pnlToBusiness.Visible = false;
            pnlToPerson.Visible = false;
            
            if ( ppSource.PersonId.HasValue )
            {
                var person = new PersonService( new RockContext() ).Get( ppSource.PersonId.Value );

                if ( person.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    pnlToPerson.Visible = true;
                    tbPersonFirstName.Text = string.Empty;
                    tbPersonLastName.Text = person.LastName;
                    dvpPersonConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Id;
                    dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
                    rblGender.BindToEnum<Gender>();

                    rblGender.SetValue( Gender.Unknown.ConvertToInt() );

                    if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "DefaultConnectionStatus" ) ) )
                    {
                        var dv = DefinedValueCache.Get( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid() );
                        if ( dv != null )
                        {
                            dvpPersonConnectionStatus.SetValue( dv.Id );
                        }
                    }
                }
                else
                {
                    //
                    // Ensure person record is a member of only one family.
                    //
                    var families = person.GetFamilies();
                    if ( families.Count() != 1 )
                    {
                        nbError.Heading = "Cannot convert person record to a business.";
                        nbError.Text = "To avoid data loss move this person to one family before proceeding.";
                        return;
                    }

                    //
                    // Ensure person record is only record in family.
                    //
                    var family = families.First();
                    if ( family.Members.Count != 1 )
                    {
                        nbError.Heading = "Cannot convert person record to a business.";
                        nbError.Text = "To avoid data loss move this person to their own family before proceeding.";
                        return;
                    }

                    //
                    // Ensure giving group is correct.
                    //
                    if ( person.GivingGroup == null || person.GivingGroup.Members.Count != 1 || person.GivingLeaderId != person.Id )
                    {
                        nbError.Heading = "Cannot convert person record to a business.";
                        nbError.Text = "Please fix the giving group and then try again.";
                        return;
                    }

                    pnlToBusiness.Visible = true;
                    tbBusinessName.Text = string.Format( "{0} {1}", person.FirstName, person.LastName ).Trim();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbToPersonSave_Click( object sender, EventArgs e )
        {
            var context = new RockContext();
            var person = new PersonService( context ).Get( ppSource.PersonId.Value );
            person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id;
            person.ConnectionStatusValueId = dvpPersonConnectionStatus.SelectedValueAsInt();
            person.FirstName = tbPersonFirstName.Text.Trim();
            person.NickName = tbPersonFirstName.Text.Trim();
            person.LastName = tbPersonLastName.Text.Trim();
            person.Gender = rblGender.SelectedValueAsEnum<Gender>();
            person.MaritalStatusValueId = dvpMaritalStatus.SelectedValueAsId();

            context.SaveChanges();

            ppSource.SetValue( null );

            var parameters = new Dictionary<string, string>
            {
                { "PersonId", person.Id.ToString() }
            };
            var pageRef = new Rock.Web.PageReference( Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES, parameters );
            nbSuccess.Text = string.Format( "The business formerly known as <a href='{1}'>{0}</a> has been converted to a person.", person.FullName, pageRef.BuildUrl() );
            
            pnlToPerson.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbToBusinessSave_Click( object sender, EventArgs e )
        {
            var context = new RockContext();
            var person = new PersonService( context ).Get( ppSource.PersonId.Value );

            person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS ).Id;
            person.ConnectionStatusValueId = null;
            person.TitleValueId = null;
            person.FirstName = null;
            person.NickName = null;
            person.MiddleName = null;
            person.LastName = tbBusinessName.Text.Trim();
            person.SuffixValueId = null;
            person.SetBirthDate( null );
            person.Gender = Gender.Unknown;
            person.MaritalStatusValueId = null;
            person.AnniversaryDate = null;
            person.GraduationYear = null;

            //
            // Check address(es) and make sure one is of type Work.
            //
            var family = person.GetFamily( context );
            if ( family.GroupLocations.Count > 0 )
            {
                var workLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK ).Id;
                var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME ).Id;

                var workLocation = family.GroupLocations.Where( gl => gl.GroupLocationTypeValueId == workLocationTypeId ).FirstOrDefault();
                if ( workLocation == null )
                {
                    var homeLocation = family.GroupLocations.Where( gl => gl.GroupLocationTypeValueId == homeLocationTypeId ).FirstOrDefault();
                    if ( homeLocation != null )
                    {
                        homeLocation.GroupLocationTypeValueId = workLocationTypeId;
                    }
                }
            }

            //
            // Check phone(es) and make sure one is of type Work.
            //
            if ( person.PhoneNumbers.Count > 0 )
            {
                var workPhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ).Id;
                var homePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ).Id;

                var workPhone = person.PhoneNumbers.Where( pn => pn.NumberTypeValueId == workPhoneTypeId ).FirstOrDefault();
                if ( workPhone == null )
                {
                    var homePhone = person.PhoneNumbers.Where( pn => pn.NumberTypeValueId == homePhoneTypeId ).FirstOrDefault();
                    if ( homePhone != null )
                    {
                        homePhone.NumberTypeValueId = workPhoneTypeId;
                    }
                }
            }

            //
            // Make sure member status in family is set to Adult.
            //
            var adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles
                .Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                .Select( a => a.Id ).First();
            family.Members.Where( m => m.PersonId == person.Id ).First().GroupRoleId = adultRoleId;

            context.SaveChanges();

            ppSource.SetValue( null );

            var parameters = new Dictionary<string, string>
            {
                { "BusinessId", person.Id.ToString() }
            };
            var pageRef = new Rock.Web.PageReference( Rock.SystemGuid.Page.BUSINESS_DETAIL, parameters );
            nbSuccess.Text = string.Format( "The person formerly known as <a href='{1}'>{0}</a> has been converted to a business.", person.LastName, pageRef.BuildUrl() );

            pnlToBusiness.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPersonCancel_Click( object sender, EventArgs e )
        {
            pnlToPerson.Visible = false;
            ppSource.SetValue( null );
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBusinessCancel_Click( object sender, EventArgs e )
        {
            pnlToBusiness.Visible = false;
            ppSource.SetValue( null );
        }

        #endregion
    }
}
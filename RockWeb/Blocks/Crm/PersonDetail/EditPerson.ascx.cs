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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Data;

/*******************************************************************************************************************************
 * NOTE: The Security/EditMyAccount.ascx block has very similiar functionality.  If updating this block, make sure to check
 * that block also.  It may need the same updates.
 *******************************************************************************************************************************/

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile block the main information about a peron 
    /// </summary>
    [DisplayName( "Edit Person" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to edit a person." )]
    public partial class EditPerson : Rock.Web.UI.PersonBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ddlTitle.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ), true );
            ddlSuffix.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ), true );
            rblMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ) );
            rblConnectionStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ) );
            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ) );
            ddlReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ), true );

            ddlGivingGroup.Items.Clear();
            ddlGivingGroup.Items.Add( new ListItem( None.Text, None.IdValue ) );
            if ( Person != null )
            {
                var personService = new PersonService( new RockContext() );
                foreach ( var family in personService.GetFamilies( Person.Id ).Select( a => new { a.Name, a.Id } ) )
                {
                    ddlGivingGroup.Items.Add( new ListItem( family.Name, family.Id.ToString() ) );
                }
            }

            ScriptManager.RegisterStartupScript( ddlGradePicker, ddlGradePicker.GetType(), "grade-selection-" + BlockId.ToString(), ddlGradePicker.GetJavascriptForYearPicker(ypGraduation), true );

            string smsScript = @"
    $('.js-sms-number').click(function () {
        if ($(this).is(':checked')) {
            $('.js-sms-number').not($(this)).prop('checked', false);
        }
    });
";
            btnSave.Visible = IsUserAuthorized( Rock.Security.Authorization.EDIT );

            ScriptManager.RegisterStartupScript( rContactInfo, rContactInfo.GetType(), "sms-number-" + BlockId.ToString(), smsScript, true );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && Person != null )
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRecordStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlRecordStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlReason.Visible = ( ddlRecordStatus.SelectedValueAsInt() == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                var rockContext = new RockContext();

                rockContext.WrapTransaction( () =>
                {
                    var personService = new PersonService( rockContext );

                    var changes = new List<string>();

                    var person = personService.Get( Person.Id );

                    int? orphanedPhotoId = null;
                    if ( person.PhotoId != imgPhoto.BinaryFileId )
                    {
                        orphanedPhotoId = person.PhotoId;
                        person.PhotoId = imgPhoto.BinaryFileId;

                        if ( orphanedPhotoId.HasValue )
                        {
                            if ( person.PhotoId.HasValue )
                            {
                                changes.Add( "Modified the photo." );
                            }
                            else
                            {
                                changes.Add( "Deleted the photo." );
                            }
                        }
                        else if ( person.PhotoId.HasValue )
                        {
                            changes.Add( "Added a photo." );
                        }
                    }

                    int? newTitleId = ddlTitle.SelectedValueAsInt();
                    History.EvaluateChange( changes, "Title", DefinedValueCache.GetName( person.TitleValueId ), DefinedValueCache.GetName( newTitleId ) );
                    person.TitleValueId = newTitleId;

                    History.EvaluateChange( changes, "First Name", person.FirstName, tbFirstName.Text );
                    person.FirstName = tbFirstName.Text;

                    string nickName = string.IsNullOrWhiteSpace( tbNickName.Text ) ? tbFirstName.Text : tbNickName.Text;
                    History.EvaluateChange( changes, "Nick Name", person.NickName, nickName );
                    person.NickName = tbNickName.Text;

                    History.EvaluateChange( changes, "Middle Name", person.MiddleName, tbMiddleName.Text );
                    person.MiddleName = tbMiddleName.Text;

                    History.EvaluateChange( changes, "Last Name", person.LastName, tbLastName.Text );
                    person.LastName = tbLastName.Text;

                    int? newSuffixId = ddlSuffix.SelectedValueAsInt();
                    History.EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( person.SuffixValueId ), DefinedValueCache.GetName( newSuffixId ) );
                    person.SuffixValueId = newSuffixId;

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

                    History.EvaluateChange( changes, "Birth Month", birthMonth, person.BirthMonth );
                    History.EvaluateChange( changes, "Birth Day", birthDay, person.BirthDay );
                    History.EvaluateChange( changes, "Birth Year", birthYear, person.BirthYear );

                    int? graduationYear = null;
                    if ( ypGraduation.SelectedYear.HasValue )
                    {
                        graduationYear = ypGraduation.SelectedYear.Value;
                    }
                    History.EvaluateChange( changes, "Graduation Year", person.GraduationYear, graduationYear );
                    person.GraduationYear = graduationYear;

                    History.EvaluateChange( changes, "Anniversary Date", person.AnniversaryDate, dpAnniversaryDate.SelectedDate );
                    person.AnniversaryDate = dpAnniversaryDate.SelectedDate;

                    var newGender = rblGender.SelectedValue.ConvertToEnum<Gender>();
                    History.EvaluateChange( changes, "Gender", person.Gender, newGender );
                    person.Gender = newGender;

                    int? newMaritalStatusId = rblMaritalStatus.SelectedValueAsInt();
                    History.EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( newMaritalStatusId ) );
                    person.MaritalStatusValueId = newMaritalStatusId;

                    int? newConnectionStatusId = rblConnectionStatus.SelectedValueAsInt();
                    History.EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( person.ConnectionStatusValueId ), DefinedValueCache.GetName( newConnectionStatusId ) );
                    person.ConnectionStatusValueId = newConnectionStatusId;

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

                                    History.EvaluateChange( changes,
                                        string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumberTypeId ) ),
                                        oldPhoneNumber, phoneNumber.NumberFormattedWithCountryCode );
                                }
                            }
                        }
                    }

                    // Remove any blank numbers
                    var phoneNumberService = new PhoneNumberService( rockContext );
                    foreach ( var phoneNumber in person.PhoneNumbers
                        .Where( n => n.NumberTypeValueId.HasValue && !phoneNumberTypeIds.Contains( n.NumberTypeValueId.Value ) )
                        .ToList() )
                    {
                        History.EvaluateChange( changes,
                            string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumber.NumberTypeValueId ) ),
                            phoneNumber.ToString(), string.Empty );

                        person.PhoneNumbers.Remove( phoneNumber );
                        phoneNumberService.Delete( phoneNumber );
                    }

                    History.EvaluateChange( changes, "Email", person.Email, tbEmail.Text );
                    person.Email = tbEmail.Text.Trim();

                    History.EvaluateChange( changes, "Email Active", ( person.IsEmailActive ?? true ), cbIsEmailActive.Checked );
                    person.IsEmailActive = cbIsEmailActive.Checked;

                    var newEmailPreference = rblEmailPreference.SelectedValue.ConvertToEnum<EmailPreference>();
                    History.EvaluateChange( changes, "Email Preference", person.EmailPreference, newEmailPreference );
                    person.EmailPreference = newEmailPreference;

                    int? newGivingGroupId = ddlGivingGroup.SelectedValueAsId();
                    if ( person.GivingGroupId != newGivingGroupId )
                    {
                        string oldGivingGroupName = person.GivingGroup != null ? person.GivingGroup.Name : string.Empty;
                        string newGivingGroupName = newGivingGroupId.HasValue ? ddlGivingGroup.Items.FindByValue( newGivingGroupId.Value.ToString() ).Text : string.Empty;
                        History.EvaluateChange( changes, "Giving Group", oldGivingGroupName, newGivingGroupName );
                    }

                    person.GivingGroupId = newGivingGroupId;

                    int? newRecordStatusId = ddlRecordStatus.SelectedValueAsInt();
                    History.EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( person.RecordStatusValueId ), DefinedValueCache.GetName( newRecordStatusId ) );
                    person.RecordStatusValueId = newRecordStatusId;

                    int? newRecordStatusReasonId = null;
                    if ( person.RecordStatusValueId.HasValue && person.RecordStatusValueId.Value == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id )
                    {
                        newRecordStatusReasonId = ddlReason.SelectedValueAsInt();
                    }
                    History.EvaluateChange( changes, "Inactive Reason", DefinedValueCache.GetName( person.RecordStatusReasonValueId ), DefinedValueCache.GetName( newRecordStatusReasonId ) );
                    person.RecordStatusReasonValueId = newRecordStatusReasonId;

                    if ( person.IsValid )
                    {
                        if ( rockContext.SaveChanges() > 0 )
                        {
                            if ( changes.Any() )
                            {
                                HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                    Person.Id, changes );
                            }
                            if ( orphanedPhotoId.HasValue )
                            {
                                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                                var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                                if ( binaryFile != null )
                                {
                                    binaryFileService.Delete( binaryFile );
                                    rockContext.SaveChanges();
                                }
                            }

                        }

                        Response.Redirect( string.Format( "~/Person/{0}", Person.Id ), false );

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

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {

            lTitle.Text = String.Format( "Edit: {0}", Person.FullName ).FormatAsHtmlTitle();

            imgPhoto.BinaryFileId = Person.PhotoId;
            imgPhoto.NoPictureUrl = Person.GetPhotoUrl( null, Person.Age, Person.Gender );

            ddlTitle.SelectedValue = Person.TitleValueId.HasValue ? Person.TitleValueId.Value.ToString() : string.Empty;
            tbFirstName.Text = Person.FirstName;
            tbNickName.Text = string.IsNullOrWhiteSpace( Person.NickName ) ? "" : ( Person.NickName.Equals( Person.FirstName, StringComparison.OrdinalIgnoreCase ) ? "" : Person.NickName );
            tbMiddleName.Text = Person.MiddleName;
            tbLastName.Text = Person.LastName;
            ddlSuffix.SelectedValue = Person.SuffixValueId.HasValue ? Person.SuffixValueId.Value.ToString() : string.Empty;
            bpBirthDay.SelectedDate = Person.BirthDate;

            if ( Person.GraduationYear.HasValue )
            {
                ypGraduation.SelectedYear = Person.GraduationYear.Value;
            }
            else
            {
                ypGraduation.SelectedYear = null;
            }

            if ( !Person.HasGraduated ?? false )
            {
                int gradeOffset = Person.GradeOffset.Value;
                var maxGradeOffset = ddlGradePicker.MaxGradeOffset; ;
                
                // keep trying until we find a Grade that has a gradeOffset that that includes the Person's gradeOffset (for example, there might be combined grades)
                while ( !ddlGradePicker.Items.OfType<ListItem>().Any( a => a.Value.AsInteger() == gradeOffset ) && gradeOffset <= maxGradeOffset )
                {
                    gradeOffset++;
                }

                ddlGradePicker.SetValue( gradeOffset );
            }
            else
            {
                ddlGradePicker.SelectedIndex = 0;
            }

            dpAnniversaryDate.SelectedDate = Person.AnniversaryDate;
            rblGender.SelectedValue = Person.Gender.ConvertToString( false );
            rblMaritalStatus.SelectedValue = Person.MaritalStatusValueId.HasValue ? Person.MaritalStatusValueId.Value.ToString() : string.Empty;
            rblConnectionStatus.SelectedValue = Person.ConnectionStatusValueId.HasValue ? Person.ConnectionStatusValueId.Value.ToString() : string.Empty;
            tbEmail.Text = Person.Email;
            cbIsEmailActive.Checked = Person.IsEmailActive ?? true;
            rblEmailPreference.SelectedValue = Person.EmailPreference.ConvertToString( false );

            ddlRecordStatus.SelectedValue = Person.RecordStatusValueId.HasValue ? Person.RecordStatusValueId.Value.ToString() : string.Empty;
            ddlReason.SelectedValue = Person.RecordStatusReasonValueId.HasValue ? Person.RecordStatusReasonValueId.Value.ToString() : string.Empty;
            ddlReason.Visible = Person.RecordStatusReasonValueId.HasValue &&
                Person.RecordStatusValueId.Value == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;

            var mobilePhoneType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );

            var phoneNumbers = new List<PhoneNumber>();
            var phoneNumberTypes = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
            if ( phoneNumberTypes.DefinedValues.Any() )
            {
                foreach ( var phoneNumberType in phoneNumberTypes.DefinedValues )
                {
                    var phoneNumber = Person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberType.Id );
                    if ( phoneNumber == null )
                    {
                        var numberType = new DefinedValue();
                        numberType.Id = phoneNumberType.Id;
                        numberType.Value = phoneNumberType.Value;

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

            ddlGivingGroup.SetValue( Person.GivingGroupId );

        }
    }
}
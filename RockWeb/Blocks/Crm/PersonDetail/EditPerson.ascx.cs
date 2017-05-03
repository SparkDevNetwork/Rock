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
using Newtonsoft.Json;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

/*******************************************************************************************************************************
 * NOTE: The Security/AccountEdit.ascx block has very similar functionality.  If updating this block, make sure to check
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
            ddlMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ), true );
            ddlConnectionStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ), true );
            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ) );
            ddlReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ), true );

            ddlGivingGroup.Items.Clear();
            ddlGivingGroup.Items.Add( new ListItem( None.Text, None.IdValue ) );
            if ( Person != null )
            {
                var personService = new PersonService( new RockContext() );
                foreach ( var family in personService.GetFamilies( Person.Id ).Select( a => new { a.Name, a.Id, a.Members } ) )
                {
                    string familyNameWithFirstNames = GetFamilyNameWithFirstNames( family.Name, family.Members );
                    ddlGivingGroup.Items.Add( new ListItem( familyNameWithFirstNames, family.Id.ToString() ) );
                }
            }

            ScriptManager.RegisterStartupScript( ddlGradePicker, ddlGradePicker.GetType(), "grade-selection-" + BlockId.ToString(), ddlGradePicker.GetJavascriptForYearPicker( ypGraduation ), true );

            string smsScript = @"
    $('.js-sms-number').click(function () {
        if ($(this).is(':checked')) {
            $('.js-sms-number').not($(this)).prop('checked', false);
        }
    });
";
            btnSave.Visible = IsUserAuthorized( Rock.Security.Authorization.EDIT );

            ScriptManager.RegisterStartupScript( rContactInfo, rContactInfo.GetType(), "sms-number-" + BlockId.ToString(), smsScript, true );

            grdPreviousNames.Actions.ShowAdd = true;
            grdPreviousNames.Actions.AddClick += grdPreviousNames_AddClick;
        }

        /// <summary>
        /// Gets the family name with first names.
        /// </summary>
        /// <param name="familyName">Name of the family.</param>
        /// <param name="familyMembers">The family members.</param>
        /// <returns></returns>
        private string GetFamilyNameWithFirstNames( string familyName, ICollection<GroupMember> familyMembers )
        {
            var adultFirstNames = familyMembers.Where( a => a.GroupRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).OrderBy( a => a.Person.Gender ).ThenBy( a => a.Person.NickName ).Select( a => a.Person.NickName ?? a.Person.FirstName ).ToList();
            var otherFirstNames = familyMembers.Where( a => a.GroupRole.Guid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).OrderBy( a => a.Person.Gender ).ThenBy( a => a.Person.NickName ).Select( a => a.Person.NickName ?? a.Person.FirstName ).ToList();
            var firstNames = new List<string>();
            firstNames.AddRange( adultFirstNames );
            firstNames.AddRange( otherFirstNames );
            string familyNameWithFirstNames;
            if ( firstNames.Any() )
            {
                familyNameWithFirstNames = string.Format( "{0} ({1})", familyName, firstNames.AsDelimited( ", ", " and " ) );
            }
            else
            {
                familyNameWithFirstNames = string.Format( "{0} (no family members)", familyName );
            }
            return familyNameWithFirstNames;
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

        #region View State related stuff

        /// <summary>
        /// Gets or sets the state of the person previous names.
        /// </summary>
        /// <value>
        /// The state of the person previous names.
        /// </value>
        private List<PersonPreviousName> PersonPreviousNamesState { get; set; }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["PersonPreviousNamesState"] as string;

            if ( string.IsNullOrWhiteSpace( json ) )
            {
                PersonPreviousNamesState = new List<PersonPreviousName>();
            }
            else
            {
                PersonPreviousNamesState = PersonPreviousName.FromJsonAsList( json ) ?? new List<PersonPreviousName>();
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
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["PersonPreviousNamesState"] = JsonConvert.SerializeObject( PersonPreviousNamesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRecordStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlRecordStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            bool showInactiveReason = ( ddlRecordStatus.SelectedValueAsInt() == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id );

            ddlReason.Visible = showInactiveReason;
            tbInactiveReasonNote.Visible = showInactiveReason;
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

                    int? newMaritalStatusId = ddlMaritalStatus.SelectedValueAsInt();
                    History.EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( newMaritalStatusId ) );
                    person.MaritalStatusValueId = newMaritalStatusId;

                    int? newConnectionStatusId = ddlConnectionStatus.SelectedValueAsInt();
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

                                    History.EvaluateChange(
                                        changes,
                                        string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumberTypeId ) ),
                                        oldPhoneNumber,
                                        phoneNumber.NumberFormattedWithCountryCode );
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
                        History.EvaluateChange(
                            changes,
                            string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumber.NumberTypeValueId ) ),
                            phoneNumber.ToString(),
                            string.Empty );

                        person.PhoneNumbers.Remove( phoneNumber );
                        phoneNumberService.Delete( phoneNumber );
                    }

                    History.EvaluateChange( changes, "Email", person.Email, tbEmail.Text );
                    person.Email = tbEmail.Text.Trim();

                    History.EvaluateChange( changes, "Email Active", person.IsEmailActive, cbIsEmailActive.Checked );
                    person.IsEmailActive = cbIsEmailActive.Checked;

                    var newEmailPreference = rblEmailPreference.SelectedValue.ConvertToEnum<EmailPreference>();
                    History.EvaluateChange( changes, "Email Preference", person.EmailPreference, newEmailPreference );
                    person.EmailPreference = newEmailPreference;

                    int? newGivingGroupId = ddlGivingGroup.SelectedValueAsId();
                    if ( person.GivingGroupId != newGivingGroupId )
                    {
                        string oldGivingGroupName = string.Empty;
                        if ( Person.GivingGroup != null )
                        {
                            oldGivingGroupName = GetFamilyNameWithFirstNames( Person.GivingGroup.Name, Person.GivingGroup.Members );
                        }
                        
                        string newGivingGroupName = newGivingGroupId.HasValue ? ddlGivingGroup.Items.FindByValue( newGivingGroupId.Value.ToString() ).Text : string.Empty;
                        History.EvaluateChange( changes, "Giving Group", oldGivingGroupName, newGivingGroupName );
                    }

                    // Save the Envelope Number attribute if it exists and has changed
                    var personGivingEnvelopeAttribute = AttributeCache.Read( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() );
                    if ( GlobalAttributesCache.Read().EnableGivingEnvelopeNumber && personGivingEnvelopeAttribute != null )
                    {
                        if ( person.Attributes == null )
                        {
                            person.LoadAttributes( rockContext );
                        }

                        var newEnvelopeNumber = tbGivingEnvelopeNumber.Text;
                        var oldEnvelopeNumber = person.GetAttributeValue( personGivingEnvelopeAttribute.Key );
                        if ( newEnvelopeNumber != oldEnvelopeNumber )
                        {
                            // If they haven't already comfirmed about duplicate, see if the envelope number if assigned to somebody else
                            if ( !string.IsNullOrWhiteSpace( newEnvelopeNumber ) && hfGivingEnvelopeNumberConfirmed.Value != newEnvelopeNumber )
                            {
                                var otherPersonIdsWithEnvelopeNumber = new AttributeValueService( rockContext ).Queryable()
                                    .Where( a => a.AttributeId == personGivingEnvelopeAttribute.Id && a.Value == newEnvelopeNumber && a.EntityId != person.Id )
                                    .Select( a => a.EntityId );
                                if ( otherPersonIdsWithEnvelopeNumber.Any() )
                                {
                                    var personList = new PersonService( rockContext ).Queryable().Where( a => otherPersonIdsWithEnvelopeNumber.Contains( a.Id ) ).AsNoTracking().ToList();
                                    string personListMessage = personList.Select( a => a.FullName ).ToList().AsDelimited( ", ", " and " );
                                    int maxCount = 5;
                                    if ( personList.Count > maxCount )
                                    {
                                        var otherCount = personList.Count() - maxCount;
                                        personListMessage = personList.Select( a => a.FullName ).Take( 10 ).ToList().AsDelimited( ", " ) + " and " + otherCount.ToString() + " other " + "person".PluralizeIf( otherCount > 1 );
                                    }

                                    string givingEnvelopeWarningText = string.Format(
                                        "The envelope #{0} is already assigned to {1}. Do you want to also assign this number to {2}?",
                                        newEnvelopeNumber,
                                        personListMessage,
                                        person.FullName );

                                    string givingEnvelopeWarningScriptFormat = @"
                                        Rock.dialogs.confirm('{0}', function (result) {{
                                            if ( result )
                                                {{
                                                   $('#{1}').val('{2}');
                                                }}
                                        }})";

                                    string givingEnvelopeWarningScript = string.Format(
                                        givingEnvelopeWarningScriptFormat,
                                        givingEnvelopeWarningText,
                                        hfGivingEnvelopeNumberConfirmed.ClientID,
                                        newEnvelopeNumber );

                                    ScriptManager.RegisterStartupScript( hfGivingEnvelopeNumberConfirmed, hfGivingEnvelopeNumberConfirmed.GetType(), "confirm-envelope-number", givingEnvelopeWarningScript, true );
                                    return;
                                }
                            }

                            History.EvaluateChange( changes, "Giving Envelope Number", oldEnvelopeNumber, newEnvelopeNumber );
                            person.SetAttributeValue( personGivingEnvelopeAttribute.Key, newEnvelopeNumber );
                        }
                    }

                    person.GivingGroupId = newGivingGroupId;

                    bool recordStatusChangedToOrFromInactive = false;
                    var recordStatusInactiveId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;

                    int? newRecordStatusId = ddlRecordStatus.SelectedValueAsInt();
                    // Is the person's record status changing?
                    if ( person.RecordStatusValueId.HasValue && person.RecordStatusValueId != newRecordStatusId )
                    {
                        //  If it was inactive OR if the new status is inactive, flag this for use later below.
                        if ( person.RecordStatusValueId == recordStatusInactiveId || newRecordStatusId == recordStatusInactiveId )
                        {
                            recordStatusChangedToOrFromInactive = true;
                        }
                    }

                    History.EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( person.RecordStatusValueId ), DefinedValueCache.GetName( newRecordStatusId ) );
                    person.RecordStatusValueId = newRecordStatusId;

                    int? newRecordStatusReasonId = null;
                    if ( person.RecordStatusValueId.HasValue && person.RecordStatusValueId.Value == recordStatusInactiveId )
                    {
                        newRecordStatusReasonId = ddlReason.SelectedValueAsInt();
                    }

                    History.EvaluateChange( changes, "Inactive Reason", DefinedValueCache.GetName( person.RecordStatusReasonValueId ), DefinedValueCache.GetName( newRecordStatusReasonId ) );
                    person.RecordStatusReasonValueId = newRecordStatusReasonId;
                    History.EvaluateChange( changes, "Inactive Reason Note", person.InactiveReasonNote, tbInactiveReasonNote.Text );
                    person.InactiveReasonNote = tbInactiveReasonNote.Text.Trim();

                    // Save any Removed/Added Previous Names
                    var personPreviousNameService = new PersonPreviousNameService( rockContext );
                    var databasePreviousNames = personPreviousNameService.Queryable().Where( a => a.PersonAlias.PersonId == person.Id ).ToList();
                    foreach ( var deletedPreviousName in databasePreviousNames.Where( a => !PersonPreviousNamesState.Any( p => p.Guid == a.Guid ) ) )
                    {
                        personPreviousNameService.Delete( deletedPreviousName );

                        History.EvaluateChange(
                            changes,
                            "Previous Name",
                            deletedPreviousName.ToString(),
                            string.Empty );
                    }

                    foreach ( var addedPreviousName in PersonPreviousNamesState.Where( a => !databasePreviousNames.Any( d => d.Guid == a.Guid ) ) )
                    {
                        addedPreviousName.PersonAliasId = person.PrimaryAliasId.Value;
                        personPreviousNameService.Add( addedPreviousName );

                        History.EvaluateChange(
                            changes,
                            "Previous Name",
                            string.Empty,
                            addedPreviousName.ToString() );
                    }

                    if ( person.IsValid )
                    {
                        var saveChangeResult = rockContext.SaveChanges();

                        // if AttributeValues where loaded and set (for example Giving Envelope Number), Save Attribute Values
                        if ( person.AttributeValues != null )
                        {
                            person.SaveAttributeValues( rockContext );
                        }

                        if ( saveChangeResult > 0 )
                        {
                            if ( changes.Any() )
                            {
                                HistoryService.SaveChanges(
                                    rockContext,
                                    typeof( Person ),
                                    Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                    Person.Id,
                                    changes );
                            }

                            if ( orphanedPhotoId.HasValue )
                            {
                                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                                var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                                if ( binaryFile != null )
                                {
                                    string errorMessage;
                                    if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                                    {
                                        binaryFileService.Delete( binaryFile );
                                        rockContext.SaveChanges();
                                    }
                                }
                            }

                            // if they used the ImageEditor, and cropped it, the uncropped file is still in BinaryFile. So clean it up
                            if ( imgPhoto.CropBinaryFileId.HasValue )
                            {
                                if ( imgPhoto.CropBinaryFileId != person.PhotoId )
                                {
                                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                                    var binaryFile = binaryFileService.Get( imgPhoto.CropBinaryFileId.Value );
                                    if ( binaryFile != null && binaryFile.IsTemporary )
                                    {
                                        string errorMessage;
                                        if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                                        {
                                            binaryFileService.Delete( binaryFile );
                                            rockContext.SaveChanges();
                                        }
                                    }
                                }
                            }

                            // If the person's record status was changed to or from inactive,
                            // we need to check if any of their families need to be activated or inactivated.
                            if ( recordStatusChangedToOrFromInactive )
                            {
                                foreach ( var family in personService.GetFamilies( person.Id ) )
                                {
                                    // Are there any more members of the family who are NOT inactive?
                                    // If not, mark the whole family inactive.
                                    if ( !family.Members.Where( m => m.Person.RecordStatusValueId != recordStatusInactiveId ).Any() )
                                    {
                                        family.IsActive = false;
                                    }
                                    else
                                    {
                                        family.IsActive = true;
                                    }
                                }

                                rockContext.SaveChanges();
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
            lTitle.Text = string.Format( "Edit: {0}", Person.FullName ).FormatAsHtmlTitle();

            imgPhoto.BinaryFileId = Person.PhotoId;
            imgPhoto.NoPictureUrl = Person.GetPersonPhotoUrl( Person, 400, 400 );

            ddlTitle.SelectedValue = Person.TitleValueId.HasValue ? Person.TitleValueId.Value.ToString() : string.Empty;
            tbFirstName.Text = Person.FirstName;
            tbNickName.Text = string.IsNullOrWhiteSpace( Person.NickName ) ? string.Empty : ( Person.NickName.Equals( Person.FirstName, StringComparison.OrdinalIgnoreCase ) ? string.Empty : Person.NickName );
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
                var maxGradeOffset = ddlGradePicker.MaxGradeOffset;

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
            ddlMaritalStatus.SelectedValue = Person.MaritalStatusValueId.HasValue ? Person.MaritalStatusValueId.Value.ToString() : string.Empty;
            ddlConnectionStatus.SelectedValue = Person.ConnectionStatusValueId.HasValue ? Person.ConnectionStatusValueId.Value.ToString() : string.Empty;
            tbEmail.Text = Person.Email;
            cbIsEmailActive.Checked = Person.IsEmailActive;
            rblEmailPreference.SelectedValue = Person.EmailPreference.ConvertToString( false );

            ddlRecordStatus.SelectedValue = Person.RecordStatusValueId.HasValue ? Person.RecordStatusValueId.Value.ToString() : string.Empty;
            ddlReason.SelectedValue = Person.RecordStatusReasonValueId.HasValue ? Person.RecordStatusReasonValueId.Value.ToString() : string.Empty;

            tbInactiveReasonNote.Text = Person.InactiveReasonNote;

            bool showInactiveReason = ( Person.RecordStatusValueId.HasValue
                                        && Person.RecordStatusValueId.Value == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id );

            ddlReason.Visible = showInactiveReason;
            tbInactiveReasonNote.Visible = showInactiveReason;

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
            var personGivingEnvelopeAttribute = AttributeCache.Read( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() );
            rcwEnvelope.Visible = GlobalAttributesCache.Read().EnableGivingEnvelopeNumber && personGivingEnvelopeAttribute != null;
            if ( personGivingEnvelopeAttribute != null )
            {
                tbGivingEnvelopeNumber.Text = Person.GetAttributeValue( personGivingEnvelopeAttribute.Key );
            }

            this.PersonPreviousNamesState = Person.GetPreviousNames().ToList();

            BindPersonPreviousNamesGrid();
        }

        /// <summary>
        /// Binds the person previous names grid.
        /// </summary>
        private void BindPersonPreviousNamesGrid()
        {
            grdPreviousNames.DataKeyNames = new string[] { "Guid" };
            grdPreviousNames.DataSource = this.PersonPreviousNamesState;
            grdPreviousNames.DataBind();
        }

        /// <summary>
        /// Handles the AddClick event of the grdPreviousNames control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void grdPreviousNames_AddClick( object sender, EventArgs e )
        {
            tbPreviousLastName.Text = string.Empty;
            mdPreviousName.Show();
        }

        /// <summary>
        /// Handles the Delete event of the grdPreviousNames control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void grdPreviousNames_Delete( object sender, RowEventArgs e )
        {
            this.PersonPreviousNamesState.RemoveEntity( (Guid)e.RowKeyValue );
            BindPersonPreviousNamesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdPreviousName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdPreviousName_SaveClick( object sender, EventArgs e )
        {
            this.PersonPreviousNamesState.Add( new PersonPreviousName { LastName = tbPreviousLastName.Text, Guid = Guid.NewGuid() } );
            BindPersonPreviousNamesGrid();

            mdPreviousName.Hide();
        }

        /// <summary>
        /// Handles the Click event of the btnGenerateEnvelopeNumber control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGenerateEnvelopeNumber_Click( object sender, EventArgs e )
        {
            var personGivingEnvelopeAttribute = AttributeCache.Read( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() );
            var maxEnvelopeNumber = new AttributeValueService( new RockContext() ).Queryable()
                                    .Where( a => a.AttributeId == personGivingEnvelopeAttribute.Id && a.ValueAsNumeric.HasValue )
                                    .Max( a => (int?)a.ValueAsNumeric );
            tbGivingEnvelopeNumber.Text = ( (maxEnvelopeNumber ?? 0) + 1 ).ToString();
        }
    }
}
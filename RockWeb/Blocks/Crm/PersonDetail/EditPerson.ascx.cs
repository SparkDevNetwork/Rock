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
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

/*******************************************************************************************************************************
 * NOTE: The Security/AccountEdit.ascx block has very similar functionality.  If updating this block, make sure to check
 * that block also.  It may need the same updates.
 *******************************************************************************************************************************/

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile block the main information about a person 
    /// </summary>
    [DisplayName( "Edit Person" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to edit a person." )]
    [SecurityAction( "EditFinancials", "The roles and/or users that can edit financial information for the selected person." )]
    [SecurityAction( "EditConnectionStatus", "The roles and/or users that can edit the connection status for the selected person." )]
    [SecurityAction( "EditRecordStatus", "The roles and/or users that can edit the record status for the selected person." )]
    [BooleanField( "Hide Grade", "Should the Grade (and Graduation Year) fields be hidden?", false, "", 0 )]
    [BooleanField( "Hide Anniversary Date", "Should the Anniversary Date field be hidden?", false, "", 1 )]
    [CustomEnhancedListField( "Search Key Types", "Optional list of search key types to limit the display in search keys grid. No selection will show all.", @"
        DECLARE @AttributeId int = (
	        SELECT [Id] 
	        FROM [Attribute]
	        WHERE [Guid] = '15C419AA-76A9-4105-AB99-8384AB0E9B44'
        )
        SELECT
	        CAST( V.[Guid] as varchar(40) ) AS [Value],
	        V.[Value] AS [Text]
        FROM [DefinedType] T
        INNER JOIN [DefinedValue] V ON V.[DefinedTypeId] = T.[Id]
        LEFT OUTER JOIN [AttributeValue] AV 
	        ON AV.[EntityId] = V.[Id]
	        AND AV.[AttributeId] = @AttributeId
	        AND AV.[Value] = 'False'
        WHERE T.[Guid] = '61BDD0E3-173D-45AB-9E8C-1FBB9FA8FDF3'
        AND AV.[Id] IS NULL
        ORDER BY V.[Order]
",
        false, "", "", 2 )]
    public partial class EditPerson : Rock.Web.UI.PersonBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            dvpTitle.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ).Id;
            dvpSuffix.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ).Id;
            dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ).Id;
            dvpConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ).Id;
            dvpRecordStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ).Id;
            dvpReason.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ).Id;

            pnlGivingGroup.Visible = UserCanAdministrate || IsUserAuthorized( "EditFinancials" );

            bool canEditConnectionStatus = UserCanAdministrate || IsUserAuthorized( "EditConnectionStatus" );
            dvpConnectionStatus.Visible = canEditConnectionStatus;
            lConnectionStatusReadOnly.Visible = !canEditConnectionStatus;

            bool canEditRecordStatus = UserCanAdministrate || IsUserAuthorized( "EditRecordStatus" );
            dvpRecordStatus.Visible = canEditRecordStatus;
            lRecordStatusReadOnly.Visible = !canEditRecordStatus;

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

            gAlternateIds.Actions.ShowAdd = true;
            gAlternateIds.Actions.AddClick += gAlternateIds_AddClick;

            gSearchKeys.Actions.ShowAdd = true;
            gSearchKeys.Actions.AddClick += gSearchKeys_AddClick;

            pnlGradeGraduation.Visible = !GetAttributeValue( "HideGrade" ).AsBoolean();
            dpAnniversaryDate.Visible = !GetAttributeValue( "HideAnniversaryDate" ).AsBoolean();
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
        /// Gets or sets the state of the person search keys.
        /// </summary>
        /// <value>
        /// The state of the person search keys.
        /// </value>
        private List<PersonSearchKey> PersonSearchKeysState { get; set; }

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

            json = ViewState["PersonSearchKeysState"] as string;

            if ( string.IsNullOrWhiteSpace( json ) )
            {
                PersonSearchKeysState = new List<PersonSearchKey>();
            }
            else
            {
                PersonSearchKeysState = PersonSearchKey.FromJsonAsList( json ) ?? new List<PersonSearchKey>();
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
            ViewState["PersonSearchKeysState"] = JsonConvert.SerializeObject( PersonSearchKeysState, Formatting.None, jsonSetting );

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
            bool showInactiveReason = ( dvpRecordStatus.SelectedValueAsInt() == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id );

            bool canEditRecordStatus = UserCanAdministrate || IsUserAuthorized( "EditRecordStatus" );
            dvpReason.Visible = showInactiveReason && canEditRecordStatus;
            lReasonReadOnly.Visible = showInactiveReason && !canEditRecordStatus;
            tbInactiveReasonNote.Visible = showInactiveReason && canEditRecordStatus;
            lReasonNoteReadOnly.Visible = showInactiveReason && !canEditRecordStatus;
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
                if ( Page.IsValid )
                {
                    var rockContext = new RockContext();

                    rockContext.WrapTransaction( () =>
                    {
                        var personService = new PersonService( rockContext );

                        var person = personService.Get( Person.Id );

                        int? orphanedPhotoId = null;
                        if ( person.PhotoId != imgPhoto.BinaryFileId )
                        {
                            orphanedPhotoId = person.PhotoId;
                            person.PhotoId = imgPhoto.BinaryFileId;
                        }

                        person.TitleValueId = dvpTitle.SelectedValueAsInt();
                        person.FirstName = tbFirstName.Text;
                        person.NickName = tbNickName.Text;
                        person.MiddleName = tbMiddleName.Text;
                        person.LastName = tbLastName.Text;
                        person.SuffixValueId = dvpSuffix.SelectedValueAsInt();

                        var birthMonth = person.BirthMonth;
                        var birthDay = person.BirthDay;
                        var birthYear = person.BirthYear;

                        var birthday = bpBirthDay.SelectedDate;
                        if ( birthday.HasValue )
                        {
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

                        int? graduationYear = null;
                        if ( ypGraduation.SelectedYear.HasValue )
                        {
                            graduationYear = ypGraduation.SelectedYear.Value;
                        }
                        person.GraduationYear = graduationYear;

                        person.MaritalStatusValueId = dvpMaritalStatus.SelectedValueAsInt();
                        person.AnniversaryDate = person.MaritalStatusValueId == DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED ).Id ? dpAnniversaryDate.SelectedDate : null;
                        person.Gender = rblGender.SelectedValue.ConvertToEnum<Gender>();
                        person.ConnectionStatusValueId = dvpConnectionStatus.SelectedValueAsInt();

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
                            person.PhoneNumbers.Remove( phoneNumber );
                            phoneNumberService.Delete( phoneNumber );
                        }

                        person.Email = tbEmail.Text.Trim();
                        person.IsEmailActive = cbIsEmailActive.Checked;
                        person.EmailPreference = rblEmailPreference.SelectedValue.ConvertToEnum<EmailPreference>();
                        person.CommunicationPreference = rblCommunicationPreference.SelectedValueAsEnum<CommunicationType>();
                        ;
                        person.GivingGroupId = ddlGivingGroup.SelectedValueAsId();
                        person.IsLockedAsChild = cbLockAsChild.Checked;

                        // Save the Envelope Number attribute if it exists and has changed
                        var personGivingEnvelopeAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() );
                        if ( GlobalAttributesCache.Get().EnableGivingEnvelopeNumber && personGivingEnvelopeAttribute != null )
                        {
                            if ( person.Attributes == null )
                            {
                                person.LoadAttributes( rockContext );
                            }

                            var newEnvelopeNumber = tbGivingEnvelopeNumber.Text;
                            var oldEnvelopeNumber = person.GetAttributeValue( personGivingEnvelopeAttribute.Key );
                            if ( newEnvelopeNumber != oldEnvelopeNumber )
                            {
                                // If they haven't already confirmed about duplicate, see if the envelope number if assigned to somebody else
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

                                person.SetAttributeValue( personGivingEnvelopeAttribute.Key, newEnvelopeNumber );
                            }
                        }

                        bool recordStatusChangedToOrFromInactive = false;
                        var recordStatusInactiveId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;

                        int? newRecordStatusId = dvpRecordStatus.SelectedValueAsInt();
                        // Is the person's record status changing?
                        if ( person.RecordStatusValueId.HasValue && person.RecordStatusValueId != newRecordStatusId )
                        {
                            //  If it was inactive OR if the new status is inactive, flag this for use later below.
                            if ( person.RecordStatusValueId == recordStatusInactiveId || newRecordStatusId == recordStatusInactiveId )
                            {
                                recordStatusChangedToOrFromInactive = true;
                            }
                        }

                        person.RecordStatusValueId = dvpRecordStatus.SelectedValueAsInt();

                        int? newRecordStatusReasonId = null;
                        if ( person.RecordStatusValueId.HasValue && person.RecordStatusValueId.Value == recordStatusInactiveId )
                        {
                            newRecordStatusReasonId = dvpReason.SelectedValueAsInt();
                        }
                        person.RecordStatusReasonValueId = newRecordStatusReasonId;

                        person.InactiveReasonNote = tbInactiveReasonNote.Text.Trim();

                        // Save any Removed/Added Previous Names
                        var personPreviousNameService = new PersonPreviousNameService( rockContext );
                        var databasePreviousNames = personPreviousNameService.Queryable().Where( a => a.PersonAlias.PersonId == person.Id ).ToList();
                        foreach ( var deletedPreviousName in databasePreviousNames.Where( a => !PersonPreviousNamesState.Any( p => p.Guid == a.Guid ) ) )
                        {
                            personPreviousNameService.Delete( deletedPreviousName );
                        }

                        foreach ( var addedPreviousName in PersonPreviousNamesState.Where( a => !databasePreviousNames.Any( d => d.Guid == a.Guid ) ) )
                        {
                            addedPreviousName.PersonAliasId = person.PrimaryAliasId.Value;
                            personPreviousNameService.Add( addedPreviousName );
                        }

                        var personSearchKeyService = new PersonSearchKeyService( rockContext );

                        var validSearchTypes = GetValidSearchKeyTypes();
                        var databaseSearchKeys = personSearchKeyService.Queryable()
                            .Where( a =>
                                validSearchTypes.Contains( a.SearchTypeValue.Guid ) &&
                                a.PersonAlias.PersonId == person.Id )
                            .ToList();

                        foreach ( var deletedSearchKey in databaseSearchKeys.Where( a => !PersonSearchKeysState.Any( p => p.Guid == a.Guid ) ) )
                        {
                            personSearchKeyService.Delete( deletedSearchKey );
                        }

                        foreach ( var personSearchKey in PersonSearchKeysState.Where( a => !databaseSearchKeys.Any( d => d.Guid == a.Guid ) ) )
                        {
                            personSearchKey.PersonAliasId = person.PrimaryAliasId.Value;
                            personSearchKeyService.Add( personSearchKey );
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
        }

        /// <summary>
        /// Gets the search key types that have been configured or are a system type.
        /// </summary>
        /// <returns></returns>
        private List<Guid> GetValidSearchKeyTypes()
        {
            var searchKeyTypes = new List<Guid> { Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() };

            var dt = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SEARCH_KEYS );
            if ( dt != null )
            {
                var values = dt.DefinedValues;
                var searchTypesList = this.GetAttributeValue( "SearchKeyTypes" ).SplitDelimitedValues().AsGuidList();
                if ( searchTypesList.Any() )
                {
                    values = values.Where( v => searchTypesList.Contains( v.Guid ) ).ToList();
                }

                foreach ( var dv in dt.DefinedValues )
                {
                    if ( dv.GetAttributeValue( "UserSelectable" ).AsBoolean() )
                    {
                        searchKeyTypes.Add( dv.Guid );
                    }
                }
            }

            return searchKeyTypes;

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
            imgPhoto.NoPictureUrl = Person.GetPersonNoPictureUrl( this.Person, 400, 400 );

            dvpTitle.SelectedValue = Person.TitleValueId.HasValue ? Person.TitleValueId.Value.ToString() : string.Empty;
            tbFirstName.Text = Person.FirstName;
            tbNickName.Text = string.IsNullOrWhiteSpace( Person.NickName ) ? string.Empty : ( Person.NickName.Equals( Person.FirstName, StringComparison.OrdinalIgnoreCase ) ? string.Empty : Person.NickName );
            tbMiddleName.Text = Person.MiddleName;
            tbLastName.Text = Person.LastName;
            dvpSuffix.SelectedValue = Person.SuffixValueId.HasValue ? Person.SuffixValueId.Value.ToString() : string.Empty;
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
            dvpMaritalStatus.SetValue( Person.MaritalStatusValueId );
            ShowAnniversaryDate();
            dvpConnectionStatus.SetValue( Person.ConnectionStatusValueId );
            lConnectionStatusReadOnly.Text = Person.ConnectionStatusValueId.HasValue ? Person.ConnectionStatusValue.Value : string.Empty;

            tbEmail.Text = Person.Email;
            cbIsEmailActive.Checked = Person.IsEmailActive;
            rblEmailPreference.SelectedValue = Person.EmailPreference.ConvertToString( false );
            rblCommunicationPreference.SetValue( Person.CommunicationPreference == CommunicationType.SMS ? "2" : "1" );

            dvpRecordStatus.SetValue( Person.RecordStatusValueId );
            lRecordStatusReadOnly.Text = Person.RecordStatusValueId.HasValue ? Person.RecordStatusValue.Value : string.Empty;
            dvpReason.SetValue( Person.RecordStatusReasonValueId );
            lReasonReadOnly.Text = Person.RecordStatusReasonValueId.HasValue ? Person.RecordStatusReasonValue.Value : string.Empty;

            tbInactiveReasonNote.Text = Person.InactiveReasonNote;
            lReasonNoteReadOnly.Text = Person.InactiveReasonNote;

            ddlRecordStatus_SelectedIndexChanged( null, null );

            var mobilePhoneType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );

            var phoneNumbers = new List<PhoneNumber>();
            var phoneNumberTypes = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
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
            cbLockAsChild.Checked = Person.IsLockedAsChild;
            var personGivingEnvelopeAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() );
            rcwEnvelope.Visible = GlobalAttributesCache.Get().EnableGivingEnvelopeNumber && personGivingEnvelopeAttribute != null;
            if ( personGivingEnvelopeAttribute != null )
            {
                tbGivingEnvelopeNumber.Text = Person.GetAttributeValue( personGivingEnvelopeAttribute.Key );
            }

            this.PersonPreviousNamesState = Person.GetPreviousNames().ToList();

            var validSearchTypes = GetValidSearchKeyTypes();
            var searchTypeQry = Person.GetPersonSearchKeys().Where( a => validSearchTypes.Contains( a.SearchTypeValue.Guid ) );
            this.PersonSearchKeysState = searchTypeQry.ToList();

            BindPersonPreviousNamesGrid();
            BindPersonAlternateIdsGrid();
            BindPersonSearchKeysGrid();
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
        /// Binds the person previous names grid.
        /// </summary>
        private void BindPersonAlternateIdsGrid()
        {
            var values = this.PersonSearchKeysState;
            var dv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );
            if ( dv != null )
            {
                values = values.Where( s => s.SearchTypeValueId == dv.Id ).ToList();
            }
            gAlternateIds.DataKeyNames = new string[] { "Guid" };
            gAlternateIds.DataSource = values;
            gAlternateIds.DataBind();
        }

        /// <summary>
        /// Binds the person previous names grid.
        /// </summary>
        private void BindPersonSearchKeysGrid()
        {
            var values = this.PersonSearchKeysState;
            var dv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );
            if ( dv != null )
            {
                values = values.Where( s => s.SearchTypeValueId != dv.Id ).ToList();
            }
            gSearchKeys.DataKeyNames = new string[] { "Guid" };
            gSearchKeys.DataSource = values;
            gSearchKeys.DataBind();
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
        /// Handles the AddClick event of the gAlternateIds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAlternateIds_AddClick( object sender, EventArgs e )
        {
            tbAlternateId.Text = string.Empty;
            mdAlternateId.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAlternateId control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAlternateId_SaveClick( object sender, EventArgs e )
        {
            var dv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );
            if ( dv != null )
            {
                this.PersonSearchKeysState.Add( new PersonSearchKey { SearchValue = tbAlternateId.Text, SearchTypeValueId = dv.Id, Guid = Guid.NewGuid() } );
            }
            BindPersonAlternateIdsGrid();
            mdAlternateId.Hide();
        }

        /// <summary>
        /// Handles the AddClick event of the gSearchKeys control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gSearchKeys_AddClick( object sender, EventArgs e )
        {
            tbSearchValue.Text = string.Empty;

            var validSearchTypes = GetValidSearchKeyTypes()
                .Where( t => t != Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() )
                .ToList();

            var searchValueTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SEARCH_KEYS ).DefinedValues;
            var searchTypesList = searchValueTypes.Where( a => validSearchTypes.Contains( a.Guid ) ).ToList();

            ddlSearchValueType.DataSource = searchTypesList;
            ddlSearchValueType.DataTextField = "Value";
            ddlSearchValueType.DataValueField = "Id";
            ddlSearchValueType.DataBind();
            ddlSearchValueType.Items.Insert( 0, new ListItem() );
            mdSearchKey.Show();
        }


        /// <summary>
        /// Handles the SaveClick event of the mdSearchKey control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSearchKey_SaveClick( object sender, EventArgs e )
        {
            this.PersonSearchKeysState.Add( new PersonSearchKey { SearchValue = tbSearchValue.Text, SearchTypeValueId = ddlSearchValueType.SelectedValue.AsInteger(), Guid = Guid.NewGuid() } );
            BindPersonSearchKeysGrid();
            mdSearchKey.Hide();
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
        /// Handles the Delete event of the grdPreviousNames control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void grdPreviousNames_Delete( object sender, RowEventArgs e )
        {
            this.PersonPreviousNamesState.RemoveEntity( ( Guid ) e.RowKeyValue );
            BindPersonPreviousNamesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gSearchKeys control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAlternateIds_Delete( object sender, RowEventArgs e )
        {
            this.PersonSearchKeysState.RemoveEntity( ( Guid ) e.RowKeyValue );
            BindPersonAlternateIdsGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gSearchKeys control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSearchKeys_Delete( object sender, RowEventArgs e )
        {
            this.PersonSearchKeysState.RemoveEntity( ( Guid ) e.RowKeyValue );
            BindPersonSearchKeysGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnGenerateEnvelopeNumber control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGenerateEnvelopeNumber_Click( object sender, EventArgs e )
        {
            var personGivingEnvelopeAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() );
            var maxEnvelopeNumber = new AttributeValueService( new RockContext() ).Queryable()
                                    .Where( a => a.AttributeId == personGivingEnvelopeAttribute.Id && a.ValueAsNumeric.HasValue )
                                    .Max( a => ( int? ) a.ValueAsNumeric );
            tbGivingEnvelopeNumber.Text = ( ( maxEnvelopeNumber ?? 0 ) + 1 ).ToString();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMaritalStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMaritalStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowAnniversaryDate();
        }

        /// <summary>
        /// Shows or hides the Anniversary DatePicker according to settings and business rules.
        /// </summary>
        protected void ShowAnniversaryDate()
        {
            if ( GetAttributeValue( "HideAnniversaryDate" ).AsBoolean() == true )
            {
                dpAnniversaryDate.Visible = false;
            }
            else if ( dvpMaritalStatus.SelectedValueAsInt() == DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED ).Id )
            {
                dpAnniversaryDate.Visible = true;
            }
            else
            {
                dpAnniversaryDate.Visible = false;
            }
        }

        /// <summary>
        /// Handles the ServerValidate event of the cvAlternateIds control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvAlternateIds_ServerValidate( object source, ServerValidateEventArgs args )
        {
            // Validate that none of the alternate ids are being used already.
            var dv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );
            if ( dv != null )
            {
                var invalidIds = new List<string>();
                using ( var rockContext = new RockContext() )
                {
                    var service = new PersonSearchKeyService( rockContext );
                    foreach ( var value in PersonSearchKeysState.Where( s => s.SearchTypeValueId == dv.Id ).ToList() )
                    {
                        if ( service.Queryable().AsNoTracking()
                            .Any( v =>
                                v.SearchTypeValueId == dv.Id &&
                                v.SearchValue == value.SearchValue &&
                                v.Guid != value.Guid ) )
                        {
                            invalidIds.Add( value.SearchValue );
                        }
                    }
                }

                if ( invalidIds.Any() )
                {
                    if ( invalidIds.Count == 1 )
                    {
                        cvAlternateIds.ErrorMessage = string.Format( "The '{0}' alternate id is already being used by another person. Please remove this value and optionally add a new unique alternate id.", invalidIds.First() );
                    }
                    else
                    {
                        cvAlternateIds.ErrorMessage = string.Format( "The '{0}' alternate ids are already being used by another person. Please remove these value and optionally add new unique alternate ids.", invalidIds.AsDelimited( "' and '" ) );
                    }

                    args.IsValid = false;
                }
            }
        }
    }
}
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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Humanizer;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Calendar
{
    /// <summary>
    /// Block used to register for a registration instance.
    /// </summary>
    [DisplayName( "Simple Registration" )]
    [Category( "com_centralaz > Calendar" )]
    [Description( "Block used for quick and easy registration" )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "", 1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false, Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE, "", 2 )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Event Registration", "", 3 )]
    [BooleanField( "Display Progress Bar", "Display a progress bar for the registration.", true, "", 4 )]
    [BooleanField( "Enable Debug", "Display the merge fields that are available for lava ( Success Page ).", false, "", 5 )]
    [SystemEmailField( "Confirm Account Template", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "", 4 )]
    public partial class SimpleRegistration : RockBlock
    {
        #region Fields

        private bool _saveNavigationHistory = false;

        // Page (query string) parameter names
        private const string REGISTRATION_ID_PARAM_NAME = "RegistrationId";
        private const string SLUG_PARAM_NAME = "Slug";
        private const string REGISTRATION_INSTANCE_ID_PARAM_NAME = "RegistrationInstanceId";
        private const string EVENT_OCCURRENCE_ID_PARAM_NAME = "EventOccurrenceId";
        private const string GROUP_ID_PARAM_NAME = "GroupId";
        private const string CAMPUS_ID_PARAM_NAME = "CampusId";

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Reset warning/error messages
            nbMain.Visible = false;
            nbPaymentValidation.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowRegistration();
            }

        }

        #endregion

        #region Events

        protected void lbAddRegistrant_Click( object sender, EventArgs e )
        {
            NavigateToPage( RockPage.Guid, new Dictionary<string, string>() );
        }

        protected void lbRegister_Click( object sender, EventArgs e )
        {
            if ( SaveRegistration() )
            {
                pnlRegistrant.Visible = false;
                pnlSuccess.Visible = true;
            }
        }

        #endregion

        #region Methods

        #region Save Methods

        /// <summary>
        /// Saves the registration.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="hasPayment">if set to <c>true</c> [has payment].</param>
        /// <returns></returns>
        private bool SaveRegistration()
        {
            var rockContext = new RockContext();

            var registrationInstanceId = REGISTRATION_INSTANCE_ID_PARAM_NAME.AsIntegerOrNull();
            if ( registrationInstanceId.HasValue )
            {
                var registrationInstance = new RegistrationInstanceService( rockContext ).Get( registrationInstanceId.Value );
                if ( registrationInstance != null )
                {

                    var registrationChanges = new List<string>();

                    var registrationService = new RegistrationService( rockContext );
                    var registrantService = new RegistrationRegistrantService( rockContext );
                    var personService = new PersonService( rockContext );
                    var groupMemberService = new GroupMemberService( rockContext );
                    var groupService = new GroupService( rockContext );
                    var noteService = new NoteService( rockContext );

                    Person registrar = null;

                    // variables to keep track of the family that new people should be added to
                    int? singleFamilyId = null;
                    var multipleFamilyGroupIds = new Dictionary<Guid, int>();

                    var dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                    var dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );
                    var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                    var adultRoleId = familyGroupType.Roles
                        .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                        .Select( r => r.Id )
                        .FirstOrDefault();
                    var childRoleId = familyGroupType.Roles
                        .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                        .Select( r => r.Id )
                        .FirstOrDefault();

                    bool additionalDetails = false;

                    var registration = new Registration();
                    registrationService.Add( registration );
                    registrationChanges.Add( "Created Registration" );

                    registration.RegistrationInstanceId = REGISTRATION_INSTANCE_ID_PARAM_NAME.AsInteger();
                    registration.GroupId = GROUP_ID_PARAM_NAME.AsInteger();

                    History.EvaluateChange( registrationChanges, "First Name", string.Empty, tbFirstName.Text );
                    registration.FirstName = tbFirstName.Text;

                    History.EvaluateChange( registrationChanges, "Last Name", string.Empty, tbLastName.Text );
                    registration.LastName = tbLastName.Text;

                    History.EvaluateChange( registrationChanges, "Confirmation Email", string.Empty, tbEmail.Text );
                    registration.ConfirmationEmail = tbEmail.Text;


                    // If the 'your name' value equals the currently logged in person, use their person alias id
                    if ( CurrentPerson != null )
                    {
                        registrar = CurrentPerson;
                        registration.PersonAliasId = registrar.PrimaryAliasId;
                    }
                    else
                    {
                        // otherwise look for one and one-only match by name/email
                        var registrarMatches = personService.GetByMatch( registration.FirstName, registration.LastName, registration.ConfirmationEmail );
                        if ( registrarMatches.Count() == 1 )
                        {
                            registrar = registrarMatches.First();
                            registration.PersonAliasId = registrar.PrimaryAliasId;
                        }
                    }

                    // Save the registration ( so we can get an id )
                    rockContext.SaveChanges();

                    // If the Registration Instance linkage specified a group, load it now
                    Group group = null;
                    if ( GROUP_ID_PARAM_NAME.AsIntegerOrNull().HasValue )
                    {
                        group = new GroupService( rockContext ).Get( GROUP_ID_PARAM_NAME.AsIntegerOrNull().Value );
                        if ( group != null )
                        {
                            History.EvaluateChange( registrationChanges, "Group", string.Empty, group.Name );
                        }
                    }

                    // Setup Note settings
                    NoteTypeCache noteType = null;
                    var registrantNames = new Dictionary<int, string>();
                    if ( registrationInstance.RegistrationTemplate != null && registrationInstance.RegistrationTemplate.AddPersonNote )
                    {
                        noteType = NoteTypeCache.Read( Rock.SystemGuid.NoteType.PERSON_EVENT_REGISTRATION.AsGuid() );
                    }

                    Task.Run( () =>
                        HistoryService.SaveChanges(
                            new RockContext(),
                            typeof( Registration ),
                            Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                            registration.Id,
                            registrationChanges )
                    );

                    var registrantChanges = new List<string>();
                    var personChanges = new List<string>();
                    var familyChanges = new List<string>();

                    Person person = null;

                    // Try to find a matching person based on name and email address
                    string firstName = tbFirstName.Text;
                    string lastName = tbLastName.Text;
                    string email = tbEmail.Text;
                    var personMatches = personService.GetByMatch( firstName, lastName, email );
                    if ( personMatches.Count() == 1 )
                    {
                        person = personMatches.First();
                    }

                    // Try to find a matching person based on name within same family as registrar
                    if ( person == null && registrar != null )
                    {
                        var familyMembers = registrar.GetFamilyMembers( true, rockContext )
                            .Where( m =>
                                ( m.Person.FirstName == firstName || m.Person.NickName == firstName ) &&
                                m.Person.LastName == lastName )
                            .Select( m => m.Person )
                            .ToList();

                        if ( familyMembers.Count() == 1 )
                        {
                            person = familyMembers.First();
                            if ( !string.IsNullOrWhiteSpace( email ) )
                            {
                                person.Email = email;
                            }
                        }

                        if ( familyMembers.Count() > 1 && !string.IsNullOrWhiteSpace( email ) )
                        {
                            familyMembers = familyMembers
                                .Where( m =>
                                    m.Email != null &&
                                    m.Email.Equals( email, StringComparison.OrdinalIgnoreCase ) )
                                .ToList();
                            if ( familyMembers.Count() == 1 )
                            {
                                person = familyMembers.First();
                            }
                        }
                    }

                    if ( person == null )
                    {
                        // If a match was not found, create a new person
                        person = new Person();
                        person.FirstName = firstName;
                        person.LastName = lastName;
                        person.IsEmailActive = true;
                        person.Email = email;
                        person.EmailPreference = EmailPreference.EmailAllowed;
                        person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        if ( dvcConnectionStatus != null )
                        {
                            person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                        }

                        if ( dvcRecordStatus != null )
                        {
                            person.RecordStatusValueId = dvcRecordStatus.Id;
                        }
                    }

                    int? campusId = null;
                    Location location = null;

                    // Set any of the template's person fields
                    foreach ( var field in registrationInstance.RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields
                            .Where( t => t.FieldSource == RegistrationFieldSource.PersonField && t.IsRequired ) ) )
                    {

                        switch ( field.PersonFieldType )
                        {
                            case RegistrationPersonFieldType.Campus:
                                {
                                    if ( person.GetCampus() == null )
                                    {
                                        additionalDetails = true;
                                    }
                                    break;
                                }

                            case RegistrationPersonFieldType.Address:
                                {
                                    if ( person.GetHomeLocation() == null )
                                    {
                                        additionalDetails = true;
                                    }
                                    break;
                                }

                            case RegistrationPersonFieldType.Birthdate:
                                {
                                    if ( !person.BirthDate.HasValue )
                                    {
                                        additionalDetails = true;
                                    }
                                    break;
                                }

                            case RegistrationPersonFieldType.Gender:
                                {
                                    if ( person.Gender == null )
                                    {
                                        additionalDetails = true;
                                    }
                                    break;
                                }

                            case RegistrationPersonFieldType.MaritalStatus:
                                {
                                    if ( person.MaritalStatusValue == null )
                                    {
                                        additionalDetails = true;
                                    }
                                    break;
                                }

                            case RegistrationPersonFieldType.MobilePhone:
                                {
                                    if ( person.PhoneNumbers.Where( p => p.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).FirstOrDefault() == null )
                                    {
                                        additionalDetails = true;
                                    }
                                    break;
                                }

                            case RegistrationPersonFieldType.HomePhone:
                                {
                                    if ( person.PhoneNumbers.Where( p => p.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ).FirstOrDefault() == null )
                                    {
                                        additionalDetails = true;
                                    } break;
                                }

                            case RegistrationPersonFieldType.WorkPhone:
                                {
                                    if ( person.PhoneNumbers.Where( p => p.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() ).FirstOrDefault() == null )
                                    {
                                        additionalDetails = true;
                                    } break;
                                }
                        }
                    }

                    // Save the person ( and family if needed )
                    SavePerson( rockContext, person, person.GetFamilies().FirstOrDefault().Guid, campusId, location, adultRoleId, childRoleId, multipleFamilyGroupIds, singleFamilyId, registrationInstance );

                    // Load the person's attributes
                    person.LoadAttributes();

                    // Set any of the template's person fields
                    foreach ( var field in registrationInstance.RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields
                            .Where( t =>
                                t.FieldSource == RegistrationFieldSource.PersonAttribute &&
                                t.AttributeId.HasValue &&
                                t.IsRequired ) ) )
                    {

                        var attribute = AttributeCache.Read( field.AttributeId.Value );
                        if ( attribute != null )
                        {
                            string originalValue = person.GetAttributeValue( attribute.Key );
                            if ( String.IsNullOrWhiteSpace( originalValue ) )
                            {
                                additionalDetails = true;
                            }
                        }
                    }

                    string registrantName = person.FullName + ": ";

                    personChanges.ForEach( c => registrantChanges.Add( c ) );

                    GroupMember groupMember = null;

                    // If the registration instance linkage specified a group to add registrant to, add them if there not already
                    // part of that group
                    if ( group != null )
                    {
                        groupMember = group.Members.Where( m => m.PersonId == person.Id ).FirstOrDefault();
                        if ( groupMember == null && group.GroupType.DefaultGroupRoleId.HasValue )
                        {
                            groupMember = new GroupMember();
                            groupMemberService.Add( groupMember );
                            groupMember.GroupId = group.Id;
                            groupMember.PersonId = person.Id;

                            if ( registrationInstance.RegistrationTemplate.GroupTypeId.HasValue &&
                                registrationInstance.RegistrationTemplate.GroupTypeId == group.GroupTypeId &&
                                registrationInstance.RegistrationTemplate.GroupMemberRoleId.HasValue )
                            {
                                groupMember.GroupRoleId = registrationInstance.RegistrationTemplate.GroupMemberRoleId.Value;
                                groupMember.GroupMemberStatus = registrationInstance.RegistrationTemplate.GroupMemberStatus;
                            }
                            else
                            {
                                groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;
                                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                            }

                            registrantChanges.Add( "Added to Group: " + group.Name );
                        }

                        rockContext.SaveChanges();

                        // Set any of the template's group member attributes 
                        groupMember.LoadAttributes();

                        foreach ( var field in registrationInstance.RegistrationTemplate.Forms
                            .SelectMany( f => f.Fields
                                .Where( t =>
                                    t.FieldSource == RegistrationFieldSource.GroupMemberAttribute &&
                                    t.AttributeId.HasValue ) ) )
                        {


                            var attribute = AttributeCache.Read( field.AttributeId.Value );
                            if ( attribute != null )
                            {
                                string originalValue = groupMember.GetAttributeValue( attribute.Key );
                                if ( String.IsNullOrWhiteSpace( originalValue ) )
                                {
                                    additionalDetails = true;
                                }
                            }
                        }
                    }

                    var registrant = new RegistrationRegistrant();
                    registrantService.Add( registrant );
                    registrant.RegistrationId = registration.Id;
                    registrant.PersonAliasId = person.PrimaryAliasId;
                    registrant.GroupMemberId = groupMember != null ? groupMember.Id : (int?)null;

                    rockContext.SaveChanges();

                    // Set any of the template's registrant attributes
                    registrant.LoadAttributes();
                    foreach ( var field in registrationInstance.RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields
                            .Where( t =>
                                t.FieldSource == RegistrationFieldSource.RegistrationAttribute &&
                                t.AttributeId.HasValue ) ) )
                    {
                        var attribute = AttributeCache.Read( field.AttributeId.Value );
                        if ( attribute != null )
                        {
                            string originalValue = registrant.GetAttributeValue( attribute.Key );
                            if ( String.IsNullOrWhiteSpace( originalValue ) )
                            {
                                additionalDetails = true;
                            }
                        }
                    }

                    // Add a note to the registrant's person notes (if they aren't the one doing the registering)
                    if ( noteType != null )
                    {
                        var noteText = new StringBuilder();
                        if ( registrar == null || registrar.Id != person.Id )
                        {
                            noteText.AppendFormat( "Registered for {0}", registrationInstance.Name );
                            if ( registrar != null )
                            {
                                noteText.AppendFormat( " by {0}", registrar.FullName );
                            }

                            var note = new Note();
                            note.NoteTypeId = noteType.Id;
                            note.IsSystem = false;
                            note.IsAlert = false;
                            note.IsPrivateNote = false;
                            note.EntityId = person.Id;
                            note.Caption = string.Empty;
                            note.Text = noteText.ToString();
                            noteService.Add( note );
                        }
                    }

                    Task.Run( () =>
                        HistoryService.SaveChanges(
                            new RockContext(),
                            typeof( Registration ),
                            Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                            registration.Id,
                            registrantChanges,
                            "Registrant: " + person.FullName,
                            null, null )
                    );

                    // Add a note to the registrars notes
                    if ( noteType != null && registrar != null && registrantNames.Any() )
                    {
                        string namesText = string.Empty;
                        if ( person.Id != registrar.Id )
                        {
                            namesText = person.FullName + " ";
                        }
                        else
                        {
                            namesText = registrar.Gender == Gender.Male ? "himself" : registrar.Gender == Gender.Female ? "herself" : "themselves";
                        }

                        var note = new Note();
                        note.NoteTypeId = noteType.Id;
                        note.IsSystem = false;
                        note.IsAlert = false;
                        note.IsPrivateNote = false;
                        note.EntityId = registrar.Id;
                        note.Caption = string.Empty;
                        note.Text = string.Format( "Registered {0} for {1}", namesText, registrationInstance.Name );
                        noteService.Add( note );
                    }

                    rockContext.SaveChanges();

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Saves the person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="familyGuid">The family unique identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="adultRoleId">The adult role identifier.</param>
        /// <param name="childRoleId">The child role identifier.</param>
        /// <param name="multipleFamilyGroupIds">The multiple family group ids.</param>
        /// <param name="singleFamilyId">The single family identifier.</param>
        /// <returns></returns>
        private Person SavePerson( RockContext rockContext, Person person, Guid? familyGuid, int? campusId, Location location, int adultRoleId, int childRoleId,
            Dictionary<Guid, int> multipleFamilyGroupIds, int? singleFamilyId, RegistrationInstance registrationInstance )
        {
            if ( person.Id > 0 )
            {
                rockContext.SaveChanges();

                // Set the family guid for any other registrants that were selected to be in the same family
                var family = person.GetFamilies( rockContext ).FirstOrDefault();
                if ( family != null )
                {
                    multipleFamilyGroupIds.AddOrIgnore( familyGuid.Value, family.Id );
                    if ( !singleFamilyId.HasValue )
                    {
                        singleFamilyId = family.Id;
                    }
                }
            }
            else
            {
                // If we've created the family aready for this registrant, add them to it
                if (
                        ( registrationInstance.RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask && multipleFamilyGroupIds.ContainsKey( familyGuid.Value ) ) ||
                        ( registrationInstance.RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes && singleFamilyId.HasValue )
                    )
                {

                    // Add person to existing family
                    var age = person.Age;
                    int familyRoleId = age.HasValue && age < 18 ? childRoleId : adultRoleId;

                    int familyId = registrationInstance.RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask ?
                        multipleFamilyGroupIds[familyGuid.Value] :
                        singleFamilyId.Value;
                    PersonService.AddPersonToFamily( person, true, multipleFamilyGroupIds[familyGuid.Value], familyRoleId, rockContext );

                    if ( location != null )
                    {
                        var homeLocationType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                        if ( homeLocationType != null )
                        {
                            var familyGroup = new GroupService( rockContext ).Get( familyId );

                            // Do not update existing location on an existing family ( only update when creating new family or location doesn't already exist )
                            if ( familyGroup != null && !familyGroup.GroupLocations
                                .Any( l =>
                                    l.GroupLocationTypeValueId.HasValue &&
                                    l.GroupLocationTypeValueId.Value == homeLocationType.Id ) )
                            {
                                GroupService.AddNewFamilyAddress(
                                    rockContext,
                                    familyGroup,
                                    Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                                    location.Street1, location.Street2, location.City, location.State, location.PostalCode, location.Country );
                            }
                        }
                    }
                }

                // otherwise create a new family
                else
                {
                    // Create Person/Family
                    var familyGroup = PersonService.SaveNewPerson( person, rockContext, campusId, false );
                    if ( familyGroup != null )
                    {
                        if ( location != null )
                        {
                            GroupService.AddNewFamilyAddress(
                                rockContext,
                                familyGroup,
                                Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                                location.Street1, location.Street2, location.City, location.State, location.PostalCode, location.Country );
                        }

                        // Store the family id for next person 
                        multipleFamilyGroupIds.AddOrIgnore( familyGuid.Value, familyGroup.Id );
                        if ( !singleFamilyId.HasValue )
                        {
                            singleFamilyId = familyGroup.Id;
                        }
                    }
                }
            }

            return new PersonService( rockContext ).Get( person.Id );
        }

        #endregion

        #region Display Methods

        private void ShowRegistration()
        {
            if ( CurrentPerson != null )
            {
                tbFirstName.Text = CurrentPerson.FirstName;
                tbLastName.Text = CurrentPerson.LastName;
                tbEmail.Text = CurrentPerson.Email;
                tbPhoneNumber.Text = CurrentPerson.PhoneNumbers.FirstOrDefault() != null ? CurrentPerson.PhoneNumbers.FirstOrDefault().Number : "";
            }
            pnlRegistrant.Visible = true;
        }

        /// <summary>
        /// Shows the success panel
        /// </summary>
        private void ShowSuccess( int registrationId )
        {
            lSuccessTitle.Text = "Congratulations";
            lSuccess.Text = "You have successfully completed this registration.";

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var registration = new RegistrationService( rockContext )
                        .Queryable( "RegistrationInstance.RegistrationTemplate" )
                        .FirstOrDefault( r => r.Id == registrationId );

                    if ( registration != null &&
                        registration.RegistrationInstance != null &&
                        registration.RegistrationInstance.RegistrationTemplate != null )
                    {
                        var template = registration.RegistrationInstance.RegistrationTemplate;

                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "CurrentPerson", CurrentPerson );
                        mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                        mergeFields.Add( "Registration", registration );

                        if ( template != null && !string.IsNullOrWhiteSpace( template.SuccessTitle ) )
                        {
                            lSuccessTitle.Text = template.SuccessTitle.ResolveMergeFields( mergeFields );
                        }
                        else
                        {
                            lSuccessTitle.Text = "Congratulations";
                        }

                        if ( template != null && !string.IsNullOrWhiteSpace( template.SuccessText ) )
                        {
                            lSuccess.Text = template.SuccessText.ResolveMergeFields( mergeFields );
                        }
                        else
                        {
                            lSuccess.Text = "You have successfully completed this Registration";
                        }

                        // show debug info
                        if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && UserCanEdit )
                        {
                            lSuccessDebug.Visible = true;
                            lSuccessDebug.Text = mergeFields.lavaDebugInfo();
                        }

                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
            }
        }

        /// <summary>
        /// Shows a warning message.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="text">The text.</param>
        private void ShowWarning( string heading, string text )
        {
            nbMain.Heading = heading;
            nbMain.Text = string.Format( "<p>{0}</p>", text );
            nbMain.NotificationBoxType = NotificationBoxType.Warning;
            nbMain.Visible = true;
        }

        /// <summary>
        /// Shows an error message.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="text">The text.</param>
        private void ShowError( string heading, string text )
        {
            nbMain.Heading = heading;
            nbMain.Text = string.Format( "<p>{0}</p>", text );
            nbMain.NotificationBoxType = NotificationBoxType.Danger;
            nbMain.Visible = true;
        }

        #endregion

        #endregion

    }
}
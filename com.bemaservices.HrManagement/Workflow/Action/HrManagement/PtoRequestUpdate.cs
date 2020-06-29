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
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.HrManagement.Workflow.Action
{
    /// <summary>
    /// Sets an entity property.
    /// </summary>
    [ActionCategory( "BEMA Services > HR Management" )]
    [Description( "Adds or Updates a PTO Request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "PTO Request Update" )]

    // Person Search Fields
    [WorkflowTextOrAttribute( "First Name", "Attribute Value", "The first name or an attribute that contains the first name of the person. <span class='tip tip-lava'></span>",
        false, "", "", 0, FIRST_NAME_KEY, new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Last Name", "Attribute Value", "The last name or an attribute that contains the last name of the person. <span class='tip tip-lava'></span>",
        false, "", "", 1, LAST_NAME_KEY, new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Email Address", "Attribute Value", "The email address or an attribute that contains the email address of the person. <span class='tip tip-lava'></span>",
        false, "", "", 2, EMAIL_KEY, new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType" } )]
    [WorkflowTextOrAttribute( "Mobile Number", "Attribute Value", "The mobile phone number or an attribute that contains the mobile phone number of the person ) <span class='tip tip-lava'></span>",
        false, "", "", 3, MOBILE_NUMBER_KEY, new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PhoneNumberFieldType" } )]

    // Person Search Birth Date Fields
    [WorkflowTextOrAttribute( "Birth Day", "Attribute Value", "The number corresponding to the birth day of a person or the attribute that contains the number corresponding to a birth day for a person  <span class='tip tip-lava'></span>",
        false, "", "", 4, BIRTH_DAY_KEY, new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Birth Month", "Attribute Value", "The number corresponding to the birth month of a person or the attribute that contains the number corresponding to a birth month for a person  <span class='tip tip-lava'></span>",
        false, "", "", 5, BIRTH_MONTH_KEY, new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Birth Year", "Attribute Value", "The number corresponding to the birth year of a person or the attribute that contains the number corresponding to a birth year for a person  <span class='tip tip-lava'></span>",
        false, "", "", 6, BIRTH_YEAR_KEY, new string[] { "Rock.Field.Types.TextFieldType" } )]

    // New Person Config
    [WorkflowAttribute( "Person Attribute", "The person attribute to set the value to the person found or created.",
        true, "", "", 7, PERSON_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Family Attribute", "The group attribute to add the person to.",
        true, "", "", 8, FAMILY_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.GroupFieldType" } )]
    [WorkflowAttribute( "Group Role Attribute", "The group role the person will have.",
        true, "", "", 9, GROUP_ROLE_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.GroupRoleFieldType" } )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Default Record Status", "The record status to use when creating a new person", false, false,
        Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "", 10 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The connection status to use when creating a new person", false, false,
        Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT, "", 11 )]
    public class PtoRequestUpdate : ActionComponent
    {
        private const string FIRST_NAME_KEY = "FirstName";
        private const string LAST_NAME_KEY = "LastName";
        private const string EMAIL_KEY = "Email";
        private const string PERSON_ATTRIBUTE_KEY = "PersonAttribute";
        private const string FAMILY_ATTRIBUTE_KEY = "FamilyAttribute";
        private const string GROUP_ROLE_ATTRIBUTE_KEY = "GroupRoleAttribute";
        private const string DEFAULT_CONNECTION_STATUS_KEY = "DefaultConnectionStatus";
        private const string DEFAULT_RECORD_STATUS_KEY = "DefaultRecordStatus";
        private const string DEFAULT_CAMPUS_KEY = "DefaultCampus";
        private const string MOBILE_NUMBER_KEY = "MobileNumber";
        private const string BIRTH_MONTH_KEY = "BirthMonth";
        private const string BIRTH_DAY_KEY = "BirthDay";
        private const string BIRTH_YEAR_KEY = "BirthYear";

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var attribute = AttributeCache.Get( GetAttributeValue( action, PERSON_ATTRIBUTE_KEY ).AsGuid(), rockContext );
            if ( attribute != null )
            {
                var mergeFields = GetMergeFields( action );
                string firstName = GetAttributeValue( action, FIRST_NAME_KEY, true ).ResolveMergeFields( mergeFields );
                string lastName = GetAttributeValue( action, LAST_NAME_KEY, true ).ResolveMergeFields( mergeFields );
                string email = GetAttributeValue( action, EMAIL_KEY, true ).ResolveMergeFields( mergeFields );
                string mobileNumber = GetAttributeValue( action, MOBILE_NUMBER_KEY, true ).ResolveMergeFields( mergeFields ) ?? string.Empty;

                int? birthDay = GetAttributeValue( action, BIRTH_DAY_KEY, true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();
                int? birthMonth = GetAttributeValue( action, BIRTH_MONTH_KEY, true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();
                int? birthYear = GetAttributeValue( action, BIRTH_YEAR_KEY, true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();

                var groupService = new GroupService( rockContext );
                var familyGroup = groupService.Get( GetAttributeValue( action, FAMILY_ATTRIBUTE_KEY, true ).AsGuid() );
                if ( familyGroup.GroupType.Guid != Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() )
                {
                    errorMessages.Add( "Provided group was not a group of type Family." );
                }
                else
                {
                    var groupRole = familyGroup.GroupType.Roles.Where( gr => gr.Guid == GetAttributeValue( action, GROUP_ROLE_ATTRIBUTE_KEY, true ).AsGuid() ).FirstOrDefault();
                    if ( groupRole == null )
                    {
                        errorMessages.Add( "Invalid or no Group Role provided." );
                    }
                    else
                    {
                        if ( string.IsNullOrWhiteSpace( firstName ) ||
                   string.IsNullOrWhiteSpace( lastName ) )
                        {
                            errorMessages.Add( "First Name and Last Name are required. One or more of these values was not provided!" );
                        }
                        else
                        {
                            Person person = null;
                            PersonAlias personAlias = null;
                            var personService = new PersonService( rockContext );
                            var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, mobileNumber, null, birthMonth, birthDay, birthYear );
                            person = personService.FindPerson( personQuery, true );

                            if ( person.IsNotNull() )
                            {
                                personAlias = person.PrimaryAlias;
                                if ( !familyGroup.Members.Where( gm => gm.PersonId == person.Id ).Any() )
                                {
                                    PersonService.AddPersonToFamily( person, false, familyGroup.Id, groupRole.Id, rockContext );
                                }
                            }
                            else
                            {
                                // Add New Person
                                person = new Person();
                                person.FirstName = firstName.FixCase();
                                person.LastName = lastName.FixCase();
                                person.IsEmailActive = true;
                                person.Email = email;
                                person.EmailPreference = EmailPreference.EmailAllowed;
                                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                                person.BirthMonth = birthMonth;
                                person.BirthDay = birthDay;
                                person.BirthYear = birthYear;

                                UpdatePhoneNumber( person, mobileNumber );

                                var defaultConnectionStatus = DefinedValueCache.Get( GetAttributeValue( action, DEFAULT_CONNECTION_STATUS_KEY ).AsGuid() );
                                if ( defaultConnectionStatus != null )
                                {
                                    person.ConnectionStatusValueId = defaultConnectionStatus.Id;
                                }

                                var defaultRecordStatus = DefinedValueCache.Get( GetAttributeValue( action, DEFAULT_RECORD_STATUS_KEY ).AsGuid() );
                                if ( defaultRecordStatus != null )
                                {
                                    person.RecordStatusValueId = defaultRecordStatus.Id;
                                }

                                PersonService.AddPersonToFamily( person, true, familyGroup.Id, groupRole.Id, rockContext );
                                if ( familyGroup != null && familyGroup.Members.Any() )
                                {
                                    person = familyGroup.Members.Select( m => m.Person ).First();
                                    personAlias = person.PrimaryAlias;
                                }
                            }

                            if ( person != null && personAlias != null )
                            {
                                SetWorkflowAttributeValue( action, attribute.Guid, personAlias.Guid.ToString() );
                                action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, person.FullName ) );
                                return true;
                            }
                            else
                            {
                                errorMessages.Add( "Person or Primary Alias could not be determined!" );
                            }
                        }
                    }
                }
            }
            else
            {
                errorMessages.Add( "Person Attribute could not be found!" );
            }

            if ( errorMessages.Any() )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            return true;
        }

        void UpdatePhoneNumber( Person person, string mobileNumber )
        {
            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( mobileNumber ) ) )
            {
                var phoneNumberType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                if ( phoneNumberType == null )
                {
                    return;
                }

                var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberType.Id );
                string oldPhoneNumber = string.Empty;
                if ( phoneNumber == null )
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberType.Id };
                    person.PhoneNumbers.Add( phoneNumber );
                }
                else
                {
                    oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                }

                // TODO handle country code here
                phoneNumber.Number = PhoneNumber.CleanNumber( mobileNumber );
            }
        }

    }
}

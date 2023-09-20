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
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace org.lakepointe.Workflow.Action.People
{
    /// <summary>
    /// Sets an attribute's value to the selected person 
    /// </summary>
    [ActionCategory( "LPC People" )]
    [Description( "Gets a person by phone number. Creates and returns a nameless person if no matching person is found." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Attribute From Phone" )]

    // Person Search Fields
    [WorkflowTextOrAttribute(
        textLabel: "Mobile Number",
        attributeLabel: "Attribute Value",
        description: "The mobile phone number or an attribute that contains the mobile phone number of the person ) <span class='tip tip-lava'></span>",
        required: false,
        defaultValue: "",
        category: "",
        order: 1,
        key: MOBILE_NUMBER_KEY,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PhoneNumberFieldType" }
    )]

    // New Person Config
    [WorkflowAttribute(
        name: "Person Attribute",
        description: "The person attribute to set the value to the person found or created.",
        required: true,
        defaultValue: "",
        category: "",
        order: 2,
        key: PERSON_ATTRIBUTE_KEY,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.PersonFieldType" }
    )]

    [DefinedValueField(
        definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        name: "Default Record Status",
        description: "The record status to use when creating a new person",
        required: false,
        allowMultiple: false,
        defaultValue: Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        category: "",
        order: 3
    )]

    [DefinedValueField(
        definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        name: "Default Connection Status",
        description: "The connection status to use when creating a new person",
        required: false,
        allowMultiple: false,
        defaultValue: Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        category: "",
        order: 4
    )]

    [WorkflowAttribute(
        name: "Default Campus",
        description: "The attribute value to use as the default campus when creating a new person.",
        required: true,
        defaultValue: "",
        category: "",
        order: 5,
        key: DEFAULT_CAMPUS_KEY,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.CampusFieldType" }
    )]

    public class GetPersonByPhone : ActionComponent
    {
        private const string PERSON_ATTRIBUTE_KEY = "PersonAttribute";
        private const string DEFAULT_CONNECTION_STATUS_KEY = "DefaultConnectionStatus";
        private const string DEFAULT_RECORD_STATUS_KEY = "DefaultRecordStatus";
        private const string DEFAULT_CAMPUS_KEY = "DefaultCampus";
        private const string MOBILE_NUMBER_KEY = "MobileNumber";

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
                string mobileNumber = GetAttributeValue( action, MOBILE_NUMBER_KEY, true ).ResolveMergeFields( mergeFields ) ?? string.Empty;

                if ( string.IsNullOrWhiteSpace( mobileNumber ) )
                {
                    errorMessages.Add( "Mobile Number is required but was not provided!" );
                }
                else
                {
                    Person person = null;
                    PersonAlias personAlias = null;
                    var personService = new PersonService( rockContext );

                    var prefixed = "1" + mobileNumber; // crude, but I don't see an more correct way to get there. The next call requires the country code to get a match, but won't tolerate the + or other formatting characters.
                    person = personService.GetPersonFromMobilePhoneNumber( prefixed, true );

                    if ( person != null )
                    {
                        personAlias = person.PrimaryAlias;

                        if ( person.RecordTypeValueId == DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id )
                        {
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

                            var defaultCampus = CampusCache.Get( GetAttributeValue( action, DEFAULT_CAMPUS_KEY, true ).AsGuid() );
                            var familyGroup = PersonService.SaveNewPerson( person, rockContext, ( defaultCampus != null ? defaultCampus.Id : ( int? ) null ), false );
                            if ( familyGroup != null && familyGroup.Members.Any() )
                            {
                                person = familyGroup.Members.Select( m => m.Person ).First();
                                personAlias = person.PrimaryAlias;
                            }
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
    }
}
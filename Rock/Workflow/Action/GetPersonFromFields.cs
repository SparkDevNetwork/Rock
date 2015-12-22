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
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected person 
    /// </summary>
    [Description( "Sets a person attribute's value to a person with matching name and email. If single match is not found a new person will be created." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Get Person From Fields" )]

    [WorkflowTextOrAttribute( "First Name", "Attribute Value", "The first name or an attribute that contains the first name of the person. <span class='tip tip-lava'></span>",
        false, "", "", 0, "FirstName", new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Last Name", "Attribute Value", "The last name or an attribute that contains the last name of the person. <span class='tip tip-lava'></span>",
        false, "", "", 1, "LastName", new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Email Address", "Attribute Value", "The email address or an attribute that contains the email address of the person. <span class='tip tip-lava'></span>", 
        false, "", "", 2, "Email", new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowAttribute( "Person Attribute", "The person attribute to set the value to the person found or created.", 
        true, "", "", 3, "PersonAttribute", new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Default Record Status", "The record status to use when creating a new person", false, false,
        Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "", 4 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The connection status to use when creating a new person", false, false, 
        Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT, "", 5)]
    [WorkflowAttribute( "Default Campus", "The attribute value to use as the default campus when creating a new person.",
        true, "", "", 6, "DefaultCampus", new string[] { "Rock.Field.Types.CampusFieldType" } )]
    public class GetPersonFromFields : ActionComponent
    {
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

            
            string attributeValue = GetAttributeValue( action, "PersonAttribute" );
            Guid? guid = attributeValue.AsGuidOrNull();
            if (guid.HasValue)
            {
                var attribute = AttributeCache.Read( guid.Value, rockContext );
                if ( attribute != null )
                {
                    string firstName = GetValue( action, "FirstName", rockContext );
                    string lastName = GetValue( action, "LastName", rockContext );
                    string email = GetValue( action, "Email", rockContext );

                    if ( string.IsNullOrWhiteSpace( firstName ) ||
                        string.IsNullOrWhiteSpace( lastName ) ||
                        string.IsNullOrWhiteSpace( email ) )
                    {
                        errorMessages.Add( "First Name, Last Name, and Email are required. One or more of these values was not provided!" );
                    }
                    else
                    { 
                        Person person = null;
                        PersonAlias personAlias = null;
                        var personService = new PersonService( rockContext );
                        var people = personService.GetByMatch( firstName, lastName, email ).ToList();
                        if ( people.Count == 1 )
                        {
                            person = people.First();
                            personAlias = person.PrimaryAlias;
                        }
                        else
                        {
                            // Add New Person
                            person = new Person();
                            person.FirstName = firstName;
                            person.LastName = lastName;
                            person.IsEmailActive = true;
                            person.Email = email;
                            person.EmailPreference = EmailPreference.EmailAllowed;
                            person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                            var defaultConnectionStatus = DefinedValueCache.Read( GetAttributeValue( action, "DefaultConnectionStatus" ).AsGuid() );
                            if ( defaultConnectionStatus != null )
                            {
                                person.ConnectionStatusValueId = defaultConnectionStatus.Id;
                            }

                            var defaultRecordStatus = DefinedValueCache.Read( GetAttributeValue( action, "DefaultRecordStatus" ).AsGuid() );
                            if ( defaultRecordStatus != null )
                            {
                                person.RecordStatusValueId = defaultRecordStatus.Id;
                            }

                            var defaultCampus = CampusCache.Read( GetActionAttributeValue( action, "DefaultCampus" ).AsGuid() );
                            var familyGroup = PersonService.SaveNewPerson( person, rockContext, ( defaultCampus != null ? defaultCampus.Id : (int?)null ), false );
                            if ( familyGroup != null && familyGroup.Members.Any() )
                            {
                                person = familyGroup.Members.Select( m => m.Person ).First();
                                personAlias = person.PrimaryAlias;
                            }
                        }

                        if ( person != null && personAlias != null )
                        {
                            action.Activity.Workflow.SetAttributeValue( attribute.Key, personAlias.Guid.ToString() );
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
                    errorMessages.Add( string.Format( "Person Attribute could not be found for selected attribute value ('{0}')!", guid.Value.ToString() ) );
                }
            }
            else
            {
                errorMessages.Add( string.Format( "Selected Person Attribute value ('{0}') was not a valid Guid!", attributeValue ) );
            }

            if ( errorMessages.Any() )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            return true;
        }

        private string GetValue( WorkflowAction action, string key, RockContext rockContext )
        {
            string value = GetAttributeValue( action, key );
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var attribute = AttributeCache.Read( guid.Value, rockContext );
                if ( attribute != null )
                {
                    return action.GetWorklowAttributeValue( guid.Value );
                }
            }

            return value;
        }
    }
}
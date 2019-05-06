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
using System.Data.Entity;
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
    [ActionCategory( "People" )]
    [Description( "Get's the spouse of the selected person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Get Spouse" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to add the note to.", true, "", "", 0, null, new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Spouse Attribute", "The workflow attribute to assign the spouse to. Spouse is deemed to be the other adult on the family.", true, "", "", 1, null, new string[] { "Rock.Field.Types.PersonFieldType" } )]
    public class PersonGetSpouse : ActionComponent
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

            var adultRoleGuid = SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();

            var maritalStatusMarried = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
            var familyGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var adultRole = familyGroupType.Roles.Where( r => r.Guid == adultRoleGuid ).FirstOrDefault();

            var person = GetPersonAliasFromActionAttribute("Person", rockContext, action, errorMessages);
            if (person != null)
            {
                var spouse = person.GetSpouse( rockContext );
                if ( spouse != null )
                {
                    var spouseAttr = SetWorkflowAttributeValue( action, "SpouseAttribute", spouse.PrimaryAlias.Guid );
                    if ( spouseAttr != null )
                    {
                        action.AddLogEntry( string.Format( "Set spouse attribute '{0}' attribute to '{1}'.", spouseAttr.Name, spouse.FullName ) );
                        return true;
                    }
                    else
                    {
                        errorMessages.Add( "Could not find Spouse Attribute." );
                    }
                }
                else
                {
                    action.AddLogEntry( string.Format( "No spouse found for {0}.", person.FullName ) );
                }
            }
            else
            {
                errorMessages.Add("No person was provided.");
                return false;
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

        private Person GetPersonAliasFromActionAttribute(string key, RockContext rockContext, WorkflowAction action, List<string> errorMessages)
        {
            string value = GetAttributeValue( action, key );
            Guid guidPersonAttribute = value.AsGuid();
            if (!guidPersonAttribute.IsEmpty())
            {
                var attributePerson = AttributeCache.Get( guidPersonAttribute, rockContext );
                if (attributePerson != null)
                {
                    string attributePersonValue = action.GetWorklowAttributeValue(guidPersonAttribute);
                    if (!string.IsNullOrWhiteSpace(attributePersonValue))
                    {
                        if (attributePerson.FieldType.Class == "Rock.Field.Types.PersonFieldType")
                        {
                            Guid personAliasGuid = attributePersonValue.AsGuid();
                            if (!personAliasGuid.IsEmpty())
                            {
                                PersonAliasService personAliasService = new PersonAliasService(rockContext);
                                return personAliasService.Queryable().AsNoTracking()
                                    .Where(a => a.Guid.Equals(personAliasGuid))
                                    .Select(a => a.Person)
                                    .FirstOrDefault();
                            }
                            else
                            {
                                errorMessages.Add(string.Format("Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString()));
                                return null;
                            }
                        }
                        else
                        {
                            errorMessages.Add(string.Format("The attribute used for {0} to provide the person was not of type 'Person'.", key));
                            return null;
                        }
                    }
                }
            }

            return null;
        }

    }
}
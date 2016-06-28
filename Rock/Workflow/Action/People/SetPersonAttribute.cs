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

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected person 
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Sets a person attribute of a given person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Attribute Set" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to update.", true, "", "", 0, null, 
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [AttributeField( "72657ED8-D16E-492E-AC12-144C5E7567E7", "Person Attribute", "The person attribute that should be updated with the provided value.", true, false, "", "", 1 )]
    [WorkflowTextOrAttribute( "Value", "Attribute Value", "The value or attribute value to set the person attribute to. <span class='tip tip-lava'></span>", false, "", "", 2, "Value" )]
    
    public class SetPersonAttribute : ActionComponent
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

            string updateValue = GetAttributeValue( action, "Value" );
            Guid? valueGuid = updateValue.AsGuidOrNull();
            if ( valueGuid.HasValue )
            {
                updateValue = action.GetWorklowAttributeValue( valueGuid.Value );
            }
            else
            {
                updateValue = updateValue.ResolveMergeFields( GetMergeFields( action ) );
            }

            string personAttribute = GetAttributeValue( action, "PersonAttribute" );

            Guid guid = personAttribute.AsGuid();
            if (!guid.IsEmpty())
            {
                var attribute = AttributeCache.Read( guid, rockContext );
                if ( attribute != null )
                {
                    string value = GetAttributeValue( action, "Person" );
                    guid = value.AsGuid();
                    if ( !guid.IsEmpty() )
                    {
                        var attributePerson = AttributeCache.Read( guid, rockContext );
                        if ( attributePerson != null )
                        {
                            string attributePersonValue = action.GetWorklowAttributeValue( guid );
                            if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                            {
                                if ( attributePerson.FieldType.Class == "Rock.Field.Types.PersonFieldType" )
                                {
                                    Guid personAliasGuid = attributePersonValue.AsGuid();
                                    if ( !personAliasGuid.IsEmpty() )
                                    {
                                        var person = new PersonAliasService( rockContext ).Queryable()
                                            .Where( a => a.Guid.Equals( personAliasGuid ) )
                                            .Select( a => a.Person )
                                            .FirstOrDefault();
                                        if ( person != null )
                                        {
                                            // update person attribute
                                            person.LoadAttributes();
                                            string originalValue = person.GetAttributeValue( attribute.Key );
                                            Rock.Attribute.Helper.SaveAttributeValue( person, attribute, updateValue, rockContext );

                                            if ( ( originalValue ?? string.Empty ).Trim() != ( updateValue ?? string.Empty ).Trim() )
                                            {
                                                var changes = new List<string>();

                                                string formattedOriginalValue = string.Empty;
                                                if ( !string.IsNullOrWhiteSpace( originalValue ) )
                                                {
                                                    formattedOriginalValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                                                }

                                                string formattedNewValue = string.Empty;
                                                if ( !string.IsNullOrWhiteSpace( updateValue ) )
                                                {
                                                    formattedNewValue = attribute.FieldType.Field.FormatValue( null, updateValue, attribute.QualifierValues, false );
                                                }

                                                History.EvaluateChange( changes, attribute.Name, formattedOriginalValue, formattedNewValue, attribute.FieldType.Field.IsSensitive() );
                                                if ( changes.Any() )
                                                {
                                                    HistoryService.SaveChanges( rockContext, typeof( Person ), 
                                                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                                        person.Id, changes );
                                                }
                                            }

                                            action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, updateValue ) );
                                            return true;

                                        }
                                        else
                                        {
                                            errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guid.ToString() ) );
                                        }
                                    }
                                }
                                else
                                {
                                    errorMessages.Add( "The attribute used to provide the person was not of type 'Person'." );
                                }
                            }
                        }
                    }
                }
                else
                {
                    errorMessages.Add( string.Format( "Person attribute could not be found for '{0}'!", guid.ToString() ) );
                }
            }
            else
            {
                errorMessages.Add( string.Format( "Selected person attribute ('{0}') was not a valid Guid!", personAttribute ) );
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}
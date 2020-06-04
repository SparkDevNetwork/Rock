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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets the workflow initiator to the value of an attribute.
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Sets the workflow initiator to the value of an attribute." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Initiator Set from Attribute" )]

    [WorkflowAttribute( "Person Attribute", "The person attribute to set to the initiator to.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType" } )]
    public class SetInitiatorFromAttribute : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Get the attribute's guid
            Guid guid = GetAttributeValue( action, "PersonAttribute" ).AsGuid();
            if ( !guid.IsEmpty() )
            {
                // Get the attribute
                var attribute = AttributeCache.Get( guid, rockContext );
                if ( attribute != null )
                {
                    if ( attribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.PERSON.AsGuid(), rockContext ).Id )
                    {
                        // If attribute type is a person, value should be person alias id
                        Guid? personAliasGuid = action.GetWorkflowAttributeValue( guid ).AsGuidOrNull();
                        if ( personAliasGuid.HasValue )
                        {
                            var personAlias = new PersonAliasService( rockContext ).Queryable( "Person" )
                                .Where( a => a.Guid.Equals( personAliasGuid.Value ) )
                                .FirstOrDefault();
                            if ( personAlias != null )
                            {
                                action.Activity.Workflow.InitiatorPersonAlias = personAlias;
                                action.Activity.Workflow.InitiatorPersonAliasId = personAlias.Id;
                                action.AddLogEntry( string.Format( "Assigned initiator to '{0}' ({1})", personAlias.Person.FullName, personAlias.Person.Id ) );
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
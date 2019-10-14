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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected person 
    /// </summary>
    [ActionCategory( "Workflow Attributes" )]
    [Description( "Sets an attribute to the selected person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Attribute Set from Person" )]

    [WorkflowAttribute( "Attribute", "The person attribute to set the value of.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [PersonField( "Person", "The person to set attribute value to. Leave blank to set person to nobody.", false, "", "", 1 )]
    public class SetAttributeFromPerson : ActionComponent
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

            var attribute = AttributeCache.Get( GetAttributeValue( action, "Attribute" ).AsGuid(), rockContext );
            if ( attribute != null )
            {
                Guid? personAliasGuid = GetAttributeValue( action, "Person" ).AsGuidOrNull();
                if ( personAliasGuid.HasValue )
                {
                    var personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid.Value );
                    if ( personAlias != null && personAlias.Person != null )
                    {
                        SetWorkflowAttributeValue( action, attribute.Guid, personAliasGuid.ToString() );
                        action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, personAlias.Person.FullName ) );
                        return true;
                    }
                    else
                    {
                        errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", personAliasGuid.Value.ToString() ) );
                    }
                }
                else
                {
                    SetWorkflowAttributeValue( action, attribute.Guid, string.Empty );
                    action.AddLogEntry( string.Format( "Set '{0}' attribute to nobody.", attribute.Name ) );
                    return true;
                }
            }
            else
            {
                errorMessages.Add( "Attribute could not be found for selected Attribute!" );
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}
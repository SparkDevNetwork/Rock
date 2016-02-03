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
    [ActionCategory( "Set Workflow Attribute" )]
    [Description( "Sets an attribute to the selected person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Attribute from Person" )]

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

            string attributeValue = GetAttributeValue( action, "Attribute" );
            Guid guid = attributeValue.AsGuid();
            if (!guid.IsEmpty())
            {
                var attribute = AttributeCache.Read( guid, rockContext );
                if ( attribute != null )
                {
                    string value = GetAttributeValue( action, "Person" );
                    guid = value.AsGuid();
                    if ( !guid.IsEmpty() )
                    {
                        var personAlias = new PersonAliasService( rockContext ).Get( guid );
                        if ( personAlias != null && personAlias.Person != null )
                        {
                            action.Activity.Workflow.SetAttributeValue( attribute.Key, value );
                            action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, personAlias.Person.FullName ) );
                            return true;
                        }
                        else
                        {
                            errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guid.ToString() ) );
                        }
                    }
                    else
                    {
                        action.Activity.Workflow.SetAttributeValue( attribute.Key, string.Empty );
                        action.AddLogEntry( string.Format( "Set '{0}' attribute to nobody.", attribute.Name ) );
                        return true;
                    }
                }
                else
                {
                    errorMessages.Add( string.Format( "Attribute could not be found for selected attribute value ('{0}')!", guid.ToString() ) );
                }
            }
            else
            {
                errorMessages.Add( string.Format( "Selected attribute value ('{0}') was not a valid Guid!", attributeValue ) );
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}
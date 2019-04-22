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
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets a person attribute equal to the currently logged in person.
    /// </summary>
    [ActionCategory( "Workflow Attributes" )]
    [Description( "Sets an attribute to the currently logged in person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Attribute Set to Current Person" )]

    [WorkflowAttribute( "Person Attribute", "The attribute to set to the currently logged in person.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType" } )]
    public class SetAttributeToCurrentPerson : ActionComponent
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

            // Get the current person alias if possible
            PersonAlias personAlias = null;
            if ( HttpContext.Current != null && HttpContext.Current.Items.Contains( "CurrentPerson" ) )
            {
                var currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
                if ( currentPerson != null && currentPerson.PrimaryAlias != null )
                {
                    personAlias = currentPerson.PrimaryAlias;

                    // Get the attribute to set
                    Guid guid = GetAttributeValue( action, "PersonAttribute" ).AsGuid();
                    if ( !guid.IsEmpty() )
                    {
                        var personAttribute = AttributeCache.Get( guid, rockContext );
                        if ( personAttribute != null )
                        {
                            // If this is a person type attribute
                            if ( personAttribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.PERSON.AsGuid(), rockContext ).Id )
                            {
                                SetWorkflowAttributeValue( action, guid, personAlias.Guid.ToString() );
                            }
                            else if ( personAttribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.TEXT.AsGuid(), rockContext ).Id )
                            {
                                SetWorkflowAttributeValue( action, guid, currentPerson.FullName );
                            }
                        }
                    }
                }
            }

            return true;
        }

    }
}
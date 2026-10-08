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
    [Rock.SystemGuid.EntityTypeGuid( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A")]
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

            // Get the attribute to set
            Guid guid = GetAttributeValue( action, "PersonAttribute" ).AsGuid();
            if ( guid.IsEmpty() )
            {
                return true;
            }

            var personAttribute = AttributeCache.Get( guid, rockContext );
            if ( personAttribute == null )
            {
                return true;
            }

            var isPersonAttribute = personAttribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.PERSON.AsGuid(), rockContext ).Id;
            var isTextAttribute = personAttribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.TEXT.AsGuid(), rockContext ).Id;

            if ( !isPersonAttribute && !isTextAttribute )
            {
                return true;
            }

            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null || currentPerson.PrimaryAlias == null )
            {
                /*
                    10/7/2026 - MSE

                    When there is no current person during a web request, the
                    attribute is reset to its default value instead of being
                    left unchanged.

                    Reason: Ensure the attribute only reflects the current person.
                */
                if ( HttpContext.Current != null )
                {
                    SetWorkflowAttributeValue( action, guid, personAttribute.DefaultValue );
                    action.AddLogEntry( string.Format( "No person is signed in. Set '{0}' attribute to its default value.", personAttribute.Name ) );
                }

                return true;
            }

            if ( isPersonAttribute )
            {
                SetWorkflowAttributeValue( action, guid, currentPerson.PrimaryAlias.Guid.ToString() );
            }
            else
            {
                SetWorkflowAttributeValue( action, guid, currentPerson.FullName );
            }

            return true;
        }

        /// <summary>
        /// Gets the person that is signed in for the current request.
        /// </summary>
        /// <returns>The current person or <c>null</c> if no one is signed in.</returns>
        private static Person GetCurrentPerson()
        {
            if ( HttpContext.Current != null && HttpContext.Current.Items["CurrentPerson"] is Person currentPerson )
            {
                return currentPerson;
            }

            return Rock.Net.RockRequestContextAccessor.Current?.CurrentPerson;
        }
    }
}
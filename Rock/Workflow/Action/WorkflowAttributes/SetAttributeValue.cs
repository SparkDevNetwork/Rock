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
    /// Sets an attribute's value to the selected value
    /// </summary>
    [ActionCategory( "Workflow Attributes" )]
    [Description( "Sets an attribute's value to the selected value." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Attribute Set Value" )]

    [WorkflowAttribute( "Attribute", "The attribute to set the value of." )]
    [WorkflowTextOrAttribute( "Text Value", "Attribute Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", false, "", "", 1, "Value" )]
    public class SetAttributeValue : ActionComponent
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
                string value = value = GetAttributeValue( action, "Value", true ).ResolveMergeFields( GetMergeFields( action ) );
                if ( attribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.ENCRYPTED_TEXT.AsGuid(), rockContext ).Id ||
                    attribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.SSN.AsGuid(), rockContext ).Id )
                {
                    value = Security.Encryption.EncryptString( value );
                }

                SetWorkflowAttributeValue( action, attribute.Guid, value );
                action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, value ) );
            }

            return true;
        }

    }
}
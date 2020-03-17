// <copyright>
// Copyright by BEMA Information Technologies
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


namespace com.bemaservices.WorkflowExtensions.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected value
    /// </summary>
    [ActionCategory( "BEMA Services > Workflow Extensions" )]
    [Description( "Sets multiple attributes' values to the selected values." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Attribute Values Set" )]

    [MatrixField( "7308FDDC-8123-4C22-8DBC-E283F9F12C1D", "Set Workflow Attributes", "", false, "", 0, "Matrix" )]
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
            var attributeMatrixGuid = GetAttributeValue( action, "Matrix" ).AsGuid();
            var attributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid );
            if ( attributeMatrix != null )
            {
                foreach ( AttributeMatrixItem attributeMatrixItem in attributeMatrix.AttributeMatrixItems )
                {
                    attributeMatrixItem.LoadAttributes();

                    var attribute = AttributeCache.Get( attributeMatrixItem.GetAttributeValue( "WorkflowAttribute" ).AsGuid(), rockContext );
                    if ( attribute != null )
                    {
                        string value = value = attributeMatrixItem.GetMatrixAttributeValue( action, "Value", true ).ResolveMergeFields( GetMergeFields( action ) );
                        if ( attribute.FieldTypeId == FieldTypeCache.Get( Rock.SystemGuid.FieldType.ENCRYPTED_TEXT.AsGuid(), rockContext ).Id ||
                            attribute.FieldTypeId == FieldTypeCache.Get( Rock.SystemGuid.FieldType.SSN.AsGuid(), rockContext ).Id )
                        {
                            value = Rock.Security.Encryption.EncryptString( value );
                        }

                        SetWorkflowAttributeValue( action, attribute.Guid, value );
                        action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, value ) );
                    }
                }
            }

            return true;
        }

    }
}
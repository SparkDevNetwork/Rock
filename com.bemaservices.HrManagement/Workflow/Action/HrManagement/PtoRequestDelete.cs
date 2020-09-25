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
using System.Reflection;
using com.bemaservices.HrManagement.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.HrManagement.Workflow.Action
{
    /// <summary>
    /// Sets an entity property.
    /// </summary>
    [ActionCategory( "BEMA Services > HR Management" )]
    [Description( "Deletes a PTO Request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "PTO Request Delete" )]

    // Existing PtoRequest
    [WorkflowAttribute( "Existing Pto Request", "The Pto Request to update.",
        false, "", "", 0, PTO_REQUEST_ATTRIBUTE_KEY, new string[] { "com.bemaservices.HrManagement.Field.Types.PtoRequestFieldType" } )]

    public class PtoRequestDelete : ActionComponent
    {
        private const string PTO_REQUEST_ATTRIBUTE_KEY = "PTO_REQUEST_ATTRIBUTE_KEY";

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
            Guid? ptoRequestGuid = null;
            PtoRequest ptoRequest = null;
            var ptoRequestService = new PtoRequestService( rockContext );

            // get the existing pto request
            Guid ptoRequestAttributeGuid = GetAttributeValue( action, PTO_REQUEST_ATTRIBUTE_KEY ).AsGuid();

            if ( !ptoRequestAttributeGuid.IsEmpty() )
            {
                ptoRequestGuid = action.GetWorklowAttributeValue( ptoRequestAttributeGuid ).AsGuidOrNull();

                if ( ptoRequestGuid.HasValue )
                {
                    ptoRequest = ptoRequestService.Get( ptoRequestGuid.Value );
                    if ( ptoRequest == null )
                    {
                        errorMessages.Add( "The pto request provided does not exist." );
                    }
                }
                else
                {
                    errorMessages.Add( "Invalid pto request provided." );
                }
            }

            if ( ptoRequest != null )
            {
                ptoRequestService.Delete( ptoRequest );
                rockContext.SaveChanges();
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}

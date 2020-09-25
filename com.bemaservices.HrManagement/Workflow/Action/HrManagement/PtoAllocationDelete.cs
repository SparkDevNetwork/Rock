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
    [Description( "Deletes a PTO Allocation." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "PTO Allocation Delete" )]

    // Existing PtoAllocation
    [WorkflowAttribute( "Existing Pto Allocation", "The Pto Allocation to update.",
        false, "", "", 0, PTO_ALLOCATION_ATTRIBUTE_KEY, new string[] { "com.bemaservices.HrManagement.Field.Types.PtoAllocationFieldType" } )]

    public class PtoAllocationDelete : ActionComponent
    {
        private const string PTO_ALLOCATION_ATTRIBUTE_KEY = "PTO_ALLOCATION_ATTRIBUTE_KEY";

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

            Guid? ptoAllocationGuid = null;
            PtoAllocation ptoAllocation = null;
            var ptoAllocationService = new PtoAllocationService( rockContext );

            // get the existing Pto Allocation
            Guid ptoAllocationAttributeGuid = GetAttributeValue( action, PTO_ALLOCATION_ATTRIBUTE_KEY ).AsGuid();

            if ( !ptoAllocationAttributeGuid.IsEmpty() )
            {
                ptoAllocationGuid = action.GetWorklowAttributeValue( ptoAllocationAttributeGuid ).AsGuidOrNull();

                if ( ptoAllocationGuid.HasValue )
                {
                    ptoAllocation = ptoAllocationService.Get( ptoAllocationGuid.Value );
                    if ( ptoAllocation == null )
                    {
                        errorMessages.Add( "The pto allocation could not be found!" );
                    }
                }
                else
                {
                    errorMessages.Add( "The pto allocation is invalid!" );
                }
            }


            if ( ptoAllocation != null )
            {
                ptoAllocationService.Delete( ptoAllocation );

                rockContext.SaveChanges();
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}

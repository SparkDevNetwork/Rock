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
    /// Activates a new activity for a given activity type
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Activates a new activity instance and all of its actions in a different workflow." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Activate Activity in Other Workflow" )]

    [WorkflowAttribute("Activity", "The activity that should be activated", true, fieldTypeClassNames: new string[] { "Rock.Field.Types.WorkflowActivityFieldType" } )]
    [WorkflowTextOrAttribute("Workflow", "Workflow Attribute", "The ID or Guid of the workflow that should be activated", true, key:"WorkflowReference" )]
    public class ActivateOtherActivity : ActionComponent
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

            var workflowActivityGuid = action.GetWorklowAttributeValue( GetAttributeValue( action, "Activity" ).AsGuid() ).AsGuid();
            if ( workflowActivityGuid.IsEmpty() )
            {
                action.AddLogEntry( "Invalid Activity Property", true );
                return false;
            }

            var reference = GetAttributeValue( action, "WorkflowReference", true );
            Rock.Model.Workflow workflow = null;
            if (reference.AsGuidOrNull() != null )
            {
                var referenceGuid = reference.AsGuid();
                workflow = new WorkflowService( rockContext ).Queryable()
                    .Where( w => w.Guid == referenceGuid )
                    .FirstOrDefault();
            }
            else if (reference.AsIntegerOrNull() != null )
            {
                var referenceInt = reference.AsInteger();
                workflow = new WorkflowService( rockContext ).Queryable()
                   .Where( w => w.Id == referenceInt )
                   .FirstOrDefault();
            }
            else
            {
                action.AddLogEntry( "Invalid Workflow Property", true );
                return false;
            }            
            
            var activityType = WorkflowActivityTypeCache.Read( workflowActivityGuid );
            if ( activityType == null )
            {
                action.AddLogEntry( "Invalid Activity Property", true );
                return false;
            }

            WorkflowActivity.Activate( activityType, workflow );
            action.AddLogEntry( string.Format( "Activated new '{0}' activity", activityType.ToString() ) );

            return true;
        }

    }
}
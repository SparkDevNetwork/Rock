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
using System.Data.Entity;
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
    [Description( "Activates a new activity instance and all of its actions in a different workflow when a given attribute matches." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Activate Activity in Other Workflow On Match" )]

    [WorkflowAttribute( "Activity", "The activity that should be activated", true, fieldTypeClassNames: new string[] { "Rock.Field.Types.WorkflowActivityFieldType" } )]
    [WorkflowTextOrAttribute( "Attribute Key to Match", "Attribute Key to Match", "The workflow attribute key to match against in the target workflow.", true, key: "WorkflowAttributeKey" )]
    [WorkflowTextOrAttribute( "Attribute Value to Match", "Attribute Value to Match", "The workflow attribute value to match against in the target workflow.", true, key: "WorkflowAttributeValue" )]
    [Rock.SystemGuid.EntityTypeGuid( "2F192ADD-3222-4BD9-8E2F-CEF338B97EBD" )]
    public class ActivateOtherActivityOnMatch : ActionComponent
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

            var workflowActivityGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "Activity" ).AsGuid() ).AsGuid();
            if ( workflowActivityGuid.IsEmpty() )
            {
                action.AddLogEntry( "Invalid Activity Property", true );
                return false;
            }

            var attributeKey = GetAttributeValue( action, "WorkflowAttributeKey", true );
            var attributeValue = GetAttributeValue( action, "WorkflowAttributeValue", true );
            if ( string.IsNullOrWhiteSpace( attributeKey ) || string.IsNullOrWhiteSpace( attributeValue ) )
            {
                action.AddLogEntry( "Invalid Workflow Property", true );
                return false;
            }

            var activityType = WorkflowActivityTypeCache.Get( workflowActivityGuid );
            if ( activityType == null )
            {
                action.AddLogEntry( "Invalid Activity Property", true );
                return false;
            }

            var entityType = EntityTypeCache.Get( typeof( Rock.Model.Workflow ) );

            // Use new context so only changes made to the activity by this action are persisted
            using ( var newRockContext = new RockContext() )
            {
                var workflowIds = new AttributeValueService( newRockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.Attribute.Key == attributeKey && a.Value == attributeValue && a.Attribute.EntityTypeId == entityType.Id )
                .Select( a => a.EntityId );

                var workflows = new WorkflowService( newRockContext )
                    .Queryable()
                    .Where( w => w.WorkflowType.ActivityTypes.Any( a => a.Guid == activityType.Guid ) && workflowIds.Contains( w.Id ) )
                    .ToList();

                foreach ( var workflow in workflows )
                {
                    WorkflowActivity.Activate( activityType, workflow, newRockContext );
                    action.AddLogEntry( string.Format( "Activated new '{0}' activity in {1} {2}", activityType.ToString(), workflow.TypeName, workflow.WorkflowId ) );
                }

                newRockContext.SaveChanges();
            }

            return true;
        }
    }
}
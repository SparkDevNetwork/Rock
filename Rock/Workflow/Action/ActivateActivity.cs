//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Activates a new activity for a given activity type
    /// </summary>
    [Description( "Activates a new activity for a given activity type" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Activate Activity" )]
    [TextField( 0, "Activity Type", "The activity type to activate", true )]
    public class ActivateActivity : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( WorkflowAction action, IEntity entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            string activityTypeId = GetAttributeValue( action, "ActivityType" );
            if ( String.IsNullOrWhiteSpace( activityTypeId ) )
            {
                action.AddLogEntry( "Invalid Activity Type Property" );
                return false;
            }

            var workflow = action.Activity.Workflow;

            var activityType = workflow.WorkflowType.ActivityTypes
                .Where( a => a.Id.ToString() == activityTypeId).FirstOrDefault();

            if (activityType != null)
            {
                WorkflowActivity.Activate( activityType, workflow );
                action.AddLogEntry( string.Format( "Activated new '{0}' activity", activityType.ToString() ) );
                return true;
            }

            action.AddLogEntry( string.Format( "Could Not activate new '{0}' activity!", activityType.ToString() ) );
            return false;
        }
    }
}
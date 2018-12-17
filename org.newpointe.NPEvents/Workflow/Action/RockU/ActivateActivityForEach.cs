// <copyright>
// Copyright by NewPointe Community Church
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0. If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
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
using Rock.Workflow;

namespace org.newpointe.RockU.Workflow.Action.RockU
{
    /// <summary>
    /// Activates a new activity for a given activity type
    /// </summary>
    [ActionCategory( "RockU" )]
    [Description( "Activates a new activity instance and all of its actions for each item in a list." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Activate Activities For Items" )]

    [WorkflowTextOrAttribute( "Item List", "Item List Attribute", "A \"|\" seperated list of values to activate activities for. <span class='tip tip-lava'></span>", true, "", "", 0, "ItemList", new string[] { "Rock.Field.Types.ValueListFieldType" } )]
    [WorkflowActivityType( "Activity", "The activity type to activate", true, "", "", 1 )]
    [TextField( "Activity Attribute Key", "The key of the attribute where the current item will be stored in each activated activity.", false, "", "", 2 )]
    public class ActivateActivityForEach : ActionComponent
    {

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var itemListValue = GetAttributeValue( action, "ItemList" );
            Guid itemListValueAsGuid = itemListValue.AsGuid();
            if (!itemListValueAsGuid.IsEmpty())
            {
                var workflowAttributeValue = action.GetWorklowAttributeValue( itemListValueAsGuid );
                if (workflowAttributeValue != null)
                {
                    itemListValue = workflowAttributeValue;
                }
            }
            else
            {
                itemListValue = itemListValue.ResolveMergeFields( GetMergeFields( action ) );
            }

            if (string.IsNullOrWhiteSpace( itemListValue ))
            {
                action.AddLogEntry( "List is empty, not activating any activities.", true );
                return true;
            }


            Guid guid = GetAttributeValue( action, "Activity" ).AsGuid();
            if (guid.IsEmpty())
            {
                action.AddLogEntry( "Invalid Activity Property", true );
                return false;
            }

            var workflow = action.Activity.Workflow;

            var activityType = new WorkflowActivityTypeService( rockContext ).Queryable()
                .Where( a => a.Guid.Equals( guid ) ).FirstOrDefault();

            if (activityType == null)
            {
                action.AddLogEntry( "Invalid Activity Property", true );
                return false;
            }

            string activityAttributeKey = GetAttributeValue( action, "ActivityAttributeKey" );
            bool hasActivityAttributeKey = !string.IsNullOrWhiteSpace( activityAttributeKey );

            foreach (var item in itemListValue.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries ))
            {
                var activity = WorkflowActivity.Activate( activityType, workflow );
                if (hasActivityAttributeKey)
                {
                    activity.SetAttributeValue( activityAttributeKey, item );
                }
                action.AddLogEntry( string.Format( "Activated new '{0}' activity", activityType.ToString() ) );
            }

            return true;
        }
    }
}

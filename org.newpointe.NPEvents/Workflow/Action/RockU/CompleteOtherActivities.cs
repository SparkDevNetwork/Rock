// <copyright>
// Copyright by NewPointe Community Church
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0. If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// </copyright>
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Data;
using Rock.Model;
using Rock.Workflow;

namespace org.newpointe.RockU.Workflow.Action.RockU
{
    /// <summary>
    /// Activates a new activity for a given activity type
    /// </summary>
    [ActionCategory( "RockU" )]
    [Description( "Completes all other activities in the current workflow." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Complete Other Activities" )]

    class CompleteOtherActivities : ActionComponent
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

            foreach (var activity in action.Activity.Workflow.Activities)
            {
                if (activity != action.Activity)
                {
                    activity.MarkComplete();
                }
            }

            return true;
        }
    }
}

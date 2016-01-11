// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Assigns the current activity to the selected group.
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Assigns the current activity to the selected group." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Assign Activity to Group" )]

    [GroupTypeGroupField( "Group", "Select group type, then group, to set the group to assign this activity to.", "Group")] 
    public class AssignActivityToGroup : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var parts = ( GetAttributeValue( action, "Group" ) ?? string.Empty ).Split( '|' );
            Guid? groupTypeGuid = null;
            Guid? groupGuid = null;
            if ( parts.Length >= 1 )
            {
                groupTypeGuid = parts[0].AsGuidOrNull();
                if ( parts.Length >= 2 )
                {
                    groupGuid = parts[1].AsGuidOrNull();
                }
            }

            if ( groupGuid.HasValue )
            {
                var group = new GroupService( rockContext ).Get( groupGuid.Value );
                if ( group != null )
                {
                    action.Activity.AssignedPersonAlias = null;
                    action.Activity.AssignedPersonAliasId = null;
                    action.Activity.AssignedGroup = group;
                    action.Activity.AssignedGroupId = group.Id;
                    action.AddLogEntry( string.Format( "Assigned activity to '{0}' group ({1})", group.Name, group.Id ) );
                    return true;
                }
            }

            return false;
        }
    }
}
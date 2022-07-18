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
    /// Group Member Requirement Met
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Completes a requirement for a person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Requirement Met" )]

    [PersonField( "Person", "The person to assign this activity to.")]
    [WorkflowTextOrAttribute( "Group Member Requirement", "Attribute Value", "The group member requirement or attribute from which to set the value. <span class='tip tip-lava'></span>", false, "", "", 1, "Value" )]
    [Rock.SystemGuid.EntityTypeGuid( "8BE862EE-B540-4CAC-92F1-36FC067C7D3C" )]
    public class GroupMemberRequirementMet : ActionComponent
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
            //var requirement = GroupRequirement
            var attribute = AttributeCache.Get( GetAttributeValue( action, "Attribute" ).AsGuid(), rockContext );
            if ( attribute != null )
            {
                string value = GetAttributeValue( action, "Value", true ).ResolveMergeFields( GetMergeFields( action ) );

                SetWorkflowAttributeValue( action, attribute.Guid, value );
                action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, value ) );
            }

            return true;
        }

    }
}
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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Group Member Requirement Met
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Completes a requirement for a person." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Requirement Met" )]

    [WorkflowAttribute( "Person",
        Description = "The attribute that contains the person to mark the requirement met.",
        IsRequired = true,
        DefaultValue = "",
        Category = "",
        Order = 0,
        Key = AttributeKey.PersonAttribute,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowTextOrAttribute(
        "Group Member Requirement",
        "Attribute Value",
        Description = "The group member requirement to mark as met. This can be either the GUID of the requirement or a workflow attribute with the requirement. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "",
        Category = "",
        Order = 1,
        Key = AttributeKey.GroupMemberRequirement,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupMemberRequirementFieldType" } )]
    [Rock.SystemGuid.EntityTypeGuid( "8BE862EE-B540-4CAC-92F1-36FC067C7D3C" )]
    public class GroupMemberRequirementMet : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string PersonAttribute = "PersonAttribute";
            public const string GroupMemberRequirement = "GroupMemberRequirement";
        }

        #endregion

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

            // Get the person
            PersonAlias personAlias = null;
            Guid personAliasGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, AttributeKey.PersonAttribute ).AsGuid() ).AsGuid();
            personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid );
            if ( personAlias == null )
            {
                errorMessages.Add( "Invalid Person Attribute or Value." );
                return false;
            }

            var groupMemberRequirementGuid = GetAttributeValue( action, AttributeKey.GroupMemberRequirement, true ).AsGuidOrNull();
            if ( groupMemberRequirementGuid != null )
            {
                var groupMemberRequirement = new GroupMemberRequirementService( rockContext ).Queryable()
                        .Where( g => g.Guid == groupMemberRequirementGuid.Value )
                        .FirstOrDefault();

                action.AddLogEntry( $"Set '{groupMemberRequirementGuid}' group member requirement to 'Met'." );

                if ( groupMemberRequirement != null )
                {
                    groupMemberRequirement.RequirementMetDateTime = RockDateTime.Now;
                    int personAliasId = personAlias.Id;
                    groupMemberRequirement.ManuallyCompletedByPersonAliasId = personAliasId;
                    groupMemberRequirement.WasManuallyCompleted = true;
                    groupMemberRequirement.ManuallyCompletedDateTime = RockDateTime.Now;

                    rockContext.SaveChanges();
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}
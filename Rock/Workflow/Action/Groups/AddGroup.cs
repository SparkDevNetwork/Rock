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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.Workflow.Action.Groups
{
    /// <summary>
    /// Adds a new group to a parent group
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Adds a new group to a parent group." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Add" )]

    [GroupTypeField( "Group Type",
        Description = "The group type to create the group with.",
        Key = AttributeKey.GroupType,
        IsRequired = true,
        Order = 0 )]

    [WorkflowAttribute(
        "Parent Group Attribute",
        Description = "The workflow attribute that contains the parent group.",
        Key = AttributeKey.ParentGroup,
        IsRequired = true,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupFieldType" },
        Order = 1 )]

    [WorkflowAttribute(
        "Campus",
        Description = "The workflow attribute that contains the campus to apply to the group.",
        Key = AttributeKey.Campus,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.CampusFieldType" },
        Order = 2 )]

    [WorkflowTextOrAttribute( "Group Name",
         "Group Name Attribute",
        Description = "The group name or attribute that contains the value for the name. <span class='tip tip-lava'></span>. The group name is required.",
        Key = AttributeKey.GroupName,
        IsRequired = true,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" },
        Order = 3 )]

    [WorkflowTextOrAttribute( "Group Description",
         "Group Name Description",
        Description = "The group description or attribute that contains the value for the description.",
        Key = AttributeKey.GroupDescription,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" },
        Order = 4 )]

    [BooleanField( "Is Security Role",
        Key = AttributeKey.IsSecurityRole,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Description = "When checked this will mark the Group as a Security Role even though it isn't a SecurityRole Group Type.",
        Order = 5 )]

    [WorkflowAttribute(
        "Group Attribute",
        Description = "The workflow attribute to hold the group after it is created",
        Key = AttributeKey.GroupAttribute,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupFieldType" },
        Order = 6 )]

    [Rock.SystemGuid.EntityTypeGuid( "BC236FD2-4BC1-4B1C-AB48-4DA43C5147D4" )]
    public class AddGroup : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string GroupType = "GroupType";
            public const string ParentGroup = "ParentGroup";
            public const string IsSecurityRole = "IsSecurityRole";
            public const string Campus = "Campus";
            public const string GroupName = "GroupName";
            public const string GroupDescription = "GroupDescription";
            public const string GroupAttribute = "GroupAttribute";
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
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var service = new GroupService( rockContext );

            // Get the merge fields
            var mergeFields = GetMergeFields( action );

            // Get GroupType
            var groupTypeGuid = GetAttributeValue( action, AttributeKey.GroupType, false ).AsGuid();
            var groupType = GroupTypeCache.Get( groupTypeGuid );

            if ( groupType == null )
            {
                var errorMessage = "The GroupType provided does not exist.";
                action.AddLogEntry( errorMessage );
                errorMessages.Add( errorMessage );
                return false;
            }

            // Get the Parent Group
            int? parentGroupId = null;
            Guid groupAttributeGuid = GetAttributeValue( action, AttributeKey.ParentGroup ).AsGuid();

            if ( !groupAttributeGuid.IsEmpty() )
            {
                var groupGuid = action.GetWorkflowAttributeValue( groupAttributeGuid ).AsGuidOrNull();

                if ( !groupGuid.HasValue )
                {
                    errorMessages.Add( "Invalid group provided." );
                    return false;
                }

                parentGroupId = new GroupService( rockContext ).GetId( groupGuid.Value );
                if ( parentGroupId == null )
                {
                    errorMessages.Add( "The group provided does not exist." );
                    return false;
                }
            }

            // Get Campus
            int? campusId = null;
            Guid? campusAttributeGuid = GetAttributeValue( action, AttributeKey.Campus ).AsGuidOrNull();
            if ( campusAttributeGuid.HasValue )
            {
                Guid? campusGuid = action.GetWorkflowAttributeValue( campusAttributeGuid.Value ).AsGuidOrNull();
                if ( campusGuid.HasValue )
                {
                    var campus = CampusCache.Get( campusGuid.Value );
                    if ( campus != null )
                    {
                        campusId = campus.Id;
                    }
                }
            }

            var groupName = GetAttributeValue( action, AttributeKey.GroupName, true ).ResolveMergeFields( mergeFields );
            if ( string.IsNullOrWhiteSpace( groupName ) )
            {
                var errorMessage = "A group name was not provided for the group.";
                action.AddLogEntry( errorMessage );
                errorMessages.Add( errorMessage );
                return false;
            }

            var isSecurityRole = GetAttributeValue( action, AttributeKey.IsSecurityRole, true ).AsBoolean();
            var groupDescription = GetAttributeValue( action, AttributeKey.GroupDescription, true );

            var group = new Group()
            {
                GroupTypeId = groupType.Id,
                Name = groupName,
                Description = groupDescription,
                IsSecurityRole = isSecurityRole,
                CampusId = campusId,
                ParentGroupId = parentGroupId
            };

            service.Add( group );
            rockContext.SaveChanges();

            action.AddLogEntry( $"{groupName} Group created." );

            // If group attribute was specified, set the attribute's value
            SetWorkflowAttributeValue( action, AttributeKey.GroupAttribute, group.Guid );

            return true;
        }
    }
}

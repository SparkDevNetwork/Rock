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
    /// Gets a person's <see cref="Campus"/> team member of the specified role
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Gets a Person's Campus team member of the selected role." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Get Campus Team Member" )]

    #region Block Attributes

    [WorkflowAttribute( "Person",
        Key = AttributeKey.Person,
        Description = "Workflow attribute that contains the person to get the Campus team member for.",
        IsRequired = false,
        Order = 0,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute( "Campus",
        Key = AttributeKey.Campus,
        Description = "Workflow attribute that contains the Campus to get the Campus team member for. If both Person and Campus are provided, Campus takes precedence over the Person's Campus. If Campus is not provided, the Person's primary Campus will be assigned to this attribute.",
        IsRequired = false,
        Order = 1,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.CampusFieldType" } )]

    [WorkflowAttribute( "Campus Role",
        Key = AttributeKey.CampusRole,
        Description = "Workflow attribute that contains the Role of the Campus team member to get. If multiple team members are in this role for a given Campus, the first match will be selected.",
        IsRequired = true,
        Order = 2,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupRoleFieldType" } )]

    [WorkflowAttribute( "Campus Team Member",
        Description = "Workflow attribute to assign the Campus team member to.",
        Key = AttributeKey.CampusTeamMember,
        IsRequired = true,
        Order = 3,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    #endregion

    public class PersonGetCampusTeamMember : ActionComponent
    {
        #region Workflow Attributes

        private static class AttributeKey
        {
            public const string Person = "Person";
            public const string Campus = "Campus";
            public const string CampusRole = "CampusRole";
            public const string CampusTeamMember = "CampusTeamMember";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            string msg;

            var person = GetPersonFromAttributeValue( action, AttributeKey.Person, true, rockContext );
            var campus = GetEntityFromAttributeValue<Campus>( action, AttributeKey.Campus, true, rockContext );

            if ( person == null && campus == null )
            {
                return HandleError( "Neither a Person nor a Campus was provided. You must provide at least one of these.", action, errorMessages );
            }

            var campusRole = GetEntityFromAttributeValue<GroupTypeRole>( action, AttributeKey.CampusRole, true, rockContext );
            if ( campusRole == null )
            {
                return HandleError( "A CampusRole was not provided.", action, errorMessages );
            }

            if ( campus == null )
            {
                // look for the Campus on the Person entity first
                campus = person.PrimaryCampus;

                if ( campus == null )
                {
                    // if the Person's PrimaryCampus was not defined, grab the Campus from the Person's primary family
                    var family = person.GetFamily( rockContext );

                    msg = string.Format( "Could not find {0}'s Campus.", person.FullName );

                    if ( family == null )
                    {
                        return HandleError( msg, action, errorMessages );
                    }

                    campus = family.Campus;
                    if ( campus == null )
                    {
                        return HandleError( msg, action, errorMessages );
                    }
                }

                var campusAttribute = SetWorkflowAttributeValue( action, AttributeKey.Campus, campus.Guid );
                if ( campusAttribute != null )
                {
                    action.AddLogEntry( string.Format( "Set '{0}' Attribute to '{1}'.", AttributeKey.Campus, campus.Name ) );
                }
            }

            msg = string.Format( "The '{0}' Campus does not have any members in the '{1}' role.", campus.Name, campusRole.Name );

            if ( campus.TeamGroup == null || campus.TeamGroup.Members == null )
            {
                return HandleError( msg, action, errorMessages );
            }

            var campusTeamMember = campus.TeamGroup
                .ActiveMembers()
                .Where( m => m.GroupRoleId == campusRole.Id )
                .FirstOrDefault();

            if ( campusTeamMember == null )
            {
                return HandleError( msg, action, errorMessages );
            }

            if ( campusTeamMember.Person == null || campusTeamMember.Person.PrimaryAlias == null )
            {
                HandleError( "Campus team member could not be found.", action, errorMessages );
            }

            var campusTeamMemberAttribute = SetWorkflowAttributeValue( action, AttributeKey.CampusTeamMember, campusTeamMember.Person.PrimaryAlias.Guid );
            if ( campusTeamMemberAttribute == null )
            {
                HandleError( string.Format( "Could not find '{0}' Attribute.", AttributeKey.CampusTeamMember ), action, errorMessages );
            }

            action.AddLogEntry( string.Format( "Set '{0}' Attribute to '{1}'.", AttributeKey.CampusTeamMember, campusTeamMember.Person.FullName ) );

            return true;
        }

        private bool HandleError( string msg, WorkflowAction action, List<string> errorMessages )
        {
            errorMessages.Add( msg );

            return false;
        }
    }
}

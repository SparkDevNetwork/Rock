﻿// <copyright>
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
    /// Sets an attribute's value to the selected person 
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Adds person to a specific group." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Add" )]

    [WorkflowAttribute( "Person", "Workflow attribute that contains the person to add to the group.", true, "", "", 0, null, 
        new string[] { "Rock.Field.Types.PersonFieldType" })]

    [GroupAndRoleFieldAttribute( "Group and Role", "Group/Role to add the person to. Leave role blank to use the default role for that group.", "Group", true, "", "", 1, "GroupAndRole" )]
    [EnumField( "Group Member Status", "The  status to set the user to in the group.", typeof( GroupMemberStatus ), true, "1", "", 2 )]
    [BooleanField("Update Existing", "If the selected person already belongs to the selected group, should their current role and status be updated to reflect the configured values above.", true, "", 3)]
    public class AddPersonToGroup : ActionComponent
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

            // Determine which group to add the person to
            Group group = null;
            int? groupRoleId = null;

            var groupAndRoleValues = ( GetAttributeValue( action, "GroupAndRole" ) ?? string.Empty ).Split( '|' );
            if ( groupAndRoleValues.Count() > 1 )
            {
                var groupGuid = groupAndRoleValues[1].AsGuidOrNull();
                if ( groupGuid.HasValue )
                {
                    group = new GroupService( rockContext ).Get( groupGuid.Value );
                         
                    if ( groupAndRoleValues.Count() > 2 )
                    {
                        var groupTypeRoleGuid = groupAndRoleValues[2].AsGuidOrNull();
                        if ( groupTypeRoleGuid.HasValue )
                        {
                            var groupRole = new GroupTypeRoleService( rockContext ).Get( groupTypeRoleGuid.Value );
                            if ( groupRole != null )
                            {
                                groupRoleId = groupRole.Id;
                            }
                        }
                    }

                    if ( !groupRoleId.HasValue && group != null )
                    {
                        // use the group's grouptype's default group role if a group role wasn't specified
                        groupRoleId = group.GroupType.DefaultGroupRoleId;
                    }
                }
            }

            if ( group == null )
            {
                errorMessages.Add( "No group was provided" );
            }

            if ( !groupRoleId.HasValue )
            {
                errorMessages.Add( "No group role was provided and group doesn't have a default group role" );
            }

            // determine the person that will be added to the group
            Person person = null;

            // get the Attribute.Guid for this workflow's Person Attribute so that we can lookup the value
            var guidPersonAttribute = GetAttributeValue( action, "Person" ).AsGuidOrNull();

            if ( guidPersonAttribute.HasValue )
            {
                var attributePerson = AttributeCache.Get( guidPersonAttribute.Value, rockContext );
                if ( attributePerson != null )
                {
                    string attributePersonValue = action.GetWorklowAttributeValue( guidPersonAttribute.Value );
                    if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                    {
                        if ( attributePerson.FieldType.Class == typeof( Rock.Field.Types.PersonFieldType ).FullName )
                        {
                            Guid personAliasGuid = attributePersonValue.AsGuid();
                            if ( !personAliasGuid.IsEmpty() )
                            {
                                person = new PersonAliasService( rockContext ).Queryable()
                                    .Where( a => a.Guid.Equals( personAliasGuid ) )
                                    .Select( a => a.Person )
                                    .FirstOrDefault();
                            }
                        }
                        else
                        {
                            errorMessages.Add( "The attribute used to provide the person was not of type 'Person'." );
                        }
                    }
                }
            }

            if ( person == null )
            {
                errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
            }

            // Add Person to Group
            if ( !errorMessages.Any() )
            {
                var status = this.GetAttributeValue( action, "GroupMemberStatus" ).ConvertToEnum<GroupMemberStatus>( GroupMemberStatus.Active );

                var groupMemberService = new GroupMemberService( rockContext );
                var groupMember = groupMemberService.GetByGroupIdAndPersonIdAndPreferredGroupRoleId( group.Id, person.Id, groupRoleId.Value );
                bool isNew = false;
                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    groupMember.PersonId = person.Id;
                    groupMember.GroupId = group.Id;
                    groupMember.GroupRoleId = groupRoleId.Value;
                    groupMember.GroupMemberStatus = status;
                    isNew = true;
                }
                else
                {
                    if ( GetAttributeValue( action, "UpdateExisting" ).AsBoolean() )
                    {
                        groupMember.GroupRoleId = groupRoleId.Value;
                        groupMember.GroupMemberStatus = status;
                    }
                    action.AddLogEntry( $"{person.FullName} was already a member of the selected group.", true );
                }

                if ( groupMember.IsValidGroupMember( rockContext ) )
                {
                    if (isNew)
                    {
                        groupMemberService.Add(groupMember);
                    }
                    rockContext.SaveChanges();
                }
                else
                {
                    // if the group member couldn't be added (for example, one of the group membership rules didn't pass), add the validation messages to the errormessages
                    errorMessages.AddRange( groupMember.ValidationResults.Select( a => a.ErrorMessage ) );
                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}
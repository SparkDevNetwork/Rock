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

namespace org.newpointe.ExtraActions
{
    /// <summary>
    /// Adds person to a group using a workflow attribute.
    /// </summary>
    [ActionCategory( "Extra Actions" )]
    [Description( "Adds a person to a group (Ignores if they are already in the group with the same role)." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Add Person To Group" )]

    [PersonField( "Person", "The Person to add to the Group.", false )]
    [WorkflowAttribute( "Person Attribute", "Workflow Attribute that contains the Person to add to the Group.", false, "", "", 1, null, new[] { "Rock.Field.Types.PersonFieldType" } )]

    [GroupField( "Group", "The Group to add the Person to.", false, "", "", 2 )]
    [WorkflowAttribute( "Group Attribute", "Workflow Attribute that contains the Group to add the Person to.", false, "", "", 3, null, new[] { "Rock.Field.Types.GroupFieldType" } )]

    [GroupRoleField( "", "Group Role", "The Group Role to add the Person with.", false, "", "", 4 )]
    [WorkflowAttribute( "Group Role Attribute", "Workflow Attribute that contains the Group Role to add the Person with.", false, "", "", 5, null, new[] { "Rock.Field.Types.GroupRoleFieldType" } )]

    [EnumField( "Group Member Status", "The Member Status to set.", typeof( GroupMemberStatus ), false, "Active", "", 6 )]
    [WorkflowAttribute( "Group Member Status Attribute", "Workflow Attribute that contains the Member Status to set.", false, "", "", 7 )]
    public class AddPersonToGroup : ExtraActionComponent
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

            // Get the Person, Group and GroupRole from the attributes
            Person person = GetEntityFromWorkflowAttributes( action, "Person", "PersonAttribute", new PersonAliasService( rockContext ) )?.Person;
            Group group = GetEntityFromWorkflowAttributes( action, "Group", "GroupAttribute", new GroupService( rockContext ) );
            GroupTypeRole groupRole = GetEntityFromWorkflowAttributes( action, "GroupRole", "GroupRoleAttribute", new GroupTypeRoleService( rockContext ) ) ?? group?.GroupType?.DefaultGroupRole;


            GroupMemberStatus? groupMemberStatus = null;

            Guid? groupMemberWorkflowAttributeAttribute = GetAttributeValue( action, "GroupMemberStatusAttribute" ).AsGuidOrNull();
            if ( groupMemberWorkflowAttributeAttribute.HasValue )
            {
                string groupMemberWorkflowAttribute = action.GetWorklowAttributeValue( groupMemberWorkflowAttributeAttribute.Value );
                if ( !string.IsNullOrWhiteSpace( groupMemberWorkflowAttribute ) )
                {
                    if ( Enum.TryParse( groupMemberWorkflowAttribute, true, out GroupMemberStatus groupMemberStatusOut ) )
                    {
                        groupMemberStatus = groupMemberStatusOut;
                    }
                }
            }

            if ( groupMemberStatus == null )
            {
                string groupMemberAttribute = GetAttributeValue( action, "GroupMemberStatus" );
                if ( !string.IsNullOrWhiteSpace( groupMemberAttribute ) )
                {
                    if ( Enum.TryParse( groupMemberAttribute, true, out GroupMemberStatus groupMemberStatusOut ) )
                    {
                        groupMemberStatus = groupMemberStatusOut;
                    }
                }
            }


            if ( person == null )
                errorMessages.Add( "Invalid Person." );

            if ( group == null )
                errorMessages.Add( "Invalid Group." );

            if ( groupRole == null )
                errorMessages.Add( "Invalid Group Role." );

            if ( person == null || group == null || groupRole == null || errorMessages.Any() )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }


            if ( group.Members.Any( m => m.PersonId == person.Id && m.GroupRoleId == groupRole.Id ) )
            {
                action.AddLogEntry( "Skipping adding duplicate Group Member." );
            }
            else
            {
                var groupMember = new GroupMember
                {
                    PersonId = person.Id,
                    GroupId = group.Id,
                    GroupRoleId = groupRole.Id,
                    GroupMemberStatus = groupMemberStatus ?? GroupMemberStatus.Pending
                };

                if ( groupMember.IsValid )
                {
                    new GroupMemberService( rockContext ).Add( groupMember );
                    rockContext.SaveChanges();
                }
                else
                {
                    // If the group member couldn't be added (for example, one of the group membership rules didn't pass), add the validation messages to the errormessages
                    errorMessages.AddRange( groupMember.ValidationResults.Select( a => a.ErrorMessage ) );
                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
            return true;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.reallifeministries.RockExtensions.Workflow.Action
{
    [Description( "Adds a Person to a Group" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Add Person to Group" )]

    [WorkflowAttribute( "PersonAttribute", "The workflow attribute containing the person.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Group Attribute", "The workflow attribute containing the group.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.GroupFieldType" } )]
    [GroupRoleField( "", "Group Role", "", true, "", "", 2, "Group Role" )]
    [EnumField( "Member Status", "", typeof( GroupMemberStatus ), true, "", "", 3, "MemberStatus" )]
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
            var groupAttribute = GetAttributeValue( action, "GroupAttribute" );
            Guid groupAttrGuid = groupAttribute.AsGuid();

            var personAttribute = GetAttributeValue( action, "PersonAttribute" );
            Guid personAttrGuid = personAttribute.AsGuid();

            var groupRoleAttr = GetAttributeValue( action, "Group Role" );
            Guid groupRoleGuid = groupRoleAttr.AsGuid();
            var memberStatus = (GroupMemberStatus)Enum.Parse( typeof( GroupMemberStatus ), GetAttributeValue( action, "MemberStatus" ) );

            if (!groupAttrGuid.IsEmpty())
            {
                string attributeGroupValue = action.GetWorklowAttributeValue( groupAttrGuid );
                Guid groupGuid = attributeGroupValue.AsGuid();

                if (!personAttrGuid.IsEmpty())
                {
                    string attributePersonValue = action.GetWorklowAttributeValue( personAttrGuid );
                    Guid personAliasGuid = attributePersonValue.AsGuid();

                    if (!groupRoleGuid.IsEmpty())
                    {
                        if (!groupGuid.IsEmpty())
                        {
                            if (!personAliasGuid.IsEmpty())
                            {
                                using (var ctx = new RockContext())
                                {
                                    var group = (new GroupService( ctx )).Get( groupGuid );
                                    var personAlias = (new PersonAliasService( ctx )).Get( personAliasGuid );
                                    var groupRole = (new GroupTypeRoleService( ctx )).Get( groupRoleGuid );


                                    if (groupRole != null)
                                    {

                                        var groupMemberService = new GroupMemberService( ctx );
                                        var groupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( group.Id, personAlias.Person.Id, groupRole.Id );
                                        if (groupMember == null)
                                        {
                                            groupMember = new GroupMember();
                                            groupMemberService.Add( groupMember );
                                        }
                                        else
                                        {
                                            action.AddLogEntry( string.Format( "{0} is already in group {1} as {2} with status {3}", personAlias.Person.FullName, group.Name, groupRole.Name, memberStatus.GetDescription() ) );
                                        }

                                        groupMember.Person = personAlias.Person;
                                        groupMember.Group = group;
                                        groupMember.GroupRole = groupRole;
                                        groupMember.GroupMemberStatus = memberStatus;

                                        ctx.SaveChanges();

                                        action.AddLogEntry( string.Format( "{0} added to group {1} as {2} with status {3}", personAlias.Person.FullName, group.Name, groupRole.Name, memberStatus.GetDescription() ) );
                                        return true;
                                    }
                                    else
                                    {
                                        action.AddLogEntry( string.Format( "GroupRole does not exist: {0}", groupRoleGuid ) );
                                    }

                                }
                            }
                            else
                            {
                                action.AddLogEntry( "Person attribute, person does not exist" );
                            }
                        }
                        else
                        {
                            action.AddLogEntry( "Group attribute, Group does not exist" );
                        }
                    }
                }
            }

            return false;
        }
    }
}

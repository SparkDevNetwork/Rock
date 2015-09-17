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
    [Description( "Assigns the activity to a role within a Group" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Assign to Group Role" )]

    [WorkflowAttribute( "Group Attribute", "The workflow attribute containing the group.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.GroupFieldType" } )]
    [GroupRoleField("","Group Role","",true,"","",0,"Group Role")]

    public class AssignToGroupRole : ActionComponent
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
            var groupAttribute =  GetAttributeValue( action, "GroupAttribute" );
            Guid groupAttrGuid = groupAttribute.AsGuid();

            if (!groupAttrGuid.IsEmpty())
            {
                
                string attributeGroupValue = action.GetWorklowAttributeValue( groupAttrGuid );
                Guid groupGuid = attributeGroupValue.AsGuid();

                var groupRoleAttr = GetAttributeValue( action, "Group Role" );
                Guid groupRoleGuid = groupRoleAttr.AsGuid();

                if (!groupRoleGuid.IsEmpty())
                {  
                    var groupM = (new GroupService( rockContext )).Get( groupGuid );
                    if (groupM != null)
                    {
                        var groupRole = (new GroupTypeRoleService( rockContext )).Get( groupRoleGuid );
                        var person = (from m in groupM.Members 
                                        where m.GroupRoleId == groupRole.Id 
                                        select m.Person).FirstOrDefault();

                        if (person != null)
                        {
                            action.Activity.AssignedPersonAlias = person.PrimaryAlias;
                            action.Activity.AssignedPersonAliasId = person.PrimaryAliasId;
                            action.Activity.AssignedGroup = null;
                            action.Activity.AssignedGroupId = null;

                            action.AddLogEntry( string.Format( "Assigned activity to '{0}' ({1})", person.FullName, person.Id ) );
                            return true;
                        }
                        else
                        {
                            action.AddLogEntry( string.Format( "Nobody assigned to Role ({0}) for Group ({1})", groupRole.Name, groupM.Name ) );
                        }

                    }
                   
                }
            }

            errorMessages.Add( "An assignment to person could not be completed." );
            return false;
        
        }
        
    }
}

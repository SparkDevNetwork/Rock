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
using System.Data.Entity;
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
    [Description( "Sets a group member attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Attribute Set" )]

    [WorkflowAttribute("Group", "The attribute containing the group to get the leader for.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.GroupFieldType" })]

    [WorkflowAttribute("Person", "The attribute to set to the person in the group.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.PersonFieldType" })]

    [TextField("Group Member Attribute Key", "The attribute key to use for the group member attribute.", true, "", "", 2)]

    [GroupRoleField("", "Group Role Filter", "Use to set the attribute if the person is a specific role. If no role filter is provided then the all roles will be used.", false, "", "", 3)]

    [WorkflowTextOrAttribute( "Text Value", "Attribute Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", false, "", "", 4, "AttributeValue" )]
    
    public class SetGroupMemberAttribute : ActionComponent
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

            Guid? groupGuid = null;
            Person person = null;
            string attributeValue = string.Empty;
            Guid groupRoleGuid = Guid.Empty;
            string attributeKey = string.Empty;

            // get the group attribute
            Guid groupAttributeGuid = GetAttributeValue(action, "Group").AsGuid();

            if ( !groupAttributeGuid.IsEmpty() )
            {
                groupGuid = action.GetWorklowAttributeValue(groupAttributeGuid).AsGuidOrNull();

                if ( !groupGuid.HasValue )
                {
                    errorMessages.Add("The group could not be found!");
                }
            }

            // get person alias guid
            Guid personAliasGuid = Guid.Empty;
            string personAttribute = GetAttributeValue( action, "Person" );

            Guid guid = personAttribute.AsGuid();
            if (!guid.IsEmpty())
            {
                var attribute = AttributeCache.Get( guid, rockContext );
                if ( attribute != null )
                {
                    string value = action.GetWorklowAttributeValue(guid);
                    personAliasGuid = value.AsGuid();
                }

                if ( personAliasGuid != Guid.Empty )
                {
                    person = new PersonAliasService(rockContext).Queryable().AsNoTracking()
                                    .Where(p => p.Guid.Equals(personAliasGuid))
                                    .Select(p => p.Person)
                                    .FirstOrDefault();
                }
                else {
                    errorMessages.Add("The person could not be found in the attribute!");
                }
            }

            // get group member attribute value
            attributeValue = GetAttributeValue(action, "AttributeValue");
            guid = attributeValue.AsGuid();
            if ( guid.IsEmpty() )
            {
                attributeValue = attributeValue.ResolveMergeFields(GetMergeFields(action));
            }
            else
            {
                var workflowAttributeValue = action.GetWorklowAttributeValue(guid);

                if ( workflowAttributeValue != null )
                {
                    attributeValue = workflowAttributeValue;
                }
            }

            // get optional role filter
            groupRoleGuid = GetAttributeValue(action, "GroupRoleFilter").AsGuid();

            // get attribute key
            attributeKey = GetAttributeValue(action, "GroupMemberAttributeKey").Replace(" ", "");

            // set attribute
            if ( groupGuid.HasValue && person != null )
            {
                var qry = new GroupMemberService(rockContext).Queryable()
                                .Where(m => m.Group.Guid == groupGuid && m.PersonId == person.Id);

                if ( groupRoleGuid != Guid.Empty )
                {
                    qry = qry.Where(m => m.GroupRole.Guid == groupRoleGuid);
                }

                foreach ( var groupMember in qry.ToList() )
                {
                    groupMember.LoadAttributes(rockContext);
                    if ( groupMember.Attributes.ContainsKey(attributeKey) )
                    {
                        var attribute = groupMember.Attributes[attributeKey];
                        Rock.Attribute.Helper.SaveAttributeValue(groupMember, attribute, attributeValue, rockContext);
                    }
                    else
                    {
                        action.AddLogEntry(string.Format("The group member attribute {0} does not exist!", attributeKey));
                        break;
                    }
                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}
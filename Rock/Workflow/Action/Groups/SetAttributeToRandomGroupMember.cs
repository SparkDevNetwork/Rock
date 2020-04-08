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
    /// Sets a person attribute to be a random person from a group with optional filter on a campus group member attribute 
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Sets an attribute to a random person from a group with optional filter on a campus group member attribute. For example this could return a random person from the group with a group member attribute of 'Surprise'." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Select Random" )]

    [GroupField( "Selection Group", "The group to select the random person from.", true, "", "", 0 )]
    [TextField( "Group Member Attribute Key", "The key of the group member attribute to filter on (optional). No no key is provided all group members will be considered. Otherwise the list of available group members will be filtered by the value you provide from the attribute below.", false, "", "", 1 )]
    [WorkflowTextOrAttribute( "Filter Value", "Attribute Value", "The text or attribute to use for the filter value. <span class='tip tip-lava'></span>", false, "", "", 2, "FilterValue" )]

    [WorkflowAttribute( "Selected Person", "The attribute to set with the random person.", true, "", "", 3, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    
    public class SetAttributeToRandomGroupMember : ActionComponent
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
            string filterValue = string.Empty;
            string filterKey = string.Empty;
            
            // get the group attribute
            groupGuid = GetAttributeValue( action, "SelectionGroup" ).AsGuid();

            if ( !groupGuid.HasValue )
            {
                errorMessages.Add("The selection group could not be found!");
            }

            // get filter key
            filterKey = GetAttributeValue( action, "GroupMemberAttributeKey" );

            // get the filter value
            filterValue = GetAttributeValue( action, "FilterValue" );
            Guid? filterValueGuid = filterValue.AsGuidOrNull();
            if ( filterValueGuid.HasValue )
            {
                filterValue = action.GetWorklowAttributeValue( filterValueGuid.Value );
            }
            else
            {
                filterValue = filterValue.ResolveMergeFields( GetMergeFields( action ) );
            }

            // get group members
            var qry = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                    .Where( g => g.Group.Guid == groupGuid );

            if (!string.IsNullOrWhiteSpace(filterKey)){
                qry = qry.WhereAttributeValue( rockContext, filterKey, filterValue );
            }
                                    
            var groupMembers = qry.Select( g => new { 
                                        g.Person.NickName, 
                                        g.Person.LastName, 
                                        g.Person.SuffixValueId, 
                                        PrimaryAliasGuid = g.Person.Aliases.FirstOrDefault().Guid } )
                                   .ToList();

            if ( groupMembers.Count() > 0 )
            {
                // get random group member from options
                Random rnd = new Random();
                int r = rnd.Next( groupMembers.Count );

                var selectedGroupMember = groupMembers[r];

                // set value
                Guid selectPersonGuid = GetAttributeValue( action, "SelectedPerson" ).AsGuid();
                if ( !selectPersonGuid.IsEmpty() )
                {
                    var selectedPersonAttribute = AttributeCache.Get( selectPersonGuid, rockContext );
                    if ( selectedPersonAttribute != null )
                    {
                        // If this is a person type attribute
                        if ( selectedPersonAttribute.FieldTypeId == FieldTypeCache.Get( Rock.SystemGuid.FieldType.PERSON.AsGuid(), rockContext ).Id )
                        {
                            SetWorkflowAttributeValue( action, selectPersonGuid, selectedGroupMember.PrimaryAliasGuid.ToString() );
                        }
                        else if ( selectedPersonAttribute.FieldTypeId == FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid(), rockContext ).Id )
                        {
                            SetWorkflowAttributeValue( action, selectPersonGuid, Person.FormatFullName(selectedGroupMember.NickName, selectedGroupMember.LastName, selectedGroupMember.SuffixValueId)  );
                        }
                    }
                }
            }
            else
            {
                errorMessages.Add( "No group member for the selected campus could be found." );
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}
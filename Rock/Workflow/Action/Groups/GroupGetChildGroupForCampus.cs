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
    /// Gets a child group of the configured parent group that matches the given campus attribute.
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Finds a matching child group for the configured parent group that matches the given campus attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Get Child Group for Campus" )]

    [WorkflowAttribute( "Group", "The attribute to set the matching child group to.", true, "", "", 0, "Group",
        new string[] { "Rock.Field.Types.GroupFieldType" } )]
    [Rock.Attribute.GroupTypeGroupFieldAttribute( "Parent Group", "The parent group to search to find a matching child group that belongs to the selected Campus.", "Parent Group", true, "", "", 1, key:"ParentGroup" )]
    [WorkflowAttribute( "Campus", "The attribute to use to determine which campus to match.", true, "", "", 2, "Campus",
        new string[] { "Rock.Field.Types.CampusFieldType" } )]
    public class GroupGetChildGroupForCampus : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>true if successful; false otherwise</returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            Guid? parentGroupGuid = null;
            Guid? campusGuid = null;
            Campus campus = null;
            Group group = null;

            // get the campus attribute
            Guid campusAttributeGuid = GetAttributeValue( action, "Campus" ).AsGuid();

            if ( !campusAttributeGuid.IsEmpty() )
            {
                campusGuid = action.GetWorklowAttributeValue( campusAttributeGuid ).AsGuidOrNull();

                if ( campusGuid.HasValue )
                {
                    campus = new CampusService( rockContext ).Get( campusGuid.Value );

                    if ( campus == null )
                    {
                        errorMessages.Add( "The campus provided does not exist." );
                        return false;
                    }
                }
                else
                {
                    action.AddLogEntry( "A campus was not provided, so the GetGroupChildGroupForCampus action will not continue." );
                    return true;
                }
            }
            else
            {
                errorMessages.Add( "Invalid campus attribute provided." );
                return false;
            }

            // stored as GroupTypeGuid|GroupGuid
            var parentGroupValues = ( GetAttributeValue( action, "ParentGroup" ) ?? string.Empty ).Split( '|' );
            if ( parentGroupValues.Count() > 1 )
            {
                parentGroupGuid = parentGroupValues[1].AsGuidOrNull();
                if ( parentGroupGuid.HasValue )
                {
                    // Find the child group of the given parent group that matches the campus id
                    group = new GroupService( rockContext ).Queryable().AsNoTracking()
                        .Where( g => g.Guid == parentGroupGuid.Value )
                        .SelectMany( g => g.Groups )
                        .FirstOrDefault( cg => cg.CampusId == campus.Id );
                }
            }

            if ( group != null )
            {                
                // get the group attribute where we'll store the matching group.
                var groupAttribute = AttributeCache.Get( GetAttributeValue( action, "Group" ).AsGuid(), rockContext );
                if ( groupAttribute != null )
                {
                    SetWorkflowAttributeValue( action, groupAttribute.Guid, group.Guid.ToStringSafe() );
                    action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", groupAttribute.Name, group.Guid ) );
                }
                else
                {
                    errorMessages.Add( "Invalid group attribute provided." );
                    return false;
                }
            }
            else
            {
                action.AddLogEntry( string.Format( "A child group of parent group '{0}' matching campus {1} could not be found.", parentGroupGuid, campus.Name ) );
            }

            return true;
        }
    }
}
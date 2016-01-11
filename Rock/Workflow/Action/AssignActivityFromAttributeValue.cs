// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Assign Activity to a Person or Group attribute value.
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Assigns Activity to a Person or Group attribute value." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Assign Activity from Attribute Value." )]

    [WorkflowAttribute("Attribute", "The person or group attribute value to assign this activity to.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType" } )]
    public class AssignActivityFromAttributeValue : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Get the attribute's guid
            Guid guid = GetAttributeValue( action, "Attribute" ).AsGuid();
            if (!guid.IsEmpty())
            {
                // Get the attribute
                var attribute = AttributeCache.Read( guid, rockContext );
                if ( attribute != null )
                {
                    if ( attribute.FieldTypeId == FieldTypeCache.Read( SystemGuid.FieldType.PERSON.AsGuid(), rockContext ).Id )
                    {
                        // If attribute type is a person, value should be person alias id
                        Guid? personAliasGuid = action.GetWorklowAttributeValue(guid).AsGuidOrNull();
                        if ( personAliasGuid.HasValue )
                        {
                            var personAlias = new PersonAliasService( rockContext ).Queryable( "Person" )
                                .Where( a => a.Guid.Equals( personAliasGuid.Value ) )
                                .FirstOrDefault();
                            if (personAlias != null)
                            {
                                action.Activity.AssignedPersonAlias = personAlias;
                                action.Activity.AssignedPersonAliasId = personAlias.Id;
                                action.Activity.AssignedGroup = null;
                                action.Activity.AssignedGroupId = null;
                                action.AddLogEntry( string.Format( "Assigned activity to '{0}' ({1})", personAlias.Person.FullName, personAlias.Person.Id ) );
                                return true;
                            }
                        }
                    }

                    else if ( attribute.FieldTypeId == FieldTypeCache.Read( SystemGuid.FieldType.GROUP.AsGuid(), rockContext ).Id )
                    {
                        // If attribute type is a group, value should be group guid
                        Guid? groupGuid = action.GetWorklowAttributeValue( guid ).AsGuidOrNull();
                        if ( groupGuid.HasValue )
                        {
                            var group = new GroupService( rockContext ).Get( groupGuid.Value );
                            if ( group != null )
                            {
                                action.Activity.AssignedPersonAlias = null;
                                action.Activity.AssignedPersonAliasId = null;
                                action.Activity.AssignedGroup = group;
                                action.Activity.AssignedGroupId = group.Id;
                                action.AddLogEntry( string.Format( "Assigned activity to '{0}' group ({1})", group.Name, group.Id ) );
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
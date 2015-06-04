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
using System.Data.Entity;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Posts attendance to a group
    /// </summary>
    [Description( "Set's attendance in a group." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Post Attendance To Group" )]

    [WorkflowAttribute("Group", "The attribute containing the group to get the leader for.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.GroupFieldType" })]

    [WorkflowAttribute("Person", "The attribute to set to the person in the group.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.PersonFieldType" })]

    [WorkflowAttribute("Attendance Date/Time", "The attribute with the date time to use for the attendance. Leave blank to use the current date time.", false, "", "", 2, "AttendanceDatetime")]

    [WorkflowAttribute("Location", "The attribute to set to the location of the attendance (optional).", false, "", "", 3, null,
        new string[] { "Rock.Field.Types.LocationFieldType" })]

    [BooleanField("Add To Group", "Adds the person to the group if they are not already a member.", true, "", 4)]
    
    public class PostAttendanceToGroup : ActionComponent
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
            DateTime attendanceDateTime = DateTime.MinValue;
            bool addToGroup = true;
           
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
                var attribute = AttributeCache.Read( guid, rockContext );
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

            // get attendance date
            Guid dateTimeAttributeGuid = GetAttributeValue(action, "AttendanceDatetime").AsGuid();
            if ( !dateTimeAttributeGuid.IsEmpty() )
            {
                string attributeDatetime = action.GetWorklowAttributeValue(dateTimeAttributeGuid);

                if ( string.IsNullOrWhiteSpace(attributeDatetime) )
                {
                    attendanceDateTime = RockDateTime.Now;
                }
                else
                {
                    if ( !DateTime.TryParse(attributeDatetime, out attendanceDateTime) )
                    {
                        errorMessages.Add(string.Format("Could not parse the date provided {0}.", attributeDatetime));
                    }
                }
            }

            // get add to group
            addToGroup = GetAttributeValue(action, "AddToGroup").AsBoolean();

            // get location
            Guid locationGuid = Guid.Empty;
            Guid locationAttributeGuid = GetAttributeValue(action, "Location").AsGuid();
            if ( !locationAttributeGuid.IsEmpty() )
            {
                var locationAttribute = AttributeCache.Read(locationAttributeGuid, rockContext);

                if ( locationAttribute != null )
                {
                    locationGuid = action.GetWorklowAttributeValue(locationAttributeGuid).AsGuid();
                }
            }

            // set attribute
            if ( groupGuid.HasValue && person != null && attendanceDateTime != DateTime.MinValue )
            {
                
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}
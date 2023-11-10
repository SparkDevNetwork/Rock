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
    /// Posts attendance to a group
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Sets attendance in a group." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Attendance Add" )]

    #region Attributes

    [WorkflowAttribute(
        "Group",
        Description = "The attribute containing the group to get the leader for.",
        IsRequired = true,
        Order = 0,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupFieldType" },
        Key = AttributeKey.Group )]

    [WorkflowAttribute(
        "Person",
        Description = "The attribute to set to the person in the group.",
        IsRequired = true,
        Order = 1,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" },
        Key = AttributeKey.Person )]

    [WorkflowAttribute(
        "Attendance Date/Time",
        Description = "The attribute with the date time to use for the attendance. Leave blank to use the current date time.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.AttendanceDatetime )]

    [WorkflowAttribute(
        "Location",
        Description = "The attribute to set to the location of the attendance (optional).",
        IsRequired = false,
        Order = 3,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.LocationFieldType" },
        Key = AttributeKey.Location )]

    [CampusField(
        name: "Campus",
        description: "The campus for the Attendance.",
        includeInactive: false,
        required: false,
        order: 4,
        key: AttributeKey.Campus )]

    [WorkflowAttribute(
        "Schedule",
        Description = "The attribute to set to the schedule of the attendance (optional).",
        IsRequired = false,
        Order = 5,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.ScheduleFieldType" },
        Key = AttributeKey.Schedule )]

    [BooleanField(
        "Add To Group",
        Description = "Adds the person to the group if they are not already a member.",
        DefaultBooleanValue = true,
        Order = 6,
        Key = AttributeKey.AddToGroup )]

    #endregion Attributes
    [Rock.SystemGuid.EntityTypeGuid( "7D939E2E-EBD5-491A-AA9C-FBCC91AAD5D3")]
    public class PostAttendanceToGroup : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Group = "Group";
            public const string Person = "Person";
            public const string AttendanceDatetime = "AttendanceDatetime";
            public const string Location = "Location";
            public const string Schedule = "Schedule";
            public const string AddToGroup = "AddToGroup";
            public const string Campus = "Campus";
        }

        #endregion Attribute Keys
        
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
            var attendanceDateTime = RockDateTime.Now;
            bool addToGroup = true;

            // get the group attribute
            Guid groupAttributeGuid = GetAttributeValue( action, AttributeKey.Group ).AsGuid();

            if ( !groupAttributeGuid.IsEmpty() )
            {
                groupGuid = action.GetWorkflowAttributeValue( groupAttributeGuid ).AsGuidOrNull();

                if ( !groupGuid.HasValue )
                {
                    errorMessages.Add( "The group could not be found!" );
                }
            }

            // get person alias guid
            Guid personAliasGuid = Guid.Empty;
            string personAttribute = GetAttributeValue( action, AttributeKey.Person );

            Guid guid = personAttribute.AsGuid();
            if ( !guid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( guid, rockContext );
                if ( attribute != null )
                {
                    string value = action.GetWorkflowAttributeValue( guid );
                    personAliasGuid = value.AsGuid();
                }

                if ( personAliasGuid != Guid.Empty )
                {
                    person = new PersonAliasService( rockContext ).Queryable().AsNoTracking()
                                    .Where( p => p.Guid.Equals( personAliasGuid ) )
                                    .Select( p => p.Person )
                                    .FirstOrDefault();
                }
                else
                {
                    errorMessages.Add( "The person could not be found in the attribute!" );
                }
            }

            // get attendance date
            Guid dateTimeAttributeGuid = GetAttributeValue( action, AttributeKey.AttendanceDatetime ).AsGuid();
            if ( !dateTimeAttributeGuid.IsEmpty() )
            {
                string attributeDatetime = action.GetWorkflowAttributeValue( dateTimeAttributeGuid );

                if ( !string.IsNullOrWhiteSpace( attributeDatetime ) )
                {
                    if ( !DateTime.TryParse( attributeDatetime, out attendanceDateTime ) )
                    {
                        errorMessages.Add( string.Format( "Could not parse the date provided {0}.", attributeDatetime ) );
                    }
                }
            }

            // get add to group
            addToGroup = GetAttributeValue( action, AttributeKey.AddToGroup ).AsBoolean();

            // get location
            Guid locationGuid = Guid.Empty;
            Guid locationAttributeGuid = GetAttributeValue( action, AttributeKey.Location ).AsGuid();
            if ( !locationAttributeGuid.IsEmpty() )
            {
                var locationAttribute = AttributeCache.Get( locationAttributeGuid, rockContext );

                if ( locationAttribute != null )
                {
                    locationGuid = action.GetWorkflowAttributeValue( locationAttributeGuid ).AsGuid();
                }
            }

            //// get Schedule
            Guid scheduleGuid = Guid.Empty;
            Guid scheduleAttributeGuid = GetAttributeValue( action, AttributeKey.Schedule ).AsGuid();
            if ( !scheduleAttributeGuid.IsEmpty() )
            {
                var scheduleAttribute = AttributeCache.Get( scheduleAttributeGuid, rockContext );
                if ( scheduleAttribute != null )
                {
                    scheduleGuid = action.GetWorkflowAttributeValue( scheduleAttributeGuid ).AsGuid();
                }
            }

            // set attribute
            if ( groupGuid.HasValue && person != null && attendanceDateTime != DateTime.MinValue )
            {
                var group = new GroupService( rockContext ).Queryable( "GroupType.DefaultGroupRole" )
                                            .Where( g => g.Guid == groupGuid )
                                            .FirstOrDefault();
                if ( group != null )
                {
                    GroupMemberService groupMemberService = new GroupMemberService( rockContext );

                    // get group member
                    var groupMember = groupMemberService.Queryable()
                                            .Where( m => m.Group.Guid == groupGuid
                                                 && m.PersonId == person.Id )
                                            .FirstOrDefault();
                    if ( groupMember == null )
                    {
                        if ( addToGroup )
                        {
                            if ( group != null )
                            {
                                groupMember = new GroupMember();
                                groupMember.GroupId = group.Id;
                                groupMember.PersonId = person.Id;
                                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                groupMember.GroupRole = group.GroupType.DefaultGroupRole;
                                groupMemberService.Add( groupMember );
                                rockContext.SaveChanges();
                            }
                        }
                        else
                        {
                            action.AddLogEntry( string.Format( "{0} was not a member of the group {1} and the action was not configured to add them.", person.FullName, group.Name ) );
                        }
                    }

                    int? locationId = null;
                    if ( locationGuid != Guid.Empty )
                    {
                        var location = new LocationService( rockContext ).Queryable().AsNoTracking()
                                            .Where( l => l.Guid == locationGuid )
                                            .FirstOrDefault();

                        if ( location != null )
                        {
                            locationId = location.Id;
                        }
                    }

                    int? scheduleId = null;
                    if ( scheduleGuid != Guid.Empty )
                    {
                        var schedule = new ScheduleService( rockContext ).Queryable().AsNoTracking()
                                            .Where( l => l.Guid == scheduleGuid )
                                            .FirstOrDefault();

                        if ( schedule != null )
                        {
                            scheduleId = schedule.Id;
                        }
                    }

                    //get attendance campus
                    var campusId = group.CampusId;
                    var campusAttributeId = GetAttributeValue( action, AttributeKey.Campus ).AsGuidOrNull();
                    if ( campusAttributeId.HasValue )
                    {
                        campusId = CampusCache.Get( campusAttributeId.Value )?.Id;
                    }

                    int? personAliasId = person.PrimaryAliasId;
                    if ( personAliasId.HasValue )
                    {
                        /*
                           3/31/2020 - SK 
                           Updated code to consider time with attendance date to fix the issue raised in #4159
                           https://github.com/SparkDevNetwork/Rock/issues/4159
                        */
                        new AttendanceService( rockContext ).AddOrUpdate( personAliasId.Value, attendanceDateTime, group.Id, locationId, scheduleId, campusId );
                        rockContext.SaveChanges();

                        if ( locationId.HasValue )
                        {
                            Rock.CheckIn.KioskLocationAttendance.Remove( locationId.Value );
                        }
                    }
                }
                else
                {
                    errorMessages.Add( string.Format( "Could not find group matching the guid '{0}'.", groupGuid ) );
                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}
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
using System.Linq;

using Rock.Model;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Provides logic for converting items from one type to another.
    /// </summary>
    internal class DefaultConversionProvider
    {
        #region Properties

        /// <summary>
        /// Gets or sets the check-in template configuration in effect during filtering.
        /// </summary>
        /// <value>The check-in template configuration.</value>
        protected TemplateConfigurationData TemplateConfiguration => Session.TemplateConfiguration;

        /// <summary>
        /// Gets or sets the check-in session.
        /// </summary>
        /// <value>The check-in session.</value>
        protected CheckInSession Session { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConversionProvider"/> class.
        /// </summary>
        /// <param name="session">The check-in session.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="session"/> is <c>null</c>.</exception>
        public DefaultConversionProvider( CheckInSession session )
        {
            Session = session ?? throw new ArgumentNullException( nameof( session ) );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the family members into bags that represent the data
        /// required for check-in.
        /// </summary>
        /// <param name="familyGuid">The primary family unique identifier, this is used to resolve duplicates where a family member is also marked as can check-in.</param>
        /// <param name="groupMembers">The <see cref="GroupMember"/> objects to be converted to bags.</param>
        /// <returns>A collection of <see cref="PersonBag"/> objects.</returns>
        public List<PersonBag> GetFamilyMemberBags( Guid familyGuid, IEnumerable<GroupMember> groupMembers )
        {
            var familyMembers = new List<PersonBag>();

            // Get the group members along with the person record in memory.
            // Then sort by those that match the correct family first so that
            // any duplicates (non family members) can be skipped. This ensures
            // that a family member has precedence over the same person record
            // that is also flagged as "can check-in".
            //
            // Even though the logic between the two cases below is the same,
            // casting it to an IQueryable first will make sure the select
            // happens at the SQL level instead of in C# code.
            var members = groupMembers is IQueryable<GroupMember> groupMembersQry
                ? groupMembersQry
                    .Select( gm => new
                    {
                        GroupGuid = gm.Person.PrimaryFamilyId.HasValue ? gm.Person.PrimaryFamily.Guid : ( Guid? ) null,
                        RoleOrder = gm.GroupRole.Order,
                        gm.Person
                    } )
                    .ToList()
                    .OrderByDescending( gm => gm.GroupGuid == familyGuid )
                    .ThenBy( gm => gm.RoleOrder )
                : groupMembers
                    .Select( gm => new
                    {
                        GroupGuid = gm.Person.PrimaryFamilyId.HasValue ? gm.Person.PrimaryFamily.Guid : ( Guid? ) null,
                        RoleOrder = gm.GroupRole.Order,
                        gm.Person
                    } )
                    .ToList()
                    .OrderByDescending( gm => gm.GroupGuid == familyGuid )
                    .ThenBy( gm => gm.RoleOrder );

            foreach ( var member in members )
            {
                // Skip any duplicates.
                if ( familyMembers.Any( fm => fm.Guid == member.Person.Guid ) )
                {
                    continue;
                }

                var familyMember = new PersonBag
                {
                    Guid = member.Person.Guid,
                    IdKey = member.Person.IdKey,
                    FamilyGuid = member.GroupGuid ?? Guid.Empty,
                    FirstName = member.Person.FirstName,
                    NickName = member.Person.NickName,
                    LastName = member.Person.LastName,
                    FullName = member.Person.FullName,
                    PhotoUrl = member.Person.PhotoUrl,
                    BirthYear = member.Person.BirthYear,
                    BirthMonth = member.Person.BirthMonth,
                    BirthDay = member.Person.BirthDay,
                    BirthDate = member.Person.BirthYear.HasValue ? member.Person.BirthDate : null,
                    Age = member.Person.Age,
                    AgePrecise = member.Person.AgePrecise,
                    GradeOffset = member.Person.GradeOffset,
                    GradeFormatted = member.Person.GradeFormatted,
                    Gender = member.Person.Gender,
                    RoleOrder = member.RoleOrder
                };

                familyMembers.Add( familyMember );
            }

            return familyMembers;
        }

        /// <summary>
        /// Gets the attendee item information for the person bags. This also
        /// gathers all required information to later perform filtering on the
        /// attendees.
        /// </summary>
        /// <param name="people">The <see cref="PersonBag"/> objects to be used when constructing the <see cref="Attendee"/> objects that will wrap them.</param>
        /// <param name="baseOpportunities">The opportunities collection to be cloned onto each attendee.</param>
        /// <param name="recentAttendance">The recent attendance data for these family members.</param>
        /// <returns>A collection of <see cref="Attendee"/> objects.</returns>
        public virtual List<Attendee> GetAttendeeItems( IReadOnlyCollection<PersonBag> people, OpportunityCollection baseOpportunities, IReadOnlyCollection<RecentAttendance> recentAttendance )
        {
            return people
                .Select( fm =>
                {
                    var attendeeAttendances = recentAttendance
                        .Where( a => a.PersonGuid == fm.Guid )
                        .ToList();

                    return new Attendee
                    {
                        Person = fm,
                        RecentAttendances = attendeeAttendances,
                        Opportunities = baseOpportunities.Clone()
                    };
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the attendance bag that represents all of the provided details.
        /// </summary>
        /// <param name="attendance">The source attendance record.</param>
        /// <param name="attendee">The attendee information for the attendance record.</param>
        /// <returns>A new instance of <see cref="AttendanceBag"/>.</returns>
        public virtual AttendanceBag GetAttendanceBag( RecentAttendance attendance, Attendee attendee )
        {
            var bag = new AttendanceBag
            {
                NickName = attendee.Person.NickName,
                FirstName = attendee.Person.FirstName,
                LastName = attendee.Person.LastName,
                FullName = attendee.Person.FullName
            };

            UpdateAttendanceBag( bag, attendance );

            return bag;
        }

        /// <summary>
        /// Gets the attendance bag that represents all of the provided details.
        /// </summary>
        /// <param name="attendance">The source attendance record.</param>
        /// <param name="person">The Person information for the attendance record.</param>
        /// <returns>A new instance of <see cref="AttendanceBag"/>.</returns>
        public virtual AttendanceBag GetAttendanceBag( RecentAttendance attendance, Person person)
        {
            var bag = new AttendanceBag
            {
                NickName = person.NickName,
                FirstName = person.FirstName,
                LastName = person.LastName,
                FullName = person.FullName
            };

            UpdateAttendanceBag( bag, attendance );

            return bag;
        }

        /// <summary>
        /// Gets the potential attendee bag from the attendee item.
        /// </summary>
        /// <param name="attendee">The attendee.</param>
        /// <returns>A new instance of <see cref="AttendeeBag"/>.</returns>
        public virtual AttendeeBag GetAttendeeBag( Attendee attendee )
        {
            return new AttendeeBag
            {
                Person = attendee.Person,
                IsPreSelected = attendee.IsPreSelected,
                IsDisabled = attendee.IsDisabled,
                DisabledMessage = attendee.DisabledMessage,
                SelectedOpportunities = attendee.SelectedOpportunities
            };
        }

        /// <summary>
        /// Gets the opportunity collection bag from the opportunity collection item.
        /// </summary>
        /// <param name="opportunityCollection">The opportunity collection.</param>
        /// <returns>A new instance of <see cref="OpportunityCollectionBag"/>.</returns>
        public virtual OpportunityCollectionBag GetOpportunityCollectionBag( OpportunityCollection opportunityCollection )
        {
            return new OpportunityCollectionBag
            {
                AbilityLevels = opportunityCollection.AbilityLevels
                    .Select( GetAbilityLevelOpportunityBag )
                    .ToList(),
                Areas = opportunityCollection.Areas
                    .Select( GetAreaOpportunityBag )
                    .ToList(),
                Groups = opportunityCollection.Groups
                    .Select( GetGroupOpportunityBag )
                    .ToList(),
                Locations = opportunityCollection.Locations
                    .Select( GetLocationOpportunityBag )
                    .ToList(),
                Schedules = opportunityCollection.Schedules
                    .Select( GetScheduleOpportunityBag )
                    .ToList()
            };
        }

        /// <summary>
        /// Gets the ability level opportunity bag from the ability level opportunity item.
        /// </summary>
        /// <param name="abilityLevel">The ability level.</param>
        /// <returns>A new instance of <see cref="AbilityLevelOpportunityBag"/>.</returns>
        public virtual AbilityLevelOpportunityBag GetAbilityLevelOpportunityBag( AbilityLevelOpportunity abilityLevel )
        {
            return new AbilityLevelOpportunityBag
            {
                Guid = abilityLevel.Guid,
                Name = abilityLevel.Name
            };
        }

        /// <summary>
        /// Gets the area opportunity bag from the area opportunity item.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <returns>A new instance of <see cref="AreaOpportunityBag"/>.</returns>
        public virtual AreaOpportunityBag GetAreaOpportunityBag( AreaOpportunity area )
        {
            return new AreaOpportunityBag
            {
                Guid = area.Guid,
                Name = area.Name
            };
        }

        /// <summary>
        /// Gets the group opportunity bag from the group opportunity item.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>A new instance of <see cref="GroupOpportunityBag"/>.</returns>
        public virtual GroupOpportunityBag GetGroupOpportunityBag( GroupOpportunity group )
        {
            return new GroupOpportunityBag
            {
                AbilityLevelGuid = group.AbilityLevelGuid,
                AreaGuid = group.AreaGuid,
                Guid = group.Guid,
                LocationGuids = group.LocationGuids,
                Name = group.Name
            };
        }

        /// <summary>
        /// Gets the location opportunity bag from the location opportunity item.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>A new instance of <see cref="LocationOpportunityBag"/>.</returns>
        public virtual LocationOpportunityBag GetLocationOpportunityBag( LocationOpportunity location )
        {
            return new LocationOpportunityBag
            {
                Capacity = location.Capacity,
                CurrentCount = location.CurrentCount,
                Guid = location.Guid,
                Name = location.Name,
                ScheduleGuids = location.ScheduleGuids
            };
        }

        /// <summary>
        /// Gets the schedule opportunity bag from the schedule opportunity item.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns>A new instance of <see cref="ScheduleOpportunityBag"/>.</returns>
        public virtual ScheduleOpportunityBag GetScheduleOpportunityBag( ScheduleOpportunity schedule )
        {
            return new ScheduleOpportunityBag
            {
                Guid = schedule.Guid,
                Name = schedule.Name
            };
        }

        /// <summary>
        /// Gets the achievement bag that will represent the achievement attempt.
        /// </summary>
        /// <param name="achievementAttempt">The achievement attempt.</param>
        /// <returns>A new instance of <see cref="AchievementBag"/>.</returns>
        public virtual AchievementBag GetAchievementBag( AchievementAttempt achievementAttempt )
        {
            return new AchievementBag
            {
                Guid = achievementAttempt.Guid,
                Name = AchievementTypeCache.Get( achievementAttempt.AchievementTypeId )?.Name ?? "Unknown Achievement",
                IsSuccess = achievementAttempt.IsSuccessful,
                IsClosed = achievementAttempt.IsClosed,
                Progress = achievementAttempt.Progress,
                StartDateTime = achievementAttempt.AchievementAttemptStartDateTime.ToRockDateTimeOffset(),
                EndDateTime = achievementAttempt.AchievementAttemptEndDateTime?.ToRockDateTimeOffset()
            };
        }

        /// <summary>
        /// Updates the attendance bag with the details from the <see cref="RecentAttendance"/>
        /// record.
        /// </summary>
        /// <param name="attendanceBag">The attendance bag to be updated.</param>
        /// <param name="attendance">The recent attendance record.</param>
        protected void UpdateAttendanceBag( AttendanceBag attendanceBag, RecentAttendance attendance )
        {
            attendanceBag.Guid = attendance.AttendanceGuid;
            attendanceBag.PersonGuid = attendance.PersonGuid;
            attendanceBag.Status = attendance.Status;

            var area = GroupTypeCache.Get( attendance.GroupTypeGuid, Session.RockContext );
            var group = GroupCache.Get( attendance.GroupGuid, Session.RockContext );
            var location = NamedLocationCache.Get( attendance.LocationGuid, Session.RockContext );
            var schedule = NamedScheduleCache.Get( attendance.ScheduleGuid, Session.RockContext );

            if ( area != null )
            {
                attendanceBag.Area = new CheckInItemBag
                {
                    Guid = area.Guid,
                    Name = area.Name
                };
            }

            if ( group != null )
            {
                attendanceBag.Group = new CheckInItemBag
                {
                    Guid = group.Guid,
                    Name = group.Name
                };
            }

            if ( location != null )
            {
                attendanceBag.Location = new CheckInItemBag
                {
                    Guid = location.Guid,
                    Name = location.Name
                };
            }

            if ( schedule != null )
            {
                attendanceBag.Schedule = new CheckInItemBag
                {
                    Guid = schedule.Guid,
                    Name = schedule.Name
                };
            }
        }

        #endregion
    }
}

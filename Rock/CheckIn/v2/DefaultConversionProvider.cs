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

using Rock.Data;
using Rock.Model;
using Rock.Utility;
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
        /// Gets or sets the context to use when accessing the database.
        /// </summary>
        protected RockContext RockContext { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConversionProvider"/> class.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="rockContext"/> is <see langword="null"/>.</exception>
        public DefaultConversionProvider( RockContext rockContext )
        {
            RockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the family members into bags that represent the data
        /// required for check-in.
        /// </summary>
        /// <param name="familyId">The primary family identifier, this is used to resolve duplicates where a family member is also marked as can check-in.</param>
        /// <param name="groupMembers">The <see cref="GroupMember"/> objects to be converted to bags.</param>
        /// <returns>A collection of <see cref="FamilyMemberBag"/> objects.</returns>
        public List<FamilyMemberBag> GetFamilyMemberBags( string familyId, IEnumerable<GroupMember> groupMembers )
        {
            var familyMembers = new List<FamilyMemberBag>();
            var familyIdNumber = IdHasher.Instance.GetId( familyId ) ?? 0;

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
                        GroupId = gm.Person.PrimaryFamilyId.HasValue ? gm.Person.PrimaryFamilyId.Value : ( int? ) null,
                        RoleOrder = gm.GroupRole.Order,
                        gm.Person
                    } )
                    .ToList()
                    .OrderByDescending( gm => gm.GroupId == familyIdNumber )
                    .ThenBy( gm => gm.RoleOrder )
                : groupMembers
                    .Select( gm => new
                    {
                        GroupId = gm.Person.PrimaryFamilyId.HasValue ? gm.Person.PrimaryFamilyId.Value : ( int? ) null,
                        RoleOrder = gm.GroupRole.Order,
                        gm.Person
                    } )
                    .ToList()
                    .OrderByDescending( gm => gm.GroupId == familyIdNumber )
                    .ThenBy( gm => gm.RoleOrder );

            members.Select( fm => fm.Person ).LoadAttributes( RockContext );

            foreach ( var member in members )
            {
                // Skip any duplicates.
                if ( familyMembers.Any( fm => fm.Person.Id == member.Person.IdKey ) )
                {
                    continue;
                }

                familyMembers.Add( new FamilyMemberBag
                {
                    Person = GetPersonBag( member.Person ),
                    FamilyId = member.GroupId.HasValue ? IdHasher.Instance.GetHash( member.GroupId.Value ) : string.Empty,
                    RoleOrder = member.RoleOrder
                } );
            }

            return familyMembers;
        }

        /// <summary>
        /// Gets the person bag represented by the person object.
        /// </summary>
        /// <param name="person">The person object.</param>
        /// <returns>A new instance of <see cref="PersonBag"/> that represents the person.</returns>
        public virtual PersonBag GetPersonBag( Person person )
        {
            var abilityLevelGuid = person.GetAttributeValue( "AbilityLevel" ).AsGuidOrNull();
            CheckInItemBag abilityLevel = null;

            if ( abilityLevelGuid.HasValue )
            {
                var definedValue = DefinedValueCache.Get( abilityLevelGuid.Value, RockContext );

                if ( definedValue != null )
                {
                    abilityLevel = new CheckInItemBag
                    {
                        Id = definedValue.IdKey,
                        Name = definedValue.Value
                    };
                }
            }

            return new PersonBag
            {
                Id = person.IdKey,
                FirstName = person.FirstName,
                NickName = person.NickName,
                LastName = person.LastName,
                FullName = person.FullName,
                PhotoUrl = person.PhotoUrl,
                BirthYear = person.BirthYear,
                BirthMonth = person.BirthMonth,
                BirthDay = person.BirthDay,
                BirthDate = person.BirthYear.HasValue ? person.BirthDate : null,
                Age = person.Age,
                AgePrecise = person.AgePrecise,
                GradeOffset = person.GradeOffset,
                GradeFormatted = person.GradeFormatted,
                AbilityLevel = abilityLevel,
                Gender = person.Gender
            };
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
                        .Where( a => a.PersonId == fm.Id )
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
        public AttendanceBag GetAttendanceBag( RecentAttendance attendance, Attendee attendee )
        {
            return GetAttendanceBag( attendance, attendee.Person );
        }

        /// <summary>
        /// Gets the attendance bag that represents all of the provided details.
        /// </summary>
        /// <param name="attendance">The source attendance record.</param>
        /// <param name="person">The Person information for the attendance record.</param>
        /// <returns>A new instance of <see cref="AttendanceBag"/>.</returns>
        public AttendanceBag GetAttendanceBag( RecentAttendance attendance, Person person )
        {
            return GetAttendanceBag( attendance, GetPersonBag( person ) );
        }

        /// <summary>
        /// Gets the attendance bag with the details from the <see cref="RecentAttendance"/>
        /// record.
        /// </summary>
        /// <param name="attendance">The recent attendance record.</param>
        /// <param name="person">The person the attendance record is for.</param>
        /// <returns>A new instance of <see cref="AttendanceBag"/>.</returns>
        public virtual AttendanceBag GetAttendanceBag( RecentAttendance attendance, PersonBag person )
        {
            var attendanceBag = new AttendanceBag
            {
                Id = attendance.AttendanceId,
                Person = person,
                Status = attendance.Status
            };

            var area = GroupTypeCache.GetByIdKey( attendance.GroupTypeId, RockContext );
            var group = GroupCache.GetByIdKey( attendance.GroupId, RockContext );
            var location = NamedLocationCache.GetByIdKey( attendance.LocationId, RockContext );
            var schedule = NamedScheduleCache.GetByIdKey( attendance.ScheduleId, RockContext );

            if ( area != null )
            {
                attendanceBag.Area = new CheckInItemBag
                {
                    Id = area.IdKey,
                    Name = area.Name
                };
            }

            if ( group != null )
            {
                attendanceBag.Group = new CheckInItemBag
                {
                    Id = group.IdKey,
                    Name = group.Name
                };
            }

            if ( location != null )
            {
                attendanceBag.Location = new CheckInItemBag
                {
                    Id = location.IdKey,
                    Name = location.Name
                };
            }

            if ( schedule != null )
            {
                attendanceBag.Schedule = new CheckInItemBag
                {
                    Id = schedule.IdKey,
                    Name = schedule.Name
                };
            }

            return attendanceBag;
        }

        /// <summary>
        /// Gets the attendance bag with the details from the <see cref="Attendance"/>
        /// record.
        /// </summary>
        /// <param name="attendance">The attendance record.</param>
        /// <returns>A new instance of <see cref="AttendanceBag"/>.</returns>
        public virtual AttendanceBag GetAttendanceBag( Attendance attendance )
        {
            var attendanceBag = new AttendanceBag
            {
                Id = attendance.IdKey,
                Person = attendance.PersonAlias?.Person != null
                    ? GetPersonBag( attendance.PersonAlias.Person )
                    : null,
                Status = attendance.CheckInStatus
            };

            var group = GroupCache.Get( attendance.Occurrence.GroupId.Value, RockContext );
            var area = GroupTypeCache.Get( group.GroupTypeId, RockContext );
            var location = NamedLocationCache.Get( attendance.Occurrence.LocationId.Value, RockContext );
            var schedule = NamedScheduleCache.Get( attendance.Occurrence.ScheduleId.Value, RockContext );

            if ( area != null )
            {
                attendanceBag.Area = new CheckInItemBag
                {
                    Id = area.IdKey,
                    Name = area.Name
                };
            }

            if ( group != null )
            {
                attendanceBag.Group = new CheckInItemBag
                {
                    Id = group.IdKey,
                    Name = group.Name
                };
            }

            if ( location != null )
            {
                attendanceBag.Location = new CheckInItemBag
                {
                    Id = location.IdKey,
                    Name = location.Name
                };
            }

            if ( schedule != null )
            {
                attendanceBag.Schedule = new CheckInItemBag
                {
                    Id = schedule.IdKey,
                    Name = schedule.Name
                };
            }

            return attendanceBag;
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
                IsUnavailable = attendee.IsUnavailable,
                IsMultipleSelectionsAvailable = attendee.IsMultipleSelectionsAvailable,
                UnavailableMessage = attendee.UnavailableMessage,
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
                Id = abilityLevel.Id,
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
                Id = area.Id,
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
                AbilityLevelId = group.AbilityLevelId,
                AreaId = group.AreaId,
                Id = group.Id,
                LocationIds = group.LocationIds,
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
                Id = location.Id,
                Name = location.Name,
                ScheduleIds = location.ScheduleIds
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
                Id = schedule.Id,
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
            var achievementType = AchievementTypeCache.Get( achievementAttempt.AchievementTypeId, RockContext );

            return new AchievementBag
            {
                Id = achievementAttempt.IdKey,
                AchievementTypeId = achievementType?.IdKey ?? string.Empty,
                Name = achievementType?.Name ?? "Unknown Achievement",
                IsSuccess = achievementAttempt.IsSuccessful,
                IsClosed = achievementAttempt.IsClosed,
                Progress = achievementAttempt.Progress,
                TargetCount = achievementType?.TargetCount,
                StartDateTime = achievementAttempt.AchievementAttemptStartDateTime.ToRockDateTimeOffset(),
                EndDateTime = achievementAttempt.AchievementAttemptEndDateTime?.ToRockDateTimeOffset()
            };
        }

        #endregion
    }
}

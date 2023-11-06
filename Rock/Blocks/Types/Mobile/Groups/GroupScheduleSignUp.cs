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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Groups.GroupScheduleSignup;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Group;
using Rock.ViewModels.Blocks.Group.GroupScheduleSignup;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// A way for users to sign up for additional serving times on mobile.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Schedule Sign Up" )]
    [Category( "Mobile > Groups" )]
    [Description( "A way for individuals to sign up for additional serving times on mobile." )]
    [IconCssClass( "fa fa-user-plus" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BlockTemplateField( "Schedule Sign Up Landing Template",
        Description = "The XAML passed into the landing page, where the user's groups are listed.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_SIGNUP_LANDING_PAGE,
        DefaultValue = "C4BFED3A-C2A1-4A68-A646-44C3B499C75A",
        IsRequired = true,
        Key = AttributeKey.LandingTemplate,
        Order = 0 )]

    [IntegerField( "Future Weeks To Show",
        Description = "The amount of weeks in the future you would like to display scheduling opportunities.",
        DefaultIntegerValue = 6,
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.FutureWeeksToShow
        )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_SIGNUP )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_SIGNUP )]
    public class GroupScheduleSignUp : RockBlockType
    {

        #region Block Attributes

        private static class AttributeKey
        {
            /// <summary>
            /// The template used to reference the landing page, where we typically display the groups information.
            /// </summary>
            public const string LandingTemplate = "LandingTemplate";

            /// <summary>
            /// An integer representing the amount of future weeks to display.
            /// </summary>
            public const string FutureWeeksToShow = "FutureWeeksToShow";
        }

        /// <summary>
        /// Gets the landing template.
        /// </summary>
        protected string LandingTemplate => Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.LandingTemplate ) );

        /// <summary>
        /// Gets the amount of future weeks to show.
        /// </summary>
        protected int FutureWeeksToShow => GetAttributeValue( AttributeKey.FutureWeeksToShow ).AsInteger();

        /// <summary>
        /// Gets the current person ID, or 0 if unable to.
        /// </summary>
        protected int CurrentPersonId => RequestContext.CurrentPerson?.Id ?? 0;

        /// <summary>
        /// Gets the current person alias.
        /// </summary>
        protected int CurrentPersonPrimaryAliasId => RequestContext.CurrentPerson != null ? RequestContext.CurrentPerson.PrimaryAliasId ?? 0 : 0;

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 4 );

        /// <summary>
        /// Gets the mobile configuration.
        /// </summary>
        /// <returns></returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                FutureWeeksToShow = FutureWeeksToShow
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the landing page content and data.
        /// </summary>
        /// <returns>A <see cref="LandingPageContentBag"/> to be used when we display the landing page.</returns>
        private LandingPageContentBag GetLandingTemplateContent()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );

                // Get groups that the selected person is an active member that have SchedulingEnabled and have at least one location with a schedule.
                var groups = groupService
                    .Queryable()
                    .AsNoTracking()
                    .Where( x => x.Members.Any( m => m.PersonId == this.CurrentPersonId && m.IsArchived == false && m.GroupMemberStatus == GroupMemberStatus.Active ) )
                    .Where( x => x.IsActive == true && x.IsArchived == false
                        && x.GroupType.IsSchedulingEnabled == true
                        && x.DisableScheduling == false
                        && x.DisableScheduleToolboxAccess == false )
                    .Where( x => x.GroupLocations.Any( gl => gl.Schedules.Any() ) )
                    .OrderBy( x => new { x.Order, x.Name } )
                    .ToList();

                var mergeFields = RequestContext.GetCommonMergeFields();

                // Add in our list of schedule exclusions
                mergeFields.AddOrReplace( "SchedulingGroupList", groups );

                // Pass those in as content
                var content = LandingTemplate.ResolveMergeFields( mergeFields );

                var skipPage = groups.Count == 1;

                // Return all of the necessary information
                return new LandingPageContentBag
                {
                    Content = content,
                    SkipPage = skipPage,
                    GroupGuid = skipPage ? groups.FirstOrDefault().Guid : Guid.Empty
                };
            }
        }

        /// <summary>
        /// Gets a list of group locations that have particular checks to ensure scheduling is enabled.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="groupLocationService"></param>
        /// <returns>A list of group locations.</returns>
        private List<GroupLocation> GetApplicableGroupLocations( Group group, GroupLocationService groupLocationService )
        {
            var personGroupLocationQry = groupLocationService.Queryable().AsNoTracking();

            // Get GroupLocations that are for Groups that the person is an active member of.
            var personGroupLocationList = personGroupLocationQry.Where( a => a.GroupId == group.Id
                 && a.Group.IsArchived == false
                 && a.Group.GroupType.IsSchedulingEnabled == true
                 && a.Group.DisableScheduling == false
                 && a.Group.DisableScheduleToolboxAccess == false
                 && a.Group.Members.Any( m => m.PersonId == CurrentPersonId && m.IsArchived == false && m.GroupMemberStatus == GroupMemberStatus.Active ) )
                .ToList();

            return personGroupLocationList;
        }

        /// <summary>
        /// Gets a list of schedule sign up data.
        /// </summary>
        /// <remarks>This method is carefully designed to return a smaller payload, since it
        /// was written to be received on a remote client through a BlockAction. It loads future weeks between the
        /// <paramref name="startWeekIndex"/> and <paramref name="endWeekIndex"/>, while making sure not to exceed
        /// the <see cref="FutureWeeksToShow" />.
        /// </remarks>
        /// <param name="signUpGroupGuid">The sign-up group. Checks should be done beforehand that this group is
        /// configured correctly.</param>
        /// <param name="selectedSignupPersonId">The person requesting the sign up data.</param>
        /// <param name="startWeekIndex">The starting index of the next weeks to load.</param>
        /// <param name="endWeekIndex">The ending index of the next weeks to load.</param>
        /// <returns></returns>
        private List<GroupScheduleSignUpBag> GetSequentialScheduleSignupDataFromGroup( Guid signUpGroupGuid, int selectedSignupPersonId, int startWeekIndex, int endWeekIndex )
        {
            List<GroupScheduleSignUpBag> groupScheduleSignups = new List<GroupScheduleSignUpBag>();

            using ( var rockContext = new RockContext() )
            {
                var scheduleService = new ScheduleService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var groupService = new GroupService( rockContext );
                var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );

                // Take whichever value is smaller between the current week
                // we are loading and the max number of weeks to show.
                var endWeekCurrentIndex = Math.Min( endWeekIndex, FutureWeeksToShow );
                var indexedStartDateTime = startWeekIndex > 0 ? RockDateTime.Now.AddDays( ( startWeekIndex ) * 7 ) : RockDateTime.Now.AddDays( 1 );
                var indexedEndDateTime = RockDateTime.Now.AddDays( endWeekCurrentIndex * 7 );

                var groupLocationService = new GroupLocationService( rockContext );
                var group = groupService.Get( signUpGroupGuid );

                bool personDoesntMeetRequirements = group.SchedulingMustMeetRequirements
                    && groupService.GroupMembersNotMeetingRequirements( group, false, false )
                        .Where( a => a.Key.PersonId == selectedSignupPersonId )
                        .Any();

                if ( personDoesntMeetRequirements )
                {
                    return new List<GroupScheduleSignUpBag>();
                }

                foreach ( var personGroupLocation in group.GroupLocations )
                {
                    foreach ( var schedule in personGroupLocation.Schedules.Where( a => ( a.IsPublic ?? true ) && a.IsActive ) )
                    {
                        // Calculate capacities for this location (from the GroupLocationScheduleConfigs).
                        int maximumCapacitySetting = 0;
                        int desiredCapacitySetting = 0;
                        int minimumCapacitySetting = 0;
                        int desiredOrMinimumNeeded = 0;

                        if ( personGroupLocation.GroupLocationScheduleConfigs.Any() )
                        {
                            foreach ( var config in personGroupLocation.GroupLocationScheduleConfigs )
                            {
                                // There should only be one GroupLocationScheduleConfig for this location.
                                if ( config.ScheduleId == schedule.Id )
                                {
                                    maximumCapacitySetting = config.MaximumCapacity ?? 0;
                                    desiredCapacitySetting = config.DesiredCapacity ?? 0;
                                    minimumCapacitySetting = config.MinimumCapacity ?? 0;
                                }
                            }

                            // Use the higher value (between "minimum" and "desired") to calculate "people needed".
                            desiredOrMinimumNeeded = Math.Max( desiredCapacitySetting, minimumCapacitySetting );
                        }

                        var startDateTimeList = schedule.GetScheduledStartTimes( indexedStartDateTime, indexedEndDateTime );
                        foreach ( var startDateTime in startDateTimeList )
                        {
                            var occurrenceDate = startDateTime.Date;
                            bool alreadyScheduled = attendanceService.IsScheduled( occurrenceDate, schedule.Id, selectedSignupPersonId );
                            if ( alreadyScheduled )
                            {
                                continue;
                            }

                            if ( personScheduleExclusionService.IsExclusionDate( selectedSignupPersonId, personGroupLocation.GroupId, occurrenceDate ) )
                            {
                                // Don't show dates they have blacked out
                                continue;
                            }

                            // Get count of scheduled Occurrences with RSVP "Yes" for the group/schedule
                            var currentlyScheduledQry = attendanceService
                                .Queryable()
                                .Where( a => a.Occurrence.OccurrenceDate == startDateTime.Date
                                    && a.Occurrence.ScheduleId == schedule.Id
                                    && a.RSVP == RSVP.Yes
                                    && a.Occurrence.GroupId == personGroupLocation.GroupId );

                            int currentlyScheduledAtLocation = currentlyScheduledQry
                                .Where( a => a.Occurrence.LocationId == personGroupLocation.Location.Id )
                                .Count();

                            int peopleNeededAtLocation = desiredOrMinimumNeeded != 0 ? desiredOrMinimumNeeded - currentlyScheduledAtLocation : 0;

                            // If this is a new location for an existing group/schedule, find it.
                            var groupScheduleSignup = groupScheduleSignups
                                .Where( x => signUpGroupGuid == personGroupLocation.Group.Guid
                                    && x.ScheduleGuid == schedule.Guid
                                    && x.ScheduledDateTime == startDateTime )
                                .FirstOrDefault();

                            if ( groupScheduleSignup == null )
                            {
                                var currentlyScheduledWithoutLocationQry = currentlyScheduledQry.Where( a => !a.Occurrence.LocationId.HasValue );
                                int currentlyScheduledWithoutLocation = currentlyScheduledWithoutLocationQry.Count();

                                // Add to master list groupScheduleSignups
                                groupScheduleSignup = new GroupScheduleSignUpBag
                                {
                                    ScheduleGuid = schedule.Guid,
                                    ScheduledDateTime = startDateTime.ToRockDateTimeOffset(),
                                    ScheduledWithoutLocation = currentlyScheduledWithoutLocation,
                                };

                                groupScheduleSignups.Add( groupScheduleSignup );
                            }

                            // add the location to this group/schedule.
                            var groupSignupLocation = new GroupScheduleSignUpLocationBag
                            {
                                LocationGuid = personGroupLocation.Location.Guid,
                                LocationName = personGroupLocation.Location.Name,
                                LocationOrder = personGroupLocation.Order,
                                MaximumCapacity = maximumCapacitySetting,
                                ScheduledAtLocation = currentlyScheduledAtLocation,
                                PeopleNeeded = peopleNeededAtLocation < 0 ? 0 : peopleNeededAtLocation
                            };

                            groupScheduleSignup.Locations.Add( groupSignupLocation );
                        }
                    }
                }

                return groupScheduleSignups;
            }
        }

        /// <summary>
        /// Gets a list of available schedules for the group the selected sign-up person belongs to.
        /// </summary>
        /// <param name="groupGuid"></param>
        /// <remarks>This is only called in the legacy block action.</remarks>
        /// <returns></returns>
        private PersonScheduleSignupBag GetScheduleSignupData( Guid groupGuid )
        {
            List<PersonScheduleSignupDataBag> personScheduleSignups = new List<PersonScheduleSignupDataBag>();
            int numOfWeeks = FutureWeeksToShow;
            var startDate = RockDateTime.Now.AddDays( 1 ).Date;
            var endDate = RockDateTime.Now.AddDays( numOfWeeks * 7 );

            using ( var rockContext = new RockContext() )
            {
                var scheduleService = new ScheduleService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var groupLocationService = new GroupLocationService( rockContext );
                var groupService = new GroupService( rockContext );
                var personScheduleExclusionService = new PersonScheduleExclusionService( rockContext );

                var group = groupService.Get( groupGuid );
                var personGroupLocationList = GetApplicableGroupLocations( group, groupLocationService );

                var groupsThatHaveSchedulingRequirements = personGroupLocationList.Where( a => a.Group.SchedulingMustMeetRequirements ).Select( a => a.Group ).Distinct().ToList();
                var personDoesntMeetSchedulingRequirementGroupIds = new HashSet<int>();

                // If the person does not meet the scheduling requirements for the current group.
                var personDoesntMeetSchedulingRequirements = groupService.GroupMembersNotMeetingRequirements( group, false, false )
                    .Where( a => a.Key.PersonId == CurrentPersonId )
                    .Any();

                if ( personDoesntMeetSchedulingRequirements )
                {
                    personDoesntMeetSchedulingRequirementGroupIds.Add( group.Id );
                }

                // For every location in the location list.
                foreach ( var personGroupLocation in personGroupLocationList )
                {
                    // Loop through each particular scheduling opportunity
                    foreach ( var schedule in personGroupLocation.Schedules )
                    {
                        // Find if this has max volunteers here.
                        int maximumCapacitySetting = 0;
                        int desiredCapacitySetting = 0;
                        int minimumCapacitySetting = 0;
                        int desiredOrMinimumNeeded = 0;

                        if ( personGroupLocation.GroupLocationScheduleConfigs.Any() )
                        {
                            var groupConfigs = personGroupLocationList.Where( x => x.GroupId == personGroupLocation.GroupId ).Select( x => x.GroupLocationScheduleConfigs );
                            foreach ( var groupConfig in groupConfigs )
                            {
                                foreach ( var config in groupConfig )
                                {
                                    if ( config.ScheduleId == schedule.Id )
                                    {
                                        maximumCapacitySetting += config.MaximumCapacity ?? 0;
                                        desiredCapacitySetting += config.DesiredCapacity ?? 0;
                                        minimumCapacitySetting += config.MinimumCapacity ?? 0;
                                    }
                                }
                            }

                            desiredOrMinimumNeeded = Math.Max( desiredCapacitySetting, minimumCapacitySetting );
                        }

                        var startDateTimeList = schedule.GetScheduledStartTimes( startDate, endDate );

                        // For every start date time in the schedule, loop through to check if it is applicable to the current person. If so, we can add it to the master list.
                        foreach ( var startDateTime in startDateTimeList )
                        {
                            var occurrenceDate = startDateTime.Date;
                            bool alreadyScheduled = attendanceService.IsScheduled( occurrenceDate, schedule.Id, CurrentPersonId );
                            if ( alreadyScheduled )
                            {
                                continue;
                            }

                            // Don't show dates they have blacked out.
                            if ( personScheduleExclusionService.IsExclusionDate( RequestContext.CurrentPerson.PrimaryAlias.PersonId, personGroupLocation.GroupId, occurrenceDate ) )
                            {
                                continue;
                            }

                            // Don't show groups that have scheduling requirements that the person hasn't met.
                            if ( personGroupLocation.Group.SchedulingMustMeetRequirements && personDoesntMeetSchedulingRequirementGroupIds.Contains( personGroupLocation.GroupId ) )
                            {
                                continue;
                            }

                            // Get count of scheduled Occurrences with RSVP "Yes" for the group/schedule.
                            int currentScheduled = attendanceService
                                .Queryable()
                                .Where( a => a.Occurrence.OccurrenceDate == startDateTime.Date
                                    && a.Occurrence.ScheduleId == schedule.Id
                                    && a.RSVP == RSVP.Yes
                                    && a.Occurrence.GroupId == personGroupLocation.GroupId )
                                .Count();

                            bool maxScheduled = maximumCapacitySetting != 0 && currentScheduled >= maximumCapacitySetting;
                            int peopleNeeded = desiredOrMinimumNeeded != 0 ? desiredOrMinimumNeeded - currentScheduled : 0;

                            // Add to master list personScheduleSignups.
                            personScheduleSignups.Add( new PersonScheduleSignupDataBag
                            {
                                GroupGuid = personGroupLocation.Group.Guid,
                                GroupOrder = personGroupLocation.Group.Order,
                                GroupName = personGroupLocation.Group.Name,
                                LocationGuid = personGroupLocation.Location.Guid,
                                LocationName = personGroupLocation.Location.Name,
                                LocationOrder = personGroupLocation.Order,
                                ScheduleGuid = schedule.Guid,
                                ScheduleName = schedule.Name,
                                ScheduledDateTime = startDateTime.ToRockDateTimeOffset(),
                                MaxScheduled = maxScheduled,
                                PeopleNeeded = peopleNeeded < 0 ? 0 : peopleNeeded
                            } );
                        }
                    }
                }

                return new PersonScheduleSignupBag
                {
                    GroupName = group.Name,
                    PersonScheduleSignups = personScheduleSignups
                };
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the landing page information to be displayed.
        /// </summary>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult GetLandingContent()
        {
            return ActionOk( GetLandingTemplateContent() );
        }

        /// <summary>
        /// Gets the schedule sign up data.
        /// </summary>
        /// <param name="groupGuid">The group to get schedules for. Checks to ensure
        /// the group is a scheduling group should be done beforehand.</param>
        /// <param name="startWeekIndex">The starting index of the next weeks to load.</param>
        /// <param name="endWeekIndex">The ending index of the next weeks to load.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetScheduleSignUpData( Guid groupGuid, int startWeekIndex, int endWeekIndex )
        {
            using ( var rockContext = new RockContext() )
            {
                var requestPersonId = RequestContext.CurrentPerson?.Id;

                if ( requestPersonId == null )
                {
                    return ActionForbidden();
                }

                var groupService = new GroupService( rockContext );
                var group = groupService.Get( groupGuid );

                if ( group == null )
                {
                    return ActionNotFound();
                }

                var availableSchedules = GetSequentialScheduleSignupDataFromGroup( groupGuid, requestPersonId.Value, startWeekIndex, endWeekIndex );

                if ( availableSchedules == null )
                {
                    return ActionBadRequest( "Failed to get any scheduling data." );
                }

                return ActionOk( new GroupScheduleSignUpResponseBag
                {
                    GroupName = group.Name,
                    Schedules = availableSchedules
                } );
            }
        }

        /// <summary>
        /// Saves a specific sign-up schedule.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult SaveSignUpSchedule( ScheduleSignUpRequestBag schedule )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupId = new GroupService( rockContext ).GetNoTracking( schedule.GroupGuid ).Id;

                int? locationId = null;
                if ( schedule.LocationGuid.HasValue )
                {
                    locationId = new LocationService( rockContext ).GetNoTracking( schedule.LocationGuid.Value ).Id;
                }

                var scheduleId = new ScheduleService( rockContext ).GetNoTracking( schedule.ScheduleGuid ).Id;

                var attendanceOccurrence = new AttendanceOccurrenceService( rockContext ).GetOrAdd( schedule.ScheduledDateTime.DateTime, groupId, locationId, scheduleId );
                var personAlias = new PersonAliasService( rockContext ).Get( CurrentPersonPrimaryAliasId );

                var attendanceService = new AttendanceService( rockContext );
                var attendance = attendanceService.ScheduledPersonAddPending( CurrentPersonId, attendanceOccurrence.Id, personAlias );

                if ( attendance == null )
                {
                    return ActionBadRequest( "Failed to schedule that particular date. Please try again." );
                }

                rockContext.SaveChanges();

                attendanceService.ScheduledPersonConfirm( attendance.Id );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Deletes a particular attendance.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult DeleteSignUpSchedule( ScheduleSignUpRequestBag schedule )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );

                int? locationId = null;
                if ( schedule.LocationGuid.HasValue )
                {
                    locationId = new LocationService( rockContext ).GetNoTracking( schedule.LocationGuid.Value ).Id;
                }

                var scheduleId = new ScheduleService( rockContext ).GetNoTracking( schedule.ScheduleGuid ).Id;
                var groupId = new GroupService( rockContext ).GetNoTracking( schedule.GroupGuid ).Id;

                var attendance = attendanceService.Get( schedule.ScheduledDateTime.DateTime, locationId, scheduleId, groupId, CurrentPersonId );

                if ( attendance == null )
                {
                    return ActionBadRequest( "Failed to remove attendance." );
                }

                attendanceService.ScheduledPersonClear( attendance.Id );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Saves a particular attendance.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        [RockObsolete( "1.15.2" )]
        [Obsolete]
        public BlockActionResult SaveSchedule( PersonScheduleSignupDataBag schedule )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupId = new GroupService( rockContext ).GetNoTracking( schedule.GroupGuid ).Id;
                var locationId = new LocationService( rockContext ).GetNoTracking( schedule.LocationGuid ).Id;
                var scheduleId = new ScheduleService( rockContext ).GetNoTracking( schedule.ScheduleGuid ).Id;

                var attendanceOccurrence = new AttendanceOccurrenceService( rockContext ).GetOrAdd( schedule.ScheduledDateTime.DateTime, groupId, locationId, scheduleId );
                var personAlias = new PersonAliasService( rockContext ).Get( CurrentPersonPrimaryAliasId );

                var attendanceService = new AttendanceService( rockContext );
                var attendance = attendanceService.ScheduledPersonAddPending( CurrentPersonId, attendanceOccurrence.Id, personAlias );

                if ( attendance == null )
                {
                    return ActionBadRequest( "Failed to schedule that particular date. Please try again." );
                }

                rockContext.SaveChanges();

                attendanceService.ScheduledPersonConfirm( attendance.Id );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Deletes a particular attendance.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        [RockObsolete( "1.15.2" )]
        [Obsolete]
        public BlockActionResult DeleteSchedule( PersonScheduleSignupDataBag schedule )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );

                var locationId = new LocationService( rockContext ).GetNoTracking( schedule.LocationGuid ).Id;
                var scheduleId = new ScheduleService( rockContext ).GetNoTracking( schedule.ScheduleGuid ).Id;
                var groupId = new GroupService( rockContext ).GetNoTracking( schedule.GroupGuid ).Id;

                var attendance = attendanceService.Get( schedule.ScheduledDateTime.DateTime, locationId, scheduleId, groupId, CurrentPersonId );

                if ( attendance == null )
                {
                    return ActionBadRequest( "Failed to remove attendance." );
                }

                attendanceService.ScheduledPersonClear( attendance.Id );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Gets a list of the schedule assignment locations.
        /// </summary>
        /// <param name="groupGuid">The group identifier.</param>
        /// <param name="scheduleGuid">The schedule identifier.</param>
        [BlockAction]
        [RockObsolete( "1.15.2" )]
        [Obsolete]
        public BlockActionResult GetGroupScheduleAssignmentLocations( Guid groupGuid, Guid scheduleGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the group & schedule from the corresponding Guids.
                var group = new GroupService( rockContext ).Get( groupGuid );
                var groupSchedule = new ScheduleService( rockContext ).Get( scheduleGuid );

                if ( groupSchedule == null )
                {
                    return ActionNotFound();
                }

                var locations = new LocationService( rockContext ).GetByGroupSchedule( groupSchedule.Id, group.Id )
                    .OrderBy( a => a.Name )
                    .ToList()
                    .Select( a => new ListItemBag
                    {
                        Text = a.Name,
                        Value = a.Guid.ToStringSafe()
                    } )
                    .ToList();

                return ActionOk( locations );
            }
        }

        /// <summary>
        /// Gets the available schedule information for the current person.
        /// </summary>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        [RockObsolete( "1.15.2" )]
        [Obsolete]
        public BlockActionResult GetAvailableScheduleData( Guid groupGuid )
        {
            var availableSchedules = GetScheduleSignupData( groupGuid );

            if ( availableSchedules == null )
            {
                return ActionBadRequest( "Failed to get any scheduling data." );
            }

            return ActionOk( availableSchedules );
        }

        #endregion

    }
}

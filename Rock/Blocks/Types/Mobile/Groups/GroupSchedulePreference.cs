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
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Group;
using Rock.ViewModels.Blocks.Group.GroupSchedulePreference;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// The mobile block to set scheduling preferences.
    /// </summary>
    /// <seealso cref="RockBlockType" />
    [DisplayName( "Schedule Preferences" )]
    [Category( "Mobile > Groups" )]
    [Description( "Allows you to set your scheduling preferences." )]
    [IconCssClass( "fa fa-user-edit" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BlockTemplateField( "Schedule Preference Landing Template",
        Description = "The XAML passed into the landing page, where the user's groups are listed.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_PREFERENCE_LANDING_PAGE,
        DefaultValue = "C3A98DBE-E977-499C-B823-0B3676731E48",
        IsRequired = true,
        Key = AttributeKey.LandingTemplate,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_PREFERENCE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_PREFERENCE )]
    public class GroupSchedulePreference : RockBlockType
    {

        #region Block Attributes

        private static class AttributeKey
        {
            /// <summary>
            /// The template used to reference the landing page, where we typically display the groups information.
            /// </summary>
            public const string LandingTemplate = "LandingTemplate";
        }

        /// <summary>
        /// Gets the landing template.
        /// </summary>
        protected string LandingTemplate => Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.LandingTemplate ) );

        /// <summary>
        /// Gets the current person ID, or 0 if unable to.
        /// </summary>
        protected int CurrentPersonId => RequestContext.CurrentPerson?.Id ?? 0;

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
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the GroupMember record for a person from the specified personId and groupId.
        /// If the person is in there more than once, prefer the IsLeader role.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        private GroupMember GetGroupMemberRecord( RockContext rockContext, int groupId, int personId )
        {
            var groupMemberQuery = new GroupMemberService( rockContext )
                .GetByGroupIdAndPersonId( groupId, personId );

            var groupMember = groupMemberQuery.OrderBy( a => a.GroupRole.IsLeader ).FirstOrDefault();

            return groupMember;
        }

        /// <summary>
        /// Gets the landing page content and data.
        /// </summary>
        /// <returns>A <see cref="LandingPageContentBag"/> to be used when the landing page is displayed.</returns>
        private LandingPageContentBag GetLandingTemplateContent()
        {
            var rockContext = new RockContext();

            var groupService = new GroupService( rockContext );

            // Get groups that the selected person is an active member of with SchedulingEnabled and have at least one location with a schedule.
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

            // The dictionary of merge fields.
            var mergeFields = RequestContext.GetCommonMergeFields();

            // Add in our list of schedule exclusions.
            mergeFields.AddOrReplace( "SchedulingGroupList", groups );

            // Pass those in as content.
            var content = LandingTemplate.ResolveMergeFields( mergeFields );

            var skipPage = groups.Count == 1;

            // Return all of the necessary information for the landing page.
            return new LandingPageContentBag
            {
                Content = content,
                SkipPage = skipPage,
                GroupGuid = skipPage ? groups.FirstOrDefault().Guid : Guid.Empty
            };
        }

        /// <summary>
        /// Gets the preference page content and view model.
        /// </summary>
        /// <param name="groupGuid"></param>
        /// <returns>A <see cref="PreferencePageContentBag"/> to be used when the preference page is displayed.</returns>
        private PreferencePageContentBag GetPreferenceTemplateContent( Guid groupGuid )
        {
            var rockContext = new RockContext();

            var groupService = new GroupService( rockContext );

            // Get the group from the corresponding Guid.
            var group = groupService.Get( groupGuid );

            // Get the corresponding group member record.
            var groupMember = GetGroupMemberRecord( rockContext, group.Id, CurrentPersonId );

            // Get the reminder days offset.
            var selectedOffset = groupMember.ScheduleReminderEmailOffsetDays == null ? 0 : groupMember.ScheduleReminderEmailOffsetDays.Value;

            // Templates for all and this group type.
            var groupMemberScheduleTemplateService = new GroupMemberScheduleTemplateService( rockContext );
            var groupMemberScheduleTemplates = groupMemberScheduleTemplateService
                .Queryable()
                .AsNoTracking()
                .Where( x => x.GroupTypeId == null || x.GroupTypeId == group.GroupTypeId )
                .Select( x => new
                {
                    Value = ( Guid? ) x.Guid,
                    Text = x.Name
                } )
                .ToList();

            var scheduleListItems = new List<ListItemBag>();
            foreach ( var scheduleKey in groupMemberScheduleTemplates )
            {
                scheduleListItems.Add( new ListItemBag
                {
                    Value = scheduleKey.Value.ToStringSafe(),
                    Text = scheduleKey.Text
                } );
            }

            var selectedScheduleTemplate = groupMember.ScheduleTemplate != null ? groupMember.ScheduleTemplate.Guid : Guid.Empty;

            // The dictionary of merge fields.
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.AddOrReplace( "Group", group );

            var assignmentScheduleList = GetAssignmentScheduleList( group.Id );

            DateTimeOffset? selectedStartDate = null;
            if(groupMember.ScheduleStartDate != null)
            {
                selectedStartDate = groupMember.ScheduleStartDate.Value.ToRockDateTimeOffset();
            }

            // Return all of the necessary information to display on the preference page.
            return new PreferencePageContentBag
            {
                SelectedOffset = selectedOffset,
                ListItems = scheduleListItems,
                SelectedSchedule = selectedScheduleTemplate,
                SelectedStartDate = selectedStartDate,
                AssignmentScheduleAndLocations = assignmentScheduleList
            };
        }

        /// <summary>
        /// Returns an individual's unique assignment schedule list.
        /// </summary>
        /// <param name="groupGuid"></param>
        /// <returns>A list of <see cref="ListItemBag"/> to be passed into mobile.</returns>
        private List<ListItemBag> GetSpecificAssignmentScheduleList( Guid groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the group.
                var group = new GroupService( rockContext ).Get( groupGuid );

                var groupLocationService = new GroupLocationService( rockContext );
                var scheduleList = groupLocationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.Group.Guid == groupGuid )
                    .SelectMany( g => g.Schedules )
                    .Distinct()
                    .ToList();

                List<Schedule> sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();

                GroupMemberAssignmentService groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
                int? groupMemberId = null;

                var groupMember = GetGroupMemberRecord( rockContext, group.Id, CurrentPersonId );
                if ( groupMember != null )
                {
                    groupMemberId = groupMember.Id;
                }

                // Shouldn't happen, but we'll be cautious.
                if ( !groupMemberId.HasValue )
                {
                    return null;
                }

                var configuredScheduleIds = groupMemberAssignmentService.Queryable()
                    .Where( a => a.GroupMemberId == groupMemberId.Value && a.ScheduleId.HasValue )
                    .Select( s => s.ScheduleId.Value )
                    .Distinct()
                    .ToList();

                // Limit to schedules that haven't had a schedule preference set yet.
                sortedScheduleList = sortedScheduleList
                    .Where( a => !configuredScheduleIds.Contains( a.Id ) )
                    .ToList();

                var scheduleListItems = new List<ListItemBag>();

                foreach ( var value in sortedScheduleList )
                {
                    var scheduleListItem = new ListItemBag
                    {
                        Value = value.Guid.ToStringSafe(),
                        Text = value.Name
                    };

                    scheduleListItems.Add( scheduleListItem );
                }

                return scheduleListItems;
            }
        }

        /// <summary>
        /// Gets a list of assignment schedules.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>A list of <see cref="AssignmentScheduleAndLocationBag"/> used to display on the preference page.</returns>
        private List<AssignmentScheduleAndLocationBag> GetAssignmentScheduleList( int? groupId )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMember = GetGroupMemberRecord( rockContext, groupId.Value, CurrentPersonId );

                if ( groupMember == null )
                {
                    return null;
                }

                var groupLocationService = new GroupLocationService( rockContext );

                var qryGroupLocations = groupLocationService
                    .Queryable()
                    .Where( g => g.GroupId == groupId );

                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
                var groupMemberAssignmentQuery = groupMemberAssignmentService
                    .Queryable()
                    .AsNoTracking()
                    .Where( x =>
                        x.GroupMemberId == groupMember.Id
                        && (
                            !x.LocationId.HasValue
                            || qryGroupLocations.Any( gl => gl.LocationId == x.LocationId && gl.Schedules.Any( s => s.Id == x.ScheduleId ) )
                        ) );

                // Calculate the Next Start Date Time based on the start of the week so that schedule columns are in the correct order.
                var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );

                var groupMemberAssignmentList = groupMemberAssignmentQuery
                    .Include( a => a.Schedule )
                    .Include( a => a.Location )
                    .AsNoTracking()
                    .ToList()
                    .OrderBy( a => a.Schedule.Order )
                    .ThenBy( a => a.Schedule.GetNextStartDateTime( occurrenceDate ) )
                    .ThenBy( a => a.Schedule.Name )
                    .ThenBy( a => a.Schedule.Id )
                    .ThenBy( a => a.LocationId.HasValue ? a.Location.ToString( true ) : string.Empty )
                    .ToList();

                List<AssignmentScheduleAndLocationBag> assignmentScheduleAndLocations = new List<AssignmentScheduleAndLocationBag>();

                // Loop through each assignment in the list.
                foreach ( var groupMemberAssignment in groupMemberAssignmentList )
                {
                    var scheduleListItem = new ListItemBag
                    {
                        Value = groupMemberAssignment.Schedule.Guid.ToStringSafe(),
                        Text = groupMemberAssignment.Schedule.Name
                    };

                    var locationListItem = new ListItemBag
                    {
                        Value = groupMemberAssignment.Location != null ? groupMemberAssignment.Location.Guid.ToStringSafe() : string.Empty,
                        Text = groupMemberAssignment.Location != null ? groupMemberAssignment.Location.Name : "No Location Preference"
                    };

                    assignmentScheduleAndLocations.Add( new AssignmentScheduleAndLocationBag
                    {
                        GroupMemberAssignmentGuid = groupMemberAssignment.Guid,
                        ScheduleListItem = scheduleListItem,
                        LocationListItem = locationListItem
                    } );
                }

                return assignmentScheduleAndLocations;
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets a list of the schedule assignment locations.
        /// </summary>
        /// <param name="groupGuid">The group identifier.</param>
        /// <param name="scheduleGuid">The schedule identifier.</param>
        [BlockAction]
        public BlockActionResult GetGroupScheduleAssignmentLocations( Guid groupGuid, Guid scheduleGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the group
                var group = new GroupService( rockContext ).Get( groupGuid );

                var schedule = new ScheduleService( rockContext ).GetNoTracking( scheduleGuid );

                // We could not get the group.
                if ( schedule == null )
                {
                    return ActionNotFound();
                }

                var locations = new LocationService( rockContext ).GetByGroupSchedule( schedule.Id, group.Id )
                    .OrderBy( a => a.Name )
                    .AsEnumerable()
                    .Select( a => new ListItemBag
                    {
                        Value = a.Guid.ToStringSafe(),
                        Text = a.Name
                    } ).ToList();

                return ActionOk( locations );
            }
        }

        /// <summary>
        /// Save the group member schedule. 
        /// </summary>
        /// <param name="groupGuid"></param>
        /// <param name="reminderOffset"></param>
        /// <param name="scheduleTemplateGuid"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveGroupMemberSchedule( Guid groupGuid, int? reminderOffset, Guid? scheduleTemplateGuid, DateTimeOffset? startDate )
        {
            // Save the preference. For now this acts as a note to the scheduler and does not effect the list of assignments presented to the user.
            using ( var rockContext = new RockContext() )
            {
                var groupId = new GroupService( rockContext ).Get( groupGuid ).Id;
                var groupMemberService = new GroupMemberService( rockContext );
                var groupMember = this.GetGroupMemberRecord( rockContext, groupId, this.CurrentPersonId );

                if ( groupMember == null )
                {
                    return ActionNotFound();
                }

                if ( scheduleTemplateGuid != null && scheduleTemplateGuid != Guid.Empty )
                {
                    var scheduleTemplate = new GroupMemberScheduleTemplateService( rockContext ).GetNoTracking( scheduleTemplateGuid.Value );
                    groupMember.ScheduleTemplateId = scheduleTemplate.Id;
                    rockContext.SaveChanges();
                }
                else
                {
                    groupMember.ScheduleTemplateId = null;
                }

                if( startDate.HasValue )
                {
                    groupMember.ScheduleStartDate = startDate.Value.Date;
                }
                else
                {
                    groupMember.ScheduleStartDate = null;
                }
                
                groupMember.ScheduleReminderEmailOffsetDays = reminderOffset ?? 0;

                rockContext.SaveChanges();
            }

            return ActionOk();
        }

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
        /// Gets the scheduling preference information to be displayed.
        /// </summary>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult GetPreferencesContent( Guid groupGuid )
        {
            var content = GetPreferenceTemplateContent( groupGuid );

            if ( content == null )
            {
                return ActionBadRequest( "There was an error fetching the specified content, please try again." );
            }

            return ActionOk( content );
        }

        /// <summary>
        /// Gets the assignment schedule preferences for the group.
        /// </summary>
        /// <param name="groupGuid"></param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult GetSpecificAssignmentSchedules( Guid groupGuid )
        {
            var assignmentSpecificValues = GetSpecificAssignmentScheduleList( groupGuid );

            if ( assignmentSpecificValues == null )
            {
                return ActionBadRequest();
            }

            return ActionOk( assignmentSpecificValues );
        }

        /// <summary>
        /// Deletes a specific assignment schedule preference.
        /// </summary>
        /// <param name="groupGuid"></param>
        /// <param name="scheduleGuid"></param>
        /// <param name="locationGuid"></param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult DeleteSpecificAssignmentSchedule( Guid groupGuid, Guid scheduleGuid, Guid? locationGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
                var group = new GroupService( rockContext ).GetNoTracking( groupGuid );
                var groupMember = GetGroupMemberRecord( rockContext, group.Id, CurrentPersonId );

                var schedule = new ScheduleService( rockContext ).GetNoTracking( scheduleGuid );

                int? locationId = null;
                if ( locationGuid != null )
                {
                    var location = new LocationService( rockContext ).GetNoTracking( locationGuid.Value );

                    if ( location != null )
                    {
                        locationId = location.Id;
                    }
                }

                var groupMemberAssignment = groupMemberAssignmentService.GetByGroupMemberScheduleAndLocation( groupMember, schedule.Id, locationId );

                if ( groupMemberAssignment == null )
                {
                    return ActionNotFound();
                }

                if ( groupMemberAssignmentService.CanDelete( groupMemberAssignment, out var errorMessage ) )
                {
                    groupMemberAssignmentService.Delete( groupMemberAssignment );
                    rockContext.SaveChanges();
                    return ActionOk();
                }

                return ActionBadRequest();
            }
        }

        /// <summary>
        /// Saves a specific assignment schedule preference.
        /// </summary>
        /// <param name="groupGuid"></param>
        /// <param name="scheduleGuid"></param>
        /// <param name="locationGuid"></param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult SaveSpecificAssignmentSchedule( Guid? groupGuid, Guid scheduleGuid, Guid? locationGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the group.
                var group = new GroupService( rockContext ).GetNoTracking( groupGuid.Value );

                // Get the specific group member assignment.
                GroupMemberAssignmentService groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
                var groupMember = GetGroupMemberRecord( rockContext, group.Id, this.CurrentPersonId );

                var schedule = new ScheduleService( rockContext ).GetNoTracking( scheduleGuid );

                int? locationId = null;
                if ( locationGuid != null )
                {
                    var location = new LocationService( rockContext ).GetNoTracking( locationGuid.Value );

                    if ( location != null )
                    {
                        locationId = location.Id;
                    }
                }

                var groupMemberAssignment = groupMemberAssignmentService.GetByGroupMemberScheduleAndLocation( groupMember, schedule.Id, locationId );

                if ( groupMemberAssignment == null )
                {
                    groupMemberAssignment = new GroupMemberAssignment();
                }

                // In case there is already a group member assignment.
                else
                {
                    return ActionBadRequest( "You are already assigned to this schedule." );
                }

                groupMemberAssignment.GroupMemberId = groupMember.Id;
                groupMemberAssignment.ScheduleId = schedule.Id;
                groupMemberAssignment.LocationId = locationId;

                groupMemberAssignmentService.Add( groupMemberAssignment );

                // Add the group member to the service.
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion

        #region Helper Classes

        #endregion

    }
}

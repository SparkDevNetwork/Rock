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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Group.GroupScheduleToolbox;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// A block used to accept, decline or cancel group serving opportunities.
    /// </summary>
    /// <seealso cref="RockBlockType" />
    [DisplayName( "Schedule Toolbox" )]
    [Category( "Mobile > Groups" )]
    [Description( "Allows management of group scheduling for a specific person (worker)." )]
    [IconCssClass( "fa fa-user-check" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BlockTemplateField( "Toolbox Template",
        Description = "The template used to render the scheduling data.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX,
        DefaultValue = "CD2629E5-8EB0-4D52-ACAB-8EDF9AF84814",
        IsRequired = true,
        Key = AttributeKey.ToolboxTemplate,
        Order = 0 )]

    [BlockTemplateField( "Confirm Decline Template",
        Description = "The template used on the decline reason modal. Must require a decline reason in group type.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX_DECLINE_MODAL,
        DefaultValue = "92D39913-7D69-4B73-8FF9-72AC161BE381",
        IsRequired = true,
        Key = AttributeKey.ConfirmDeclineTemplate,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_TOOLBOX )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_TOOLBOX )]
    public class GroupScheduleToolbox : RockBlockType
    {

        #region Block Attributes

        private static class AttributeKey
        {
            /// <summary>
            /// The key regarding the "ToolboxTemplate"
            /// </summary>
            public const string ToolboxTemplate = "ToolboxTemplate";

            /// <summary>
            /// The key regarding the "ConfirmDeclineTemplate".
            /// </summary>
            public const string ConfirmDeclineTemplate = "ConfirmDeclineTemplate";
        }

        /// <summary>
        /// Gets the type template.
        /// </summary>
        /// <value>
        /// The type template.
        /// </value>
        protected string TypeTemplate => Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.ToolboxTemplate ) );

        /// <summary>
        /// Get the decline template.
        /// </summary>
        /// <value>
        /// The decline reason template.
        /// </value>
        protected string ConfirmDeclineTemplate => Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.ConfirmDeclineTemplate ) );

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
                DeclineReasons = LoadDeclineReasons()
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the decline reasons from the DefinedTypeCache.
        /// </summary>
        /// <returns>A list of defined value cache items.</returns>
        private List<ListItemBag> LoadDeclineReasons()
        {
            // Get the list of decline reasons from defined types.
            var defineTypeGroupScheduleReason = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUP_SCHEDULE_DECLINE_REASON );

            // Pass the values into a list.
            var definedValues = defineTypeGroupScheduleReason.DefinedValues
                .Select( x => new ListItemBag
                {
                    Text = x.Value.ToStringSafe(),
                    Value = x.Guid.ToStringSafe()
                } )
                .ToList();

            return definedValues;
        }

        /// <summary>
        /// Schedule content view model for mobile.
        /// </summary>
        /// <returns>The content view model to be used when displaying on mobile.</returns>
        private ContentBag GetScheduleContent()
        {
            using ( var rockContext = new RockContext() )
            {
                var currentDateTime = RockDateTime.Now.Date;

                // Query the confirmed attendances.
                var confirmedScheduleList = new AttendanceService( rockContext )
                    .GetConfirmedScheduled()
                    .Where( a => a.PersonAlias.PersonId == this.CurrentPersonId )
                    .Where( a => a.Occurrence.OccurrenceDate >= currentDateTime )
                    .Select( a => new GroupScheduleRowInfo
                    {
                        Guid = a.Guid,
                        Id = a.Id,
                        OccurrenceStartDate = a.Occurrence.OccurrenceDate,
                        Group = a.Occurrence.Group,
                        Schedule = a.Occurrence.Schedule,
                        Location = a.Occurrence.Location,
                        GroupScheduleType = GroupScheduleType.Upcoming
                    } ).ToList();

                // Query the pending attendances.
                var pendingScheduleList = new AttendanceService( rockContext )
                    .GetPendingScheduledConfirmations()
                    .Where( a => a.PersonAlias.PersonId == this.CurrentPersonId )
                    .Where( a => a.Occurrence.OccurrenceDate >= currentDateTime )
                    .Select( a => new GroupScheduleRowInfo
                    {
                        Guid = a.Guid,
                        Id = a.Id,
                        OccurrenceStartDate = a.Occurrence.OccurrenceDate,
                        Group = a.Occurrence.Group,
                        Schedule = a.Occurrence.Schedule,
                        Location = a.Occurrence.Location,
                        GroupScheduleType = GroupScheduleType.Pending
                    } ).ToList();

                // Query the declined attendances.
                var declinedScheduleList = new AttendanceService( rockContext )
                    .GetDeclinedScheduleConfirmations()
                    .Where( a => a.PersonAlias.PersonId == this.CurrentPersonId )
                    .Where( a => a.Occurrence.OccurrenceDate >= currentDateTime )
                    .Select( a => new GroupScheduleRowInfo
                    {
                        Guid = a.Guid,
                        Id = a.Id,
                        OccurrenceStartDate = a.Occurrence.OccurrenceDate,
                        Group = a.Occurrence.Group,
                        Schedule = a.Occurrence.Schedule,
                        Location = a.Occurrence.Location,
                        GroupScheduleType = GroupScheduleType.Unavailable
                    } ).ToList();

                // Now we are going to take the two lists and merge them. Since we already know the size of the list, we can use that.
                // We can do this to help increase performance.
                // Source: https://stackoverflow.com/a/4488073/15341894.
                var scheduleList = new List<GroupScheduleRowInfo>( confirmedScheduleList.Count() + pendingScheduleList.Count() );
                scheduleList.AddRange( confirmedScheduleList );
                scheduleList.AddRange( pendingScheduleList );
                scheduleList.AddRange( declinedScheduleList );
                scheduleList = scheduleList.OrderBy( x => x.OccurrenceStartDate ).ToList();

                // The dictionary of merge fields.
                var mergeFields = RequestContext.GetCommonMergeFields();

                // Add in the pending and confirmed attendances.
                mergeFields.AddOrReplace( "ScheduleList", scheduleList );

                // Pass those in as content.
                var content = TypeTemplate.ResolveMergeFields( mergeFields );

                return new ContentBag
                {
                    Content = content,
                };
            }
        }

        /// <summary>
        /// Gets the scheduling information to be displayed.
        /// </summary>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult GetContent()
        {
            return ActionOk( GetScheduleContent() );
        }

        /// <summary>
        /// Gets the confirm decline content to be displayed.
        /// </summary>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult GetConfirmDeclineContent( Guid attendanceGuid )
        {
            // We cannot operate without a logged in person.
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionStatusCode( System.Net.HttpStatusCode.Unauthorized );
            }

            using ( var rockContext = new RockContext() )
            {
                // Query the specific attendance attempting to be declined.
                var attendanceService = new AttendanceService( rockContext );
                var attendance = attendanceService.Queryable().Where( a => a.Guid == attendanceGuid && a.PersonAlias.PersonId == CurrentPersonId ).FirstOrDefault();

                // Add in the attendance to the template.
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.AddOrReplace( "Attendance", attendance );

                var content = ConfirmDeclineTemplate.ResolveMergeFields( mergeFields );

                // There is no content to pass down, so let's send an error.
                if ( content == null )
                {
                    return ActionBadRequest( "Unable to fetch content." );
                }

                // Here, we are going to return the view model content, a list of the decline reasons, and whether or not a reason is required.
                var viewModel = new ConfirmDeclineBag
                {
                    Content = content,
                    DeclineReasonRequired = attendanceService.Get( attendanceGuid ).Occurrence.Group.GroupType.RequiresReasonIfDeclineSchedule
                };

                return ActionOk( viewModel );
            }
        }

        /// <summary>
        /// Confirms a specified attendance. 
        /// </summary>
        /// <param name="attendanceGuid"></param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult ConfirmAttend( Guid attendanceGuid )
        {
            // We cannot operate without a logged in person.
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionStatusCode( System.Net.HttpStatusCode.Unauthorized );
            }

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            // Get the corresponding attendance Id from the Guid.
            var attendanceId = attendanceService.GetId( attendanceGuid );

            // If unable to get, let's return a bad request to mobile.
            if ( attendanceId == null )
            {
                return ActionBadRequest( "Unable to fetch the corresponding attendance from the Guid." );
            }

            // Mark the attendance as confirmed.
            attendanceService.ScheduledPersonConfirm( attendanceId.Value );

            rockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Declines the attendance.
        /// </summary>
        /// <param name="attendanceGuid">The specific attendance Guid.</param>
        /// <param name="declineReasonGuid">The specific decline reason Guid.</param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult DeclineAttend( Guid attendanceGuid, Guid? declineReasonGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // We cannot operate without a logged in person.
                if ( RequestContext.CurrentPerson == null )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.Unauthorized );
                }

                var attendanceService = new AttendanceService( rockContext );
                var attendanceId = attendanceService.GetId( attendanceGuid );

                // If the specified attendance Id can't be fetched by Guid, let's return an error.
                if ( !attendanceId.HasValue )
                {
                    return ActionBadRequest( "Unable to fetch the corresponding attendance from the Guid." );
                }

                // If the Group Type requires a decline reason.
                var requiresDeclineReason = attendanceService.Get( attendanceId.Value ).Occurrence.Group.GroupType.RequiresReasonIfDeclineSchedule;

                // If a decline reason is required and not provided.
                if ( !declineReasonGuid.HasValue && requiresDeclineReason )
                {
                    return ActionBadRequest( "Please enter a decline reason." );
                }

                // If the decline reason Guid has a value, let's fetch it. Otherwise, let's assign it to null.
                int? declineReason = declineReasonGuid.HasValue ? DefinedValueCache.GetId( declineReasonGuid.Value ) : null;

                // Mark the attendance as declined.
                attendanceService.ScheduledPersonDecline( attendanceId.Value, declineReason );

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Cancels an attendance that has been marked as declined.
        /// </summary>
        /// <param name="attendanceGuid"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SetPending( Guid attendanceGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // We cannot operate without a logged in person.
                if ( RequestContext.CurrentPerson == null )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.Unauthorized );
                }

                var attendanceService = new AttendanceService( rockContext );
                var attendanceId = attendanceService.GetId( attendanceGuid );

                // If the specified attendance Id can't be fetched by Guid, let's return an error.
                if ( !attendanceId.HasValue )
                {
                    return ActionNotFound();
                }

                attendanceService.ScheduledPersonConfirmCancel( attendanceId.Value );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion

        #region Support Classes

        private class GroupScheduleRowInfo : RockDynamic
        {
            /// <summary>
            /// Gets or sets the attendance Guid.
            /// </summary>
            /// <value>
            /// A <see cref="System.Guid"/> representing the attendance guid.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the attendance Id.
            /// </summary>
            /// <value>
            /// An integer representing the Id.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the date of the Attendance. Only the date is used.
            /// </summary>
            /// <value>
            /// A <see cref="System.DateTime"/> representing the start date and time/check in date and time.
            /// </value>
            public DateTimeOffset OccurrenceStartDate { get; set; }

            /// <summary>
            /// Gets or sets the end date of the Attendance. Only the date is used.
            /// </summary>
            /// <value>
            /// A <see cref="System.DateTime"/> representing the end date and time/check in date and time.
            /// </value>
            public DateTimeOffset OccurrenceEndDate { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Group"/>.
            /// </summary>
            /// <value>
            /// The <see cref="Rock.Model.Group"/>.
            /// </value>
            public Group Group { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Location"/> where the Person attended.
            /// </summary>
            /// <value>
            /// The <see cref="Rock.Model.Location"/> where the <see cref="Rock.Model.Person"/> attended.
            /// </value>
            public Location Location { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Schedule"/>.
            /// </summary>
            /// <value>
            /// The schedule.
            /// </value>
            public Schedule Schedule { get; set; }

            /// <summary>
            /// Gets or sets the GroupScheduleType.
            /// </summary>
            /// <value>
            /// The GroupScheduleType.
            /// </value>
            public GroupScheduleType GroupScheduleType { get; set; }

            /// <summary>
            /// Gets or sets the person alias identifier of the Person that this exclusion is for
            /// </summary>
            /// <value>
            /// The person alias identifier.
            /// </value>
            public PersonAlias PersonAlias { get; set; }
        }

        /// <summary>
        /// An enum describing the different schedule type. 
        /// </summary>
        private enum GroupScheduleType
        {
            Pending = 0,
            Upcoming = 1,
            Unavailable = 2
        }

        #endregion

    }
}

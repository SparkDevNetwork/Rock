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
using System.Text;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to process sign-up reminders.
    /// </summary>
    [DisplayName( "Sign-Up Reminder" )]
    [Description( "Send any sign-up reminders that are due to be sent." )]

    public class SendSignUpReminders : RockJob
    {
        #region Fields

        private SystemCommunicationService _systemCommunicationService;

        private readonly Dictionary<int, SystemCommunication> _systemCommunications = new Dictionary<int, SystemCommunication>();

        private readonly SendMessageResult _sendMessageResult = new SendMessageResult();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SendReminders"/> class.
        /// </summary>
        public SendSignUpReminders()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var now = RockDateTime.Now;

            using ( var rockContext = new RockContext() )
            {
                var recipientsByOpportunity = GetRecipientsByOpportunity( rockContext, now );

                if ( recipientsByOpportunity?.Any() == true )
                {
                    SendReminders( rockContext, recipientsByOpportunity, now );

                    if ( _sendMessageResult.MessagesSent > 0 )
                    {
                        // Save the timestamps we wrote to the recipients' GroupMemberAssignment.LastReminderSentDateTime(s).
                        rockContext.SaveChanges();
                    }
                }
            }

            string remindersSentMsg = _sendMessageResult.MessagesSent == 0
                ? "No reminders sent"
                : $"{_sendMessageResult.MessagesSent} {"reminder".PluralizeIf( _sendMessageResult.MessagesSent > 1 )} sent";

            if ( _sendMessageResult.Errors?.Any() == true || _sendMessageResult.Exceptions?.Any() == true )
            {
                var resultSb = new StringBuilder();

                if ( _sendMessageResult.Errors?.Any() == true )
                {
                    resultSb.Append( $"{remindersSentMsg}, completed with {FormatMessages( _sendMessageResult.Errors, "error" )}" );
                }
                else
                {
                    resultSb.Append( $"{remindersSentMsg}, completed with errors." );
                }

                AggregateException aggregateException = null;
                if ( _sendMessageResult.Exceptions?.Any() == true )
                {
                    aggregateException = new AggregateException( $"{nameof( SendSignUpReminders )} completed with errors.", _sendMessageResult.Exceptions );

                    resultSb.AppendLine();
                    resultSb.Append( "See exception log for more details." );
                }

                var result = resultSb.ToString();
                this.Result = result;

                throw new Exception( result, aggregateException );
            }
            else if ( _sendMessageResult.Warnings?.Any() == true )
            {
                this.Result = $"{remindersSentMsg}, completed with {FormatMessages( _sendMessageResult.Warnings, "warning" )}";

                throw new RockJobWarningException( $"{nameof( SendSignUpReminders )} completed with warnings." );
            }
            else
            {
                this.Result = $"<i class='fa fa-circle text-success'></i> {remindersSentMsg}";
            }
        }

        #region Private Methods

        /// <summary>
        /// Gets the sign-up reminder recipients - if any - grouped by opportunity (<see cref="Group"/>, <see cref="Location"/> and
        /// <see cref="Schedule"/>).
        /// <para>
        /// All filtering is done within this method; the recipients returned are qualified to receive reminders.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="now">A <see cref="DateTime"/> representing "now".</param>
        /// <returns>Sign-up reminder recipients - if any - grouped by opportunity (<see cref="Group"/>, <see cref="Location"/> and
        /// <see cref="Schedule"/>).</returns>
        private List<SignUpOpportunity> GetRecipientsByOpportunity( RockContext rockContext, DateTime now )
        {
            var groupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() );
            if ( !groupTypeId.HasValue )
            {
                _sendMessageResult.Errors.Add( "Unable to determine Sign-Up Group GroupType ID." );
                return null;
            }

            /*
             * Ensure we haven't already sent reminders to a given recipient today. The way this is currently written, we could send
             * reminders at 11:59:59 PM today and then again at 12:00:00 AM tomorrow. If this is problematic, we could change this to
             * be a true 24 hour TimeSpan requirement.
             * 
             * We'll also use this value to compare against Schedule.EffectiveEndDate below, in order to filter out past opportunities.
             */
            var startOfToday = now.StartOfDay();

            var recipientsByOpportunity = new GroupLocationService( rockContext )
                .Queryable()
                .Where( gl =>
                    gl.Group.IsActive
                    && gl.Group.ReminderSystemCommunicationId.HasValue
                    && ( gl.Group.GroupTypeId == groupTypeId.Value || gl.Group.GroupType.InheritedGroupTypeId == groupTypeId.Value )
                )
                /*
                 * Adding the following commented-out line to point out that it won't work for our purposes, because:
                 *  1) The Attribute we care about exists on the Group Entity, and not the GroupLocation Entity that our IQueryable is querying against.
                 *  2) According to our 202 guide, "Group attributes are the most complicated to load since they can inherit attributes from their parent
                 *     GroupType(s) and the [below] snippet wouldn't work if a Group inherited an attribute value from a GroupType." This is our exact
                 *     scenario: we can have GroupTypes that inherit from the default "Sign-Up Group" GroupType, and the Attribute we care about filtering
                 *     against - "ProjectType" - exists on the parent "Sign-Up Group" GroupType; we'll need to get all Groups up front, and filter out
                 *     the ones we don't care about below, in-memory.
                 */
                //.WhereAttributeValue( rockContext, "ProjectType", Rock.SystemGuid.DefinedValue.PROJECT_TYPE_IN_PERSON )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( glsc => glsc.ScheduleId == s.Id )
                } )
                /*
                 * We now have GroupLocationSchedules & each respective GroupLocationScheduleConfig (if defined):
                 * One instance for each combination of [active] Group, Location & Schedule, where GroupType (or inherited GroupType) == Sign-Up Group.
                 */
                .Where( gls => !gls.Schedule.EffectiveEndDate.HasValue || gls.Schedule.EffectiveEndDate.Value >= startOfToday )
                /*
                 * We've now filtered out past Schedules to reduce the initial results set returned; note that we'll need to perform additional
                 * Schedule filtering below, once we materialize the Schedule objects.
                 */
                .Select( gls => new
                {
                    gls.Group,
                    gls.Location,
                    gls.Schedule,
                    gls.Config,
                    Recipients = gls.Group.Members
                        .SelectMany( gm => gm.GroupMemberAssignments, ( gm, gma ) => new
                        {
                            gm.Person,
                            GroupMember = gm,
                            Assignment = gma
                        } )
                        .Where( gmas =>
                            !gmas.Person.IsDeceased
                            && gmas.Assignment.LocationId == gls.Location.Id
                            && gmas.Assignment.ScheduleId == gls.Schedule.Id
                            && (
                                !gmas.Assignment.LastReminderSentDateTime.HasValue
                                || gmas.Assignment.LastReminderSentDateTime < startOfToday
                            )
                        )
                } )
                .Where( opportunities => opportunities.Recipients.Any() )
                /*
                 * We now have a collection of People and their associated GroupMember & GroupMemberAssignment records,
                 * who we haven't sent a reminder to within the last day, grouped by sign-up opportunity.
                 */
                .ToList();

            if ( !recipientsByOpportunity.Any() )
            {
                return null;
            }

            /*
             * Next, we need to further refine these initial results to include only:
             *  1) Groups whose "Project Type" is "In Person".
             *  2) Schedules that actually have an upcoming start date equal to today +
             *      a) their Group's ReminderOffsetDays, or
             *      b) a default offset of 2 days if not defined.
             * 
             * Let's perform the Schedule filtering first, so we only look up the attributes for Groups that we actually care about.
             */
            recipientsByOpportunity = recipientsByOpportunity
                .Where( o =>
                {
                    var nextStartDate = o.Schedule.NextStartDateTime?.Date;
                    if ( !nextStartDate.HasValue )
                    {
                        return false;
                    }

                    var offsetDays = o.Group.ReminderOffsetDays ?? 2;

                    return nextStartDate == now.Date.AddDays( offsetDays );
                } )
            .ToList();

            if ( !recipientsByOpportunity.Any() )
            {
                return null;
            }

            // Load Groups' Attributes in bulk for the final comparison.
            var groups = recipientsByOpportunity.Select( o => o.Group ).Distinct().ToList();
            groups.LoadAttributes( new RockContext() );

            var inPersonProjectTypeGuid = Rock.SystemGuid.DefinedValue.PROJECT_TYPE_IN_PERSON.AsGuid();

            return recipientsByOpportunity
                .Where( o =>
                {
                    return o.Group.GetAttributeValue( "ProjectType" ).AsGuidOrNull() == inPersonProjectTypeGuid;
                } )
                .Select( o => new SignUpOpportunity
                {
                    Group = o.Group,
                    Location = o.Location,
                    Schedule = o.Schedule,
                    Config = o.Config,
                    Recipients = o.Recipients.Select( r => new ReminderRecipient
                    {
                        Person = r.Person,
                        GroupMember = r.GroupMember,
                        Assignment = r.Assignment
                    } )
                    .ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Attempts to send reminders to the specified recipients.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="recipientsByOpportunity">The sign-up reminder recipients.</param>
        /// <param name="now">A <see cref="DateTime"/> representing "now".</param>
        private void SendReminders( RockContext rockContext, List<SignUpOpportunity> recipientsByOpportunity, DateTime now )
        {
            var isSmsEnabled = MediumContainer.HasActiveSmsTransport();

            foreach ( var opportunity in recipientsByOpportunity )
            {
                var group = opportunity.Group;
                var location = opportunity.Location;
                var schedule = opportunity.Schedule;
                var config = opportunity.Config;

                var systemCommunication = GetSystemCommunication( rockContext, group.ReminderSystemCommunicationId.Value, isSmsEnabled );
                var jobCommunicationPreference = CommunicationType.RecipientPreference;

                if ( !isSmsEnabled || string.IsNullOrWhiteSpace( systemCommunication.SMSMessage ) )
                {
                    jobCommunicationPreference = CommunicationType.Email;
                }

                var additionalDetailsSb = new StringBuilder();
                if ( !string.IsNullOrWhiteSpace( group.ReminderAdditionalDetails ) )
                {
                    additionalDetailsSb.Append( group.ReminderAdditionalDetails );
                }

                if ( !string.IsNullOrWhiteSpace( config?.ReminderAdditionalDetails ) )
                {
                    additionalDetailsSb.Append( config.ReminderAdditionalDetails );
                }

                foreach ( var recipient in opportunity.Recipients )
                {
                    var person = recipient.Person;

                    var mediumType = Rock.Model.Communication.DetermineMediumEntityTypeId(
                        ( int ) CommunicationType.Email,
                        ( int ) CommunicationType.SMS,
                        ( int ) CommunicationType.PushNotification,
                        jobCommunicationPreference,
                        recipient.GroupMember.CommunicationPreference,
                        recipient.Person.CommunicationPreference );

                    var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeObjects.Add( "Registrant", person );
                    mergeObjects.Add( "ProjectName", group.Name );
                    mergeObjects.Add( "OpportunityName", config?.ConfigurationName );
                    mergeObjects.Add( "FriendlyLocation", location.ToString( true ) );
                    mergeObjects.Add( "StartDateTime", schedule.NextStartDateTime );
                    mergeObjects.Add( "Group", group );
                    mergeObjects.Add( "Location", location );
                    mergeObjects.Add( "Schedule", schedule );
                    mergeObjects.Add( "AdditionalDetails", additionalDetailsSb.ToString() );

                    try
                    {
                        var sendResult = CommunicationHelper.SendMessage( recipient.Person, mediumType, systemCommunication, mergeObjects );

                        _sendMessageResult.Warnings.AddRange( sendResult.Warnings );
                        _sendMessageResult.Errors.AddRange( sendResult.Errors );
                        _sendMessageResult.Exceptions.AddRange( sendResult.Exceptions );
                        _sendMessageResult.MessagesSent += sendResult.MessagesSent;

                        if ( sendResult.MessagesSent > 0 )
                        {
                            recipient.Assignment.LastReminderSentDateTime = now;
                        }
                    }
                    catch ( Exception ex )
                    {
                        _sendMessageResult.Exceptions.Add( ex );
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="SystemCommunication"/> from the local cache if already loaded, or from the database (and caches it locally) if not.
        /// <para>
        /// If the <see cref="SystemCommunication"/> is set up for SMS, but SMS is not enabled, a warning will be logged to the results of this job.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="systemCommunicationId">The <see cref="SystemCommunication"/> identifier.</param>
        /// <param name="isSmsEnabled">Whether SMS is enabled system-wide.</param>
        /// <returns>The <see cref="SystemCommunication"/> for the specified identifier.</returns>
        private SystemCommunication GetSystemCommunication( RockContext rockContext, int systemCommunicationId, bool isSmsEnabled )
        {
            if ( _systemCommunications.TryGetValue( systemCommunicationId, out SystemCommunication systemCommunication ) )
            {
                return systemCommunication;
            }

            if ( _systemCommunicationService == null )
            {
                _systemCommunicationService = new SystemCommunicationService( rockContext );
            }

            systemCommunication = _systemCommunicationService.Get( systemCommunicationId );

            if ( !string.IsNullOrWhiteSpace( systemCommunication.SMSMessage ) && !isSmsEnabled )
            {
                _sendMessageResult.Warnings.Add( $@"System communication ""{systemCommunication.Title}"" is set up to send SMS messages, but SMS isn't enabled. All sign-up reminders will be sent via email." );
            }

            _systemCommunications.AddOrReplace( systemCommunicationId, systemCommunication );

            return systemCommunication;
        }

        /// <summary>
        /// Formats messages in a consistent way, to be appended to the Job's final result string, Etc.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <param name="messageGroupLabel">The label for this group of messages; this label will be pluralized if necessary.</param>
        /// <returns>The formatted messages.</returns>
        private string FormatMessages( List<string> messages, string messageGroupLabel )
        {
            var messageCount = messages.Count;

            var sb = new StringBuilder();
            sb.Append( $"{messageCount} {messageGroupLabel.PluralizeIf( messageCount > 1 )}: " );

            messages.ForEach( m =>
            {
                var icon = string.Empty;
                if ( messageGroupLabel == "error" )
                {
                    icon = "<i class='fa fa-circle text-danger'></i> ";
                }
                else if ( messageGroupLabel == "warning" )
                {
                    icon = "<i class='fa fa-circle text-warning'></i> ";
                }

                sb.AppendLine();
                sb.Append( $"{icon}{m}" );
            } );

            return sb.ToString();
        }

        #endregion

        #region Supporting Classes

        private class SignUpOpportunity
        {
            public Group Group { get; set; }

            public Location Location { get; set; }

            public Schedule Schedule { get; set; }

            public GroupLocationScheduleConfig Config { get; set; }

            public List<ReminderRecipient> Recipients { get; set; }
        }

        private class ReminderRecipient
        {
            public Person Person { get; set; }

            public GroupMember GroupMember { get; set; }

            public GroupMemberAssignment Assignment { get; set; }
        }

        #endregion
    }
}

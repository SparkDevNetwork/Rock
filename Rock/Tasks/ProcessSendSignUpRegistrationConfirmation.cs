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
using System.Linq;
using System.Text;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Sends a sign-up registration confirmation email.
    /// </summary>
    public sealed class ProcessSendSignUpRegistrationConfirmation : BusStartedTask<ProcessSendSignUpRegistrationConfirmation.Message>
    {
        /// <summary>
        /// Executes this instance to send a sign-up registration confirmation email.
        /// </summary>
        /// <param name="message">The message class containing info needed to send a sign-up registration confirmation email.</param>
        public override void Execute( Message message )
        {
            if ( message == null || message.GroupMemberAssignmentId == default )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var confirmationData = new GroupLocationService( rockContext )
                    .Queryable()
                    .Where( gl =>
                        gl.Group.IsActive
                        && gl.Group.Id == message.GroupId
                        && gl.Location.Id == message.LocationId
                    )
                    .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                    {
                        gl.Group,
                        gl.Location,
                        Schedule = s,
                        Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( glsc => glsc.ScheduleId == s.Id )
                    } )
                    .Where( gls => gls.Schedule.Id == message.ScheduleId )
                    .Select( gls => new
                    {
                        gls.Group,
                        gls.Location,
                        gls.Schedule,
                        gls.Config,
                        Recipient = gls.Group.Members
                            .SelectMany( gm => gm.GroupMemberAssignments, ( gm, gma ) => new
                            {
                                gm.Person,
                                GroupMember = gm,
                                Assignment = gma
                            } )
                            .FirstOrDefault( gmas =>
                                !gmas.Person.IsDeceased
                                && gmas.Assignment.Id == message.GroupMemberAssignmentId
                            )
                    } )
                    .FirstOrDefault();

                if ( confirmationData == null
                    || confirmationData.Group == null
                    || confirmationData.Location == null
                    || confirmationData.Schedule == null
                    || confirmationData.Recipient?.GroupMember == null
                    || confirmationData.Recipient?.Person == null )
                {
                    ExceptionLogService.LogException( $"Unable to find necessary data to send sign-up project registration confirmation. Group ID = {message.GroupId}, Location ID = {message.LocationId}, Schedule ID = {message.ScheduleId}, GroupMemberAssignment ID = {message.GroupMemberAssignmentId}." );
                    return;
                }

                var systemCommunication = new SystemCommunicationService( rockContext ).GetNoTracking( message.SystemCommunicationGuid );
                if ( systemCommunication == null )
                {
                    ExceptionLogService.LogException( $"Unable to find system communication with Guid '{message.SystemCommunicationGuid}'." );
                    return;
                }

                if ( string.IsNullOrEmpty( systemCommunication.Body ) )
                {
                    ExceptionLogService.LogException( $"System communication with Guid '{message.SystemCommunicationGuid}' has no body." );
                    return;
                }

                var isSmsEnabled = MediumContainer.HasActiveSmsTransport();
                var templateCommunicationPreference = CommunicationType.RecipientPreference;
                if ( !isSmsEnabled || string.IsNullOrWhiteSpace( systemCommunication.SMSMessage ) )
                {
                    templateCommunicationPreference = CommunicationType.Email;
                }

                var group = confirmationData.Group;
                var location = confirmationData.Location;
                var schedule = confirmationData.Schedule;
                var config = confirmationData.Config;
                var groupMember = confirmationData.Recipient.GroupMember;
                var assignment = confirmationData.Recipient.Assignment;
                var person = confirmationData.Recipient.Person;

                var additionalDetailsSb = new StringBuilder();
                if ( !string.IsNullOrWhiteSpace( group.ConfirmationAdditionalDetails ) )
                {
                    additionalDetailsSb.Append( group.ConfirmationAdditionalDetails );
                }

                if ( !string.IsNullOrWhiteSpace( config?.ConfirmationAdditionalDetails ) )
                {
                    additionalDetailsSb.Append( config.ConfirmationAdditionalDetails );
                }

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

                var mediumType = Rock.Model.Communication.DetermineMediumEntityTypeId(
                    ( int ) CommunicationType.Email,
                    ( int ) CommunicationType.SMS,
                    ( int ) CommunicationType.PushNotification,
                    templateCommunicationPreference,
                    groupMember.CommunicationPreference,
                    person.CommunicationPreference );

                try
                {
                    var sendResult = CommunicationHelper.SendMessage( person, mediumType, systemCommunication, mergeObjects );

                    var errorMessage = $"Error while sending sign-up project registration confirmation for Group ID = {message.GroupId}, Location ID = {message.LocationId}, Schedule ID = {message.ScheduleId}, GroupMemberAssignment ID = {message.GroupMemberAssignmentId}, SystemCommunication Guid = '{message.SystemCommunicationGuid}'.";

                    if ( sendResult.Errors?.Any() == true )
                    {
                        ExceptionLogService.LogException( $"{errorMessage} {string.Join( "; ", sendResult.Errors )}" );
                    }

                    if ( sendResult.Exceptions?.Any() == true )
                    {
                        ExceptionLogService.LogException( new AggregateException( errorMessage, sendResult.Exceptions ) );
                    }

                    if ( sendResult.MessagesSent > 0 )
                    {
                        assignment.ConfirmationSentDateTime = RockDateTime.Now;
                        rockContext.SaveChanges();
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        /// <summary>
        /// The message class containing info needed to send a sign-up registration confirmation email.
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the sign-up project's <see cref="Rock.Model.Group"/> identifier.
            /// </summary>
            /// <value>
            /// The sign-up project's <see cref="Rock.Model.Group"/> identifier.
            /// </value>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets or sets the sign-up project's <see cref="Rock.Model.Location"/> identifier.
            /// </summary>
            /// <value>
            /// The sign-up project's <see cref="Rock.Model.Location"/> identifier.
            /// </value>
            public int LocationId { get; set; }

            /// <summary>
            /// Gets or sets the sign-up project's <see cref="Rock.Model.Schedule"/> identifier.
            /// </summary>
            /// <value>
            /// The sign-up project's <see cref="Rock.Model.Schedule"/> identifier.
            /// </value>
            public int ScheduleId { get; set; }

            /// <summary>
            /// Gets or sets the sign-up registrant's <see cref="Rock.Model.GroupMemberAssignment"/> identifier.
            /// </summary>
            /// <value>
            /// The sign-up registrant <see cref="Rock.Model.GroupMemberAssignment"/> identifier.
            /// </value>
            public int GroupMemberAssignmentId { get; set; }

            /// <summary>
            /// Gets or sets the registrant confirmation <see cref="SystemCommunication"/> identifier.
            /// </summary>
            /// <value>
            /// The registrant confirmation <see cref="SystemCommunication"/> identifier.
            /// </value>
            public Guid SystemCommunicationGuid { get; set; }

            /// <summary>
            /// Gets or sets the application root.
            /// </summary>
            /// <value>
            /// The application root.
            /// </value>
            public string AppRoot { get; set; }

            /// <summary>
            /// Gets or sets the theme root.
            /// </summary>
            /// <value>
            /// The theme root.
            /// </value>
            public string ThemeRoot { get; set; }
        }
    }
}

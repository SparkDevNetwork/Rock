using System;
using System.Data.Entity;
using System.Linq;
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
                var groupMemberAssignment = new GroupMemberAssignmentService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( gma => gma.GroupMember.Person )
                    .Include( gma => gma.GroupMember.Group )
                    .Include( gma => gma.Location )
                    .Include( gma => gma.Schedule )
                    .FirstOrDefault( gma => gma.Id == message.GroupMemberAssignmentId );

                if ( groupMemberAssignment == null
                     || string.IsNullOrEmpty( groupMemberAssignment.GroupMember.Person.Email )
                     || groupMemberAssignment.Location == null
                     || groupMemberAssignment.Schedule == null )
                {
                    return;
                }

                var systemCommunication = new SystemCommunicationService( rockContext )
                    .GetNoTracking( message.SystemCommunicationGuid );

                if ( systemCommunication == null || string.IsNullOrEmpty( systemCommunication.Body ) )
                {
                    return;
                }

                var person = groupMemberAssignment.GroupMember.Person;
                var group = groupMemberAssignment.GroupMember.Group;
                var location = groupMemberAssignment.Location;
                var schedule = groupMemberAssignment.Schedule;

                var groupLocationScheduleConfig = new GroupLocationService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( gl => gl.GroupLocationScheduleConfigs )
                    .Where( gl => gl.GroupId == group.Id && gl.LocationId == location.Id )
                    .Select( gl => gl.GroupLocationScheduleConfigs.FirstOrDefault( glsc => glsc.ScheduleId == schedule.Id ) )
                    .FirstOrDefault();

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, groupMemberAssignment.GroupMember.Person );
                mergeFields.Add( "Registrant", person );
                mergeFields.Add( "ProjectName", group.Name );
                mergeFields.Add( "OpportunityName", groupLocationScheduleConfig?.ConfigurationName );
                mergeFields.Add( "FriendlyLocation", location.ToString( true ) );
                mergeFields.Add( "StartDateTime", schedule.NextStartDateTime );
                mergeFields.Add( "Group", group );
                mergeFields.Add( "Location", location );
                mergeFields.Add( "Schedule", schedule );

                var emailMessage = new RockEmailMessage();
                emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
                emailMessage.FromEmail = systemCommunication.From;
                emailMessage.FromName = systemCommunication.FromName;
                emailMessage.Subject = systemCommunication.Subject;
                emailMessage.Message = systemCommunication.Body;
                emailMessage.AppRoot = message.AppRoot;
                emailMessage.ThemeRoot = message.ThemeRoot;
                emailMessage.Send();
            }
        }

        /// <summary>
        /// The message class containing info needed to send a sign-up registration confirmation email.
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the sign-up registrant's <see cref="Rock.Model.GroupMemberAssignment"/> identifier.
            /// </summary>
            /// <value>
            /// The sign-up registrant <see cref="Rock.Model.GroupMemberAssignment"/> identifier.
            /// </value>
            public int GroupMemberAssignmentId { get; set; }

            /// <summary>
            /// Gets or sets the registrant confimation <see cref="SystemCommunication"/> identifier.
            /// </summary>
            /// <value>
            /// The registrant confimation <see cref="SystemCommunication"/> identifier.
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

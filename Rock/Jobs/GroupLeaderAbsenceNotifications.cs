using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    [GroupTypeField( "Group Type", "The group type to look for absent group members.", true, "", "", 0 )]
    [SystemEmailField( "Notification Email", "", true, "", "", 1 )]
    [GroupRoleField( null, "Group Role Filter", " Optional group role to filter the absent members by.To select the role you’ll need to select a group type.", false, null, null, 2 )]
    [IntegerField( "Minimum Absences",
                   @"The number of most recent consecutive meeting occurrences that the group member will need to have
                      missed to be included in the notification email. If group attendance is not recorded or the group did
                      not meet, that occurrence will not be considered.", false, 3, order: 3 )]
    [DisallowConcurrentExecution]
    public class GroupLeaderAbsenceNotifications : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GroupLeaderAbsenceNotifications()
        {
        }

        /// <summary>
        /// Job that will sync groups.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            try
            {
                int notificationsSent = 0;
                int errorsEncountered = 0;
                int pendingMembersCount = 0;
                int sendFailed = 0;

                // get groups set to sync
                RockContext rockContext = new RockContext();

                Guid? groupTypeGuid = dataMap.GetString( "GroupType" ).AsGuidOrNull();
                Guid? systemEmailGuid = dataMap.GetString( "NotificationEmail" ).AsGuidOrNull();
                Guid? groupRoleFilterGuid = dataMap.GetString( "GroupRoleFilter" ).AsGuidOrNull();
                int minimumAbsences = dataMap.GetString( "MinimumAbsences" ).AsInteger();

                // get system email
                SystemEmailService emailService = new SystemEmailService( rockContext );

                SystemEmail systemEmail = null;
                if ( !systemEmailGuid.HasValue || systemEmailGuid == Guid.Empty )
                {
                    context.Result = "Job failed. Unable to find System Email";
                    throw new Exception( "No system email found." );
                }

                if ( minimumAbsences == default( int ) )
                {
                    context.Result = "Job failed. The is no minimum absense count entered.";
                    throw new Exception( "no minimum absense count found." );
                }

                systemEmail = emailService.Get( systemEmailGuid.Value );

                // get group members
                if ( !groupTypeGuid.HasValue || groupTypeGuid == Guid.Empty )
                {
                    context.Result = "Job failed. Unable to find group type";
                    throw new Exception( "No group type found" );
                }

                var groupMemberQry = new GroupMemberService( rockContext ).Queryable( "Group, Group.Members.GroupRole" )
                                         .Where( m => m.Group.GroupType.Guid == groupTypeGuid.Value );

                if ( groupRoleFilterGuid.HasValue )
                {
                    groupMemberQry = groupMemberQry.Where( m => m.GroupRole.Guid == groupRoleFilterGuid.Value );
                }

                var groupMembers = groupMemberQry.GroupBy( m => m.Group );
                var errorList = new List<string>();
                foreach ( var groupGroupMember in groupMembers )
                {
                    var group = groupGroupMember.Key;
                    // get list of leaders
                    var groupLeaders = group.Members.Where( m => m.GroupRole.IsLeader == true && m.Person != null && m.Person.Email != null && m.Person.Email != string.Empty );

                    if ( !groupLeaders.Any() )
                    {
                        errorList.Add( "Unable to send emails to members in group " + group.Name + " because there is no group leader" );
                        continue;
                    }

                    // Get all the occurrences for this group for the selected dates, location and schedule
                    var occurrences = new AttendanceOccurrenceService( rockContext )
                        .Queryable( "Attendees.PersonAlias.Person" )
                        .Where( a => a.DidNotOccur.HasValue && !a.DidNotOccur.Value )
                        .OrderByDescending( a => a.OccurrenceDate )
                        .Take( minimumAbsences )
                        .ToList();

                    if ( occurrences.Count == minimumAbsences )
                    {
                        var absentPersons = occurrences
                                                    .SelectMany( a => a.Attendees )
                                                    .Where( a => a.DidAttend.HasValue && !a.DidAttend.Value )
                                                    .GroupBy( a => a.PersonAlias.Person )
                                                    .Where( a => a.Count() == minimumAbsences )
                                                    .Select( a => a.Key )
                                                    .ToList();

                        if ( absentPersons.Count > 0 )
                        {
                            var recipients = new List<RecipientData>();
                            foreach ( var leader in groupLeaders )
                            {
                                // create merge object
                                var mergeFields = new Dictionary<string, object>();
                                mergeFields.Add( "AbsentMembers", absentPersons );
                                mergeFields.Add( "Group", group );
                                mergeFields.Add( "Person", leader.Person );
                                recipients.Add( new RecipientData( leader.Person.Email, mergeFields ) );
                            }


                            var errorMessages = new List<string>();
                            var emailMessage = new RockEmailMessage( systemEmail.Guid );
                            emailMessage.SetRecipients( recipients );
                            var sendSuccess = emailMessage.Send( out errorMessages );
                            if ( !sendSuccess )
                            {
                                sendFailed++;
                            }

                            errorsEncountered += errorMessages.Count;
                            errorList.AddRange( errorMessages );

                            // be conservative: only mark as notified if we are sure the email didn't fail 
                            if ( errorMessages.Any() )
                            {
                                continue;
                            }

                            notificationsSent += recipients.Count();
                        }
                    }
                }

                context.Result = string.Format( "Sent {0} emails to leaders for {1} absent members. {2} errors encountered. {3} times Send reported a fail.", notificationsSent, pendingMembersCount, errorsEncountered, sendFailed );
                if ( errorList.Any() )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.Append( "Errors in GroupLeaderPendingNotificationJob: " );
                    errorList.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errors = sb.ToString();
                    context.Result += errors;
                    throw new Exception( errors );
                }
            }
            catch ( Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw;
            }
        }
    }
}
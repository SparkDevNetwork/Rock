// <copyright>
// Copyright Pillars Inc.
// </copyright>
//
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Tasks;
using Rock.Web.Cache;

namespace rocks.pillars.Jobs
{
    /// <summary>
    /// Custom job to process communications. This job inherits from core version, but adds a
    /// check to see if person sending communication is actually sending from their own email
    /// address or from an email they are authorized to send on behalf of. If not, the job will 
    /// set the communication to pending approval and notify the approval group.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: For this job to work correctly, it is important that the Communication 
    /// entry blocks are configured to not 'Send When Approved!'
    /// </remarks>
    [AttributeField(
        name: "Send On Behalf Of Attribute",
        description: "The person attribute that contains email address that user is allowed to send on behalf of.",
        required: false,
        entityTypeGuid: Rock.SystemGuid.EntityType.PERSON,
        order: 2,
        key: "SendOnBehalfOfAttribute" )]

    [SecurityRoleField(
        name: "Approval Role",
        description: "Security role who's members are authorized to approve communications and thus allowed to send on anyone's behalf.",
        required: false,
        order: 3,
        key: "ApprovalRole" )]

    [TimeField(
        name: "Daily Start Time",
        description: "The time each day that this job will start to process communications. It will not process any communications prior to this time each day.",
        required: false,
        order: 4,
        key: "StartTime")]

    [TimeField(
        name: "Daily Stop Time",
        description: "The time each day that this job will stop processing communications. It will not process any communications after this time each day.",
        required: false,
        order: 4,
        key: "StopTime" )]

    public class SendCommunications : Rock.Jobs.SendCommunications
    {
        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute()
        {
            var now = RockDateTime.Now;
            var startTime = this.GetAttributeValue( "StartTime" ).AsTimeSpan();
            if ( startTime.HasValue )
            {
                var startDateTime = RockDateTime.Today.Add( startTime.Value );
                if ( startDateTime > now )
                {
                    Result = $"Current time is earlier than the configured Daily Start Time of {startDateTime:h:mm tt}; No communications were processed.";
                    return;
                }
            }
            var stopTime = this.GetAttributeValue( "StopTime" ).AsTimeSpan();
            if ( stopTime.HasValue )
            {
                var stopDateTime = RockDateTime.Today.Add( stopTime.Value );
                if ( stopDateTime < now )
                {
                    Result = $"Current time is later than the configured Daily Stop Time of {stopDateTime:hh:mm tt}; No communications were processed.";
                    return;
                }
            }

            int expirationDays = this.GetAttributeValue( "ExpirationPeriod" ).AsInteger();
            int delayMinutes = this.GetAttributeValue( "DelayPeriod" ).AsInteger();
            var attributeGuid = this.GetAttributeValue( "SendOnBehalfOfAttribute" ).AsGuidOrNull();
            var approvalRoleGuid = this.GetAttributeValue( "ApprovalRole" ).AsGuidOrNull();

            using ( var rockContext = new RockContext() )
            {
                // Get Person Attribute
                AttributeCache sendOnBehalfOfAttr = null;
                if ( attributeGuid.HasValue )
                {
                    sendOnBehalfOfAttr = AttributeCache.Get( attributeGuid.Value );
                }

                // Get List of Approver Person Ids
                var approvers = new List<int>();
                if ( approvalRoleGuid.HasValue )
                {
                    approvers = new GroupMemberService( rockContext )
                        .GetByGroupGuid( approvalRoleGuid.Value )
                        .Where( m =>
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            !m.IsArchived )
                        .Select( m => m.PersonId )
                        .ToList();
                }

                // Loop through each queued communicatoin
                foreach ( var comm in new CommunicationService( rockContext )
                    .GetQueued( expirationDays, delayMinutes, false, false )
                    .ToList() )
                {
                    // Ignore if sender is an approver
                    var sender = comm.SenderPersonAlias?.Person;
                    if ( sender != null && approvers.Contains( sender.Id ) )
                    {
                        continue;
                    }

                    // Ignore if person's email is same as from email address
                    string personEmail = sender?.Email;
                    string fromEmail = comm.FromEmail;
                    if ( personEmail.Equals( fromEmail, System.StringComparison.OrdinalIgnoreCase ) )
                    {
                        continue;
                    }

                    // Ignore if user is authorized to send on behalf of email
                    if ( sendOnBehalfOfAttr != null )
                    {
                        sender.LoadAttributes();
                        var emails = sender.GetAttributeValues( sendOnBehalfOfAttr.Key );
                        if ( emails.Any( e => e.Equals( fromEmail, System.StringComparison.OrdinalIgnoreCase ) ) )
                        {
                            continue;
                        }
                    }

                    // Ignore if approver is different than sender (already approved)
                    var reviewerId = comm.ReviewerPersonAlias?.Person?.Id;
                    var senderId = sender?.Id;
                    if ( reviewerId.HasValue && senderId.HasValue && reviewerId.Value != senderId.Value )
                    {
                        continue;
                    }

                    // Update communication to require approval
                    comm.Status = CommunicationStatus.PendingApproval;
                    comm.ReviewedDateTime = null;
                    comm.ReviewerPersonAliasId = null;

                    rockContext.SaveChanges();

                    // Notify approvers of email
                    var processSendCommunicationMsg = new ProcessSendCommunication.Message
                    {
                        CommunicationId = comm.Id,
                    };
                    processSendCommunicationMsg.Send();
                }
            }

            base.Execute();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace church.ccv.Utility.Jobs
{
    /// <summary>
    /// ..
    /// </summary>
    [SystemEmailField( "Email Template", required: true )]
    [GroupTypeField("Group Type", "Group type to use as a geo-fence (aka neighborhood).",true)]
    [DisallowConcurrentExecution]
    class NextStepFollowUp : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public NextStepFollowUp()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute(IJobExecutionContext context)
        {
            var rockContext = new RockContext();

            SystemEmailService emailService = new SystemEmailService( rockContext );
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            Guid? systemEmailGuid = dataMap.GetString( "EmailTemplate" ).AsGuidOrNull();
            Guid groupTypeGuid = dataMap.GetString( "GroupType" ).AsGuid();

            // get system email
            SystemEmail systemEmail = null;
            if ( systemEmailGuid.HasValue )
            {
                systemEmail = emailService.Get( systemEmailGuid.Value );
            }

            if ( groupTypeGuid != null && groupTypeGuid != Guid.Empty )
            {
                var groupType = new GroupTypeService( rockContext ).Get( groupTypeGuid );

                var groupMemberEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( typeof( GroupMember ) );

                var followUpGroupMembers = new AttributeValueService( rockContext ).Queryable()
                    .Where( a => a.Attribute.Key == "FollowUpDate" &&
                            a.Attribute.EntityTypeId == groupMemberEntityTypeId &&
                            a.ValueAsDateTime <= RockDateTime.Now )
                    .Select( a => a.EntityId );

                var groupMembers = new GroupMemberService( rockContext ).Queryable()
                    .Where( g =>
                            g.Group.GroupTypeId == groupType.Id &&
                            g.GroupMemberStatus == GroupMemberStatus.Inactive &&
                            followUpGroupMembers.Contains( ( g as GroupMember ).Id ) );

                foreach ( var groupMember in groupMembers )
                {

                    // set status to pending
                    groupMember.GroupMemberStatus = GroupMemberStatus.Pending;

                    // get coach
                    var coach = new GroupMemberService( rockContext )
                        .GetLeaders( groupMember.Group.Id )
                        .Select( m => m.Person )
                        .FirstOrDefault();

                    // send email
                    if ( coach != null )
                    {
                        var recipients = new List<string>();
                        var mergeFields = new Dictionary<string, object>();

                        recipients.Add( groupMember.Person.Email );

                        var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                        Email.Send( systemEmail.From, systemEmail.FromName, systemEmail.Subject, recipients, systemEmail.Body, appRoot );
                    }
                }

                rockContext.SaveChanges();
            }
        }
    }
}

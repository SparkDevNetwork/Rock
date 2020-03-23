using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;


namespace com.bemaservices.OpenConnectionsDigest.Jobs
{
    [GroupField( "Connection Group", "Members of this group who are connectors of any opportunity with connections assigned will be emailed.", false, null, null, 1 )]
    [CustomCheckboxListField( "Connection Opportunities", "Select the connection opportunities you would like to include.", "SELECT Guid AS Value, Name AS Text FROM ConnectionOpportunity WHERE IsActive = 1;" )]
    [SystemEmailField( "System Email", "The system email to use when sending reminder.", true, "a1911882-19dd-4197-a8d8-63cbd8a7d80b", null, 2 )]
    [BooleanField( "Save Communication History", "Should a record of this communication be saved to the recipient's profile", false, "" )]
    [BooleanField( "Include Opportunity Breakdown", "Should the email digest include an opportunity breakdown?", true, "" )]
    [BooleanField( "Include All Requests", "Should the email digest include a line for every connection request?", true, "" )]

    [DisallowConcurrentExecution]
    public class SendOpenConnectionsDigestEmail : IJob
    {
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var connectionRequestService = new ConnectionRequestService( rockContext );

            var group = groupService.GetByGuid( dataMap.GetString( "ConnectionGroup" ).AsGuid() );

            var systemEmail = dataMap.GetString( "SystemEmail" ).AsGuidOrNull();
            if ( !systemEmail.HasValue )
            {
                throw new Exception( "System Email is required!" );
            }

            List<int> connectorPersonAliasIds = new List<int>();

            var recipients = new List<RecipientData>();

            if ( group != null )
            {

                var childrenGroups = groupService.GetAllDescendentGroups( group.Id, false );

                var allGroups = childrenGroups.ToList();

                allGroups.Add( group );

                connectorPersonAliasIds = allGroups.SelectMany( p => p.Members.Select( m => m.Person ).SelectMany( psn => psn.Aliases ).Select( a => a.Id ) ).ToList();

            }


            var connectionOpportunities = dataMap.GetString( "ConnectionOpportunities" ).SplitDelimitedValues();

            // get job type id
            int jobId = Convert.ToInt16( context.JobDetail.Description );
            var jobService = new ServiceJobService( rockContext );
            var job = jobService.Get( jobId );

            DateTime _midnightToday = RockDateTime.Today.AddDays( 1 );
            var currentDateTime = RockDateTime.Now;
            PersonService personService = new PersonService( rockContext );
            var connectionRequestsQry = connectionRequestService.Queryable().Where( cr =>
                                       connectionOpportunities.Contains( cr.ConnectionOpportunity.Guid.ToString() )
                                       && cr.ConnectorPersonAliasId != null
                                       && (
                                            cr.ConnectionState == ConnectionState.Active
                                            || ( cr.ConnectionState == ConnectionState.FutureFollowUp && cr.FollowupDate.HasValue && cr.FollowupDate.Value < _midnightToday )
                                        ) );

            // If we have a group of connectors, limit it to them.
            if ( group != null )
            {
                List<int> groupconnectorPersonIds = group.ActiveMembers().SelectMany( gm => gm.Person.Aliases ).Select( a => a.Id ).ToList();
                connectionRequestsQry = connectionRequestsQry.Where( cr => cr.ConnectorPersonAliasId.HasValue && groupconnectorPersonIds.Contains( cr.ConnectorPersonAliasId.Value ) );
            }

            // Now get all the connection data for everyone.
            var connectionRequestGroups = connectionRequestsQry.GroupBy( cr => cr.ConnectorPersonAlias.PersonId );
            foreach ( var connectionRequestGroup in connectionRequestGroups )
            {
                Person person = personService.Get( connectionRequestGroup.Key );
                List<ConnectionOpportunity> opportunities = connectionRequestGroup.Select( a => a.ConnectionOpportunity ).Distinct().ToList();
                var newConnectionRequests = connectionRequestGroup.Where( cr => cr.CreatedDateTime >= job.LastSuccessfulRunDateTime ).GroupBy( cr => cr.ConnectionOpportunityId ).ToList();
                // Get all the idle connections
                var idleConnectionRequests = connectionRequestGroup
                                    .Where( cr => (
                                            ( cr.ConnectionRequestActivities.Any() && cr.ConnectionRequestActivities.Max( ra => ra.CreatedDateTime ) < currentDateTime.AddDays( -cr.ConnectionOpportunity.ConnectionType.DaysUntilRequestIdle ) ) )
                                            || ( !cr.ConnectionRequestActivities.Any() && cr.CreatedDateTime < currentDateTime.AddDays( -cr.ConnectionOpportunity.ConnectionType.DaysUntilRequestIdle ) )
                                           )
                                    .Select( a => new { ConnectionOpportunityId = a.ConnectionOpportunityId, Id = a.Id } )
                                    .GroupBy( cr => cr.ConnectionOpportunityId ).ToList();

                // get list of requests that have a status that is considered critical.
                var criticalConnectionRequests = connectionRequestGroup
                                        .Where( r =>
                                            r.ConnectionStatus.IsCritical
                                        )
                                        .Select( a => new { ConnectionOpportunityId = a.ConnectionOpportunityId, Id = a.Id } )
                                        .GroupBy( cr => cr.ConnectionOpportunityId ).ToList();

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "Requests", connectionRequestGroup.Select( c => c ).ToList() );
                mergeFields.Add( "ConnectionOpportunities", opportunities );
                mergeFields.Add( "ConnectionRequests", connectionRequestGroup.GroupBy( cr => cr.ConnectionOpportunity ).ToList() );
                mergeFields.Add( "NewConnectionRequests", newConnectionRequests );
                mergeFields.Add( "IdleConnectionRequestIds", idleConnectionRequests );
                mergeFields.Add( "CriticalConnectionRequestIds", criticalConnectionRequests );
                mergeFields.Add( "Person", person );
                mergeFields.Add( "LastRunDate", job.LastSuccessfulRunDateTime );
                mergeFields.Add( "IncludeOpportunityBreakdown", dataMap.GetString( "IncludeOpportunityBreakdown" ).AsBoolean() );
                mergeFields.Add( "IncludeAllRequests", dataMap.GetString( "IncludeAllRequests" ).AsBoolean() );

                recipients.Add( new RecipientData( person.Email, mergeFields ) );

            }

            // If we have valid recipients, send the email
            if ( recipients.Count > 0 )
            {
                RockEmailMessage email = new RockEmailMessage( systemEmail.Value );
                email.SetRecipients( recipients );
                email.CreateCommunicationRecord = dataMap.GetString( "SaveCommunicationHistory" ).AsBoolean();
                email.Send();
            }

            context.Result = string.Format( "{0} Connection reminders sent", recipients.Count );

        }
    }
}
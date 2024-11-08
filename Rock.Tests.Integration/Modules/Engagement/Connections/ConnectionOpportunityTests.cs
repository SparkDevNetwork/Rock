using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Engagement.Connections
{
    [TestClass]
    public class ConnectionOpportunityTests : DatabaseTestsBase
    {
        [TestMethod]
        public void ConnectionOpportunity_NewSignupProducesWorkflow()
        {
            var testStartDateTime = RockDateTime.Now;

            var rockContext = new RockContext();

            var connectionRequestService = new ConnectionRequestService( rockContext );

            // Serve Children's Opportunity
            var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( "2c09211f-e19e-4685-9b67-4edaa9945a97".AsGuid() );

            int defaultStatusId = connectionOpportunity.ConnectionType.ConnectionStatuses
                    .Where( s => s.IsDefault )
                    .Select( s => s.Id )
                    .FirstOrDefault();

            var connectionRequest = new ConnectionRequest();
            connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
            connectionRequest.ConnectionTypeId = connectionOpportunity.ConnectionTypeId;
            connectionRequest.PersonAliasId = connectionOpportunity.ModifiedByPersonAliasId.Value;
            connectionRequest.ConnectionStatusId = defaultStatusId;
            ConnectionWorkflow testConnectionWorkflow = null;

            if ( !connectionOpportunity.ConnectionWorkflows.Any() )
            {
                testConnectionWorkflow = new ConnectionWorkflow();

                // Whatever workflow type id. 13 is 'External Inquiry'
                var workflowType = new WorkflowTypeService(rockContext).Get( 13 );
                if ( !workflowType.IsPersisted )
                {
                    // make sure it is Persisted so that the produced workflow gets saved to the database
                    workflowType.IsPersisted = true;
                    rockContext.SaveChanges();
                }

                testConnectionWorkflow.WorkflowTypeId = 13;
                testConnectionWorkflow.TriggerType = ConnectionWorkflowTriggerType.RequestStarted;
                testConnectionWorkflow.ConnectionTypeId = connectionOpportunity.ConnectionTypeId;
                connectionOpportunity.ConnectionWorkflows.Add( testConnectionWorkflow );
                rockContext.SaveChanges();
            }

            rockContext.WrapTransaction( () =>
            {
                connectionRequestService.Add( connectionRequest );
                rockContext.SaveChanges();
                connectionRequest.SaveAttributeValues( rockContext );

                // simulate the transaction taking longer. This helps test that the 'ProcessConnectionRequestChange.SendWhen' message is waiting for the database commit.
                Thread.Sleep( 1000 );
            } );

            using ( var rockContext2 = new RockContext() )
            {
                var connectionRequestWorkflowService = new ConnectionRequestWorkflowService( rockContext2 );
                bool producedConnectionRequestWorkflow = false;
                int retryAttempt = 0;

                // the connection request workflow gets produced in a seperate thread, so check for up to 10 seconds to see if it created one
                while ( !producedConnectionRequestWorkflow && retryAttempt < 10 )
                {
                    var connectionRequestWorkflowQuery = connectionRequestWorkflowService.Queryable()
                        .Where( a => a.ConnectionRequestId == connectionRequest.Id && a.CreatedDateTime > testStartDateTime );

                    producedConnectionRequestWorkflow = connectionRequestWorkflowQuery.Any();
                    if ( producedConnectionRequestWorkflow )
                    {
                        // Looks like it created it, so cleanup and break out
                        connectionRequestWorkflowService.DeleteRange( connectionRequestWorkflowQuery.ToList() );

                        break;
                    }

                    retryAttempt++;
                    Task.Delay( 1000 ).Wait();
                }

                // clean up if we needed to create a testConnectionWorkflow
                if ( testConnectionWorkflow != null)
                {
                    new ConnectionWorkflowService( rockContext ).Delete( testConnectionWorkflow );
                    rockContext.SaveChanges();
                }

                // test expected condition
                Assert.IsTrue( producedConnectionRequestWorkflow );
            }
        }
    }
}

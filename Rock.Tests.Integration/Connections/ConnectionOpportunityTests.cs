using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Integration.Connections
{
    [TestClass]
    public class ConnectionOpportunityTests
    {
        [TestMethod]
        public void ConnectionOpportunity_NewSignupProducesWorkflow()
        {
            //ConfigureEntitySaveHooks();
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
            connectionRequest.PersonAliasId = connectionOpportunity.ModifiedByPersonAliasId.Value;
            connectionRequest.ConnectionStatusId = defaultStatusId;

            if ( !connectionOpportunity.ConnectionWorkflows.Any() )
            {
                var connectionWorkflow = new ConnectionWorkflow();

                // Whatever workflow type id. 13 is 'External Inquiry'
                var workflowType = new WorkflowTypeService(rockContext).Get( 13 );
                if ( !workflowType.IsPersisted )
                {
                    // make sure it is Persisted so that the produced workflow gets saved to the database
                    workflowType.IsPersisted = true;
                    rockContext.SaveChanges();
                }

                
                connectionWorkflow.WorkflowTypeId = 13;
                connectionWorkflow.TriggerType = ConnectionWorkflowTriggerType.RequestStarted;
                connectionWorkflow.ConnectionTypeId = connectionOpportunity.ConnectionTypeId;
                connectionOpportunity.ConnectionWorkflows.Add( connectionWorkflow );
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
                        break;
                    }

                    retryAttempt++;
                    Task.Delay( 1000 ).Wait();
                }

                Debug.WriteLine( retryAttempt );
                Assert.IsTrue( producedConnectionRequestWorkflow );
            }
        }

        /// <summary>
        /// Searches all assemblies for <see cref="IEntitySaveHook"/> subclasses
        /// that need to be registered in the default save hook provider.
        /// </summary>
        private static void ConfigureEntitySaveHooks()
        {
            var hookProvider = Rock.Data.DbContext.SharedSaveHookProvider;
            var entityHookType = typeof( EntitySaveHook<> );

            var hookTypes = Rock.Reflection.FindTypes( typeof( Rock.Data.IEntitySaveHook ) )
                .Select( a => a.Value )
                .ToList();

            foreach ( var hookType in hookTypes )
            {
                if ( !hookType.IsDescendentOf( entityHookType ) )
                {
                    continue;
                }

                var genericTypes = hookType.GetGenericArgumentsOfBaseType( entityHookType );
                var entityType = genericTypes[0];

                if ( entityType.Assembly == hookType.Assembly )
                {
                    hookProvider.AddHook( entityType, hookType );
                }
            }
        }
    }
}

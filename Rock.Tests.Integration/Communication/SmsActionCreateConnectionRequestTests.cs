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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication;
using Rock.Communication.SmsActions;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.Constants;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communication
{
    /// <summary>
    /// Exercises the runtime behavior of <see cref="SmsActionCreateConnectionRequest"/>
    /// against the seeded sample database.
    /// </summary>
    /// <remarks>
    /// Each test sets up a transient SmsPipeline plus a single SmsAction configured to
    /// point at the same seeded Connection Opportunity, dispatches an inbound message,
    /// and asserts the resulting ConnectionRequest (or absence thereof).
    /// </remarks>
    [TestClass]
    public class SmsActionCreateConnectionRequestTests : DatabaseTestsBase
    {
        /// <summary>
        /// Seeded "Serve Children's Opportunity".
        /// </summary>
        /// <remarks>
        /// Has an IsDefault Status and is registered under an active Connection Type,
        /// so the action's required-fallback chains have real data to resolve against
        /// without a per-test setup.
        /// </remarks>
        private static readonly Guid OpportunityGuid = new Guid( "2C09211F-E19E-4685-9B67-4EDAA9945A97" );

        private const string TestFromNumber = "+15555550100";
        private const string TestToNumber = "+15555550200";

        #region Test Lifecycle

        /// <summary>
        /// Registers the Connection Type Settings field type and creates the component's
        /// attributes so the per-action ConnectionTypeSettings value persists and resolves.
        /// </summary>
        [TestInitialize]
        public void RegisterComponentAttributes()
        {
            /*
                6/3/26 - JMH

                The integration test database is built from EF migrations and sample
                data only; plugin hotfix migrations never run against it, so the field
                type that backs ConnectionTypeSettings is absent. Without it,
                SmsActionContainer cannot create the component's attribute and the
                per-action value never persists. Provision the field type and create the
                attributes here, mirroring SmsActionContainer.Refresh, so the value
                round-trips through the cache.

                Reason: Supply the ConnectionTypeSettings field type the test DB lacks.
            */
            var fieldTypeGuid = new Guid( Rock.SystemGuid.FieldType.CONNECTION_TYPE_SETTINGS );

            using ( var rockContext = new RockContext() )
            {
                var fieldTypeService = new FieldTypeService( rockContext );
                if ( fieldTypeService.Get( fieldTypeGuid ) == null )
                {
                    fieldTypeService.Add( new FieldType
                    {
                        Name = "Connection Type Settings",
                        Assembly = "Rock",
                        Class = "Rock.Field.Types.ConnectionTypeSettingsFieldType",
                        Guid = fieldTypeGuid,
                        IsSystem = true
                    } );
                    rockContext.SaveChanges();

                    FieldTypeCache.Clear();
                }
            }

            using ( var rockContext = new RockContext() )
            {
                var smsActionEntityTypeId = EntityTypeCache.Get( typeof( SmsAction ) ).Id;
                var componentEntityTypeId = EntityTypeCache.Get( typeof( SmsActionCreateConnectionRequest ) ).Id;

                Rock.Attribute.Helper.UpdateAttributes(
                    typeof( SmsActionCreateConnectionRequest ),
                    smsActionEntityTypeId,
                    "SmsActionComponentEntityTypeId",
                    componentEntityTypeId.ToString(),
                    rockContext );
            }
        }

        #endregion Test Lifecycle

        #region Tests

        [TestMethod]
        public void ProcessMessage_CreatesConnectionRequestForKnownPerson()
        {
            var personGuid = new Guid( TestGuids.TestPeople.TedDecker );
            var pipelineId = 0;
            var actionId = 0;
            var createdRequestId = 0;

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    pipelineId = CreateTestPipeline( rockContext );
                    actionId = CreateTestAction( rockContext, pipelineId, opportunityGuid: OpportunityGuid );
                }

                var fromPerson = ResolvePerson( personGuid );
                Assert.IsNotNull( fromPerson, "Ted Decker should be present in the seeded database." );

                var message = BuildMessage( fromPerson );
                var actionCache = SmsActionCache.Get( actionId );
                var component = new SmsActionCreateConnectionRequest();

                var response = component.ProcessMessage( actionCache, message, out var errorMessage );

                Assert.IsNull( response, "Create Connection Request should not produce a downstream response." );
                Assert.IsTrue( string.IsNullOrEmpty( errorMessage ), $"Expected no error, got: {errorMessage}" );

                createdRequestId = AssertConnectionRequestCreated( fromPerson, OpportunityGuid, out var request );
                Assert.AreEqual( ConnectionState.Active, request.ConnectionState, "Newly created request should be Active." );
                Assert.AreEqual( request.ConnectionOpportunity.ConnectionTypeId, request.ConnectionTypeId, "ConnectionTypeId should follow the opportunity's parent type." );
                Assert.IsTrue( request.ConnectionStatusId > 0, "Status should fall back to the type's default when none is configured." );
            }
            finally
            {
                CleanUp( createdRequestId, actionId, pipelineId );
            }
        }

        [TestMethod]
        public void ProcessMessage_CreatesConnectionRequestForNamelessPersonWhenAllowed()
        {
            var pipelineId = 0;
            var actionId = 0;
            var createdRequestId = 0;
            Person namelessPerson = null;

            try
            {
                namelessPerson = EnsureNamelessPerson( "+15555559101" );

                using ( var rockContext = new RockContext() )
                {
                    pipelineId = CreateTestPipeline( rockContext );
                    actionId = CreateTestAction( rockContext, pipelineId, opportunityGuid: OpportunityGuid, passNamelessPerson: true );
                }

                var message = BuildMessage( namelessPerson );
                var actionCache = SmsActionCache.Get( actionId );
                var component = new SmsActionCreateConnectionRequest();

                var response = component.ProcessMessage( actionCache, message, out var errorMessage );

                Assert.IsNull( response );
                Assert.IsTrue( string.IsNullOrEmpty( errorMessage ), $"Expected no error, got: {errorMessage}" );

                createdRequestId = AssertConnectionRequestCreated( namelessPerson, OpportunityGuid, out _ );
            }
            finally
            {
                CleanUp( createdRequestId, actionId, pipelineId );
                CleanUpNamelessPerson( namelessPerson?.Id );
            }
        }

        [TestMethod]
        public void ProcessMessage_SkipsNamelessPersonWhenDisallowed()
        {
            var pipelineId = 0;
            var actionId = 0;
            Person namelessPerson = null;

            try
            {
                namelessPerson = EnsureNamelessPerson( "+15555559102" );

                using ( var rockContext = new RockContext() )
                {
                    pipelineId = CreateTestPipeline( rockContext );
                    actionId = CreateTestAction( rockContext, pipelineId, opportunityGuid: OpportunityGuid, passNamelessPerson: false );
                }

                var message = BuildMessage( namelessPerson );
                var actionCache = SmsActionCache.Get( actionId );
                var component = new SmsActionCreateConnectionRequest();

                var response = component.ProcessMessage( actionCache, message, out var errorMessage );

                Assert.IsNull( response );
                Assert.IsFalse( string.IsNullOrEmpty( errorMessage ), "Expected a non-empty error message when PassNamelessPerson is false and the inbound is nameless." );

                AssertNoConnectionRequestCreated( namelessPerson, OpportunityGuid );
            }
            finally
            {
                CleanUp( connectionRequestId: 0, actionId, pipelineId );
                CleanUpNamelessPerson( namelessPerson?.Id );
            }
        }

        [TestMethod]
        public void ProcessMessage_SetsErrorWhenOpportunityIsUnresolved()
        {
            var personGuid = new Guid( TestGuids.TestPeople.TedDecker );
            var pipelineId = 0;
            var actionId = 0;

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    pipelineId = CreateTestPipeline( rockContext );
                    actionId = CreateTestAction( rockContext, pipelineId, opportunityGuid: Guid.NewGuid() );
                }

                var fromPerson = ResolvePerson( personGuid );
                var message = BuildMessage( fromPerson );
                var actionCache = SmsActionCache.Get( actionId );
                var component = new SmsActionCreateConnectionRequest();

                var response = component.ProcessMessage( actionCache, message, out var errorMessage );

                Assert.IsNull( response );
                Assert.IsFalse( string.IsNullOrEmpty( errorMessage ), "Expected an error message when the Opportunity guid does not resolve." );

                AssertNoConnectionRequestCreated( fromPerson, OpportunityGuid );
            }
            finally
            {
                CleanUp( connectionRequestId: 0, actionId, pipelineId );
            }
        }

        #endregion Tests

        #region Setup Helpers

        private static int CreateTestPipeline( RockContext rockContext )
        {
            var pipeline = new SmsPipeline
            {
                Name = $"AutoTest Pipeline {Guid.NewGuid():N}",
                IsActive = true
            };

            new SmsPipelineService( rockContext ).Add( pipeline );
            rockContext.SaveChanges();

            return pipeline.Id;
        }

        private static int CreateTestAction( RockContext rockContext, int pipelineId, Guid opportunityGuid, bool passNamelessPerson = true )
        {
            var componentEntityType = EntityTypeCache.Get( typeof( SmsActionCreateConnectionRequest ) );

            var action = new SmsAction
            {
                Name = "Connection Request",
                SmsPipelineId = pipelineId,
                SmsActionComponentEntityTypeId = componentEntityType.Id,
                IsActive = true,
                Order = 0
            };

            new SmsActionService( rockContext ).Add( action );
            rockContext.SaveChanges();

            action.LoadAttributes( rockContext );

            /*
                5/28/26 - JMH

                ConnectionTypeSettings persists as pipe-delimited GUIDs in fixed slot
                order: type|opportunity|status|source. Tests leave the rest empty so
                the runtime exercises its fallback chains for Status and source
                resolution.

                Reason: Confirm fallback chains run with only the required slot filled.
            */
            action.SetAttributeValue( "ConnectionTypeSettings", $"|{opportunityGuid}||" );
            action.SetAttributeValue( "PassNamelessPerson", passNamelessPerson.ToString() );
            action.SetAttributeValue( "CommentTemplate", "{{ Message }}" );
            action.SaveAttributeValues( rockContext );

            return action.Id;
        }

        private static Person ResolvePerson( Guid personGuid )
        {
            /*
                6/3/26 - JMH

                Eager-load Aliases so PrimaryAlias resolves after this context is
                disposed. ProcessMessage reads fromPerson.PrimaryAliasId, which lazy-
                loads the Aliases collection; the runtime pipeline keeps its context
                open for that read, but this setup helper does not.

                Reason: Prevent ObjectDisposedException when PrimaryAlias lazy-loads.
            */
            using ( var rockContext = new RockContext() )
            {
                return new PersonService( rockContext )
                    .Queryable( "Aliases" )
                    .FirstOrDefault( p => p.Guid == personGuid );
            }
        }

        private static Person EnsureNamelessPerson( string phoneNumber )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var existing = personService.GetPersonFromMobilePhoneNumber( phoneNumber, createNamelessPersonIfNotFound: true );

                if ( existing != null && !existing.IsNameless() )
                {
                    Assert.Fail( $"Test phone {phoneNumber} resolved to a named person; pick a different number." );
                }

                return existing;
            }
        }

        private static SmsMessage BuildMessage( Person fromPerson )
        {
            return new SmsMessage
            {
                FromNumber = TestFromNumber,
                ToNumber = TestToNumber,
                Message = "Sign me up",
                FromPerson = fromPerson
            };
        }

        #endregion Setup Helpers

        #region Assertion Helpers

        private static int AssertConnectionRequestCreated( Person fromPerson, Guid opportunityGuid, out ConnectionRequest request )
        {
            using ( var rockContext = new RockContext() )
            {
                request = new ConnectionRequestService( rockContext )
                    .Queryable( "ConnectionOpportunity" )
                    .Where( cr => cr.PersonAlias.PersonId == fromPerson.Id && cr.ConnectionOpportunity.Guid == opportunityGuid )
                    .OrderByDescending( cr => cr.CreatedDateTime )
                    .FirstOrDefault();

                Assert.IsNotNull( request, "Expected a ConnectionRequest to be created." );
                return request.Id;
            }
        }

        private static void AssertNoConnectionRequestCreated( Person fromPerson, Guid opportunityGuid )
        {
            if ( fromPerson == null )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var any = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Any( cr => cr.PersonAlias.PersonId == fromPerson.Id && cr.ConnectionOpportunity.Guid == opportunityGuid );

                Assert.IsFalse( any, "Expected no ConnectionRequest to be created." );
            }
        }

        #endregion Assertion Helpers

        #region Cleanup Helpers

        private static void CleanUp( int connectionRequestId, int actionId, int pipelineId )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( connectionRequestId > 0 )
                {
                    var requestService = new ConnectionRequestService( rockContext );
                    var request = requestService.Get( connectionRequestId );
                    if ( request != null )
                    {
                        requestService.Delete( request );
                    }
                }

                if ( actionId > 0 )
                {
                    var actionService = new SmsActionService( rockContext );
                    var action = actionService.Get( actionId );
                    if ( action != null )
                    {
                        actionService.Delete( action );
                    }
                }

                if ( pipelineId > 0 )
                {
                    var pipelineService = new SmsPipelineService( rockContext );
                    var pipeline = pipelineService.Get( pipelineId );
                    if ( pipeline != null )
                    {
                        pipelineService.Delete( pipeline );
                    }
                }

                rockContext.SaveChanges();
            }
        }

        private static void CleanUpNamelessPerson( int? personId )
        {
            /*
                5/28/26 - JMH

                Intentionally not deleting the nameless person record. Rock
                treats nameless persons as a permanent phone-number-to-record
                mapping; deleting them on test teardown would defeat the next
                run's GetPersonFromMobilePhoneNumber and could affect other
                tests sharing the same number range.

                Reason: Preserve cross-test stability of phone-number lookups.
            */
        }

        #endregion Cleanup Helpers
    }
}

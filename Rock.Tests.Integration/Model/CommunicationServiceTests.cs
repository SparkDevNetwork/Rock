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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class CommunicationServiceTests
    {
        private const int _expirationDays = 3;
        private const int _delayMinutes = 5;
        private const int _expectedRuntimeMax = 1000;

        [ClassInitialize]
        public static void TestInitialize( TestContext context )
        {
            CreateCommunicationsTestData( _expirationDays, _delayMinutes );
        }

        [TestMethod]
        [DataRow( false, false )]
        [DataRow( false, true )]
        [DataRow( true, false )]
        [DataRow( true, true )]
        public void GetQueuedShouldNeverIncludeSentCommunications( bool includeFuture, bool includePendingApproval )
        {
            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var stopwatch = Stopwatch.StartNew();
                var qry = communicationService
                    .GetQueued( _expirationDays, _delayMinutes, includeFuture, includePendingApproval )
                    .AsNoTracking()
                    .ToList();

                Assert.That.IsTrue( stopwatch.ElapsedMilliseconds < _expectedRuntimeMax, "Query took longer then a second to run." );
                Assert.That.IsFalse( qry.Any( c => c.SendDateKey.HasValue ), "Query returned sent communications." );
            }
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void GetQueuedShouldIncludeOnlyApprovedCommunications( bool includeFuture )
        {
            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var stopwatch = Stopwatch.StartNew();
                var qry = communicationService
                    .GetQueued( _expirationDays, _delayMinutes, includeFuture, false )
                    .AsNoTracking()
                    .ToList();

                Assert.That.IsFalse( qry.Any( c => c.Status != CommunicationStatus.Approved ), "Query returned unapproved communications." );
            }
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void GetQueuedShouldIncludeApprovedAndPendingApprovalCommunications( bool includeFuture )
        {
            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var stopwatch = Stopwatch.StartNew();
                var qry = communicationService
                    .GetQueued( _expirationDays, _delayMinutes, includeFuture, true )
                    .AsNoTracking()
                    .ToList();

                Assert.That.IsFalse( qry.Any( c => c.Status != CommunicationStatus.Approved && c.Status != CommunicationStatus.PendingApproval ), "Query returned unapproved communications." );
            }
        }

        [TestMethod]
        [DataRow( false, false )]
        [DataRow( false, true )]
        [DataRow( true, false )]
        [DataRow( true, true )]
        public void GetQueuedShouldIncludeCommunicationsWithListGroupIdsButNoRecipients( bool includeFuture, bool includePendingApproval )
        {
            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var stopwatch = Stopwatch.StartNew();
                var qry = communicationService
                    .GetQueued( _expirationDays, _delayMinutes, includeFuture, includePendingApproval )
                    .Include( c => c.Recipients )
                    .AsNoTracking()
                    .ToList();
                var communicationsWithNoRecipients = qry.Count( c => c.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Pending ) == 0 );

                Assert.That.NotEqual( 0, communicationsWithNoRecipients );
                Assert.That.AreEqual( communicationsWithNoRecipients,
                    qry.Count( c => c.ListGroupId.HasValue
                        && c.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Pending ) == 0 ),
                    "Query communications with no recipients or a list group id." );
            }
        }

        [TestMethod]
        [DataRow( false, false )]
        [DataRow( false, true )]
        [DataRow( true, false )]
        [DataRow( true, true )]
        public void GetQueuedShouldIncludeCommunicationsWithOnlyRecipientsAndListGroupIds( bool includeFuture, bool includePendingApproval )
        {
            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var stopwatch = Stopwatch.StartNew();
                var qry = communicationService
                    .GetQueued( _expirationDays, _delayMinutes, includeFuture, includePendingApproval )
                    .Include( c => c.Recipients )
                    .AsNoTracking()
                    .ToList();

                Assert.That.IsFalse( qry.Any( c => !c.ListGroupId.HasValue
                    && c.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Pending ) == 0 ),
                    "Query returned communications with no recipients." );
            }
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void GetQueuedShouldOnlyIncludeCommunicationsWithCorrectReviewedDate( bool includePending )
        {
            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var stopwatch = Stopwatch.StartNew();
                var qry = communicationService
                    .GetQueued( _expirationDays, _delayMinutes, false, includePending )
                    .AsNoTracking()
                    .ToList();

                var expectedBeginWindow = RockDateTime.Now.AddDays( 0 - _expirationDays );
                var expectedEndWindow = RockDateTime.Now.AddMinutes( 0 - _delayMinutes );
                Assert.That.IsFalse( qry.Any( c => c.ReviewedDateTime > expectedEndWindow && c.FutureSendDateTime == null ), "Query returned communications that are greater then the expected end window." );
                Assert.That.IsFalse( qry.Any( c => c.ReviewedDateTime < expectedBeginWindow && c.FutureSendDateTime == null ), "Query returned communications that are less then the expected begin window." );
                Assert.That.IsFalse( qry.Any( c => c.ReviewedDateTime == null && c.FutureSendDateTime == null ), "Query returned communications without a created date." );
                Assert.That.IsFalse( qry.Any( c => c.FutureSendDateTime > DateTime.Now ), "Query returned communications that are greater then the expected end window." );
                Assert.That.IsFalse( qry.Any( c => c.FutureSendDateTime < expectedBeginWindow ), "Query returned communications that are less then the expected begin window." );
            }
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void GetQueuedShouldOnlyIncludeCommunicationsWithCorrectReviewedDateAndFutureDate( bool includePending )
        {
            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var stopwatch = Stopwatch.StartNew();
                var qry = communicationService
                    .GetQueued( _expirationDays, _delayMinutes, true, includePending )
                    .AsNoTracking()
                    .ToList();

                var expectedBeginWindow = RockDateTime.Now.AddDays( 0 - _expirationDays );
                var expectedEndWindow = RockDateTime.Now.AddMinutes( 0 - _delayMinutes );
                Assert.That.IsFalse( qry.Any( c => c.ReviewedDateTime > expectedEndWindow && c.FutureSendDateTime == null ), "Query returned communications that are greater then the expected end window." );
                Assert.That.IsFalse( qry.Any( c => c.ReviewedDateTime < expectedBeginWindow && c.FutureSendDateTime == null ), "Query returned communications that are less then the expected begin window." );
                Assert.That.IsFalse( qry.Any( c => c.ReviewedDateTime == null && c.FutureSendDateTime == null ), "Query returned communications without a created date." );
                Assert.That.IsFalse( qry.Any( c => c.FutureSendDateTime < expectedBeginWindow ), "Query returned communications that were out of range." );
            }
        }

        private static void CreateCommunicationsTestData( int expirationDays, int delayMinutes )
        {
            var beginWindow = RockDateTime.Now.AddDays( 0 - expirationDays ).AddDays( 0 - 2 );
            var endWindow = RockDateTime.Now.AddMinutes( 0 - delayMinutes ).AddDays( 2 );
            var currentDateTime = beginWindow;
            var sender = GetNewPersonAlias();

            var communications = new List<Rock.Model.Communication>();
            communications.AddRange( CreateCommunication( sender, null, null ) );

            while ( beginWindow < endWindow )
            {
                communications.AddRange( CreateCommunication( sender, beginWindow, null ) );
                communications.AddRange( CreateCommunication( sender, beginWindow, beginWindow.AddDays( 2 ) ) );
                communications.AddRange( CreateCommunication( sender, null, beginWindow ) );

                beginWindow = beginWindow.AddDays( 1 );
            }

            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                communicationService.AddRange( communications );
                rockContext.SaveChanges();
            }
        }

        private static int GetNewPersonAlias()
        {
            using ( var rockContext = new RockContext() )
            {
                var personGuid = Guid.NewGuid();
                var person = new Person
                {
                    FirstName = $"First {personGuid}",
                    LastName = $"Last {personGuid}",
                    Email = $"{personGuid}@gmail.com"
                };
                var personService = new PersonService( rockContext );
                personService.Add( person );
                rockContext.SaveChanges();

                return person.PrimaryAliasId.Value;
            }
        }

        private static List<Rock.Model.Communication> CreateCommunication( int sender, DateTime? reviewedDateTime, DateTime? futureSendDateTime )
        {
            var communications = new List<Rock.Model.Communication>();

            foreach ( CommunicationStatus communicationStatus in Enum.GetValues( typeof( CommunicationStatus ) ) )
            {
                communications.AddRange( CreateCommunications( sender, communicationStatus, reviewedDateTime, futureSendDateTime, false, null ) );
                communications.AddRange( CreateCommunications( sender, communicationStatus, reviewedDateTime, futureSendDateTime, true, null ) );

                var listGroupId = GetListGroupId();
                communications.AddRange( CreateCommunications( sender, communicationStatus, reviewedDateTime, futureSendDateTime, false, listGroupId ) );
                communications.AddRange( CreateCommunications( sender, communicationStatus, reviewedDateTime, futureSendDateTime, true, listGroupId ) );
            }

            return communications;
        }

        private static int GetListGroupId()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupGuid = Guid.NewGuid();
                var group = new Group
                {
                    GroupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP ).Id,
                    Name = $"Name {groupGuid}",
                    IsActive = true,
                };
                var groupService = new GroupService( rockContext );
                groupService.Add( group );
                rockContext.SaveChanges();

                return group.Id;
            }
        }

        private static List<Rock.Model.Communication> CreateCommunications( int sender, CommunicationStatus communicationStatus, DateTime? reviewedDateTime, DateTime? futureSendDateTime, bool AddPendingRecipient, int? listGroupId )
        {

            // Create communication with no recipients.
            var communicationNoReciepients = new Rock.Model.Communication
            {
                Name = $"Test Communication {Guid.NewGuid()}",
                FutureSendDateTime = futureSendDateTime,
                ListGroupId = listGroupId,
                Message = $"Test Communication {Guid.NewGuid()}",
                Subject = $"Test Communication {Guid.NewGuid()}",
                FromEmail = "test@test.com",
                CommunicationType = CommunicationType.Email,
                SenderPersonAliasId = sender,
                IsBulkCommunication = false,
                Status = communicationStatus,
                ReviewedDateTime = reviewedDateTime
            };

            // Create communication with recipients.
            var communicationReciepients = new Rock.Model.Communication
            {
                Name = $"Test Communication {Guid.NewGuid()}",
                FutureSendDateTime = futureSendDateTime,
                ListGroupId = listGroupId,
                Message = $"Test Communication {Guid.NewGuid()}",
                Subject = $"Test Communication {Guid.NewGuid()}",
                FromEmail = "test@test.com",
                CommunicationType = CommunicationType.Email,
                SenderPersonAliasId = sender,
                IsBulkCommunication = false,
                Status = communicationStatus,
                ReviewedDateTime = reviewedDateTime
            };
            foreach ( CommunicationRecipientStatus communicationRecipientStatus in Enum.GetValues( typeof( CommunicationRecipientStatus ) ) )
            {
                if ( !AddPendingRecipient && communicationRecipientStatus == CommunicationRecipientStatus.Pending )
                {
                    continue;
                }
                communicationReciepients.Recipients.Add( new CommunicationRecipient
                {
                    Status = communicationRecipientStatus,
                    PersonAliasId = GetNewPersonAlias(),
                    MediumEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL )
                } );
            }

            return new List<Rock.Model.Communication>
            {
                communicationNoReciepients,
                communicationReciepients
            };
        }
    }
}

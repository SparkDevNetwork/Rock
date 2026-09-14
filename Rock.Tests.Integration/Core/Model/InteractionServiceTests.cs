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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Integration.Core.Model
{
    /// <summary>
    /// Integration tests for <see cref="InteractionService"/> behavior added by
    /// the PersonSession feature: the <c>GetInteractionSessionId</c> upsert that
    /// stamps (and later adopts) <see cref="InteractionSession.PersonSessionId"/>.
    /// </summary>
    [TestClass]
    public class InteractionServiceTests : DatabaseTestsBase
    {
        /// <summary>
        /// First-time browser-session presented by an authenticated request
        /// inserts a brand-new <see cref="InteractionSession"/> row with
        /// <see cref="InteractionSession.PersonSessionId"/> already populated.
        /// Covers the "stamp at creation" path from the spec's
        /// InteractionSession sync table.
        /// </summary>
        [TestMethod]
        [IsolatedTestDatabase]
        public void GetInteractionSessionId_InsertPath_StampsPersonSessionIdAtInsert()
        {
            using var rockContext = RockApp.Current.CreateRockContext();
            var personSession = CreatePersistedComponentPersonSession( rockContext );
            var browserSessionId = Guid.NewGuid();

            var interactionService = new InteractionService( rockContext );
            var interactionSessionId = interactionService.GetInteractionSessionId(
                browserSessionId,
                ipAddress: "127.0.0.1",
                interactionDeviceTypeId: null,
                interactionDateKey: null,
                interactionSessionLocationId: null,
                personSessionId: personSession.Id );

            var row = new InteractionSessionService( rockContext ).Get( interactionSessionId );
            Assert.IsNotNull( row, "Insert path should have created an InteractionSession row." );
            Assert.AreEqual( browserSessionId, row.Guid );
            Assert.AreEqual( personSession.Id, row.PersonSessionId,
                "Newly inserted InteractionSession row should carry the supplied PersonSessionId." );
        }

        /// <summary>
        /// Anonymous browser-session row whose
        /// <see cref="InteractionSession.PersonSessionId"/> is null gets its
        /// <c>PersonSessionId</c> set when an authenticated request later
        /// presents the same <c>RockSessionId</c>. Covers the "adopt by update
        /// at login" and "adopt by update at legacy upgrade" paths.
        /// </summary>
        [TestMethod]
        [IsolatedTestDatabase]
        public void GetInteractionSessionId_UpdatePath_AdoptsExistingRowWhenPersonSessionIdNull()
        {
            using var rockContext = RockApp.Current.CreateRockContext();
            var personSession = CreatePersistedComponentPersonSession( rockContext );
            var browserSessionId = Guid.NewGuid();

            var interactionService = new InteractionService( rockContext );

            // Anonymous interaction creates the row with PersonSessionId = null.
            var firstId = interactionService.GetInteractionSessionId(
                browserSessionId,
                ipAddress: "127.0.0.1",
                interactionDeviceTypeId: null,
                interactionDateKey: null,
                interactionSessionLocationId: null,
                personSessionId: null );

            // Authenticated request presents the same RockSessionId.
            var secondId = interactionService.GetInteractionSessionId(
                browserSessionId,
                ipAddress: "127.0.0.1",
                interactionDeviceTypeId: null,
                interactionDateKey: null,
                interactionSessionLocationId: null,
                personSessionId: personSession.Id );

            Assert.AreEqual( firstId, secondId, "Same RockSessionId should resolve to the same InteractionSession row." );

            // Force a re-read so we observe the UPDATE that ran on a separate
            // ADO.NET connection inside SqlQuery<int>.
            using ( var verifyContext = RockApp.Current.CreateRockContext() )
            {
                var row = new InteractionSessionService( verifyContext ).Get( firstId );
                Assert.AreEqual( personSession.Id, row.PersonSessionId,
                    "Existing row should have been adopted by the authenticated request." );
            }
        }

        /// <summary>
        /// The adopt-by-update path only touches the row whose
        /// <c>Guid</c> matches the presented <c>browserSessionId</c>; other
        /// <see cref="InteractionSession"/> rows in the table are untouched.
        /// Regression guard against an UPDATE missing its WHERE clause.
        /// </summary>
        [TestMethod]
        [IsolatedTestDatabase]
        public void GetInteractionSessionId_UpdatePath_DoesNotTouchOtherRows()
        {
            using var rockContext = RockApp.Current.CreateRockContext();
            var personSession = CreatePersistedComponentPersonSession( rockContext );

            var interactionService = new InteractionService( rockContext );

            var targetBrowserSessionId = Guid.NewGuid();
            var bystanderBrowserSessionId = Guid.NewGuid();

            var targetId = interactionService.GetInteractionSessionId(
                targetBrowserSessionId,
                ipAddress: "127.0.0.1",
                interactionDeviceTypeId: null,
                personSessionId: null );

            var bystanderId = interactionService.GetInteractionSessionId(
                bystanderBrowserSessionId,
                ipAddress: "127.0.0.2",
                interactionDeviceTypeId: null,
                personSessionId: null );

            // Adopt the target row.
            var resolvedTargetId = interactionService.GetInteractionSessionId(
                targetBrowserSessionId,
                ipAddress: "127.0.0.1",
                interactionDeviceTypeId: null,
                personSessionId: personSession.Id );

            Assert.AreEqual( targetId, resolvedTargetId );

            using ( var verifyContext = RockApp.Current.CreateRockContext() )
            {
                var sessionService = new InteractionSessionService( verifyContext );
                var target = sessionService.Get( targetId );
                var bystander = sessionService.Get( bystanderId );

                Assert.AreEqual( personSession.Id, target.PersonSessionId,
                    "Target row should reflect the adoption." );
                Assert.IsNull( bystander.PersonSessionId,
                    "Bystander row should not have been touched by the UPDATE." );
            }
        }

        /// <summary>
        /// Creates a <see cref="PersonSessionCreationSource.Component"/>
        /// <see cref="PersonSession"/> row for Ted Decker and persists it. Used
        /// by the upsert tests to get a valid <c>PersonSession.Id</c> the SQL
        /// path can FK against.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/> to persist into.</param>
        /// <returns>The persisted <see cref="PersonSession"/>.</returns>
        private static PersonSession CreatePersistedComponentPersonSession( RockContext rockContext )
        {
            var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            Assert.IsNotNull( tedDecker?.PrimaryAliasId, "Ted Decker test data with a primary alias is required for this test." );

            var userLogin = new UserLoginService( rockContext )
                .Queryable()
                .FirstOrDefault( ul => ul.PersonId == tedDecker.Id );
            Assert.IsNotNull( userLogin, "Ted Decker must have at least one UserLogin in the seed data." );

            var anyEntityTypeId = new EntityTypeService( rockContext )
                .Queryable()
                .Select( et => et.Id )
                .First();

            var session = new PersonSessionService( rockContext )
                .StartComponentSession(
                    requestContext: null,
                    personAliasId: tedDecker.PrimaryAliasId.Value,
                    userLoginId: userLogin.Id,
                    authComponentEntityTypeId: anyEntityTypeId,
                    isPersistent: false );

            rockContext.Set<PersonSession>().Add( session );
            rockContext.SaveChanges();

            return session;
        }
    }
}

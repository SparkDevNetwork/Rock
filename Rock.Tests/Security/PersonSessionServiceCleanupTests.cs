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
using Rock.Enums.Security;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Security;

/// <summary>
/// Mocked-database tests for
/// <see cref="PersonSessionService.MarkExpiredSessionsInactive(int, int)"/>, the
/// seam the Rock Cleanup job calls. The method creates its own contexts via
/// <c>RockApp.Current.CreateRockContext()</c>, which the mock scope routes to the
/// shared in-memory context, so the batched loop is exercised end-to-end here.
/// </summary>
/// <remarks>
/// These tests assert which rows are selected and that <c>IsActive</c> flips, but
/// not that <c>InactiveDateTime</c> is stamped: that stamp comes from the
/// <c>PersonSession</c> save hook, which only runs against a real database. The
/// stamp is covered by the full-integration tests in
/// <c>Rock.Tests.Integration.Security.PersonSessionTests</c>, and the cleanup
/// drives the same <c>IsActive = false</c> + <c>SaveChanges</c> path.
/// </remarks>
[TestClass]
public class PersonSessionServiceCleanupTests
{
    /// <summary>
    /// Only active sessions whose <see cref="PersonSession.ExpiresDateTime"/> is
    /// in the past are marked inactive. Not-yet-expired sessions and open-ended
    /// sessions (no expiration) are left active.
    /// </summary>
    [TestMethod]
    public void MarkExpiredSessionsInactive_MarksOnlyExpiredActiveSessions()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var expiredGuid = Guid.NewGuid();
        var futureGuid = Guid.NewGuid();
        var openEndedGuid = Guid.NewGuid();

        // Active and already past its expiration: should be marked inactive.
        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 1,
            Guid = expiredGuid,
            PersonAliasId = 100,
            IsActive = true,
            ExpiresDateTime = RockDateTime.Now.AddDays( -1 ),
            CreationSource = PersonSessionCreationSource.Component,
        } );

        // Active but not yet expired: should be left active.
        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 2,
            Guid = futureGuid,
            PersonAliasId = 100,
            IsActive = true,
            ExpiresDateTime = RockDateTime.Now.AddDays( 10 ),
            CreationSource = PersonSessionCreationSource.Component,
        } );

        // Active with no expiration (open-ended): should be left active.
        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 3,
            Guid = openEndedGuid,
            PersonAliasId = 100,
            IsActive = true,
            ExpiresDateTime = null,
            CreationSource = PersonSessionCreationSource.Component,
        } );

        rockContext.SaveChanges();

        var recordsUpdated = PersonSessionService.MarkExpiredSessionsInactive( batchSize: 1000, commandTimeout: 30 );

        Assert.AreEqual( 1, recordsUpdated, "Only the single expired active session should be processed." );

        var sessions = new PersonSessionService( rockContext ).Queryable().ToList();
        Assert.IsFalse( sessions.First( s => s.Guid == expiredGuid ).IsActive, "Expired session should be marked inactive." );
        Assert.IsTrue( sessions.First( s => s.Guid == futureGuid ).IsActive, "Not-yet-expired session should remain active." );
        Assert.IsTrue( sessions.First( s => s.Guid == openEndedGuid ).IsActive, "Open-ended session (no ExpiresDateTime) should remain active." );
    }

    /// <summary>
    /// An already-inactive expired session is not re-processed (the filter
    /// requires <c>IsActive == true</c>), so the run reports zero rows updated.
    /// </summary>
    [TestMethod]
    public void MarkExpiredSessionsInactive_IgnoresAlreadyInactiveSessions()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            IsActive = false,
            ExpiresDateTime = RockDateTime.Now.AddDays( -1 ),
            CreationSource = PersonSessionCreationSource.Component,
        } );

        rockContext.SaveChanges();

        var recordsUpdated = PersonSessionService.MarkExpiredSessionsInactive( batchSize: 1000, commandTimeout: 30 );

        Assert.AreEqual( 0, recordsUpdated );
    }
}

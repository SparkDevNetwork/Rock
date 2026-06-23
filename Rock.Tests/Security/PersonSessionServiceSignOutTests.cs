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
using Rock.Net;
using Rock.Tests.Shared.TestFramework;
using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Security;

/// <summary>
/// Mocked-database tests for <see cref="PersonSessionService.SignOut"/>, the
/// single seam every logout path migrates onto. The tests verify
/// the post-condition "this request is anonymous": the current session is
/// marked inactive, the <c>.ROCK</c> cookie is expired, and the session is
/// detached from the request context.
/// </summary>
/// <remarks>
/// These tests assert <c>IsActive == false</c> but intentionally do NOT assert
/// <c>InactiveDateTime != null</c>. That stamp is applied by
/// <c>PersonSession.SaveHook</c>, which runs only against a real database — the
/// mocked <c>RockContext</c> intercepts <c>SaveChanges</c> without executing
/// entity save hooks. The <c>InactiveDateTime</c> stamp is covered by the
/// Phase 1 full-integration invariant (any <c>IsActive = false</c> +
/// <c>SaveChanges</c> stamps it), and <c>SignOut</c> drives exactly that path.
/// </remarks>
[TestClass]
public class PersonSessionServiceSignOutTests
{
    /// <summary>
    /// Signing out an authenticated request marks the current
    /// <see cref="PersonSession"/> inactive, expires the <c>.ROCK</c> cookie
    /// via the response, and clears the session from the request context so
    /// the remainder of the request observes the anonymous state.
    /// </summary>
    [TestMethod]
    public void SignOut_MarksSessionInactiveAndClearsCookieAndContext()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var session = new PersonSession
        {
            Id = 11,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( session );

        var response = new TrackingResponseContext();
        var requestContext = new RockRequestContext( response );
        requestContext.SetPersonSession( session );

        var service = new PersonSessionService( rockContext );
        service.SignOut( requestContext );

        Assert.IsFalse( session.IsActive, "The current session should be marked inactive on sign-out." );
        Assert.HasCount( 1, response.RemovedCookies );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.RemovedCookies[0].Name );
        Assert.IsNull( requestContext.PersonSession, "The session should be detached from the request context on sign-out." );
    }

    /// <summary>
    /// Signing out a request that has no current <see cref="PersonSession"/>
    /// (already anonymous) is a silent no-op: no cookie is removed and the
    /// context stays anonymous. Defensive regression against the seam doing
    /// work when there is nothing to sign out.
    /// </summary>
    [TestMethod]
    public void SignOut_IsNoOp_WhenNoPersonSessionIsAttached()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var response = new TrackingResponseContext();
        var requestContext = new RockRequestContext( response );

        var service = new PersonSessionService( rockContext );
        service.SignOut( requestContext );

        Assert.IsEmpty( response.RemovedCookies, "No cookie should be removed when there is no session to sign out." );
        Assert.IsNull( requestContext.PersonSession );
    }
}

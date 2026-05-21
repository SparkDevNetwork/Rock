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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Security;

/// <summary>
/// Mocked-database unit tests for <see cref="PersonSessionService"/>. Tests
/// that need real save semantics (PreSave hook, raw-SQL upsert, etc.) live in
/// <c>Rock.Tests.Integration.Security.PersonSessionTests</c>.
/// </summary>
[TestClass]
public class PersonSessionServiceTests
{
    /// <summary>
    /// An admin-impersonation session whose
    /// <see cref="PersonSessionAdminImpersonationSettings.ImpersonatorPersonSessionGuid"/>
    /// resolves to a real <see cref="PersonSession"/> returns that prior
    /// session.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsPriorSession_ForValidImpersonationRestoreReference()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var impersonator = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( impersonator );

        var impersonationSession = new PersonSession
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };
        impersonationSession.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = impersonator.Guid,
            ImpersonatorInteractionSessionGuid = Guid.NewGuid(),
        } );

        var service = new PersonSessionService( rockContext );

        var restored = service.GetImpersonatorSession( impersonationSession );

        Assert.IsNotNull( restored );
        Assert.AreEqual( impersonator.Guid, restored.Guid );
    }

    /// <summary>
    /// A <see cref="PersonSessionCreationSource.UserToken"/> session never has
    /// a restorable impersonator (the recipient is the legitimate owner of the
    /// data, not an admin), so <c>GetImpersonatorSession</c> returns null even
    /// when the session was incorrectly stamped with admin-impersonation
    /// settings.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsNull_ForUserTokenSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };
        session.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = Guid.NewGuid(),
            ImpersonatorInteractionSessionGuid = Guid.NewGuid(),
        } );

        var service = new PersonSessionService( rockContext );

        Assert.IsNull( service.GetImpersonatorSession( session ) );
    }

    /// <summary>
    /// A <see cref="PersonSessionCreationSource.Component"/> session has no
    /// impersonator and returns null.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsNull_ForComponentSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };

        var service = new PersonSessionService( rockContext );

        Assert.IsNull( service.GetImpersonatorSession( session ) );
    }

    /// <summary>
    /// An impersonation session whose stamped
    /// <see cref="PersonSessionAdminImpersonationSettings.ImpersonatorPersonSessionGuid"/>
    /// does not resolve to any <see cref="PersonSession"/> row (deleted, or
    /// never existed) returns null. This is the dangling-reference branch
    /// — the impersonation does not silently continue.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsNull_WhenImpersonatorReferenceIsDangling()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var impersonationSession = new PersonSession
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };
        impersonationSession.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = Guid.NewGuid(), // not seeded
            ImpersonatorInteractionSessionGuid = Guid.NewGuid(),
        } );

        var service = new PersonSessionService( rockContext );

        Assert.IsNull( service.GetImpersonatorSession( impersonationSession ) );
    }

    /// <summary>
    /// <c>GetImpersonatorSession( null )</c> returns null without throwing.
    /// Defensive guard; the spec's strength mapping says "no PersonSession"
    /// is conceptual, but a service that takes a parameter must still tolerate
    /// the caller passing null.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsNull_WhenSessionArgumentIsNull()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        Assert.IsNull( service.GetImpersonatorSession( null ) );
    }
}

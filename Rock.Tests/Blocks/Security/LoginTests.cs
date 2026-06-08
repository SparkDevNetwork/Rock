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

using Rock.Blocks.Security;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Security.Authentication;
using Rock.Tests.Shared.TestFramework;
using Rock.Tests.Shared.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Blocks.Security;

/// <summary>
/// Mocked-database tests for <see cref="Login.Authenticate"/> — the
/// post-credential-validation handoff that creates a
/// <see cref="PersonSessionCreationSource.Component"/>-source
/// <see cref="PersonSession"/>, writes the new-format <c>.ROCK</c>
/// cookie, and stamps the session onto the current
/// <see cref="RockRequestContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// Tests instantiate a real <see cref="Login"/> block (with the minimum
/// <c>BlockCache</c> / <c>PageCache</c> scaffolding it needs to
/// function) and call the now-<c>internal</c> <c>Authenticate</c>
/// helper directly. Each test seeds the <see cref="UserLogin"/> +
/// <see cref="Person"/> + <see cref="PersonAlias"/> graph manually
/// because the mocked <see cref="RockContext"/> does not auto-load
/// Includes; the authentication component is the real
/// <see cref="Database"/> instance pulled from
/// <see cref="AuthenticationContainer"/>.
/// </para>
/// <para>
/// Cookie writes are observed by passing a
/// <see cref="TrackingResponseContext"/> (from
/// <c>Rock.Tests.Shared.Utility</c>) as the response context; tests
/// that do not care about cookie writes use
/// <see cref="NullRockResponseContext"/>.
/// </para>
/// </remarks>
[TestClass]
public class LoginTests
{
    /// <summary>
    /// Standard credential login (2FA not required) creates a
    /// <see cref="PersonSession"/> with
    /// <see cref="PersonSessionCreationSource.Component"/>,
    /// <c>LastStepUpAuthenticationDateTime</c> stamped to now, and
    /// <c>LastMultiFactorAuthenticationDateTime</c> left null. Also pins
    /// the session to the authenticating <see cref="UserLogin"/> and
    /// records which <see cref="AuthenticationComponent"/> handled the
    /// request.
    /// </summary>
    [TestMethod]
    public void Authenticate_NonMfa_CreatesComponentSession_WithStepUpRecencyOnly()
    {
        // Arrange.
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );
        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1, primaryAliasId: 1 );
        var block = BuildLoginBlock( rockContext, new NullRockResponseContext() );

        // Act.
        block.Authenticate( userLogin: userLogin,
            authComponent: authComponent,
            isPersisted: false,
            isTwoFactorAuthenticated: false );

        // Assert.
        var session = rockContext.Set<PersonSession>().Single();
        Assert.AreEqual( PersonSessionCreationSource.Component, session.CreationSource );
        Assert.IsNotNull( session.LastStepUpAuthenticationDateTime );
        Assert.IsNull( session.LastMultiFactorAuthenticationDateTime );
        Assert.AreEqual( userLogin.Id, session.UserLoginId );
        Assert.AreEqual( authComponent.TypeId, session.AuthenticationComponentId );
    }

    /// <summary>
    /// A login that completes MFA (passes
    /// <c>isTwoFactorAuthenticated: true</c>) ALSO stamps
    /// <c>LastMultiFactorAuthenticationDateTime</c>. Step-up recency is
    /// always stamped regardless of the MFA flag.
    /// </summary>
    [TestMethod]
    public void Authenticate_Mfa_CreatesComponentSession_WithBothRecencyStamps()
    {
        // Arrange.
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );
        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1, primaryAliasId: 1 );
        var block = BuildLoginBlock( rockContext, new NullRockResponseContext() );

        // Act.
        block.Authenticate( userLogin: userLogin,
            authComponent: authComponent,
            isPersisted: true,
            isTwoFactorAuthenticated: true );

        // Assert.
        var session = rockContext.Set<PersonSession>().Single();
        Assert.AreEqual( PersonSessionCreationSource.Component, session.CreationSource );
        Assert.IsNotNull( session.LastStepUpAuthenticationDateTime );
        Assert.IsNotNull( session.LastMultiFactorAuthenticationDateTime );
    }

    /// <summary>
    /// Passing <c>isPersisted: true</c> stamps the new session's
    /// <see cref="PersonSession.IsPersistent"/> flag, which is what
    /// controls the cookie's browser-side <c>Expires</c> attribute under
    /// the <c>MIN( ExpiresDateTime ?? MaxValue, Now + Timeout )</c>
    /// formula in <see cref="PersonSessionService.SetAuthCookie"/>.
    /// </summary>
    [TestMethod]
    public void Authenticate_PersistedFlag_StampsIsPersistentOnSession()
    {
        // Arrange.
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );
        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1, primaryAliasId: 1 );
        var block = BuildLoginBlock( rockContext, new NullRockResponseContext() );

        // Act.
        block.Authenticate( userLogin: userLogin,
            authComponent: authComponent,
            isPersisted: true,
            isTwoFactorAuthenticated: false );

        // Assert.
        Assert.IsTrue( rockContext.Set<PersonSession>().Single().IsPersistent );
    }

    /// <summary>
    /// When the caller supplies <c>expiresIn</c> (the passwordless and
    /// MFA-satisfied credential paths do), the new session's
    /// <see cref="PersonSession.ExpiresDateTime"/> is stamped to
    /// <c>Now + expiresIn</c>. This is what passwordless's
    /// <c>PasswordlessSignInSessionDuration</c> setting feeds into.
    /// </summary>
    [TestMethod]
    public void Authenticate_WithExpiresIn_StampsExpiresDateTime()
    {
        // Arrange.
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );
        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1, primaryAliasId: 1 );
        var block = BuildLoginBlock( rockContext, new NullRockResponseContext() );
        var expiresIn = TimeSpan.FromMinutes( 30 );
        var expectedFloor = RockDateTime.Now.Add( expiresIn );

        // Act.
        block.Authenticate( userLogin: userLogin,
            authComponent: authComponent,
            isPersisted: true,
            isTwoFactorAuthenticated: false,
            expiresIn: expiresIn );

        // Assert.
        var session = rockContext.Set<PersonSession>().Single();
        Assert.IsTrue( session.ExpiresDateTime.HasValue );
        Assert.IsTrue( ( session.ExpiresDateTime.Value - expectedFloor ).Duration() < TimeSpan.FromSeconds( 5 ) );
    }

    /// <summary>
    /// After <c>Authenticate</c> returns, the newly-saved session is
    /// stashed on <see cref="RockRequestContext.PersonSession"/> so any
    /// downstream code in the same request (a redirect handler, an
    /// inline page render) sees the just-authenticated session without
    /// having to re-resolve from the cookie.
    /// </summary>
    [TestMethod]
    public void Authenticate_AttachesNewSession_ToRequestContext()
    {
        // Arrange.
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );
        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1, primaryAliasId: 1 );
        var block = BuildLoginBlock( rockContext, new NullRockResponseContext() );

        // Act.
        block.Authenticate( userLogin: userLogin,
            authComponent: authComponent,
            isPersisted: false,
            isTwoFactorAuthenticated: false );

        // Assert.
        var dbSession = rockContext.Set<PersonSession>().Single();
        Assert.IsNotNull( block.RequestContext.PersonSession );
        Assert.AreEqual( dbSession.Guid, block.RequestContext.PersonSession.Guid );
    }

    /// <summary>
    /// The <c>.ROCK</c> auth cookie is written to the response via the
    /// shared <see cref="PersonSessionService.SetAuthCookie"/> path, not
    /// the legacy <c>Authorization.SetAuthCookie</c>. Test asserts a
    /// new-format cookie was added to the response (decoding the cookie value
    /// is covered by other tests; this test only verifies the wiring fires).
    /// </summary>
    [TestMethod]
    public void Authenticate_WritesAuthCookie_ViaPersonSessionService()
    {
        // Arrange.
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );
        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1, primaryAliasId: 1 );
        var response = new TrackingResponseContext();
        var block = BuildLoginBlock( rockContext, response );

        // Act.
        block.Authenticate( userLogin: userLogin,
            authComponent: authComponent,
            isPersisted: false,
            isTwoFactorAuthenticated: false );

        // Assert.
        Assert.HasCount( 1, response.AddedCookies );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.AddedCookies[0].Name );
        Assert.IsFalse( string.IsNullOrEmpty( response.AddedCookies[0].Value ) );
    }

    /// <summary>
    /// Authenticating a <see cref="UserLogin"/> whose <see cref="Person"/>
    /// has no primary alias throws (rather than silently producing an
    /// orphaned session with <c>PersonAliasId = 0</c>). Guards against a
    /// data-shape regression in the upstream registration flow.
    /// </summary>
    [TestMethod]
    public void Authenticate_ThrowsWhenPersonHasNoPrimaryAlias()
    {
        // Arrange.
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );
        var userLogin = SeedUserLoginWithoutPrimaryAlias( rockContext, authComponent, userLoginId: 1, userName: "orphan", personId: 1 );
        var block = BuildLoginBlock( rockContext, new NullRockResponseContext() );

        // Act + Assert.
        Assert.ThrowsExactly<InvalidOperationException>( () =>
            block.Authenticate( userLogin: userLogin,
                authComponent: authComponent,
                isPersisted: false,
                isTwoFactorAuthenticated: false ) );
    }

    #region Test infrastructure

    /// <summary>
    /// Constructs a <see cref="Login"/> block instance ready to call
    /// <c>Authenticate</c>. Seeds the minimum
    /// <see cref="Page"/> / <see cref="BlockType"/> / <see cref="Block"/>
    /// rows the block's <c>BlockCache</c> and <c>PageCache</c> properties
    /// need, then wires <see cref="Login.RequestContext"/> to a synthetic
    /// <see cref="RockRequestContext"/> backed by <paramref name="response"/>.
    /// Pass a <see cref="TrackingResponseContext"/> to observe cookie
    /// writes; pass <see cref="NullRockResponseContext"/> otherwise.
    /// </summary>
    private static Login BuildLoginBlock( RockContext rockContext, IRockResponseContext response )
    {
        var page = new Page
        {
            Id = 1,
        };

        var blockTypeEntityType = EntityTypeCache.Get<Login>( true, rockContext );

        var blockType = new BlockType
        {
            Id = 1,
            Name = "Login",
            EntityTypeId = blockTypeEntityType.Id,
        };

        var block = new Block
        {
            Id = 1,
            BlockTypeId = blockType.Id,
            PageId = page.Id,
        };

        rockContext.Set<Page>().Add( page );
        rockContext.Set<BlockType>().Add( blockType );
        rockContext.Set<Block>().Add( block );

        return new Login
        {
            RockContext = rockContext,
            RequestContext = new RockRequestContext( response ),
            PageCache = PageCache.Get( page.Id, rockContext ),
            BlockCache = BlockCache.Get( block.Id, rockContext ),
        };
    }

    /// <summary>
    /// Seeds a <see cref="UserLogin"/> + <see cref="Person"/> +
    /// <see cref="PersonAlias"/> graph into the mocked
    /// <see cref="RockContext"/>, with the <c>UserLogin</c> pointed at
    /// <paramref name="authComponent"/>'s <see cref="EntityType"/> and
    /// the <c>Person</c>'s primary-alias FK wired up. Navigation
    /// properties are set manually because the mocked EF context does
    /// not auto-load <c>Include</c>.
    /// </summary>
    private static UserLogin SeedUserLogin( RockContext rockContext, AuthenticationComponent authComponent, int userLoginId, string userName, int personId, int primaryAliasId )
    {
        var componentEntityType = EntityTypeCache.Get( authComponent.GetType().FullName, true, rockContext );

        var person = new Person
        {
            Id = personId,
            FirstName = "Ted",
            LastName = "Decker",
        };

        var personAlias = new PersonAlias
        {
            Id = primaryAliasId,
            PersonId = person.Id,
        };
        person.PrimaryAliasId = personAlias.Id;

        var userLogin = new UserLogin
        {
            Id = userLoginId,
            UserName = userName,
            EntityType = rockContext.Set<EntityType>().Single( et => et.Id == componentEntityType.Id ),
            EntityTypeId = componentEntityType.Id,
            PersonId = person.Id,
            Person = person,
        };

        rockContext.Set<Person>().Add( person );
        rockContext.Set<PersonAlias>().Add( personAlias );
        rockContext.Set<UserLogin>().Add( userLogin );

        return userLogin;
    }

    /// <summary>
    /// Variant of <see cref="SeedUserLogin"/> that omits the
    /// <see cref="PersonAlias"/> row and leaves
    /// <see cref="Person.PrimaryAliasId"/> null, so
    /// <c>Authenticate</c> hits the missing-alias branch of its
    /// resolver.
    /// </summary>
    private static UserLogin SeedUserLoginWithoutPrimaryAlias( RockContext rockContext, AuthenticationComponent authComponent, int userLoginId, string userName, int personId )
    {
        var componentEntityType = EntityTypeCache.Get( authComponent.GetType().FullName, true, rockContext );

        var person = new Person
        {
            Id = personId,
            FirstName = "Orphan",
            LastName = "Person",
            // No PrimaryAliasId / PersonAlias - this is the case under test.
        };

        var userLogin = new UserLogin
        {
            Id = userLoginId,
            UserName = userName,
            EntityType = rockContext.Set<EntityType>().Single( et => et.Id == componentEntityType.Id ),
            EntityTypeId = componentEntityType.Id,
            PersonId = person.Id,
            Person = person,
        };

        rockContext.Set<Person>().Add( person );
        rockContext.Set<UserLogin>().Add( userLogin );

        return userLogin;
    }

    /// <summary>
    /// Returns the real <see cref="Database"/> auth-component instance
    /// from the <see cref="AuthenticationContainer"/>, with its
    /// component attributes loaded and <c>Active</c> set to <c>true</c>.
    /// This is the standard username/password component; it does not
    /// declare 2FA support, so tests that need the MFA-recency stamp
    /// drive it by passing <c>isTwoFactorAuthenticated: true</c> rather
    /// than swapping in a different component.
    /// </summary>
    private static AuthenticationComponent GetDatabaseAuthComponent( RockContext rockContext )
    {
        var container = AuthenticationContainer.Instance;

        // Work around a bug in the way component attributes are initialized
        // since we don't have save hooks to clear the cache.
        EntityTypeAttributesCache.Clear();

        var component = container.Components.Values.Select( v => v.Value ).OfType<Database>().FirstOrDefault();

        component.LoadAttributes( rockContext );
        component.SetAttributeValue( "Active", true.ToString() );
        component.SaveAttributeValues( rockContext );

        return component;
    }

    #endregion Test infrastructure
}

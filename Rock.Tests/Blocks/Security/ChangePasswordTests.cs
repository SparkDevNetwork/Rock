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
/// Mocked-database tests for <see cref="ChangePassword"/>: the block must gate
/// access on the current request
/// having a non-impersonated <see cref="PersonSession"/>, replacing the
/// legacy <c>UserLogin.IsAuthenticated</c> read (which historically returned
/// false for impersonated forms tickets).
/// </summary>
/// <remarks>
/// The block's "allow" gate is satisfied when <c>RequestContext.CurrentUser</c>
/// is set AND <c>RequestContext.PersonSession?.IsImpersonated()</c> is false.
/// Tests exercise each branch directly so the assertion can be a positive read
/// of <see cref="Rock.ViewModels.Blocks.Security.ChangePassword.ChangePasswordBag.IsChangePasswordVisible"/>
/// rather than a fragile string match against an alert message.
/// </remarks>
[TestClass]
public class ChangePasswordTests
{
    /// <summary>
    /// A request without a current user is gated out: the box is returned
    /// with <c>IsChangePasswordVisible = false</c>. Mirrors an anonymous
    /// request hitting the block.
    /// </summary>
    [TestMethod]
    public void GetObsidianBlockInitialization_HidesPasswordChange_WhenNoCurrentUser()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var block = BuildBlock( rockContext, currentUser: null, session: null );

        var box = ( Rock.ViewModels.Blocks.Security.ChangePassword.ChangePasswordBag ) block.GetObsidianBlockInitialization();

        Assert.IsFalse( box.IsChangePasswordVisible );
    }

    /// <summary>
    /// A request whose current <see cref="PersonSession"/> was created via
    /// admin <see cref="PersonSessionCreationSource.Impersonation"/> is gated
    /// out: an admin acting as another user must not be able to change that
    /// user's password.
    /// </summary>
    [TestMethod]
    public void GetObsidianBlockInitialization_HidesPasswordChange_WhenSessionIsImpersonation()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );

        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1 );
        var session = BuildSession( PersonSessionCreationSource.Impersonation );
        var block = BuildBlock( rockContext, currentUser: userLogin, session: session );

        var box = ( Rock.ViewModels.Blocks.Security.ChangePassword.ChangePasswordBag ) block.GetObsidianBlockInitialization();

        Assert.IsFalse( box.IsChangePasswordVisible );
    }

    /// <summary>
    /// A request whose current <see cref="PersonSession"/> was created via a
    /// <see cref="PersonSessionCreationSource.UserToken"/> (rckipid email
    /// link) is gated out: the recipient of the email link has not provided
    /// a credential and must not be able to change the target person's
    /// password.
    /// </summary>
    [TestMethod]
    public void GetObsidianBlockInitialization_HidesPasswordChange_WhenSessionIsUserToken()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );

        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1 );
        var session = BuildSession( PersonSessionCreationSource.UserToken );
        var block = BuildBlock( rockContext, currentUser: userLogin, session: session );

        var box = ( Rock.ViewModels.Blocks.Security.ChangePassword.ChangePasswordBag ) block.GetObsidianBlockInitialization();

        Assert.IsFalse( box.IsChangePasswordVisible );
    }

    /// <summary>
    /// A current user authenticated via a normal credential
    /// (<see cref="PersonSessionCreationSource.Component"/>) passes the gate
    /// and the block surfaces the change-password form
    /// (<c>IsChangePasswordVisible = true</c>). This is the positive path —
    /// pointing the <see cref="UserLogin"/> at the <see cref="Database"/>
    /// auth component lets the downstream <c>SupportsChangePassword</c>
    /// check succeed so we can assert on the visibility flag directly
    /// instead of pattern-matching an error message.
    /// </summary>
    [TestMethod]
    public void GetObsidianBlockInitialization_AllowsPasswordChange_WhenSessionIsComponent()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );

        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1 );
        var session = BuildSession( PersonSessionCreationSource.Component );
        var block = BuildBlock( rockContext, currentUser: userLogin, session: session );

        var box = ( Rock.ViewModels.Blocks.Security.ChangePassword.ChangePasswordBag ) block.GetObsidianBlockInitialization();

        Assert.IsTrue( box.IsChangePasswordVisible );
    }

    /// <summary>
    /// A current user backed by a session with no
    /// <see cref="PersonSessionCreationSource"/> exposure of impersonation
    /// (the <c>PersonSession</c> is null entirely) still passes the gate
    /// because the block's check is <c>PersonSession?.IsImpersonated() ==
    /// true</c>. This guards the edge case where a code path sets
    /// <c>CurrentUser</c> without populating <c>PersonSession</c> (e.g., a
    /// legacy test rig or a non-PersonSession auth flow).
    /// </summary>
    [TestMethod]
    public void GetObsidianBlockInitialization_AllowsPasswordChange_WhenCurrentUserButNoPersonSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var authComponent = GetDatabaseAuthComponent( rockContext );

        var userLogin = SeedUserLogin( rockContext, authComponent, userLoginId: 1, userName: "testuser", personId: 1 );
        var block = BuildBlock( rockContext, currentUser: userLogin, session: null );

        var box = ( Rock.ViewModels.Blocks.Security.ChangePassword.ChangePasswordBag ) block.GetObsidianBlockInitialization();

        Assert.IsTrue( box.IsChangePasswordVisible );
    }

    #region Test infrastructure

    /// <summary>
    /// Builds a minimal <see cref="ChangePassword"/> instance with a synthetic
    /// <see cref="RockRequestContext"/> wired to <paramref name="currentUser"/>
    /// and <paramref name="session"/>.
    /// </summary>
    private static ChangePassword BuildBlock( RockContext rockContext, UserLogin currentUser, PersonSession session )
    {
        var page = new Page { Id = 1 };
        var blockTypeEntityType = EntityTypeCache.Get<ChangePassword>( true, rockContext );
        var blockType = new BlockType
        {
            Id = 1,
            Name = "Change Password",
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

        var requestContext = new RockRequestContext( new NullRockResponseContext() )
        {
            CurrentUser = currentUser,
            PersonSession = session,
        };

        return new ChangePassword
        {
            RockContext = rockContext,
            RequestContext = requestContext,
            PageCache = PageCache.Get( page.Id, rockContext ),
            BlockCache = BlockCache.Get( block.Id, rockContext ),
        };
    }

    /// <summary>
    /// Builds an active <see cref="PersonSession"/> with the supplied
    /// <paramref name="creationSource"/>. The block's gate only inspects
    /// <see cref="PersonSession.IsImpersonated"/>, which is a pure function
    /// of <see cref="PersonSession.CreationSource"/>, so no recency stamps
    /// are needed.
    /// </summary>
    private static PersonSession BuildSession( PersonSessionCreationSource creationSource )
    {
        return new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 1,
            IsActive = true,
            CreationSource = creationSource,
        };
    }

    /// <summary>
    /// Seeds a <see cref="UserLogin"/> + <see cref="Person"/> graph into the
    /// mocked <see cref="RockContext"/>, with the <c>UserLogin</c> pointed at
    /// <paramref name="authComponent"/>'s <see cref="EntityType"/> so the
    /// block's <c>AuthenticationContainer.GetComponent</c> +
    /// <c>SupportsChangePassword</c> chain resolves to the real
    /// <see cref="Database"/> component and the positive-path tests can
    /// assert on <c>IsChangePasswordVisible = true</c> directly.
    /// </summary>
    private static UserLogin SeedUserLogin( RockContext rockContext, AuthenticationComponent authComponent, int userLoginId, string userName, int personId )
    {
        var componentEntityType = EntityTypeCache.Get( authComponent.GetType().FullName, true, rockContext );

        var person = new Person
        {
            Id = personId,
            FirstName = "Ted",
            LastName = "Decker",
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
    /// Returns the real <see cref="Database"/> auth-component instance from
    /// the <see cref="AuthenticationContainer"/>, with <c>IsActive</c>
    /// reliably set to <c>true</c>.
    /// </summary>
    private static AuthenticationComponent GetDatabaseAuthComponent( RockContext rockContext )
    {
        var container = AuthenticationContainer.Instance;

        // Work around a bug in the way component attributes are initialized
        // since we don't have save hooks to clear the cache.
        EntityTypeAttributesCache.Clear();

        var component = container.Components.Values.Select( v => v.Value ).OfType<Database>().FirstOrDefault();

        component.AttributeValues = new System.Collections.Generic.Dictionary<string, AttributeValueCache>
        {
            ["Active"] = new AttributeValueCache { Value = "True" },
        };

        return component;
    }

    #endregion Test infrastructure
}

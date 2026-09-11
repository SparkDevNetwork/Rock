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
using System.Collections.Specialized;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Net;

/// <summary>
/// Plain unit tests for <see cref="RockRequestContext"/>. Currently
/// covers the <see cref="PersonSession"/>-aware authentication surface:
/// the <see cref="RockRequestContext.PersonSession"/> cache slot, the
/// <see cref="RockRequestContext.SetPersonSession(PersonSession)"/> writer,
/// and the <see cref="RockRequestContext.MeetsRequirement(AuthenticationRequirement)"/>
/// policy method.
/// </summary>
[TestClass]
public class RockRequestContextTests
{
    #region SetPersonSession / PersonSession

    /// <summary>
    /// A fresh request context exposes no <see cref="PersonSession"/>.
    /// Anonymous requests are a legitimate state; consumers must handle
    /// null. The setter being callable is what the
    /// <c>Application_BeginRequest</c> shim depends on, so the default
    /// state and the round-trip have to be verifiable in isolation.
    /// </summary>
    [TestMethod]
    public void PersonSession_DefaultsToNull_OnFreshContext()
    {
        var context = new RockRequestContext();

        Assert.IsNull( context.PersonSession );
    }

    /// <summary>
    /// <see cref="RockRequestContext.SetPersonSession(PersonSession)"/>
    /// stashes the supplied session and the
    /// <see cref="RockRequestContext.PersonSession"/> property returns
    /// the same reference. This is the exact contract Global.asax relies
    /// on after <c>ResolveSessionForRequest</c> returns.
    /// </summary>
    [TestMethod]
    public void SetPersonSession_StashesSession_AndPersonSessionReturnsIt()
    {
        var context = new RockRequestContext();
        var session = new PersonSession { IsActive = true };

        context.SetPersonSession( session );

        Assert.AreSame( session, context.PersonSession );
    }

    /// <summary>
    /// Setting null clears any previously-stashed session. The
    /// PostAuthenticateRequest hook does not call this path today, but
    /// the setter must be symmetric so a future code path can clear the
    /// session (e.g. during logout) without reaching for reflection.
    /// </summary>
    [TestMethod]
    public void SetPersonSession_ClearsSession_WhenPassedNull()
    {
        var context = new RockRequestContext();
        context.SetPersonSession( new PersonSession { IsActive = true } );

        context.SetPersonSession( null );

        Assert.IsNull( context.PersonSession );
    }

    #endregion SetPersonSession / PersonSession

    #region MeetsRequirement

    /// <summary>
    /// With no <see cref="PersonSession"/> on the context (anonymous
    /// request), <see cref="AuthenticationRequirement.Elevated"/> is NOT
    /// satisfied. This is the most common "block requires step-up but
    /// user is anonymous" case.
    /// </summary>
    [TestMethod]
    public void MeetsRequirement_Elevated_IsFalse_ForNullPersonSession()
    {
        var context = new RockRequestContext();

        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.Elevated ) );
    }

    /// <summary>
    /// With no <see cref="PersonSession"/> on the context,
    /// <see cref="AuthenticationRequirement.MultiFactor"/> is NOT satisfied.
    /// </summary>
    [TestMethod]
    public void MeetsRequirement_MultiFactor_IsFalse_ForNullPersonSession()
    {
        var context = new RockRequestContext();

        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.MultiFactor ) );
    }

    /// <summary>
    /// An inactive session reports
    /// <see cref="AuthenticationStrength.NotAuthenticated"/> regardless of
    /// recency stamps; both requirements must return false.
    /// </summary>
    [TestMethod]
    public void MeetsRequirement_Elevated_IsFalse_ForInactiveSession()
    {
        var context = new RockRequestContext();
        context.SetPersonSession( new PersonSession
        {
            IsActive = false,
            LastStepUpAuthenticationDateTime = RockDateTime.Now,
            LastMultiFactorAuthenticationDateTime = RockDateTime.Now,
        } );

        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.Elevated ) );
        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.MultiFactor ) );
    }

    /// <summary>
    /// An active session with no recency stamps reports
    /// <see cref="AuthenticationStrength.Authenticated"/>; neither
    /// requirement is satisfied.
    /// </summary>
    [TestMethod]
    public void MeetsRequirement_IsFalse_ForAuthenticatedStrength()
    {
        var context = new RockRequestContext();
        context.SetPersonSession( new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = null,
            LastMultiFactorAuthenticationDateTime = null,
        } );

        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.Elevated ) );
        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.MultiFactor ) );
    }

    /// <summary>
    /// A session with recent step-up but no MFA reports
    /// <see cref="AuthenticationStrength.Elevated"/>:
    /// <see cref="AuthenticationRequirement.Elevated"/> is satisfied;
    /// <see cref="AuthenticationRequirement.MultiFactor"/> is NOT.
    /// </summary>
    [TestMethod]
    public void MeetsRequirement_ElevatedStrength_SatisfiesElevatedButNotMultiFactor()
    {
        var context = new RockRequestContext();
        context.SetPersonSession( new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = RockDateTime.Now,
            LastMultiFactorAuthenticationDateTime = null,
        } );

        Assert.IsTrue( context.MeetsRequirement( AuthenticationRequirement.Elevated ) );
        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.MultiFactor ) );
    }

    /// <summary>
    /// A session with recent MFA reports
    /// <see cref="AuthenticationStrength.MultiFactor"/>: BOTH
    /// requirements are satisfied. Elevated is satisfied because MFA is
    /// strictly stronger; the policy collapses to "MFA implies Elevated".
    /// </summary>
    [TestMethod]
    public void MeetsRequirement_MultiFactorStrength_SatisfiesBothRequirements()
    {
        var context = new RockRequestContext();
        context.SetPersonSession( new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = RockDateTime.Now,
            LastMultiFactorAuthenticationDateTime = RockDateTime.Now,
        } );

        Assert.IsTrue( context.MeetsRequirement( AuthenticationRequirement.Elevated ) );
        Assert.IsTrue( context.MeetsRequirement( AuthenticationRequirement.MultiFactor ) );
    }

    /// <summary>
    /// A session whose step-up timestamp predates the elevated-recency
    /// window reports <see cref="AuthenticationStrength.Authenticated"/>;
    /// neither requirement is satisfied. Guards against accidentally
    /// honoring stale recency.
    /// </summary>
    [TestMethod]
    public void MeetsRequirement_IsFalse_ForExpiredElevatedRecency()
    {
        var context = new RockRequestContext();
        context.SetPersonSession( new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = RockDateTime.Now.AddHours( -2 ),
            LastMultiFactorAuthenticationDateTime = null,
        } );

        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.Elevated ) );
        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.MultiFactor ) );
    }

    /// <summary>
    /// A session whose MFA timestamp predates the multi-factor recency
    /// window but whose step-up timestamp is fresh reports
    /// <see cref="AuthenticationStrength.Elevated"/>: Elevated is
    /// satisfied; MultiFactor is NOT. Closes the "MFA window expires
    /// independently" edge case.
    /// </summary>
    [TestMethod]
    public void MeetsRequirement_IsFalse_ForExpiredMfaButFreshStepUp()
    {
        var context = new RockRequestContext();
        context.SetPersonSession( new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = RockDateTime.Now,
            LastMultiFactorAuthenticationDateTime = RockDateTime.Now.AddHours( -2 ),
        } );

        Assert.IsTrue( context.MeetsRequirement( AuthenticationRequirement.Elevated ) );
        Assert.IsFalse( context.MeetsRequirement( AuthenticationRequirement.MultiFactor ) );
    }

    #endregion MeetsRequirement

    #region Identity resolution (explicit vs. factory precedence)

    /// <summary>
    /// A fresh context with neither an explicit identity nor a factory exposes a
    /// null <see cref="RockRequestContext.CurrentPerson"/> and
    /// <see cref="RockRequestContext.CurrentUser"/> (anonymous).
    /// </summary>
    [TestMethod]
    public void Identity_DefaultsToNull_OnFreshContext()
    {
        var context = new RockRequestContext();

        Assert.IsNull( context.CurrentPerson );
        Assert.IsNull( context.CurrentUser );
        Assert.IsNull( context.PersonSession );
    }

    /// <summary>
    /// <see cref="RockRequestContext.SetCurrentIdentity(Person, UserLogin)"/> pins
    /// <see cref="RockRequestContext.CurrentPerson"/> and
    /// <see cref="RockRequestContext.CurrentUser"/>; with no session set and no
    /// factory, <see cref="RockRequestContext.PersonSession"/> stays null. This is
    /// the <c>ClientConnectedAsync</c> shape (identity without a session).
    /// </summary>
    [TestMethod]
    public void SetCurrentIdentity_PinsPersonAndUser_LeavesPersonSessionNull()
    {
        var person = new Person();
        var user = new UserLogin { UserName = "ted" };
        var context = new RockRequestContext();

        context.SetCurrentIdentity( person, user );

        Assert.AreSame( person, context.CurrentPerson );
        Assert.AreSame( user, context.CurrentUser );
        Assert.IsNull( context.PersonSession );
    }

    /// <summary>
    /// The factory resolves all three properties from one <see cref="PersonSession"/>
    /// when no explicit value was set: <see cref="RockRequestContext.PersonSession"/>
    /// is the resolved session, <see cref="RockRequestContext.CurrentPerson"/> derives
    /// from <c>PersonAlias.Person</c>, and <see cref="RockRequestContext.CurrentUser"/>
    /// from <c>UserLogin</c>.
    /// </summary>
    [TestMethod]
    public void Factory_ResolvesAllThree_FromResolvedSession()
    {
        var person = new Person();
        var user = new UserLogin { UserName = "ted" };
        var session = new PersonSession
        {
            IsActive = true,
            PersonAlias = new PersonAlias { Person = person },
            UserLogin = user,
        };
        var context = new RockRequestContext();

        context.SetPersonSessionFactory( () => session );

        Assert.AreSame( session, context.PersonSession );
        Assert.AreSame( person, context.CurrentPerson );
        Assert.AreSame( user, context.CurrentUser );
    }

    /// <summary>
    /// The factory is invoked lazily - not until a property is first read - and
    /// exactly once, with the result shared across all three properties.
    /// </summary>
    [TestMethod]
    public void Factory_IsInvokedLazily_AndOnce()
    {
        var session = new PersonSession
        {
            IsActive = true,
            PersonAlias = new PersonAlias { Person = new Person() },
            UserLogin = new UserLogin(),
        };
        var callCount = 0;
        var context = new RockRequestContext();

        context.SetPersonSessionFactory( () =>
        {
            callCount++;
            return session;
        } );

        Assert.AreEqual( 0, callCount, "Factory must not run until an identity property is read." );

        _ = context.PersonSession;
        _ = context.CurrentPerson;
        _ = context.CurrentUser;

        Assert.AreEqual( 1, callCount, "Factory must resolve once and cache the result for all three properties." );
    }

    /// <summary>
    /// A factory that resolves null (anonymous) leaves all three properties null.
    /// </summary>
    [TestMethod]
    public void Factory_ResolvingNull_LeavesIdentityNull()
    {
        var context = new RockRequestContext();

        context.SetPersonSessionFactory( () => null );

        Assert.IsNull( context.PersonSession );
        Assert.IsNull( context.CurrentPerson );
        Assert.IsNull( context.CurrentUser );
    }

    /// <summary>
    /// An explicit identity wins over the factory for
    /// <see cref="RockRequestContext.CurrentPerson"/> /
    /// <see cref="RockRequestContext.CurrentUser"/>, while
    /// <see cref="RockRequestContext.PersonSession"/> (not set explicitly) still
    /// comes from the factory. This is the pipeline shape: <c>BeginRequest</c>
    /// installs the factory, then the managed pipeline pins the identity via
    /// <c>SetCurrentIdentity</c>.
    /// </summary>
    [TestMethod]
    public void ExplicitIdentity_WinsOverFactory_ForPersonAndUser()
    {
        var explicitPerson = new Person();
        var explicitUser = new UserLogin { UserName = "explicit" };
        var factorySession = new PersonSession
        {
            IsActive = true,
            PersonAlias = new PersonAlias { Person = new Person() },
            UserLogin = new UserLogin { UserName = "factory" },
        };
        var context = new RockRequestContext();

        context.SetPersonSessionFactory( () => factorySession );
        context.SetCurrentIdentity( explicitPerson, explicitUser );

        Assert.AreSame( explicitPerson, context.CurrentPerson );
        Assert.AreSame( explicitUser, context.CurrentUser );
        Assert.AreSame( factorySession, context.PersonSession );
    }

    /// <summary>
    /// An explicit <see cref="PersonSession"/> wins over the factory for
    /// <see cref="RockRequestContext.PersonSession"/>, while
    /// <see cref="RockRequestContext.CurrentPerson"/> /
    /// <see cref="RockRequestContext.CurrentUser"/> (no explicit identity) still
    /// derive from the factory's session.
    /// </summary>
    [TestMethod]
    public void ExplicitSession_WinsOverFactory_ForPersonSession()
    {
        var explicitSession = new PersonSession { IsActive = true };
        var factoryPerson = new Person();
        var factoryUser = new UserLogin { UserName = "factory" };
        var factorySession = new PersonSession
        {
            IsActive = true,
            PersonAlias = new PersonAlias { Person = factoryPerson },
            UserLogin = factoryUser,
        };
        var context = new RockRequestContext();

        context.SetPersonSessionFactory( () => factorySession );
        context.SetPersonSession( explicitSession );

        Assert.AreSame( explicitSession, context.PersonSession );
        Assert.AreSame( factoryPerson, context.CurrentPerson );
        Assert.AreSame( factoryUser, context.CurrentUser );
    }

    /// <summary>
    /// <see cref="RockRequestContext.SetPersonSessionFactory(Func{PersonSession})"/>
    /// with null removes a previously-installed factory, returning the context to
    /// the anonymous default.
    /// </summary>
    [TestMethod]
    public void SetPersonSessionFactory_Null_ClearsFactory()
    {
        var session = new PersonSession
        {
            IsActive = true,
            PersonAlias = new PersonAlias { Person = new Person() },
        };
        var context = new RockRequestContext();
        context.SetPersonSessionFactory( () => session );

        context.SetPersonSessionFactory( null );

        Assert.IsNull( context.PersonSession );
        Assert.IsNull( context.CurrentPerson );
        Assert.IsNull( context.CurrentUser );
    }

    #endregion Identity resolution (explicit vs. factory precedence)

    #region Route-data page parameter filtering

    /// <summary>
    /// Framework route-data keys prefixed <c>"MS_"</c> (e.g. System.Web.Http's
    /// <c>"MS_SubRoutes"</c>, whose value is an <c>IHttpRouteData[]</c>) are NOT
    /// page parameters and must be filtered out when a request's route data is
    /// merged into <see cref="RockRequestContext.PageParameters"/>. Otherwise
    /// they leak into any URL rebuilt from the page parameters (e.g.
    /// <c>GetCurrentPageUrl</c>) - which is how <c>?MS_SubRoutes=...</c> ended
    /// up on an impersonation redirect URL. Real route parameters (PascalCase,
    /// like <c>PersonId</c>) must still come through.
    /// </summary>
    [TestMethod]
    public void PageParameters_ExcludeFrameworkRouteKeys_ButKeepRealRouteParameters()
    {
        // The request-context constructor resolves geolocation / visitor state,
        // which needs a configured RockApp.
        using var scope = TestHelper.CreateScopedRockApp();

        var routeData = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase )
        {
            ["MS_SubRoutes"] = new object[0],
            ["PersonId"] = "42",
        };

        var context = BuildRequestContextWithRouteData( routeData );

        var parameters = context.GetPageParameters();

        Assert.IsFalse( parameters.ContainsKey( "MS_SubRoutes" ), "Framework route key MS_SubRoutes should be filtered out of page parameters." );
        Assert.AreEqual( "42", parameters["PersonId"] );
    }

    #endregion Route-data page parameter filtering

    #region Test infrastructure

    /// <summary>
    /// Builds a <see cref="RockRequestContext"/> backed by a Moq
    /// <see cref="IRequest"/> whose route data is <paramref name="routeData"/>,
    /// so page-parameter merging from route values can be asserted. All other
    /// request members are stubbed to empty.
    /// </summary>
    private static RockRequestContext BuildRequestContextWithRouteData( IDictionary<string, object> routeData )
    {
        var requestMock = new Mock<IRequest>( MockBehavior.Strict );
        requestMock.SetupGet( r => r.RemoteAddress ).Returns( IPAddress.Loopback );
        requestMock.SetupGet( r => r.RequestUri ).Returns( ( Uri ) null );
        requestMock.SetupGet( r => r.Method ).Returns( "GET" );
        requestMock.SetupGet( r => r.QueryString ).Returns( new NameValueCollection( StringComparer.OrdinalIgnoreCase ) );
        requestMock.SetupGet( r => r.RouteData ).Returns( routeData );
        requestMock.SetupGet( r => r.Headers ).Returns( new NameValueCollection( StringComparer.OrdinalIgnoreCase ) );
        requestMock.SetupGet( r => r.Cookies ).Returns( new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase ) );
        requestMock.SetupGet( r => r.CookiesValuesAreUrlDecoded ).Returns( false );

        return new RockRequestContext( requestMock.Object, new NullRockResponseContext(), currentUser: null );
    }

    #endregion Test infrastructure
}

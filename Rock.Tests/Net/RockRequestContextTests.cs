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

using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;

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
}

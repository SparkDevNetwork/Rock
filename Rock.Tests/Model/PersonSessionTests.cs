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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Enums.Security;
using Rock.Model;

namespace Rock.Tests.Model;

/// <summary>
/// Plain unit tests for the <see cref="PersonSession"/> entity. Anything that
/// needs a real database lives in
/// <c>Rock.Tests.Integration.Security.PersonSessionTests</c>; anything that
/// needs a mocked <c>RockContext</c> lives in
/// <see cref="PersonSessionServiceTests"/>.
/// </summary>
[TestClass]
public class PersonSessionTests
{
    /// <summary>
    /// New <see cref="PersonSession"/> instances default to
    /// <see cref="PersonSession.IsActive"/> = <c>true</c>. Inverting this
    /// default would silently break every <c>Start*Session</c> path in
    /// <c>PersonSessionService</c> (none of them stamp the flag themselves).
    /// </summary>
    [TestMethod]
    public void IsActive_DefaultsToTrue_OnConstruction()
    {
        var session = new PersonSession();

        Assert.IsTrue( session.IsActive );
    }

    #region GetAuthenticationStrength

    /// <summary>
    /// An inactive session reports <see cref="AuthenticationStrength.NotAuthenticated"/>
    /// regardless of how recently its credential timestamps were stamped.
    /// </summary>
    [TestMethod]
    public void GetAuthenticationStrength_ReturnsNotAuthenticated_WhenSessionIsInactive()
    {
        var session = new PersonSession
        {
            IsActive = false,
            LastStepUpAuthenticationDateTime = RockDateTime.Now,
            LastMultiFactorAuthenticationDateTime = RockDateTime.Now,
        };

        Assert.AreEqual( AuthenticationStrength.NotAuthenticated, session.GetAuthenticationStrength() );
    }

    /// <summary>
    /// An active session with no recency stamps reports
    /// <see cref="AuthenticationStrength.Authenticated"/>.
    /// </summary>
    [TestMethod]
    public void GetAuthenticationStrength_ReturnsAuthenticated_WhenNeitherRecencyTimestampIsSet()
    {
        var session = new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = null,
            LastMultiFactorAuthenticationDateTime = null,
        };

        Assert.AreEqual( AuthenticationStrength.Authenticated, session.GetAuthenticationStrength() );
    }

    /// <summary>
    /// A step-up timestamp inside the recency window reports
    /// <see cref="AuthenticationStrength.Elevated"/>.
    /// </summary>
    [TestMethod]
    public void GetAuthenticationStrength_ReturnsElevated_WhenStepUpIsRecent()
    {
        var session = new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = RockDateTime.Now.AddMinutes( -5 ),
            LastMultiFactorAuthenticationDateTime = null,
        };

        Assert.AreEqual( AuthenticationStrength.Elevated, session.GetAuthenticationStrength() );
    }

    /// <summary>
    /// A step-up timestamp outside the recency window falls back to
    /// <see cref="AuthenticationStrength.Authenticated"/>.
    /// </summary>
    [TestMethod]
    public void GetAuthenticationStrength_ReturnsAuthenticated_WhenStepUpIsStale()
    {
        var session = new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = RockDateTime.Now.AddHours( -24 ),
            LastMultiFactorAuthenticationDateTime = null,
        };

        Assert.AreEqual( AuthenticationStrength.Authenticated, session.GetAuthenticationStrength() );
    }

    /// <summary>
    /// An MFA timestamp inside the recency window reports
    /// <see cref="AuthenticationStrength.MultiFactor"/>.
    /// </summary>
    [TestMethod]
    public void GetAuthenticationStrength_ReturnsMultiFactor_WhenMfaIsRecent()
    {
        var session = new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = null,
            LastMultiFactorAuthenticationDateTime = RockDateTime.Now.AddMinutes( -5 ),
        };

        Assert.AreEqual( AuthenticationStrength.MultiFactor, session.GetAuthenticationStrength() );
    }

    /// <summary>
    /// When both windows are satisfied, the strongest applicable value wins —
    /// <see cref="AuthenticationStrength.MultiFactor"/>, not
    /// <see cref="AuthenticationStrength.Elevated"/>.
    /// </summary>
    [TestMethod]
    public void GetAuthenticationStrength_ReturnsMultiFactor_WhenBothRecencyWindowsAreSatisfied()
    {
        var session = new PersonSession
        {
            IsActive = true,
            LastStepUpAuthenticationDateTime = RockDateTime.Now.AddMinutes( -3 ),
            LastMultiFactorAuthenticationDateTime = RockDateTime.Now.AddMinutes( -5 ),
        };

        Assert.AreEqual( AuthenticationStrength.MultiFactor, session.GetAuthenticationStrength() );
    }

    #endregion GetAuthenticationStrength

    #region IsImpersonated

    /// <summary>
    /// <see cref="PersonSession.IsImpersonated()"/> truth table. The two
    /// "impersonated" creation sources are
    /// <see cref="PersonSessionCreationSource.Impersonation"/> (admin) and
    /// <see cref="PersonSessionCreationSource.UserToken"/> (rckipid email
    /// link). Everything else is a normal authenticated session.
    /// </summary>
    [TestMethod]
    [DataRow( PersonSessionCreationSource.Unknown, false )]
    [DataRow( PersonSessionCreationSource.Component, false )]
    [DataRow( PersonSessionCreationSource.Impersonation, true )]
    [DataRow( PersonSessionCreationSource.UserToken, true )]
    [DataRow( PersonSessionCreationSource.ApiKey, false )]
    [DataRow( PersonSessionCreationSource.Legacy, false )]
    public void IsImpersonated_ReturnsExpected_ForCreationSource( PersonSessionCreationSource source, bool expected )
    {
        var session = new PersonSession { CreationSource = source };

        Assert.AreEqual( expected, session.IsImpersonated() );
    }

    #endregion IsImpersonated
}

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
using System.Net.Http;
using System.Web.Http.Controllers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Rest.Filters;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Rest.Filters;

/// <summary>
/// Mocked-database tests for the API-key / bearer-token branches of
/// <see cref="AuthenticateAttribute.OnAuthorization"/> - the integration seam
/// where PersonSession participation is actually decided for those credentials.
/// The cookie-session (PIN) branch is covered by the integration
/// <c>Rock.Tests.Integration.Rest.Filters.AuthenticateAttributeTests</c>.
/// </summary>
[TestClass]
public class AuthenticateAttributeTests
{
    /// <summary>
    /// An API-key request (via the <c>?apikey=</c> query string) drives
    /// <c>PersonSessionService.FindOrCreateApiKeySession</c>: a
    /// <see cref="PersonSessionCreationSource.ApiKey"/> session is created for the
    /// key's <see cref="UserLogin"/> and attached to the request context. This
    /// guards the attribute-level seam - the service method is unit-tested
    /// separately, but that AuthenticateAttribute actually calls it on an API-key
    /// request is what this covers.
    /// </summary>
    [TestMethod]
    public void OnAuthorization_ApiKeyRequest_CreatesApiKeySessionForUserLogin()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var databaseEntityTypeId = EntityTypeCache.Get( typeof( Rock.Security.Authentication.Database ) ).Id;

        var person = new Person { Id = 100, FirstName = "Ted", LastName = "Decker", PrimaryAliasId = 200 };
        var personAlias = new PersonAlias { Id = 200, PersonId = 100, AliasPersonId = 100, Person = person, Guid = Guid.NewGuid() };
        var userLogin = new UserLogin
        {
            Id = 52,
            UserName = "ted-decker-apikey",
            ApiKey = "SECRET-API-KEY",
            EntityTypeId = databaseEntityTypeId,
            PersonId = 100,
            Person = person,
            IsConfirmed = true,
            IsLockedOut = false,
        };

        rockContext.Set<Person>().Add( person );
        rockContext.Set<PersonAlias>().Add( personAlias );
        rockContext.Set<UserLogin>().Add( userLogin );
        rockContext.SaveChanges();

        var requestContext = new RockRequestContext();
        var actionContext = BuildApiKeyActionContext( requestContext, apiKey: "SECRET-API-KEY" );

        new AuthenticateAttribute().OnAuthorization( actionContext );

        var session = new PersonSessionService( scope.App.CreateRockContext() ).Queryable()
            .FirstOrDefault( s => s.UserLoginId == userLogin.Id && s.CreationSource == PersonSessionCreationSource.ApiKey );

        Assert.IsNotNull( session, "An ApiKey-source PersonSession should be created for the authenticating UserLogin." );
        Assert.IsNotNull( requestContext.PersonSession, "The resolved session should be attached to the request context." );
        Assert.AreEqual( session.Id, requestContext.PersonSession.Id );
    }

    /// <summary>
    /// Builds an <see cref="HttpActionContext"/> for an <c>?apikey=</c> request,
    /// wiring the same IServiceProvider -&gt; IRockRequestContextAccessor lookup the
    /// filter uses in production. No PersonSession / principal is pre-set so the
    /// cookie and OIDC branches fall through to the API-key branch.
    /// </summary>
    private static HttpActionContext BuildApiKeyActionContext( RockRequestContext requestContext, string apiKey )
    {
        var accessor = new TestRockRequestContextAccessor { RockRequestContext = requestContext };
        var serviceProvider = new TestServiceProvider( accessor );

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri( $"http://localhost/api/v2/models/people?apikey={apiKey}" )
        };
        request.Properties["RockServiceProvider"] = serviceProvider;

        return new HttpActionContext
        {
            ControllerContext = new HttpControllerContext
            {
                Request = request,
                RequestContext = new HttpRequestContext()
            }
        };
    }

    /// <summary>
    /// Minimal <see cref="IRockRequestContextAccessor"/> returning a preset context.
    /// </summary>
    private sealed class TestRockRequestContextAccessor : IRockRequestContextAccessor
    {
        public RockRequestContext RockRequestContext { get; set; }
    }

    /// <summary>
    /// Minimal <see cref="IServiceProvider"/> resolving only the
    /// <see cref="IRockRequestContextAccessor"/> the filter asks for.
    /// </summary>
    private sealed class TestServiceProvider : IServiceProvider
    {
        private readonly IRockRequestContextAccessor _accessor;

        public TestServiceProvider( IRockRequestContextAccessor accessor )
        {
            _accessor = accessor;
        }

        public object GetService( Type serviceType )
        {
            if ( serviceType == typeof( IRockRequestContextAccessor ) )
            {
                return _accessor;
            }

            return null;
        }
    }
}

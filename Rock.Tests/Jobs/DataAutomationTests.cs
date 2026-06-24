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
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Jobs
{
    /// <summary>
    /// Mocked-database tests for the <see cref="DataAutomation"/> job. Phase 14
    /// of the PersonSession plan: GetPeopleWhoHaveSiteLogins must read activity
    /// from <see cref="PersonSession.LastActivityDateTime"/>, not
    /// <see cref="UserLogin.LastActivityDateTime"/> (which is no longer
    /// written under the PersonSession model).
    /// </summary>
    [TestClass]
    public class DataAutomationTests
    {
        #region GetPeopleWhoHaveSiteLogins

        /// <summary>
        /// A person with a recent <see cref="PersonSession"/> is reported as
        /// having a recent site login, even when their
        /// <see cref="UserLogin.LastActivityDateTime"/> is stale. This is the
        /// regression guard against the legacy UserLogin-based read.
        /// </summary>
        [TestMethod]
        public void GetPeopleWhoHaveSiteLogins_ReadsFromPersonSession_NotUserLogin()
        {
            using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
            var rockContext = scope.App.CreateRockContext();

            const int targetPersonId = 42;
            const int periodInDays = 30;
            var staleDate = RockDateTime.Now.AddDays( -90 );
            var recentDate = RockDateTime.Now.AddDays( -1 );

            // Stale UserLogin activity: under the legacy path this person would
            // be excluded; under the PersonSession path they must be included.
#pragma warning disable 618 // Intentionally seeding the obsolete UserLogin.LastActivityDateTime to prove the PersonSession-based path ignores legacy data.
            var userLogin = new UserLogin
            {
                Id = 1,
                UserName = "stalecharlie",
                PersonId = targetPersonId,
                LastActivityDateTime = staleDate,
            };
#pragma warning restore 618
            rockContext.Set<UserLogin>().Add( userLogin );

            var personAlias = new PersonAlias
            {
                Id = 7,
                PersonId = targetPersonId,
                Guid = Guid.NewGuid(),
            };
            rockContext.Set<PersonAlias>().Add( personAlias );

            var recentSession = new PersonSession
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                PersonAliasId = personAlias.Id,
                PersonAlias = personAlias,
                IsActive = true,
                LastActivityDateTime = recentDate,
                CreationSource = Rock.Enums.Security.PersonSessionCreationSource.Component,
            };
            rockContext.Set<PersonSession>().Add( recentSession );

            var result = InvokeGetPeopleWhoHaveSiteLogins( enabled: true, periodInDays: periodInDays, rockContext: rockContext );

            CollectionAssert.Contains( result, targetPersonId );
        }

        /// <summary>
        /// When the gate is off, the method short-circuits and returns an empty
        /// list without querying the database. Mirrors the existing public
        /// contract of <c>GetPeopleWhoHaveSiteLogins</c>.
        /// </summary>
        [TestMethod]
        public void GetPeopleWhoHaveSiteLogins_ReturnsEmpty_WhenDisabled()
        {
            using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
            var rockContext = scope.App.CreateRockContext();

            var result = InvokeGetPeopleWhoHaveSiteLogins( enabled: false, periodInDays: 30, rockContext: rockContext );

            Assert.IsEmpty( result );
        }

        /// <summary>
        /// A person whose only <see cref="PersonSession"/> activity is older
        /// than the configured period is NOT reported, even when other people
        /// have recent activity.
        /// </summary>
        [TestMethod]
        public void GetPeopleWhoHaveSiteLogins_ExcludesStalePersonSessionActivity()
        {
            using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
            var rockContext = scope.App.CreateRockContext();

            const int recentPersonId = 42;
            const int stalePersonId = 99;
            const int periodInDays = 30;

            var recentPersonAlias = new PersonAlias { Id = 1, PersonId = recentPersonId, Guid = Guid.NewGuid() };
            var stalePersonAlias = new PersonAlias { Id = 2, PersonId = stalePersonId, Guid = Guid.NewGuid() };
            rockContext.Set<PersonAlias>().Add( recentPersonAlias );
            rockContext.Set<PersonAlias>().Add( stalePersonAlias );

            rockContext.Set<PersonSession>().Add( new PersonSession
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                PersonAliasId = recentPersonAlias.Id,
                PersonAlias = recentPersonAlias,
                IsActive = true,
                LastActivityDateTime = RockDateTime.Now.AddDays( -1 ),
                CreationSource = Rock.Enums.Security.PersonSessionCreationSource.Component,
            } );

            rockContext.Set<PersonSession>().Add( new PersonSession
            {
                Id = 2,
                Guid = Guid.NewGuid(),
                PersonAliasId = stalePersonAlias.Id,
                PersonAlias = stalePersonAlias,
                IsActive = true,
                LastActivityDateTime = RockDateTime.Now.AddDays( -90 ),
                CreationSource = Rock.Enums.Security.PersonSessionCreationSource.Component,
            } );

            var result = InvokeGetPeopleWhoHaveSiteLogins( enabled: true, periodInDays: periodInDays, rockContext: rockContext );

            CollectionAssert.Contains( result, recentPersonId );
            CollectionAssert.DoesNotContain( result, stalePersonId );
        }

        #endregion GetPeopleWhoHaveSiteLogins

        /// <summary>
        /// Invokes the private instance method
        /// <c>DataAutomation.GetPeopleWhoHaveSiteLogins</c> via reflection.
        /// Production surface is left unchanged; this is the standard pattern
        /// for exercising a job's internal building blocks in isolation.
        /// </summary>
        private static List<int> InvokeGetPeopleWhoHaveSiteLogins( bool enabled, int periodInDays, RockContext rockContext )
        {
            var job = new DataAutomation();
            var method = typeof( DataAutomation ).GetMethod(
                "GetPeopleWhoHaveSiteLogins",
                BindingFlags.Instance | BindingFlags.NonPublic );

            Assert.IsNotNull( method, "GetPeopleWhoHaveSiteLogins was not found via reflection." );

            return ( List<int> ) method.Invoke( job, [enabled, periodInDays, rockContext] );
        }
    }
}

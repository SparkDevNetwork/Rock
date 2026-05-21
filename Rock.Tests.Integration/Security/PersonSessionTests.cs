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

using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Integration.Security;

/// <summary>
/// Full integration tests for the <see cref="PersonSession"/> entity. These
/// verify behavior that depends on the real save pipeline firing the
/// <c>PersonSession.SaveHook</c>; plain-POCO assertions live in
/// <c>Rock.Tests.Security.PersonSessionTests</c>.
/// </summary>
[TestClass]
public class PersonSessionTests : DatabaseTestsBase
{
    /// <summary>
    /// Saving a <see cref="PersonSession"/> with
    /// <see cref="PersonSession.IsActive"/> = <c>false</c> must stamp
    /// <c>InactiveDateTime</c> automatically via the save hook. The two
    /// columns are kept in lockstep so a caller cannot leave the row in a
    /// contradictory state.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void SaveHook_StampsInactiveDateTime_WhenIsActiveFlipsFalse()
    {
        var sessionGuid = Guid.NewGuid();
        int personAliasId;

        using ( var rockContext = new RockContext() )
        {
            var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            Assert.IsNotNull( tedDecker, "Ted Decker test data is required for this test." );
            Assert.IsNotNull( tedDecker.PrimaryAliasId, "Ted Decker must have a primary alias." );
            personAliasId = tedDecker.PrimaryAliasId.Value;

            // Create the session in the active state and confirm the save
            // hook leaves InactiveDateTime null.
            var session = new PersonSession
            {
                Guid = sessionGuid,
                PersonAliasId = personAliasId,
                IsActive = true,
                IssuedDateTime = RockDateTime.Now,
                LastActivityDateTime = RockDateTime.Now,
                IsPersistent = false,
                CreationSource = PersonSessionCreationSource.Component,
            };

            rockContext.Set<PersonSession>().Add( session );
            rockContext.SaveChanges();
        }

        using ( var rockContext = new RockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );
            Assert.IsTrue( session.IsActive );
            Assert.IsNull( session.InactiveDateTime, "InactiveDateTime should be null while the session is active." );

            // Flip the flag. The save hook is what stamps InactiveDateTime
            // — callers cannot write the column directly (private setter,
            // compile-time enforced).
            session.IsActive = false;
            rockContext.SaveChanges();
        }

        using ( var rockContext = new RockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );
            Assert.IsFalse( session.IsActive );
            Assert.IsNotNull( session.InactiveDateTime, "InactiveDateTime should be stamped once IsActive flips to false." );
        }
    }
}

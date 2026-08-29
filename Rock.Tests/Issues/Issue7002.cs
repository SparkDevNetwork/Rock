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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Issues
{
    /// <summary>
    /// Tests that verify correct functionality after fixing github issue #7002,
    /// using a mocked database rather than a live one for speed.
    ///
    /// A non-administrator could not access a Lava Application when the
    /// "RSR - Lava Application Developer" security role was inactivated, because
    /// <see cref="LavaApplicationCache.IsAuthorized(string, Person)"/> dereferenced
    /// the result of <see cref="RoleCache.Get(Guid)"/> without a null check, and
    /// <see cref="RoleCache.Get(Guid)"/> returns <c>null</c> for an inactive role.
    ///
    /// This is the mocked-database counterpart to the integration test of the same
    /// name in Rock.Tests.Integration; both exercise the same real authorization
    /// code path.
    /// </summary>
    [TestClass]
    public class Issue7002 : MockDatabaseTestsBase
    {
        #region Tests

        /// <summary>
        /// When the Lava Application Developer override role is inactive,
        /// <see cref="RoleCache.Get(Guid)"/> returns <c>null</c>. Checking
        /// authorization for a non-administrator should still complete normally
        /// (falling through to the standard authorization rules) rather than
        /// throwing a <see cref="NullReferenceException"/>.
        /// </summary>
        [TestMethod]
        public void LavaApplicationIsAuthorized_WithInactiveDeveloperRole_DoesNotThrow()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            // The security role group type is required by RoleCache.LoadById() so it can
            // recognize the seeded groups as security roles.
            var securityRoleGroupType = new GroupType
            {
                Id = 1,
                Guid = Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(),
                Name = "Security Role"
            };
            rockContextMock.Object.Set<GroupType>().Add( securityRoleGroupType );

            // The Administrators role must resolve to a non-null RoleCache so that the first
            // operand of the override-role check evaluates normally (and returns false for our
            // non-admin person). This isolates the failure to the inactive developer role.
            var administratorsRole = new Group
            {
                Id = 2,
                Guid = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid(),
                Name = "Rock Administration",
                IsActive = true,
                IsSecurityRole = true,
                GroupTypeId = securityRoleGroupType.Id
            };
            rockContextMock.Object.Set<Group>().Add( administratorsRole );

            // The Lava Application Developer role is INACTIVE. RoleCache.LoadById() returns null
            // for an inactive group, which is the exact condition that triggers the bug.
            var lavaApplicationDeveloperRole = new Group
            {
                Id = 3,
                Guid = Rock.SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS.AsGuid(),
                Name = "Lava Application Developer",
                IsActive = false,
                IsSecurityRole = true,
                GroupTypeId = securityRoleGroupType.Id
            };
            rockContextMock.Object.Set<Group>().Add( lavaApplicationDeveloperRole );

            // Seed the Lava Application's entity type so that resolving TypeId during
            // SetFromEntity() does not need to fabricate one.
            var lavaApplicationEntityType = new EntityType
            {
                Id = 4,
                Guid = "FFFE0DE1-B410-435E-9AA8-3A0B18AAF0F7".AsGuid(),
                Name = typeof( LavaApplication ).FullName
            };
            rockContextMock.Object.Set<EntityType>().Add( lavaApplicationEntityType );

            // A non-administrator person who is not a member of any override role.
            var nonAdministrator = new Person
            {
                Id = 10,
                Guid = new Guid( "8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4" )
            };

            // Build the application to authorize against. Empty attribute dictionaries prevent
            // SetFromEntity() from triggering a database attribute load.
            var lavaApplication = new LavaApplication
            {
                Id = 20,
                Guid = new Guid( "2C1CB26B-AB22-4D5B-8CE0-9B9B5D6F8F4E" ),
                Name = "Issue 7002 Test Application",
                Attributes = new Dictionary<string, AttributeCache>(),
                AttributeValues = new Dictionary<string, AttributeValueCache>()
            };

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var lavaApplicationCache = new LavaApplicationCache();
                lavaApplicationCache.SetFromEntity( lavaApplication );

                // Before the fix this throws a NullReferenceException because the inactive
                // developer role resolves to a null RoleCache, which is then dereferenced.
                var isAuthorized = lavaApplicationCache.IsAuthorized( LavaApplication.EXECUTE_VIEW, nonAdministrator );

                // The non-administrator has no explicit grant on this application and Lava
                // Applications intentionally break security inheritance, so access is denied.
                // The important part of this test is simply that the call completed without throwing.
                Assert.IsFalse( isAuthorized, "A non-administrator with no explicit grant should not be authorized." );
            }
        }

        #endregion
    }
}

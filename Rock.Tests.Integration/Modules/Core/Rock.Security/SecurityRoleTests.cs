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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared;
using Rock.Tests.Integration.TestData.Core;

namespace Rock.Tests.Integration.Core.Security
{
    /// <summary>
    /// Tests that verify the correct operation of Security Roles.
    /// </summary>
    [TestClass]
    public class SecurityRoleTests
    {
        [TestMethod]
        public void SecurityRole_DeleteRoleWithAuthAuditLogEntries_IsDeleted()
        {
            const string securityRoleGuid = "9081988D-CFC6-4A6A-9E02-2F557E5FF72F";

            var rockContext = new RockContext();

            // Add a new Security Group.
            var securityManager = SecurityDataManager.Instance;
            var addRoleArgs = new SecurityDataManager.AddSecurityRoleActionArgs
            {
                RoleName = "Test Security Role 1",
                RoleGuid = securityRoleGuid
            };
            securityManager.AddSecurityRole( rockContext, addRoleArgs );

            var groupService = new GroupService( rockContext );
            var groupTestRole = groupService.GetByIdentifierOrThrow( securityRoleGuid );

            // Add an Authorization for this Role to view the Decker Group.
            // This will create a corresponding entry in the AuthAuditLog table.
            var groupDecker = groupService.GetByIdentifierOrThrow( TestGuids.Groups.SmallGroupDeckerGuid );
            
            Authorization.AllowSecurityRole( groupDecker, "VIEW", groupTestRole, rockContext );

            // Save the changes.
            rockContext.SaveChanges();

            // Remove the Security Group and all associated AuthAuditLog entries.
            securityManager.DeleteSecurityRole( securityRoleGuid );

            groupTestRole = groupService.GetByIdentifier( securityRoleGuid );
            Assert.IsNull( groupTestRole, "Security Role is not deleted." );
        }
    }
}

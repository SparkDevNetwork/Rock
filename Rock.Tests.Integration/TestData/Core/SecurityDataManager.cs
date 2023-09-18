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
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestData.Core
{
    /// <summary>
    /// Provides actions to manage security and authorization data for users.
    /// </summary>
    public class SecurityDataManager
    {
        private static Lazy<SecurityDataManager> _dataManager = new Lazy<SecurityDataManager>();
        public static SecurityDataManager Instance => _dataManager.Value;

        #region Security Roles

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class SecurityRoleInfo
        {
            public string ForeignKey { get; set; }
            public string RoleName { get; set; }
            public string RoleGuid { get; set; }
            public bool? IsActive { get; set; } = true;
        }

        #region Add Security Role

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class AddSecurityRoleActionArgs : SecurityRoleInfo
        {
            public bool ReplaceIfExists { get; set; }
        }

        /// <summary>
        /// Add a new Security Role.
        /// A Security Role is a Group with a specific Group Type of "Security Role".
        /// A Group of any other Group Type can also be nominated as a Security Role,
        /// but it should be managed using the GroupDataManager.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public int AddSecurityRole( RockContext rockContext, AddSecurityRoleActionArgs args )
        {
            var roleGroup = new Group();

            rockContext.WrapTransaction( () =>
            {
                var groupService = new GroupService( rockContext );

                var roleGuid = args.RoleGuid.AsGuidOrNull();
                if ( roleGuid != null )
                {
                    var existingRoleGroup = groupService.Queryable().FirstOrDefault( g => g.Guid == roleGuid );
                    if ( existingRoleGroup != null )
                    {
                        if ( !args.ReplaceIfExists )
                        {
                            return;
                        }
                        DeleteSecurityRole( args.RoleGuid );
                        rockContext.SaveChanges();
                    }
                }

                roleGroup.IsSecurityRole = true;

                UpdateSecurityFromSecurityRoleInfo( rockContext, roleGroup, args );

                groupService.Add( roleGroup );

                rockContext.SaveChanges();
            } );

            return roleGroup.Id;
        }

        #endregion

        #region Update Security Role

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class UpdateSecurityRoleActionArgs : SecurityRoleInfo
        {
            public string UpdateTargetIdentifier { get; set; }
        }

        /// <summary>
        /// Update an existing Security Role.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public void UpdateSecurity( RockContext rockContext, UpdateSecurityRoleActionArgs args )
        {
            rockContext.WrapTransaction( () =>
            {
                var groupService = new GroupService( rockContext );
                var roleGroup = groupService.GetByIdentifierOrThrow( args.UpdateTargetIdentifier );

                UpdateSecurityFromSecurityRoleInfo( rockContext, roleGroup, args );

                rockContext.SaveChanges();
            } );
        }

        private void UpdateSecurityFromSecurityRoleInfo( RockContext rockContext, Group roleGroup, SecurityRoleInfo args )
        {
            // Set Guid.
            if ( args.RoleGuid.IsNotNullOrWhiteSpace() )
            {
                roleGroup.Guid = InputParser.ParseToGuidOrThrow( nameof( SecurityRoleInfo.RoleGuid ), args.RoleGuid );
            }

            roleGroup.GroupTypeId = GroupTypeCache.GetSecurityRoleGroupType().Id;

            // Set Name.
            if ( args.RoleName.IsNotNullOrWhiteSpace() )
            {
                roleGroup.Name = args.RoleName;
            }

            // Set ForeignKey.
            if ( args.ForeignKey.IsNotNullOrWhiteSpace() )
            {
                roleGroup.ForeignKey = args.ForeignKey;
            }

            // Set IsActive.
            if ( args.IsActive.HasValue )
            {
                roleGroup.IsActive = args.IsActive.Value;
            }

            rockContext.SaveChanges();
        }

        #endregion

        #region Delete Security Role

        /// <summary>
        /// Delete a Security Role.
        /// </summary>
        /// <param name="roleGroupIdentifier"></param>
        /// <returns></returns>
        public bool DeleteSecurityRole( string roleGroupIdentifier )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var roleGroup = groupService.Get( roleGroupIdentifier );

            GroupService.DeleteSecurityRoleGroup( roleGroup.Id );

            return true;
        }

        #endregion

        #endregion
    }
}


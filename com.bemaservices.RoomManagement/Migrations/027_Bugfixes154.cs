// <copyright>
// Copyright by BEMA Software Services
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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 27, "1.9.4" )]
    public class Bugfixes154 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.DeleteSecurityAuth( "7f3ee024-bff0-4872-b3d1-585b74fbc2d4" );
            RockMigrationHelper.DeleteSecurityAuth( "6cf7e214-98f3-4071-8f71-d6d7fd9c4989" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "com.bemaservices.RoomManagement.Model.ReservationType", 0, "Edit", true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, 0, "6cf7e214-98f3-4071-8f71-d6d7fd9c4989" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "com.bemaservices.RoomManagement.Model.ReservationType", 1, "Edit", true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, 0, "7f3ee024-bff0-4872-b3d1-585b74fbc2d4" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "7f3ee024-bff0-4872-b3d1-585b74fbc2d4" );
            RockMigrationHelper.DeleteSecurityAuth( "6cf7e214-98f3-4071-8f71-d6d7fd9c4989" );
        }
    }
}
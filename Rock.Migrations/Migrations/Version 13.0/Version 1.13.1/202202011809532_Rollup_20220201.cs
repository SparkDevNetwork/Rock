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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20220201 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CheckInDeleteAttenanceSecurityUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CheckInDeleteAttenanceSecurityDown();
        }

        /// <summary>
        /// DV: Adds the new security auth for Delete Attendance on the Roster block.
        /// </summary>
        private void CheckInDeleteAttenanceSecurityUp()
        {
	        RockMigrationHelper.AddSecurityAuthForBlock(
		        "26b9d6b8-8153-4f17-805a-8512bac656e0",
		        0,
		        Rock.Security.Authorization.DELETE_ATTENDANCE,
		        true,
		        string.Empty,
		        Rock.Model.SpecialRole.AllAuthenticatedUsers,
		        "2A70823D-9E50-42D9-A332-A195A8D9F6ED" );
        }

        /// <summary>
        /// DV: Removes the new security auth for Delete Attendance on the Roster block.
        /// </summary>
        private void CheckInDeleteAttenanceSecurityDown()
        {
	        RockMigrationHelper.DeleteSecurityAuth( "2A70823D-9E50-42D9-A332-A195A8D9F6ED" );
        }
    }
}

// <copyright>
// Copyright by the Central Christian Church
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
using Rock.Plugin;

namespace com.centralaz.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 8, "1.6.0" )]
    public class WorkflowUpdate : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            #region Room Reservation Approval Notification

            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Complete Workflow if Reservation is Unapproved", 7, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "D9FDA437-781D-47AC-856B-14E9F779AACD" ); // Room Reservation Approval Notification:Set Attributes:Complete Workflow if Reservation is Unapproved
            RockMigrationHelper.AddActionTypeAttributeValue( "D9FDA437-781D-47AC-856B-14E9F779AACD", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Room Reservation Approval Notification:Set Attributes:Complete Workflow if Reservation is Unapproved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D9FDA437-781D-47AC-856B-14E9F779AACD", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Room Reservation Approval Notification:Set Attributes:Complete Workflow if Reservation is Unapproved:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D9FDA437-781D-47AC-856B-14E9F779AACD", "509E2CCE-3E12-499E-920A-F8535F50D6FE", @"Completed" ); // Room Reservation Approval Notification:Set Attributes:Complete Workflow if Reservation is Unapproved:Status|Status Attribute

            #endregion
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}

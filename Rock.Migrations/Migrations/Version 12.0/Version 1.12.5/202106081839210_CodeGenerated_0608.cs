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
    public partial class CodeGenerated_0608 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType: Dynamic Data:Show Launch Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Launch Workflow", "ShowLaunchWorkflow", "Show Launch Workflow", @"Show Launch Workflow button in grid footer?", 0, @"True", "EB24917C-FE59-4D80-B088-1D280F3D364B" );

            // Attribute for BlockType: Connection Request Board:Connection Request History Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connection Request History Page", "ConnectionRequestHistoryPage", "Connection Request History Page", @"Page used to display history details.", 15, @"4E237286-B715-4109-A578-C1445EC02707", "96315B2C-9072-4D8E-BCF7-EBF3FD98692F" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Show Launch Workflow Attribute for BlockType: Dynamic Data
            RockMigrationHelper.DeleteAttribute("EB24917C-FE59-4D80-B088-1D280F3D364B");

            // Connection Request History Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("96315B2C-9072-4D8E-BCF7-EBF3FD98692F");
        }
    }
}

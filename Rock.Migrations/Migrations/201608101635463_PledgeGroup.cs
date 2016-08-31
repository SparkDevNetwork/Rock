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
    public partial class PledgeGroup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialPledge", "GroupId", c => c.Int());
            CreateIndex("dbo.FinancialPledge", "GroupId");
            AddForeignKey("dbo.FinancialPledge", "GroupId", "dbo.Group", "Id");

            // Attrib for BlockType: Pledge Entry:Select Group Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Select Group Type", "SelectGroupType", "", "Optional Group Type that if selected will display a selection of groups that current user belongs to that can then be associated with the pledge", 12, @"", "E3B6ECDC-BC0D-4AC2-BD6A-86D7C196D60B" );
            // Attrib for BlockType: Pledge List:Show Group Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Column", "ShowGroupColumn", "", "Allows the grup column to be hidden.", 3, @"False", "F2476138-7C16-404C-A4B6-600E39602601" );
            // Attrib for BlockType: Pledge Detail:Select Group Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "E08504ED-B84C-4115-A058-03AAB8E8A307", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Select Group Type", "SelectGroupType", "", "Optional Group Type that if selected will display a list of groups that pledge can be associated to for selected user", 1, @"", "ADE5DE16-55C7-4C83-8174-44B57ABE3CD9" );

            // Attrib for BlockType: Person Update - Kiosk:Complete Message Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "61C5C8F2-6F76-4583-AB97-228878A6AB65", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Complete Message Lava", "CompleteMessageLava", "", "Message to display when complete.", 10, @"<div class='alert alert-success'>We have received your updated information. Thank you for helping us keep your information current.</div>", "DB16EAFA-99BE-423A-96C7-2BCB7F65AF97" );

            // JE: Set the default on the directory to show all on the page configured
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "E0799820-4319-4C46-8E88-7F136F2B928F", @"False" ); // Show All People

            // TC: Migration for the 'Group Max Member' Pull Request'
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Overcapacity Groups", "HideOvercapacityGroups", "", "When set to true, groups that are at capacity or whose default GroupTypeRole are at capacity are hidden.", 0, @"True", "B656DDCD-3C82-4483-BF2D-A1EB3DD3A9F8" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prevent Overcapacity Registrations", "PreventOvercapacityRegistrations", "", "When set to true, user cannot register for groups that are at capacity or whose default GroupTypeRole are at capacity. If only one spot is available, no spouses can be registered.", 11, @"True", "0BB1076E-EA64-41A1-A9DE-055F801007B1" );

            // JE: Update attendance analytics procs to honor DidAttend flag
            Sql( MigrationSQL._201608101635463_PledgeGroup_SP1 );
            Sql( MigrationSQL._201608101635463_PledgeGroup_SP2 );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "0BB1076E-EA64-41A1-A9DE-055F801007B1" );
            RockMigrationHelper.DeleteAttribute( "B656DDCD-3C82-4483-BF2D-A1EB3DD3A9F8" );

            DropForeignKey("dbo.FinancialPledge", "GroupId", "dbo.Group");
            DropIndex("dbo.FinancialPledge", new[] { "GroupId" });
            DropColumn("dbo.FinancialPledge", "GroupId");
        }
    }
}

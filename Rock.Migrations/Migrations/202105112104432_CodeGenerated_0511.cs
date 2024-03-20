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
    public partial class CodeGenerated_0511 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Add/Update BlockType Financial Statement Template Detail
            RockMigrationHelper.UpdateBlockType("Financial Statement Template Detail","Displays the details of the statement template.","~/Blocks/Finance/FinancialStatementTemplateDetail.ascx","Finance","230D14B7-27C1-4968-B479-7A2A784DF173");

            // Add/Update BlockType Financial Statement Template List
            RockMigrationHelper.UpdateBlockType("Financial Statement Template List","Block used to list statement templates.","~/Blocks/Finance/FinancialStatementTemplateList.ascx","Finance","E4332C68-4848-4FA5-99EC-A1CED7F136E3");

            // Attribute for BlockType: Personal Step List:Show Start Date Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Start Date Column", "ShowStartedDateColumn", "Show Start Date Column", @"Should the step start date be shown on the grid and card display?", 6, @"True", "6DA865D9-90F6-4887-B78D-896BE13B052B" );

            // Attribute for BlockType: Financial Statement Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E4332C68-4848-4FA5-99EC-A1CED7F136E3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "67768ACD-2D6E-4319-96D0-60F9444C4DCF" );

            // Attribute for BlockType: Financial Statement Template List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E4332C68-4848-4FA5-99EC-A1CED7F136E3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "D969C71E-A01E-402A-9424-4F21890F85B3" );

            // Attribute for BlockType: Financial Statement Template List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E4332C68-4848-4FA5-99EC-A1CED7F136E3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8F230C83-FB02-4BBD-A351-3627F3393102" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Financial Statement Template List
            RockMigrationHelper.DeleteAttribute("67768ACD-2D6E-4319-96D0-60F9444C4DCF");

            // core.CustomActionsConfigs Attribute for BlockType: Financial Statement Template List
            RockMigrationHelper.DeleteAttribute("8F230C83-FB02-4BBD-A351-3627F3393102");

            // Detail Page Attribute for BlockType: Financial Statement Template List
            RockMigrationHelper.DeleteAttribute("D969C71E-A01E-402A-9424-4F21890F85B3");

            // Show Start Date Column Attribute for BlockType: Personal Step List
            RockMigrationHelper.DeleteAttribute("6DA865D9-90F6-4887-B78D-896BE13B052B");

            // Delete BlockType Financial Statement Template List
            RockMigrationHelper.DeleteBlockType("E4332C68-4848-4FA5-99EC-A1CED7F136E3"); // Financial Statement Template List

            // Delete BlockType Financial Statement Template Detail
            RockMigrationHelper.DeleteBlockType("230D14B7-27C1-4968-B479-7A2A784DF173"); // Financial Statement Template Detail

        }
    }
}

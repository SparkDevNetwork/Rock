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

    /// <summary>
    /// 
    /// </summary>
    public partial class CodeGenerated_20240620 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.PersonDetail.GivingConfiguration
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.PersonDetail.GivingConfiguration", "Giving Configuration", "Rock.Blocks.Crm.PersonDetail.GivingConfiguration, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "6B977F51-4B33-44F3-A6FF-89FCC9D1AE08" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Workflow.WorkflowList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Workflow.WorkflowList", "Workflow List", "Rock.Blocks.Workflow.WorkflowList, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "1208BFDD-18CF-4539-B36B-9744B10D7635" );

            // Add/Update Obsidian Block Type
            //   Name:Giving Configuration
            //   Category:CRM > Person Detail
            //   EntityType:Rock.Blocks.Crm.PersonDetail.GivingConfiguration
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Giving Configuration", "Block used to view the scheduled transactions, saved accounts and pledges of a person.", "Rock.Blocks.Crm.PersonDetail.GivingConfiguration", "CRM > Person Detail", "BBA3A660-9A8B-4707-A553-D314C21B0A12" );

            // Add/Update Obsidian Block Type
            //   Name:Workflow List
            //   Category:Workflow
            //   EntityType:Rock.Blocks.Workflow.WorkflowList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Workflow List", "Lists all the workflows.", "Rock.Blocks.Workflow.WorkflowList", "Workflow", "EA76C61F-AA94-4E8B-B105-1EFFC0FEA59A" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Add Transaction Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBA3A660-9A8B-4707-A553-D314C21B0A12", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Transaction Page", "AddTransactionPage", "Add Transaction Page", @"", 0, @"B1CA86DC-9890-4D26-8EBD-488044E1B3DD", "1DC20543-86E5-43EA-821D-54C4E324B96B" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Person Token Expire Minutes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBA3A660-9A8B-4707-A553-D314C21B0A12", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Expire Minutes", "PersonTokenExpireMinutes", "Person Token Expire Minutes", @"The number of minutes the person token for the transaction is valid after it is issued.", 1, @"60", "CDE6B948-5AA5-4970-AB0E-4669C1FC9622" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Person Token Usage Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBA3A660-9A8B-4707-A553-D314C21B0A12", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Usage Limit", "PersonTokenUsageLimit", "Person Token Usage Limit", @"The maximum number of times the person token for the transaction can be used.", 2, @"1", "AC46767B-C8DD-4F91-8412-0EE88C768F0D" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBA3A660-9A8B-4707-A553-D314C21B0A12", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"A selection of accounts to use for checking if transactions for the current user exist.", 3, @"", "45197CB7-524F-4BC7-A4A7-296C5AB41649" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Pledge Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBA3A660-9A8B-4707-A553-D314C21B0A12", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Pledge Detail Page", "PledgeDetailPage", "Pledge Detail Page", @"", 4, @"EF7AA296-CA69-49BC-A28B-901A8AAA9466", "095B541E-529A-4C6C-9AC6-68C6D7AADA3C" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Max Years To Display
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBA3A660-9A8B-4707-A553-D314C21B0A12", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Years To Display", "MaxYearsToDisplay", "Max Years To Display", @"The maximum number of years to display (including the current year).", 5, @"3", "5D7328BB-7C2A-4822-8CC4-24BFFD475C22" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Contribution Statement Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBA3A660-9A8B-4707-A553-D314C21B0A12", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Contribution Statement Detail Page", "ContributionStatementDetailPage", "Contribution Statement Detail Page", @"The contribution statement detail page.", 6, @"98EBADAF-CCA9-4893-9DD3-D8201D8BD7FA", "405778F5-06E1-4384-8FEB-3563E2D699C2" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Scheduled Transaction Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BBA3A660-9A8B-4707-A553-D314C21B0A12", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction Detail Page", "ScheduledTransactionDetailPage", "Scheduled Transaction Detail Page", @"", 7, @"996F5541-D2E1-47E4-8078-80A388203CEC", "78DFC6C3-D7FE-474B-A256-E5EB86D54899" );

            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA76C61F-AA94-4E8B-B105-1EFFC0FEA59A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Entry Page", "EntryPage", "Entry Page", @"Page used to launch a new workflow of the selected type.", 0, @"", "18EA3A4B-93AA-4E36-A05B-B49492FC07B9" );

            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA76C61F-AA94-4E8B-B105-1EFFC0FEA59A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the workflow details.", 0, @"", "271816BC-51DC-4368-82D2-1B079491A7B3" );

            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: Default WorkflowType
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA76C61F-AA94-4E8B-B105-1EFFC0FEA59A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Default WorkflowType", "DefaultWorkflowType", "Default WorkflowType", @"The default workflow type to use. If provided the query string will be ignored.", 0, @"", "3E4A0FD6-3ED2-49EF-AF9C-0C6C22DB5907" );

            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA76C61F-AA94-4E8B-B105-1EFFC0FEA59A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "13BE5C82-14E6-4B0A-971D-55729D7BBA68" );

            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA76C61F-AA94-4E8B-B105-1EFFC0FEA59A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "86FEA954-830C-4400-89B7-BD88FAB99AAE" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "86FEA954-830C-4400-89B7-BD88FAB99AAE" );

            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "13BE5C82-14E6-4B0A-971D-55729D7BBA68" );

            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: Default WorkflowType
            RockMigrationHelper.DeleteAttribute( "3E4A0FD6-3ED2-49EF-AF9C-0C6C22DB5907" );

            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "271816BC-51DC-4368-82D2-1B079491A7B3" );

            // Attribute for BlockType
            //   BlockType: Workflow List
            //   Category: Workflow
            //   Attribute: Entry Page
            RockMigrationHelper.DeleteAttribute( "18EA3A4B-93AA-4E36-A05B-B49492FC07B9" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Scheduled Transaction Detail Page
            RockMigrationHelper.DeleteAttribute( "78DFC6C3-D7FE-474B-A256-E5EB86D54899" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Contribution Statement Detail Page
            RockMigrationHelper.DeleteAttribute( "405778F5-06E1-4384-8FEB-3563E2D699C2" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Max Years To Display
            RockMigrationHelper.DeleteAttribute( "5D7328BB-7C2A-4822-8CC4-24BFFD475C22" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Pledge Detail Page
            RockMigrationHelper.DeleteAttribute( "095B541E-529A-4C6C-9AC6-68C6D7AADA3C" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Accounts
            RockMigrationHelper.DeleteAttribute( "45197CB7-524F-4BC7-A4A7-296C5AB41649" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Person Token Usage Limit
            RockMigrationHelper.DeleteAttribute( "AC46767B-C8DD-4F91-8412-0EE88C768F0D" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Person Token Expire Minutes
            RockMigrationHelper.DeleteAttribute( "CDE6B948-5AA5-4970-AB0E-4669C1FC9622" );

            // Attribute for BlockType
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Attribute: Add Transaction Page
            RockMigrationHelper.DeleteAttribute( "1DC20543-86E5-43EA-821D-54C4E324B96B" );

            // Delete BlockType 
            //   Name: Workflow List
            //   Category: Workflow
            //   Path: -
            //   EntityType: Workflow List
            RockMigrationHelper.DeleteBlockType( "EA76C61F-AA94-4E8B-B105-1EFFC0FEA59A" );

            // Delete BlockType 
            //   Name: Giving Configuration
            //   Category: CRM > Person Detail
            //   Path: -
            //   EntityType: Giving Configuration
            RockMigrationHelper.DeleteBlockType( "BBA3A660-9A8B-4707-A553-D314C21B0A12" );
        }
    }
}

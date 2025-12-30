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
    public partial class CodeGenerated_20250805 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Connection.ConnectionOpportunitySignup
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Connection.ConnectionOpportunitySignup", "Connection Opportunity Signup", "Rock.Blocks.Connection.ConnectionOpportunitySignup, Rock.Blocks, Version=18.0.9.0, Culture=neutral, PublicKeyToken=null", false, false, "A10BF374-F97E-49FA-955C-3B22A9F31787" );

            // Add/Update Obsidian Block Type
            //   Name:Connection Opportunity Signup
            //   Category:Connection
            //   EntityType:Rock.Blocks.Connection.ConnectionOpportunitySignup
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Opportunity Signup", "Block used to sign up for a connection opportunity.", "Rock.Blocks.Connection.ConnectionOpportunitySignup", "Connection", "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F" );

            // Attribute for BlockType
            //   BlockType: Connection Request Board
            //   Category: Connection
            //   Attribute: Default Filtered Connection States
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Default Filtered Connection States", "DefaultFilteredConnectionStates", "Default Filtered Connection States", @"Specifies the default states that should be used for the Connection State Filter.", 14, @"Active", "091C34C9-FBCB-423A-9E47-AC8B6AA08B82" );

            // Attribute for BlockType
            //   BlockType: Connection Request Board
            //   Category: Connection
            //   Attribute: Default Filtered Connection Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Default Filtered Connection Statuses", "DefaultFilteredConnectionStatuses", "Default Filtered Connection Statuses", @"Specifies the default statuses that should be used for the Connection Statuses Filter.", 15, @"", "7DA7F9AB-E731-4642-8D8D-18F79A85B8E8" );

            // Attribute for BlockType
            //   BlockType: Campus List
            //   Category: Core
            //   Attribute: Show Campus Phone Number
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "52DF00E5-BC19-43F2-8533-A386DB53C74F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Phone Number", "ShowCampusPhoneNumber", "Show Campus Phone Number", @"When enabled, the Campus Phone Number column will be displayed.", 1, @"False", "E76C5EB6-8CA1-49B8-B364-D7CF619F11EC" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Allow Unrestricted Uploads
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Unrestricted Uploads", "AllowUnrestrictedUploads", "Allow Unrestricted Uploads", @"If true, anyone with access to send messages can upload images or files. Otherwise, it'll use the permissions set for Communication Attachment binary file type.", 10, @"True", "7936041A-99FB-4B50-B6F7-CA7D8B206985" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Allow Unrestricted Uploads
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Unrestricted Uploads", "AllowUnrestrictedUploads", "Allow Unrestricted Uploads", @"If true, anyone with access to send messages can upload images or files. Otherwise, it'll use the permissions set for Communication Attachment binary file type.", 11, @"True", "1AAF6B11-19D5-48FC-9DD2-5721DE016D46" );

            // Attribute for BlockType
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08233FC4-3E15-42F9-97BF-1BFF89A7A0D0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "3E265F90-363D-4E0D-B2FC-6DDBED3760FB" );

            // Attribute for BlockType
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08233FC4-3E15-42F9-97BF-1BFF89A7A0D0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "3CA6AAD6-3BC8-404E-96F5-D09DDCA7AB1B" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "B33B0C55-2C58-45F7-AE04-508500E200B8" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Display Home Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Home Phone", "DisplayHomePhone", "Display Home Phone", @"Whether to display the home phone field.", 0, @"True", "900E5756-877B-4FDA-B4A4-3C4407DFFB8D" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Display Mobile Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Mobile Phone", "DisplayMobilePhone", "Display Mobile Phone", @"Whether to display the mobile phone field.", 1, @"True", "0D72F824-7F5F-4343-943E-0E4A43A35DB0" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"Lava template to use to display the response message.", 2, @"{% include '~~/Assets/Lava/OpportunityResponseMessage.lava' %}", "211FD620-27C9-44E0-85DB-67F21E1F20F2" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Enable Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Context", "EnableCampusContext", "Enable Campus Context", @"If the page has a campus context its value will be used as a filter.", 3, @"True", "7B5C0BC7-BA15-4A22-A710-C2236DCCDEC9" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Web Prospect').", 4, @"368DD475-242C-49C4-A42C-7278BE690CC2", "D159BE3D-9D5C-4154-95F1-D0657C6AFBAE" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending').", 5, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "5885E4B7-6137-4E3C-8963-755F8ED7F318" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Connection Opportunity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "B188B729-FE6D-498B-8871-65AB8FD1E11E", "Connection Opportunity", "ConnectionOpportunity", "Connection Opportunity", @"If a Connection Opportunity is set, only details for it will be displayed (regardless of the querystring parameters).", 6, @"", "2C9B7B54-103D-4D9F-815C-8409B60CC9EE" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Include Attribute Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Include Attribute Categories", "IncludeAttributeCategories", "Include Attribute Categories", @"Attributes in these Categories will be displayed.", 7, @"", "72071F7E-9EB2-4BC2-A187-9670C05970B4" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Exclude Attribute Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Exclude Attribute Categories", "ExcludeAttributeCategories", "Exclude Attribute Categories", @"Attributes in these Categories will not be displayed.", 8, @"", "9BC3C014-C30B-4330-B64D-9FC4B7A6D807" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Exclude Non-Public Connection Request Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Exclude Non-Public Connection Request Attributes", "ExcludeNonPublicAttributes", "Exclude Non-Public Connection Request Attributes", @"Attributes without 'Public' checked will not be displayed.", 9, @"True", "3D77869F-91F3-43AC-B1CE-B6CBF3679326" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Comment Field Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Comment Field Label", "CommentFieldLabel", "Comment Field Label", @"The label to apply to the comment field.", 10, @"Comments", "8F837F49-47E8-44D6-B57F-EC168299B230" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 11, @"False", "11EEBBDE-AC6D-4B1B-9CDD-1FCB06A4D7F1" );

            // Add Block Attribute Value
            //   Block: Benevolence Request List
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Block Location: Page=Benevolence, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "EC4CFCA5-36BB-4C38-BB08-195EA2D6778A", "6BA16F29-3011-4E94-8B60-C54B305328F7", @"" );

            // Add Block Attribute Value
            //   Block: Benevolence Request List
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Block Location: Page=Benevolence, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "EC4CFCA5-36BB-4C38-BB08-195EA2D6778A", "CD331014-D5C1-486E-B1C2-7F368A7EEC14", @"False" );

            // Add Block Attribute Value
            //   Block: Benevolence Request List
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Block Location: Page=Benevolence, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "76AF9253-F5C0-48E4-83CA-901A3452FAE2", "6BA16F29-3011-4E94-8B60-C54B305328F7", @"" );

            // Add Block Attribute Value
            //   Block: Benevolence Request List
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Block Location: Page=Benevolence, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "76AF9253-F5C0-48E4-83CA-901A3452FAE2", "CD331014-D5C1-486E-B1C2-7F368A7EEC14", @"False" );

            // Add Block Attribute Value
            //   Block: Benevolence Request List
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Block Location: Page=Benevolence V1, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "8BFCDC9F-A09D-4328-9335-AE8104B719DB", "CD331014-D5C1-486E-B1C2-7F368A7EEC14", @"False" );

            // Add Block Attribute Value
            //   Block: Benevolence Request List
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Block Location: Page=Benevolence V1, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "8BFCDC9F-A09D-4328-9335-AE8104B719DB", "6BA16F29-3011-4E94-8B60-C54B305328F7", @"" );

            // Add Block Attribute Value
            //   Block: Benevolence Request List
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Block Location: Page=Assessments V1, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "DD8565DC-E089-4E69-A3F9-99DB02FE7E0D", "CD331014-D5C1-486E-B1C2-7F368A7EEC14", @"False" );

            // Add Block Attribute Value
            //   Block: Benevolence Request List
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Block Location: Page=Assessments V1, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "DD8565DC-E089-4E69-A3F9-99DB02FE7E0D", "6BA16F29-3011-4E94-8B60-C54B305328F7", @"" );

            // Add Block Attribute Value
            //   Block: Nameless Person List
            //   BlockType: Nameless Person List
            //   Category: CRM
            //   Block Location: Page=Nameless People, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "4D3C4A53-44A0-4D0B-9C1E-1C74F7C33404", "2E0C3714-5AD6-432C-A706-A3F7928389FF", @"" );

            // Add Block Attribute Value
            //   Block: Nameless Person List
            //   BlockType: Nameless Person List
            //   Category: CRM
            //   Block Location: Page=Nameless People, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "4D3C4A53-44A0-4D0B-9C1E-1C74F7C33404", "3E6459B7-1917-410D-B044-7BD564B6BBD9", @"False" );

            // Add Block Attribute Value
            //   Block: Block Type List
            //   BlockType: Block Type List
            //   Category: CMS
            //   Block Location: Page=Block Types, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "43662FA5-F51D-4B33-8CA1-DAAE5B961A61", "F2F850F4-E42D-46D8-BF76-972E33606333", @"" );

            // Add Block Attribute Value
            //   Block: Block Type List
            //   BlockType: Block Type List
            //   Category: CMS
            //   Block Location: Page=Block Types, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "43662FA5-F51D-4B33-8CA1-DAAE5B961A61", "5C5BDC8F-60EA-4E07-ACF7-1782C7AD83B2", @"False" );

            // Add Block Attribute Value
            //   Block: Campus List
            //   BlockType: Campus List
            //   Category: Core
            //   Block Location: Page=Campuses, Site=Rock RMS
            //   Attribute: Show Campus Phone Number
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "337BA5F5-4721-429E-A484-B6111DA013FB", "E76C5EB6-8CA1-49B8-B364-D7CF619F11EC", @"False" );

            // Add Block Attribute Value
            //   Block: Campus List
            //   BlockType: Campus List
            //   Category: Core
            //   Block Location: Page=Campuses, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "337BA5F5-4721-429E-A484-B6111DA013FB", "A409AEEC-00CA-44E8-AEC8-E549008A714A", @"" );

            // Add Block Attribute Value
            //   Block: Campus List
            //   BlockType: Campus List
            //   Category: Core
            //   Block Location: Page=Campuses, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "337BA5F5-4721-429E-A484-B6111DA013FB", "63E16E92-4D24-4608-A09A-EB11C83644DB", @"False" );

            // Add Block Attribute Value
            //   Block: Saved Account List
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Block Location: Page=Payment Accounts, Site=External Website
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B9D8D6B1-8719-4617-A1B6-38DD167CF1EC", "54A4B6F8-B5EA-40EC-A4CB-B5C20AF05D2D", @"" );

            // Add Block Attribute Value
            //   Block: Saved Account List
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Block Location: Page=Payment Accounts, Site=External Website
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B9D8D6B1-8719-4617-A1B6-38DD167CF1EC", "D07F74C1-AE90-47ED-944E-90500EAA5851", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "11EEBBDE-AC6D-4B1B-9CDD-1FCB06A4D7F1" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Comment Field Label
            RockMigrationHelper.DeleteAttribute( "8F837F49-47E8-44D6-B57F-EC168299B230" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Exclude Non-Public Connection Request Attributes
            RockMigrationHelper.DeleteAttribute( "3D77869F-91F3-43AC-B1CE-B6CBF3679326" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Exclude Attribute Categories
            RockMigrationHelper.DeleteAttribute( "9BC3C014-C30B-4330-B64D-9FC4B7A6D807" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Include Attribute Categories
            RockMigrationHelper.DeleteAttribute( "72071F7E-9EB2-4BC2-A187-9670C05970B4" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Connection Opportunity
            RockMigrationHelper.DeleteAttribute( "2C9B7B54-103D-4D9F-815C-8409B60CC9EE" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Record Status
            RockMigrationHelper.DeleteAttribute( "5885E4B7-6137-4E3C-8963-755F8ED7F318" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Connection Status
            RockMigrationHelper.DeleteAttribute( "D159BE3D-9D5C-4154-95F1-D0657C6AFBAE" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Enable Campus Context
            RockMigrationHelper.DeleteAttribute( "7B5C0BC7-BA15-4A22-A710-C2236DCCDEC9" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "211FD620-27C9-44E0-85DB-67F21E1F20F2" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Display Mobile Phone
            RockMigrationHelper.DeleteAttribute( "0D72F824-7F5F-4343-943E-0E4A43A35DB0" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Display Home Phone
            RockMigrationHelper.DeleteAttribute( "900E5756-877B-4FDA-B4A4-3C4407DFFB8D" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Entity Type
            RockMigrationHelper.DeleteAttribute( "B33B0C55-2C58-45F7-AE04-508500E200B8" );

            // Attribute for BlockType
            //   BlockType: Campus List
            //   Category: Core
            //   Attribute: Show Campus Phone Number
            RockMigrationHelper.DeleteAttribute( "E76C5EB6-8CA1-49B8-B364-D7CF619F11EC" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Allow Unrestricted Uploads
            RockMigrationHelper.DeleteAttribute( "7936041A-99FB-4B50-B6F7-CA7D8B206985" );

            // Attribute for BlockType
            //   BlockType: Connection Request Board
            //   Category: Connection
            //   Attribute: Default Filtered Connection Statuses
            RockMigrationHelper.DeleteAttribute( "7DA7F9AB-E731-4642-8D8D-18F79A85B8E8" );

            // Attribute for BlockType
            //   BlockType: Connection Request Board
            //   Category: Connection
            //   Attribute: Default Filtered Connection States
            RockMigrationHelper.DeleteAttribute( "091C34C9-FBCB-423A-9E47-AC8B6AA08B82" );

            // Attribute for BlockType
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "3CA6AAD6-3BC8-404E-96F5-D09DDCA7AB1B" );

            // Attribute for BlockType
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "3E265F90-363D-4E0D-B2FC-6DDBED3760FB" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Allow Unrestricted Uploads
            RockMigrationHelper.DeleteAttribute( "1AAF6B11-19D5-48FC-9DD2-5721DE016D46" );

            // Delete BlockType 
            //   Name: Connection Opportunity Signup
            //   Category: Connection
            //   Path: -
            //   EntityType: Connection Opportunity Signup
            RockMigrationHelper.DeleteBlockType( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F" );
        }
    }
}

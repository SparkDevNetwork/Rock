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
    public partial class CodeGenerated_20210921 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType: Family Select:Prioritize families for this campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prioritize families for this campus", "PrioritizeFamiliesForThisCampus", "Prioritize families for this campus", @"If enabled, families matching this kiosk's campus will appear first. Otherwise families will appear in alphabetical order regardless of their campus.", 8, @"False", "867CAFDC-43AF-4929-A8DC-900A4149B58E" );

            // Attribute for BlockType: Registration Entry:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "7D52BB01-3AC6-4B2D-8EE4-1AD26222C6A5" );

            // Attribute for BlockType: Registration Entry:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending'.)", 1, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "359A441A-EB3E-4D7A-8063-1B9D641E07B0" );

            // Attribute for BlockType: Registration Entry:Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Source", "Source", "Source", @"The Financial Source Type to use when creating transactions", 2, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "F1D05B48-31DD-4287-8515-FEF117A92BDD" );

            // Attribute for BlockType: Registration Entry:Batch Name Prefix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "Batch Name Prefix", @"The batch prefix name to use when creating a new batch", 3, @"Event Registration", "73B4361A-6787-4D1B-9B82-77A8A0263C85" );

            // Attribute for BlockType: Registration Entry:Display Progress Bar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Progress Bar", "DisplayProgressBar", "Display Progress Bar", @"Display a progress bar for the registration.", 4, @"True", "06188EF3-3232-483B-9281-CA663E8A17B0" );

            // Attribute for BlockType: Registration Entry:Allow InLine Digital Signature Documents
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow InLine Digital Signature Documents", "SignInline", "Allow InLine Digital Signature Documents", @"Should inline digital documents be allowed? This requires that the registration template is configured to display the document inline", 6, @"True", "CF29AD87-3567-4680-9B27-48209A768A0B" );

            // Attribute for BlockType: Registration Entry:Family Term
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Term", "FamilyTerm", "Family Term", @"The term to use for specifying which household or family a person is a member of.", 8, @"immediate family", "EDC4717F-727A-44DE-BAF1-ECD0FBADE37C" );

            // Attribute for BlockType: Registration Entry:Force Email Update
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Force Email Update", "ForceEmailUpdate", "Force Email Update", @"Force the email to be updated on the person's record.", 9, @"False", "68E5C3E3-B380-433D-83C6-559CA5F7658E" );

            // Attribute for BlockType: Registration Entry:Show Field Descriptions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Field Descriptions", "ShowFieldDescriptions", "Show Field Descriptions", @"Show the field description as help text", 10, @"True", "987A05CF-DADA-48ED-8852-2B281D9DA1D4" );

            // Attribute for BlockType: Registration Entry:Enabled Saved Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enabled Saved Account", "EnableSavedAccount", "Enabled Saved Account", @"Set this to false to disable the using Saved Account as a payment option, and to also disable the option to create saved account for future use.", 11, @"True", "7DCD032D-2E15-41AB-9618-F7A6DD7F97D3" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Enabled Saved Account Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("7DCD032D-2E15-41AB-9618-F7A6DD7F97D3");

            // Show Field Descriptions Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("987A05CF-DADA-48ED-8852-2B281D9DA1D4");

            // Force Email Update Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("68E5C3E3-B380-433D-83C6-559CA5F7658E");

            // Family Term Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("EDC4717F-727A-44DE-BAF1-ECD0FBADE37C");

            // Allow InLine Digital Signature Documents Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("CF29AD87-3567-4680-9B27-48209A768A0B");

            // Display Progress Bar Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("06188EF3-3232-483B-9281-CA663E8A17B0");

            // Batch Name Prefix Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("73B4361A-6787-4D1B-9B82-77A8A0263C85");

            // Source Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("F1D05B48-31DD-4287-8515-FEF117A92BDD");

            // Record Status Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("359A441A-EB3E-4D7A-8063-1B9D641E07B0");

            // Connection Status Attribute for BlockType: Registration Entry
            RockMigrationHelper.DeleteAttribute("7D52BB01-3AC6-4B2D-8EE4-1AD26222C6A5");

            // Prioritize families for this campus Attribute for BlockType: Family Select
            RockMigrationHelper.DeleteAttribute("867CAFDC-43AF-4929-A8DC-900A4149B58E");

            // Delete BlockType Field Type Gallery
            RockMigrationHelper.DeleteBlockType("424DE498-2C1E-4293-AB3C-1D9F75C911A8"); // Field Type Gallery

            // Delete BlockType Control Gallery
            RockMigrationHelper.DeleteBlockType("7DB302FF-9970-4767-B681-5D44DB807E83"); // Control Gallery

            // Delete BlockType Widget List
            RockMigrationHelper.DeleteBlockType("9D229B40-085D-44DE-96F3-4F3A79E85520"); // Widget List

            // Delete BlockType Context Group
            RockMigrationHelper.DeleteBlockType("B0DFF202-5BC4-4F2B-BE92-DC7BA3D29952"); // Context Group

            // Delete BlockType Context Entities
            RockMigrationHelper.DeleteBlockType("7D11C042-F2E1-40A0-B30D-D553AC132E1F"); // Context Entities
        }
    }
}

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
    public partial class GatewayTransactionKey : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Columns are not needed and are deleted (if they exists) in later migration
            //AddColumn( "dbo.FinancialTransaction", "GatewayTransactionKey", c => c.String(maxLength: 100));
            //CreateIndex("dbo.FinancialTransaction", "GatewayTransactionKey");

            // MP: Power BI ReportViewer,Registration Block/Page
            // Delete BlockType (it may have been incorrectly added in an earlier migration: 201710192047587_PowerBiReportViewer)
            RockMigrationHelper.DeleteBlockType( Rock.SystemGuid.BlockType.POWERBI_ACCOUNT_REGISTRATION );
            
            RockMigrationHelper.UpdateBlockType( "Power Bi Account Register", "This block registers a Power BI account for Rock to use.", "~/Blocks/Reporting/PowerBiAccountRegister.ascx", "Reporting", Rock.SystemGuid.BlockType.POWERBI_ACCOUNT_REGISTRATION );
            RockMigrationHelper.UpdateBlockType( "Power Bi Report Viewer", "This block displays a selected report from Power BI.", "~/Blocks/Reporting/PowerBiReportViewer.ascx", "Reporting", "76A64656-7BAB-4ADC-82DD-9CD207F548F9" );

            // Delete Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION that had wrong Guid (it may have been incorrectly added in an earlier migration: 201710192047587_PowerBiReportViewer)
            RockMigrationHelper.DeletePage( "B0F190FA-6BD6-4C8F-97DB-4C11A4BCDA52" );

            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION );
            RockMigrationHelper.AddPage( true, "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Power BI Registration", "", Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION, "fa fa-bar-chart" ); // Site:Rock RMS
            
            // add route to page for redirect usage
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION, "PowerBiAccountRedirect" );

            // Add Block to Page: Power BI Registration, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION, "", Rock.SystemGuid.BlockType.POWERBI_ACCOUNT_REGISTRATION, "Power Bi Account Register", "Main", @"", @"", 0, "A114F642-2D03-4A63-95FB-EA77BD90EC63" );

            // The file '~/Blocks/CheckIn/CheckInGroupTypeList.ascx' [12e586cf-db55-4654-a13e-1f825bba1c7c] does not exist
            RockMigrationHelper.DeleteBlockType( "12e586cf-db55-4654-a13e-1f825bba1c7c" );

            // The file '~/Blocks/CheckIn/CheckinConfiguration.ascx' [2506b048-f62c-4945-b09a-1e053f66c592] does not exist.
            RockMigrationHelper.DeleteBlockType( "2506b048-f62c-4945-b09a-1e053f66c592" );

            // MP: Update Default Communication Template to include template-logo
            Sql( MigrationSQL._201711011841422_GatewayTransactionKey_UpdateDefaultCommunicationTemplate );

            // MP: Communication Template Categories
            RockMigrationHelper.AddPage( true, "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Template Categories", "", "4D6DEAB3-46A0-4B27-B67B-71383EFE1171", "fa fa-folder" ); // Site:Rock RMS
                                                                                                                                                                                                                                  // Add Block to Page: Communication Template Categories, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4D6DEAB3-46A0-4B27-B67B-71383EFE1171", "", "620FC4A2-6587-409F-8972-22065919D9AC", "Communication Template Categories", "Main", @"", @"", 0, "42CC56DB-FAA0-4E51-BE03-132BCAD65AF8" );

            RockMigrationHelper.AddBlockAttributeValue( "3AF92889-C689-4EF9-A269-8EA59290DD5C", "98366AB7-0B69-40D7-99FD-92409812215D", @"1d8fd188-d641-49b6-8a2c-6194fa70af1b" );
            // Attrib Value for Block:Communication Template Categories, Attribute:Entity Type Page: Communication Template Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "42CC56DB-FAA0-4E51-BE03-132BCAD65AF8", "C405A507-7889-4287-8342-105B89710044", @"a9493afe-4316-4651-800d-5028e4c7444d" );
            // Attrib Value for Block:Communication Template Categories, Attribute:Enable Hierarchy Page: Communication Template Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "42CC56DB-FAA0-4E51-BE03-132BCAD65AF8", "736C3F4B-34CC-4B4B-9811-7171C82DDC41", @"False" );
            // Attrib Value for Block:Communication Template Categories, Attribute:Entity Qualifier Column Page: Communication Template Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "42CC56DB-FAA0-4E51-BE03-132BCAD65AF8", "65C4A655-6E1D-4504-838B-28B91FCC6DF8", @"" );
            // Attrib Value for Block:Communication Template Categories, Attribute:Entity Qualifier Value Page: Communication Template Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "42CC56DB-FAA0-4E51-BE03-132BCAD65AF8", "19A79376-3F07-45E4-95CB-5AD5D3C4DDCF", @"" );
            // Attrib Value for Block:Communication Template Categories, Attribute:core.CustomGridColumnsConfig Page: Communication Template Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "42CC56DB-FAA0-4E51-BE03-132BCAD65AF8", "E3534AE2-57FE-49F9-8CD8-DEDE0AE492CC", @"" );
            // Add 'General' category for Communication Templates
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.COMMUNICATION_TEMPLATE, "General", string.Empty, string.Empty, "A7F79054-5539-4910-A13F-AA5884B8C01D" );
            // Set 'Default' communication template to have a category of 'General'
            Sql( @"UPDATE [CommunicationTemplate]
SET CategoryId = (
 SELECT TOP 1 Id
 FROM [Category]
 WHERE [Guid] = 'A7F79054-5539-4910-A13F-AA5884B8C01D'
 )
WHERE [Guid] = '88B7DF18-9C30-4BAC-8CA2-5AD253D57E4D'" );


            // MP: Remove BlockTypes that no longer exist
            // The file '~/Blocks/CheckIn/CheckInGroupTypeList.ascx' [12e586cf-db55-4654-a13e-1f825bba1c7c] does not exist
            RockMigrationHelper.DeleteBlockType( "12e586cf-db55-4654-a13e-1f825bba1c7c" );
            // The file '~/Blocks/CheckIn/CheckinConfiguration.ascx' [2506b048-f62c-4945-b09a-1e053f66c592] does not exist.
            RockMigrationHelper.DeleteBlockType( "2506b048-f62c-4945-b09a-1e053f66c592" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // MP: Communication Template Categories
            // Remove Block: Communication Template Categories, from Page: Communication Template Categories, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "42CC56DB-FAA0-4E51-BE03-132BCAD65AF8" );
            RockMigrationHelper.DeletePage( "4D6DEAB3-46A0-4B27-B67B-71383EFE1171" ); // Page: Communication Template Categories, Layout: Full Width, Site: Rock RMS

            // MP: Power BI ReportViewer,Registration Blocks/Pages
            RockMigrationHelper.DeleteBlockType( Rock.SystemGuid.BlockType.POWERBI_ACCOUNT_REGISTRATION );

            // Remove Block: Power Bi Account Register, from Page: Power BI Registration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A114F642-2D03-4A63-95FB-EA77BD90EC63" );

            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION );
        }
    }
}

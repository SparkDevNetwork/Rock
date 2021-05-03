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
    using System.Linq;
    using System.Web.Hosting;
    using Rock.Store;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_0428 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            AddCampaignConnection();
            AddPageMapBlockSettingForSiteTypeUp();
            AzureCloudStorage();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
            AddPageMapBlockSettingForSiteTypeDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateBlockType("Add Campaign Requests","Adds Campaign Connection Requests","~/Blocks/Connection/AddCampaignRequests.ascx","Connection","BF14D093-A8F0-4FE6-8784-7D6F273A48AE");
            RockMigrationHelper.UpdateBlockType("Campaign Configuration","Block used for Campaign Connection configuration which is also used by job.","~/Blocks/Connection/CampaignConfiguration.ascx","Connection","F0401EC9-424E-4109-BBB9-529A2E075B6F");
            RockMigrationHelper.UpdateBlockType("Campaign List","Block for viewing list of campaign connection configurations.","~/Blocks/Connection/CampaignList.ascx","Connection","C5DA1E79-FCF9-4681-8EDA-0EEE559313FC");
            // Attrib for BlockType: Campaign List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C5DA1E79-FCF9-4681-8EDA-0EEE559313FC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "5E996A69-A6E2-45EB-8E93-5B4F7E91061C" );
            // Attrib for BlockType: Campaign List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C5DA1E79-FCF9-4681-8EDA-0EEE559313FC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "59D45C63-981B-4E72-8CB2-03E12CD4DB5C" );
            // Attrib for BlockType: Campaign List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C5DA1E79-FCF9-4681-8EDA-0EEE559313FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "02F6D62E-92FC-4BFE-85C2-E7FF15072032" );
            // Attrib for BlockType: Page Map:Site Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2700A1B8-BD1A-40F1-A660-476DA86D0432", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Site Type", "SiteType", "Site Type", @"Select the Site Types of the root-level pages shown in the page map. If no items are selected, all root-level pages will be shown.", 0, @"", "3E852C96-1951-4CA0-8637-B1AA2C520A2D" );
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C30F00F2-89F3-4892-BFCD-84AF2FD481A4" );
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7CB06B35-6A8F-43CA-A433-92428990D8C2" );
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E9D16CFB-E4BE-4DC6-BCA0-9D94B3BD29F1" );
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "98873E9E-6CC1-4625-A562-184906C3BF3F" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("98873E9E-6CC1-4625-A562-184906C3BF3F");
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("E9D16CFB-E4BE-4DC6-BCA0-9D94B3BD29F1");
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("7CB06B35-6A8F-43CA-A433-92428990D8C2");
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("C30F00F2-89F3-4892-BFCD-84AF2FD481A4");
            // Attrib for BlockType: Page Map:Site Type
            RockMigrationHelper.DeleteAttribute("3E852C96-1951-4CA0-8637-B1AA2C520A2D");
            // Attrib for BlockType: Campaign List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("02F6D62E-92FC-4BFE-85C2-E7FF15072032");
            // Attrib for BlockType: Campaign List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("59D45C63-981B-4E72-8CB2-03E12CD4DB5C");
            // Attrib for BlockType: Campaign List:Detail Page
            RockMigrationHelper.DeleteAttribute("5E996A69-A6E2-45EB-8E93-5B4F7E91061C");
            RockMigrationHelper.DeleteBlockType("C5DA1E79-FCF9-4681-8EDA-0EEE559313FC"); // Campaign List
            RockMigrationHelper.DeleteBlockType("F0401EC9-424E-4109-BBB9-529A2E075B6F"); // Campaign Configuration
            RockMigrationHelper.DeleteBlockType("BF14D093-A8F0-4FE6-8784-7D6F273A48AE"); // Add Campaign Requests

        }
    
        /// <summary>
        /// SK: Add Campaign Connection
        /// </summary>
        private void AddCampaignConnection()
        {
            RockMigrationHelper.AddPage( true, "9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connection Campaigns", "", "B252FAA6-0E9D-41CD-A00D-E7159E881714", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B252FAA6-0E9D-41CD-A00D-E7159E881714", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Campaign Configuration", "", "A22133B5-B5C6-455A-A300-690F7926356D", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "B252FAA6-0E9D-41CD-A00D-E7159E881714", "CampaignConfiguration", "57CEB84D-5EC7-4AE2-AA4C-CFE045F78456" );// for Page:Connection Campaigns

            RockMigrationHelper.UpdateBlockType( "Add Campaign Requests", "Adds Campaign Connection Requests", "~/Blocks/Connection/AddCampaignRequests.ascx", "Connection Campaign", "11630BB9-E685-4582-91F8-620448AA34B0" );
            RockMigrationHelper.UpdateBlockType( "Campaign Configuration", "Block used for Campaign Connection configuration which is also used by job.", "~/Blocks/Connection/CampaignConfiguration.ascx", "Connection Campaign", "9E6C4174-5F2B-4A78-9781-55D7DD209B6C" );
            RockMigrationHelper.UpdateBlockType( "Campaign List", "Block for viewing list of campaign connection configurations.", "~/Blocks/Connection/CampaignList.ascx", "Connection Campaign", "6BA9D764-E30F-48D1-AB20-8991371B6316" );

            // Add Block to Page: Connections Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "530860ED-BC73-4A43-8E7C-69533EF2B6AD".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "11630BB9-E685-4582-91F8-620448AA34B0".AsGuid(), "Add Campaign Requests", "Main", @"", @"", 0, "BF39BE49-B4F6-4A5B-BDA2-EB343FC80CCA" );
            // Add Block to Page: Connection Campaigns Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B252FAA6-0E9D-41CD-A00D-E7159E881714".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6BA9D764-E30F-48D1-AB20-8991371B6316".AsGuid(), "Campaign List", "Main", @"", @"", 0, "3A62AD36-5031-4C62-BCC1-7800AE43F78B" );
            // Add Block to Page: Campaign Configuration Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A22133B5-B5C6-455A-A300-690F7926356D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9E6C4174-5F2B-4A78-9781-55D7DD209B6C".AsGuid(), "Campaign Configuration", "Main", @"", @"", 0, "5DC2943E-EFBD-4F25-B1D7-738CB86AB628" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'BF39BE49-B4F6-4A5B-BDA2-EB343FC80CCA'" );  // Page: Connections,  Zone: Main,  Block: Add Campaign Requests
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '80710A2C-9B90-40AE-B887-B885AAA43538'" );  // Page: Connections,  Zone: Main,  Block: My Connection Opportunities

            // Attrib for BlockType: Campaign List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "6BA9D764-E30F-48D1-AB20-8991371B6316", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 0, @"", "0932CBC5-11E8-40AC-8707-F0472D1BCD31" );
            // Attrib Value for Block:Campaign List, Attribute:Detail Page Page: Connection Campaigns, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A62AD36-5031-4C62-BCC1-7800AE43F78B", "0932CBC5-11E8-40AC-8707-F0472D1BCD31", @"a22133b5-b5c6-455a-a300-690f7926356d" );
            // Attrib Value for Block:Campaign List, Attribute:core.CustomGridEnableStickyHeaders Page: Connection Campaigns, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A62AD36-5031-4C62-BCC1-7800AE43F78B", "393DAC13-0626-47B1-ADB7-C7D3B481EFB1", @"False" );

            // Add Campaign Manager Job
            Sql( @"
        IF NOT EXISTS (
            SELECT [Id]
            FROM [ServiceJob]
            WHERE [Guid] = '27D1BE06-1BC0-468D-8DE9-748BAFFF2F1A')
        BEGIN
            INSERT INTO [dbo].[ServiceJob] (
                 [IsSystem]
                ,[IsActive]
                ,[Name]
                ,[Description]
                ,[Class]
                ,[CronExpression]
                ,[NotificationStatus]
                ,[Guid]
            )
            VALUES (
                 0 
                ,1 
                ,'Campaign Manager'
                ,'Handles processing all configured campaigns, creating new connection requests and assigning them to connectors as needed.'
                ,'Rock.Jobs.CampaignManager'
                ,'0 0 7 1/1 * ? *'
                ,1
                ,'27D1BE06-1BC0-468D-8DE9-748BAFFF2F1A' )
        END" );

            Sql( @"DELETE FROM 
                        [PluginMigration]
                    WHERE
                        [PluginAssemblyName] = 'org.sparkdevnetwork.ConnectionCampaign'");


            var installedPackages = InstalledPackageService.GetInstalledPackages();
            if ( installedPackages != null && installedPackages.Any( a => a.PackageId == 129 ) )
            {
                installedPackages.RemoveAll( a => a.PackageId == 129 );
                string packageFile = HostingEnvironment.MapPath( "~/App_Data/InstalledStorePackages.json" );
                string packagesAsJson = installedPackages.ToJson();
                System.IO.File.WriteAllText( packageFile, packagesAsJson );
            }
        }

        /// <summary>
        /// DL: Add Site Type Attribute to Page Map block up.
        /// </summary>
        private void AddPageMapBlockSettingForSiteTypeUp()
        {
            // Attrib for BlockType: Page Map:Site Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2700A1B8-BD1A-40F1-A660-476DA86D0432", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Site Type", "SiteType", "Site Type", @"Select the Site Types of the root-level pages shown in the page map. If no items are selected, all root-level pages will be shown.", 0, @"", "A7020076-6EF9-4920-B736-D3B047F0FC7A" );

            // Attrib Value for Block:Site Map, Attribute:Site Type Page: Pages, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "68192536-3CE8-433B-9DF8-A895EF037FD7", "A7020076-6EF9-4920-B736-D3B047F0FC7A", $"{( int )Rock.Model.SiteType.Web}" );
        }

        /// <summary>
        /// DL: Add Site Type Attribute to Page Map block down.
        /// </summary>
        private void AddPageMapBlockSettingForSiteTypeDown()
        {
            // Attrib for BlockType: Page Map:Site Type
            RockMigrationHelper.DeleteAttribute( "A7020076-6EF9-4920-B736-D3B047F0FC7A" );
        }

        /// <summary>
        /// SK: Add Azure Cloud Storage Component Entity Type
        /// </summary>
        private void AzureCloudStorage()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Storage.AssetStorage.AzureCloudStorageComponent", "Azure Cloud Storage Component", "Rock.Storage.AssetStorage.AzureCloudStorageComponent, Rock, Version=1.11.0.14, Culture=neutral, PublicKeyToken=null", false, true, "1576800F-BFD2-4309-A2C9-AE6DF6C0A1A5" );
        }
    }
}

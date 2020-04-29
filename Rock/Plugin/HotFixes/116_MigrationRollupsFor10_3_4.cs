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

using System.Linq;
using System.Web.Hosting;
using Rock.Store;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 116, "1.10.0" )]
    public class MigrationRollupsFor10_3_4 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //AddCampaignConnection();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
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
    }
}

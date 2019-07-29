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
    public partial class ServiceJobHistory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ServiceJobHistory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServiceJobId = c.Int(nullable: false),
                        ServiceWorker = c.String(maxLength: 45),
                        StartDateTime = c.DateTime(),
                        StopDateTime = c.DateTime(),
                        Status = c.String(maxLength: 50),
                        StatusMessage = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.ServiceJob", t => t.ServiceJobId, cascadeDelete: true)
                .Index(t => t.ServiceJobId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.ServiceJob", "EnableHistory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ServiceJob", "HistoryCount", c => c.Int(nullable: false));

            // Page: Scheduled Job History
            RockMigrationHelper.AddPage( true, "C58ADA1A-6322-4998-8FED-C3565DE87EFA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Scheduled Job History", "", "B388793F-077C-4E5C-95CA-C331B00DF986", "fa fa-history" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Scheduled Job History", "Lists all scheduled job's History.", "~/Blocks/Core/ScheduledJobHistoryList.ascx", "Core", "B6E289D5-610D-4D85-83BE-B70D5B5E2EEB" );
            // Add Block to Page: Scheduled Job History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B388793F-077C-4E5C-95CA-C331B00DF986", "", "B6E289D5-610D-4D85-83BE-B70D5B5E2EEB", "Scheduled Job History", "Feature", "", "", 0, "536B8101-3B98-4707-B1F5-6A0EAFEFBF81" );
            // Attrib for BlockType: Scheduled Job History:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "B6E289D5-610D-4D85-83BE-B70D5B5E2EEB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "F483D8EA-7D3B-4F22-9543-583560D3747C" );
            // Attrib for BlockType: Scheduled Job History:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "B6E289D5-610D-4D85-83BE-B70D5B5E2EEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "5B179F04-F25B-4B43-8D1F-BD5E4CB3FA1F" );

            // Attrib Value for BlockType: Scheduled Job List
            RockMigrationHelper.AddBlockTypeAttribute( "6D3F924E-BDD0-4C78-981E-B698351E75AD", Rock.SystemGuid.FieldType.PAGE_REFERENCE, "History Page", "HistoryPage", "", @"The page to display group history.", 0, @"", "82C1A2BD-CFDA-43AF-8504-A196A8AE08AE", true );
            // Attrib Value for Page/BlockJobs Administration/Scheduled Job List:History Page (FieldType: Page Reference)
            RockMigrationHelper.AddBlockAttributeValue( "191F7008-38D7-409B-A2AF-B48A340A7C78", "82C1A2BD-CFDA-43AF-8504-A196A8AE08AE", @"b388793f-077c-4e5c-95ca-c331b00df986" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "82C1A2BD-CFDA-43AF-8504-A196A8AE08AE" );

            RockMigrationHelper.DeleteAttribute( "5B179F04-F25B-4B43-8D1F-BD5E4CB3FA1F" );
            RockMigrationHelper.DeleteAttribute( "F483D8EA-7D3B-4F22-9543-583560D3747C" );
            RockMigrationHelper.DeleteBlock( "536B8101-3B98-4707-B1F5-6A0EAFEFBF81" );
            RockMigrationHelper.DeleteBlockType( "B6E289D5-610D-4D85-83BE-B70D5B5E2EEB" );
            RockMigrationHelper.DeletePage( "B388793F-077C-4E5C-95CA-C331B00DF986" ); //  Page: Scheduled Job History

            DropForeignKey( "dbo.ServiceJobHistory", "ServiceJobId", "dbo.ServiceJob");
            DropForeignKey("dbo.ServiceJobHistory", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ServiceJobHistory", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.ServiceJobHistory", new[] { "Guid" });
            DropIndex("dbo.ServiceJobHistory", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ServiceJobHistory", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ServiceJobHistory", new[] { "ServiceJobId" });
            DropColumn("dbo.ServiceJob", "HistoryCount");
            DropColumn("dbo.ServiceJob", "EnableHistory");
            DropTable("dbo.ServiceJobHistory");
        }
    }
}

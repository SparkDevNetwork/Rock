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
    public partial class AssetStorageSystem : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AssetStorageSystem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Order = c.Int(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Description = c.String(),
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
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Asset Storage Systems", "", "7E83D0E3-97EF-4C1C-A351-1349D17401D3", "fa fa-cloud-upload" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "7E83D0E3-97EF-4C1C-A351-1349D17401D3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Asset Storage System Detail", "", "8E07FBD3-3A3D-43A0-BACF-9EE6BA09EA7B", "fa fa-cloud-upload" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Asset Manager", "", "22D51536-3292-453B-91FA-11254886058E", "fa fa-cloud" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Asset Storage System List", "Block for viewing list of asset storage systems.", "~/Blocks/Core/AssetStorageSystemList.ascx", "Core", "A3B77D14-6351-48EE-AA17-019FE0104A1C" );
            RockMigrationHelper.UpdateBlockType( "Asset Storage System Detail", "Displays the details of the given asset storage system.", "~/Blocks/Core/AssetStorageSystemDetail.ascx", "Core", "48819481-5930-4A54-B235-8351A5F8396C" );
            RockMigrationHelper.UpdateBlockType( "Asset Manager", "Manage files stored on a remote server or 3rd party cloud storage", "~/Blocks/Core/AssetStorageSystemBrowser.ascx", "Core", "B41B631C-9CF2-4BE5-ADC7-39C3F9D5AA74" );

            // Add Block to Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7E83D0E3-97EF-4C1C-A351-1349D17401D3", "", "A3B77D14-6351-48EE-AA17-019FE0104A1C", "Asset Storage System List", "Main", @"", @"", 0, "7DA622EE-0491-4425-B45F-58BAA1CEC243" );
            // Add Block to Page: Asset Storage Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8E07FBD3-3A3D-43A0-BACF-9EE6BA09EA7B", "", "48819481-5930-4A54-B235-8351A5F8396C", "Asset Storage System Detail", "Main", @"", @"", 0, "C7A19897-7A7D-4D2F-842E-5AA69EC98141" );
            // Add Block to Page: Asset Storage System Browser, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "22D51536-3292-453B-91FA-11254886058E", "", "B41B631C-9CF2-4BE5-ADC7-39C3F9D5AA74", "Asset Manager", "Main", @"", @"", 0, "700DC78A-70F7-4653-9C5D-6921760AB38F" );
            // Attrib for BlockType: Asset Storage System List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "A3B77D14-6351-48EE-AA17-019FE0104A1C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "1CCD6B98-5F4F-4BF4-8E30-B7E88E185A63" );
            // Attrib for BlockType: Asset Storage System List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "A3B77D14-6351-48EE-AA17-019FE0104A1C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", @"", 0, @"False", "82202FCB-0FCB-4A3D-B616-648242FE2C7B" );
            // Attrib for BlockType: Asset Storage System List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "A3B77D14-6351-48EE-AA17-019FE0104A1C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 0, @"", "7E223F26-070F-46B0-9133-D24502416CBE" );
            // Attrib for BlockType: Utility > Pick Something:Header Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "E2FF3015-8A2A-44D7-B50C-64D34FE63DF6", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Header Text", "HeaderText", "", @"", 0, @"", "28B51AB2-F77F-4FC3-91F3-07BD66EFC04D" );
            // Attrib Value for Block:Asset Storage System List, Attribute:Detail Page Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7DA622EE-0491-4425-B45F-58BAA1CEC243", "7E223F26-070F-46B0-9133-D24502416CBE", @"8e07fbd3-3a3d-43a0-bacf-9ee6ba09ea7b" );
            // Attrib Value for Block:Asset Storage System List, Attribute:core.CustomGridColumnsConfig Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7DA622EE-0491-4425-B45F-58BAA1CEC243", "1CCD6B98-5F4F-4BF4-8E30-B7E88E185A63", @"" );
            // Attrib Value for Block:Asset Storage System List, Attribute:core.CustomGridEnableStickyHeaders Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7DA622EE-0491-4425-B45F-58BAA1CEC243", "82202FCB-0FCB-4A3D-B616-648242FE2C7B", @"False" );
            RockMigrationHelper.UpdateFieldType( "Asset Storage System", "", "Rock", "Rock.Field.Types.AssetStorageSystemFieldType", "A57109EE-35A2-45D0-A575-02DBBDFBC0D0" );
            RockMigrationHelper.UpdateFieldType( "Asset", "", "Rock", "Rock.Field.Types.AssetFieldType", "FC27D395-8F57-467E-91FF-E648F807110E" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Utility > Pick Something:Header Text
            RockMigrationHelper.DeleteAttribute( "28B51AB2-F77F-4FC3-91F3-07BD66EFC04D" );
            // Attrib for BlockType: Asset Storage System List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.DeleteAttribute( "82202FCB-0FCB-4A3D-B616-648242FE2C7B" );
            // Attrib for BlockType: Asset Storage System List:core.CustomGridColumnsConfig
            RockMigrationHelper.DeleteAttribute( "1CCD6B98-5F4F-4BF4-8E30-B7E88E185A63" );
            // Attrib for BlockType: Asset Storage System List:Detail Page
            RockMigrationHelper.DeleteAttribute( "7E223F26-070F-46B0-9133-D24502416CBE" );
            // Remove Block: Asset Storage System File Browser, from Page: Asset Storage System Browser, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "700DC78A-70F7-4653-9C5D-6921760AB38F" );
            // Remove Block: Asset Storage System Detail, from Page: Asset Storage Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C7A19897-7A7D-4D2F-842E-5AA69EC98141" );
            // Remove Block: Asset Storage System List, from Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7DA622EE-0491-4425-B45F-58BAA1CEC243" );
            RockMigrationHelper.DeleteBlockType( "B41B631C-9CF2-4BE5-ADC7-39C3F9D5AA74" ); // Asset Storage System File Browser
            RockMigrationHelper.DeleteBlockType( "48819481-5930-4A54-B235-8351A5F8396C" ); // Asset Storage System Detail
            RockMigrationHelper.DeleteBlockType( "A3B77D14-6351-48EE-AA17-019FE0104A1C" ); // Asset Storage System List
            RockMigrationHelper.DeletePage( "22D51536-3292-453B-91FA-11254886058E" ); //  Page: Asset Storage System Browser, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "8E07FBD3-3A3D-43A0-BACF-9EE6BA09EA7B" ); //  Page: Asset Storage Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "7E83D0E3-97EF-4C1C-A351-1349D17401D3" ); //  Page: Asset Storage Systems, Layout: Full Width, Site: Rock RMS

            DropForeignKey("dbo.AssetStorageSystem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AssetStorageSystem", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.AssetStorageSystem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.AssetStorageSystem", new[] { "Guid" });
            DropIndex("dbo.AssetStorageSystem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AssetStorageSystem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.AssetStorageSystem", new[] { "EntityTypeId" });
            DropTable("dbo.AssetStorageSystem");
        }
    }
}

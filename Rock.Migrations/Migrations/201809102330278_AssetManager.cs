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
    public partial class AssetManager : Rock.Migrations.RockMigration
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

            RockMigrationHelper.UpdateEntityType( "Rock.Model.AssetStorageSystem", "Asset Storage System", "Rock.Model.AssetStorageSystem, Rock, Version=1.9.0.2, Culture=neutral, PublicKeyToken=null", true, true, "E0B4BE77-B29F-4BD4-AE45-CF833AC3A482" );
            RockMigrationHelper.UpdateEntityType( "Rock.Storage.AssetStorage.AmazonS3Component", "Amazon S3 Component", "Rock.Storage.AssetStorage.AmazonS3Component, Rock, Version=1.9.0.2, Culture=neutral, PublicKeyToken=null", false, true, "FFE9C4A0-7AB7-48CA-8938-EC73DEC134E8" );
            RockMigrationHelper.UpdateEntityType( "Rock.Storage.AssetStorage.FileSystemComponent", "File System Component", "Rock.Storage.AssetStorage.FileSystemComponent, Rock, Version=1.9.0.2, Culture=neutral, PublicKeyToken=null", false, true, "FFEA94EA-D394-4C1A-A3AE-23E6C50F047A" );

            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Asset Storage Systems", "", "1F5D5991-C586-45FC-A5AC-B7CD4D533990", "fa fa-cloud" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "1F5D5991-C586-45FC-A5AC-B7CD4D533990", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Asset Storage System Detail", "", "299751A1-EBE2-467C-8271-44BA13278331", "fa fa-cloud" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Asset Manager", "", "D2B919E2-3725-438F-8A86-AC87F81A72EB", "fa fa-folder-open" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Asset Manager", "Manage files stored on a remote server or 3rd party cloud storage", "~/Blocks/Cms/AssetManager.ascx", "Core", "13165D92-9CCD-4071-8484-3956169CB640" );
            RockMigrationHelper.UpdateBlockType( "Asset Storage System Detail", "Displays the details of the given asset storage system.", "~/Blocks/Core/AssetStorageSystemDetail.ascx", "Core", "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC" );
            RockMigrationHelper.UpdateBlockType( "Asset Storage System List", "Block for viewing list of asset storage systems.", "~/Blocks/Core/AssetStorageSystemList.ascx", "Core", "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8" );

            // Add Block to Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1F5D5991-C586-45FC-A5AC-B7CD4D533990", "", "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "Asset Storage System List", "Feature", @"", @"", 0, "0579DEB6-5E53-4295-8578-D8EC8916D84D" );
            // Add Block to Page: Asset Storage System Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "299751A1-EBE2-467C-8271-44BA13278331", "", "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC", "Asset Storage System Detail", "Feature", @"", @"", 0, "9594FC91-5FB9-465E-B2AB-18BE196CF831" );
            // Add Block to Page: Asset Manager, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D2B919E2-3725-438F-8A86-AC87F81A72EB", "", "13165D92-9CCD-4071-8484-3956169CB640", "Asset Manager", "Feature", @"<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h1 class=""panel-title""><i class=""fa fa-folder-open""></i> Asset manager</h1>
    </div>
    <div class=""panel-body"">", @"    </div>
</div>", 0, "894D8B50-FDB8-4D33-82C9-0D464CB56216" );
            // Attrib for BlockType: Asset Storage System List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 0, @"", "CEFB34E6-2C06-41DF-821C-283E2282C4DF" );
            // Attrib for BlockType: Asset Storage System List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "445F7885-1747-49B1-BF5A-98ACE237B3D2" );
            // Attrib for BlockType: Asset Storage System List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", @"", 0, @"False", "D427EC52-1F82-4C02-B4EE-FCA78D038CB0" );
            // Attrib for BlockType: Content Channel View Detail:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"Page used to view a content item.", 1, @"", "5767C5C2-533A-4F04-A4DD-AD75FCF54F7C" );
            // Attrib for BlockType: Group Map:Show Child Groups as Default
            RockMigrationHelper.UpdateBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Child Groups as Default", "ShowChildGroupsAsDefault", "", @"Defaults to showing all child groups if no user preference is set", 7, @"False", "5491D32B-BEBD-48DE-A57A-34C70F257CD3" );
            // Attrib Value for Block:Asset Storage System List, Attribute:Detail Page Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0579DEB6-5E53-4295-8578-D8EC8916D84D", "CEFB34E6-2C06-41DF-821C-283E2282C4DF", @"299751a1-ebe2-467c-8271-44ba13278331" );
            // Attrib Value for Block:Asset Storage System List, Attribute:core.CustomGridColumnsConfig Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0579DEB6-5E53-4295-8578-D8EC8916D84D", "445F7885-1747-49B1-BF5A-98ACE237B3D2", @"" );
            // Attrib Value for Block:Asset Storage System List, Attribute:core.CustomGridEnableStickyHeaders Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0579DEB6-5E53-4295-8578-D8EC8916D84D", "D427EC52-1F82-4C02-B4EE-FCA78D038CB0", @"False" );
            RockMigrationHelper.UpdateFieldType( "Asset", "", "Rock", "Rock.Field.Types.AssetFieldType", "4E4E8692-23B4-49EA-88B4-2AB07899E0EE" );
            RockMigrationHelper.UpdateFieldType( "Asset Storage System", "", "Rock", "Rock.Field.Types.AssetStorageSystemFieldType", "1596F562-E8D0-4C5F-9A00-23B5594F17E2" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //Attrib for BlockType: Group Map:Show Child Groups as Default
           RockMigrationHelper.DeleteAttribute( "5491D32B-BEBD-48DE-A57A-34C70F257CD3" );
           // Attrib for BlockType: Content Channel View Detail:Detail Page
           RockMigrationHelper.DeleteAttribute( "5767C5C2-533A-4F04-A4DD-AD75FCF54F7C" );
           // Attrib for BlockType: Asset Storage System List:core.CustomGridEnableStickyHeaders
           RockMigrationHelper.DeleteAttribute( "D427EC52-1F82-4C02-B4EE-FCA78D038CB0" );
            // Attrib for BlockType: Asset Storage System List:core.CustomGridColumnsConfig
            RockMigrationHelper.DeleteAttribute( "445F7885-1747-49B1-BF5A-98ACE237B3D2" );
            // Attrib for BlockType: Asset Storage System List:Detail Page
            RockMigrationHelper.DeleteAttribute( "CEFB34E6-2C06-41DF-821C-283E2282C4DF" );
            // Remove Block: Asset Manager, from Page: Asset Manager, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "894D8B50-FDB8-4D33-82C9-0D464CB56216" );
            // Remove Block: Asset Storage System Detail, from Page: Asset Storage System Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9594FC91-5FB9-465E-B2AB-18BE196CF831" );
            // Remove Block: Asset Storage System List, from Page: Asset Storage Systems, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0579DEB6-5E53-4295-8578-D8EC8916D84D" );
            RockMigrationHelper.DeleteBlockType( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8" ); // Asset Storage System List
            RockMigrationHelper.DeleteBlockType( "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC" ); // Asset Storage System Detail
            RockMigrationHelper.DeleteBlockType( "13165D92-9CCD-4071-8484-3956169CB640" ); // Asset Manager
            RockMigrationHelper.DeletePage( "D2B919E2-3725-438F-8A86-AC87F81A72EB" ); //  Page: Asset Manager, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "299751A1-EBE2-467C-8271-44BA13278331" ); //  Page: Asset Storage System Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "1F5D5991-C586-45FC-A5AC-B7CD4D533990" ); //  Page: Asset Storage Systems, Layout: Full Width, Site: Rock RMS

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

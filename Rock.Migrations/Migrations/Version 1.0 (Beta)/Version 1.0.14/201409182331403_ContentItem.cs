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
    public partial class ContentItem : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ContentChannel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContentTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IconCssClass = c.String(maxLength: 100),
                        RequiresApproval = c.Boolean(nullable: false),
                        EnableRss = c.Boolean(nullable: false),
                        ChannelUrl = c.String(),
                        ItemUrl = c.String(),
                        TimeToLive = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContentType", t => t.ContentTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ContentTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.ContentType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        DateRangeType = c.Byte(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.ContentItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContentChannelId = c.Int(nullable: false),
                        ContentTypeId = c.Int(nullable: false),
                        Title = c.String(maxLength: 200),
                        Content = c.String(),
                        Priority = c.Int(nullable: false),
                        Status = c.Byte(nullable: false),
                        ApprovedByPersonAliasId = c.Int(),
                        ApprovedDateTime = c.DateTime(),
                        StartDateTime = c.DateTime(nullable: false),
                        ExpireDateTime = c.DateTime(),
                        Permalink = c.String(maxLength: 200),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.ApprovedByPersonAliasId)
                .ForeignKey("dbo.ContentChannel", t => t.ContentChannelId)
                .ForeignKey("dbo.ContentType", t => t.ContentTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ContentChannelId)
                .Index(t => t.ContentTypeId)
                .Index(t => t.ApprovedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AlterColumn("dbo.WorkflowAction", "FormAction", c => c.String(maxLength: 200));

            RockMigrationHelper.AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Types", "", "37E3D602-5D7D-4818-BCAA-C67EBB301E55", "fa fa-lightbulb-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "37E3D602-5D7D-4818-BCAA-C67EBB301E55", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Type Detail", "", "91EAB2A2-4D44-4701-9ABE-37AE3E7A1B8F", "fa fa-lightbulb-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Channels", "", "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E", "fa fa-bullhorn" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Channel Detail", "", "4AE244F5-A5BF-48CF-B53B-785148EC367D", "fa fa-bullhorn" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "4AE244F5-A5BF-48CF-B53B-785148EC367D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Item Detail", "", "ABF26679-1051-4F4F-8A67-5958E5BF71F8", "" ); // Site:Rock RMS

            Sql( @"
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0
    WHERE [Guid] IN ( '91EAB2A2-4D44-4701-9ABE-37AE3E7A1B8F', '4AE244F5-A5BF-48CF-B53B-785148EC367D', 'ABF26679-1051-4F4F-8A67-5958E5BF71F8' )
" );
            RockMigrationHelper.UpdateBlockType( "Content Type Detail", "Displays the details for a content type.", "~/Blocks/Cms/ContentTypeDetail.ascx", "CMS", "451E9690-D851-4641-8BA0-317B65819918" );
            RockMigrationHelper.UpdateBlockType( "Content Type List", "Lists content types in the system.", "~/Blocks/Cms/ContentTypeList.ascx", "CMS", "A580027F-56DB-43B0-AAD6-7C2B8A952012" );
            RockMigrationHelper.UpdateBlockType( "Content Channel List", "Lists marketing campaigns.", "~/Blocks/Cms/ContentChannelList.ascx", "CMS", "991507B6-D222-45E5-BA0D-B61EA72DFB64" );
            RockMigrationHelper.UpdateBlockType( "Content Channel Detail", "Displays the details for a content channel.", "~/Blocks/Cms/ContentChannelDetail.ascx", "CMS", "B28075DA-46C1-4F6B-933D-DFCFEFB439EE" );
            RockMigrationHelper.UpdateBlockType( "Content Item List", "Lists content items.", "~/Blocks/Cms/ContentItemList.ascx", "CMS", "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE" );
            RockMigrationHelper.UpdateBlockType( "Content Item Detail", "Displays the details for a content item.", "~/Blocks/Cms/ContentItemDetail.ascx", "CMS", "5B99687B-5FE9-4EE2-8679-5040CAEB9E2E" );
            
            // Add Block to Page: Content Types, Site: Rock RMS
            RockMigrationHelper.AddBlock( "37E3D602-5D7D-4818-BCAA-C67EBB301E55", "", "A580027F-56DB-43B0-AAD6-7C2B8A952012", "Content Type List", "Main", "", "", 0, "6FC985F3-0A20-4AA4-A110-7CB11F284F49" );
            // Add Block to Page: Content Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "91EAB2A2-4D44-4701-9ABE-37AE3E7A1B8F", "", "451E9690-D851-4641-8BA0-317B65819918", "Content Type Detail", "Main", "", "", 0, "CF82EDFE-02FC-48AF-8A42-7ABF57DDFB88" );
            // Add Block to Page: Content Channel Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "4AE244F5-A5BF-48CF-B53B-785148EC367D", "", "B28075DA-46C1-4F6B-933D-DFCFEFB439EE", "Content Channel Detail", "Main", "", "", 0, "3ADDFB8F-F90C-4301-94A3-B1464BFEB817" );
            // Add Block to Page: Content Channel Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "4AE244F5-A5BF-48CF-B53B-785148EC367D", "", "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "Content Item List", "Main", "", "", 1, "98B5B613-0DDE-4C74-8B45-F634E7C2B36C" );
            // Add Block to Page: Content Item Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "ABF26679-1051-4F4F-8A67-5958E5BF71F8", "", "5B99687B-5FE9-4EE2-8679-5040CAEB9E2E", "Content Item Detail", "Main", "", "", 0, "B29047A8-EB92-40BB-AACA-F5667FD3A347" );
            // Add Block to Page: Content Channels, Site: Rock RMS
            RockMigrationHelper.AddBlock( "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E", "", "991507B6-D222-45E5-BA0D-B61EA72DFB64", "Content Channel List", "Main", "", "", 0, "D4018074-6204-4512-B33C-E4E676801864" );

            // Attrib for BlockType: Content Type List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "A580027F-56DB-43B0-AAD6-7C2B8A952012", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "08E5585C-AC01-4056-B8AD-EE4AC913D5BF" );
            // Attrib for BlockType: Content Channel List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "991507B6-D222-45E5-BA0D-B61EA72DFB64", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "0D2A443F-8A0A-44DF-B541-1BBE8DF5E4A6" );
            // Attrib for BlockType: Content Item List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "4B09294B-D660-4F56-89D5-CB7F9BD7CF84" );

            // Attrib Value for Block:Content Type List, Attribute:Detail Page Page: Content Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FC985F3-0A20-4AA4-A110-7CB11F284F49", "08E5585C-AC01-4056-B8AD-EE4AC913D5BF", @"91eab2a2-4d44-4701-9abe-37ae3e7a1b8f" );
            // Attrib Value for Block:Content Item List, Attribute:Detail Page Page: Content Channel Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "98B5B613-0DDE-4C74-8B45-F634E7C2B36C", "4B09294B-D660-4F56-89D5-CB7F9BD7CF84", @"abf26679-1051-4f4f-8a67-5958e5bf71f8" );
            // Attrib Value for Block:Content Channel List, Attribute:Detail Page Page: Content Channels, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D4018074-6204-4512-B33C-E4E676801864", "0D2A443F-8A0A-44DF-B541-1BBE8DF5E4A6", @"4ae244f5-a5bf-48cf-b53b-785148ec367d" );
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Content Item List:Detail Page
            RockMigrationHelper.DeleteAttribute( "4B09294B-D660-4F56-89D5-CB7F9BD7CF84" );
            // Attrib for BlockType: Content Channel List:Detail Page
            RockMigrationHelper.DeleteAttribute( "0D2A443F-8A0A-44DF-B541-1BBE8DF5E4A6" );
            // Attrib for BlockType: Content Type List:Detail Page
            RockMigrationHelper.DeleteAttribute( "08E5585C-AC01-4056-B8AD-EE4AC913D5BF" );

            // Remove Block: Content Channel List, from Page: Content Channels, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D4018074-6204-4512-B33C-E4E676801864" );
            // Remove Block: Content Item Detail, from Page: Content Item Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B29047A8-EB92-40BB-AACA-F5667FD3A347" );
            // Remove Block: Content Item List, from Page: Content Channel Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "98B5B613-0DDE-4C74-8B45-F634E7C2B36C" );
            // Remove Block: Content Channel Detail, from Page: Content Channel Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3ADDFB8F-F90C-4301-94A3-B1464BFEB817" );
            // Remove Block: Content - Type Detail, from Page: Content Type Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CF82EDFE-02FC-48AF-8A42-7ABF57DDFB88" );
            // Remove Block: Content Type List, from Page: Content Types, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6FC985F3-0A20-4AA4-A110-7CB11F284F49" );

            RockMigrationHelper.DeleteBlockType( "5B99687B-5FE9-4EE2-8679-5040CAEB9E2E" ); // Content Item Detail
            RockMigrationHelper.DeleteBlockType( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE" ); // Content Item List
            RockMigrationHelper.DeleteBlockType( "B28075DA-46C1-4F6B-933D-DFCFEFB439EE" ); // Content Channel Detail
            RockMigrationHelper.DeleteBlockType( "991507B6-D222-45E5-BA0D-B61EA72DFB64" ); // Content Channel List
            RockMigrationHelper.DeleteBlockType( "A580027F-56DB-43B0-AAD6-7C2B8A952012" ); // Content Type List
            RockMigrationHelper.DeleteBlockType( "451E9690-D851-4641-8BA0-317B65819918" ); // Content Type Detail

            RockMigrationHelper.DeletePage( "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E" ); //  Page: Content Channels, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "ABF26679-1051-4F4F-8A67-5958E5BF71F8" ); //  Page: Content Item Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "4AE244F5-A5BF-48CF-B53B-785148EC367D" ); //  Page: Content Channel Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "91EAB2A2-4D44-4701-9ABE-37AE3E7A1B8F" ); //  Page: Content Type Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "37E3D602-5D7D-4818-BCAA-C67EBB301E55" ); //  Page: Content Types, Layout: Full Width, Site: Rock RMS

            DropForeignKey("dbo.ContentChannel", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentItem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentItem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentItem", "ContentTypeId", "dbo.ContentType");
            DropForeignKey("dbo.ContentItem", "ContentChannelId", "dbo.ContentChannel");
            DropForeignKey("dbo.ContentItem", "ApprovedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentChannel", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentChannel", "ContentTypeId", "dbo.ContentType");
            DropForeignKey("dbo.ContentType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ContentType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.ContentItem", new[] { "Guid" });
            DropIndex("dbo.ContentItem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ContentItem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ContentItem", new[] { "ApprovedByPersonAliasId" });
            DropIndex("dbo.ContentItem", new[] { "ContentTypeId" });
            DropIndex("dbo.ContentItem", new[] { "ContentChannelId" });
            DropIndex("dbo.ContentType", new[] { "Guid" });
            DropIndex("dbo.ContentType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ContentType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ContentChannel", new[] { "Guid" });
            DropIndex("dbo.ContentChannel", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ContentChannel", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ContentChannel", new[] { "ContentTypeId" });
            AlterColumn("dbo.WorkflowAction", "FormAction", c => c.String(maxLength: 20));
            DropTable("dbo.ContentItem");
            DropTable("dbo.ContentType");
            DropTable("dbo.ContentChannel");
        }
    }
}

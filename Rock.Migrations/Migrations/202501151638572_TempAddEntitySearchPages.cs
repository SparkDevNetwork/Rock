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
    public partial class TempAddEntitySearchPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page 
            //  Internal Name: API v2 Docs
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "API v2 Docs", "", "32551448-8602-4200-9F69-BD4C04770F9F", "fa fa-diamond", "c132f1d5-9f43-4aeb-9172-cd45138b4cea" );

            // Add Page 
            //  Internal Name: Entity Search
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Entity Search", "", "40ACFFEF-B9E4-4D7E-9A20-30C2B8D66471", "fa fa-search" );

            // Add Page 
            //  Internal Name: Entity Search Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "40ACFFEF-B9E4-4D7E-9A20-30C2B8D66471", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Entity Search Detail", "", "E206A8E4-2087-4E79-B570-FA12E94BE746", "" );

            // Add Page Route
            //   Page:API v2 Docs
            //   Route:admin/power-tools/api-v2-docs
            RockMigrationHelper.AddOrUpdatePageRoute( "32551448-8602-4200-9F69-BD4C04770F9F", "admin/power-tools/api-v2-docs", "5CC24530-7542-4AB3-94EE-B45597A5DAD2" );

            // Add Page Route
            //   Page:Entity Search
            //   Route:admin/power-tools/entity-search
            RockMigrationHelper.AddOrUpdatePageRoute( "40ACFFEF-B9E4-4D7E-9A20-30C2B8D66471", "admin/power-tools/entity-search", "E1A8BE10-D1B1-47A7-9B2F-284C920493A6" );

            // Add Page Route
            //   Page:Entity Search Detail
            //   Route:admin/power-tools/entity-search/{EntitySearchId}
            RockMigrationHelper.AddOrUpdatePageRoute( "E206A8E4-2087-4E79-B570-FA12E94BE746", "admin/power-tools/entity-search/{EntitySearchId}", "A686BE9D-C508-4BD4-BEA0-F6402F01CAAE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.EntitySearchDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.EntitySearchDetail", "Entity Search Detail", "Rock.Blocks.Core.EntitySearchDetail, Rock.Blocks, Version=17.0.34.0, Culture=neutral, PublicKeyToken=null", false, false, "DB9F0335-91CB-4F89-A3BD-C084829798C6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.EntitySearchList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.EntitySearchList", "Entity Search List", "Rock.Blocks.Core.EntitySearchList, Rock.Blocks, Version=17.0.34.0, Culture=neutral, PublicKeyToken=null", false, false, "D6E8C3CE-8981-4086-B1C2-111819634BB4" );

            // Add/Update Obsidian Block Type
            //   Name:Entity Search Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.EntitySearchDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Entity Search Detail", "Displays the details of a particular entity search.", "Rock.Blocks.Core.EntitySearchDetail", "Core", "EB07313E-A0F6-4EB7-BDD1-6E5A22D456FF" );

            // Add/Update Obsidian Block Type
            //   Name:Entity Search List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.EntitySearchList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Entity Search List", "Displays a list of entity searches.", "Rock.Blocks.Core.EntitySearchList", "Core", "618265A6-1738-4B12-A9A8-153E260B8A79" );

            // Add Block 
            //  Block Name: Entity Search List
            //  Page Name: Entity Search
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "40ACFFEF-B9E4-4D7E-9A20-30C2B8D66471".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "618265A6-1738-4B12-A9A8-153E260B8A79".AsGuid(), "Entity Search List", "Main", @"", @"", 0, "3121F36C-42A8-4097-94D7-E55A8AC4C361" );

            // Add Block 
            //  Block Name: Entity Search Detail
            //  Page Name: Entity Search Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E206A8E4-2087-4E79-B570-FA12E94BE746".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "EB07313E-A0F6-4EB7-BDD1-6E5A22D456FF".AsGuid(), "Entity Search Detail", "Main", @"", @"", 0, "46D08940-FD48-4434-9BF2-95311884C3B6" );

            // Add Block 
            //  Block Name: Redirect
            //  Page Name: API v2 Docs
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "32551448-8602-4200-9F69-BD4C04770F9F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B97FB779-5D3E-4663-B3B5-3C2C227AE14A".AsGuid(), "Redirect", "Main", @"", @"", 0, "31135C01-3B65-4838-9A15-FE9A46D8F202" );

            // Attribute for BlockType
            //   BlockType: Entity Search List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "618265A6-1738-4B12-A9A8-153E260B8A79", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the entity search details.", 0, @"", "BFA1A28F-B91D-4D1C-8C18-66F7420D95BB" );

            // Add Block Attribute Value
            //   Block: Redirect
            //   BlockType: Redirect
            //   Category: CMS
            //   Block Location: Page=API v2 Docs, Site=Rock RMS
            //   Attribute: Url
            /*   Attribute Value: /api/v2/docs */
            RockMigrationHelper.AddBlockAttributeValue( "31135C01-3B65-4838-9A15-FE9A46D8F202", "964D33F4-27D0-4715-86BE-D30CEB895044", @"/api/v2/docs" );

            // Add Block Attribute Value
            //   Block: Redirect
            //   BlockType: Redirect
            //   Category: CMS
            //   Block Location: Page=API v2 Docs, Site=Rock RMS
            //   Attribute: Redirect When
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "31135C01-3B65-4838-9A15-FE9A46D8F202", "F09F2F0C-9FB0-4BC2-818C-FAD25900CF26", @"1" );

            // Add Block Attribute Value
            //   Block: Entity Search List
            //   BlockType: Entity Search List
            //   Category: Core
            //   Block Location: Page=Entity Search, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: e206a8e4-2087-4e79-b570-fa12e94be746 */
            RockMigrationHelper.AddBlockAttributeValue( "3121F36C-42A8-4097-94D7-E55A8AC4C361", "BFA1A28F-B91D-4D1C-8C18-66F7420D95BB", @"e206a8e4-2087-4e79-b570-fa12e94be746" );

            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'e206a8e4-2087-4e79-b570-fa12e94be746'" );

            // Deny all users from administering the redirect block so it will
            // actually redirect admins.
            RockMigrationHelper.AddSecurityAuthForBlock( "31135C01-3B65-4838-9A15-FE9A46D8F202",
                0,
                Security.Authorization.ADMINISTRATE,
                false,
                null,
                Model.SpecialRole.AllUsers,
                "1f55d17c-906c-4a41-bde2-e69648d475ff" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "1f55d17c-906c-4a41-bde2-e69648d475ff" );

            // Attribute for BlockType
            //   BlockType: Entity Search List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "BFA1A28F-B91D-4D1C-8C18-66F7420D95BB" );

            // Remove Block
            //  Name: Entity Search Detail, from Page: Entity Search Detail, Site: Rock RMS
            //  from Page: Entity Search Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "46D08940-FD48-4434-9BF2-95311884C3B6" );

            // Remove Block
            //  Name: Entity Search List, from Page: Entity Search, Site: Rock RMS
            //  from Page: Entity Search, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3121F36C-42A8-4097-94D7-E55A8AC4C361" );

            // Remove Block
            //  Name: Redirect, from Page: API v2 Docs, Site: Rock RMS
            //  from Page: API v2 Docs, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "31135C01-3B65-4838-9A15-FE9A46D8F202" );

            // Delete BlockType 
            //   Name: Entity Search List
            //   Category: Core
            //   Path: -
            //   EntityType: Entity Search List
            RockMigrationHelper.DeleteBlockType( "618265A6-1738-4B12-A9A8-153E260B8A79" );

            // Delete BlockType 
            //   Name: Entity Search Detail
            //   Category: Core
            //   Path: -
            //   EntityType: Entity Search Detail
            RockMigrationHelper.DeleteBlockType( "EB07313E-A0F6-4EB7-BDD1-6E5A22D456FF" );

            // Delete Page 
            //  Internal Name: Entity Search Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "E206A8E4-2087-4E79-B570-FA12E94BE746" );

            // Delete Page 
            //  Internal Name: Entity Search
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "40ACFFEF-B9E4-4D7E-9A20-30C2B8D66471" );

            // Delete Page 
            //  Internal Name: API v2 Docs
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "32551448-8602-4200-9F69-BD4C04770F9F" );
        }
    }
}

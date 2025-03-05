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
using Rock.Model;
using Rock.Security;

namespace Rock.Migrations
{
    /// <summary>
    /// This migration contains a number of steps to add the new API v2
    /// feature set to Rock.
    /// </summary>
    public partial class AddNewRestV2CrudControllers : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateEntitySearchModelUp();
            AddAdditionalSettingsToRestModelsUp();
            ConvertExistingRestV2SecurityUp();
            AddGlobalRestrictedDefaultUp();
            AddEntitySearchPagesUp();
            AddMissingDefaultSecurityToFinanceUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddMissingDefaultSecurityToFinanceDown();
            AddEntitySearchPagesDown();
            AddGlobalRestrictedDefaultDown();
            ConvertExistingRestV2SecurityDown();
            AddAdditionalSettingsToRestModelsDown();
            CreateEntitySearchModelDown();
        }

        private void CreateEntitySearchModelUp()
        {
            CreateTable(
                "dbo.EntitySearch",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    EntityTypeId = c.Int( nullable: false ),
                    Key = c.String( nullable: false, maxLength: 50 ),
                    Description = c.String(),
                    IsActive = c.Boolean( nullable: false ),
                    WhereExpression = c.String(),
                    GroupByExpression = c.String(),
                    SelectExpression = c.String(),
                    SelectManyExpression = c.String(),
                    SortExpression = c.String(),
                    MaximumResultsPerQuery = c.Int(),
                    IsEntitySecurityEnabled = c.Boolean( nullable: false ),
                    IncludePaths = c.String( maxLength: 200 ),
                    IsRefinementAllowed = c.Boolean( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.EntityTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            // Add default EXECUTE permissions for administrators on EntitySearch.
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.EntitySearch",
                Rock.SystemGuid.EntityType.ENTITY_SEARCH,
                true,
                true );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.EntitySearch",
                0,
                Authorization.EXECUTE,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "83113606-57f3-4262-b92f-f1865780e425" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.EntitySearch",
                1,
                Authorization.EXECUTE,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "3d603aed-38c6-4837-b830-bd47166de794" );
        }

        private void CreateEntitySearchModelDown()
        {
            RockMigrationHelper.DeleteSecurityAuth( "3d603aed-38c6-4837-b830-bd47166de794" );
            RockMigrationHelper.DeleteSecurityAuth( "83113606-57f3-4262-b92f-f1865780e425" );
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.ENTITY_SEARCH );

            DropForeignKey( "dbo.EntitySearch", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.EntitySearch", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.EntitySearch", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.EntitySearch", new[] { "Guid" } );
            DropIndex( "dbo.EntitySearch", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.EntitySearch", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.EntitySearch", new[] { "EntityTypeId" } );
            DropTable( "dbo.EntitySearch" );
        }

        private void AddAdditionalSettingsToRestModelsUp()
        {
            AddColumn( "dbo.RestAction", "AdditionalSettingsJson", c => c.String() );
            AddColumn( "dbo.RestController", "AdditionalSettingsJson", c => c.String() );
        }

        private void AddAdditionalSettingsToRestModelsDown()
        {
            DropColumn( "dbo.RestController", "AdditionalSettingsJson" );
            DropColumn( "dbo.RestAction", "AdditionalSettingsJson" );
        }

        private void ConvertExistingRestV2SecurityUp()
        {
            // API v2 endpoints are switching from "View" and "Edit" to
            // "ExecuteRead" and "ExecuteWrite". There are also some special
            // cases that need to be handled.
            Sql( @"
UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'View' THEN 'ExecuteRead'
        WHEN [A].[Action] = 'Edit' THEN 'ExecuteWrite'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestController] AS [RC] ON [RC].[Id] = [A].[EntityId]
WHERE [ET].[Name] = 'Rock.Model.RestController'
  AND [RC].[ClassName] LIKE 'Rock.Rest.v2.%'

UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'FullSearch' THEN 'ExecuteUnrestrictedRead'
        WHEN [A].[Action] = 'Edit' THEN 'ExecuteRead'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestAction] AS [RA] ON [RA].[Id] = [A].[EntityId]
WHERE [ET].[Name] = 'Rock.Model.RestAction'
  AND [RA].[Guid] = 'dca338b6-9749-427e-8238-1686c9587d16'

UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'View' THEN 'ExecuteRead'
        WHEN [A].[Action] = 'Edit' THEN 'ExecuteWrite'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestAction] AS [RA] ON [RA].[Id] = [A].[EntityId]
LEFT OUTER JOIN [RestController] AS [RC] ON [RC].[Id] = [RA].[ControllerId]
WHERE [ET].[Name] = 'Rock.Model.RestAction'
  AND [RC].[ClassName] LIKE 'Rock.Rest.v2.%'" );
        }

        private void ConvertExistingRestV2SecurityDown()
        {
            Sql( @"
UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'ExecuteRead' THEN 'View'
        WHEN [A].[Action] = 'ExecuteWrite' THEN 'Edit'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestController] AS [RC] ON [RC].[Id] = [A].[EntityId]
WHERE [ET].[Name] = 'Rock.Model.RestController'
  AND [RC].[ClassName] LIKE 'Rock.Rest.v2.%'

UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'ExecuteUnrestrictedRead' THEN 'FullSearch'
        WHEN [A].[Action] = 'ExecuteRead' THEN 'Edit'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestAction] AS [RA] ON [RA].[Id] = [A].[EntityId]
WHERE [ET].[Name] = 'Rock.Model.RestAction'
  AND [RA].[Guid] = 'dca338b6-9749-427e-8238-1686c9587d16'

UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'View' THEN 'ExecuteRead'
        WHEN [A].[Action] = 'Edit' THEN 'ExecuteWrite'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestAction] AS [RA] ON [RA].[Id] = [A].[EntityId]
LEFT OUTER JOIN [RestController] AS [RC] ON [RC].[Id] = [RA].[ControllerId]
WHERE [ET].[Name] = 'Rock.Model.RestAction'
  AND [RC].[ClassName] LIKE 'Rock.Rest.v2.%'" );
        }

        private void AddGlobalRestrictedDefaultUp()
        {
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Security.GlobalRestrictedDefault",
                "0a326a00-6f00-45b8-bb39-ec6ebe53ce88",
                false,
                true );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Security.GlobalRestrictedDefault",
                0,
                Authorization.ADMINISTRATE,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "7c01fdb4-7e70-45c5-9f51-ef3b45f5b9f5" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Security.GlobalRestrictedDefault",
                1,
                Authorization.ADMINISTRATE,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "5266bd3b-bcf2-433a-82c7-eff06f430d3c" );

            // Add additional security auths to the Check-in Controller to mimic
            // what it was before becoming restricted by default.
            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                SpecialRole.None,
                "7ed56e29-b07c-46d7-b7ad-1d2b54d6fd8a" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                SpecialRole.None,
                "98b917ed-0d9a-4d0e-bd5d-11b95e96c90f" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "06787034-6808-4b29-983f-8ba30b3773c2" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                false,
                null,
                SpecialRole.AllUsers,
                "183659f2-ac00-420d-8813-6e3c59c59a9c" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                SpecialRole.None,
                "5fccefae-cb5a-48a2-ab50-5f0901df47ca" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                SpecialRole.None,
                "b0be618d-fb3f-4890-8af5-734b27e7bc5d" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "5880ef39-f135-4760-a69f-9568edd8f2f1" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                false,
                null,
                SpecialRole.AllUsers,
                "735acdc1-1922-484c-baf7-3e8b53abfdcf" );

            // Add additional security auths to the Controls controller
            // to mimic what it was before becoming restricted by default.
            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                SpecialRole.None,
                "63b1a279-5637-4133-89c1-5b51d2a28155" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                SpecialRole.None,
                "0b7c8e65-1094-4032-a49d-d42209a62210" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "951f95b7-81bc-4cd0-95b1-8e0438a59c96" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                false,
                null,
                SpecialRole.AllUsers,
                "8ed5b612-4a31-4167-8bf5-79b666a34aef" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                SpecialRole.None,
                "6d303c10-ef8f-4901-8b85-334f4ccec44d" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                SpecialRole.None,
                "313bccf7-fbba-411a-96bc-52814703f612" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "72ec0496-1066-407e-8d96-40490af6ddfb" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                false,
                null,
                SpecialRole.AllUsers,
                "539f0406-fcad-4f1c-b9b8-c9e24f3ee06a" );
        }

        private void AddGlobalRestrictedDefaultDown()
        {
            // Controls Controller
            RockMigrationHelper.DeleteSecurityAuth( "539f0406-fcad-4f1c-b9b8-c9e24f3ee06a" );
            RockMigrationHelper.DeleteSecurityAuth( "72ec0496-1066-407e-8d96-40490af6ddfb" );
            RockMigrationHelper.DeleteSecurityAuth( "313bccf7-fbba-411a-96bc-52814703f612" );
            RockMigrationHelper.DeleteSecurityAuth( "6d303c10-ef8f-4901-8b85-334f4ccec44d" );
            RockMigrationHelper.DeleteSecurityAuth( "8ed5b612-4a31-4167-8bf5-79b666a34aef" );
            RockMigrationHelper.DeleteSecurityAuth( "951f95b7-81bc-4cd0-95b1-8e0438a59c96" );
            RockMigrationHelper.DeleteSecurityAuth( "0b7c8e65-1094-4032-a49d-d42209a62210" );
            RockMigrationHelper.DeleteSecurityAuth( "63b1a279-5637-4133-89c1-5b51d2a28155" );

            // Check-in Controller
            RockMigrationHelper.DeleteSecurityAuth( "735acdc1-1922-484c-baf7-3e8b53abfdcf" );
            RockMigrationHelper.DeleteSecurityAuth( "5880ef39-f135-4760-a69f-9568edd8f2f1" );
            RockMigrationHelper.DeleteSecurityAuth( "b0be618d-fb3f-4890-8af5-734b27e7bc5d" );
            RockMigrationHelper.DeleteSecurityAuth( "5fccefae-cb5a-48a2-ab50-5f0901df47ca" );
            RockMigrationHelper.DeleteSecurityAuth( "183659f2-ac00-420d-8813-6e3c59c59a9c" );
            RockMigrationHelper.DeleteSecurityAuth( "06787034-6808-4b29-983f-8ba30b3773c2" );
            RockMigrationHelper.DeleteSecurityAuth( "98b917ed-0d9a-4d0e-bd5d-11b95e96c90f" );
            RockMigrationHelper.DeleteSecurityAuth( "7ed56e29-b07c-46d7-b7ad-1d2b54d6fd8a" );

            // GlobalRestrictedDefault
            RockMigrationHelper.DeleteSecurityAuth( "5266bd3b-bcf2-433a-82c7-eff06f430d3c" );
            RockMigrationHelper.DeleteSecurityAuth( "7c01fdb4-7e70-45c5-9f51-ef3b45f5b9f5" );
        }

        private void AddEntitySearchPagesUp()
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

        private void AddEntitySearchPagesDown()
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

        private void AddMissingDefaultSecurityToFinanceUp()
        {
            // Default Security for FinancialPaymentDetail
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "02476fa2-6ba0-4364-833f-cabd7931a0c8" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                1,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "a0ce3e2e-684a-430d-bc85-b5517bdcd93d" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                2,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                0,
                "a55f551c-8591-4dfc-8018-9eb5eee939cc" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                3,
                Security.Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "971dc09b-f934-4238-9d83-e4754ae9343b" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "33ea051b-a040-495d-92a2-7bd0aaeac5bf" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                1,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "f9370185-7790-40ae-b087-4e6e03745c13" );

            // Default Security for FinancialStatementTemplate
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "010533a5-0446-4ce2-8d4e-d3853966a9ff" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                1,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "a255c49f-97cf-4ba4-9c09-1ef38a44aa4e" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                2,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                0,
                "f664b1af-3e89-4bdb-9539-8951b03e8b89" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                3,
                Security.Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "443b13b9-55c4-4183-ab68-73a100f45777" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "a2ce65ff-89e8-4f9c-8c8c-8ae72dbbd910" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                1,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "3a860c98-df3e-419e-96e3-4e898f0da07a" );

            // Default Security for FinancialTransactionAlertType
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "fa22c42a-1ade-40a7-8b98-8ab02f8ba009" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                1,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "be113be9-b026-4fa2-843b-324637aece50" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                2,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                0,
                "20f45bb5-6960-4749-a9c7-69f60c34ea75" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                3,
                Security.Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "ff39c885-909c-43e4-ac7a-d8e1f862b486" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "dad3a6b4-cac5-4eed-a79b-3dd518b65fa9" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                1,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "b1b8a1ce-fdf0-45b2-a5c4-6d19ed9b3c66" );

            // Default Security for FinancialTransactionAlert
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "c7579205-b54b-4429-9f9b-7bf30ef88532" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                1,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "78b1c098-3498-449f-93cb-f83bd896fadf" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                2,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                0,
                "247e9bb5-4756-43f6-927f-2499f1eb1549" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                3,
                Security.Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "44d46714-855d-4c2a-9c4f-2a05d07e98e7" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "6e530bba-8643-4917-bebb-eda7bb7e89a2" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                1,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "e6c1e0d9-38b0-47dd-a6c8-4017eeeceb6d" );
        }

        private void AddMissingDefaultSecurityToFinanceDown()
        {
            // Default Security for FinancialTransactionAlert
            RockMigrationHelper.DeleteSecurityAuth( "e6c1e0d9-38b0-47dd-a6c8-4017eeeceb6d" );
            RockMigrationHelper.DeleteSecurityAuth( "6e530bba-8643-4917-bebb-eda7bb7e89a2" );
            RockMigrationHelper.DeleteSecurityAuth( "44d46714-855d-4c2a-9c4f-2a05d07e98e7" );
            RockMigrationHelper.DeleteSecurityAuth( "247e9bb5-4756-43f6-927f-2499f1eb1549" );
            RockMigrationHelper.DeleteSecurityAuth( "78b1c098-3498-449f-93cb-f83bd896fadf" );
            RockMigrationHelper.DeleteSecurityAuth( "c7579205-b54b-4429-9f9b-7bf30ef88532" );

            // Default Security for FinancialTransactionAlertType
            RockMigrationHelper.DeleteSecurityAuth( "b1b8a1ce-fdf0-45b2-a5c4-6d19ed9b3c66" );
            RockMigrationHelper.DeleteSecurityAuth( "dad3a6b4-cac5-4eed-a79b-3dd518b65fa9" );
            RockMigrationHelper.DeleteSecurityAuth( "ff39c885-909c-43e4-ac7a-d8e1f862b486" );
            RockMigrationHelper.DeleteSecurityAuth( "20f45bb5-6960-4749-a9c7-69f60c34ea75" );
            RockMigrationHelper.DeleteSecurityAuth( "be113be9-b026-4fa2-843b-324637aece50" );
            RockMigrationHelper.DeleteSecurityAuth( "fa22c42a-1ade-40a7-8b98-8ab02f8ba009" );

            // Default Security for FinancialStatementTemplate
            RockMigrationHelper.DeleteSecurityAuth( "3a860c98-df3e-419e-96e3-4e898f0da07a" );
            RockMigrationHelper.DeleteSecurityAuth( "a2ce65ff-89e8-4f9c-8c8c-8ae72dbbd910" );
            RockMigrationHelper.DeleteSecurityAuth( "443b13b9-55c4-4183-ab68-73a100f45777" );
            RockMigrationHelper.DeleteSecurityAuth( "f664b1af-3e89-4bdb-9539-8951b03e8b89" );
            RockMigrationHelper.DeleteSecurityAuth( "a255c49f-97cf-4ba4-9c09-1ef38a44aa4e" );
            RockMigrationHelper.DeleteSecurityAuth( "010533a5-0446-4ce2-8d4e-d3853966a9ff" );

            // Default Security for FinancialPaymentDetail
            RockMigrationHelper.DeleteSecurityAuth( "f9370185-7790-40ae-b087-4e6e03745c13" );
            RockMigrationHelper.DeleteSecurityAuth( "33ea051b-a040-495d-92a2-7bd0aaeac5bf" );
            RockMigrationHelper.DeleteSecurityAuth( "971dc09b-f934-4238-9d83-e4754ae9343b" );
            RockMigrationHelper.DeleteSecurityAuth( "a55f551c-8591-4dfc-8018-9eb5eee939cc" );
            RockMigrationHelper.DeleteSecurityAuth( "a0ce3e2e-684a-430d-bc85-b5517bdcd93d" );
            RockMigrationHelper.DeleteSecurityAuth( "02476fa2-6ba0-4364-833f-cabd7931a0c8" );
        }
    }
}

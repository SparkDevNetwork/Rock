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
    public partial class AssetManagerRefactor : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable(name: "dbo.AssetStorageSystem", newName: "AssetStorageProvider");

            RockMigrationHelper.UpdateEntityType( "Rock.Model.AssetStorageProvider", "Asset Storage Provider", "Rock.Model.AssetStorageProvider, Rock, Version=1.9.0.2, Culture=neutral, PublicKeyToken=null", true, true, "E0B4BE77-B29F-4BD4-AE45-CF833AC3A482" );
            RockMigrationHelper.UpdateEntityType( "Rock.Storage.AssetStorage.AmazonS3Component", "Amazon S3 Component", "Rock.Storage.AssetStorage.AmazonS3Component, Rock, Version=1.9.0.2, Culture=neutral, PublicKeyToken=null", false, true, "FFE9C4A0-7AB7-48CA-8938-EC73DEC134E8" );
            RockMigrationHelper.UpdateEntityType( "Rock.Storage.AssetStorage.FileSystemComponent", "File System Component", "Rock.Storage.AssetStorage.FileSystemComponent, Rock, Version=1.9.0.2, Culture=neutral, PublicKeyToken=null", false, true, "FFEA94EA-D394-4C1A-A3AE-23E6C50F047A" );

            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Asset Storage Providers", "", "1F5D5991-C586-45FC-A5AC-B7CD4D533990", "fa fa-cloud" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "1F5D5991-C586-45FC-A5AC-B7CD4D533990", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Asset Storage Provider Detail", "", "299751A1-EBE2-467C-8271-44BA13278331", "fa fa-cloud" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Asset Manager", "", "D2B919E2-3725-438F-8A86-AC87F81A72EB", "fa fa-folder-open" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Asset Manager", "Manage files stored on a remote server or 3rd party cloud storage", "~/Blocks/Cms/AssetManager.ascx", "Core", "13165D92-9CCD-4071-8484-3956169CB640" );
            RockMigrationHelper.UpdateBlockType( "Asset Storage Provider Detail", "Displays the details of the given asset storage provider.", "~/Blocks/Core/AssetStorageProviderDetail.ascx", "Core", "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC" );
            RockMigrationHelper.UpdateBlockType( "Asset Storage Provider List", "Block for viewing list of asset storage providers.", "~/Blocks/Core/AssetStorageProviderList.ascx", "Core", "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8" );

            // Add Block to Page: Asset Storage Providers, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1F5D5991-C586-45FC-A5AC-B7CD4D533990", "", "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "Asset Storage Provider List", "Feature", @"", @"", 0, "0579DEB6-5E53-4295-8578-D8EC8916D84D" );
            // Add Block to Page: Asset Storage Provider Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "299751A1-EBE2-467C-8271-44BA13278331", "", "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC", "Asset Storage Provider Detail", "Feature", @"", @"", 0, "9594FC91-5FB9-465E-B2AB-18BE196CF831" );
            // Add Block to Page: Asset Manager, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D2B919E2-3725-438F-8A86-AC87F81A72EB", "", "13165D92-9CCD-4071-8484-3956169CB640", "Asset Manager", "Feature", @"<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h1 class=""panel-title""><i class=""fa fa-folder-open""></i> Asset Manager</h1>
    </div>
    <div class=""panel-body"">", @"    </div>
</div>", 0, "894D8B50-FDB8-4D33-82C9-0D464CB56216" );
            // Attrib for BlockType: Asset Storage Provider List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 0, @"", "CEFB34E6-2C06-41DF-821C-283E2282C4DF" );
            // Attrib for BlockType: Asset Storage Provider List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "445F7885-1747-49B1-BF5A-98ACE237B3D2" );
            // Attrib for BlockType: Asset Storage Provider List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", @"", 0, @"False", "D427EC52-1F82-4C02-B4EE-FCA78D038CB0" );
            // Attrib for BlockType: Content Channel View Detail:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"Page used to view a content item.", 1, @"", "5767C5C2-533A-4F04-A4DD-AD75FCF54F7C" );
            // Attrib for BlockType: Group Map:Show Child Groups as Default
            RockMigrationHelper.UpdateBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Child Groups as Default", "ShowChildGroupsAsDefault", "", @"Defaults to showing all child groups if no user preference is set", 7, @"False", "5491D32B-BEBD-48DE-A57A-34C70F257CD3" );
            // Attrib Value for Block:Asset Storage Provider List, Attribute:Detail Page Page: Asset Storage Providers, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0579DEB6-5E53-4295-8578-D8EC8916D84D", "CEFB34E6-2C06-41DF-821C-283E2282C4DF", @"299751a1-ebe2-467c-8271-44ba13278331" );
            // Attrib Value for Block:Asset Storage Provider List, Attribute:core.CustomGridColumnsConfig Page: Asset Storage Providers, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0579DEB6-5E53-4295-8578-D8EC8916D84D", "445F7885-1747-49B1-BF5A-98ACE237B3D2", @"" );
            // Attrib Value for Block:Asset Storage Provider List, Attribute:core.CustomGridEnableStickyHeaders Page: Asset Storage Providers, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0579DEB6-5E53-4295-8578-D8EC8916D84D", "D427EC52-1F82-4C02-B4EE-FCA78D038CB0", @"False" );
            RockMigrationHelper.UpdateFieldType( "Asset", "", "Rock", "Rock.Field.Types.AssetFieldType", "4E4E8692-23B4-49EA-88B4-2AB07899E0EE" );
            RockMigrationHelper.UpdateFieldType( "Asset Storage Provider", "", "Rock", "Rock.Field.Types.AssetStorageProviderFieldType", "1596F562-E8D0-4C5F-9A00-23B5594F17E2" );

            AddLocalContentAsset();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group Map:Show Child Groups as Default
            RockMigrationHelper.DeleteAttribute( "5491D32B-BEBD-48DE-A57A-34C70F257CD3" );
            // Attrib for BlockType: Content Channel View Detail:Detail Page
            RockMigrationHelper.DeleteAttribute( "5767C5C2-533A-4F04-A4DD-AD75FCF54F7C" );
            // Attrib for BlockType: Asset Storage Provider List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.DeleteAttribute( "D427EC52-1F82-4C02-B4EE-FCA78D038CB0" );
            // Attrib for BlockType: Asset Storage Provider List:core.CustomGridColumnsConfig
            RockMigrationHelper.DeleteAttribute( "445F7885-1747-49B1-BF5A-98ACE237B3D2" );
            // Attrib for BlockType: Asset Storage Provider List:Detail Page
            RockMigrationHelper.DeleteAttribute( "CEFB34E6-2C06-41DF-821C-283E2282C4DF" );
            // Remove Block: Asset Manager, from Page: Asset Manager, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "894D8B50-FDB8-4D33-82C9-0D464CB56216" );
            // Remove Block: Asset Storage Provider Detail, from Page: Asset Storage Provider Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9594FC91-5FB9-465E-B2AB-18BE196CF831" );
            // Remove Block: Asset Storage Provider List, from Page: Asset Storage Provider, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0579DEB6-5E53-4295-8578-D8EC8916D84D" );
            RockMigrationHelper.DeleteBlockType( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8" ); // Asset Storage Provider List
            RockMigrationHelper.DeleteBlockType( "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC" ); // Asset Storage Provider Detail
            RockMigrationHelper.DeleteBlockType( "13165D92-9CCD-4071-8484-3956169CB640" ); // Asset Manager
            RockMigrationHelper.DeletePage( "D2B919E2-3725-438F-8A86-AC87F81A72EB" ); //  Page: Asset Manager, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "299751A1-EBE2-467C-8271-44BA13278331" ); //  Page: Asset Storage Provider Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "1F5D5991-C586-45FC-A5AC-B7CD4D533990" ); //  Page: Asset Storage Providers, Layout: Full Width, Site: Rock RMS

            RenameTable(name: "dbo.AssetStorageProvider", newName: "AssetStorageSystem");
        }

        private void AddLocalContentAsset()
        {
            Sql( @"-- INSERT an initial AssetStorageProvider with local content

                -- Get the entity and field types
                DECLARE @assetEntityTypeId INT = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = 'E0B4BE77-B29F-4BD4-AE45-CF833AC3A482')
                DECLARE @fsComponentEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Storage.AssetStorage.FileSystemComponent')
                DECLARE @textFieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')
				DECLARE @assetId INT = 0
				DECLARE @attributeId INT = 0

                -- Insert the row for Local Content Asset Storage Provider
                IF ( SELECT COUNT(*) FROM [dbo].[AssetStorageProvider] WHERE [Name] = 'Local Content') = 0
				BEGIN
					INSERT INTO [dbo].[AssetStorageProvider]([Name], [Order], [EntityTypeId], [IsActive], [guid])
					VALUES('Local Content', 0, @fsComponentEntityTypeId, 1, NEWID())
					SET @assetId = (SELECT SCOPE_IDENTITY())
				END
                ELSE BEGIN
					SET @assetId = (SELECT [Id] FROM [dbo].[AssetStorageProvider] WHERE [Name] = 'Local Content')
				END

				IF ( SELECT COUNT(*) FROM [dbo].[Attribute] WHERE [Guid] = '75763D22-2A6D-4215-9056-8A4D38C2DE43') = 0
				BEGIN
					-- Create attribute now instead of when the component is initialized in the code.
					INSERT INTO [dbo].[Attribute]([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [IconCssClass], [AllowSearch], [ForeignGuid], [ForeignId], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory], [IsActive], [EnableHistory])
					SELECT 
						  0 -- IsSystem
						, @textFieldTypeId -- FieldTypeId
						, @assetEntityTypeId -- [EntityTypeId]
						, 'EntityTypeId' -- [EntityTypeQualifierColumn]
						, @fsComponentEntityTypeId -- 
						, 'RootFolder' -- 
						, 'Root Folder' -- 
						,  '' -- [Description]
						,  0 -- [Order]
						,  0 -- [IsGridColumn]
						,  '~/Content' -- [DefaultValue]
						,  0 -- [IsMultiValue]
						,  1 -- [IsRequired]
						,  '75763D22-2A6D-4215-9056-8A4D38C2DE43' -- [Guid]
						,  NULL -- [CreatedDateTime]
						,  NULL -- [ModifiedDateTime]
						,  NULL -- [CreatedByPersonAliasId]
						,  NULL -- [ModifiedByPersonAliasId]
						,  NULL -- [ForeignKey]
						,  '' -- [IconCssClass]
						,  0 -- [AllowSearch]
						,  NULL -- [ForeignGuid]
						,  NULL -- [ForeignId]
						,  0 -- [IsIndexEnabled]
						,  0 -- [IsAnalytic]
						,  0 -- [IsAnalyticHistory]
						,  1 -- [IsActive]
						,  0 -- [EnableHistory]
				END
				
				IF (SELECT COUNT(*) FROM [dbo].[AttributeValue] WHERE [Guid] = '7F593536-CD1A-4956-8203-7938D8D9512D') = 0
				BEGIN

					SET @attributeId = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '75763D22-2A6D-4215-9056-8A4D38C2DE43')

					-- Create a value for the new attribute since we will not be using the default
					INSERT INTO [dbo].[AttributeValue]([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId])
					SELECT
						  0 -- [IsSystem]
						, @attributeId -- [AttributeId]
						, @assetId -- [EntityId] In this case the ID of the AssetStorageProvider Local Content
						, '~/Content/' -- [Value]
						, '7F593536-CD1A-4956-8203-7938D8D9512D' -- [Guid]
						, NULL -- [CreatedDateTime]
						, NULL -- [ModifiedDateTime]
						, NULL -- [CreatedByPersonAliasId]
						, NULL -- [ModifiedByPersonAliasId]
						, NULL -- [ForeignKey]
						, NULL -- [ForeignGuid]
						, NULL -- [ForeignId]
				END" );
        }
    }
}

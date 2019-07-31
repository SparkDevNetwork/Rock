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

            // Rename AssetStorageSystem to AssetStorageProvider in EntityType
            RenameAssetStorageEntity( "Rock.Model.AssetStorageProvider", "Rock.Model.AssetStorageProvider", "Asset Storage Provider" );

            // Update Page info to use the name Asset Storage Provider
            UpdateAssetStoragePage( "Asset Storage Providers", "Asset Storage Providers", "Asset Storage Providers", "1F5D5991-C586-45FC-A5AC-B7CD4D533990" );
            UpdateAssetStoragePage( "Asset Storage Provider Detail", "Asset Storage Provider Detail", "Asset Storage Provider Detail", "299751A1-EBE2-467C-8271-44BA13278331" );

            // Update BlockType to use the name Asset Strage Provider and point to the new paths
            RockMigrationHelper.UpdateBlockTypeByGuid( "Asset Storage Provider Detail", "Displays the details of the given asset storage provider.", "~/Blocks/Core/AssetStorageProviderDetail.ascx", "Core", "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Asset Storage Provider List", "Block for viewing list of asset storage providers.", "~/Blocks/Core/AssetStorageProviderList.ascx", "Core", "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8" );

            // Correct casing in prehtml, this doesn't need a down
            Sql( @"UPDATE [dbo].[Block]
SET PreHtml = '<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h1 class=""panel-title""><i class=""fa fa-folder-open""></i> Asset Manager</h1>
    </div>
    <div class=""panel-body"">'
WHERE[Guid] = '894D8B50-FDB8-4D33-82C9-0D464CB56216'" );

            // Update blocks with the name Asset Storage Provider
            UdpateAssetStorageBlock( "Asset Storage Provider List", "0579DEB6-5E53-4295-8578-D8EC8916D84D" );
            UdpateAssetStorageBlock( "Asset Storage Provider Detail", "9594FC91-5FB9-465E-B2AB-18BE196CF831" );

            // Update Asset Storage FieldType to use the new name and class name
            UpdateAssetStorageFieldType( "Asset Storage Provider", "Rock.Field.Types.AssetStorageProviderFieldType", "1596F562-E8D0-4C5F-9A00-23B5594F17E2" );

            AddLocalContentAsset();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateAssetStorageFieldType( "Asset Storage System", "Rock.Field.Types.AssetStorageSystemFieldType", "1596F562-E8D0-4C5F-9A00-23B5594F17E2" );

            UdpateAssetStorageBlock( "Asset Storage System List", "0579DEB6-5E53-4295-8578-D8EC8916D84D" );
            UdpateAssetStorageBlock( "Asset Storage System Detail", "9594FC91-5FB9-465E-B2AB-18BE196CF831" );

            RockMigrationHelper.UpdateBlockTypeByGuid( "Asset Storage System Detail", "Displays the details of the given asset storage system.", "~/Blocks/Core/AssetStorageSystemDetail.ascx", "Core", "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Asset Storage System List", "Block for viewing list of asset storage systems.", "~/Blocks/Core/AssetStorageSystemList.ascx", "Core", "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8" );

            UpdateAssetStoragePage( "Asset Storage Systems", "Asset Storage Systems", "Asset Storage Systems", "1F5D5991-C586-45FC-A5AC-B7CD4D533990" );
            UpdateAssetStoragePage( "Asset Storage System Detail", "Asset Storage System Detail", "Asset Storage System Detail", "299751A1-EBE2-467C-8271-44BA13278331" );

            RenameAssetStorageEntity( "Rock.Model.AssetStorageSystem", "Rock.Model.AssetStorageSystem", "Asset Storage System" );
            RenameTable(name: "dbo.AssetStorageProvider", newName: "AssetStorageSystem");
        }

        /// <summary>
        /// Adds the local content asset storage provider if it does not already exist
        /// </summary>
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

        /// <summary>
        /// Renames the asset storage entity.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="friendlyName">Name of the friendly.</param>
        private void RenameAssetStorageEntity( string name, string assemblyName, string friendlyName )
        {
            Sql( $@"UPDATE [dbo].[EntityType]
                SET [Name] = '{name}'
                , [AssemblyName] = '{assemblyName}'
                , [FriendlyName] = '{friendlyName}'
                WHERE [Guid] = 'E0B4BE77-B29F-4BD4-AE45-CF833AC3A482'" );
        }

        /// <summary>
        /// Updates the asset storage page.
        /// </summary>
        /// <param name="internalName">Name of the internal.</param>
        /// <param name="pageTitle">The page title.</param>
        /// <param name="browserTitle">The browser title.</param>
        /// <param name="pageGuid">The page unique identifier.</param>
        private void UpdateAssetStoragePage( string internalName, string pageTitle, string browserTitle, string pageGuid )
        {
            Sql( $@"UPDATE [dbo].[Page]
                SET InternalName = '{internalName}'
	                , PageTitle = '{pageTitle}'
	                , BrowserTitle = '{browserTitle}'
                WHERE [Guid] = '{pageGuid}'" );
        }

        /// <summary>
        /// Udpates the name on the asset storage block.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="guid">The unique identifier.</param>
        private void UdpateAssetStorageBlock( string name, string guid )
        {
            Sql( $@"UPDATE [dbo].[Block]
                SET [Name] = '{name}'
                WHERE [Guid] = '{guid}'");
        }

        /// <summary>
        /// Updates the name and class of the field type
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="classAndNamespace">The class and namespace.</param>
        /// <param name="guid">The unique identifier.</param>
        private void UpdateAssetStorageFieldType(string name, string classAndNamespace, string guid )
        {
            Sql( $@"UPDATE [dbo].[FieldType]
                SET [Name] = '{name}'
	                , Class = '{classAndNamespace}'
                WHERE [Guid] = '{guid}'" );
        }
    }
}

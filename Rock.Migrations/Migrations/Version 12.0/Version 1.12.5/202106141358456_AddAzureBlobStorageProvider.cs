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
    public partial class AddAzureBlobStorageProvider : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateAzureStorageProvider_Up();
        }

        private void CreateAzureStorageProvider_Up()
        {
            Sql( @"
DECLARE @azureBlobProviderGuid NVARCHAR(37) = N'9925a20a-7262-4fc7-b86e-856f6d98be17';

-- This shouldn't happen, but just in case the assembly was auto-added to the EntityType table in Rock, update the GUID to ensure we have a well-known reference.
UPDATE [dbo].[EntityType] SET [Guid] = @azureBlobProviderGuid WHERE [Name] = N'Rock.Storage.Provider.AzureBlobStorage';

IF EXISTS(SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = @azureBlobProviderGuid)
BEGIN
	UPDATE [dbo].[EntityType]
	SET
		  [Name] = N'Rock.Storage.Provider.AzureBlobStorage'
		, [AssemblyName] = N'Rock.Storage.Provider.AzureBlobStorage, Rock, Version=1.13.0.10, Culture=neutral, PublicKeyToken=null'
		, [FriendlyName] = N'Azure Blob Storage'
		, [IsEntity] = 0
		, [IsSecured] = 1
		, [IsCommon] = 0
		, [ForeignKey] = NULL
		, [SingleValueFieldTypeId] = NULL
		, [MultiValueFieldTypeId] = NULL
		, [ForeignGuid] = NULL
		, [ForeignId] = NULL
		, [IsIndexingEnabled] = 0
		, [IndexResultTemplate] = NULL
		, [IndexDocumentUrl] = NULL
		, [LinkUrlLavaTemplate] = NULL
		, [AttributesSupportPrePostHtml] = 0
		, [AttributesSupportShowOnBulk] = 0
	WHERE [Guid] = @azureBlobProviderGuid;
END
ELSE
BEGIN
	INSERT [dbo].[EntityType] ([Name], [AssemblyName], [FriendlyName], [IsEntity], [IsSecured], [IsCommon], [Guid], [ForeignKey], [SingleValueFieldTypeId], [MultiValueFieldTypeId], [ForeignGuid], [ForeignId], [IsIndexingEnabled], [IndexResultTemplate], [IndexDocumentUrl], [LinkUrlLavaTemplate], [AttributesSupportPrePostHtml], [AttributesSupportShowOnBulk]) VALUES (N'Rock.Storage.Provider.AzureBlobStorage', N'Rock.Storage.Provider.AzureBlobStorage, Rock, Version=1.13.0.10, Culture=neutral, PublicKeyToken=null', N'Azure Blob Storage', 0, 1, 0, @azureBlobProviderGuid, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, 0, 0);
END


DECLARE @BinaryFileEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '62AF597F-F193-412B-94EA-291CF713327D');
DECLARE @AzureBlobStorageEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = @azureBlobProviderGuid);
DECLARE @FieldTypeId int = (SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA');

IF NOT EXISTS(SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = N'5d921dde-623a-4079-b987-25c74b4cdb7b')
	INSERT [dbo].[Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [IconCssClass], [AllowSearch], [ForeignGuid], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory], [IsActive], [EnableHistory], [PreHtml], [PostHtml], [AbbreviatedName], [ShowOnBulk], [IsPublic]) VALUES (1, @FieldTypeId, @BinaryFileEntityTypeId, N'StorageEntityTypeId', @AzureBlobStorageEntityTypeId, N'AzureBlobContainerName', N'Azure Blob Container Name', N'The Azure Blob Storage container name to use for files of this type.', 0, 0, N'', 0, 0, N'5d921dde-623a-4079-b987-25c74b4cdb7b', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0);

IF NOT EXISTS(SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = N'ba7c28e6-b45e-4983-8a8d-96985e2c4ef4')
	INSERT [dbo].[Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [IconCssClass], [AllowSearch], [ForeignGuid], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory], [IsActive], [EnableHistory], [PreHtml], [PostHtml], [AbbreviatedName], [ShowOnBulk], [IsPublic]) VALUES (1, @FieldTypeId, @BinaryFileEntityTypeId, N'StorageEntityTypeId', @AzureBlobStorageEntityTypeId, N'AzureBlobContainerFolderPath', N'Azure Blob Container Folder Path', N'An optional folder path inside the container to use for files of this type.', 0, 0, N'', 0, 0, N'ba7c28e6-b45e-4983-8a8d-96985e2c4ef4', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0);
" );

            // update the friendly name of the Pillars plugin if it was already installed.
            Sql( "UPDATE [EntityType] SET [FriendlyName] = [FriendlyName] + N' (plugin)' WHERE [Guid] = 'F7DE0437-3A21-4EE7-AA53-DFCB366E252C';" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // A Down() migration is not safe, here, because the EntityType record could be referenced by BinaryFileType records.
        }
    }
}
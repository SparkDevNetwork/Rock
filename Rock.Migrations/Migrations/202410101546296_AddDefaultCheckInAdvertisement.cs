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
using System;

namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class AddDefaultCheckInAdvertisement : Rock.Migrations.RockMigration
    {
        private const string DefaultImageBinaryFileGuid = "414b0540-3bc8-4c3a-a74e-0fcc528e58dc";
        private const string ContentChannelItemGuid = "c4adfd70-3b67-4fed-bdc6-48d2e116fc24";
        private const string ImageAttributeValueGuid = "c0bc26a6-3552-4f29-8c04-f10133cb4cd4";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            var hexData = BitConverter.ToString( MigrationSQL._202410101546296_AddDefaultCheckInAdvertisement_Image ).Replace( "-", "" );

            Sql( $@"
DECLARE	@BinaryFileId INT
DECLARE @BinaryFileTypeId INT= (SELECT TOP 1 Id from [BinaryFileType] where [Guid] = '{Rock.SystemGuid.BinaryFiletype.CONTENT_CHANNEL_ITEM_IMAGE}')
DECLARE @StorageEntityTypeIdDatabase INT = (SELECT TOP 1 Id FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE}')

-- Add logo.jpg
IF NOT EXISTS (SELECT * FROM [BinaryFile] WHERE [Guid] = '{DefaultImageBinaryFileGuid}' )
BEGIN
INSERT INTO [BinaryFile] ([IsTemporary], [IsSystem], [BinaryFileTypeId], [FileName], [MimeType], [StorageEntityTypeId], [Guid])
			VALUES (0,0, @BinaryFileTypeId, 'welcome.jpg', 'image/jpeg', @StorageEntityTypeIdDatabase, '{DefaultImageBinaryFileGuid}')

SET @BinaryFileId = SCOPE_IDENTITY()

INSERT INTO [BinaryFileData] ([Id], [Content], [Guid])
  VALUES ( 
    @BinaryFileId
    ,0x{hexData}
    ,'{DefaultImageBinaryFileGuid}'
    )
END
" );

            Sql( $@"
DECLARE @ContentChannelItemGuid UNIQUEIDENTIFIER = '{ContentChannelItemGuid}'
DECLARE @ImageAttributeValueGuid UNIQUEIDENTIFIER = '{ImageAttributeValueGuid}'
DECLARE @ImageBinaryFileGuid UNIQUEIDENTIFIER = '{DefaultImageBinaryFileGuid}'

DECLARE @ContentChannelId INT = (SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = 'a57bdbcd-fa77-4a6e-967d-1c5ace962587')
DECLARE @ContentChannelTypeId INT = (SELECT TOP 1 [Id] FROM [ContentChannelType] WHERE [Guid] = '09f46d0d-72bc-4445-90e7-c256e4778666')
DECLARE @ImageAttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '268220a5-01b2-4aa9-a75d-94a4ac7a30f6')

IF NOT EXISTS (SELECT * FROM [ContentChannelItem] WHERE [Guid] = @ContentChannelItemGuid)
BEGIN
    INSERT INTO [ContentChannelItem] (
        [ContentChannelId],
        [ContentChannelTypeId],
        [Title],
        [Content],
        [Priority],
        [Status],
        [StartDateTime],
        [Guid],
        [Order],
        [ItemGlobalKey],
        [StructuredContent])
        VALUES (
            @ContentChannelId,
            @ContentChannelTypeId,
            'Default Welcome',
            '',
            0,
            1,
            '2024-01-01 00:00:00.000',
            @ContentChannelItemGuid,
            0,
            'default-checkin-welcome-ad',
            '')

    DECLARE @ContentChannelItemId INT = (SELECT TOP 1 [Id] FROM [ContentChannelItem] WHERE [Guid] = @ContentChannelItemGuid)

    DELETE FROM [AttributeValue] WHERE [Guid] = @ImageAttributeValueGuid

    INSERT INTO [AttributeValue] (
        [IsSystem],
        [AttributeId],
        [EntityId],
        [Value],
        [Guid],
        [IsPersistedValueDirty])
        VALUES (
            0,
            @ImageAttributeId,
            @ContentChannelItemId,
            @ImageBinaryFileGuid,
            @ImageAttributeValueGuid,
            1)
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $"DELETE FROM [AttributeValue] WHERE [Guid] = '{ImageAttributeValueGuid}'" );
            Sql( $"DELETE FROM [ContentChannelItem] WHERE [Guid] = '{ContentChannelItemGuid}'" );
            Sql( $"DELETE FROM [BinaryFileData] WHERE [Guid] = '{DefaultImageBinaryFileGuid}'" );
            Sql( $"DELETE FROM [BinaryFile] WHERE [Guid] = '{DefaultImageBinaryFileGuid}'" );
        }
    }
}

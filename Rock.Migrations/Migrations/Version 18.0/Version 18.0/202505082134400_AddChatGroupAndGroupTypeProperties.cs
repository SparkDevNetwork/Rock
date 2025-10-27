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
    using Rock.Enums.Communication.Chat;

    /// <summary>
    ///
    /// </summary>
    public partial class AddChatGroupAndGroupTypeProperties : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Group", "ChatChannelAvatarBinaryFileId", c => c.Int() );
            AddColumn( "dbo.Group", "ChatPushNotificationModeOverride", c => c.Int() );
            AddColumn( "dbo.GroupType", "ChatPushNotificationMode", c => c.Int( nullable: false ) );
            AddForeignKey( "dbo.Group", "ChatChannelAvatarBinaryFileId", "dbo.BinaryFile", "Id" );

            Sql( $@"
-- Set the ChatPushNotificationMode to 'Mentions and Replies' for chat shared channels.
UPDATE [GroupType]
SET [ChatPushNotificationMode] = {ChatNotificationMode.Mentions.ConvertToInt()}
WHERE [Guid] = '{Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL}';

-- Get the 'Avatar Image' Group Type > Group [Attribute].[Id].
DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'CB6178C6-4A32-4008-B56E-9D548FD8303B');

-- Migrate any existing 'Avatar Image' Attribute Values to each respective [Group].[ChatChannelAvatarBinaryFileId] field.
IF @AttributeId IS NOT NULL
BEGIN
    ;WITH AvatarAttributeValues AS (
        SELECT [EntityId] AS [GroupId]
            , TRY_CAST([Value] AS [UNIQUEIDENTIFIER]) AS [BinaryFileGuid]
        FROM [AttributeValue] av
        WHERE av.[AttributeId] = @AttributeId
    )
    UPDATE g
    SET g.[ChatChannelAvatarBinaryFileId] = bf.[Id]
    FROM [Group] g
    INNER JOIN AvatarAttributeValues aav
        ON aav.[GroupId] = g.[Id]
    INNER JOIN [BinaryFile] bf
        ON bf.[Guid] = aav.[BinaryFileGuid]
    WHERE g.[ChatChannelAvatarBinaryFileId] IS NULL
END" );

            // Delete the no-longer-needed Group Type > Group "Avatar Image" Attribute.
            RockMigrationHelper.DeleteAttribute( "CB6178C6-4A32-4008-B56E-9D548FD8303B" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.Group", "ChatChannelAvatarBinaryFileId", "dbo.BinaryFile" );
            DropColumn( "dbo.GroupType", "ChatPushNotificationMode" );
            DropColumn( "dbo.Group", "ChatPushNotificationModeOverride" );
            DropColumn( "dbo.Group", "ChatChannelAvatarBinaryFileId" );
        }
    }
}

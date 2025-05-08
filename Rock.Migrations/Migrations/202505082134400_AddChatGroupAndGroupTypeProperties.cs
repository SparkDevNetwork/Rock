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
UPDATE [GroupType]
SET [ChatPushNotificationMode] = {ChatNotificationMode.MentionsAndReplies.ConvertToInt()}
WHERE [Guid] = '{Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL}';" );
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

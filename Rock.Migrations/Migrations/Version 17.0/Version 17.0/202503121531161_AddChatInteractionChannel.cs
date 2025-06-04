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
    public partial class AddChatInteractionChannel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add "Chat" interaction medium.
            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM,
                value: "Chat",
                description: "Used for tracking engagement with Rock's chat system.",
                guid: Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CHAT,
                isSystem: true
            );

            // Add "Chat" interaction channel.
            Sql( $@"
IF NOT EXISTS (SELECT * FROM [InteractionChannel] WHERE [Guid] = '{Rock.SystemGuid.InteractionChannel.CHAT}')
BEGIN
    DECLARE @ComponentEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.GROUP}');
    DECLARE @ChannelTypeMediumValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CHAT}');

    INSERT INTO [InteractionChannel]
    (
        [Name]
        , [ComponentEntityTypeId]
        , [ChannelTypeMediumValueId]
        , [Guid]
        , [UsesSession]
        , [IsActive]
    )
    VALUES
    (
        'Chat'
        , @ComponentEntityTypeId
        , @ChannelTypeMediumValueId
        , '{Rock.SystemGuid.InteractionChannel.CHAT}'
        , 0
        , 1
    );
END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete "Chat" interaction channel.
            Sql( $"DELETE FROM [InteractionChannel] WHERE [Guid] = '{Rock.SystemGuid.InteractionChannel.CHAT}';" );

            // Delete "Chat" interaction medium.
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CHAT );
        }
    }
}

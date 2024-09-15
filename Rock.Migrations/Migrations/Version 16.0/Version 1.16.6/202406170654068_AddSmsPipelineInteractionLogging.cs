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
    /// Add Interaction Logging for Sms Pipeline Actions.
    /// </summary>
    public partial class AddSmsPipelineInteractionLogging : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.SmsAction", "IsInteractionLoggedAfterProcessing", c => c.Boolean( nullable: false ) );

            // Add Interaction Channel "SMS Pipeline".
            Sql( @"
-- Interaction Channel: SMS_PIPELINE
DECLARE @ChannelGuid UNIQUEIDENTIFIER = '282694BE-3062-4857-AC4E-83269F075351'
-- Channel Medium Type: Communication
DECLARE @ChannelTypeMediumValueGuid UNIQUEIDENTIFIER = '55004F5C-A8ED-7CB7-47EE-5988E9F8E0A8'
-- Entity Type: Rock.Model.SmsPipeline
DECLARE @ComponentEntityTypeGuid UNIQUEIDENTIFIER = '64DA3A06-FD39-4E5B-8126-38404FB0092A'

IF NOT EXISTS( SELECT * FROM [InteractionChannel] WHERE [Guid] = @ChannelGuid )
BEGIN
    DECLARE @ChannelTypeMediumValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @ChannelTypeMediumValueGuid)
    DECLARE @ComponentEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = @ComponentEntityTypeGuid)

    INSERT INTO [InteractionChannel]
	    ([Name], [ComponentEntityTypeId], [ChannelTypeMediumValueId], [Guid], [UsesSession], [IsActive])
	    VALUES ('SMS Pipeline', @ComponentEntityTypeId, @ChannelTypeMediumValueId, @ChannelGuid, 0, 1)
END
"
            );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.SmsAction", "IsInteractionLoggedAfterProcessing" );

            // Delete Interaction Channel "SMS Pipeline".
            Sql( @"
-- Interaction Channel: SMS_PIPELINE
DECLARE @ChannelGuid UNIQUEIDENTIFIER = '282694BE-3062-4857-AC4E-83269F075351'

DELETE FROM [InteractionChannel] WHERE [Guid] = @ChannelGuid
"
            );
        }
    }
}

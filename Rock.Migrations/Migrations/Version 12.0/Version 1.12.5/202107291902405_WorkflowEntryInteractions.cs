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
    public partial class WorkflowEntryInteractions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
DECLARE @ChannelGuid UNIQUEIDENTIFIER = '{Rock.SystemGuid.InteractionChannel.WORKFLOW_LAUNCHES}'
DECLARE @ChannelTypeMediumValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_SYSTEM_EVENTS}')
DECLARE @ComponentEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowType')
DECLARE @InteractionEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Workflow')

IF NOT EXISTS( SELECT * FROM [InteractionChannel] WHERE [Guid] = @ChannelGuid )
BEGIN
    INSERT INTO [InteractionChannel]
	    ([Name], [ComponentEntityTypeId], [InteractionEntityTypeId], [ChannelTypeMediumValueId], [Guid], [UsesSession], [IsActive])
	    VALUES ('Workflow Launches', @ComponentEntityTypeId, @InteractionEntityTypeId, @ChannelTypeMediumValueId, @ChannelGuid, 1, 1)
END
ELSE
BEGIN
 UPDATE [InteractionChannel]
    SET [Name] = 'Workflow Launches',
     ComponentEntityTypeId = @ComponentEntityTypeId,
     InteractionEntityTypeId = @InteractionEntityTypeId,
     ChannelTypeMediumValueId = @ChannelTypeMediumValueId,
     UsesSession = 1, 
     IsActive = 1
     WHERE [Guid] = @ChannelGuid
END
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}

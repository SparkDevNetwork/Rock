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
    public partial class Rollup_0326 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            PrayerInteractionsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PrayerInteractionsDown();
        }

        /// <summary>
        /// DH: Add Prayer Interactions Medium Type and Channel
        /// </summary>
        public void PrayerInteractionsUp()
        {
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM,
                "System Events",
                "Used for tracking general system events where the individual channels will identify the type of event.",
                SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_SYSTEM_EVENTS );

            Sql( @"
DECLARE @ChannelGuid UNIQUEIDENTIFIER = '3D49FB99-94D1-4F63-B1A2-30D4FEDE11E9'

IF NOT EXISTS( SELECT * FROM [InteractionChannel] WHERE [Guid] = @ChannelGuid )
BEGIN
    DECLARE @ChannelTypeMediumValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '5919214F-9C59-4913-BE4E-0DFB6A05F528')
    DECLARE @ComponentEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PrayerRequest')

    INSERT INTO [InteractionChannel]
	    ([Name], [ComponentEntityTypeId], [ChannelTypeMediumValueId], [Guid], [UsesSession], [IsActive])
	    VALUES ('Prayer Events', @ComponentEntityTypeId, @ChannelTypeMediumValueId, @ChannelGuid, 0, 1)
END" );
        }
        
        public void PrayerInteractionsDown()
        {
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_SYSTEM_EVENTS );
        }
    }
}

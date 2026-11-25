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
namespace Rock.Data.Migrations;

/// <summary>
///
/// </summary>
[SqlMigration( 20260615194906, "20.0" )]
internal class AddInteractionComponentDailyCount : Migration
{
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        // -----------------------------------------------------------------
        // Migration A - Schema
        // -----------------------------------------------------------------
        Sql( @"
                CREATE TABLE [dbo].[InteractionComponentDailyCount] (
                    [InteractionComponentId] [int] NOT NULL,
                    [InteractionDate] [date] NOT NULL,
                    [Operation] [nvarchar](25) NOT NULL,
                    [InteractionDateKey] [int] NOT NULL,
                    [LoggedInInteractionCount] [int] NOT NULL,
                    [AnonymousInteractionCount] [int] NOT NULL,
                    [LoggedInSessionCount] [int] NOT NULL,
                    [AnonymousSessionCount] [int] NOT NULL,
                    [TotalInteractionCount] [int] NOT NULL,
                    [TotalSessionCount] [int] NOT NULL,
                    [AverageInteractionLength] [decimal](18, 2),
                    CONSTRAINT [PK_dbo.InteractionComponentDailyCount] PRIMARY KEY ([InteractionComponentId], [InteractionDate], [Operation])
                )

                ALTER TABLE [dbo].[InteractionComponentDailyCount] ADD CONSTRAINT [FK_dbo.InteractionComponentDailyCount_dbo.InteractionComponent_InteractionComponentId] FOREIGN KEY ([InteractionComponentId]) REFERENCES [dbo].[InteractionComponent] ([Id]) ON DELETE CASCADE

                CREATE INDEX [IX_InteractionComponentId] ON [dbo].[InteractionComponentDailyCount]([InteractionComponentId])

                CREATE INDEX [IX_InteractionDateKey] ON [dbo].[InteractionComponentDailyCount]([InteractionDateKey])" );

        Sql( @"
                ALTER TABLE [dbo].[InteractionChannel] ADD [EnableComponentDailyCounts] [bit] NOT NULL DEFAULT 0" );

        // -----------------------------------------------------------------
        // Migration B - Defined Type Attribute + seed values
        // -----------------------------------------------------------------
        AddDefaultComponentDailyCountAttributeAndSeed_Up();

        // -----------------------------------------------------------------
        // Migration C - Backfill EnableComponentDailyCounts from medium
        // -----------------------------------------------------------------
        BackfillEnableComponentDailyCountsFromMedium_Up();
    }

    /// <summary>
    /// Adds the "Default Component Daily Counts" boolean attribute to the
    /// Interaction Mediums defined type and seeds <c>True</c> on the well-known
    /// mediums that should default to having component daily counts enabled.
    /// All other mediums rely on the attribute's default of <c>False</c>.
    /// </summary>
    private void AddDefaultComponentDailyCountAttributeAndSeed_Up()
    {
        // Attribute definition.
        RockMigrationHelper.AddDefinedTypeAttribute(
            definedTypeGuid: Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM,
            fieldTypeGuid: Rock.SystemGuid.FieldType.BOOLEAN,
            name: "Default Component Daily Counts",
            key: "DefaultComponentDailyCounts",
            description: "When enabled, newly created interaction channels will automatically have Enable Component Daily Counts turned on.",
            order: 0,
            defaultValue: "False",
            guid: Rock.SystemGuid.Attribute.DEFINED_TYPE_INTERACTION_MEDIUM_DEFAULT_COMPONENT_DAILY_COUNT );

        Sql( $@"
        UPDATE [Attribute] 
           SET [IsGridColumn] = 1
         WHERE [Guid] = '{Rock.SystemGuid.Attribute.DEFINED_TYPE_INTERACTION_MEDIUM_DEFAULT_COMPONENT_DAILY_COUNT}'
                " );

        // Seed True on the mediums that should default to enabled.
        var enabledMediumGuids = new[]
        {
            Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE,
            Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL,
            Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_URLSHORTENER,
            Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_SYSTEM_EVENTS,
            Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CHAT,
        };

        foreach ( var mediumGuid in enabledMediumGuids )
        {
            RockMigrationHelper.AddDefinedValueAttributeValue(
                definedValueGuid: mediumGuid,
                attributeGuid: Rock.SystemGuid.Attribute.DEFINED_TYPE_INTERACTION_MEDIUM_DEFAULT_COMPONENT_DAILY_COUNT,
                value: "True" );
        }

        // Hydrate the persisted columns on the AttributeValue rows we just
        // seeded. Reason: New attribute has IsGridColumn = 1 and must show values on first load.
        Sql( $@"
UPDATE [av]
SET [av].[ValueAsBoolean]              = 1,
    [av].[ValueAsNumeric]              = NULL,
    [av].[ValueAsDateTime]             = NULL,
    [av].[PersistedTextValue]          = 'Yes',
    [av].[PersistedHtmlValue]          = 'Yes',
    [av].[PersistedCondensedTextValue] = 'Y',
    [av].[PersistedCondensedHtmlValue] = 'Y',
    [av].[IsPersistedValueDirty]       = 0
FROM [AttributeValue] AS [av]
INNER JOIN [Attribute] AS [a]
    ON [a].[Id] = [av].[AttributeId]
   AND [a].[Guid] = '{Rock.SystemGuid.Attribute.DEFINED_TYPE_INTERACTION_MEDIUM_DEFAULT_COMPONENT_DAILY_COUNT}'
WHERE [av].[Value] = 'True';
" );
    }

    /// <summary>
    /// Backfills <c>InteractionChannel.EnableComponentDailyCounts = 1</c> for every
    /// existing channel whose medium has the new <c>Default Component Daily Counts</c>
    /// attribute set to <c>True</c>, so the Rock Cleanup job picks up immediately
    /// on the next run.
    /// </summary>
    private void BackfillEnableComponentDailyCountsFromMedium_Up()
    {
        Sql( $@"
UPDATE [ic]
SET [ic].[EnableComponentDailyCounts] = 1
FROM [InteractionChannel] AS [ic]
INNER JOIN [AttributeValue] AS [av]
    ON [av].[EntityId] = [ic].[ChannelTypeMediumValueId]
INNER JOIN [Attribute] AS [a]
    ON [a].[Id] = [av].[AttributeId]
   AND [a].[Guid] = '{Rock.SystemGuid.Attribute.DEFINED_TYPE_INTERACTION_MEDIUM_DEFAULT_COMPONENT_DAILY_COUNT}'
WHERE [av].[Value] = 'True';
" );
    }
}

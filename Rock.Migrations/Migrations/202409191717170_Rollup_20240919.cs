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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20240919 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChopAccountEditBlockUp();
            AddBlockSettingsToObsidianGroupSchedulerUp();
            MobileMyNotesQuickNoteUp();
            ContentCollectionViewSettingUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ChopAccountEditBlockDown();
            AddBlockSettingsToObsidianGroupSchedulerDown();
            MobileMyNotesQuickNoteDown();
            ContentCollectionViewSettingDown();
        }

        #region KH: Chop AccountEdit Block to PublicProfileEdit Webforms block.

        private void ChopAccountEditBlockUp()
        {
            UpdateAttributesUp();
            ChopBlocksUp();
        }

        private void ChopAccountEditBlockDown()
        {
            UpdateAttributesDown();
            ChopBlocksDown();
        }

        private void UpdateAttributesUp()
        {
            Sql( @"
UPDATE [Attribute] SET [Name] = 'Show Addresses', [Key] = 'ShowAddresses', [AbbreviatedName] = 'Show Addresses' WHERE [Guid] = '85534E70-C876-4E09-8DD5-B143896FAB38'

UPDATE [Attribute] SET [Name] = 'Address Type', [Key] = 'AddressType', [AbbreviatedName] = 'Address Type' WHERE [Guid] = 'DC31914A-0F13-4243-91E7-69A7EBBCCF10'" );
        }

        private void UpdateAttributesDown()
        {
            // Attribute for BlockType
            //   BlockType: Account Edit
            //   Category: Security
            //   Attribute: Address Type
            RockMigrationHelper.DeleteAttribute( "85534e70-c876-4e09-8dd5-b143896fab38" );

            // Attribute for BlockType
            //   BlockType: Account Edit
            //   Category: Security
            //   Attribute: Show Addresses
            RockMigrationHelper.DeleteAttribute( "dc31914a-0f13-4243-91e7-69a7ebbccf10" );
        }

        private void ChopBlocksUp()
        {
            // Custom chop to replace legacy AccountEdit Block with Webforms PublicProfileEdit Block.
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop AccountEdit Block",
                blockTypeReplacements: new Dictionary<string, string> {
                { "F501AB3F-1F41-4C06-9BC2-57C42E702995", "841D1670-8BFD-4913-8409-FB47EB7A2AB9" }, // AccountEdit -> PublicProfileEdit.ascx
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_167_CHOP_ACCOUNT_EDIT_BLOCK,
                blockAttributeKeysToIgnore: new Dictionary<string, string>
                {
                { "F501AB3F-1F41-4C06-9BC2-57C42E702995", "AddressRequired" }
                } );
        }

        private void ChopBlocksDown()
        {
            // Delete the Service Job Entity
            RockMigrationHelper.DeleteByGuid( SystemGuid.ServiceJob.DATA_MIGRATIONS_167_CHOP_ACCOUNT_EDIT_BLOCK, "ServiceJob" );
        }

        #endregion

        #region JPH: Add block settings to the Obsidian Group Scheduler block.

        private void AddBlockSettingsToObsidianGroupSchedulerUp()
        {
            // Attribute for BlockType
            //   BlockType: Group Scheduler
            //   Category: Group Scheduling
            //   Attribute: Hide Clone Schedules
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Clone Schedules", "HideCloneSchedules", "Hide Clone Schedules", @"When enabled, will hide the ""Clone Schedules"" button and disable this functionality.", 5, @"False", "AC75C493-C54B-4864-8464-03E927EE94FD" );

            // Attribute for BlockType
            //   BlockType: Group Scheduler
            //   Category: Group Scheduling
            //   Attribute: Hide Auto Schedule
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Auto Schedule", "HideAutoSchedule", "Hide Auto Schedule", @"When enabled, will hide the ""Auto Schedule"" button and disable this functionality.", 6, @"False", "E971A150-0FD6-4CB0-9566-3D7E0259B7C2" );
        }

        private void AddBlockSettingsToObsidianGroupSchedulerDown()
        {
            // Attribute for BlockType
            //   BlockType: Group Scheduler
            //   Category: Group Scheduling
            //   Attribute: Hide Auto Schedule
            RockMigrationHelper.DeleteAttribute( "E971A150-0FD6-4CB0-9566-3D7E0259B7C2" );

            // Attribute for BlockType
            //   BlockType: Group Scheduler
            //   Category: Group Scheduling
            //   Attribute: Hide Clone Schedules
            RockMigrationHelper.DeleteAttribute( "AC75C493-C54B-4864-8464-03E927EE94FD" );
        }

        #endregion

        #region BC: Add Quick Note NoteType and My Notes Default Template

        /// <summary>
        /// The standard icon to use for new templates.
        /// </summary>
        private const string _standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        private readonly string _myNotesDefaultTemplate = @"{% assign entityName = Note.EntityName %}
{% assign noteTypeColor = Note.NoteTypeColor %}
{% assign noteTypeName = Note.NoteTypeName %}

//- If there is no entity associated with the note, assume it's a quick note. 
{% if Note.EntityId == null %}
    {% assign entityName = 'Quick Note' %}
    {% assign iconSource = 'fa-sticky-note' %}
    {% assign iconFrameStyle = 'bg-warning-soft' %}
    {% assign iconStyle = 'text-warning-strong' %}
    {% assign iconFamily = 'FontAwesomeRegular' %}
    {% assign noteTypeName = '' %}
    {% capture menuActions %}
        <Rock:MenuAction Title=""Link to Person"" 
            Command=""{Binding LinkToPerson}""
            SystemIcon=""{OnPlatform iOS=person}"" />
        <Rock:MenuAction Title=""Add Connection"" 
            Command=""{Binding AddConnection}""
            SystemIcon=""{OnPlatform iOS=link.circle}"" />
        <Rock:MenuAction Title=""Add Reminder"" 
            Command=""{Binding AddReminder}""
            SystemIcon=""{OnPlatform iOS=bell}"" />
    {% endcapture %}
//- Otherwise, determine the icon based on the entity type.
{% else %}
    //- Configure icon for connection request notes.
    {% if Note.NoteTypeEntityTypeId == ConnectionEntityTypeId %}
        {% assign iconSource = 'fa-plug' %}
        {% assign iconFrameStyle = 'bg-info-soft' %}
        {% assign iconStyle = 'text-info-strong' %}
        {% assign iconFamily = 'FontAwesomeSolid' %}
        {% capture menuActions %}
            <Rock:MenuAction Title=""View Connection"" 
                Command=""{Binding PushPage}""
                CommandParameter=""{{ ConnectionDetailPage }}?ConnectionRequestGuid={{ Note.EntityGuid }}""
                SystemIcon=""{OnPlatform iOS=link.circle}"" />
        {% endcapture %}
    //- Configure the icon for reminder notes.
    {% elseif Note.NoteTypeEntityTypeId == ReminderEntityTypeId %}
        {% assign iconSource = 'fa-bell' %}
        {% assign iconFrameStyle = 'bg-success-soft' %}
        {% assign iconStyle = 'text-success-strong' %}
        {% assign iconFamily = 'FontAwesomeSolid' %}
        {% capture menuActions %}
            <Rock:MenuAction Title=""View Reminder"" 
                Command=""{Binding PushPage}""
                CommandParameter=""{{ ReminderDetailPage }}?ReminderGuid={{ Note.EntityGuid }}""
                SystemIcon=""{OnPlatform iOS=bell}"" />
        {% endcapture %}
    //- Configure the icon for reminder notes.
    {% elseif Note.NoteTypeEntityTypeId == PersonEntityTypeId %}
        {% capture menuActions %}
            <Rock:MenuAction Title=""View Person"" 
                Command=""{Binding PushPage}""
                CommandParameter=""{{ PersonDetailPage }}?PersonGuid={{ Note.EntityGuid }}""
                SystemIcon=""{OnPlatform iOS=person}"" />
        {% endcapture %}
    {% endif %}
{% endif %}

<Grid ColumnDefinitions=""Auto, *""
    StyleClass=""gap-column-8, px-16, py-8""
    Rock:ContextMenu.ClickCommand=""{Binding EditNote}"">

    {% if Note.PhotoUrl and Note.PhotoUrl != '' %}
        <Rock:Avatar Source=""{{ Note.PhotoUrl | Escape }}""
            StyleClass=""w-48, h-48""
            Grid.Column=""0""
            ShowStroke=""False"" />
    {% elseif iconSource and iconSource != '' %}
        <Rock:StyledBorder StyleClass=""{{ iconFrameStyle }}, h-48, w-48"" 
            CornerRadius=""24""
            Grid.Column=""0"">
            <Rock:Icon IconClass=""{{ iconSource }}""
                StyleClass=""{{ iconStyle }}""
                FontSize=""24""
                HorizontalOptions=""Center""
                VerticalOptions=""Center""
                IconFamily=""{{ iconFamily }}"" />
        </Rock:StyledBorder>
    {% endif %}

    <VerticalStackLayout Grid.Column=""1""
        VerticalOptions=""Center""
        HorizontalOptions=""Fill"">
        <Grid ColumnDefinitions=""*, Auto, Auto"">
            <Label Text=""{{ entityName | Escape }}""
                StyleClass=""body, bold, text-interface-stronger""
                MaxLines=""1""
                LineBreakMode=""TailTruncation"" />

            {% assign dateTimeFormat = 'sd' %}
            {% if GroupNotesByDate %}
                {% assign dateTimeFormat = 'h:mmtt' %}
            {% endif %}

            <Label Text=""{{ Note.CreatedDateTime | Date:dateTimeFormat }}""
                Grid.Column=""1""
                StyleClass=""caption2, text-interface-strong""
                VerticalOptions=""Start"" />

            <Rock:IconButton IconClass=""fa fa-ellipsis-v""
                StyleClass=""text-interface-medium""
                Grid.Column=""2""
                FontSize=""12""
                Padding=""8, 0, 0, 0""
                VerticalOptions=""Start""
                BackgroundColor=""Transparent""
                Rock:ContextMenu.ShowMenuOnClick=""True"">
                <Rock:ContextMenu.Menu>
                    <DataTemplate>
                        <Rock:Menu>
                            <Rock:MenuGroup Title=""Note Actions"">
                                {{ menuActions }}
                                <Rock:MenuAction Title=""Edit Note"" 
                                    Command=""{Binding EditNote}""
                                    SystemIcon=""{OnPlatform iOS=pencil}"" />
                                <Rock:MenuAction Title=""Delete Note"" 
                                    Command=""{Binding DeleteNote}""
                                    SystemIcon=""{OnPlatform iOS=trash}""
                                    IsDestructive=""True"" />
                            </Rock:MenuGroup>
                        </Rock:Menu>
                    </DataTemplate>
                </Rock:ContextMenu.Menu>
            </Rock:IconButton>
        </Grid>

        {% if noteTypeName and noteTypeName != '' %}
            <HorizontalStackLayout Spacing=""4"">
                {% if noteTypeColor and noteTypeColor != '' %}
                    <Ellipse Fill=""{{ noteTypeColor }}""
                        WidthRequest=""8""
                        HeightRequest=""8"" />
                {% endif %}

                <Label Text=""{{ noteTypeName | Escape }}""
                    StyleClass=""caption1, text-interface-medium""
                    MaxLines=""1""
                    LineBreakMode=""TailTruncation"" />
            </HorizontalStackLayout>
        {% endif %}
            
        <Label Text=""{{ Note.NoteText | Escape }}""
            StyleClass=""footnote, text-interface-strong, mt-4""
            MaxLines=""2"" 
            LineBreakMode=""TailTruncation"" />

    </VerticalStackLayout>
    <Rock:ContextMenu.Menu>
        <DataTemplate>
            <Rock:Menu>
                <Rock:MenuGroup Title=""Note Actions"">
                    {{ menuActions }}
                    <Rock:MenuAction Title=""Edit Note"" 
                        Command=""{Binding EditNote}""
                        CommandParameter=""{{ Note.Id }}""
                        SystemIcon=""{OnPlatform iOS=pencil}"" />
                    <Rock:MenuAction Title=""Delete Note"" 
                        Command=""{Binding DeleteNote}""
                        CommandParameter=""{{ Note.Guid }}""
                        SystemIcon=""{OnPlatform iOS=trash}""
                        IsDestructive=""True"" />
                </Rock:MenuGroup>
            </Rock:Menu>
        </DataTemplate>
    </Rock:ContextMenu.Menu>
    <Rock:ContextMenu.Preview>
        <Rock:Preview>
            <Rock:Preview.PreviewTemplate>
                <DataTemplate>
                    <Grid ColumnDefinitions=""Auto, *""
                        StyleClass=""gap-column-8, px-16, py-8, bg-interface-softest"">
                    
                        {% if Note.PhotoUrl and Note.PhotoUrl != '' %}
                            <Rock:Avatar Source=""{{ Note.PhotoUrl | Escape }}""
                                StyleClass=""w-48, h-48""
                                Grid.Column=""0""
                                ShowStroke=""False"" />
                        {% elseif iconSource and iconSource != '' %}
                            <Rock:StyledBorder StyleClass=""{{ iconFrameStyle }}, h-48, w-48"" 
                                CornerRadius=""24""
                                Grid.Column=""0"">
                                <Rock:Icon IconClass=""{{ iconSource }}""
                                    StyleClass=""{{ iconStyle }}""
                                    FontSize=""24""
                                    HorizontalOptions=""Center""
                                    VerticalOptions=""Center""
                                    IconFamily=""{{ iconFamily }}"" />
                            </Rock:StyledBorder>
                        {% endif %}
                    
                        <VerticalStackLayout Grid.Column=""1""
                            VerticalOptions=""Center""
                            HorizontalOptions=""Fill"">
                            <Grid ColumnDefinitions=""*, Auto""
                                StyleClass=""gap-column-8"">
                                <Label Text=""{{ entityName | Escape }}""
                                    StyleClass=""body, bold, text-interface-stronger""
                                    MaxLines=""1""
                                    LineBreakMode=""TailTruncation"" />
                    
                                <Label Text=""{{ Note.CreatedDateTime | Date:'h:mmtt' }}""
                                    Grid.Column=""1""
                                    StyleClass=""caption2, text-interface-strong""
                                    VerticalOptions=""Start"" />
                            </Grid>
                    
                            {% if noteTypeName and noteTypeName != '' %}
                                <HorizontalStackLayout Spacing=""4"">
                                    {% if noteTypeColor and noteTypeColor != '' %}
                                        <Ellipse Fill=""{{ noteTypeColor }}""
                                            WidthRequest=""8""
                                            HeightRequest=""8"" />
                                    {% endif %}
                    
                                    <Label Text=""{{ noteTypeName | Escape }}""
                                        StyleClass=""caption1, text-interface-medium""
                                        MaxLines=""1""
                                        LineBreakMode=""TailTruncation"" />
                                </HorizontalStackLayout>
                            {% endif %}
                                
                            <Label Text=""{{ Note.NoteText | Escape }}""
                                StyleClass=""footnote, text-interface-strong, mt-4""
                                MaxLines=""16"" 
                                LineBreakMode=""TailTruncation"" />
                        </VerticalStackLayout>
                    </Grid>
                </DataTemplate>
            </Rock:Preview.PreviewTemplate>
            <Rock:Preview.VisiblePath>
                <RoundRectangle CornerRadius=""8"" />
            </Rock:Preview.VisiblePath>
        </Rock:Preview>
    </Rock:ContextMenu.Preview>
</Grid>";

        public void MobileMyNotesQuickNoteUp()
        {
            // Add Default "Quick Note" Note Type
            RockMigrationHelper.AddOrUpdateNoteTypeByMatchingNameAndEntityType( "Quick Note", "Rock.Model.Person", true, Rock.SystemGuid.NoteType.QUICK_NOTE, true, "fa fa-sticky-note-o", false );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
              "Mobile > Core > My Notes",
              string.Empty,
              SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_MY_NOTES );

            // Add Default Block Template for My Notes Block
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "421F2759-B6B6-4C47-AA42-320B6DB9F0A7",
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_MY_NOTES,
                "Default",
                _myNotesDefaultTemplate,
                _standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public void MobileMyNotesQuickNoteDown()
        {
            RockMigrationHelper.DeleteTemplateBlockTemplate( "421F2759-B6B6-4C47-AA42-320B6DB9F0A7" );
        }

        #endregion

        #region BC: Update Default Value of new Content Collection View setting

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public void ContentCollectionViewSettingUp()
        {
            // We need to set an attribute value for any Content Collection block.
            // This will default 'IncludeUnapproved' to true for any existing block,
            // but ensure new blocks use the default value of false.

            // First, ensure that the BlockType attribute exists.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", SystemGuid.FieldType.BOOLEAN, "Include Unapproved", "IncludeUnapproved", "Include Unapproved", "Determines if unapproved items should be shown.", 0, "False", "FD68C0BE-1FC2-4161-AD5D-6B2371D24326" );

            // Second, execute a SQL command to set the default value for all existing blocks.
            // Get all Block (entity) IDs with the BlockTypeGuid, excluding those from the attribute value table that match the AttributeId.
            // This will allow us to set the default value for any block that doesn't already have a value.
            var sql = @"DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM BlockType WHERE Guid='CC387575-3530-4CD6-97E0-1F449DCA1869');
DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'FD68C0BE-1FC2-4161-AD5D-6B2371D24326');
DECLARE @CurrentDateTime DATETIME = GETDATE();

-- If either @BlockTypeId or @AttributeId is NULL, exit the script.
IF @BlockTypeId IS NULL OR @AttributeId IS NULL
BEGIN
    PRINT 'BlockTypeId or AttributeId is NULL, exiting script.';
    RETURN;
END;

-- Set 'IncludeUnapproved' to true for all blocks that have not been explicitly set yet.
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime])
SELECT 
    0, 
    @AttributeId, 
    [Id], 
    'True', 
    NEWID(), 
    @CurrentDateTime
FROM [Block]
WHERE [BlockTypeId] = @BlockTypeId
AND [Id] NOT IN (
    SELECT [EntityId] 
    FROM [AttributeValue] 
    WHERE [AttributeId] = @AttributeId
);";

            Sql( sql );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public void ContentCollectionViewSettingDown()
        {
            // Remove any attribute value for the new attribute.
            Sql( "DELETE FROM [AttributeValue] WHERE [AttributeId] = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'FD68C0BE-1FC2-4161-AD5D-6B2371D24326');" );

            // Remove the BlockType attribute.
            RockMigrationHelper.DeleteAttribute( "FD68C0BE-1FC2-4161-AD5D-6B2371D24326" );
        }

        #endregion
    }
}

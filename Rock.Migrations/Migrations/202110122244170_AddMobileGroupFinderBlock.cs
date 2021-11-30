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
    public partial class AddMobileGroupFinderBlock : Rock.Migrations.RockMigration
    {
        const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameMobileAddToGroup_GroupRegistration();
            AddMobileGroupFinderBlockType();
            AddDefaultGroupFinderTemplate();
        }

        private void RenameMobileAddToGroup_GroupRegistration()
        {
            Sql( $@"
UPDATE [EntityType]
SET	[Name] = 'Rock.Blocks.Types.Mobile.Groups.GroupRegistration',
	[AssemblyName] = 'Rock.Blocks.Types.Mobile.Groups.GroupRegistration, Rock, Version=1.13.0.20, Culture=neutral, PublicKeyToken=null',
	[FriendlyName] = 'Group Registration'
WHERE [Guid] = '{Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_REGISTRATION_BLOCK_TYPE}'

UPDATE [BlockType]
SET [Name] = 'Group Registration'
WHERE [Guid] = '{Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_REGISTRATION}'
" );
        }

        private void AddMobileGroupFinderBlockType()
        {
            RockMigrationHelper.UpdateEntityType(
                "Rock.Blocks.Types.Mobile.Groups.GroupFinder",
                "Group Finder",
                "Rock.Blocks.Types.Mobile.Groups.GroupRegistration, Rock, Version=1.13.0.20, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_FINDER_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType(
                "Group Finder",
                "Allows a person to register for a group.",
                "Rock.Blocks.Types.Mobile.Groups.GroupFinder",
                "Mobile > Groups",
                Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_FINDER );
        }

        private void AddDefaultGroupFinderTemplate()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Groups Group Finder",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUPS_GROUP_FINDER );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "CC117DBB-5C3C-4A32-8ABA-88A7493C7F70",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUPS_GROUP_FINDER,
                "Default",
                @"{% if Groups == empty %}
    <Rock:NotificationBox NotificationType=""Warning"" Text=""No groups match your search criteria."" />
{% else %}
    <StackLayout>
        <Rock:Divider />
        {% for group in Groups %}
        {% assign distance = Distances[group.Id] }} %}
        <Grid ColumnDefinitions=""1*, 15"" ColumnSpacing=""12"" StyleClass=""group-content"">
            {% if DetailPage != null %}
                <Grid.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?GroupGuid={{ group.Guid }}"" />
                </Grid.GestureRecognizers>
            {% endif %}
            <StackLayout Grid.Column=""0"" StyleClass=""group-primary-content"">
                {% if group.Schedule.WeeklyDayOfWeek != null %}
                    <Label Text=""{{ group.Schedule.WeeklyDayOfWeek }}"" StyleClass=""group-meeting-day"" />
                {% endif %}
                <Label Text=""{{ group.Name | Escape }}"" StyleClass=""group-name"" />
                <StackLayout Orientation=""Horizontal"">
                    {% if group.Schedule.WeeklyTimeOfDay != null %}
                        <Label Text=""Weekly at {{ group.Schedule.WeeklyTimeOfDayText }}"" HorizontalOptions=""Start"" StyleClass=""group-meeting-time"" />
                    {% elsif group.Schedule != null %}
                        <Label Text=""{{ group.Schedule.FriendlyScheduleText }}"" HorizontalOptions=""Start"" StyleClass=""group-meeting-time"" />
                    {% endif %}
                    {% assign topic = group | Attribute:'Topic' %}
                    {% if topic != empty %}
                        <Label Text=""{{ topic | Escape }}"" HorizontalTextAlignment=""End"" HorizontalOptions=""EndAndExpand"" StyleClass=""group-topic"" />
                    {% endif %}
                </StackLayout>
                {% if distance != null %}
                    <Label Text=""{{ distance | Format:'#,##0.0' }} mi"" StyleClass=""group-distance"" />
                {% endif %}
            </StackLayout>

            <Rock:Icon IconClass=""chevron-right"" Grid.Column=""1"" HorizontalOptions=""End"" VerticalOptions=""Center"" StyleClass=""group-more-icon"" />
        </Grid>

        <Rock:Divider />
        {% endfor %}
    </StackLayout>
{% endif %}",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteTemplateBlockTemplate( "CC117DBB-5C3C-4A32-8ABA-88A7493C7F70" );
            RockMigrationHelper.DeleteTemplateBlock( Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUPS_GROUP_FINDER );

            RockMigrationHelper.DeleteBlockType( Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_FINDER );
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_FINDER_BLOCK_TYPE );

            Sql( $@"
UPDATE [EntityType]
SET	[Name] = 'Rock.Blocks.Types.Mobile.Groups.AddToGroup',
	[AssemblyName] = 'Rock.Blocks.Types.Mobile.Groups.AddToGroup, Rock, Version=1.12.4.1, Culture=neutral, PublicKeyToken=null',
	[FriendlyName] = 'Add To Group'
WHERE [Guid] = '{Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_REGISTRATION_BLOCK_TYPE}'

UPDATE [BlockType]
SET [Name] = 'Add To Group'
WHERE [Guid] = '{Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_REGISTRATION}'
" );
        }
    }
}

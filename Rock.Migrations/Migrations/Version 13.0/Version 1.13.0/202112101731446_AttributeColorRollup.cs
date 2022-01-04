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
    public partial class AttributeColorRollup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.Attribute", "AttributeColor", c => c.String(maxLength: 100));

            RemoveDataIntegrityMenuChange_Up();
            UpdateMobileGroupFinderDefaultTemplate_Up();
            UpdatePreSelectedOptionsFormatLava_Up();
            PastoralNoteAndFollowingEventPersonNoteAdded_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.Attribute", "AttributeColor", c => c.String(nullable: false, maxLength: 100));

            PastoralNoteAndFollowingEventPersonNoteAdded_Down();
        }

        /// <summary>
        /// GJ: Remove DataIntegrity Menu Change
        /// </summary>
        private void RemoveDataIntegrityMenuChange_Up()
        {
            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Data Integrity, Site=Rock RMS
            //   Attribute: Template /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsBlocks.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "5A428636-5522-4AFD-8FDE-228F711E51C1", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );
        }

        /// <summary>
        /// Updates the mobile group finder default template.
        /// </summary>
        private void UpdateMobileGroupFinderDefaultTemplate_Up()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

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
        {% assign distance = Distances[group.Id] %}
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
        /// SK: Updated Pre Selected Options Format Lava
        /// </summary>
        public void UpdatePreSelectedOptionsFormatLava_Up()
        {
            string newValue = "{{ Schedule.Name }} - {{ Group.Name }} - {{ Location.Name }} {% if DisplayLocationCount == true %} <span class='ml-3'>Count: {{ LocationCount }}</span> {% endif %}".Replace( "'", "''" );
            string oldValue = "{{ Schedule.Name }} - {{ Group.Name }} - {{ Location.Name }}".Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Value" );

            Sql( $@"DECLARE @attributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '55580865-E792-469F-B45C-45713477D033')
UPDATE [dbo].[AttributeValue] 
SET [Value] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
WHERE {targetColumn} NOT LIKE '%{newValue}%' AND [AttributeId] = @attributeId" );
        }

        /// <summary>
        /// CR: Pastoral Note and FollowingEvent PersonNoteAdded (Up)
        /// </summary>
        public void PastoralNoteAndFollowingEventPersonNoteAdded_Up()
        {
            RockMigrationHelper.UpdateNoteType(
                "Pastoral Note",
                "Rock.Model.Person",
                false,
                Rock.SystemGuid.NoteType.PASTORAL_NOTE,
                true,
                "fa fa-medkit",
                false );

            RockMigrationHelper.UpdateEntityType(
                "Rock.Follow.Event.PersonNoteAdded",
                "Person Note Added",
                "Rock.Follow.Event.PersonNoteAdded, Rock, Version=1.13.0.28, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.PERSON_NOTE_ADDED );

            Sql( @"
                IF NOT EXISTS ( SELECT [Id] FROM [FollowingEventType] WHERE [Guid] = '" + Rock.SystemGuid.FollowingEventType.PASTORAL_NOTE_ADDED + @"' )
                BEGIN

                    DECLARE @PersonAliasEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '" + Rock.SystemGuid.EntityType.PERSON_ALIAS + @"' )
                    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '" + Rock.SystemGuid.EntityType.PERSON_NOTE_ADDED + @"' )

            INSERT [FollowingEventType] ([Name], [Description], [EntityTypeId], [FollowedEntityTypeId], [IsActive], [SendOnWeekends], [LastCheckDateTime], [IsNoticeRequired], [EntityNotificationFormatLava], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [Guid], [ForeignKey], [Order], [ForeignGuid], [ForeignId])
                                                            VALUES (N'Pastoral Note Added', N'Sends following alerts to people who are following someone who had a note added to them.', @EntityTypeId, @PersonAliasEntityTypeId, 1, 0, NULL, 0, N'<tr>
                <td style=''padding-bottom: 12px; padding-right: 12px; min-width: 87px;''>
                    {% if Entity.Person.PhotoId %} 
                        <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
                    {% endif %}
                </td>
                <td valign=""top"" style=''padding-bottom: 12px; min-width: 300px;''>
                    <strong>{{ NoteData.AddedBy }} added a {{ NoteData.NoteType }} note to <a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}"">{{ Entity.Person.FullName }}</a>.</strong><br />
                    {% if Entity.Person.Email != empty %}
                        Email: <a href=""mailto:{{ Entity.Person.Email }}"">{{ Entity.Person.Email }}</a><br />
                    {% endif %}

                    {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
                    {% if mobilePhone != empty %}
                        Cell: {{ mobilePhone }}<br />
                    {% endif %}

                    {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
                    {% if homePhone != empty %}
                        Home: {{ homePhone }}<br />
                    {% endif %}
                </td>
            </tr>', NULL, NULL, NULL, NULL, N'" + Rock.SystemGuid.FollowingEventType.PASTORAL_NOTE_ADDED + @"', NULL, 0, NULL, NULL);
            END
            " );

            RockMigrationHelper.AddOrUpdateEntityAttribute(
                "Rock.Model.FollowingEventType",
                "Rock.Follow.Event.PersonNoteAdded",
                Rock.SystemGuid.FieldType.NOTE_TYPES,
                "NoteTypes",
                "Note Types",
                @"List of note types that may be followed.  Selecting none allows for all types to be followed.",
                0,
                @"",
                "892FD83E-75C8-4505-8D77-0EF5EC656F3C",
                "NoteTypes" );

            Sql( @"
                DECLARE 
                    @AttributeId int,
                    @EntityId int
                SET @AttributeId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '892FD83E-75C8-4505-8D77-0EF5EC656F3C')
                SET @EntityId = (SELECT TOP 1 [Id] FROM [FollowingEventType] WHERE [Guid] = '" + Rock.SystemGuid.FollowingEventType.PASTORAL_NOTE_ADDED + @"')
                IF NOT EXISTS(SELECT [Id] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @EntityId)
                BEGIN
                INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                VALUES(
                    1,@AttributeId,@EntityId,'" + Rock.SystemGuid.NoteType.PASTORAL_NOTE + @"','1c240df2-a046-4e52-95dc-24f04b5dced0')
                END" );

            RockMigrationHelper.AddSecurityAuthForNoteType(
                Rock.SystemGuid.NoteType.PASTORAL_NOTE,
                1,
                Rock.Security.Authorization.EDIT,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Rock.Model.SpecialRole.None,
                "9cf4fcf9-391e-45ae-8650-5df876fa634f" );
            RockMigrationHelper.AddSecurityAuthForNoteType(
                Rock.SystemGuid.NoteType.PASTORAL_NOTE,
                2,
                Rock.Security.Authorization.EDIT,
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                ( int ) Rock.Model.SpecialRole.None,
                "fb7c524e-2719-4213-a4e6-3b68fcfbb2e9" );
            RockMigrationHelper.AddSecurityAuthForNoteType(
                Rock.SystemGuid.NoteType.PASTORAL_NOTE,
                3,
                Rock.Security.Authorization.EDIT,
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
                ( int ) Rock.Model.SpecialRole.None,
                "a04025d2-4213-4797-8da0-cc02dd8b32f3" );
        }

        /// <summary>
        /// CR: Pastoral Note and FollowingEvent PersonNoteAdded (Down)
        /// </summary>
        public void PastoralNoteAndFollowingEventPersonNoteAdded_Down()
        {
            Sql( @"
                    DECLARE 
                    @EntityTypeId int,
                    @EntityId int
                SET @EntityTypeId = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '" + Rock.SystemGuid.EntityType.PERSON_NOTE_ADDED + @"')
                SET @EntityId = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '8a0d208b-762d-403a-a972-3a0f079866d4')
                DELETE FROM [Attribute] WHERE [EntityTypeId] = @EntityId AND [EntityTypeQualifierColumn] = 'EntityTypeId' AND [EntityTypeQualifierValue] = @EntityTypeId;" );

            Sql( $"DELETE FROM [FollowingEventType] WHERE [Guid] = '{Rock.SystemGuid.FollowingEventType.PASTORAL_NOTE_ADDED}';" );
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.PERSON_NOTE_ADDED );
            RockMigrationHelper.DeleteSecurityAuth( "9cf4fcf9-391e-45ae-8650-5df876fa634f" );
            RockMigrationHelper.DeleteSecurityAuth( "fb7c524e-2719-4213-a4e6-3b68fcfbb2e9" );
            RockMigrationHelper.DeleteSecurityAuth( "a04025d2-4213-4797-8da0-cc02dd8b32f3" );
            Sql( $"DELETE FROM [NoteType] WHERE [Guid] = '{Rock.SystemGuid.NoteType.PASTORAL_NOTE}';" );
        }
    }
}

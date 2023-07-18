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
    public partial class RemoveNoteApprovals : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.NoteType", "Color", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.NoteType", "FormatType", c => c.Int( nullable: false, defaultValue: 0 ) );
            AddColumn( "dbo.NoteType", "IsMentionEnabled", c => c.Boolean( nullable: false, defaultValue: false ) );

            MobileNotesTemplateUp();

            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_NOTE_DATA}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v16.0 - Update Note Data.'
                    , 'This job updates note data to use new format.'
                    , 'Rock.Jobs.PostV16UpdateNoteData'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_NOTE_DATA}'
                );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_NOTE_DATA}" );

            MobileNotesTemplateDown();

            DropColumn( "dbo.NoteType", "IsMentionEnabled" );
            DropColumn( "dbo.NoteType", "FormatType" );
            DropColumn( "dbo.NoteType", "Color" );
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void MobileNotesTemplateUp()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
              "Mobile > Core > Notes",
              string.Empty,
              SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_NOTES );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
              "C9134085-D433-444D-9803-8E5CE1B053DE",
              Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_NOTES,
              "Default",
              @"<StackLayout StyleClass=""notes-container"" Spacing=""0"">
    <FlexLayout JustifyContent=""SpaceBetween""
        StyleClass=""px-12"">
        <Label StyleClass=""h3""
            Text=""Notes""
        />
        {% if ListPage != null %}
            <Button StyleClass=""btn,btn-link"" 
                Text=""See All""
                HorizontalOptions=""End""
                Padding=""0""
                Command=""{Binding PushPage}""
                CommandParameter=""{{ ListPage }}?PersonGuid={{ PageParameter.PersonGuid | Escape }}"">
            </Button>
        {% else %}
            <Button StyleClass=""btn,btn-link"" 
                Text=""See All""
                HorizontalOptions=""End""
                Padding=""0""
                Command=""{Binding ShowAllNotes}"">
            </Button>
        {% endif %}
    </FlexLayout>

    {% for note in Notes %}
            <Frame Margin=""0""
                BackgroundColor=""White""
                HasShadow=""false""
                Padding=""12""
                HeightRequest=""64""
                StyleClass=""note-container"">
                <StackLayout Orientation=""Horizontal""
                    Spacing=""0"">
                    <Rock:Image StyleClass=""note-author-image""
                        VerticalOptions=""Start""
                        Aspect=""AspectFit""
                        Margin=""0, 4, 14, 0""
                        BackgroundColor=""#e4e4e4""
                        Source=""{{ note.PhotoUrl | Escape}}"">
                        <Rock:CircleTransformation />
                    </Rock:Image>
                    
                    <StackLayout Spacing=""0"" 
                        HorizontalOptions=""FillAndExpand"">
                        <StackLayout Orientation=""Horizontal"">
                            <Label StyleClass=""note-author""
                                Text=""{{ note.Name | Escape }}""
                                LineBreakMode=""TailTruncation""
                                HorizontalOptions=""FillAndExpand"" />

                            <Grid ColumnSpacing=""4"" 
                                RowSpacing=""0""
                                ColumnDefinitions=""*, Auto""
                            >
                            {% if note.Date != null %}
                                <Label StyleClass=""note-date""
                                    Text=""{{ note.Date | HumanizeDateTime }}""
                                    HorizontalTextAlignment=""End"" 
                                    HorizontalOptions=""End""
                                    VerticalTextAlignment=""Start""
                                    Grid.Column=""0""  />
                            {% endif %}
					{% if note.CanEdit %}
                                <Rock:Icon IconClass=""chevron-right""
                                    VerticalTextAlignment=""Start""
                                    Grid.Column=""1"" 
                                    StyleClass=""note-read-more-icon""
                                    />
					{% endif %}
                            </Grid>
                        </StackLayout>
                            <Label StyleClass=""note-text""
                                Grid.Column=""0""
                                MaxLines=""2""
                                LineBreakMode=""TailTruncation"" 
                                Text=""{{ note.Text | StripHtml | Escape }}"" /> 

                    </StackLayout>
                </StackLayout>
                {% if note.CanEdit %}
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding EditNote}"" 
                     CommandParameter=""{{ note.Guid }}"" />
                </Frame.GestureRecognizers>
                {% endif %}
            </Frame>
        <BoxView HorizontalOptions=""FillAndExpand""
            HeightRequest=""1""
            Color=""#cccccc""
        />
    {% endfor %}
    
    {% if CurrentPerson != null %}
        <FlexLayout JustifyContent=""SpaceBetween""
            StyleClass=""px-12"">
            <ContentView />
            <Rock:Icon 
                HorizontalOptions=""End""
                IconClass=""plus""
                HorizontalTextAlignment=""End""
                StyleClass=""btn,btn-link""
                Padding=""0, 4, 0, 0""
                VerticalTextAlignment=""Center"">
                    <Rock:Icon.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding AddNote}"" />
                    </Rock:Icon.GestureRecognizers>
            </Rock:Icon>
        </FlexLayout>
    {% endif %}
</StackLayout>",
              standardIconSvg,
              "standard-template.svg",
              "image/svg+xml" );

        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void MobileNotesTemplateDown()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
              "Mobile > Core > Notes",
              string.Empty,
              SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_NOTES );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
              "C9134085-D433-444D-9803-8E5CE1B053DE",
              Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_NOTES,
              "Default",
              @"<StackLayout StyleClass=""notes-container"" Spacing=""0"">
    <FlexLayout JustifyContent=""SpaceBetween""
        StyleClass=""px-12"">
        <Label StyleClass=""h3""
            Text=""Notes""
        />
        <Button StyleClass=""btn,btn-link"" 
            Text=""See All""
            HorizontalOptions=""End""
            Padding=""0""
            Command=""{Binding PushPage}""
            CommandParameter=""{{ ListPage }}?PersonGuid={{ PageParameter.PersonGuid }}"">
        </Button>
    </FlexLayout>

    {% for note in Notes %}
            <Frame Margin=""0""
                BackgroundColor=""White""
                HasShadow=""false""
                Padding=""12""
                HeightRequest=""64""
                StyleClass=""note-container"">
                <StackLayout Orientation=""Horizontal""
                    Spacing=""0"">
                    <Rock:Image StyleClass=""note-author-image""
                        VerticalOptions=""Start""
                        Aspect=""AspectFit""
                        Margin=""0, 4, 14, 0""
                        BackgroundColor=""#e4e4e4""
                        Source=""{{ note.PhotoUrl }}"">
                        <Rock:CircleTransformation />
                    </Rock:Image>
                    
                    <StackLayout Spacing=""0"" 
                        HorizontalOptions=""FillAndExpand"">
                        <StackLayout Orientation=""Horizontal"">
                            <Label StyleClass=""note-author""
                                Text=""{{ note.Name }}""
                                LineBreakMode=""TailTruncation""
                                HorizontalOptions=""FillAndExpand"" />

                            <Grid ColumnSpacing=""4"" 
                                RowSpacing=""0""
                                ColumnDefinitions=""*, Auto""
                            >
                            {% if note.Date != null %}
                                <Label StyleClass=""note-date""
                                    Text=""{{ note.Date | HumanizeDateTime }}""
                                    HorizontalTextAlignment=""End"" 
                                    HorizontalOptions=""End""
                                    VerticalTextAlignment=""Start""
                                    Grid.Column=""0""  />
                            {% endif %}
					{% if note.CanEdit %}
                                <Rock:Icon IconClass=""chevron-right""
                                    VerticalTextAlignment=""Start""
                                    Grid.Column=""1"" 
                                    StyleClass=""note-read-more-icon""
                                    />
					{% endif %}
                            </Grid>
                        </StackLayout>
                            <Label StyleClass=""note-text""
                                Grid.Column=""0""
                                MaxLines=""2""
                                LineBreakMode=""TailTruncation"" 
                                Text=""{{ note.Text | Escape }}"" /> 

                    </StackLayout>
                </StackLayout>
                {% if note.CanEdit %}
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding EditNote}"" 
                     CommandParameter=""{{ note.Guid }}"" />
                </Frame.GestureRecognizers>
                {% endif %}
            </Frame>
        <BoxView HorizontalOptions=""FillAndExpand""
            HeightRequest=""1""
            Color=""#cccccc""
        />
    {% endfor %}
    
    {% if CurrentPerson != null %}
        <FlexLayout JustifyContent=""SpaceBetween""
            StyleClass=""px-12"">
            <ContentView />
            <Rock:Icon 
                HorizontalOptions=""End""
                IconClass=""plus""
                HorizontalTextAlignment=""End""
                StyleClass=""btn,btn-link""
                Padding=""0, 4, 0, 0""
                VerticalTextAlignment=""Center"">
                    <Rock:Icon.GestureRecognizers>
                        <TapGestureRecognizer Command=""{Binding AddNote}"" />
                    </Rock:Icon.GestureRecognizers>
            </Rock:Icon>
        </FlexLayout>
    {% endif %}
</StackLayout>",
              standardIconSvg,
              "standard-template.svg",
              "image/svg+xml" );

        }
    }
}

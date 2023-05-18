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
using System;

namespace Rock.Migrations
{

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20230209 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            EnableTextToGiveMultiAcct();
            MobileNotesTemplateUp();
            UpdateReminderSystemsCommunications();
            AddSnippetListPageAndSnippetDetailPageUp();
            UpdatespCrmPersonDuplicateFinderProcedure();
            UpdateKioskBlockNamesDeprecatedUp();
            UpUpdateExceptionLogKeepDays();
            FixUniversalSearchResultsURL();
            UpdateProcessRemindersJobSchedule();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddSnippetListPageAndSnippetDetailPageDown();
            UpdateKioskBlockNamesDeprecatedDown();
        }

        private void EnableTextToGiveMultiAcct()
        {
            // Set "Enable Multi-Account" Block Attribute Value for the Text To Give Utility Payment Entry block.
            RockMigrationHelper.AddBlockAttributeValue( true,
                "9684D991-8B26-4D39-BAD5-B520F91D27B8", // Utility Payment Entry block instance for Text To Give.
                "BF331D8A-E872-4E4A-8A44-57121D8B6E63", // "Enable Multi-Account" Attribute for "Utility Payment Entry" BlockType.
                "False" );
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

        /// <summary>
        /// GJ: Update Reminders System Communication
        /// </summary>
        private void UpdateReminderSystemsCommunications()
        {
            Sql( MigrationSQL._202302092033593_Rollup_20230209_UpdateRemindersSystemCommunication );
        }

        /// <summary>
        /// KA: Migration to Add SnippetListPage and SnippetDetailPage 
        /// </summary>
        private void AddSnippetListPageAndSnippetDetailPageUp()
        {
            // Add SMS Snippet Type
            Sql( @"DECLARE @Guid uniqueidentifier = (SELECT [Guid] FROM [SnippetType] WHERE [Name] = 'SMS');
                IF @Guid IS NULL
                BEGIN
                    INSERT INTO [SnippetType] (
                        [Name],[Description],[HelpText],[IsPersonalAllowed],[IsSharedAllowed],[Guid])
                    VALUES(
                        'SMS', 'Snippets to be used for various SMS replies.', '', 1,1,'D6074803-9405-47E3-974C-E95C9AD05874')
                END" );

            // Add Snippets List Page to Communications Page
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.COMMUNICATIONS_ROCK_SETTINGS, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "SMS Snippets", string.Empty, "67661F85-ECA6-4791-AE7A-D1454D7B1FEB", "fa fa-keyboard" );

            // Add Snippet Details Page
            RockMigrationHelper.AddPage( true, "67661F85-ECA6-4791-AE7A-D1454D7B1FEB", SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Snippet Detail", string.Empty, "E315FCD1-3942-415E-BED2-E30428928955" );

            // Add Snippet Detail Obsidian block entity Type
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.SnippetDetail", "Snippet Detail", "Rock.Blocks.Communication.SnippetDetail, Rock.Blocks, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "4b445492-20e7-41e3-847a-f4d4723e9973" );

            // Add Snippet Detail Obsidian block
            UpdateBlockType( "Snippet Detail", "Displays details of a particular Snippet.", null, "Communication", "8b0f3048-99ba-4ed1-8de6-6a34f498f556", "4b445492-20e7-41e3-847a-f4d4723e9973" );

            // Update Snippet Type block setting attribute.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8b0f3048-99ba-4ed1-8de6-6a34f498f556", SystemGuid.FieldType.SINGLE_SELECT, "Snippet Type", "SnippetType", "Snippet Type", "", 0, "D6074803-9405-47E3-974C-E95C9AD05874", "48AE0214-5AB4-4307-ADD8-78BFB30462E0" );

            // Add Snippet Detail block to Snippet Detail Page
            RockMigrationHelper.AddBlock( true, "E315FCD1-3942-415E-BED2-E30428928955".AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "8B0F3048-99BA-4ED1-8DE6-6A34F498F556".AsGuid(), "Snippet Detail", "Main", @"", @"", 0, "DA0A5925-499C-4CE3-8ACA-A40C9D30B1CB" );

            // Set Snippet Type block setting Attribute Value to SMS Snippet Type for the newly added Snippet Detail block.
            RockMigrationHelper.AddBlockAttributeValue( "DA0A5925-499C-4CE3-8ACA-A40C9D30B1CB", "48AE0214-5AB4-4307-ADD8-78BFB30462E0", "D6074803-9405-47E3-974C-E95C9AD05874" );

            // Add Snippet List block Type
            RockMigrationHelper.UpdateBlockType( "Snippet List", "Displays Snippets to be used for various SMS replies.", "~/Blocks/Communication/SnippetList.ascx", "Communication", "2EDAD934-6129-480B-9812-4BA7B9978AD2" );

            // Update Snippet Detail block setting attribute guid value.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EDAD934-6129-480B-9812-4BA7B9978AD2", SystemGuid.FieldType.PAGE_REFERENCE, "Snippet Detail", "SnippetDetail", "Snippet Detail", "", 0, "", "58C5B522-7A82-4304-B1D4-CDDB73980B95" );

            // Update Snippet Type block setting attribute guid value.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EDAD934-6129-480B-9812-4BA7B9978AD2", SystemGuid.FieldType.SINGLE_SELECT, "Snippet Type", "SnippetType", "Snippet Type", "", 1, "", "A2CF5EF3-36A5-4340-A8BA-03C9BF7BEC96" );

            // Update Show Personal Column block setting attribute guid value.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EDAD934-6129-480B-9812-4BA7B9978AD2", SystemGuid.FieldType.BOOLEAN, "Show Personal Column", "ShowPersonalColumn", "Show Personal Column", "", 2, "", "B83AAC68-1480-40DD-8812-12A7A2E72BD8" );

            // Add Snippet List block to Snippet List Page
            RockMigrationHelper.AddBlock( true, "67661F85-ECA6-4791-AE7A-D1454D7B1FEB".AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "2EDAD934-6129-480B-9812-4BA7B9978AD2".AsGuid(), "Snippet List", "Main", @"", @"", 0, "7171F489-F370-47BB-B012-14DB03011E1A" );

            // Set Snippet Type block setting Attribute Value to SMS Snippet Type for the newly added Snippet List block.
            RockMigrationHelper.AddBlockAttributeValue( "7171F489-F370-47BB-B012-14DB03011E1A", "A2CF5EF3-36A5-4340-A8BA-03C9BF7BEC96", "D6074803-9405-47E3-974C-E95C9AD05874" );

            // Update Snippet List Page block setting DetailPage Attribute Value to SnippetDetail page for the newly added Snippet List block.
            RockMigrationHelper.AddBlockAttributeValue( "7171F489-F370-47BB-B012-14DB03011E1A", "58C5B522-7A82-4304-B1D4-CDDB73980B95", "E315FCD1-3942-415E-BED2-E30428928955" );

            // Update Snippet List Page block setting ShowPersonal Attribute Value to True for the newly added Snippet List block.
            RockMigrationHelper.AddBlockAttributeValue( "7171F489-F370-47BB-B012-14DB03011E1A", "B83AAC68-1480-40DD-8812-12A7A2E72BD8", "True" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void AddSnippetListPageAndSnippetDetailPageDown()
        {
            // Remove Block - Name: Snippet List, from Page: SMS Snippets, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7171F489-F370-47BB-B012-14DB03011E1A" );

            // Remove Block - Name: Snippet Detail, from Page: Snippet Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DA0A5925-499C-4CE3-8ACA-A40C9D30B1CB" );

            // Delete Page Internal Name: Snippet Detail: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( "E315FCD1-3942-415E-BED2-E30428928955" );

            // Delete Page Internal Name: SMS Snippets: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( "67661F85-ECA6-4791-AE7A-D1454D7B1FEB" );
        }

        /// <summary>
        /// Updates the BlockType by Guid and sets the entity type id.
        /// otherwise it inserts a new record. In either case it will be marked IsSystem.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="path">The path.</param>
        /// <param name="category">The category.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="entityTypeGuid">The entity type GUID.</param>
        private void UpdateBlockType( string name, string description, string path, string category, string guid, string entityTypeGuid )
        {
            Sql( string.Format( @"
                DECLARE @Id int = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{4}' )
                DECLARE @EntityTypeId int = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{5}' )
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [BlockType] (
                        [IsSystem],[Path],[Category],[Name],[Description],
                        [Guid],[EntityTypeId])
                    VALUES(
                        1,'{0}','{1}','{2}','{3}',
                        '{4}',@EntityTypeId)
                END
                ELSE
                BEGIN
                    UPDATE [BlockType] SET
                        [IsSystem] = 1,
                        [Category] = '{1}',
                        [Name] = '{2}',
                        [Description] = '{3}',
                        [Guid] = '{4}',
                        [EntityTypeId] = @EntityTypeId
                    WHERE [Guid] = '{4}'
                END
",
                    path,
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid,
                    entityTypeGuid
                    ) );
        }

        /// <summary>
        /// KA: Migration to update spCrm_PersonDuplicateFinder procedure
        /// </summary>
        private void UpdatespCrmPersonDuplicateFinderProcedure()
        {
            Sql( MigrationSQL._202302092033593_Rollup_20230209_UpdateRemindersSystemCommunication );
        }

        /// <summary>
        /// CR: Deprecate "Kiosk" block types rename
        /// </summary>
        private void UpdateKioskBlockNamesDeprecatedUp()
        {
            string personUpdateKioskBlockTypeName = $@"
            	    UPDATE [BlockType]
	                SET [Name] = 'Person Update - Kiosk (deprecated)'
	                WHERE [Guid] = '61C5C8F2-6F76-4583-AB97-228878A6AB65'";
            Sql( personUpdateKioskBlockTypeName );

            string transactionEntryKioskBlockTypeName = $@"
            	    UPDATE [BlockType]
	                SET [Name] = 'Transaction Entry - Kiosk (deprecated)'
	                WHERE [Guid] = 'D10900A8-C2C1-4414-A443-3781A5CF371C'";
            Sql( transactionEntryKioskBlockTypeName );

            string prayerRequestEntryKioskBlockTypeName = $@"
            	    UPDATE [BlockType]
	                SET [Name] = 'Prayer Request Entry - Kiosk (deprecated)'
	                WHERE [Guid] = '9D8ED334-F1F5-4377-9E27-B8C0852CF34D'";
            Sql( prayerRequestEntryKioskBlockTypeName );
        }

        /// <summary>
        /// CR: Deprecate "Kiosk" block types rename
        /// </summary>
        private void UpdateKioskBlockNamesDeprecatedDown()
        {
            string personUpdateKioskBlockTypeName = $@"
            	    UPDATE [BlockType]
	                SET [Name] = 'Person Update - Kiosk'
	                WHERE [Guid] = '61C5C8F2-6F76-4583-AB97-228878A6AB65'";
            Sql( personUpdateKioskBlockTypeName );

            string transactionEntryKioskBlockTypeName = $@"
            	    UPDATE [BlockType]
	                SET [Name] = 'Transaction Entry - Kiosk'
	                WHERE [Guid] = 'D10900A8-C2C1-4414-A443-3781A5CF371C'";
            Sql( transactionEntryKioskBlockTypeName );

            string prayerRequestEntryKioskBlockTypeName = $@"
            	    UPDATE [BlockType]
	                SET [Name] = 'Prayer Request Entry - Kiosk'
	                WHERE [Guid] = '9D8ED334-F1F5-4377-9E27-B8C0852CF34D'";
            Sql( prayerRequestEntryKioskBlockTypeName );
        }

        /// <summary>
        /// MP: Update RockCleanup ExceptionKeepDays
        /// Updates the RockCleanup job to keep 30 days of exceptions instead of 14.
        /// </summary>
        private void UpUpdateExceptionLogKeepDays()
        {
            Sql( $@"
DECLARE @serviceJobEntityTypeId INT = (
        SELECT TOP 1 Id
        FROM EntityType
        WHERE [Guid] = '{Rock.SystemGuid.EntityType.SERVICE_JOB}'
        )
    , @rockCleanupJobId INT = (
        SELECT TOP 1 id
        FROM ServiceJob
        WHERE Class = 'Rock.Jobs.RockCleanup'
        )

DECLARE @DaysKeepExceptionsAttributeId INT = (
        SELECT TOP 1 Id
        FROM [Attribute]
        WHERE [Key] = 'DaysKeepExceptions'
            AND EntityTypeId = @serviceJobEntityTypeId
        )

UPDATE Attribute
SET DefaultValue = '30'
WHERE Id = @DaysKeepExceptionsAttributeId
    AND DefaultValue = '14';

UPDATE AttributeValue
SET [Value] = '30'
WHERE [Value] = '14'
    AND AttributeId = @DaysKeepExceptionsAttributeId
    AND EntityId = @rockCleanupJobId

" );
        }

        /// <summary>
        /// DL: Fix Universal Search Results URL
        /// </summary>
        private void FixUniversalSearchResultsURL()
        {
            Sql( @"
DECLARE @UniversalSearchEntityTypeId INT = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = 'bd0faac1-2313-4d36-8b78-268715320f02')
DECLARE @UniversalSearchUrlAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [EntityTypeId] = @UniversalSearchEntityTypeId AND [Key] = 'ResultURL')

UPDATE 
    [AttributeValue]
SET 
   [Value] = REPLACE([Value],'/{0}?','?q={0}&')
WHERE [AttributeId] = @UniversalSearchUrlAttributeId
   AND [Value] LIKE '%universalsearch%'
" );
        }

        /// <summary>
        /// SC: Update Process Reminders Job schedule
        /// </summary>
        private void UpdateProcessRemindersJobSchedule()
        {
            Sql( @"UPDATE [ServiceJob] SET [CronExpression] = '0 0 6 ? * MON-FRI *' WHERE [Guid] = '3F697C80-4C33-4552-9038-D3470445EA40'" );
        }
    }
}

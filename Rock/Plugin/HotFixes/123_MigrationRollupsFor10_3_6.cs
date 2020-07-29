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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 123, "1.10.2" )]
    public class MigrationRollupsFor10_3_6 : Migration
    {
        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //UpdateMobileBlockTemplates();
            //AddMobileNotesChannel();
            //EnableLabelReprintingByDefault();
            //AddStructuredContentMessageNote();
            //AttendanceSelfEntryBlockSetting();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// DH: Update mobile block templates for Id => Guid conversion.
        /// </summary>
        private void UpdateMobileBlockTemplates()
        {
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "6207AF10-B6C9-40B5-8AA5-4C11FA6D0966",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_VIEW,
                "Default",
                @"<StackLayout Spacing=""0"">
    <StackLayout Orientation=""Horizontal"" Spacing=""20"">
        <StackLayout Orientation=""Vertical"" Spacing=""0"" HorizontalOptions=""FillAndExpand"">
            <Label StyleClass=""h1"" Text=""{{ Group.Name | Escape }} Group"" />
            <Label Text=""{{ Group.Members | Size }} members"" LineHeight=""0.8"" />
        </StackLayout>
        {% if GroupEditPage != '' and AllowedActions.Edit == true %}
        <Rock:Icon IconClass=""Ellipsis-v"" FontSize=""24"" TextColor=""#ccc"" Command=""{Binding ShowActionPanel}"">
            <Rock:Icon.CommandParameter>
                <Rock:ShowActionPanelParameters Title=""Group Actions"" CancelTitle=""Cancel"">
                    <Rock:ActionPanelButton Title=""Edit Group"" Command=""{Binding PushPage}"" CommandParameter=""{{ GroupEditPage }}?GroupGuid={{ Group.Guid }}"" />
                </Rock:ShowActionPanelParameters>
            </Rock:Icon.CommandParameter>
        </Rock:Icon>
        {% endif %}
    </StackLayout>

    <BoxView Color=""#ccc"" HeightRequest=""1"" Margin=""0, 30, 0, 10"" />

    <!-- Handle Group Attributes -->
    {% if VisibleAttributes != empty %}
        <Rock:ResponsiveLayout>
        {% for attribute in VisibleAttributes %}
            <Rock:ResponsiveColumn ExtraSmall=""6"">
                <Rock:FieldContainer>
                    <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
                </Rock:FieldContainer>
            </Rock:ResponsiveColumn>
        </Rock:ResponsiveLayout>
        {% endfor %}
    {% endif %}

    <!-- Handle displaying of leaders -->
    {% if ShowLeaderList == true %}
        <Label Text=""Leaders"" StyleClass=""field-title"" Margin=""0, 40, 0, 0"" />
        <Grid RowSpacing=""0"" ColumnSpacing=""20"">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width=""Auto"" />
                <ColumnDefinition Width=""*"" />
            </Grid.ColumnDefinitions>
        {% assign row = 0 %}
        {% assign members = Group.Members | OrderBy:'Person.FullName' %}
        {% for member in members %}
            {% if member.GroupRole.IsLeader == false %}{% continue %}{% endif %}
            <Label Grid.Row=""{{ row }}"" Grid.Column=""0"" Text=""{{ member.Person.FullName }}"" />
            <Label Grid.Row=""{{ row }}"" Grid.Column=""1"" Text=""{{ member.GroupRole.Name }}"" />
            {% assign row = row | Plus:1 %}
        {% endfor %}
        </Grid>
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "F093A516-6D95-429E-8EEB-1DFB0303DF71",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_VIEW,
                "Default",
                @"<StackLayout Spacing=""0"">
    <Label StyleClass=""h1"" Text=""{{ Member.Group.Name | Escape }} Group"" />
    <Label Text=""{{ Member.Group.Members | Size }} members"" LineHeight=""0.8"" />

    <StackLayout Orientation=""Horizontal"" Spacing=""20"" Margin=""0, 20, 0, 40"">
            <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% if Member.Person.PhotoId != null %}{{ Member.Person.PhotoUrl | Append:'&width=120' | Escape }}{% else %}{{ Member.Person.PhotoUrl | Escape }}{% endif %}"" WidthRequest=""80"">
                <Rock:CircleTransformation />
            </Rock:Image>
            <StackLayout Spacing=""0"" VerticalOptions=""Center"">
                <Label FontSize=""20"" FontAttributes=""Bold"" Text=""{{ Member.Person.FullName | Escape }}"" />
                {% if Member.Person.BirthDate != null %}
                    <Label LineHeight=""0.85"" TextColor=""#888"" Text=""Age: {{ Member.Person.AgePrecise | Floor }}"" />
                    <Label LineHeight=""0.85"" TextColor=""#888"" Text=""Birthdate: {{ Member.Person.BirthDate | Date:'MMMM' }} {{ Member.Person.BirthDate | Date:'d' | NumberToOrdinal }}"" />
                {% endif %}
            </StackLayout>
    </StackLayout>

    <!-- Handle Member Attributes -->
    {% if VisibleAttributes != empty %}
        {% for attribute in VisibleAttributes %}
        <Rock:FieldContainer Margin=""0, 0, 0, {% if forloop.last %}40{% else %}10{% endif %}"">
            <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
        </Rock:FieldContainer>
        {% endfor %}
    {% endif %}

    <!-- Contact options -->
    {% assign hasContact = false %}
    {% if Member.Person.Email != '' %}
        {% assign hasContact = true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        <StackLayout Orientation=""Horizontal"" Padding=""12"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ Member.Person.Email | Escape }}"" />
                <Label Text=""Email"" />
            </StackLayout>
            <Rock:Icon IconClass=""Envelope"" FontSize=""36"" Command=""{Binding SendEmail}"" CommandParameter=""{{ Member.Person.Email | Escape }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% assign phoneNumber = Member.Person | PhoneNumber:'Mobile' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Mobile',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        <StackLayout Orientation=""Horizontal"" Padding=""12"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Mobile"" />
            </StackLayout>
            <Rock:Icon IconClass=""Comment"" FontSize=""36"" Command=""{Binding SendSms}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
            <Rock:Icon IconClass=""Phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% assign phoneNumber = Member.Person | PhoneNumber:'Home' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Home',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        <StackLayout Orientation=""Horizontal"" Padding=""12"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Home"" />
            </StackLayout>
            <Rock:Icon IconClass=""Phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% if hasContact == true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
    {% endif %}

    {% if GroupMemberEditPage != '' %}
        <Button StyleClass=""btn,btn-primary"" Text=""Edit"" Margin=""0, 40, 0, 0"" WidthRequest=""200"" HorizontalOptions=""Center"" Command=""{Binding PushPage}"" CommandParameter=""{{ GroupMemberEditPage }}?GroupMemberGuid={{ Member.Guid }}"" />
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "674CF1E3-561C-430D-B4A8-39957AC1BCF1",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST,
                "Default",
                @"<StackLayout>
    <Label StyleClass=""h1"" Text=""{{ Title | Escape }}"" />
    <Label Text=""{{ Members | Size }} members"" LineHeight=""0.8"" />

    {% if Members != empty %}
    <StackLayout Spacing=""0"" Margin=""0,20,0,0"">
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        {% for member in Members %}
        <StackLayout Orientation=""Horizontal"" HeightRequest=""60"">
            <StackLayout.GestureRecognizers>
                <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?GroupMemberGuid={{ member.Guid }}"" />
            </StackLayout.GestureRecognizers>
            {%- if member.PhotoId != null -%}
                <Rock:Image Source=""{{ member.PhotoUrl | Append:'&width=60' | Escape }}"" WidthRequest=""60"" BackgroundColor=""#ccc"" />
            {%- else -%}
                <Rock:Image Source=""{{ member.PhotoUrl | Escape }}"" WidthRequest=""60"" BackgroundColor=""#ccc"" />
            {%- endif -%}

            <StackLayout Spacing=""0"" HorizontalOptions=""FillAndExpand"" VerticalOptions=""Center"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ member.FullName | Escape }}"" />
                <Label LineHeight=""0.85"" FontSize=""12"" TextColor=""#888"" Text=""{{ member.GroupRole | Escape }}"" />
            </StackLayout>
            <Rock:Icon IconClass=""chevron-right"" Margin=""0,0,20,0"" VerticalOptions=""Center"" />
        </StackLayout>
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        {% endfor %}
    </StackLayout>
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }

        /// <summary>
        /// JE:  Migrationfor Mobile - Added Notes Channel
        /// </summary>
        private void AddMobileNotesChannel()
        {
            Sql( @"
-- Add new Content Channel Type for Message Notes
IF NOT EXISTS( SELECT * FROM [ContentChannelType] WHERE [Guid] = '48951e97-0e45-4494-b87c-4eb9fca067eb' )
BEGIN
    INSERT INTO [ContentChannelType]
           ([IsSystem], [Name], [DateRangeType], [Guid], [DisablePriority], [IncludeTime], [DisableContentField], [DisableStatus], [ShowInChannelList])
     VALUES
           (1, 'Podcast Message Notes', 1, '48951e97-0e45-4494-b87c-4eb9fca067eb', 1, 0, 0, 1, 1)
END

-- Add new Content Channel for Message Notes
IF NOT EXISTS( SELECT * FROM [ContentChannel] WHERE [Guid] = '888ef5ea-e075-4a56-a61c-13a6dad93d6f' )
BEGIN

DECLARE @NotesContentChannelTypeId int = (SELECT TOP 1 [Id] FROM [ContentChannelType] WHERE [Guid] = '48951e97-0e45-4494-b87c-4eb9fca067eb')
DECLARE @NoteStructuredToolValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '31c63fb9-1365-4eef-851d-8ab9a188a06c')

INSERT INTO [dbo].[ContentChannel]
           ([ContentChannelTypeId], [Name], [Description], [IconCssClass], [RequiresApproval], [EnableRss], [ChannelUrl], [ItemUrl], [TimeToLive], [Guid], [ContentControlType], [RootImageDirectory], [ItemsManuallyOrdered], [ChildItemsManuallyOrdered], [IsIndexEnabled], [IsTaggingEnabled], [ItemTagCategoryId], [IsStructuredContent], [StructuredContentToolValueId])
     VALUES
           ( @NotesContentChannelTypeId, 'Message Notes', 'Notes for messages that are being podcasted.', 'fa fa-sticky-note', 0, 0, '', '', 0, '48951e97-0e45-4494-b87c-4eb9fca067eb', 0, '', 0, 0, 0, 0, null, 1, @NoteStructuredToolValueId)

END

-- Add new note content channel as a child to the existing message notes content channel
DECLARE @MessageContentChannelId int = (SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = '0a63a427-e6b5-2284-45b3-789b293c02ea')
DECLARE @MessageNotesContentChannelId int = (SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = '48951e97-0e45-4494-b87c-4eb9fca067eb')

IF NOT EXISTS( SELECT * FROM [ContentChannelAssociation] WHERE [ContentChannelId] = @MessageContentChannelId AND [ChildContentChannelId] = @MessageNotesContentChannelId )
BEGIN
    INSERT INTO [ContentChannelAssociation]
        ([ContentChannelId], [ChildContentChannelId])
    VALUES  
        (@MessageContentChannelId, @MessageNotesContentChannelId)
END

-- Remove the word Podcast from existing channels
UPDATE [ContentChannel]
SET [Name] = 'Messages'
WHERE [Name] = 'Podcast Message'

UPDATE [ContentChannel]
SET [Name] = 'Message Series'
WHERE [Name] = 'Podcast Series'" );
        }

        /// <summary>
        /// SK: Enable Label Reprinting by Default in the Check-in System's Welcome.ascx Block
        /// </summary>
        private void EnableLabelReprintingByDefault()
        {

            Sql( $@"
                    DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '563FF060-D7FD-4704-A4AA-8F4B6D4F75CE')

                    IF @AttributeId IS NOT NULL
                    BEGIN
                        -- Update the primary attribute
                        UPDATE [Attribute] SET [DefaultValue] = 'True' 
                        WHERE [Id] = @AttributeId

                        UPDATE [AttributeValue] SET [Value] = 'True' 
                        WHERE [AttributeId] = @AttributeId
                    END" );
        }

        /// <summary>
        /// GJ: Added new Message Note defined value for Structured Content
        /// </summary>
        private void AddStructuredContentMessageNote()
        {
            RockMigrationHelper.UpdateDefinedValue("E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Default",
                "{     header: {     class: Header,     inlineToolbar: ['link'],     config: {         placeholder: 'Header'     },     shortcut: 'CMD+SHIFT+H'     },     image: {     class: SimpleImage,     inlineToolbar: ['link'],     },     list: {     class: List,     inlineToolbar: true,     shortcut: 'CMD+SHIFT+L'     },     checklist: {     class: Checklist,     inlineToolbar: true,     },     quote: {     class: Quote,     inlineToolbar: true,     config: {         quotePlaceholder: 'Enter a quote',         captionPlaceholder: 'Quote\'s author',     },     shortcut: 'CMD+SHIFT+O'     },     warning: Warning,     marker: {     class:  Marker,     shortcut: 'CMD+SHIFT+M'     }, code: {     class:  CodeTool,     shortcut: 'CMD+SHIFT+C'     },     delimiter: Delimiter,     inlineCode: {     class: InlineCode,     shortcut: 'CMD+SHIFT+C'     },     linkTool: LinkTool,     embed: Embed,     table: {     class: Table,     inlineToolbar: true,     shortcut: 'CMD+ALT+T'     } }",
                "09B25845-B879-4E69-87E9-003F9380B8DD",
                false);

            RockMigrationHelper.UpdateDefinedValue("E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Message Notes",
                "{     header: {     class: Header,     inlineToolbar: ['link'],     config: {         placeholder: 'Header'     },     shortcut: 'CMD+SHIFT+H'     },     image: {     class: SimpleImage,     inlineToolbar: ['link'],     },     list: {     class: List,     inlineToolbar: true,     shortcut: 'CMD+SHIFT+L'     },     checklist: {     class: Checklist,     inlineToolbar: true,     },     quote: {     class: Quote,     inlineToolbar: true,     config: {         quotePlaceholder: 'Enter a quote',         captionPlaceholder: 'Quote\'s author',     },     shortcut: 'CMD+SHIFT+O'     },     warning: Warning,     marker: {     class:  Marker,     shortcut: 'CMD+SHIFT+M'     },     fillin: {     class:  FillIn,     shortcut: 'CMD+SHIFT+F'     },     code: {     class:  CodeTool,     shortcut: 'CMD+SHIFT+C'     },     note: {     class:  NoteTool,     shortcut: 'CMD+SHIFT+N'     },     delimiter: Delimiter,     inlineCode: {     class: InlineCode,     shortcut: 'CMD+SHIFT+C'     },     linkTool: LinkTool,     embed: Embed,     table: {     class: Table,     inlineToolbar: true,     shortcut: 'CMD+ALT+T'     } }",
                "31C63FB9-1365-4EEF-851D-8AB9A188A06C",
                false);
        }

        /// <summary>
        /// SK: Add block setting to the Attendance Self Entry block.
        /// </summary>
        private void AttendanceSelfEntryBlockSetting()
        {
            // Attrib for BlockType: Attendance Self Entry:Hide Individuals Younger Than
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Hide Individuals Younger Than", "HideIndividualsYoungerThan", "Hide Individuals Younger Than", @"The age that should be used as the cut-off for displaying on the attendance list. The value of 14 will hide individuals younger than 14. Individuals without an age will always be shown. Defaults to blank.", 25, @"", "1612C057-122C-4110-9E06-2750828653FD" );
            // Attrib for BlockType: Attendance Self Entry:Hide Individuals In Grade Less Than
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Hide Individuals in Grade Less Than", "HideIndividualsInGradeLessThan", "Hide Individuals in Grade Less Than", @"Individuals in grades lower than this value will not be show on the attendance list. Defaults to empty (not set).", 26, @"", "8F12D1F7-0855-43E5-9D6D-F5F8F3F8F13F" );  
        }
    }
}

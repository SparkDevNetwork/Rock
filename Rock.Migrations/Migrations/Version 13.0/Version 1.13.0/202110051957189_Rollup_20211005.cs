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
    public partial class Rollup_20211005 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CommunicationTemplatesPreHeader();
            ShowWebFarmAndMessageBusPages();
            LoggingDomainsBusValueUp();
            RockMobilePrayerCardViewBlockUp();
            UpdateStructuredContentDefinedValuesUp();
            DeletePersonGivingAttributes();
            UpdatePageRoutes();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            LoggingDomainsBusValueDown();
            RockMobilePrayerCardViewBlockDown();

        }

        /// <summary>
        /// GJ: Update CommunicationTemplates to include preheader text.
        /// </summary>
        private void CommunicationTemplatesPreHeader()
        {
            Sql( @"
                UPDATE [CommunicationTemplate]
                SET [Message] = replace([Message], '<body style=""-moz-box-sizing: border-box; -ms-text-size-adjust: 100%; -webkit-box-sizing: border-box; -webkit-text-size-adjust: 100%; Margin: 0; background: {{ bodyBackgroundColor }} !important; box-sizing: border-box; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; min-width: 100%; padding: 0; text-align: left; width: 100% !important;"">','<body style=""-moz-box-sizing: border-box; -ms-text-size-adjust: 100%; -webkit-box-sizing: border-box; -webkit-text-size-adjust: 100%; Margin: 0; background: {{ bodyBackgroundColor }} !important; box-sizing: border-box; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; min-width: 100%; padding: 0; text-align: left; width: 100% !important;"">' + CHAR(13) + '<div id=""preheader-text"" style=""display: none; font-size: 1px; color: #ffffff; line-height: 1px; font-family: Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;""> </div>')
                WHERE [Guid] IN ( '88B7DF18-9C30-4BAC-8CA2-5AD253D57E4D','4805E5DB-ED2B-415F-9ECD-1FC476EF085C' )
                AND [ModifiedByPersonAliasId] IS NULL
                AND [Message] NOT LIKE N'%preheader-text%'


                UPDATE [CommunicationTemplate]
                SET [Message] = replace([Message], '<body style=""margin: 0 !important; padding: 0 !important;"">','<body style=""margin: 0 !important; padding: 0 !important;"">' + CHAR(13) + '<div id=""preheader-text"" style=""display: none; font-size: 1px; color: #ffffff; line-height: 1px; font-family: Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;""> </div>')
                WHERE [Guid] = 'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A'
                AND [ModifiedByPersonAliasId] IS NULL
                AND [Message] NOT LIKE N'%preheader-text%'" );
        }

        /// <summary>
        /// SK: Web Farm and Message Bus Pages
        /// </summary>
        private void ShowWebFarmAndMessageBusPages()
        {
            Sql( @"UPDATE
	                [Page]
                  SET [DisplayInNavWhen] = 0
                  WHERE [Guid] IN ('249BE98D-9DDE-4B19-9D97-9C76D9EA3056','0FF43CC8-1C29-4882-B2F6-7B6F4C25FE41')" );
        }

        /// <summary>
        /// SK: Added new 'Bus' defined value to the Logging Domains defined type
        /// </summary>
        private void LoggingDomainsBusValueUp()
        {
            RockMigrationHelper.UpdateDefinedValue( "60487370-DE7E-4962-B58F-1865303F0414", "Bus", "", "78fe645a-89a3-4764-aa40-72fa6bb806f1", false );
        }

        /// <summary>
        /// SK: Added new 'Bus' defined value to the Logging Domains defined type
        /// </summary>
        private void LoggingDomainsBusValueDown()
        {
            RockMigrationHelper.DeleteDefinedValue("78fe645a-89a3-4764-aa40-72fa6bb806f1"); // Bus
        }

        /// <summary>
        /// DH: Add Mobile Prayer Card View block.
        /// </summary>
        private void RockMobilePrayerCardViewBlockUp()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            // Mobile Prayer > Prayer Card View.
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Prayer.PrayerCardView",
                "Answer To Prayer",
                "Rock.Blocks.Types.Mobile.Prayer.PrayerCardView, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_PRAYER_PRAYER_CARD_VIEW_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "Prayer Card View",
                "Provides an additional experience to pray using a card based view.",
                "Rock.Blocks.Types.Mobile.Prayer.PrayerCardView",
                "Mobile > Prayer",
                Rock.SystemGuid.BlockType.MOBILE_PRAYER_PRAYER_CARD_VIEW );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Prayer Prayer Card View",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_PRAYER_CARD_VIEW );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "757935E7-AB6D-47B6-A6C4-1CA5920C922E",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_PRAYER_CARD_VIEW,
                "Default",
                @"<Rock:ResponsiveLayout>
{% for item in PrayerRequestItems %}
    <Rock:ResponsiveColumn Medium=""6"">
        <Frame StyleClass=""prayer-card-container"" HasShadow=""false"">
            <StackLayout>
                <Label Text=""{{ item.FirstName | Escape }} {{ item.LastName | Escape }}"" StyleClass=""prayer-card-name"" />

                <ContentView StyleClass=""prayer-card-category"" HorizontalOptions=""Start"">
                    <Label Text=""{{ item.Category.Name | Escape }}"" />
                </ContentView>

                <Label StyleClass=""prayer-card-text"">{{ item.Text | XamlWrap }}</Label>

                <Button x:Name=""PrayedBtn{{ forloop.index }}""
                    IsVisible=""false""
                    StyleClass=""btn,btn-primary,prayer-card-prayed-button""
                    HorizontalOptions=""End""
                    Text=""Prayed""
                    IsEnabled=""false"" />
                <Button x:Name=""PrayBtn{{ forloop.index }}""
                    StyleClass=""btn,btn-primary,prayer-card-pray-button""
                    HorizontalOptions=""End""
                    Text=""Pray""
                    Command=""{Binding AggregateCommand}"">
                    <Button.CommandParameter>
                        <Rock:AggregateCommandParameters>
                            <Rock:CommandReference Command=""{Binding PrayForRequest}""
                                CommandParameter=""{Rock:PrayForRequestParameters Guid={{ item.Guid }}, WorkflowTypeGuid='{{ PrayedWorkflowType }}'}"" />

                            <Rock:CommandReference Command=""{Binding SetViewProperty}""
                                CommandParameter=""{Rock:SetViewPropertyParameters View={x:Reference PrayedBtn{{ forloop.index }}}, Name=IsVisible, Value=true}"" />

                            <Rock:CommandReference Command=""{Binding SetViewProperty}""
                                CommandParameter=""{Rock:SetViewPropertyParameters View={x:Reference PrayBtn{{ forloop.index }}}, Name=IsVisible, Value=false}"" />
                        </Rock:AggregateCommandParameters>
                    </Button.CommandParameter>
                </Button>
            </StackLayout>
        </Frame>
    </Rock:ResponsiveColumn>
{% endfor %}
</Rock:ResponsiveLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            // Adds the All Authenticated Users -> Edit -> Allow security to the Prayed endpoint.
            // This references the old Guid that is generated in CreateDatabase. 
            RockMigrationHelper.AddRestAction( "81913C24-8C7C-4308-BC2D-12759E5F26EE", "PrayerRequests", "Rock.Rest.Controllers.PrayerRequestsController" );
            RockMigrationHelper.AddSecurityAuthForRestAction( "81913C24-8C7C-4308-BC2D-12759E5F26EE",
                0,
                Rock.Security.Authorization.EDIT,
                true,
                string.Empty,
                Rock.Model.SpecialRole.AllAuthenticatedUsers,
                "9D5DBDC5-12F1-4ED1-B369-440DEF4F0CDF" );
        }

        /// <summary>
        /// DH: Remove Mobile Prayer Card View block.
        /// </summary>
        private void RockMobilePrayerCardViewBlockDown()
        {
            RockMigrationHelper.DeleteSecurityAuth( "9D5DBDC5-12F1-4ED1-B369-440DEF4F0CDF" );

            RockMigrationHelper.DeleteTemplateBlockTemplate( "757935E7-AB6D-47B6-A6C4-1CA5920C922E" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_PRAYER_CARD_VIEW );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_PRAYER_PRAYER_CARD_VIEW );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_ANSWER_TO_PRAYER_BLOCK_TYPE );
        }

        /// <summary>
        /// DH: Update defined values for new Structured Content editor.
        /// </summary>
        private void UpdateStructuredContentDefinedValuesUp()
        {
            RockMigrationHelper.UpdateDefinedValue( "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Default",
                @"{
    header: {
        class: Rock.UI.StructuredContent.EditorTools.Header,
        inlineToolbar: ['link'],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContent.EditorTools.RockImage,
        inlineToolbar: ['link'],
    },
    list: {
        class: Rock.UI.StructuredContent.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContent.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContent.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    warning: Rock.UI.StructuredContent.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContent.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    code: {
        class: Rock.UI.StructuredContent.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    delimiter: Rock.UI.StructuredContent.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContent.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContent.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContent.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                "09B25845-B879-4E69-87E9-003F9380B8DD",
                false );

            RockMigrationHelper.UpdateDefinedValue( "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Message Notes",
                @"{
    header: {
        class: Rock.UI.StructuredContent.EditorTools.Header,
        inlineToolbar: ['link'],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContent.EditorTools.RockImage,
        inlineToolbar: ['link'],
    },
    list: {
        class: Rock.UI.StructuredContent.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContent.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContent.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    warning: Rock.UI.StructuredContent.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContent.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    fillin: {
        class: Rock.UI.StructuredContent.EditorTools.Fillin,
        shortcut: 'CMD+SHIFT+F'
    },
    code: {
        class: Rock.UI.StructuredContent.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    note: {
        class: Rock.UI.StructuredContent.EditorTools.Note,
        shortcut: 'CMD+SHIFT+N'
    },
    delimiter: Rock.UI.StructuredContent.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContent.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContent.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContent.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                "31C63FB9-1365-4EEF-851D-8AB9A188A06C",
                false );
        }

        /// <summary>
        /// MP: Delete obsolete Person Giving Overview Attributes
        /// </summary>
        private void DeletePersonGivingAttributes()
        {
            Sql( @"
                DELETE
                FROM [Attribute]
                WHERE [Guid] IN (
                    '3BF34F25-4D50-4417-B436-37FEA3FA5473', -- PERSON_GIVING_HISTORY_JSON
                    'ADD9BE86-49CA-46C4-B4EA-547F2F277294', -- PERSON_GIVING_12_MONTHS
                    '0DE95B77-D26E-4513-9A71-92A7FD5C4B7C', -- PERSON_GIVING_90_DAYS
                    '0170A267-942A-480A-A9CF-E4EA60CAA529', -- PERSON_GIVING_PRIOR_90_DAYS
                    '23B6A7BD-BBBB-4F2D-9695-2B1E03B3013A', -- PERSON_GIVING_12_MONTHS_COUNT
                    '356B8F0B-AA54-4F44-8513-F8A5FF592F18') -- PERSON_GIVING_90_DAYS_COUNT" );
        }

        /// <summary>
        /// GJ: Additional Page Route Updates
        /// </summary>
        private void UpdatePageRoutes()
        {
            Sql( @"
                UPDATE [PageRoute] SET [Route]=N'admin/cms/content-channels/{ContentChannelId}' WHERE ([Guid]='0265B86C-474F-9DE3-4BCC-CA8D57580D80')

                UPDATE [PageRoute] SET [Route]=N'admin/cms/content-channel-type/{TypeId}' WHERE ([Guid]='2DB339B2-7C40-984D-2098-8373410C71C2')

                UPDATE [PageRoute] SET [Route]=N'admin/cms/routes/{PageRouteId}' WHERE ([Guid]='990B3E95-95B2-81BC-675F-65346A5E4D21')

                UPDATE [PageRoute] SET [Route]=N'admin/general/campuses/{CampusId}' WHERE ([Guid]='DF183C66-A0A0-9659-8124-BB9BE7FB1EB7')

                UPDATE [PageRoute] SET [Route]=N'admin/general/defined-types/{DefinedTypeId}' WHERE ([Guid]='EA7212EE-700E-0DA9-81EA-A794773414B6')

                UPDATE [PageRoute] SET [Route]=N'admin/general/locations/{LocationId}' WHERE ([Guid]='121BB53D-83D8-47B9-9B91-7858A5E65902')

                UPDATE [PageRoute] SET [Route]=N'admin/security/rest/{Controller}' WHERE ([Guid]='84CF5E9B-3A0D-6915-0D21-D6C9D6D19740')

                UPDATE [PageRoute] SET [Route]=N'admin/security/rest-keys/{RestUserId}' WHERE ([Guid]='9809E592-6322-0940-0A4C-0A94994D5389')

                UPDATE [PageRoute] SET [Route]=N'admin/system/following-events/{EventId}' WHERE ([Guid]='A8A6F3F9-1A98-0A88-6D70-2B2C557030AE')

                UPDATE [PageRoute] SET [Route]=N'admin/system/following-suggestions/{EventId}' WHERE ([Guid]='92477AAF-7392-04E0-A5EA-A305769329BE')

                UPDATE [PageRoute] SET [Route]=N'finance/batches/{BatchId}' WHERE ([Guid]='0553B322-1096-41D4-615F-C5355C89073D')

                UPDATE [PageRoute] SET [Route]=N'finance/pledges/{PledgeId}' WHERE ([Guid]='BEF1BD41-1885-7AEB-9DAA-E3A816E64F88')

                UPDATE [PageRoute] SET [Route]=N'admin/cms/block-types/{BlockTypeId}' WHERE ([Guid]='15F9757C-0546-9B40-491D-B4BDD7C02FFA')

                -- Page: Content Channel Item Attribute Categories - Prevent Duplicate Breadcrumbs
                UPDATE [Page] SET [BreadCrumbDisplayName]='0' WHERE ([Guid]='BBDE39C3-01C9-4C9E-9506-C2205508BC77')

                -- Page: Content Channel Categories - Prevent Duplicate Breadcrumbs
                UPDATE [Page] SET [BreadCrumbDisplayName]='0' WHERE ([Guid]='0F1B45B8-032D-4306-834D-670FA3933589')


                DECLARE @PageId int
                -- Page: Prayer Categories
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FA2A1171-9308-41C7-948C-C9EBEA5BD668')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/prayer-categories/{CategoryId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'CDB0C0F7-2DE2-4FB1-9C4B-A32299B4E6B6')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'admin/general/prayer-categories/{CategoryId}', 'CDB0C0F7-2DE2-4FB1-9C4B-A32299B4E6B6' )
                END

                -- Page: Merge Templates
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '679AF013-0093-435E-AA49-E73B99EB9710')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/merge-templates/{MergeTemplateId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'F5E7A361-8FD5-45D2-80BB-521286D9006B')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'admin/general/merge-templates/{MergeTemplateId}', 'F5E7A361-8FD5-45D2-80BB-521286D9006B' )
                END

                -- Page: Tag Categories
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/tag-categories/{CategoryId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = '9776E0D2-5B7C-498C-90DE-1448CBAC1825')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'admin/general/tag-categories/{CategoryId}', '9776E0D2-5B7C-498C-90DE-1448CBAC1825' )
                END

                -- Page: System Communication Categories
                -- SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B55323CD-F494-43E7-97BF-4E13DAB58E0B')
                -- IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/system-communication-categories/{CategoryId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = '2F4314EF-72CE-43C8-BD48-E11A81185705')
                -- BEGIN
                --     INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                --     VALUES( 1, @PageId, 'admin/communications/system-communication-categories/{CategoryId}', '2F4314EF-72CE-43C8-BD48-E11A81185705' )
                -- END

                -- Page: Communication List Categories
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '307570FD-9472-48D5-A67F-80B2056C5308')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/communication-list-categories/{CategoryId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'CBF326E8-C62A-4A3C-8D99-32DE6956895D')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'admin/communications/communication-list-categories/{CategoryId}', 'CBF326E8-C62A-4A3C-8D99-32DE6956895D' )
                END

                -- Page: Category Manager
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '95ACFF8C-B9EE-41C6-BAC0-D117D6E1FADC')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/category-manager/{CategoryId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = '94FBA2D1-6D64-44B0-8CAB-840921D1227F')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'admin/system/category-manager/{CategoryId}', '94FBA2D1-6D64-44B0-8CAB-840921D1227F' )
                END

                -- Page: Packages By Category
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '50D17FE7-88DB-46B2-9C58-DF8C0DE376A4')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'RockShop/category/{CategoryId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = '0D3E2C2C-F420-4682-AAF9-C650A9207E46')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'RockShop/category/{CategoryId}', '0D3E2C2C-F420-4682-AAF9-C650A9207E46' )
                END

                -- Page: Data Views
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4011CB37-28AA-46C4-99D5-826F4A9CADF5')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/dataviews/{DataViewId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'C5F5A1F2-82F2-4108-B048-77D9811D8841')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'reporting/dataviews/{DataViewId}', 'C5F5A1F2-82F2-4108-B048-77D9811D8841' )
                END

                -- Page: Reports
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/reports/{ReportId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = '29D5BA3A-25E8-4F4F-901B-7368BA9AECE9')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'reporting/reports/{ReportId}', '29D5BA3A-25E8-4F4F-901B-7368BA9AECE9' )
                END

                -- Page: Giving Alerts
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '57650485-7727-4392-9C42-36DE50FBEEEA')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/giving-alerts') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = '03CCA46D-B8EB-49EB-8645-EEE9461FC799')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'finance/giving-alerts', '03CCA46D-B8EB-49EB-8645-EEE9461FC799' )
                END

                -- Page: Giving Automation Configuration
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '490F8A53-85C5-42D1-B305-A531F4924DC6')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/giving-alerts/configuration') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = '73DE063D-36CD-495E-A94E-23255377F72C')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'finance/giving-alerts/configuration', '73DE063D-36CD-495E-A94E-23255377F72C' )
                END

                -- Page: Metric Value Detail
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '64E16878-D5AE-40A5-94FE-C2E8BE62DF61')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/metrics/{MetricCategoryId}/value/{MetricValueId}') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = '6FB7445B-F487-4577-801B-A496D6787860')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'reporting/metrics/{MetricCategoryId}/value/{MetricValueId}', '6FB7445B-F487-4577-801B-A496D6787860' )
                END

                -- Page: Achievements - Streak Type Achievements
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FCE0D006-F854-4107-9298-667563FA8D77')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/streaks/type/{StreakTypeId}/achievements') AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'DB96B225-1794-48FC-927C-7805642A86BF')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'people/streaks/type/{StreakTypeId}/achievements', 'DB96B225-1794-48FC-927C-7805642A86BF' )
                END" );
        }
    }
}

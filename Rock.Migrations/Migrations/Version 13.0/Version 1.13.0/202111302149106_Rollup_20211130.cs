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
    public partial class Rollup_20211130 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateStructuredContentDefinedValuesUp();
            RockMobileSearchBlockUp();
            AddJobToAddIntegrationComponentIdIndexUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMobileSearchBlockDown();
            RemoveJobToAddIntegrationComponentIdIndexDown();
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
            captionPlaceholder: 'Quote\'s author',
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
            captionPlaceholder: 'Quote\'s author',
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
        /// DH: Mobile Search Block
        /// </summary>
        private void RockMobileSearchBlockUp()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            // Mobile > Core > Search.
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Core.Search",
                "Search",
                "Rock.Blocks.Types.Mobile.Core.Search, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_CORE_SEARCH_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "Search",
                "Performs a search using one of the configured search components and displays the results.",
                "Rock.Blocks.Types.Mobile.Core.Search",
                "Mobile > Core",
                Rock.SystemGuid.BlockType.MOBILE_CORE_SEARCH );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Core > Search",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CORE_SEARCH );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "50FABA2A-B23C-46CD-A634-2F54BC1AE8C3",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CORE_SEARCH,
                "Default",
                @"{% assign photoUrl = null %}
{% assign itemName = null %}
{% assign itemText = null %}

{% if ItemType == 'Person' %}
    {% capture photoUrl %}{% if Item.PhotoId != null %}{{ Item.PhotoUrl | Append:'&width=200' | Escape }}{% else %}{{ Item.PhotoUrl | Escape }}{% endif %}{% endcapture %}
    {% assign itemName = Item.FullName %}
    {% assign itemText = Item.Email %}
{% else %}
    {% assign itemName = Item | AsString %}
{% endif %}

<StackLayout Spacing=""0"">
    <StackLayout Orientation=""Horizontal"" StyleClass=""search-result-content"">
        {% if photoUrl != null %}
            <Rock:Image StyleClass=""search-result-image""
                Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ photoUrl }}"">
                <Rock:CircleTransformation />
            </Rock:Image>
        {% endif %}
        
        <StackLayout Spacing=""0""
            HorizontalOptions=""FillAndExpand""
            VerticalOptions=""Center"">
            <Label StyleClass=""search-result-name""
                Text=""{{ itemName | Escape }}""
                HorizontalOptions=""FillAndExpand"" />
    
            {% if itemText != null and itemText != '' %}
                <Label StyleClass=""search-result-text"">{{ itemText | XamlWrap }}</Label>
            {% endif %}
        </StackLayout>
        
        <Rock:Icon IconClass=""chevron-right""
            VerticalOptions=""Center""
            StyleClass=""search-result-detail-arrow"" />
    </StackLayout>

    <Rock:Divider />
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );
        }

        /// <summary>
        /// DH: Mobile Search Block
        /// </summary>
        private void RockMobileSearchBlockDown()
        {
            // Mobile > Core > Search
            RockMigrationHelper.DeleteTemplateBlockTemplate( "50FABA2A-B23C-46CD-A634-2F54BC1AE8C3" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CORE_SEARCH );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_CORE_SEARCH );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_CORE_SEARCH_BLOCK_TYPE );
        }

        // Goes with commit: https://github.com/SparkDevNetwork/Rock/commit/fe119246dd6429e7ce146a89852646c1fda62923
        private void AddJobToAddIntegrationComponentIdIndexUp()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV13DataMigrationsAddInteractionComponentIndexToInteraction'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_130_ADD_INTERACTION_INTERACTION_COMPONENT_ID_INDEX}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Class]
                    ,[CronExpression]
                    ,[NotificationStatus]
                    ,[Guid]
                ) VALUES (
                    1
                    ,1
                    ,'Rock Update Helper v13.0 - Add index for Interaction InteractionComponentId.'
                    ,'This job will add an index for the Interaction InteractionComponentId column.'
                    ,'Rock.Jobs.PostV13DataMigrationsAddInteractionComponentIndexToInteraction'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_130_ADD_INTERACTION_INTERACTION_COMPONENT_ID_INDEX}'
                );
            END" );
        }

        private void RemoveJobToAddIntegrationComponentIdIndexDown()
        {
            Sql( $@"
                DELETE [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV13DataMigrationsAddInteractionComponentIndexToInteraction'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_130_ADD_INTERACTION_INTERACTION_COMPONENT_ID_INDEX}'
                " );
        }
    }
}

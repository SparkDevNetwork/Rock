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
    public partial class ContentComponents : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._201809142151587_ContentComponents_CreateContentComponentChannelType );

            RockMigrationHelper.AddDefinedType( "Content Channel", "Content Component Template", "Lava Template that can be used with a Content Component block.", Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE );
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, Rock.SystemGuid.FieldType.LAVA, "Display Lava", "DisplayLava", "The Lava Template to use when rendering the Content Component", 0, "", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE" );
            RockMigrationHelper.AddAttributeQualifier( "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", "editorHeight", "400", "573D06B9-5BDE-4E64-B0B1-85E167FCB47A" );
            RockMigrationHelper.AddAttributeQualifier( "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", "editorTheme", "0", "94E49DA2-F796-4F83-A857-A9751F273CF6" );
            RockMigrationHelper.AddAttributeQualifier( "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", "editorMode", "3", "17B8ECCA-2F67-4130-A43D-4D76C71D11D0" );

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, "Hero", "", "3E7D4D0C-8238-4A5F-9E5F-34E4DFBF7725" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3E7D4D0C-8238-4A5F-9E5F-34E4DFBF7725", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", MigrationSQL._201809142151587_ContentComponents_Hero );

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, "Ad Unit", "", "902D960C-0B7B-425E-9CEA-94CF215AABE4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "902D960C-0B7B-425E-9CEA-94CF215AABE4", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", MigrationSQL._201809142151587_ContentComponents_AdUnit );

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, "Side By Side", "", "EC429625-767E-4F69-BB48-F55DA3C836A3" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EC429625-767E-4F69-BB48-F55DA3C836A3", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", MigrationSQL._201809142151587_ContentComponents_SideBySide );

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE, "Card", "", "54A6FE8C-B38F-46DB-81F7-A7648886B592" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "54A6FE8C-B38F-46DB-81F7-A7648886B592", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", MigrationSQL._201809142151587_ContentComponents_Card );

            RockMigrationHelper.UpdateBlockType( "Content Component", "Block to manage and display content.", "~/Blocks/Cms/ContentComponent.ascx", "CMS", Rock.SystemGuid.BlockType.CONTENT_COMPONENT );
            // Attrib for BlockType: Content Component:Item Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONTENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Item Cache Duration", "ItemCacheDuration", "", @"Number of seconds to cache the content item specified by the parameter.", 0, @"0", "89B2C635-7F93-469B-BA8A-8F52609238DF" );
            // Attrib for BlockType: Content Component:Content Channel
            RockMigrationHelper.UpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONTENT_COMPONENT, "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "", @"", 0, @"", "8F7BBA6A-88B8-4568-BF7F-5043AAB23BDC" );
            // Attrib for BlockType: Content Component:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONTENT_COMPONENT, "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 0, @"", "DD660761-0894-4297-81EE-535135EF12E7" );
            // Attrib for BlockType: Content Component:Filter Id
            RockMigrationHelper.UpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONTENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "", @"The data filter that is used to filter items", 0, @"0", "0F7971F3-DB75-4E4E-9193-D87A02CB5691" );
            // Attrib for BlockType: Content Component:Content Component Template
            RockMigrationHelper.UpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONTENT_COMPONENT, "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Content Component Template", "ContentComponentTemplate", "", @"", 0, @"", "F33C44C4-615E-4254-9A97-9DD1D9922C32" );
            // Attrib for BlockType: Content Component:Allow Multiple Content Items
            RockMigrationHelper.UpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONTENT_COMPONENT, "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Multiple Content Items", "AllowMultipleContentItems", "", @"", 0, @"False", "F11E0BF3-13BD-4A00-86F6-1BA74790EF61" );
            // Attrib for BlockType: Content Component:Output Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONTENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Output Cache Duration", "OutputCacheDuration", "", @"Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", 0, @"", "1344878E-D977-489F-BE0D-5AA761521145" );

            Sql( $"UPDATE [BlockType] SET [IsCommon] = 1 WHERE [Guid] in ('{Rock.SystemGuid.BlockType.CONTENT_COMPONENT}','{Rock.SystemGuid.BlockType.CONTENT_CHANNEL_ITEM_VIEW}')" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Content Component:Output Cache Duration
            RockMigrationHelper.DeleteAttribute( "1344878E-D977-489F-BE0D-5AA761521145" );
            // Attrib for BlockType: Content Component:Allow Multiple Content Items
            RockMigrationHelper.DeleteAttribute( "F11E0BF3-13BD-4A00-86F6-1BA74790EF61" );
            // Attrib for BlockType: Content Component:Content Component Template
            RockMigrationHelper.DeleteAttribute( "F33C44C4-615E-4254-9A97-9DD1D9922C32" );
            // Attrib for BlockType: Content Component:Filter Id
            RockMigrationHelper.DeleteAttribute( "0F7971F3-DB75-4E4E-9193-D87A02CB5691" );
            // Attrib for BlockType: Content Component:Cache Tags
            RockMigrationHelper.DeleteAttribute( "DD660761-0894-4297-81EE-535135EF12E7" );
            // Attrib for BlockType: Content Component:Content Channel
            RockMigrationHelper.DeleteAttribute( "8F7BBA6A-88B8-4568-BF7F-5043AAB23BDC" );
            // Attrib for BlockType: Content Component:Item Cache Duration
            RockMigrationHelper.DeleteAttribute( "89B2C635-7F93-469B-BA8A-8F52609238DF" );
            RockMigrationHelper.DeleteBlockType( Rock.SystemGuid.BlockType.CONTENT_COMPONENT ); // Content Component
        }
    }
}

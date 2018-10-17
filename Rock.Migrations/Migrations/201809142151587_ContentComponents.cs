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
            /* Create Content Channel Type of Content Component*/
            Sql( MigrationSQL._201809142151587_ContentComponents_CreateContentComponentChannelType );

            /* Add Content Component Template defined type */
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

            /* Add Content Component Templates page to CMS Config page*/
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Component Templates", "", "F1ED10C2-A17D-4310-9F86-76E11A4A7ED2", "fa fa-list-alt" ); // Site:Rock RMS
            // Add Block to Page: Content Component Templates, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F1ED10C2-A17D-4310-9F86-76E11A4A7ED2", "", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Content Component Template List", "Main", @"", @"", 0, "9DFBA51C-FDBE-452E-9763-38CA182555F2" );
            // Attrib Value for Block:Content Component Template List, Attribute:Defined Type Page: Content Component Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9DFBA51C-FDBE-452E-9763-38CA182555F2", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637", @"313B579F-F442-4247-ADBB-BBD25E255003" );
            // Attrib Value for Block:Content Component Template List, Attribute:core.CustomGridColumnsConfig Page: Content Component Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9DFBA51C-FDBE-452E-9763-38CA182555F2", "87DAF7ED-AAF5-4D5C-8339-CB30B16CC9FF", @"" ); 
            // Attrib Value for Block:Content Component Template List, Attribute:core.CustomGridEnableStickyHeaders Page: Content Component Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9DFBA51C-FDBE-452E-9763-38CA182555F2", "945FE831-D33C-49C6-B21D-00EACA9A43D7", @"False" );

            /* New Content Component Block Type*/
            RockMigrationHelper.UpdateBlockType( "Content Component", "Block to manage and display content.", "~/Blocks/Cms/ContentComponent.ascx", "CMS", "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD" );
            // Attrib for BlockType: Content Component:Item Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Item Cache Duration", "ItemCacheDuration", "", @"Number of seconds to cache the content item specified by the parameter.", 0, @"0", "89B2C635-7F93-469B-BA8A-8F52609238DF" );
            // Attrib for BlockType: Content Component:Content Channel
            RockMigrationHelper.UpdateBlockTypeAttribute( "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "", @"", 0, @"", "8F7BBA6A-88B8-4568-BF7F-5043AAB23BDC" );
            // Attrib for BlockType: Content Component:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 0, @"", "DD660761-0894-4297-81EE-535135EF12E7" );
            // Attrib for BlockType: Content Component:Filter Id
            RockMigrationHelper.UpdateBlockTypeAttribute( "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "", @"The data filter that is used to filter items", 0, @"0", "0F7971F3-DB75-4E4E-9193-D87A02CB5691" );
            // Attrib for BlockType: Content Component:Content Component Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Content Component Template", "ContentComponentTemplate", "", @"", 0, @"", "F33C44C4-615E-4254-9A97-9DD1D9922C32" );
            // Attrib for BlockType: Content Component:Allow Multiple Content Items
            RockMigrationHelper.UpdateBlockTypeAttribute( "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Multiple Content Items", "AllowMultipleContentItems", "", @"", 0, @"False", "F11E0BF3-13BD-4A00-86F6-1BA74790EF61" );
            // Attrib for BlockType: Content Component:Output Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Output Cache Duration", "OutputCacheDuration", "", @"Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", 0, @"", "1344878E-D977-489F-BE0D-5AA761521145" );

            /* Set Content Component and Content Channel Item View as 'Common' block types */
            Sql( $"UPDATE [BlockType] SET [IsCommon] = 1 WHERE [Guid] in ('AD802CA1-842C-47F0-B5E9-739FE2B4A2BD{Rock.SystemGuid.BlockType.CONTENT_COMPONENT}','{Rock.SystemGuid.BlockType.CONTENT_CHANNEL_ITEM_VIEW}')" );

            /* Do a Switch-a-roo to get a couple of CMS blocks renamed to Content Channel Item View and Content Channel Navigation blocks so they make more sense */

            // Change the Name and Path of old "Content Channel Item View" to "Content Channel Navigation" (This is a better name for it and it also lets us name "Content Channel View Detail" get renamed "Content Channel Item View")
            RockMigrationHelper.UpdateBlockTypeByGuid( "Content Channel Navigation", "Block to display a menu of content channels/items that user is authorized to view.", "~/Blocks/Cms/ContentChannelNavigation.ascx", "CMS", Rock.SystemGuid.BlockType.CONTENT_CHANNEL_NAVIGATION );

            // Change the Name and Path of old "Content Channel View Detail" to "Content Channel Item View" (This is a better name for it now that we got the old one renamed to make the name available) )
            RockMigrationHelper.UpdateBlockTypeByGuid( "Content Channel Item View", "Block to display a specific content channel item.", "~/Blocks/Cms/ContentChannelItemView.ascx", "CMS", Rock.SystemGuid.BlockType.CONTENT_CHANNEL_ITEM_VIEW );

            // Update the name of the Content CHannel Item View blocks on Series Detail, Site: External Website and  Message Detail, Site: External Website
            Sql( @"UPDATE [Block]
SET [Name] = 'Content Channel Item View'
WHERE [Name] = 'Content Channel View Detail'
	AND [Guid] IN (
		'847E12E0-A7FC-4BD5-BD7E-1E9D435510E7'
		,'71D998C7-9F27-4B8A-937A-64C5EFC4783A'
		)" );
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
            // Attrib for BlockType: Defined Value List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.DeleteAttribute( "945FE831-D33C-49C6-B21D-00EACA9A43D7" );
            // Remove Block: Content Component Template List, from Page: Content Component Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9DFBA51C-FDBE-452E-9763-38CA182555F2" );
            RockMigrationHelper.DeleteBlockType( "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD" ); // Content Component
            RockMigrationHelper.DeletePage( "F1ED10C2-A17D-4310-9F86-76E11A4A7ED2" ); //  Page: Content Component Templates, Layout: Full Width, Site: Rock RMS
        }
    }
}

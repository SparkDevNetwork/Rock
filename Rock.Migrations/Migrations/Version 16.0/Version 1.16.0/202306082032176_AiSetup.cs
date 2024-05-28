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
    public partial class AiSetup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Shortcode Category
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.LAVA_SHORTCODE, "AI", "fa fa-brain", "Category for grouping AI Lava Shortcodes.", SystemGuid.Category.LAVA_SHORTCODE_AI );

            // Add Page 
            //  Internal Name: AI Providers
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "AI Providers", "", "54E421B1-B89C-4C3B-BECA-16349D750691", "fa fa-brain" );

#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route
            //   Page:AI Providers
            //   Route:admin/system/ai
            RockMigrationHelper.AddPageRoute( "54E421B1-B89C-4C3B-BECA-16349D750691", "admin/system/ai", "4F95EC18-BD5D-4560-A2EC-7576C536E6A8" );
#pragma warning restore CS0618 // Type or member is obsolete
                        
            // Add Block 
            //  Block Name: AI
            //  Page Name: AI Providers
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "54E421B1-B89C-4C3B-BECA-16349D750691".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "21F5F466-59BC-40B2-8D73-7314D936C3CB".AsGuid(), "AI", "Main", @"", @"", 0, "F62F88D5-9E56-4DA0-8990-D2B6BBA0D28E" );

            // Add Block Attribute Value
            //   Block: AI
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Providers, Site=Rock RMS
            //   Attribute: Component Container
            /*   Attribute Value: Rock.AI.Provider.AIProviderContainer, Rock */
            RockMigrationHelper.AddBlockAttributeValue( "F62F88D5-9E56-4DA0-8990-D2B6BBA0D28E", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.AI.Provider.AIProviderContainer, Rock" );

            // Add Block Attribute Value
            //   Block: AI
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Providers, Site=Rock RMS
            //   Attribute: Support Ordering
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F62F88D5-9E56-4DA0-8990-D2B6BBA0D28E", "A4889D7B-87AA-419D-846C-3E618E79D875", @"False" );

            // Add Block Attribute Value
            //   Block: AI
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Providers, Site=Rock RMS
            //   Attribute: Support Security
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F62F88D5-9E56-4DA0-8990-D2B6BBA0D28E", "A8F1D1B8-0709-497C-9DCB-44826F26AE7A", @"False" );

            // Add Block Attribute Value
            //   Block: AI
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Providers, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "F62F88D5-9E56-4DA0-8990-D2B6BBA0D28E", "C29E9E43-B246-4CBB-9A8A-274C8C377FDF", @"True" );

            // Add Block Attribute Value
            //   Block: AI
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Providers, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F62F88D5-9E56-4DA0-8990-D2B6BBA0D28E", "762095AA-C455-43A4-9134-10366C60B4C4", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete shortcode category
            RockMigrationHelper.DeleteCategory( SystemGuid.Category.LAVA_SHORTCODE_AI );

            // Remove Block
            //  Name: AI, from Page: AI Providers, Site: Rock RMS
            //  from Page: AI Providers, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F62F88D5-9E56-4DA0-8990-D2B6BBA0D28E" );

            // Delete Page 
            //  Internal Name: AI Providers
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "54E421B1-B89C-4C3B-BECA-16349D750691" );

        }
    }
}

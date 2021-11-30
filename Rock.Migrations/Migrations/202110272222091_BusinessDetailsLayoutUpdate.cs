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
    public partial class BusinessDetailsLayoutUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
           // Add/Update BlockType 
           //   Name: Business Contact List
           //   Category: Finance
           //   Path: ~/Blocks/Finance/BusinessContactList.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Business Contact List","Displays the list of contacts for a business.","~/Blocks/Finance/BusinessContactList.ascx","Finance","E8F41C21-7D0F-41AC-B5D7-2BA3FA016CB4");

            // Add Block 
            //  Block Name: Giving Configuration
            //  Page Name: Business Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D2B43273-C64F-4F57-9AAE-9571E1982BAC".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"486E470A-DBD8-48D6-9A97-5B1B490A401E".AsGuid(), "Giving Configuration","Main",@"<div class='col-md-4'>",@"    </div>
</div>",3,"809C6885-9CC5-46DB-8F0E-EF862726501A"); 

            // Add Block 
            //  Block Name: Business Contact List
            //  Page Name: Business Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D2B43273-C64F-4F57-9AAE-9571E1982BAC".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"E8F41C21-7D0F-41AC-B5D7-2BA3FA016CB4".AsGuid(), "Business Contact List","Main",@"<div class='row'>
<div class='col-md-8'>",@"",1,"44CCEB5E-0684-43C8-8BB0-4D609D4CA496"); 

            // update block order for pages with new blocks if the page,zone has multiple blocks

            
            // Update Order for Page: Business Detail,  Zone: Main,  Block: Business Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '77AB2D30-FCBE-45E9-9757-401AE2676A7F'"  );

            // Update Order for Page: Business Detail,  Zone: Main,  Block: Transaction Yearly Summary Lava
            Sql( @"UPDATE [Block] SET [Order] = 2, [PreHtml] = '', [PostHtml] = '</div>' WHERE [Guid] = '5322C1C2-0387-4752-9E87-67700F485C5E'"  );

            // Update Order for Page: Business Detail,  Zone: Main,  Block: Transaction List
            Sql( @"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = '0A567E24-80BE-4906-B303-77D1A5FB89DE'"  );

            
            RockMigrationHelper.DeleteBlock( "4A7394DA-4E92-4E15-B75E-0C79E691A9B2" );
            RockMigrationHelper.DeleteBlock( "13EF2086-37D4-42FD-B629-6D4292495BC8" );

            // Attribute for BlockType
            //   BlockType: Attendance Analytics
            //   Category: Check-in
            //   Attribute: Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 15, @"180", "CEAE8225-E5C6-4369-AE9C-2193BC1C9F8A" );

            // Attribute for BlockType
            //   BlockType: Pledge Analytics
            //   Category: Finance
            //   Attribute: Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72B4BBC0-1E8A-46B7-B956-A399624F513C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 0, @"180", "A949896B-BC88-4734-AD8E-574A71400EB8" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Display Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Tags", "DisplayTags", "Display Tags", @"Should tags be displayed?", 2, @"True", "234AA59D-852C-42D8-8C6B-F8F11A0BED27" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Tag Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Tag Category", "TagCategory", "Tag Category", @"Optional category to limit the tags to. If specified all new personal tags will be added with this category.", 3, @"", "44EC9DAA-96EB-4D8B-B398-C3C9BA2B4B8E" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges", "Badges", "Badges", @"The label badges to display in this block.", 1, @"", "ACCDED55-3A02-4016-AFC9-ADE7A9E74137" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "5C10B655-6416-4BEC-8990-A94097680F91" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Search Key Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Search Key Types", "SearchKeyTypes", "Search Key Types", @"Optional list of search key types to limit the display in search keys grid. No selection will show all.", 4, @"", "C04DEF87-E0AA-4D6B-8010-CF8BE5EF6074" );

            // Attribute for BlockType
            //   BlockType: Business Detail
            //   Category: Finance
            //   Attribute: Business Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Business Attributes", "BusinessAttributes", "Business Attributes", @"The person attributes that should be displayed / edited for adults.", 5, @"", "4B9C1355-B3AC-4B94-91D8-77ABCB7FDDC4" );

            // Attribute for BlockType
            //   BlockType: Business Contact List
            //   Category: Finance
            //   Attribute: Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E8F41C21-7D0F-41AC-B5D7-2BA3FA016CB4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"The page used to view the details of a business contact", 0, @"", "50549FB9-AD68-499C-9A71-18785FA3BE17" );

            // Add the link to the person profile page for the Business Contact List block
            RockMigrationHelper.AddBlockAttributeValue( "44CCEB5E-0684-43C8-8BB0-4D609D4CA496", "50549FB9-AD68-499C-9A71-18785FA3BE17", "08DBD8A5-2C35-4146-B4A8-0F7652348B25" );

            // Add/Update PageContext for Page:Business Detail, Entity: Rock.Model.Person, Parameter: businessId
            RockMigrationHelper.UpdatePageContext( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "Rock.Model.Person", "businessId", "8F1D2A7F-3320-4FFD-9A02-BF335C0BF5B0");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
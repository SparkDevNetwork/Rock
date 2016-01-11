// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class ExtendFamilyBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Active Users:Show Guest Visitors
            RockMigrationHelper.AddBlockTypeAttribute( "3E7033EE-31A3-4484-AFA9-240C856A500C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Guest Visitors", "ShowGuestVisitors", "", "Displays the number of guests visiting the site. (Guests are considered users not logged in.)", 0, @"True", "8DFF36C0-BA91-4CE5-A385-4053B5DA2745" );

            // Attrib for BlockType: Add Family:Group Type
            RockMigrationHelper.AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", "The group type to display groups for (default is Family)", 0, @"790E3215-3B10-442B-AF69-616C0DCB998E", "F349D3AE-9025-4F62-BB59-ED2D5BF525CF" );
            // Attrib for BlockType: Add Family:Parent Group
            RockMigrationHelper.AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Parent Group", "ParentGroup", "", "The parent group to add the new group to (default is none)", 1, @"", "AE3FF4B2-6B79-45B5-92D2-851D51C6A9AA" );
            // Attrib for BlockType: Add Family:Marital Status Confirmation
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Marital Status Confirmation", "MaritalStatusConfirmation", "", "When Family group type, should user be asked to confirm saving an adult without a marital status?", 7, @"True", "5E62C385-C24B-4B02-A49D-803733DF9E05" );
            // Attrib for BlockType: Add Family:SMS
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "SMS", "SMS", "", "Should SMS be enabled for cell phone numbers by default?", 9, @"False", "C48243DC-CD23-4B7F-9857-A64C4DBDBF94" );

            // Attrib for BlockType: Person Bio:Display Middle Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Middle Name", "DisplayMiddleName", "", "Display the middle name of the person.", 0, @"False", "384EA763-B8E5-4A41-997F-ACD1B002AF8D" );

            // Attrib for BlockType: Family Members:Group Type
            RockMigrationHelper.AddBlockTypeAttribute( "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", "The group type to display groups for (default is Family)", 0, @"790E3215-3B10-442B-AF69-616C0DCB998E", "F988BC15-4D12-4D37-9690-770394FDCB24" );
            // Attrib for BlockType: Family Members:Group Edit Page
            RockMigrationHelper.AddBlockTypeAttribute( "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Edit Page", "GroupEditPage", "", "Page used to edit the members of the selected group.", 1, @"", "C12B3192-4B51-4733-AE9F-8D2D46B82DA9" );

            // Attrib Value for Block:Family Members, Attribute:Group Type , Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CC50BE8-72ED-43E0-8D11-7E2A590453CC", "F988BC15-4D12-4D37-9690-770394FDCB24", @"790e3215-3b10-442b-af69-616c0dcb998e" );
            // Attrib Value for Block:Family Members, Attribute:Group Edit Page , Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CC50BE8-72ED-43E0-8D11-7E2A590453CC", "C12B3192-4B51-4733-AE9F-8D2D46B82DA9", @"e9e1e5f2-467d-47cb-af41-b4d9ef8b0b27,224e5ac3-8370-4eed-9812-f08f9fc7efa7" );

            RockMigrationHelper.UpdateFieldType( "Range Slider", "", "Rock", "Rock.Field.Types.RangeSliderFieldType", "C8B6C51A-DD7C-4B75-8604-F0580697088E" );

            // Add/Update PageContext for Page:Edit Family, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "E9E1E5F2-467D-47CB-AF41-B4D9EF8B0B27", "Rock.Model.Person", "PersonId", "187ED8DB-7B91-4AF2-9E43-13F3A657087C" );

            Sql( @"
    UPDATE [PageRoute] SET [Route] = 'EditFamily/{PersonId}/{GroupId}' WHERE [Guid] = '224e5ac3-8370-4eed-9812-f08f9fc7efa7';
");

            RockMigrationHelper.RenameBlockType( "~/Blocks/Crm/PersonDetail/EditFamily.ascx", "~/Blocks/Crm/PersonDetail/EditGroup.ascx", null, "Edit Group", "Allows you to edit a group that person belongs to." );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Crm/PersonDetail/FamilyMembers.ascx", "~/Blocks/Crm/PersonDetail/GroupMembers.ascx", null, "Group Members", "Allows you to view the other members of a group person belongs to (e.g. Family groups)." );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Crm/PersonDetail/AddFamily.ascx", "~/Blocks/Crm/PersonDetail/AddGroup.ascx", null, "Add Group", "Allows for adding a new group and the people in the group (e.g. New Families." );

            RockMigrationHelper.UpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Save Success Text", "SaveSuccessText", "", "Text to display upon successful save. (Only applies if not navigating to parent page on save.) <span class='tip tip-lava'></span><span class='tip tip-html'></span>", 12, @"<p>Thank you for allowing us to pray for you.</p>", "07C47FA5-4A1A-4B70-AA1B-BD042120233C" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "", "An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 13, @"", "4B17B518-D876-4838-8A72-5912DC275B5A" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Outputs the object graph to help create your lava syntax.", 14, @"False", "519A4ECC-153E-4B31-A69C-7BC0776F4B27" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "4B17B518-D876-4838-8A72-5912DC275B5A" );
            RockMigrationHelper.DeleteAttribute( "519A4ECC-153E-4B31-A69C-7BC0776F4B27" );

            // Attrib for BlockType: Family Members:Group Edit Page
            RockMigrationHelper.DeleteAttribute( "C12B3192-4B51-4733-AE9F-8D2D46B82DA9" );
            // Attrib for BlockType: Family Members:Group Type
            RockMigrationHelper.DeleteAttribute( "F988BC15-4D12-4D37-9690-770394FDCB24" );
            // Attrib for BlockType: Add Family:Parent Group
            RockMigrationHelper.DeleteAttribute( "AE3FF4B2-6B79-45B5-92D2-851D51C6A9AA" );
            // Attrib for BlockType: Add Family:Group Type
            RockMigrationHelper.DeleteAttribute( "F349D3AE-9025-4F62-BB59-ED2D5BF525CF" );
            // Attrib for BlockType: Active Users:Show Guest Visitors
            RockMigrationHelper.DeleteAttribute( "8DFF36C0-BA91-4CE5-A385-4053B5DA2745" );

            RockMigrationHelper.RenameBlockType( "~/Blocks/Crm/PersonDetail/EditGroup.ascx", "~/Blocks/Crm/PersonDetail/EditFamily.ascx", null, "Edit Group", "Allows you to edit a group that person belongs to." );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Crm/PersonDetail/GroupMembers.ascx", "~/Blocks/Crm/PersonDetail/FamilyMembers.ascx", null, "Group Members", "Allows you to view the other members of a group person belongs to (e.g. Family groups)." );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Crm/PersonDetail/AddGroup.ascx", "~/Blocks/Crm/PersonDetail/AddFamily.ascx", null, "Add Group", "Allows for adding a new group and the people in the group (e.g. New Families." );
        }
    }
}

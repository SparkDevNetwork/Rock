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
    public partial class CodeGenerated_20220705 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4648796C-BC9F-43D9-B096-BE835932539D".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"FDFA4897-F563-4A8E-9FC7-3D4BFC22D312"); 

            // Attribute for BlockType
            //   BlockType: Personalization Segment Detail
            //   Category: Cms
            //   Attribute: Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F0A0A57-952D-4774-8760-52C6D56B9DB5", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeout", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 0, @"180", "18C5F26B-E9B6-4811-B42F-C417D74AB67B" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Profile Photos
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Profile Photos", "AdultProfilePhoto", "Profile Photos", @"How should Profile Photo be displayed for adults?", 9, @"Hide", "4F2D296A-4166-4AE0-846E-75E2B01B013E" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: First Adult Create Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "First Adult Create Account", "FirstAdultCreateAccount", "First Adult Create Account", @"Allows the first adult to create an account for themselves.", 10, @"Hide", "50F54C40-00EE-4F3B-8544-79878130983C" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Create Account Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Create Account Title", "CreateAccountTitle", "Create Account Title", @"Configures the description for the create account card.", 11, @"Create Account", "26B7B226-1492-4111-A181-EFA6B08E4DDD" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Create Account Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Create Account Description", "CreateAccountDescription", "Create Account Description", @"Allows the first adult to create an account for themselves.", 12, @"Create an account to personalize your experience and access additional capabilities on our site.", "1A372514-9540-4840-983B-BBB3677C07AF" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Profile Photos
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Profile Photos", "ChildProfilePhoto", "Profile Photos", @"How should Profile Photo be displayed for children?", 8, @"Hide", "C368861C-17AD-4FF2-888F-C3DD3153C026" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6FBE0419-5404-4866-85A1-135542D33725", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "19FB59F4-64D3-4B91-A3ED-00447C7482F7" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("FDFA4897-F563-4A8E-9FC7-3D4BFC22D312","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
            
            RockMigrationHelper.UpdateFieldType("Group Member Requirement","","Rock","Rock.Field.Types.GroupMemberRequirementFieldType","C0797A18-B489-46C7-8C30-F5E4F8246E23");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("19FB59F4-64D3-4B91-A3ED-00447C7482F7");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Profile Photos
            RockMigrationHelper.DeleteAttribute("C368861C-17AD-4FF2-888F-C3DD3153C026");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Create Account Description
            RockMigrationHelper.DeleteAttribute("1A372514-9540-4840-983B-BBB3677C07AF");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Create Account Title
            RockMigrationHelper.DeleteAttribute("26B7B226-1492-4111-A181-EFA6B08E4DDD");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: First Adult Create Account
            RockMigrationHelper.DeleteAttribute("50F54C40-00EE-4F3B-8544-79878130983C");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Profile Photos
            RockMigrationHelper.DeleteAttribute("4F2D296A-4166-4AE0-846E-75E2B01B013E");

            // Attribute for BlockType
            //   BlockType: Personalization Segment Detail
            //   Category: Cms
            //   Attribute: Database Timeout
            RockMigrationHelper.DeleteAttribute("18C5F26B-E9B6-4811-B42F-C417D74AB67B");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("FDFA4897-F563-4A8E-9FC7-3D4BFC22D312");
        }
    }
}

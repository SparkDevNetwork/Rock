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
    public partial class NewPersonPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create a copy of the current Person Pages
            CreateV1Pages();

            // Group Members Block V1
            RockMigrationHelper.UpdateBlockTypeByGuid( "Group Members (V1)", "Allows you to view the other members of a group person belongs to (e.g. Family groups).", "~/Blocks/Crm/PersonDetail/GroupMembersV1.ascx", "CRM > Person Detail", "FC137BDA-4F05-4ECE-9899-A249C90D11FC" );

            // Group Members Block V2
            RockMigrationHelper.AddBlockType( "Group Members", "Allows you to view the other members of a group person belongs to (e.g. Family groups).", "~/Blocks/Crm/PersonDetail/GroupMembers.ascx", "CRM > Person Detail", "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F" );

            // Bio Block V1
            RockMigrationHelper.UpdateBlockTypeByGuid( "Person Bio (V1)", "Person biographic/demographic information and picture (Person detail page).", "~/Blocks/Crm/PersonDetail/BioV1.ascx", "CRM > Person Detail", "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C" );

            // Bio Block V2
            RockMigrationHelper.AddBlockType( "Person Bio", "Person biographic/demographic information and picture (Person detail page).", "~/Blocks/Crm/PersonDetail/Bio.ascx", "CRM > Person Detail", "030CCDDC-8D43-40F8-A298-78B416F9E828" );

            // Icon CSS Class attribute and AttributeValues for DefinedType "LocationType"
            RockMigrationHelper.AddDefinedTypeAttribute("2E68D37C-FB7B-4AA5-9E09-3785D52156CB","9C204CD0-1233-41C5-818A-C5DA439445AA","Icon CSS Class","IconCSSClass","",2014,"","348B1563-BBC9-4853-BD4E-4CEAE87CA2B6");
            RockMigrationHelper.AddDefinedValueAttributeValue("8C52E53C-2A66-435A-AE6E-5EE307D9A0DC","348B1563-BBC9-4853-BD4E-4CEAE87CA2B6",@"fa fa-home");
            RockMigrationHelper.AddDefinedValueAttributeValue("E071472A-F805-4FC4-917A-D5E3C095C35C","348B1563-BBC9-4853-BD4E-4CEAE87CA2B6",@"fa fa-building");
            RockMigrationHelper.AddDefinedValueAttributeValue("853D98F1-6E08-4321-861B-520B4106CFE0","348B1563-BBC9-4853-BD4E-4CEAE87CA2B6",@"fa fa-map-marker");

            // Update Icon for Snapchat
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "iconcssclass", "fa fa-fw fa-snapchat", "E9168011-2719-40EB-A082-9337B5F52233" );

            // Add/Update BlockType 
            //   Name: Family Navigation
            //   Category: CRM > Person Detail
            //   Path: ~/Blocks/Crm/PersonDetail/FamilyNav.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Family Navigation","Allows you to switch between the members of the family the person belongs to.","~/Blocks/Crm/PersonDetail/FamilyNav.ascx","CRM > Person Detail","35D091FA-8311-42D1-83F7-3E67B9EE9675");

            // Add/Update BlockType 
            //   Name: Person Edit
            //   Category: CRM > Person Edit
            //   Path: ~/Blocks/Crm/PersonDetail/PersonEditControl.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Person Edit","Allows you to navigate to the person edit page.","~/Blocks/Crm/PersonDetail/PersonEditControl.ascx","CRM > Person Edit","8C94620B-0FC1-4C39-9474-1714546E7D9E");

           // Add/Update BlockType 
           //   Name: Person Bio Summary
           //   Category: CRM > Person Detail
           //   Path: ~/Blocks/Crm/PersonDetail/BioSummary.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Person Bio Summary","Person name, picture, and badges.","~/Blocks/Crm/PersonDetail/BioSummary.ascx","CRM > Person Detail","7249D05F-0FD1-4F44-88EB-AD46DEB1DAEA");

            PersonsBioV2BlockTypeAttributesUp();
            AddPersonProfileHomeLayout();
            AddPersonProfileDetailLayout();

            UpdatePersonPageLayouts();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove V2 PersonProfile BlockTypes
            RockMigrationHelper.DeleteBlockType( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F" );
            RockMigrationHelper.DeleteBlockType( "030CCDDC-8D43-40F8-A298-78B416F9E828" );

            // Rename V1 PersonProfile BlockTypes
            RockMigrationHelper.UpdateBlockTypeByGuid( "Group Members", "Allows you to view the other members of a group person belongs to (e.g. Family groups).", "~/Blocks/Crm/PersonDetail/GroupMembers.ascx", "CRM > Person Detail", "FC137BDA-4F05-4ECE-9899-A249C90D11FC" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Person Bio", "Person biographic/demographic information and picture (Person detail page).", "~/Blocks/Crm/PersonDetail/Bio.ascx", "CRM > Person Detail", "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C" );
            
            // Remove Icon CSS Class attribute and AttributeValues for DefinedType "LocationType"
            RockMigrationHelper.DeleteAttribute("348B1563-BBC9-4853-BD4E-4CEAE87CA2B6");

            // Update Icon for Snapchat
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "iconcssclass", "fa fa-snapchat-ghost text-shadow", "E9168011-2719-40EB-A082-9337B5F52233" );

            // Delete BlockType 
            //   Name: Family Navigation
            //   Category: CRM > Person Detail
            //   Path: ~/Blocks/Crm/PersonDetail/FamilyNav.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("35D091FA-8311-42D1-83F7-3E67B9EE9675");

            // Delete BlockType 
            //   Name: Person Edit
            //   Category: CRM > Person Edit
            //   Path: ~/Blocks/Crm/PersonDetail/PersonEditControl.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("8C94620B-0FC1-4C39-9474-1714546E7D9E");

            AddNewProfileLayoutsDown();
        }

        private void PersonsBioV2BlockTypeAttributesUp()
        {
            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges", "Badges", "Badges", @"The label badges to display in this block.", 0, @"", "63F838EB-0B94-4F03-8E59-9AFAC8E72FAC" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Workflow Actions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Actions", "WorkflowActions", "Workflow Actions", @"The workflows to make available as actions.", 1, @"", "B8419489-84A9-40AB-B85D-DD5E07255B17" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Additional Custom Actions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Additional Custom Actions", "Actions", "Additional Custom Actions", @"
Additional custom actions (will be displayed after the list of workflow actions). Any instance of '{0}' will be replaced with the current person's id.
Because the contents of this setting will be rendered inside a &lt;ul&gt; element, it is recommended to use an 
&lt;li&gt; element for each available action.  Example:
<pre>
    &lt;li&gt;&lt;a href='~/WorkflowEntry/4?PersonId={0}' tabindex='0'&gt;Fourth Action&lt;/a&gt;&lt;/li&gt;
</pre>", 2, @"", "419196C7-46F9-4AB3-85AA-3ABEE55BD210" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Enable Impersonation
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Impersonation", "EnableImpersonation", "Enable Impersonation", @"Should the Impersonate custom action be enabled? Note: If enabled, it is only visible to users that are authorized to administrate the person.", 3, @"False", "D77CB1E1-E4BC-429A-81C3-FB4AFC924618" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Impersonation Start Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Impersonation Start Page", "ImpersonationStartPage", "Impersonation Start Page", @"The page to navigate to after clicking the Impersonate action.", 4, @"", "63964E1F-42E6-4E5E-BAD1-3DB5A971DB10" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Business Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Business Detail Page", "BusinessDetailPage", "Business Detail Page", @"The page to redirect user to if a business is requested.", 5, @"", "64695DF9-2196-4EB8-AD3B-AE4988FECD65" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Nameless Person Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Nameless Person Detail Page", "NamelessPersonDetailPage", "Nameless Person Detail Page", @"The page to redirect user to if the person record is a Nameless Person record type.", 6, @"", "8D355AC2-BB9D-468C-98BF-D0D426EE98EA" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Display Country Code
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Country Code", "DisplayCountryCode", "Display Country Code", @"When enabled prepends the country code to all phone numbers.", 7, @"False", "B8CF7391-3AFD-4D71-ADB2-BB95714425EC" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Display Middle Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Middle Name", "DisplayMiddleName", "Display Middle Name", @"Display the middle name of the person.", 8, @"False", "9FC18F95-7857-4A4C-8A5F-A6AF5550D9D2" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Custom Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Custom Content", "CustomContent", "Custom Content", @"Custom Content will be rendered after the person's demographic information <span class='tip tip-lava'></span>.", 9, @"", "1CD682C8-FEF3-420A-A925-E158194DCC69" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Allow Following
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Following", "AllowFollowing", "Allow Following", @"Should people be able to follow a person by selecting the following badge?", 10, @"True", "6E268729-EEED-48F9-A2EF-47188E37A538" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Display Graduation
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Graduation", "DisplayGraduation", "Display Graduation", @"Should the Grade/Graduation be displayed?", 11, @"True", "0B501CDE-15F6-4882-8FF9-214676875503" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Display Anniversary Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Anniversary Date", "DisplayAnniversaryDate", "Display Anniversary Date", @"Should the Anniversary Date be displayed?", 12, @"True", "2EA7A2C0-64D8-450B-AD06-FC085E724AD1" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Social Media Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Social Media Category", "SocialMediaCategory", "Social Media Category", @"The Attribute Category to display attributes from.", 13, @"DD8F467D-B83C-444F-B04C-C681167046A1", "53583EDF-1932-4A16-8602-8B083DE5FC8F" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Enable Call Origination
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Call Origination", "EnableCallOrigination", "Enable Call Origination", @"Should click-to-call links be added to phone numbers.", 14, @"True", "65241392-6A20-4EBA-8C54-0DA5E03124E4" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: Communication Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Page", "CommunicationPage", "Communication Page", @"The communication page to use for when the person's email address is clicked. Leave this blank to use the default.", 15, @"", "66CFDF24-8D19-4885-8C09-31DBE8C4126D" );

            // Attribute for BlockType
            //   BlockType: Person Bio Summary
            //   Category: CRM > Person Detail
            //   Attribute: Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7249D05F-0FD1-4F44-88EB-AD46DEB1DAEA", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges", "Badges", "Badges", @"The label badges to display in this block.", 0, @"", "FD5A5196-4231-4CD6-9D34-71EF6C65A312" );
        }
    
        private void AddPersonProfileDetailLayout()
        {
            // Site:Rock RMS
            RockMigrationHelper.AddLayout("C2D29296-6A87-47A9-A753-EE4E9159C4C4","PersonProfileDetail","Person Profile Detail","","6AD84AFC-B3A1-4E30-B53B-C6E57B513839");

            // Add Block 
            //  Block Name: Login Status
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"04712F3D-9667-4901-A49D-4507573EF7AD".AsGuid(), "Login Status","Login",@"",@"",0,"A87E9E32-5EA6-461A-8590-526B1E0B0C3A"); 
            
            // Add Block Attribute Value
            //   Block: Login Status
            //   BlockType: Login Status
            //   Category: Security
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: My Settings Page
            /*   Attribute Value: cf54e680-2e02-4f16-b54b-a2f2d29cd932 */
            RockMigrationHelper.AddBlockAttributeValue("A87E9E32-5EA6-461A-8590-526B1E0B0C3A","FAF7DAAF-4927-44A8-BF4B-080FF556EBB0",@"cf54e680-2e02-4f16-b54b-a2f2d29cd932");

            // Add Block Attribute Value
            //   Block: Login Status
            //   BlockType: Login Status
            //   Category: Security
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: My Profile Page
            /*   Attribute Value: 08dbd8a5-2c35-4146-b4a8-0f7652348b25 */
            RockMigrationHelper.AddBlockAttributeValue("A87E9E32-5EA6-461A-8590-526B1E0B0C3A","6CFDDF63-0B21-48FC-90AE-362C0E73420B",@"08dbd8a5-2c35-4146-b4a8-0f7652348b25");

            // Add Block 
            //  Block Name: Smart Search
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"9D406BD5-88C1-45E5-AFEA-70F9CFB66C74".AsGuid(), "Smart Search","Header",@"",@"",0,"4A6902BC-6594-48FE-8750-7BF935DEFB2C"); 

            // Add Block 
            //  Block Name: Menu
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Menu","Navigation",@"",@"",0,"61BE0C07-D64A-4748-9F51-AD27830E0BAA"); 
        
            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("61BE0C07-D64A-4748-9F51-AD27830E0BAA","EEE71DDE-C6BC-489B-BAA5-1753E322F183",@"False");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageNav.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue("61BE0C07-D64A-4748-9F51-AD27830E0BAA","1322186A-862A-4CF1-B349-28ECB67229BA",@"{% include '~~/Assets/Lava/PageNav.lava' %}");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 20f97a93-7949-4c2a-8a5e-c756fe8585ca */
            RockMigrationHelper.AddBlockAttributeValue("61BE0C07-D64A-4748-9F51-AD27830E0BAA","41F1C42E-2395-4063-BD4F-031DF8D5B231",@"20f97a93-7949-4c2a-8a5e-c756fe8585ca");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("61BE0C07-D64A-4748-9F51-AD27830E0BAA","C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2",@"False");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 3 */
            RockMigrationHelper.AddBlockAttributeValue("61BE0C07-D64A-4748-9F51-AD27830E0BAA","6C952052-BC79-41BA-8B88-AB8EA3E99648",@"3");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("61BE0C07-D64A-4748-9F51-AD27830E0BAA","E4CF237D-1D12-4C93-AFD7-78EB296C4B69",@"False");
        
            // Add Block 
            //  Block Name: Family Navigation
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"35D091FA-8311-42D1-83F7-3E67B9EE9675".AsGuid(), "Family Navigation","ProfileNavigationLeft",@"",@"",0,"7F709821-5B69-49EA-948C-976F63C1A82F"); 
        
            // Add Block 
            //  Block Name: Sub Page Menu
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Sub Page Menu","ProfileNavigation",@"",@"",0,"74EA6E95-4DC8-4490-8911-C56FA0AB4E4A"); 

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("74EA6E95-4DC8-4490-8911-C56FA0AB4E4A","E4CF237D-1D12-4C93-AFD7-78EB296C4B69",@"False");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue("74EA6E95-4DC8-4490-8911-C56FA0AB4E4A","6C952052-BC79-41BA-8B88-AB8EA3E99648",@"1");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("74EA6E95-4DC8-4490-8911-C56FA0AB4E4A","C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2",@"False");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue("74EA6E95-4DC8-4490-8911-C56FA0AB4E4A","1322186A-862A-4CF1-B349-28ECB67229BA",@"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: bf04bb7e-be3a-4a38-a37c-386b55496303 */
            RockMigrationHelper.AddBlockAttributeValue("74EA6E95-4DC8-4490-8911-C56FA0AB4E4A","41F1C42E-2395-4063-BD4F-031DF8D5B231",@"bf04bb7e-be3a-4a38-a37c-386b55496303");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("74EA6E95-4DC8-4490-8911-C56FA0AB4E4A","EEE71DDE-C6BC-489B-BAA5-1753E322F183",@"True");

            // Add Block 
            //  Block Name: Person Edit
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"8C94620B-0FC1-4C39-9474-1714546E7D9E".AsGuid(), "Person Edit","ProfileNavigationRight",@"",@"",0,"9EECF41A-36C6-489C-93DF-2480A3E3BD9B"); 

            // Add Block 
            //  Block Name: Badges
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"2412C653-9369-4772-955E-80EE8FA051E3".AsGuid(), "Badges","BadgeBar",@"",@"",0,"B2680AB4-CC89-47D5-80CD-58F3BA575A08"); 

            // Add Block Attribute Value
            //   Block: Badges
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Top Left Badges
            /*   Attribute Value: b4b336ce-137e-44be-9123-27740d0064c2,8a9ad88e-359f-46fd-9ba1-8b0603644f17,260ead7d-5073-4f88-a6a9-427f6e95985e,66972bff-42cd-49ab-9a7a-e1b9deca4eba,7fc986b9-ca1e-cbb7-4e63-c79eac34f39d */
            RockMigrationHelper.AddBlockAttributeValue("B2680AB4-CC89-47D5-80CD-58F3BA575A08","B22C1920-16D2-45B3-97FD-2EF00D38DD25",@"b4b336ce-137e-44be-9123-27740d0064c2,8a9ad88e-359f-46fd-9ba1-8b0603644f17,260ead7d-5073-4f88-a6a9-427f6e95985e,66972bff-42cd-49ab-9a7a-e1b9deca4eba,7fc986b9-ca1e-cbb7-4e63-c79eac34f39d");

            // Add Block Attribute Value
            //   Block: Badges
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Top Middle Badges
            /*   Attribute Value: 3f7d648d-d6ba-4f03-931c-afbdfa24bbd8 */
            RockMigrationHelper.AddBlockAttributeValue("B2680AB4-CC89-47D5-80CD-58F3BA575A08","61540A33-0905-4ABE-A164-F5F5BA8524DD",@"3f7d648d-d6ba-4f03-931c-afbdfa24bbd8");

            // Add Block Attribute Value
            //   Block: Badges
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Top Right Badges
            /*   Attribute Value: 66972bff-42cd-49ab-9a7a-e1b9deca4ebe,e0455598-82b0-4f49-b806-c3a41c71e9da */
            RockMigrationHelper.AddBlockAttributeValue("B2680AB4-CC89-47D5-80CD-58F3BA575A08","D87184D5-50A4-47D3-BA50-06D39FCCD328",@"66972bff-42cd-49ab-9a7a-e1b9deca4ebe,e0455598-82b0-4f49-b806-c3a41c71e9da");

            // Add Block Attribute Value
            //   Block: Badges
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Bottom Right Badges
            /*   Attribute Value: cce09793-89f6-4042-a98a-ed38392bcfcc */
            RockMigrationHelper.AddBlockAttributeValue("B2680AB4-CC89-47D5-80CD-58F3BA575A08","B39D9047-7D28-42C5-A776-03D71E3A0F36",@"cce09793-89f6-4042-a98a-ed38392bcfcc");

            // Add Block 
            //  Block Name: Person Bio Summary
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"7249D05F-0FD1-4F44-88EB-AD46DEB1DAEA".AsGuid(), "Person Bio Summary","Profile",@"",@"",0,"C9523ABF-7FFA-4F43-ACEE-EE20D5D2C9E5"); 
            
            // Add Block Attribute Value
            //   Block: Person Bio Summary
            //   BlockType: Person Bio Summary
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Badges
            /*   Attribute Value: 66972bff-42cd-49ab-9a7a-e1b9deca4ebf,b21dcd49-ac35-4b2b-9857-75213209b643 */
            RockMigrationHelper.AddBlockAttributeValue("C9523ABF-7FFA-4F43-ACEE-EE20D5D2C9E5","FD5A5196-4231-4CD6-9D34-71EF6C65A312",@"66972bff-42cd-49ab-9a7a-e1b9deca4ebf,b21dcd49-ac35-4b2b-9857-75213209b643");
        }

        private void AddPersonProfileHomeLayout()
        {
            // Site:Rock RMS
            RockMigrationHelper.AddLayout("C2D29296-6A87-47A9-A753-EE4E9159C4C4","PersonProfileHome","Person Profile Home","","92A60013-B8D4-403A-BDFB-C3DA4D867B12");

            // Add Block 
            //  Block Name: Login Status
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"04712F3D-9667-4901-A49D-4507573EF7AD".AsGuid(), "Login Status","Login",@"",@"",0,"7E9F8B26-BD00-464B-B209-F4D60FFD829C"); 

            // Add Block Attribute Value
            //   Block: Login Status
            //   BlockType: Login Status
            //   Category: Security
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: My Profile Page
            /*   Attribute Value: 08dbd8a5-2c35-4146-b4a8-0f7652348b25 */
            RockMigrationHelper.AddBlockAttributeValue("7E9F8B26-BD00-464B-B209-F4D60FFD829C","6CFDDF63-0B21-48FC-90AE-362C0E73420B",@"08dbd8a5-2c35-4146-b4a8-0f7652348b25");

            // Add Block Attribute Value
            //   Block: Login Status
            //   BlockType: Login Status
            //   Category: Security
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: My Settings Page
            /*   Attribute Value: cf54e680-2e02-4f16-b54b-a2f2d29cd932 */
            RockMigrationHelper.AddBlockAttributeValue("7E9F8B26-BD00-464B-B209-F4D60FFD829C","FAF7DAAF-4927-44A8-BF4B-080FF556EBB0",@"cf54e680-2e02-4f16-b54b-a2f2d29cd932");

            // Add Block Attribute Value
            //   Block: Login Status
            //   BlockType: Login Status
            //   Category: Security
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Logged In Page List
            /*   Attribute Value: My Dashboard^~/MyDashboard */
            RockMigrationHelper.AddBlockAttributeValue("7E9F8B26-BD00-464B-B209-F4D60FFD829C","1B0E8904-196B-433E-B1CC-937AD3CA5BF2",@"My Dashboard^~/MyDashboard");

            // Add Block 
            //  Block Name: Smart Search
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"9D406BD5-88C1-45E5-AFEA-70F9CFB66C74".AsGuid(), "Smart Search","Header",@"",@"",0,"34BE95FD-1CC9-4A08-BB8C-15088CAAAB8B");

            // Add Block 
            //  Block Name: Menu
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Menu","Navigation",@"",@"",0,"6A62DC1C-1A29-4156-A431-B3355A3B84AB"); 

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("6A62DC1C-1A29-4156-A431-B3355A3B84AB","C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2",@"False");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("6A62DC1C-1A29-4156-A431-B3355A3B84AB","E4CF237D-1D12-4C93-AFD7-78EB296C4B69",@"False");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageNav.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue("6A62DC1C-1A29-4156-A431-B3355A3B84AB","1322186A-862A-4CF1-B349-28ECB67229BA",@"{% include '~~/Assets/Lava/PageNav.lava' %}");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("6A62DC1C-1A29-4156-A431-B3355A3B84AB","EEE71DDE-C6BC-489B-BAA5-1753E322F183",@"False");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 3 */
            RockMigrationHelper.AddBlockAttributeValue("6A62DC1C-1A29-4156-A431-B3355A3B84AB","6C952052-BC79-41BA-8B88-AB8EA3E99648",@"3");

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 20f97a93-7949-4c2a-8a5e-c756fe8585ca */
            RockMigrationHelper.AddBlockAttributeValue("6A62DC1C-1A29-4156-A431-B3355A3B84AB","41F1C42E-2395-4063-BD4F-031DF8D5B231",@"20f97a93-7949-4c2a-8a5e-c756fe8585ca");
            
            // Add Block 
            //  Block Name: Family Navigation
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"35D091FA-8311-42D1-83F7-3E67B9EE9675".AsGuid(), "Family Navigation","ProfileNavigationLeft",@"",@"",0,"B54D1924-BEE5-43C5-8F19-91B320DF99EC"); 

            // Add Block 
            //  Block Name: Sub Page Menu
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Sub Page Menu","ProfileNavigation",@"",@"",0,"CB964B6B-107E-44D4-8731-7A2D40A9F15B");
            
            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: bf04bb7e-be3a-4a38-a37c-386b55496303 */
            RockMigrationHelper.AddBlockAttributeValue("CB964B6B-107E-44D4-8731-7A2D40A9F15B","41F1C42E-2395-4063-BD4F-031DF8D5B231",@"bf04bb7e-be3a-4a38-a37c-386b55496303");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue("CB964B6B-107E-44D4-8731-7A2D40A9F15B","6C952052-BC79-41BA-8B88-AB8EA3E99648",@"1");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("CB964B6B-107E-44D4-8731-7A2D40A9F15B","EEE71DDE-C6BC-489B-BAA5-1753E322F183",@"True");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue("CB964B6B-107E-44D4-8731-7A2D40A9F15B","1322186A-862A-4CF1-B349-28ECB67229BA",@"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("CB964B6B-107E-44D4-8731-7A2D40A9F15B","E4CF237D-1D12-4C93-AFD7-78EB296C4B69",@"False");

            // Add Block Attribute Value
            //   Block: Sub Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("CB964B6B-107E-44D4-8731-7A2D40A9F15B","C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2",@"False");

            // Add Block 
            //  Block Name: Person Edit
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"8C94620B-0FC1-4C39-9474-1714546E7D9E".AsGuid(), "Person Edit","ProfileNavigationRight",@"",@"",0,"99AD76C5-0D75-44CB-95AA-7AED3E9FC860"); 

            // Add Block 
            //  Block Name: Person Bio
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"030CCDDC-8D43-40F8-A298-78B416F9E828".AsGuid(), "Person Bio","Profile",@"",@"",0,"1E6AF671-9C1A-4C6C-8156-36B6D7589F34"); 

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Badges
            /*   Attribute Value: 66972bff-42cd-49ab-9a7a-e1b9deca4ebf,b21dcd49-ac35-4b2b-9857-75213209b643,66972bff-42cd-49ab-9a7a-e1b9deca4eca */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","63F838EB-0B94-4F03-8E59-9AFAC8E72FAC",@"66972bff-42cd-49ab-9a7a-e1b9deca4ebf,b21dcd49-ac35-4b2b-9857-75213209b643,66972bff-42cd-49ab-9a7a-e1b9deca4eca");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Workflow Actions
            /*   Attribute Value: 221bf486-a82c-40a7-85b7-bb44da45582f,036f2f0b-c2dc-49d0-a17b-ccdac7fc71e2,31ddc001-c91a-4418-b375-cab1475f7a62,9bc07356-3b2f-4bff-9320-fa8f3a28fc39 */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","B8419489-84A9-40AB-B85D-DD5E07255B17",@"221bf486-a82c-40a7-85b7-bb44da45582f,036f2f0b-c2dc-49d0-a17b-ccdac7fc71e2,31ddc001-c91a-4418-b375-cab1475f7a62,9bc07356-3b2f-4bff-9320-fa8f3a28fc39");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Enable Impersonation
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","D77CB1E1-E4BC-429A-81C3-FB4AFC924618",@"False");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Business Detail Page
            /*   Attribute Value: d2b43273-c64f-4f57-9aae-9571e1982bac */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","64695DF9-2196-4EB8-AD3B-AE4988FECD65",@"d2b43273-c64f-4f57-9aae-9571e1982bac");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Nameless Person Detail Page
            /*   Attribute Value: 62f18233-0395-4bea-adc7-bc08271edaf1 */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","8D355AC2-BB9D-468C-98BF-D0D426EE98EA",@"62f18233-0395-4bea-adc7-bc08271edaf1");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Display Country Code
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","B8CF7391-3AFD-4D71-ADB2-BB95714425EC",@"False");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Display Middle Name
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","9FC18F95-7857-4A4C-8A5F-A6AF5550D9D2",@"False");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Allow Following
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","6E268729-EEED-48F9-A2EF-47188E37A538",@"True");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Display Graduation
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","0B501CDE-15F6-4882-8FF9-214676875503",@"True");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Display Anniversary Date
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","2EA7A2C0-64D8-450B-AD06-FC085E724AD1",@"True");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Social Media Category
            /*   Attribute Value: dd8f467d-b83c-444f-b04c-c681167046a1 */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","53583EDF-1932-4A16-8602-8B083DE5FC8F",@"dd8f467d-b83c-444f-b04c-c681167046a1");

            // Add Block Attribute Value
            //   Block: Person Bio
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Enable Call Origination
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("1E6AF671-9C1A-4C6C-8156-36B6D7589F34","65241392-6A20-4EBA-8C54-0DA5E03124E4",@"True");

            // Add Block 
            //  Block Name: Family Members
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"7BFD4000-ED0E-41B8-8DD5-C36973C36E1F".AsGuid(), "Family Members","Profile",@"",@"",1,"0E6D894F-EF32-45F4-A189-32E05E5559CB"); 

            // Add Block Attribute Value
            //   Block: Family Members
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Group Type
            /*   Attribute Value: 790e3215-3b10-442b-af69-616c0dcb998e */
            RockMigrationHelper.AddBlockAttributeValue("0E6D894F-EF32-45F4-A189-32E05E5559CB","A243BEF8-D6B1-4F63-BA6A-1830DAF11729",@"790e3215-3b10-442b-af69-616c0dcb998e");

            // Add Block Attribute Value
            //   Block: Family Members
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Auto Create Group
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("0E6D894F-EF32-45F4-A189-32E05E5559CB","0BD34461-AFEB-4D0D-84D5-3779DB559EDE",@"True");

            // Add Block Attribute Value
            //   Block: Family Members
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Group Edit Page
            /*   Attribute Value: e9e1e5f2-467d-47cb-af41-b4d9ef8b0b27 */
            RockMigrationHelper.AddBlockAttributeValue("0E6D894F-EF32-45F4-A189-32E05E5559CB","2B17A52F-1FB6-4DA8-A2C2-447E1AB57571",@"e9e1e5f2-467d-47cb-af41-b4d9ef8b0b27");

            // Add Block Attribute Value
            //   Block: Family Members
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Location Detail Page
            /*   Attribute Value: 4ce2a5da-15f3-454c-8172-d146d938e203 */
            RockMigrationHelper.AddBlockAttributeValue("0E6D894F-EF32-45F4-A189-32E05E5559CB","5F121290-D32B-4F36-BAC1-B3036C696E10",@"4ce2a5da-15f3-454c-8172-d146d938e203");

            // Add Block Attribute Value
            //   Block: Family Members
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Show County
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("0E6D894F-EF32-45F4-A189-32E05E5559CB","7E032ABA-2AFA-4B29-9024-A9281AEA1BE8",@"False");

            // Add Block 
            //  Block Name: Badges
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"2412C653-9369-4772-955E-80EE8FA051E3".AsGuid(), "Badges","BadgeBar",@"",@"",0,"E82F7A07-FBD5-417B-A976-AC63CC53BF68"); 

            // Add Block Attribute Value
            //   Block: Badges
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Top Left Badges
            /*   Attribute Value: b4b336ce-137e-44be-9123-27740d0064c2,8a9ad88e-359f-46fd-9ba1-8b0603644f17,260ead7d-5073-4f88-a6a9-427f6e95985e,66972bff-42cd-49ab-9a7a-e1b9deca4eba,7fc986b9-ca1e-cbb7-4e63-c79eac34f39d */
            RockMigrationHelper.AddBlockAttributeValue("E82F7A07-FBD5-417B-A976-AC63CC53BF68","B22C1920-16D2-45B3-97FD-2EF00D38DD25",@"b4b336ce-137e-44be-9123-27740d0064c2,8a9ad88e-359f-46fd-9ba1-8b0603644f17,260ead7d-5073-4f88-a6a9-427f6e95985e,66972bff-42cd-49ab-9a7a-e1b9deca4eba,7fc986b9-ca1e-cbb7-4e63-c79eac34f39d");

            // Add Block Attribute Value
            //   Block: Badges
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Top Middle Badges
            /*   Attribute Value: 3f7d648d-d6ba-4f03-931c-afbdfa24bbd8 */
            RockMigrationHelper.AddBlockAttributeValue("E82F7A07-FBD5-417B-A976-AC63CC53BF68","61540A33-0905-4ABE-A164-F5F5BA8524DD",@"3f7d648d-d6ba-4f03-931c-afbdfa24bbd8");

            // Add Block Attribute Value
            //   Block: Badges
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Top Right Badges
            /*   Attribute Value: 66972bff-42cd-49ab-9a7a-e1b9deca4ebe,e0455598-82b0-4f49-b806-c3a41c71e9da */
            RockMigrationHelper.AddBlockAttributeValue("E82F7A07-FBD5-417B-A976-AC63CC53BF68","D87184D5-50A4-47D3-BA50-06D39FCCD328",@"66972bff-42cd-49ab-9a7a-e1b9deca4ebe,e0455598-82b0-4f49-b806-c3a41c71e9da");

            // Add Block Attribute Value
            //   Block: Badges
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Bottom Right Badges
            /*   Attribute Value: cce09793-89f6-4042-a98a-ed38392bcfcc */
            RockMigrationHelper.AddBlockAttributeValue("E82F7A07-FBD5-417B-A976-AC63CC53BF68","B39D9047-7D28-42C5-A776-03D71E3A0F36",@"cce09793-89f6-4042-a98a-ed38392bcfcc");

            // Add Block 
            //  Block Name: Footer Content
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null,"92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(),"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Footer Content","Footer",@"",@"",0,"85A5921B-41CE-4E90-BAB4-24C51F8C1D23"); 

            // Add/Update HtmlContent for Block: Footer Content
            RockMigrationHelper.UpdateHtmlContentBlock("85A5921B-41CE-4E90-BAB4-24C51F8C1D23",@"<p>Crafted by <a href=""https://www.rockrms.com"" tabindex=""0"">Spark Development Network</a> / <a href=""~/License.aspx"" tabindex=""0"">License</a></p>","C940149C-3C57-4949-9EB2-61E326CC2772"); 

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Start in Code Editor mode
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","0673E015-F8DD-4A52-B380-C758011331B2",@"True");

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Image Root Folder
            /*   Attribute Value: ~/Content */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","26F3AFC6-C05B-44A4-8593-AFE1D9969B0E",@"~/Content");

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: User Specific Folders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE",@"False");

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Document Root Folder
            /*   Attribute Value: ~/Content */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","3BDB8AED-32C5-4879-B1CB-8FC7C8336534",@"~/Content");

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4",@"False");

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Validate Markup
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","6E71FE26-5628-4DDA-BDBC-8E4D47BE72CD",@"True");

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Context Name
            /*   Attribute Value: RockFooterText */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","466993F7-D838-447A-97E7-8BBDA6A57289",@"RockFooterText");

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Cache Duration
            /*   Attribute Value: 3600 */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4",@"3600");

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Require Approval
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A",@"False");

            // Add Block Attribute Value
            //   Block: Footer Content
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Layout=Person Profile Home, Site=Rock RMS
            //   Attribute: Enable Versioning
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue("85A5921B-41CE-4E90-BAB4-24C51F8C1D23","7C1CE199-86CF-4EAE-8AB3-848416A72C58",@"False");
        }

        private void AddNewProfileLayoutsDown()
        {
            RockMigrationHelper.DeleteLayout("92A60013-B8D4-403A-BDFB-C3DA4D867B12"); //  Layout: Person Profile Home, Site: Rock RMS
            RockMigrationHelper.DeleteLayout("6AD84AFC-B3A1-4E30-B53B-C6E57B513839"); //  Layout: Person Profile Detail, Site: Rock RMS
        }
    
        private void CreateV1Pages()
        {
            Sql( @"
                -- Get the PersonPage Id
                DECLARE @PersonPages_PageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = 'BF04BB7E-BE3A-4A38-A37C-386B55496303');

                -- Use the the person page is the first page
                DECLARE @PageId INT = @PersonPages_PageId;
                DECLARE @PageGuid UNIQUEIDENTIFIER = 'BF04BB7E-BE3A-4A38-A37C-386B55496303';
                DECLARE @InsertedPageId INT;
                DECLARE @InsertedPageGuid UNIQUEIDENTIFIER;
                DECLARE @InsertedBlockID INT;
                DECLARE @Block_EntityType_Id INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65');
                DECLARE @Page_EntityType_Id INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'E104DCDF-247C-4CED-A119-8CC51632761F');
                DECLARE @PageReferenceFieldType_Id INT = (SELECT [Id] FROM FieldType WHERE [Guid] = 'BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108')

                -- Create a table of the new and old page IDs. This is needed to update the parent page because some of the parents have a higher ID then the child page and so must be updated after all pages are inserted.
                CREATE TABLE #tmpPageIds(OldPageId INT, OldPageGuid UNIQUEIDENTIFIER, NewPageId INT, NewPageGuid UNIQUEIDENTIFIER);

                -- Get PersonPage Decendent IDs
                DECLARE page_cursor CURSOR FOR
                WITH CTE ([Id], [Guid]) AS (
                SELECT [Id], [Guid] FROM [Page] WHERE [ParentPageId] = @PersonPages_PageId
                UNION ALL
                SELECT a.[Id], a.[Guid] FROM [Page] a
                INNER JOIN CTE pcte ON pcte.[Id] = a.[ParentPageId])

                SELECT * FROM CTE

                OPEN page_cursor
                WHILE @@FETCH_STATUS = 0
                BEGIN

	                SET @InsertedPageGuid = NEWID()

	                -- Create a copy of the page
	                INSERT INTO [Page] ([InternalName], [ParentPageId], [PageTitle], [IsSystem], [LayoutId], [RequiresEncryption], [EnableViewState], [PageDisplayTitle], [PageDisplayBreadCrumb], [PageDisplayIcon], [PageDisplayDescription], [DisplayInNavWhen], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [BreadCrumbDisplayName], [BreadCrumbDisplayIcon], [Order], [OutputCacheDuration], [Description], [IconCssClass], [IncludeAdminFooter], [Guid], [BrowserTitle], [KeyWords], [HeaderContent], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId], [AllowIndexing], [BodyCssClass], [IconBinaryFileId], [AdditionalSettings], [MedianPageLoadTimeDurationSeconds], [CacheControlHeaderSettings])
	                SELECT [InternalName] + ' V1', [ParentPageId], [PageTitle] + ' V1', [IsSystem], [LayoutId], [RequiresEncryption], [EnableViewState], [PageDisplayTitle], [PageDisplayBreadCrumb], [PageDisplayIcon], [PageDisplayDescription], [DisplayInNavWhen], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [BreadCrumbDisplayName], [BreadCrumbDisplayIcon], [Order], [OutputCacheDuration], [Description], [IconCssClass], [IncludeAdminFooter], @InsertedPageGuid, [BrowserTitle], [KeyWords], [HeaderContent], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId], [AllowIndexing], [BodyCssClass], [IconBinaryFileId], [AdditionalSettings], [MedianPageLoadTimeDurationSeconds], [CacheControlHeaderSettings]
	                FROM [Page]
	                WHERE [Id] = @PageId

	                SET @InsertedPageId = @@IDENTITY;

	                -- Page Route
	                INSERT INTO [PageRoute] ([IsSystem], [PageId], [Route], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId], [IsGlobal])
	                SELECT [IsSystem], @InsertedPageId, 'v1/' + [Route], NEWID(), [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId], [IsGlobal]
	                FROM [PageRoute]
	                WHERE [PageId] = @PageId

	                -- Page Context
	                INSERT INTO [PageContext] ([IsSystem], [PageId], [Entity], [IdParameter], [CreatedDateTime], [Guid], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId])
	                SELECT [IsSystem], @InsertedPageId, [Entity], [IdParameter], [CreatedDateTime], NEWID(), [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId]
	                FROM [PageContext]
	                WHERE [PageId] = @PageId

	                -- Auth
	                INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [PersonAliasId], [ForeignGuid], [ForeignId])
	                SELECT [EntityTypeId], @InsertedPageId, [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], NEWID(), [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [PersonAliasId], [ForeignGuid], [ForeignId]
	                FROM [Auth]
	                WHERE [EntityTypeId] = @Page_EntityType_Id AND [EntityId] = @PageId

	                INSERT INTO #tmpPageIds VALUES (@PageId, @PageGuid, @InsertedPageId, @InsertedPageGuid);

	                -- Create a copy of the blocks and their attribute values
	                DECLARE @BlockId INT
	                DECLARE @BlockTypeId INT
	                DECLARE block_cursor CURSOR FOR SELECT [Id], [BlockTypeId] FROM [Block] WHERE [PageId] = @PageId
	                OPEN block_cursor
		                FETCH NEXT FROM block_cursor INTO @BlockId, @BlockTypeId

		                -- Create a copy of the blocks with the new page Id
		                INSERT INTO [Block] ([IsSystem], [PageId], [LayoutId], [BlockTypeId], [Zone], [Order], [Name], [CssClass], [OutputCacheDuration], [Guid], [PreHtml], [PostHtml], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId], [SiteId], [AdditionalSettings])
		                SELECT [IsSystem], @InsertedPageId, [LayoutId], [BlockTypeId], [Zone], [Order], [Name], [CssClass], [OutputCacheDuration], NEWID(), [PreHtml], [PostHtml], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId], [SiteId], [AdditionalSettings]
		                FROM [Block]
		                WHERE [Id] = @BlockId

		                SET @InsertedBlockID = @@IDENTITY;

		                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ValueAsDateTime], [ForeignGuid], [ForeignId], [ValueAsNumeric])
		                SELECT av.[IsSystem], av.[AttributeId], @InsertedBlockID, av.[Value], NEWID(), av.[CreatedDateTime], av.[ModifiedDateTime], av.[CreatedByPersonAliasId], av.[ModifiedByPersonAliasId], av.[ForeignKey], av.[ValueAsDateTime], av.[ForeignGuid], av.[ForeignId], av.[ValueAsNumeric]
		                FROM Attribute a
		                JOIN AttributeValue av ON a.Id = av.AttributeId
		                WHERE a.EntityTypeId = @Block_EntityType_Id
			                AND a.EntityTypeQualifierColumn = 'BlockTypeId'
			                AND a.EntityTypeQualifierValue = @BlockTypeId
			                AND av.EntityId = @BlockId

		                FETCH NEXT FROM block_cursor INTO @BlockId, @BlockTypeId
	                CLOSE block_cursor
	                DEALLOCATE block_cursor

	                FETCH NEXT FROM page_cursor INTO @PageId, @PageGuid
                END
                CLOSE page_cursor
                DEALLOCATE page_cursor

                -- Update the ParentPageIds
                UPDATE [Page]
                SET [ParentPageId] = (SELECT NewPageId FROM #tmpPageIds WHERE [Page].[ParentPageId] = #tmpPageIds.[OldPageId])
                WHERE [Page].[Id] IN (SELECT NewPageId FROM #tmpPageIds)
                AND [page].[InternalName] != 'Person Pages V1'

                -- Update the page references for the V1 pages
                UPDATE av
                SET av.Value = (SELECT [NewPageGuid] FROM #tmpPageIds WHERE CHARINDEX( CAST(#tmpPageIds.[OldPageGuid] AS VARCHAR(50)), av.[Value]) > 0)
                FROM [Page] p
                JOIN [Block] b ON p.[Id] = b.[PageId]
                JOIN [BlockType] bt ON bt.[Id] = b.[BlockTypeId]
                JOIN [Attribute] a ON a.[EntityTypeQualifierColumn] = 'BlockTypeId' AND a.[EntityTypeQualifierValue] = bt.[Id] AND a.FieldTypeId = @PageReferenceFieldType_Id
                JOIN [AttributeValue] av ON av.[AttributeId] = a.[Id] AND av.EntityId = b.Id
                WHERE p.[Id] IN (SELECT [NewPageId] FROM #tmpPageIds)
	                AND (SELECT [NewPageGuid] FROM #tmpPageIds WHERE CHARINDEX( CAST(#tmpPageIds.[OldPageGuid] AS VARCHAR(50)), av.[Value]) > 0) IS NOT NULL

                DROP TABLE #tmpPageIds" );
        }
    
        private void UpdatePersonPageLayouts()
        {
            RockMigrationHelper.UpdatePageLayout( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "92A60013-B8D4-403A-BDFB-C3DA4D867B12" );
            RockMigrationHelper.UpdatePageLayout( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE", "6AD84AFC-B3A1-4E30-B53B-C6E57B513839" );
            RockMigrationHelper.UpdatePageLayout( "CB9ABA3B-6962-4A42-BDA1-EA71B7309232", "6AD84AFC-B3A1-4E30-B53B-C6E57B513839" );
            RockMigrationHelper.UpdatePageLayout( "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D", "6AD84AFC-B3A1-4E30-B53B-C6E57B513839" );
            RockMigrationHelper.UpdatePageLayout( "6155FBC2-03E9-48C1-B2E7-554CBB7589A5", "6AD84AFC-B3A1-4E30-B53B-C6E57B513839" );
            RockMigrationHelper.UpdatePageLayout( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "6AD84AFC-B3A1-4E30-B53B-C6E57B513839" );
            RockMigrationHelper.UpdatePageLayout( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "6AD84AFC-B3A1-4E30-B53B-C6E57B513839" );
            RockMigrationHelper.UpdatePageLayout( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "6AD84AFC-B3A1-4E30-B53B-C6E57B513839" );
            RockMigrationHelper.UpdatePageLayout( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "6AD84AFC-B3A1-4E30-B53B-C6E57B513839" );
        }
    }
}

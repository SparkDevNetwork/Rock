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
            RockMigrationHelper.AddDefinedTypeAttribute( "2E68D37C-FB7B-4AA5-9E09-3785D52156CB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Icon CSS Class", "IconCSSClass", "", 2014, "", "348B1563-BBC9-4853-BD4E-4CEAE87CA2B6" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "348B1563-BBC9-4853-BD4E-4CEAE87CA2B6", @"fa fa-home" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E071472A-F805-4FC4-917A-D5E3C095C35C", "348B1563-BBC9-4853-BD4E-4CEAE87CA2B6", @"fa fa-building" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "853D98F1-6E08-4321-861B-520B4106CFE0", "348B1563-BBC9-4853-BD4E-4CEAE87CA2B6", @"fa fa-map-marker" );

            // Update Icon for Snapchat
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "iconcssclass", "fa fa-fw fa-snapchat", "E9168011-2719-40EB-A082-9337B5F52233" );

            // Add/Update BlockType 
            //   Name: Family Navigation
            //   Category: CRM > Person Detail
            //   Path: ~/Blocks/Crm/PersonDetail/FamilyNav.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Family Navigation", "Allows you to switch between the members of the family the person belongs to.", "~/Blocks/Crm/PersonDetail/FamilyNav.ascx", "CRM > Person Detail", "35D091FA-8311-42D1-83F7-3E67B9EE9675" );

            // Add/Update BlockType 
            //   Name: Person Edit
            //   Category: CRM > Person Edit
            //   Path: ~/Blocks/Crm/PersonDetail/PersonEditControl.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Person Edit", "Allows you to navigate to the person edit page.", "~/Blocks/Crm/PersonDetail/PersonEditControl.ascx", "CRM > Person Edit", "8C94620B-0FC1-4C39-9474-1714546E7D9E" );

            // Add/Update BlockType 
            //   Name: Person Bio Summary
            //   Category: CRM > Person Detail
            //   Path: ~/Blocks/Crm/PersonDetail/BioSummary.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Person Bio Summary", "Person name, picture, and badges.", "~/Blocks/Crm/PersonDetail/BioSummary.ascx", "CRM > Person Detail", "7249D05F-0FD1-4F44-88EB-AD46DEB1DAEA" );

            PersonsBioV2BlockTypeAttributesUp();
            AddPersonProfileHomeLayout();
            AddPersonProfileDetailLayout();

            UpdatePersonPageLayouts();

            Sql("UPDATE [Page] set PageTitle = 'Profile', InternalName = 'Person Profile', BrowserTitle = 'Profile' where [Guid] ='08DBD8A5-2C35-4146-B4A8-0F7652348B25'");

            // Clear Notes Block Attribute Values for Heading and Heading Icon
            //   Block: TimeLine
            //   BlockType: Notes
            //   Category: Core
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Heading
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue("0B2B550C-B0C9-420E-9CF3-BEC8979108F2","3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69",@"");

            //   Attribute: Heading Icon CSS Class
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue("0B2B550C-B0C9-420E-9CF3-BEC8979108F2","B69937BE-000A-4B94-852F-16DE92344392",@"");
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
            RockMigrationHelper.DeleteAttribute( "348B1563-BBC9-4853-BD4E-4CEAE87CA2B6" );

            // Update Icon for Snapchat
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "iconcssclass", "fa fa-snapchat-ghost text-shadow", "E9168011-2719-40EB-A082-9337B5F52233" );

            // Delete BlockType 
            //   Name: Family Navigation
            //   Category: CRM > Person Detail
            //   Path: ~/Blocks/Crm/PersonDetail/FamilyNav.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "35D091FA-8311-42D1-83F7-3E67B9EE9675" );

            // Delete BlockType 
            //   Name: Person Edit
            //   Category: CRM > Person Edit
            //   Path: ~/Blocks/Crm/PersonDetail/PersonEditControl.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "8C94620B-0FC1-4C39-9474-1714546E7D9E" );

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
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Page", "CommunicationPage", "Communication Page", @"The communication page to use when the email button or person's email address is clicked. Leave this blank to use the default.", 15, @"", "66CFDF24-8D19-4885-8C09-31DBE8C4126D" );

            // Attribute for BlockType
            //   BlockType: Person Bio
            //   Category: CRM > Person Detail
            //   Attribute: SMS Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "030CCDDC-8D43-40F8-A298-78B416F9E828", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "SMS Page", "SmsPage", "SMS Page", @"The communication page to use when the text button is clicked. Leave this blank to use the default.", 16, @"", "860FCAFF-6DE4-4221-BCD9-6533BE90FC0C" );

            // Attribute for BlockType
            //   BlockType: Person Bio Summary
            //   Category: CRM > Person Detail
            //   Attribute: Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7249D05F-0FD1-4F44-88EB-AD46DEB1DAEA", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges", "Badges", "Badges", @"The label badges to display in this block.", 0, @"", "FD5A5196-4231-4CD6-9D34-71EF6C65A312" );

            // Attribute for BlockType
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Attribute: Group Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "Group Type", @"The group type to display groups for (default is Family)", 0, @"790E3215-3B10-442B-AF69-616C0DCB998E", "8F07815E-791F-4CC2-BB90-8A0552EA6697" );

            // Attribute for BlockType
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Attribute: Auto Create Group
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Auto Create Group", "AutoCreateGroup", "Auto Create Group", @"If person doesn't belong to a group of this type, should one be created for them (default is Yes).", 1, @"True", "BDB2514F-95C1-4054-B3EC-76C972048D56" );

            // Attribute for BlockType
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Attribute: Group Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Edit Page", "GroupEditPage", "Group Edit Page", @"Page used to edit the members of the selected group.", 2, @"", "FA36CC50-9FFC-4AC6-BC96-874C967EA44D" );

            // Attribute for BlockType
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Attribute: Location Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Location Detail Page", "LocationDetailPage", "Location Detail Page", @"Page used to edit the settings for a particular location.", 3, @"", "41D83CE7-8052-402E-9815-E4E0EDABD85E" );

            // Attribute for BlockType
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Attribute: Show County
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show County", "ShowCounty", "Show County", @"Should County be displayed when editing an address?.", 4, @"False", "31233DD5-DCB7-42FC-8151-517E32B29CBD" );

            // Attribute for BlockType
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Attribute: Group Header Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Group Header Lava", "GroupHeaderLava", "Group Header Lava", @"Lava to put at the top of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.", 5, @"", "E620907F-A5BF-4AF0-8A6C-8618CFC0CBB4" );

            // Attribute for BlockType
            //   BlockType: Group Members
            //   Category: CRM > Person Detail
            //   Attribute: Group Footer Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Group Footer Lava", "GroupFooterLava", "Group Footer Lava", @"Lava to put at the bottom of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.", 6, @"", "036C6A27-B0A9-4219-A0E0-A368FF8FE8D2" );
        }

        private void AddPersonProfileDetailLayout()
        {
            // Site:Rock RMS
            RockMigrationHelper.AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "PersonProfileDetail", "Person Profile Detail", "", Rock.SystemGuid.Layout.PERSON_PROFILE_DETAIL );

            #region Login Status Block

            // Add Block 
            //  Block Name: Login Status
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "04712F3D-9667-4901-A49D-4507573EF7AD".AsGuid(), "Login Status", "Login", @"", @"", 0, "A87E9E32-5EA6-461A-8590-526B1E0B0C3A" );

            // Add Block Attribute Value
            // Block: Login Status
            // BlockType: Login Status
            // Category: Security
            // Block Location: Layout=Person Profile Detail, Site=Rock RMS

            // Attribute: My Profile Page
            RockMigrationHelper.AddBlockAttributeValue( "A87E9E32-5EA6-461A-8590-526B1E0B0C3A", "6CFDDF63-0B21-48FC-90AE-362C0E73420B", @"08DBD8A5-2C35-4146-B4A8-0F7652348B25" );

            // Attribute: My Settings Page
            AddPersonPageBlockAttributeValue( "A87E9E32-5EA6-461A-8590-526B1E0B0C3A", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0", Rock.SystemGuid.Layout.PERSON_DETAIL, "04712F3D-9667-4901-A49D-4507573EF7AD", "Login Status", "MySettingsPage" );

            // Attribute: Logged In Page List
            AddPersonPageBlockAttributeValue( "A87E9E32-5EA6-461A-8590-526B1E0B0C3A", "1B0E8904-196B-433E-B1CC-937AD3CA5BF2", Rock.SystemGuid.Layout.PERSON_DETAIL, "04712F3D-9667-4901-A49D-4507573EF7AD", "Login Status", "LoggedInPageList" );

            #endregion Login Status Block

            #region Smart Search Block

            // Add Block 
            //  Block Name: Smart Search
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74".AsGuid(), "Smart Search", "Header", @"", @"", 0, "4A6902BC-6594-48FE-8750-7BF935DEFB2C" );

            #endregion Smart Search Block

            #region Menu Block

            // Add Block 
            //  Block Name: Menu
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Menu", "Navigation", @"", @"", 0, "61BE0C07-D64A-4748-9F51-AD27830E0BAA" );

            // Add Block Attribute Value
            //   Block: Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS

            // Attribute: CSS File
            AddPersonPageBlockAttributeValue( "61BE0C07-D64A-4748-9F51-AD27830E0BAA", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "CSSFile" );

            // Attribute: Include Current Parameters
            AddPersonPageBlockAttributeValue( "61BE0C07-D64A-4748-9F51-AD27830E0BAA", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "IncludeCurrentParameters" );

            // Attribute: Include Current QueryString
            AddPersonPageBlockAttributeValue( "61BE0C07-D64A-4748-9F51-AD27830E0BAA", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "IncludeCurrentQueryString" );

            // Attribute: Include Page List
            AddPersonPageBlockAttributeValue( "61BE0C07-D64A-4748-9F51-AD27830E0BAA", "0A49DABE-42EE-40E5-9E06-0E6530944865", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "IncludePageList" );

            // Attribute: Is Secondary Block
            AddPersonPageBlockAttributeValue( "61BE0C07-D64A-4748-9F51-AD27830E0BAA", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "IsSecondaryBlock" );

            // Attribute: Number of Levels
            AddPersonPageBlockAttributeValue( "61BE0C07-D64A-4748-9F51-AD27830E0BAA", "6C952052-BC79-41BA-8B88-AB8EA3E99648", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "NumberofLevels" );

            // Attribute: Root Page
            AddPersonPageBlockAttributeValue( "61BE0C07-D64A-4748-9F51-AD27830E0BAA", "41F1C42E-2395-4063-BD4F-031DF8D5B231", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "RootPage" );

            // Attribute: Template
            AddPersonPageBlockAttributeValue( "61BE0C07-D64A-4748-9F51-AD27830E0BAA", "1322186A-862A-4CF1-B349-28ECB67229BA", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "Template" );

            // Attribute: Enabled Lava Commands
            AddPersonPageBlockAttributeValue( "61BE0C07-D64A-4748-9F51-AD27830E0BAA", "EF10B2F9-93E5-426F-8D43-8C020224670F", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "EnabledLavaCommands" );

            #endregion Menu Block

            #region Family Navigation Block

            // Add Block 
            //  Block Name: Family Navigation
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "35D091FA-8311-42D1-83F7-3E67B9EE9675".AsGuid(), "Family Navigation", "ProfileNavigationLeft", @"", @"", 0, "7F709821-5B69-49EA-948C-976F63C1A82F" );

            #endregion Family Navigation Block

            #region Sub Page Menu

            // Add Block 
            //  Block Name: Sub Page Menu
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Sub Page Menu", "ProfileNavigation", @"", @"", 0, "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A" );

            // Add Block Attribute Value
            // Block: Sub Page Menu
            // BlockType: Page Menu
            // Category: CMS
            // Block Location: Layout=Person Profile Detail, Site=Rock RMS

            // Attribute: CSS File
            AddPersonPageBlockAttributeValue( "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "CSSFile" );

            // Attribute: Include Current Parameters
            AddPersonPageBlockAttributeValue( "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "IncludeCurrentParameters" );

            // Attribute: Include Current QueryString
            AddPersonPageBlockAttributeValue( "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "IncludeCurrentQueryString" );

            // Attribute: Include Page List
            AddPersonPageBlockAttributeValue( "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A", "0A49DABE-42EE-40E5-9E06-0E6530944865", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "IncludePageList" );

            // Attribute: Is Secondary Block
            AddPersonPageBlockAttributeValue( "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "IsSecondaryBlock" );

            // Attribute: Number of Levels
            AddPersonPageBlockAttributeValue( "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A", "6C952052-BC79-41BA-8B88-AB8EA3E99648", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "NumberofLevels" );

            // Attribute: Root Page
            RockMigrationHelper.AddBlockAttributeValue( "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"BF04BB7E-BE3A-4A38-A37C-386B55496303" );

            // Attribute: Template
            RockMigrationHelper.AddBlockAttributeValue( "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageListProfile.lava' %}" );

            // Attribute: Enabled Lava Commands
            AddPersonPageBlockAttributeValue( "74EA6E95-4DC8-4490-8911-C56FA0AB4E4A", "EF10B2F9-93E5-426F-8D43-8C020224670F", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "EnabledLavaCommands" );

            #endregion Sub Page Menu

            #region Person Edit Block

            // Add Block 
            //  Block Name: Person Edit
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8C94620B-0FC1-4C39-9474-1714546E7D9E".AsGuid(), "Person Edit", "ProfileNavigationRight", @"", @"", 0, "9EECF41A-36C6-489C-93DF-2480A3E3BD9B" );

            #endregion Person Edit Block

            #region Badges Block

            // Add Block 
            //  Block Name: Badges
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2412C653-9369-4772-955E-80EE8FA051E3".AsGuid(), "Badges", "BadgeBar", @"", @"", 0, "B2680AB4-CC89-47D5-80CD-58F3BA575A08" );

            // Add Block Attribute Value
            // Block: Badges
            // BlockType: Badges
            // Category: Obsidian > CRM > Person Detail
            // Block Location: Layout=Person Profile Detail, Site=Rock RMS

            // Attribute: Top Left Badges
            AddPersonPageBlockAttributeValue( "B2680AB4-CC89-47D5-80CD-58F3BA575A08", "B22C1920-16D2-45B3-97FD-2EF00D38DD25", Rock.SystemGuid.Layout.PERSON_DETAIL, "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "Badges 1", "Badges" );

            // Attribute: Top Middle Badges
            AddPersonPageBlockAttributeValue( "B2680AB4-CC89-47D5-80CD-58F3BA575A08", "61540A33-0905-4ABE-A164-F5F5BA8524DD", Rock.SystemGuid.Layout.PERSON_DETAIL, "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "Badges 2", "Badges" );

            // Attribute: Top Right Badges. Needed to remove the assesments badge from this list if it exists because it will be displayed below.
            var sqlStringForBadges3Values = $@"
                DECLARE @BlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
                DECLARE @V1LayoutId INT = (SELECT [Id] FROM [Layout] WHERE [Guid] = '{Rock.SystemGuid.Layout.PERSON_DETAIL}')
                DECLARE @V1BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'FC8AF928-C4AF-40C7-A667-4B24390F03A1')
                DECLARE @V1BlockId INT = (SELECT [Id] FROM [Block] WHERE [LayoutId] = @V1LayoutId AND [Name] = 'Badges 3')
                DECLARE @AttributeKey VARCHAR(1000) = 'Badges'

                SELECT av.[Value]
                FROM [Attribute] a
                JOIN [AttributeValue] av on a.[Id] = av.[AttributeId]
                WHERE a.[EntityTypeId] = @BlockEntityTypeId
                    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND a.[EntityTypeQualifierValue] = @V1BlockTypeId
                    AND a.[Key] = @AttributeKey
                    AND av.EntityId = @V1BlockId";

            var personBioBadgesAttributeValue = SqlScalar( sqlStringForBadges3Values ).ToStringSafe().Replace( "CCE09793-89F6-4042-A98A-ED38392BCFCC", string.Empty );

            if ( personBioBadgesAttributeValue != null )
            {
                RockMigrationHelper.AddBlockAttributeValue( "B2680AB4-CC89-47D5-80CD-58F3BA575A08", "D87184D5-50A4-47D3-BA50-06D39FCCD328", personBioBadgesAttributeValue );
            }

            // Attribute: Bottom Right Badges
            RockMigrationHelper.AddBlockAttributeValue( "B2680AB4-CC89-47D5-80CD-58F3BA575A08", "B39D9047-7D28-42C5-A776-03D71E3A0F36", @"CCE09793-89F6-4042-A98A-ED38392BCFCC" );

            #endregion Badges Block

            #region Person Bio Summary Block

            // Add Block 
            //  Block Name: Person Bio Summary
            //  Page Name: -
            //  Layout: Person Profile Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "6AD84AFC-B3A1-4E30-B53B-C6E57B513839".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7249D05F-0FD1-4F44-88EB-AD46DEB1DAEA".AsGuid(), "Person Bio Summary", "Profile", @"", @"", 0, "C9523ABF-7FFA-4F43-ACEE-EE20D5D2C9E5" );

            // Add Block Attribute Value
            //   Block: Person Bio Summary
            //   BlockType: Person Bio Summary
            //   Category: CRM > Person Detail
            //   Block Location: Layout=Person Profile Detail, Site=Rock RMS
            //   Attribute: Badges
            /*   Attribute Value: 66972bff-42cd-49ab-9a7a-e1b9deca4ebf,b21dcd49-ac35-4b2b-9857-75213209b643 */
            RockMigrationHelper.AddBlockAttributeValue( "C9523ABF-7FFA-4F43-ACEE-EE20D5D2C9E5", "FD5A5196-4231-4CD6-9D34-71EF6C65A312", @"66972bff-42cd-49ab-9a7a-e1b9deca4ebf,b21dcd49-ac35-4b2b-9857-75213209b643" );

            #endregion Person Bio Summary Block
        }

        private void AddPersonProfileHomeLayout()
        {
            // Site:Rock RMS
            RockMigrationHelper.AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "PersonProfileHome", "Person Profile Home", "", Rock.SystemGuid.Layout.PERSON_PROFILE_HOME );

            #region Login Status Block

            // Add Block 
            //  Block Name: Login Status
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "04712F3D-9667-4901-A49D-4507573EF7AD".AsGuid(), "Login Status", "Login", @"", @"", 0, "7E9F8B26-BD00-464B-B209-F4D60FFD829C" );

            // Add Block Attribute Value from the v1 value
            // Block: Login Status
            // BlockType: Login Status
            // Category: Security
            // Block Location: Layout=Person Profile Home, Site=Rock RMS

            // Attribute: My Profile Page
            RockMigrationHelper.AddBlockAttributeValue( "7E9F8B26-BD00-464B-B209-F4D60FFD829C", "6CFDDF63-0B21-48FC-90AE-362C0E73420B", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );

            // Attribute: My Settings Page
            AddPersonPageBlockAttributeValue( "7E9F8B26-BD00-464B-B209-F4D60FFD829C", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0", Rock.SystemGuid.Layout.PERSON_DETAIL, "04712F3D-9667-4901-A49D-4507573EF7AD", "Login Status", "MySettingsPage" );

            // Attribute: Logged In Page List
            AddPersonPageBlockAttributeValue( "7E9F8B26-BD00-464B-B209-F4D60FFD829C", "1B0E8904-196B-433E-B1CC-937AD3CA5BF2", Rock.SystemGuid.Layout.PERSON_DETAIL, "04712F3D-9667-4901-A49D-4507573EF7AD", "Login Status", "LoggedInPageList" );

            #endregion Login Status Block

            #region Smart Search Block

            // Add Block 
            //  Block Name: Smart Search
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74".AsGuid(), "Smart Search", "Header", @"", @"", 0, "34BE95FD-1CC9-4A08-BB8C-15088CAAAB8B" );

            #endregion Smart Search Block

            #region Menu Block

            // Add Block 
            // Block Name: Menu
            // Page Name: -
            // Layout: Person Profile Home
            // Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Menu", "Navigation", @"", @"", 0, "6A62DC1C-1A29-4156-A431-B3355A3B84AB" );

            // Add Block Attribute Value from the v1 value
            // Block: Menu
            // BlockType: Page Menu
            // Category: CMS
            // Block Location: Layout=Person Profile Home, Site=Rock RMS

            // Attribute: CSS File
            AddPersonPageBlockAttributeValue( "6A62DC1C-1A29-4156-A431-B3355A3B84AB", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "CSSFile" );

            // Attribute: Include Current Parameters
            AddPersonPageBlockAttributeValue( "6A62DC1C-1A29-4156-A431-B3355A3B84AB", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "IncludeCurrentParameters" );

            // Attribute: Include Current QueryString
            AddPersonPageBlockAttributeValue( "6A62DC1C-1A29-4156-A431-B3355A3B84AB", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "IncludeCurrentQueryString" );

            // Attribute: Include Page List
            AddPersonPageBlockAttributeValue( "6A62DC1C-1A29-4156-A431-B3355A3B84AB", "0A49DABE-42EE-40E5-9E06-0E6530944865", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "IncludePageList" );

            // Attribute: Is Secondary Block
            AddPersonPageBlockAttributeValue( "6A62DC1C-1A29-4156-A431-B3355A3B84AB", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "IsSecondaryBlock" );

            // Attribute: Number of Levels
            AddPersonPageBlockAttributeValue( "6A62DC1C-1A29-4156-A431-B3355A3B84AB", "6C952052-BC79-41BA-8B88-AB8EA3E99648", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "NumberofLevels" );

            // Attribute: Root Page
            AddPersonPageBlockAttributeValue( "6A62DC1C-1A29-4156-A431-B3355A3B84AB", "41F1C42E-2395-4063-BD4F-031DF8D5B231", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "RootPage" );

            // Attribute: Template
            AddPersonPageBlockAttributeValue( "6A62DC1C-1A29-4156-A431-B3355A3B84AB", "1322186A-862A-4CF1-B349-28ECB67229BA", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "Template" );

            // Attribute: Enabled Lava Commands
            AddPersonPageBlockAttributeValue( "6A62DC1C-1A29-4156-A431-B3355A3B84AB", "EF10B2F9-93E5-426F-8D43-8C020224670F", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Menu", "EnabledLavaCommands" );

            #endregion Menu Block

            #region Family Navigation Block

            // Add Block 
            //  Block Name: Family Navigation
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "35D091FA-8311-42D1-83F7-3E67B9EE9675".AsGuid(), "Family Navigation", "ProfileNavigationLeft", @"", @"", 0, "B54D1924-BEE5-43C5-8F19-91B320DF99EC" );

            #endregion Family Navigation Block

            #region Sub Page Menu Block

            // Add Block 
            // Block Name: Sub Page Menu
            // Page Name: -
            // Layout: Person Profile Home
            // Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Sub Page Menu", "ProfileNavigation", @"", @"", 0, "CB964B6B-107E-44D4-8731-7A2D40A9F15B" );

            // Add Block Attribute Value
            // Block: Sub Page Menu
            // BlockType: Page Menu
            // Category: CMS
            // Block Location: Layout=Person Profile Home, Site=Rock RMS

            // Attribute: CSS File
            AddPersonPageBlockAttributeValue( "CB964B6B-107E-44D4-8731-7A2D40A9F15B", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "CSSFile" );

            // Attribute: Include Current Parameters
            AddPersonPageBlockAttributeValue( "CB964B6B-107E-44D4-8731-7A2D40A9F15B", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "IncludeCurrentParameters" );

            // Attribute: Include Current QueryString
            AddPersonPageBlockAttributeValue( "CB964B6B-107E-44D4-8731-7A2D40A9F15B", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "IncludeCurrentQueryString" );

            // Attribute: Include Page List
            AddPersonPageBlockAttributeValue( "CB964B6B-107E-44D4-8731-7A2D40A9F15B", "0A49DABE-42EE-40E5-9E06-0E6530944865", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "IncludePageList" );

            // Attribute: Is Secondary Block
            AddPersonPageBlockAttributeValue( "CB964B6B-107E-44D4-8731-7A2D40A9F15B", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "IsSecondaryBlock" );

            // Attribute: Number of Levels
            AddPersonPageBlockAttributeValue( "CB964B6B-107E-44D4-8731-7A2D40A9F15B", "6C952052-BC79-41BA-8B88-AB8EA3E99648", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "NumberofLevels" );

            // Attribute: Root Page
            RockMigrationHelper.AddBlockAttributeValue( "CB964B6B-107E-44D4-8731-7A2D40A9F15B", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"BF04BB7E-BE3A-4A38-A37C-386B55496303" );

            // Attribute: Template
            RockMigrationHelper.AddBlockAttributeValue( "CB964B6B-107E-44D4-8731-7A2D40A9F15B", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageListProfile.lava' %}" );

            // Attribute: Enabled Lava Commands
            AddPersonPageBlockAttributeValue( "CB964B6B-107E-44D4-8731-7A2D40A9F15B", "EF10B2F9-93E5-426F-8D43-8C020224670F", Rock.SystemGuid.Layout.PERSON_DETAIL, "CACB9D1A-A820-4587-986A-D66A69EE9948", "Sub Page Menu", "EnabledLavaCommands" );

            #endregion Sub Page Menu Block

            #region Person Edit Block

            // Add Block 
            //  Block Name: Person Edit
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8C94620B-0FC1-4C39-9474-1714546E7D9E".AsGuid(), "Person Edit", "ProfileNavigationRight", @"", @"", 0, "99AD76C5-0D75-44CB-95AA-7AED3E9FC860" );

            #endregion Person Edit Block

            #region Person Bio Block

            // Add Block 
            //  Block Name: Person Bio
            //  Page Name: -
            //  Layout: Person Profile Home
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "030CCDDC-8D43-40F8-A298-78B416F9E828".AsGuid(), "Person Bio", "Profile", @"", @"", 0, "1E6AF671-9C1A-4C6C-8156-36B6D7589F34" );

            // Add Block Attribute Value from the v1 value
            // Block: Person Bio
            // BlockType: Person Bio
            // Category: CRM > Person Detail
            // Block Location: Layout=Person Profile Home, Site=Rock RMS

            // Attribute: Badges
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "63F838EB-0B94-4F03-8E59-9AFAC8E72FAC", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "Badges" );

            // Attribute: Workflow Actions
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "B8419489-84A9-40AB-B85D-DD5E07255B17", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "WorkflowActions" );

            // Attribute: Actions
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "419196C7-46F9-4AB3-85AA-3ABEE55BD210", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "Actions" );

            // Attribute: Enable Impersonation
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "D77CB1E1-E4BC-429A-81C3-FB4AFC924618", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "EnableImpersonation" );

            // Attribute: ImpersonationStartPage
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "63964E1F-42E6-4E5E-BAD1-3DB5A971DB10", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "ImpersonationStartPage" );

            // Attribute: Business Detail Page
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "64695DF9-2196-4EB8-AD3B-AE4988FECD65", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "BusinessDetailPage" );

            // Attribute: Nameless Person Detail Page
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "8D355AC2-BB9D-468C-98BF-D0D426EE98EA", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "NamelessPersonDetailPage" );

            // Attribute: Display Country Code
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "B8CF7391-3AFD-4D71-ADB2-BB95714425EC", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "DisplayCountryCode" );

            // Attribute: Display Middle Name
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "9FC18F95-7857-4A4C-8A5F-A6AF5550D9D2", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "DisplayMiddleName" );

            // Attribute: CustomContent
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "1CD682C8-FEF3-420A-A925-E158194DCC69", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "CustomContent" );

            // Attribute: Allow Following
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "6E268729-EEED-48F9-A2EF-47188E37A538", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "AllowFollowing" );

            // Attribute: Display Graduation
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "0B501CDE-15F6-4882-8FF9-214676875503", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "DisplayGraduation" );

            // Attribute: Display Anniversary Date
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "2EA7A2C0-64D8-450B-AD06-FC085E724AD1", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "DisplayAnniversaryDate" );

            // Attribute: Social Media Category
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "53583EDF-1932-4A16-8602-8B083DE5FC8F", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "SocialMediaCategory" );

            // Attribute: Enable Call Origination
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "65241392-6A20-4EBA-8C54-0DA5E03124E4", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "EnableCallOrigination" );

            // Attribute: CommunicationPage 
            AddPersonPageBlockAttributeValue( "1E6AF671-9C1A-4C6C-8156-36B6D7589F34", "66CFDF24-8D19-4885-8C09-31DBE8C4126D", Rock.SystemGuid.Layout.PERSON_DETAIL, "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "CommunicationPage" );

            #endregion Person Bio Block

            #region Family Members Block

            // Add Block 
            // Block Name: Family Members
            // Page Name: -
            // Layout: Person Profile Home
            // Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F".AsGuid(), "Family Members", "Profile", @"", @"", 1, "0E6D894F-EF32-45F4-A189-32E05E5559CB" );

            // Add Block Attribute Value
            // Block: Family Members
            // BlockType: Group Members
            // Category: CRM > Person Detail
            // Block Location: Layout=Person Profile Home, Site=Rock RMS

            // Attribute: Group Type
            AddPersonPageBlockAttributeValue( "0E6D894F-EF32-45F4-A189-32E05E5559CB", "8F07815E-791F-4CC2-BB90-8A0552EA6697", Rock.SystemGuid.Layout.PERSON_DETAIL, "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "Family Members", "GroupType" );

            // Attribute: Auto Create Group
            AddPersonPageBlockAttributeValue( "0E6D894F-EF32-45F4-A189-32E05E5559CB", "BDB2514F-95C1-4054-B3EC-76C972048D56", Rock.SystemGuid.Layout.PERSON_DETAIL, "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "Family Members", "AutoCreateGroup" );

            // Attribute: Group Edit Page
            RockMigrationHelper.AddBlockAttributeValue( "0E6D894F-EF32-45F4-A189-32E05E5559CB", "FA36CC50-9FFC-4AC6-BC96-874C967EA44D", @"E9E1E5F2-467D-47CB-AF41-B4D9EF8B0B27" );

            // Attribute: Location Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "0E6D894F-EF32-45F4-A189-32E05E5559CB", "41D83CE7-8052-402E-9815-E4E0EDABD85E", @"4CE2A5DA-15F3-454C-8172-D146D938E203" );

            // Attribute: Show County
            AddPersonPageBlockAttributeValue( "0E6D894F-EF32-45F4-A189-32E05E5559CB", "31233DD5-DCB7-42FC-8151-517E32B29CBD", Rock.SystemGuid.Layout.PERSON_DETAIL, "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "Family Members", "ShowCounty" );

            // Attribute: Group Header Lava
            AddPersonPageBlockAttributeValue( "0E6D894F-EF32-45F4-A189-32E05E5559CB", "E620907F-A5BF-4AF0-8A6C-8618CFC0CBB4", Rock.SystemGuid.Layout.PERSON_DETAIL, "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "Family Members", "GroupHeaderLava" );

            // Attribute: Group Footer Lava
            AddPersonPageBlockAttributeValue( "0E6D894F-EF32-45F4-A189-32E05E5559CB", "036C6A27-B0A9-4219-A0E0-A368FF8FE8D2", Rock.SystemGuid.Layout.PERSON_DETAIL, "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "Family Members", "GroupFooterLava" );

            #endregion Family Members Block

            #region Badges Block

            // Add Block 
            // Block Name: Badges
            // Page Name: -
            // Layout: Person Profile Home
            // Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2412C653-9369-4772-955E-80EE8FA051E3".AsGuid(), "Badges", "BadgeBar", @"", @"", 0, "E82F7A07-FBD5-417B-A976-AC63CC53BF68" );

            // Add Block Attribute Value
            // Block: Badges
            // BlockType: Badges
            // Category: Obsidian > CRM > Person Detail
            // Block Location: Layout=Person Profile Home, Site=Rock RMS

            // Attribute: Top Left Badges
            AddPersonPageBlockAttributeValue( "E82F7A07-FBD5-417B-A976-AC63CC53BF68", "B22C1920-16D2-45B3-97FD-2EF00D38DD25", Rock.SystemGuid.Layout.PERSON_DETAIL, "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "Badges 1", "Badges" );

            // Attribute: Top Middle Badges
            AddPersonPageBlockAttributeValue( "E82F7A07-FBD5-417B-A976-AC63CC53BF68", "61540A33-0905-4ABE-A164-F5F5BA8524DD", Rock.SystemGuid.Layout.PERSON_DETAIL, "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "Badges 2", "Badges" );

            // Attribute: Top Right Badges. Needed to remove the assesments badge from this list if it exists because it will be displayed below.
            var sqlStringForBadges3Values = $@"
                DECLARE @BlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
                DECLARE @V1LayoutId INT = (SELECT [Id] FROM [Layout] WHERE [Guid] = '{Rock.SystemGuid.Layout.PERSON_DETAIL}')
                DECLARE @V1BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'FC8AF928-C4AF-40C7-A667-4B24390F03A1')
                DECLARE @V1BlockId INT = (SELECT [Id] FROM [Block] WHERE [LayoutId] = @V1LayoutId AND [Name] = 'Badges 3')
                DECLARE @AttributeKey VARCHAR(1000) = 'Badges'

                SELECT av.[Value]
                FROM [Attribute] a
                JOIN [AttributeValue] av on a.[Id] = av.[AttributeId]
                WHERE a.[EntityTypeId] = @BlockEntityTypeId
                    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND a.[EntityTypeQualifierValue] = @V1BlockTypeId
                    AND a.[Key] = @AttributeKey
                    AND av.EntityId = @V1BlockId";

            var personBioBadgesAttributeValue = SqlScalar( sqlStringForBadges3Values ).ToStringSafe().Replace( "CCE09793-89F6-4042-A98A-ED38392BCFCC", string.Empty );

            if ( personBioBadgesAttributeValue != null )
            {
                RockMigrationHelper.AddBlockAttributeValue( "E82F7A07-FBD5-417B-A976-AC63CC53BF68", "D87184D5-50A4-47D3-BA50-06D39FCCD328", personBioBadgesAttributeValue );
            }

            // Attribute: Bottom Right Badges
            RockMigrationHelper.AddBlockAttributeValue( "E82F7A07-FBD5-417B-A976-AC63CC53BF68", "B39D9047-7D28-42C5-A776-03D71E3A0F36", @"CCE09793-89F6-4042-A98A-ED38392BCFCC" );

            #endregion Badges Block

            #region Footer Content Block

            // Add Block 
            // Block Name: Footer Content
            // Page Name: -
            // Layout: Person Profile Home
            // Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "92A60013-B8D4-403A-BDFB-C3DA4D867B12".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Footer Content", "Footer", @"", @"", 0, "85A5921B-41CE-4E90-BAB4-24C51F8C1D23" );

            // Add/Update HtmlContent for Block: Footer Content
            var footerContentHtmlSql = $@"
                DECLARE @V1PersonDetailLayoutId INT = (SELECT [Id] FROM [Layout] WHERE [Guid] = 'F66758C6-3E3D-4598-AF4C-B317047B5987')
                DECLARE @V1BlockId INT = (SELECT [Id] FROM [Block] WHERE [LayoutId] = @V1PersonDetailLayoutId AND [Name] = 'Footer Content')

                SELECT * FROM [HtmlContent] WHERE [BlockId] = @V1BlockId";

            var footerContentHtmlValue = SqlScalar( footerContentHtmlSql ).ToStringSafe();
            RockMigrationHelper.UpdateHtmlContentBlock( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", footerContentHtmlValue, "C940149C-3C57-4949-9EB2-61E326CC2772" );

            // Add Block Attribute Value
            // Block: Footer Content
            // BlockType: HTML Content
            // Category: CMS
            // Block Location: Layout=Person Profile Home, Site=Rock RMS

            // Attribute: Start in Code Editor mode
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "0673E015-F8DD-4A52-B380-C758011331B2", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "UseCodeEditor" );

            // Attribute: Image Root Folder
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "ImageRootFolder" );

            // Attribute: User Specific Folders
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "UserSpecificFolders" );

            //   Attribute: Document Root Folder
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "DocumentRootFolder" );

            //   Attribute: Is Secondary Block
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "IsSecondaryBlock" );

            //   Attribute: Validate Markup
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "6E71FE26-5628-4DDA-BDBC-8E4D47BE72CD", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "ValidateMarkup" );

            //   Attribute: Context Name
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "466993F7-D838-447A-97E7-8BBDA6A57289", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "ContextName" );

            //   Attribute: Cache Duration
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "CacheDuration" );

            //   Attribute: Require Approval
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "RequireApproval" );

            //   Attribute: Enable Versioning
            AddPersonPageBlockAttributeValue( "85A5921B-41CE-4E90-BAB4-24C51F8C1D23", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", Rock.SystemGuid.Layout.PERSON_DETAIL, "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer Content", "SupportVersions" );

            #endregion Footer Content Block
        }

        private void AddNewProfileLayoutsDown()
        {
            RockMigrationHelper.DeleteLayout( "92A60013-B8D4-403A-BDFB-C3DA4D867B12" ); //  Layout: Person Profile Home, Site: Rock RMS
            RockMigrationHelper.DeleteLayout( "6AD84AFC-B3A1-4E30-B53B-C6E57B513839" ); //  Layout: Person Profile Detail, Site: Rock RMS
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

                -- Update the page references for the V1 PersonDetail layout
                UPDATE av
                SET av.Value = (SELECT [NewPageGuid] FROM #tmpPageIds WHERE CHARINDEX( CAST(#tmpPageIds.[OldPageGuid] AS VARCHAR(50)), av.[Value]) > 0)
                FROM [Layout] l
                JOIN [Block] b ON l.[Id] = b.[LayoutId]
                JOIN [BlockType] bt ON bt.[Id] = b.[BlockTypeId]
                JOIN [Attribute] a ON a.[EntityTypeQualifierColumn] = 'BlockTypeId' AND a.[EntityTypeQualifierValue] = bt.[Id] AND a.FieldTypeId = @PageReferenceFieldType_Id
                JOIN [AttributeValue] av ON av.[AttributeId] = a.[Id] AND av.EntityId = b.Id
                WHERE l.[Guid] = 'F66758C6-3E3D-4598-AF4C-B317047B5987'
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

        /// <summary>
        /// Adds the attribute value from the old block to the new one.
        /// </summary>
        /// <param name="v2blockGuid">The v2block unique identifier.</param>
        /// <param name="v2attributeGuid">The v2attribute unique identifier.</param>
        /// <param name="v1LayoutGuid">The v1 layout unique identifier.</param>
        /// <param name="v1BlockTypeGuid">The v1 block type unique identifier.</param>
        /// <param name="v1BlockName">Name of the v1 block.</param>
        /// <param name="v1AttributeKey">The v1 attribute key.</param>
        private void AddPersonPageBlockAttributeValue( string v2blockGuid, string v2attributeGuid, string v1LayoutGuid, string v1BlockTypeGuid, string v1BlockName, string v1AttributeKey )
        {
            var sqlString = $@"
                DECLARE @BlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
                DECLARE @V1LayoutId INT = (SELECT [Id] FROM [Layout] WHERE [Guid] = '{v1LayoutGuid}')
                DECLARE @V1BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{v1BlockTypeGuid}')
                DECLARE @V1BlockId INT = (SELECT [Id] FROM [Block] WHERE [LayoutId] = @V1LayoutId AND [Name] = '{v1BlockName}')
                DECLARE @AttributeKey VARCHAR(1000) = '{v1AttributeKey}'

                SELECT av.[Value]
                FROM [Attribute] a
                JOIN [AttributeValue] av on a.[Id] = av.[AttributeId]
                WHERE a.[EntityTypeId] = @BlockEntityTypeId
	                AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
	                AND a.[EntityTypeQualifierValue] = @V1BlockTypeId
	                AND a.[Key] = @AttributeKey
	                AND av.EntityId = @V1BlockId";

            var attributeValueToInsert = SqlScalar( sqlString ).ToStringSafe();

            RockMigrationHelper.AddBlockAttributeValue( v2blockGuid, v2attributeGuid, attributeValueToInsert );
        }
    }
}

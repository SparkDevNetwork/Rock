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
    public partial class PageBuilder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* JE: Add Universal Search Re-Index */
            Sql( @"INSERT INTO [ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid] )
    VALUES (
         0
        ,1
        ,'Universal Search Re-Index'
        ,'Re-indexes the selected entity types in Universal Search.'
        ,'Rock.Jobs.IndexEntities'
        ,'0 0 5 1/1 * ? *'
        ,1
        ,'1814F95D-5F82-ECB4-45B8-334CC2C12AA7')" );


            /* MP: PageBuilder Migration */
            // Change the PageMap Page to use 'LeftSidebar' layout, and move pagetree block to 'Sidebar1' zone
            Sql( @"DECLARE @leftSidebarLayoutId INT = (
		SELECT TOP 1 Id
		FROM Layout
		WHERE [FileName] = 'LeftSidebar'
			AND SiteId = 1
		)
	,@pageMapPageId INT = (
		SELECT TOP 1 Id
		FROM [Page]
		WHERE [Guid] = 'EC7A06CD-AAB5-4455-962E-B4043EA2440E'
		)
	,@pageMapBlockId INT = (
		SELECT TOP 1 Id
		FROM [Block]
		WHERE [Guid] = '68192536-3CE8-433B-9DF8-A895EF037FD7'
		)

UPDATE [Page]
SET LayoutId = @leftSidebarLayoutId
WHERE Id = @pageMapPageId
	AND LayoutId != @leftSidebarLayoutId

UPDATE [Block] SET [Zone] = 'Sidebar1' 
WHERE Id = @pageMapBlockId and [Zone] != 'Sidebar1'" );

            RockMigrationHelper.UpdateBlockType( "Page/Zone Blocks Editor", "Edit the Blocks for a Zone on a specific page/layout.", "~/Blocks/Cms/PageZoneBlocksEditor.ascx", "CMS", "D2AC406D-4CA3-4239-9388-870335805A92" );

            // Add Block to Page: Page Map, Site: Rock RMS
            RockMigrationHelper.AddBlock( "EC7A06CD-AAB5-4455-962E-B4043EA2440E", "", "C7988C3E-822D-4E73-882E-9B7684398BAA", "Page Properties", "Main", @"", @"", 0, "5B083BB2-2B6F-4883-BA62-270AC3F55ACA" );
            // Add Block to Page: Page Map, Site: Rock RMS
            RockMigrationHelper.AddBlock( "EC7A06CD-AAB5-4455-962E-B4043EA2440E", "", "D2AC406D-4CA3-4239-9388-870335805A92", "Page/Zone Blocks Editor", "Main", @"", @"", 1, "CF102D05-6534-4A99-B593-80194EE92987" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '5B083BB2-2B6F-4883-BA62-270AC3F55ACA'" );  // Page: Page Map,  Zone: Main,  Block: Page Properties
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'CF102D05-6534-4A99-B593-80194EE92987'" );  // Page: Page Map,  Zone: Main,  Block: Page/Zone Blocks Editor

            // Attrib for BlockType: Page Properties:Enable Full Edit Mode
            RockMigrationHelper.AddBlockTypeAttribute( "C7988C3E-822D-4E73-882E-9B7684398BAA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Full Edit Mode", "EnableFullEditMode", "", "Have the block initially show a readonly summary view, in a panel, with Edit and Delete buttons. Also include Save and Cancel buttons.", 0, @"False", "F458BBC5-69E8-496A-A7FD-09BCA6D4F5CA" );
            // Attrib Value for Block:Page Properties, Attribute:Enable Full Edit Mode Page: Page Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5B083BB2-2B6F-4883-BA62-270AC3F55ACA", "F458BBC5-69E8-496A-A7FD-09BCA6D4F5CA", @"True" );

            /* MP: BatchID on TransactionList */
            // Update the Transaction List block name to 'Transaction List' instead of 'Financial Transaction'
            Sql( @"update [Block] set [Name] = 'Transaction List' where [Guid] = 'B447AB11-3A19-4527-921A-2266A6B4E181'" );

            // Attrib for BlockType: Transaction List:Batch Page
            RockMigrationHelper.AddBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Batch Page", "BatchPage", "", "", 6, @"", "683C4694-4FAC-4AFC-8987-F062EE491BC3" );

            // Attrib Value for Block:Transaction List, Attribute:Batch Page Page: Transactions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B447AB11-3A19-4527-921A-2266A6B4E181", "683C4694-4FAC-4AFC-8987-F062EE491BC3", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );


            /* MP: Business Contribution Blocks */
            /* Catch up on new BlockTypes that haven't been in a migration yet */
            RockMigrationHelper.UpdateBlockType( "Page/Zone Blocks Editor", "Edit the Blocks for a Zone on a specific page/layout.", "~/Blocks/Cms/PageZoneBlocksEditor.ascx", "CMS", "1063D63E-8136-479A-BA96-57E93E0194B5" );
            RockMigrationHelper.UpdateBlockType( "Campus Schedule Context Setter", "Block that can be used to set the default campus context and/or schedule for the site or page.", "~/Blocks/Core/CampusScheduleContextSetter.ascx", "Core", "A0405364-C722-495B-879C-C57B5BC5E213" );
            RockMigrationHelper.UpdateBlockType( "Date Range Context Setter", "Block that can be used to set a user specific date range preference.", "~/Blocks/Core/DateRangeContextSetter.ascx", "Core", "ABC4A04E-6FA8-4817-8113-A653251A16B3" );
            RockMigrationHelper.UpdateBlockType( "Person Context Setter", "Block that can be used to set the default person context for the site.", "~/Blocks/Core/PersonContextSetter.ascx", "Core", "EA33088D-7B18-46A7-ADFA-B9DA9512B4A4" );
            RockMigrationHelper.UpdateBlockType( "Person Attribute Forms", "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "~/Blocks/CRM/PersonAttributeForms.ascx", "CRM", "5464F6B3-E0E4-4F9F-8FBA-44A18DB83F44" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance Active List", "Block to display active Registration Instances.", "~/Blocks/Event/RegistrationInstanceActiveList.ascx", "Event", "CFE8CAFA-587B-4EF2-A457-18047AC6BA39" );
            RockMigrationHelper.UpdateBlockType( "Sample Linq Report", "Sample Block that executes a Linq statment and displays the result (if any).", "~/Blocks/Reporting/SampleLinqReport.ascx", "Reporting", "E98E0584-0D87-4DC6-9085-DC93F17AFB7F" );
            RockMigrationHelper.UpdateBlockType( "Logout", "This block logs the current person out.", "~/Blocks/Security/Logout.ascx", "Security", "CCB87054-8AA3-4F44-AA48-19BD028C4190" );

            /* Catch up new BlockType Attributes that haven't been in a migration yet */
            // Attrib for BlockType: HTML Content:Quick Edit
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Quick Edit", "QuickEdit", "", "Allow quick editing of HTML contents.", 11, @"", "2034DE48-4643-45A3-9FF7-9539F6731EFF" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: Rock Solid Church Sample Data:Enable Giving
            RockMigrationHelper.UpdateBlockTypeAttribute( "A42E0031-B2B9-403A-845B-9C968D7716A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Giving", "EnableGiving", "", "If true, the giving data will be loaded otherwise it will be skipped.", 4, @"True", "EF391043-0B8C-40E7-9153-47332E626503" );
            // Attrib for BlockType: Group Members:Auto Create Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Auto Create Group", "AutoCreateGroup", "", "If person doesn't belong to a group of this type, should one be created for them (default is Yes).", 1, @"True", "26018994-D9DD-4F3A-BE69-719AF5EB866F" );
            // Attrib for BlockType: Person Bio:Allow Following
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Following", "AllowFollowing", "", "Should people be able to follow a person by selecting the star on the person's photo?", 7, @"True", "D52AD5E1-41B6-45D2-8979-4C04619A86EE" );
            // Attrib for BlockType: Notes:Note Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Note Types", "NoteTypes", "", "Optional list of note types to limit display to", 12, @"", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4" );
            // Attrib for BlockType: Pledge List:Show Last Modified Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Modified Filter", "ShowLastModifiedFilter", "", "Allows last modified filter to be hidden.", 3, @"True", "550E6B86-98BF-4DA7-9B54-634ADE0EE466" );
            // Attrib for BlockType: Pledge List:Limit Pledges To Current Person
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit Pledges To Current Person", "LimitPledgesToCurrentPerson", "", "Limit the results to pledges for the current person.", 4, @"False", "6A056518-3E38-4E78-AF6F-16D5C23A057D" );
            // Attrib for BlockType: Pledge List:Show Last Modified Date Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Modified Date Column", "ShowLastModifiedDateColumn", "", "Allows the Last Modified Date column to be hidden.", 2, @"True", "B27608E5-E5BF-4AC4-8C7E-C2A26456480B" );
            // Attrib for BlockType: Pledge List:Show Account Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Filter", "ShowAccountFilter", "", "Allows account filter to be hidden.", 1, @"True", "B16A3F35-C8A4-47B3-BA7A-E20098E7B028" );
            // Attrib for BlockType: Pledge List:Show Account Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Column", "ShowAccountColumn", "", "Allows the account column to be hidden.", 1, @"True", "63A83579-C73A-4387-B317-D9852F6647F3" );
            // Attrib for BlockType: Pledge List:Show Person Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Person Filter", "ShowPersonFilter", "", "Allows person filter to be hidden.", 0, @"True", "807B41A4-4286-434C-918A-FE3942A75F7B" );
            // Attrib for BlockType: Pledge List:Show Date Range Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Range Filter", "ShowDateRangeFilter", "", "Allows date range filter to be hidden.", 2, @"True", "0049EC69-9814-4322-833F-BD82F92C64E9" );
            // Attrib for BlockType: Person Search:Show Birthdate
            RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Birthdate", "ShowBirthdate", "", "Should a birthdate column be displayed?", 1, @"False", "76EF16F4-7E1D-49C5-9DC9-0A9976B801BB" );
            
            // Attrib for BlockType: Transaction Entry:Enable Business Giving
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Business Giving", "EnableBusinessGiving", "", "Should the option to give as as a business be displayed", 31, @"True", "16E84C69-7E88-440C-930F-2AA03BA4B8B7" );
            // Attrib for BlockType: Transaction Entry:Comment Entry Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Comment Entry Label", "CommentEntryLabel", "", "The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).", 30, @"Comment", "7C1C46F9-6713-4825-976E-6859702EDBAA" );
            // Attrib for BlockType: Transaction Entry:Enable Comment Entry
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Comment Entry", "EnableCommentEntry", "", "Allows the guest to enter the the value that's put into the comment field (will be appended to the 'Payment Comment' setting)", 29, @"False", "12FDEC08-5257-4E67-B486-480AAFC43E6B" );
            // Attrib for BlockType: Contribution Statement Lava:Excluded Currency Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF986B72-ADD9-4E05-971F-1DE4EBED8667", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Excluded Currency Types", "ExcludedCurrencyTypes", "", "Select the currency types you would like to excluded.", 4, @"", "244EDB88-10B0-47C0-BC1A-ECEDAE70AA70" );


            /* Migration for Business Page Contribution Blocks */
            // Add Block to Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "", "22BF5B51-6511-4D31-8A48-4978A454C386", "Contribution Statement List Lava", "Main", @"", @"            </div>
		</div>
	</div>
</div>", 3, "13EF2086-37D4-42FD-B629-6D4292495BC8" );
            // Add Block to Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Transaction Links", "Main", @"<div class='col-md-4'>
<div class=""panel panel-block""><div class=""panel-body"">", @"", 2, "84F800D3-C32E-4A16-9F84-081F8CB4DCBF" );
            // Add Block to Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "", "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "Pledge List", "Main", @"", @"", 4, "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F" );
            // Add Block to Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "", "C4191011-0391-43DF-9A9D-BE4987C679A4", "Bank Account List", "Main", @"", @"", 7, "21D15CAB-EC52-4DEB-87DB-38F6C393FBE7" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '77AB2D30-FCBE-45E9-9757-401AE2676A7F'" );  // Page: Business Detail,  Zone: Main,  Block: Business Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '5322C1C2-0387-4752-9E87-67700F485C5E'" );  // Page: Business Detail,  Zone: Main,  Block: Transaction Yearly Summary Lava
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '84F800D3-C32E-4A16-9F84-081F8CB4DCBF'" );  // Page: Business Detail,  Zone: Main,  Block: Transaction Links
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '13EF2086-37D4-42FD-B629-6D4292495BC8'" );  // Page: Business Detail,  Zone: Main,  Block: Contribution Statement List Lava
            Sql( @"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = '39A2DA08-1995-4A39-A6AF-5F8B8DE7372F'" );  // Page: Business Detail,  Zone: Main,  Block: Pledge List
            Sql( @"UPDATE [Block] SET [Order] = 5 WHERE [Guid] = '91850A29-BB1A-4E92-A798-DE7D6E09E671'" );  // Page: Business Detail,  Zone: Main,  Block: Scheduled Transaction List
            Sql( @"UPDATE [Block] SET [Order] = 6 WHERE [Guid] = '0A567E24-80BE-4906-B303-77D1A5FB89DE'" );  // Page: Business Detail,  Zone: Main,  Block: Transaction List
            Sql( @"UPDATE [Block] SET [Order] = 7 WHERE [Guid] = '21D15CAB-EC52-4DEB-87DB-38F6C393FBE7'" );  // Page: Business Detail,  Zone: Main,  Block: Bank Account List
            // Add/Update HtmlContent for Block: Transaction Links
            RockMigrationHelper.UpdateHtmlContentBlock( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", @"<a href=""../../AddTransaction?Person={{ Context.Person.UrlEncodedKey }}"" class=""btn btn-default btn-block"">Add One-time Gift</a>
        <a href=""../../AddTransaction?Person={{ Context.Person.UrlEncodedKey }}"" class=""btn btn-default btn-block"">New Scheduled Transaction</a>", "5FC52F76-4146-45A6-88EA-9462A1271583" );
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Use Person Context Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "13EF2086-37D4-42FD-B629-6D4292495BC8", "F37EB885-416A-4B70-B48E-8A25557C7B12", @"True" );
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Entity Type Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "13EF2086-37D4-42FD-B629-6D4292495BC8", "F9A168F1-3E59-4C5F-8019-7B17D00B94C9", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Lava Template Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "13EF2086-37D4-42FD-B629-6D4292495BC8", "7B554631-3CD5-40C4-8E67-ECED56D4D7C1", @"{% assign yearCount = StatementYears | Size %}
{% if yearCount > 0 %}<hr /><p class=""margin-t-md"">
    <strong><i class='fa fa-file-text-o'></i> Available Contribution Statements</strong>
</p>



{% assign currentYear = 'Now' | Date:'yyyy' %}

<div>
{% for statementyear in StatementYears %}
    {% if currentYear == statementyear.Year %}
        <a href=""{{ DetailPage }}?PersonGuid={{ PersonGuid }}&StatementYear={{ statementyear.Year }}"" class=""btn btn-sm btn-default"">{{ statementyear.Year }} <small>YTD</small></a>
    {% else %}
        <a href=""{{ DetailPage }}?PersonGuid={{ PersonGuid }}&StatementYear={{ statementyear.Year }}"" class=""btn btn-sm btn-default"">{{ statementyear.Year }}</a>
    {% endif %}
{% endfor %}
</div>
{% endif %}" );
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Accounts Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "13EF2086-37D4-42FD-B629-6D4292495BC8", "AC1EF7F3-7B06-4978-84DD-B38025FC2E7B", @"" );
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Max Years To Display Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "13EF2086-37D4-42FD-B629-6D4292495BC8", "346384B5-1ECE-4949-BFF4-712E1FAA4335", @"3" );
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Detail Page Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "13EF2086-37D4-42FD-B629-6D4292495BC8", "5B439A86-D2AD-4223-8D1E-A50FF883D7C2", @"98ebadaf-cca9-4893-9dd3-d8201d8bd7fa" );
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Enable Debug Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "13EF2086-37D4-42FD-B629-6D4292495BC8", "D8865D7D-F3FA-48FF-8339-8E2649D8CC29", @"False" );
            // Attrib Value for Block:Transaction Links, Attribute:Quick Edit Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "2034DE48-4643-45A3-9FF7-9539F6731EFF", @"" );
            // Attrib Value for Block:Transaction Links, Attribute:Enabled Lava Commands Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"" );
            // Attrib Value for Block:Transaction Links, Attribute:Cache Duration Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Transaction Links, Attribute:Require Approval Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Transaction Links, Attribute:Enable Versioning Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Transaction Links, Attribute:Context Parameter Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            // Attrib Value for Block:Transaction Links, Attribute:Context Name Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );
            // Attrib Value for Block:Transaction Links, Attribute:Use Code Editor Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Transaction Links, Attribute:Image Root Folder Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Transaction Links, Attribute:User Specific Folders Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Transaction Links, Attribute:Document Root Folder Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Transaction Links, Attribute:Enable Debug Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );
            // Attrib Value for Block:Pledge List, Attribute:Entity Type Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "E9245CFD-4B11-4CE2-A120-BB3AC47C0974", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );
            // Attrib Value for Block:Pledge List, Attribute:Detail Page Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "3E26B7DF-7A7F-4829-987F-47304C0F845E", @"ef7aa296-ca69-49bc-a28b-901a8aaa9466" );
            // Attrib Value for Block:Pledge List, Attribute:Show Last Modified Filter Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "550E6B86-98BF-4DA7-9B54-634ADE0EE466", @"True" );
            // Attrib Value for Block:Pledge List, Attribute:Limit Pledges To Current Person Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "6A056518-3E38-4E78-AF6F-16D5C23A057D", @"False" );
            // Attrib Value for Block:Pledge List, Attribute:Show Last Modified Date Column Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "B27608E5-E5BF-4AC4-8C7E-C2A26456480B", @"True" );
            // Attrib Value for Block:Pledge List, Attribute:Show Account Filter Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "B16A3F35-C8A4-47B3-BA7A-E20098E7B028", @"True" );
            // Attrib Value for Block:Pledge List, Attribute:Show Account Column Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "63A83579-C73A-4387-B317-D9852F6647F3", @"True" );
            // Attrib Value for Block:Pledge List, Attribute:Show Person Filter Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "807B41A4-4286-434C-918A-FE3942A75F7B", @"True" );
            // Attrib Value for Block:Pledge List, Attribute:Show Date Range Filter Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "0049EC69-9814-4322-833F-BD82F92C64E9", @"True" );
            // Attrib Value for Block:Pledge List, Attribute:Show Group Column Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "F2476138-7C16-404C-A4B6-600E39602601", @"False" );
            // Add/Update PageContext for Page:Business Detail, Entity: Rock.Model.Person, Parameter: businessId
            RockMigrationHelper.UpdatePageContext( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "Rock.Model.Person", "businessId", "A3A76866-EF29-4E75-8D34-BCB68EE2474D" );

            Sql( @"update [Block] set PreHtml = '<div class=''row''>
<div class=''col-md-8''>', PostHtml = '</div>' where [Guid] = '5322C1C2-0387-4752-9E87-67700F485C5E'" );


            /* MP: Interaction DateTime Index */
            Sql( @"IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_InteractionDateTime'
			AND object_id = OBJECT_ID('Interaction')
		)
BEGIN
	DROP INDEX [IX_InteractionDateTime] ON [dbo].[Interaction]
END

CREATE NONCLUSTERED INDEX [IX_InteractionDateTime] ON [dbo].[Interaction] ([InteractionDateTime] ASC) INCLUDE (
	[InteractionComponentId]
	,[PersonAliasId]
	)" );

            /* MP: CKEditor Cleanup */
            Sql( @"UPDATE [Block]
SET [Name] = 'HtmlEditor FileBrowser'
WHERE [Guid] = '74BD3FE7-E758-45AE-B598-F2A1AE133B7D'
UPDATE [Block]
SET [Name] = 'HtmlEditor MergeField'
WHERE [Guid] = 'A795CF36-E6AD-4B1A-9076-4435B5FF0267'
UPDATE [Page]
SET [InternalName] = 'HtmlEditor RockFileBrowser Plugin Frame'
 ,[PageTitle] = 'HtmlEditor RockFileBrowser Plugin Frame'
 ,[BrowserTitle] = 'HtmlEditor RockFileBrowser Plugin Frame'
WHERE [Guid] = '4A4995CA-24F6-4D33-B861-A24274F53AA6'
UPDATE [Page]
SET [InternalName] = 'HtmlEditor RockMergeField Plugin Frame'
 ,[PageTitle] = 'HtmlEditor RockMergeField Plugin Frame'
 ,[BrowserTitle] = 'HtmlEditor RockMergeField Plugin Frame'
WHERE [Guid] = '1FC09F0D-72F2-44E6-9D16-2884F9AF33DD'
DELETE
FROM [PageRoute]
WHERE [Route] = 'ckeditorplugins/RockFileBrowser'
 AND PageId IN (
 SELECT Id
 FROM page
 WHERE [Guid] = '4A4995CA-24F6-4D33-B861-A24274F53AA6'
 )
DELETE
FROM [PageRoute]
WHERE [Route] = 'ckeditorplugins/RockMergeField'
 AND PageId IN (
 SELECT Id
 FROM page
 WHERE [Guid] = '1FC09F0D-72F2-44E6-9D16-2884F9AF33DD'
 )" );

            /* JE: Update Page Map Title to Pages */
            Sql( @"UPDATE [Page]
SET 
	[BrowserTitle] = 'Pages',
	[InternalName] = 'Pages',
	[PageTitle] = 'Pages'
WHERE 
	[Guid] = 'EC7A06CD-AAB5-4455-962E-B4043EA2440E'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            /* MP: Business Contribution Blocks Down */
            // Remove Block: Bank Account List, from Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "21D15CAB-EC52-4DEB-87DB-38F6C393FBE7" );
            // Remove Block: Pledge List, from Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F" );
            // Remove Block: Transaction Links, from Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF" );
            // Remove Block: Contribution Statement List Lava, from Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "13EF2086-37D4-42FD-B629-6D4292495BC8" );

            // Delete PageContext for Page:Business Detail, Entity: Rock.Model.Person, Parameter: businessId
            RockMigrationHelper.DeletePageContext( "A3A76866-EF29-4E75-8D34-BCB68EE2474D" );


            /* MP:PageBuilder Down */
            // delete PageProperties block from PageMap page
            RockMigrationHelper.DeleteBlock( "5B083BB2-2B6F-4883-BA62-270AC3F55ACA" );

            // delete Page/Zone blocks editor from PageMap page
            RockMigrationHelper.DeleteBlock( "CF102D05-6534-4A99-B593-80194EE92987" );

            // Change PageMap back to just a FullWidth layout with a single block
            Sql( @"DECLARE @fullWidthLayoutId INT = (
		SELECT TOP 1 Id
		FROM Layout
		WHERE [FileName] = 'FullWidth'
			AND SiteId = 1
		)
	,@pageMapPageId INT = (
		SELECT TOP 1 Id
		FROM [Page]
		WHERE [Guid] = 'EC7A06CD-AAB5-4455-962E-B4043EA2440E'
		)
	,@pageMapBlockId INT = (
		SELECT TOP 1 Id
		FROM [Block]
		WHERE [Guid] = '68192536-3CE8-433B-9DF8-A895EF037FD7'
		)

UPDATE [Page]
SET LayoutId = @fullWidthLayoutId
WHERE Id = @pageMapPageId
	AND LayoutId != @fullWidthLayoutId

UPDATE [Block] SET [Zone] = 'Main' 
WHERE Id = @pageMapBlockId and [Zone] != 'Main'" );

            // delete the new Page/Zone Blocks Editor blocktype
            RockMigrationHelper.DeleteBlockType( "D2AC406D-4CA3-4239-9388-870335805A92" );
        }
    }
}

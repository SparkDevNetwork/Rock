using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bemaservices.MinistrySafe.Migrations
{
    [MigrationNumber( 7, "1.9.4" )]
    public partial class BackgroundChecks : Migration
    {
        public override void Up()
        {
            UpdateTerminology();

            UpdateMinistrySafePage();

            AddBackgroundCheckPage();

            BackgroundCheckDefinedTypeAttributes();

            BackgroundCheckWorkflow();

            BackgroundCheckBadge();

            BackgroundCheckReportPage();

        }

        private void BackgroundCheckDefinedTypeAttributes()
        {
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "3EE69CBC-35CE-4496-88CC-8327A447603F", "MinistrySafe Package Price", "MinistrySafePackagePrice", "", 1044, "", "64066AB0-CFAB-4F2F-BFF9-5919B0961345" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "MinistrySafe User Type", "MinistrySafeUserType", "", 1046, "", "7B18B548-E5EE-4973-8E9D-F0BF6F7A9F4D" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MinistrySafe Package Code", "MinistrySafePackageCode", "", 1043, "", "875B25A2-B9EA-4D29-AE3E-30D93216FBA5" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MinistrySafe Package Name", "MinistrySafePackageName", "", 1045, "", "BDF854B3-D62A-456B-A267-5DB95793CEC7" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "MinistrySafe Package Level", "MinistrySafePackageLevel", "", 1042, "", "7498F509-D8B5-49B5-8431-B3F36DB2BF06" );

            RockMigrationHelper.AddAttributeQualifier( "7B18B548-E5EE-4973-8E9D-F0BF6F7A9F4D", "allowmultiple", "False", "41DA4A9D-D39C-4BD7-9B1C-B8567BED5FA8" );
            RockMigrationHelper.AddAttributeQualifier( "7B18B548-E5EE-4973-8E9D-F0BF6F7A9F4D", "definedtype", "86", "90DD7CAA-DB3B-4FF0-9FD7-DD5DB1895791" );
            RockMigrationHelper.AddAttributeQualifier( "7B18B548-E5EE-4973-8E9D-F0BF6F7A9F4D", "displaydescription", "False", "15BDD6C9-25D8-4BEF-8B11-21DE63FBA632" );
            RockMigrationHelper.AddAttributeQualifier( "7B18B548-E5EE-4973-8E9D-F0BF6F7A9F4D", "enhancedselection", "False", "74A6B839-C25D-4C8B-A620-E5BB082E0198" );
            RockMigrationHelper.AddAttributeQualifier( "7B18B548-E5EE-4973-8E9D-F0BF6F7A9F4D", "includeInactive", "False", "9BD5F440-9885-44D1-8394-7B24F5EFB3A4" );
            RockMigrationHelper.AddAttributeQualifier( "875B25A2-B9EA-4D29-AE3E-30D93216FBA5", "ispassword", "False", "237D93E3-8DFD-40EF-B67E-5F81C134C25C" );
            RockMigrationHelper.AddAttributeQualifier( "875B25A2-B9EA-4D29-AE3E-30D93216FBA5", "maxcharacters", "", "28C80A35-50B8-4FCC-98E2-3AA71C1E3B19" );
            RockMigrationHelper.AddAttributeQualifier( "875B25A2-B9EA-4D29-AE3E-30D93216FBA5", "showcountdown", "False", "D2CB9558-246D-4BD2-B27A-A30E02EF9C51" );
            RockMigrationHelper.AddAttributeQualifier( "BDF854B3-D62A-456B-A267-5DB95793CEC7", "ispassword", "False", "3F82C7EA-1370-427E-AAEB-3A766EF9B576" );
            RockMigrationHelper.AddAttributeQualifier( "BDF854B3-D62A-456B-A267-5DB95793CEC7", "maxcharacters", "", "5B8F5DB4-B21C-4E3A-8B9F-43DF32E50592" );
            RockMigrationHelper.AddAttributeQualifier( "BDF854B3-D62A-456B-A267-5DB95793CEC7", "showcountdown", "False", "4AD120D7-81F8-41CB-B1E6-E0AD0BA8CD43" );
        }

        private void BackgroundCheckReportPage()
        {
            // Page: Background Check Report
            RockMigrationHelper.AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Background Check Report", "", "85198F00-A00F-40BD-92E3-DF9AFF2CD196", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Dynamic Data", "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "~/Blocks/Reporting/DynamicData.ascx", "Reporting", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" );
            // Add Block to Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "85198F00-A00F-40BD-92E3-DF9AFF2CD196", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Dynamic Data", "Main", "", "", 0, "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E" );
            // Attrib for BlockType: Dynamic Data:Update Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Update Page", "UpdatePage", "", "If True, provides fields for updating the parent page's Name and Description", 0, @"True", "230EDFE8-33CA-478D-8C9A-572323AF3466" );
            // Attrib for BlockType: Dynamic Data:Query Params
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Query Params", "QueryParams", "", "Parameters to pass to query", 0, @"", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012" );
            // Attrib for BlockType: Dynamic Data:Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Columns", "Columns", "", "The columns to hide or show", 0, @"", "90B0E6AF-B2F4-4397-953B-737A40D4023B" );
            // Attrib for BlockType: Dynamic Data:Query
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Query", "Query", "", "The query to execute. Note that if you are providing SQL you can add items from the query string using Lava like {{ QueryParmName }}.", 0, @"", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356" );
            // Attrib for BlockType: Dynamic Data:Url Mask
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Url Mask", "UrlMask", "", "The Url to redirect to when a row is clicked", 0, @"", "B9163A35-E09C-466D-8A2D-4ED81DF0114C" );
            // Attrib for BlockType: Dynamic Data:Show Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Columns", "ShowColumns", "", "Should the 'Columns' specified below be the only ones shown (vs. the only ones hidden)", 0, @"False", "202A82BF-7772-481C-8419-600012607972" );
            // Attrib for BlockType: Dynamic Data:Merge Fields
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Merge Fields", "MergeFields", "", "Any fields to make available as merge fields for any new communications", 0, @"", "8EB882CE-5BB1-4844-9C28-10190903EECD" );
            // Attrib for BlockType: Dynamic Data:Formatted Output
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Formatted Output", "FormattedOutput", "", "Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}", 0, @"", "6A233402-446C-47E9-94A5-6A247C29BC21" );
            // Attrib for BlockType: Dynamic Data:Person Report
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Person Report", "PersonReport", "", "Is this report a list of people?", 0, @"False", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C" );
            // Attrib for BlockType: Dynamic Data:Communication Recipient Person Id Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Communication Recipient Person Id Columns", "CommunicationRecipientPersonIdColumns", "", "Columns that contain a communication recipient person id.", 0, @"", "75DDB977-9E71-44E8-924B-27134659D3A4" );
            // Attrib for BlockType: Dynamic Data:Show Excel Export
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Excel Export", "ShowExcelExport", "", "Show Export to Excel button in grid footer?", 0, @"True", "E11B57E5-EC7D-4C42-9ADA-37594D71F145" );
            // Attrib for BlockType: Dynamic Data:Show Communicate
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Communicate", "ShowCommunicate", "", "Show Communicate button in grid footer?", 0, @"True", "5B2C115A-C187-4AB3-93AE-7010644B39DA" );
            // Attrib for BlockType: Dynamic Data:Show Merge Person
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Merge Person", "ShowMergePerson", "", "Show Merge Person button in grid footer?", 0, @"True", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E" );
            // Attrib for BlockType: Dynamic Data:Show Bulk Update
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Bulk Update", "ShowBulkUpdate", "", "Show Bulk Update button in grid footer?", 0, @"True", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78" );
            // Attrib for BlockType: Dynamic Data:Stored Procedure
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Stored Procedure", "StoredProcedure", "", "Is the query a stored procedure?", 0, @"False", "A4439703-5432-489A-9C14-155903D6A43E" );
            // Attrib for BlockType: Dynamic Data:Show Merge Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Merge Template", "ShowMergeTemplate", "", "Show Export to Merge Template button in grid footer?", 0, @"True", "6697B0A2-C8FE-497A-B5B4-A9D459474338" );
            // Attrib for BlockType: Dynamic Data:Paneled Grid
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Paneled Grid", "PaneledGrid", "", "Add the 'grid-panel' class to the grid to allow it to fit nicely in a block.", 0, @"False", "5449CB61-2DFC-4B55-A697-38F1C2AF128B" );
            // Attrib for BlockType: Dynamic Data:Show Grid Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grid Filter", "ShowGridFilter", "", "Show filtering controls that are dynamically generated to match the columns of the dynamic data.", 0, @"True", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C" );
            // Attrib for BlockType: Dynamic Data:Timeout
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Timeout", "Timeout", "", "The amount of time in xxx to allow the query to run before timing out.", 0, @"30", "BEEE38DD-2791-4242-84B6-0495904143CC" );
            // Attrib for BlockType: Dynamic Data:Page Title Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Page Title Lava", "PageTitleLava", "", "Optional Lava for setting the page title. If nothing is provided then the page's title will be used.", 0, @"", "3F4BA170-F5C5-405E-976F-0AFBB8855FE8" );
            // Attrib for BlockType: Dynamic Data:Encrypted Fields
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Encrypted Fields", "EncryptedFields", "", "Any fields that need to be decrypted before displaying their value", 0, @"", "AF7714D4-D825-419A-B136-FF8293396635" );
            // Attrib for BlockType: Dynamic Data:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this dynamic data block.", 1, @"", "824634D6-7F75-465B-A2D2-BA3CE1662CAC" );
            // Attrib Value for Block:Dynamic Data, Attribute:Update Page Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "230EDFE8-33CA-478D-8C9A-572323AF3466", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Params Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Columns Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "90B0E6AF-B2F4-4397-953B-737A40D4023B", @"Id, GroupId" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"{% include '~/Plugins/com_bemaservices/MinistrySafe/Assets/Sql/BackgroundCheckReport.sql' %}" );
            // Attrib Value for Block:Dynamic Data, Attribute:Url Mask Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "B9163A35-E09C-466D-8A2D-4ED81DF0114C", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Columns Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "202A82BF-7772-481C-8419-600012607972", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Merge Fields Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "8EB882CE-5BB1-4844-9C28-10190903EECD", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Formatted Output Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "6A233402-446C-47E9-94A5-6A247C29BC21", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Person Report Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Communication Recipient Person Id Columns Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "75DDB977-9E71-44E8-924B-27134659D3A4", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Excel Export Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "E11B57E5-EC7D-4C42-9ADA-37594D71F145", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Communicate Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "5B2C115A-C187-4AB3-93AE-7010644B39DA", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Person Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Bulk Update Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Stored Procedure Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "A4439703-5432-489A-9C14-155903D6A43E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Template Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "6697B0A2-C8FE-497A-B5B4-A9D459474338", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Paneled Grid Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "5449CB61-2DFC-4B55-A697-38F1C2AF128B", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Grid Filter Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Timeout Page: Background Check Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D8ACCC9-2EDF-4C87-A6AB-EA22DBC62D8E", "BEEE38DD-2791-4242-84B6-0495904143CC", @"30" );
        }

        private void BackgroundCheckBadge()
        {
            // Person Profile Badge
            RockMigrationHelper.UpdatePersonBadge( "MinistrySafe Background Check Badge", "Shows whether someone has taken a MinistrySafe Background Check.", "Rock.PersonProfile.Badge.Liquid", 0, "44FE66BE-C43F-43C7-AA8E-5209926C9945" );
            RockMigrationHelper.AddPersonBadgeAttributeValue( "44FE66BE-C43F-43C7-AA8E-5209926C9945", "01C9BA59-D8D4-4137-90A6-B3C06C70BBC3", @"{% assign backgroundCheckDate = Person | Attribute:'BackgroundCheckDate' %}
{% assign backgroundCheckResult = Person | Attribute:'BackgroundChecResult' %}
{% assign isDisabled = false %}
{% assign badgeColor = '#939393' %}

{% if backgroundCheckResult == 'Pending' %}
		{% capture tooltipText %}{{ Person.NickName }} is currently taking a MinistrySafe Background Check.{% endcapture %}
		{% capture badgeColor %}yellow{% endcapture %}
{% elseif backgroundCheckResult == 'Fail' %}
		{% capture tooltipText %}{{ Person.NickName }} has failed their MinistrySafe Background Check.{% endcapture %}
		{% capture badgeColor %}red{% endcapture %}
{% else %}
		{% assign daysSinceBackgroundCheck = 'Now' | DateDiff:backgroundCheckDate,'d' %}
		{% if daysSinceBackgroundCheck < -730 or backgroundCheckDate == blank %}
				{% assign isDisabled = true %}
				{% if daysSinceBackgroundCheck < -730 %}
						{% capture tooltipText %}{{ Person.NickName }}'s MinistrySafe Background Check has expired.{% endcapture %}
				{% elseif backgroundCheckDate == blank %}
						{% capture tooltipText %}{{ Person.NickName }} has not completed a MinistrySafe Background Check.{% endcapture %}
				{% endif %}
		{% else %}
				{% capture tooltipText %}{{ Person.NickName }} has completed their MinistrySafe Background Check.{% endcapture %}
				{% capture badgeColor %}green{% endcapture %}
		{% endif %}
{% endif %}
<div class='badge badge-lastvisitonsite {% if isDisabled == true %}badge-disabled{% endif %}' data-toggle='tooltip' data-original-title='{{ tooltipText }}'>
		<div class='badge-content'>
				<i class='badge-icon fa fa-lock {% if isDisabled == true %}badge-disabled{% endif %}' style='color: {{badgeColor}};'></i>
				<span class='duration {% if isDisabled == true %}badge-disabled{% endif %}'  style='color: {{badgeColor}};'></span>
		</div>
</div>
" );
            RockMigrationHelper.AddBlockAttributeValue( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B", "F5AB231E-3836-4D52-BD03-BF79773C237A", "44FE66BE-C43F-43C7-AA8E-5209926C9945", true );
        }

        private void BackgroundCheckWorkflow()
        {
            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AssignActivityFromAttributeValue", "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AssignActivityToGroup", "DB2D8C44-6E57-4B45-8973-5DE327D61554", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AssignActivityToSecurityRole", "08189B3F-B506-45E8-AA68-99EC51085CF3", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.BackgroundCheckRequest", "C4DAE3D6-931F-497F-AC00-60BAFA87B758", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.DeleteWorkflow", "0E79AF40-4FB0-49D7-AB0E-E95BD828C62D", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunSQL", "A41216D6-6FB0-4019-B222-2C29B4519CF4", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeFromEntity", "972F19B9-598B-474B-97A4-50E56E7B59D2", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeToCurrentPerson", "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeValue", "C789E457-0783-44B3-9D8F-2EBAB5F11110", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetPersonAttribute", "320622DA-52E0-41AE-AF90-2BF78B488552", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetStatus", "96D371A7-A291-4F8F-8B38-B8F72CE5407E", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetWorkflowName", "36005473-BD5D-470B-B28D-98E6D7ED808D", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "08189B3F-B506-45E8-AA68-99EC51085CF3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "27BAC9C8-2BF7-405A-AA01-845A3D374295" ); // Rock.Workflow.Action.AssignActivityToSecurityRole:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "08189B3F-B506-45E8-AA68-99EC51085CF3", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Security Role", "SecurityRole", "The security role to assign this activity to.", 0, @"", "D53823A1-28CB-4BA0-A24C-873ECF4079C5" ); // Rock.Workflow.Action.AssignActivityToSecurityRole:Security Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "08189B3F-B506-45E8-AA68-99EC51085CF3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "120D39B5-8D2A-4B96-9419-C73BE0F2451A" ); // Rock.Workflow.Action.AssignActivityToSecurityRole:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "0E79AF40-4FB0-49D7-AB0E-E95BD828C62D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "361A1EC8-FFD0-4880-AF68-91DC0E0D7CDC" ); // Rock.Workflow.Action.DeleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "0E79AF40-4FB0-49D7-AB0E-E95BD828C62D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "79D23F8B-0DC8-4B48-8A86-AEA48B396C82" ); // Rock.Workflow.Action.DeleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE9CB292-4785-4EA3-976D-3826F91E9E98" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The attribute to set to the currently logged in person.", 0, @"", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "320622DA-52E0-41AE-AF90-2BF78B488552", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E5BAC4A6-FF7F-4016-BA9C-72D16CB60184" ); // Rock.Workflow.Action.SetPersonAttribute:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "320622DA-52E0-41AE-AF90-2BF78B488552", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person", "Person", "Workflow attribute that contains the person to update.", 0, @"", "E456FB6F-05DB-4826-A612-5B704BC4EA13" ); // Rock.Workflow.Action.SetPersonAttribute:Person
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "320622DA-52E0-41AE-AF90-2BF78B488552", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Value|Attribute Value", "Value", "The value or attribute value to set the person attribute to. <span class='tip tip-lava'></span>", 2, @"", "94689BDE-493E-4869-A614-2D54822D747C" ); // Rock.Workflow.Action.SetPersonAttribute:Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "320622DA-52E0-41AE-AF90-2BF78B488552", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Person Attribute", "PersonAttribute", "The person attribute that should be updated with the provided value.", 1, @"", "8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762" ); // Rock.Workflow.Action.SetPersonAttribute:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "320622DA-52E0-41AE-AF90-2BF78B488552", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3F3BF3E6-AD53-491E-A40F-441F2AFCBB5B" ); // Rock.Workflow.Action.SetPersonAttribute:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0A800013-51F7-4902-885A-5BE215D67D3D" ); // Rock.Workflow.Action.SetWorkflowName:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "NameValue", "The value to use for the workflow's name. <span class='tip tip-lava'></span>", 1, @"", "93852244-A667-4749-961A-D47F88675BE4" ); // Rock.Workflow.Action.SetWorkflowName:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5D95C15A-CCAE-40AD-A9DD-F929DA587115" ); // Rock.Workflow.Action.SetWorkflowName:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 8, @"False", "51C1E5C2-2422-46DD-BA47-5D9E1308DC32" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 5, @"", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C" ); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, @"", "A059767A-5592-4926-948A-1065AF4E9748" ); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 6, @"", "FFD9193A-451F-40E6-9776-74D5DCAC1450" ); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", 2, @"", "AFF1FD19-0F86-40A2-881E-298268F852B0" ); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "96D371A7-A291-4F8F-8B38-B8F72CE5407E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36CE41F4-4C87-4096-B0C6-8269163BCC0A" ); // Rock.Workflow.Action.SetStatus:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "96D371A7-A291-4F8F-8B38-B8F72CE5407E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Status", "Status", "The status to set workflow to. <span class='tip tip-lava'></span>", 0, @"", "91A9F4BE-4A8E-430A-B466-A88DB2D33B34" ); // Rock.Workflow.Action.SetStatus:Status
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "96D371A7-A291-4F8F-8B38-B8F72CE5407E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AE8C180C-E370-414A-B10D-97891B95D105" ); // Rock.Workflow.Action.SetStatus:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "5112B3B3-52F7-4527-B7BE-3CE7FBD8F92B" ); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeFromEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Entity Is Required", "EntityIsRequired", "Should an error be returned if the entity is missing or not a valid entity type?", 2, @"True", "DAE99CA3-E7B6-42A0-9E65-7A4C5029EEFC" ); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Id instead of Guid", "UseId", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", 3, @"False", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B" ); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 1, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeFromEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQLQuery", "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", 0, @"", "F3B9908B-096F-460B-8320-122CF046D1F9" ); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A18C3143-0586-4565-9F36-E603BC674B4E" ); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Continue On Error", "ContinueOnError", "Should processing continue even if SQL Error occurs?", 3, @"False", "D992DB0A-B528-4833-ADCE-61C5BD9BD156" ); // Rock.Workflow.Action.RunSQL:Continue On Error
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Result Attribute", "ResultAttribute", "An optional attribute to set to the scaler result of SQL query.", 2, @"", "56997192-2545-4EA1-B5B2-313B04588984" ); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Parameters", "Parameters", "The parameters to supply to the SQL query. <span class='tip tip-lava'></span>", 1, @"", "45C97B53-B45E-44CD-8BD9-12DB8302BE38" ); // Rock.Workflow.Action.RunSQL:Parameters
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "FA7C685D-8636-41EF-9998-90FFF3998F76" ); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "6BEBD4BE-EDC7-4757-B597-445FC60DB6ED" ); // Rock.Workflow.Action.BackgroundCheckRequest:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Billing Code Attribute", "BillingCodeAttribute", "The attribute that contains the billing code to use when submitting background check.", 4, @"", "232B2F98-3B2F-4C53-81FC-061A92675C41" ); // Rock.Workflow.Action.BackgroundCheckRequest:Billing Code Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The Person attribute that contains the person who the background check should be submitted for.", 1, @"", "077A9C4E-86E7-42F6-BEC3-DBC8F57E6A13" ); // Rock.Workflow.Action.BackgroundCheckRequest:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Request Type Attribute", "RequestTypeAttribute", "The attribute that contains the type of background check to submit (Specific to provider).", 3, @"", "EC759165-949E-4966-BAFD-68A656A4EBF7" ); // Rock.Workflow.Action.BackgroundCheckRequest:Request Type Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "33E6DF69-BDFA-407A-9744-C175B60643AE", "SSN Attribute", "SSNAttribute", "The attribute that contains the Social Security Number of the person who the background check should be submitted for ( Must be an 'Encrypted Text' attribute )", 2, @"", "2631E72B-1D9B-40E8-B857-8B1D41943451" ); // Rock.Workflow.Action.BackgroundCheckRequest:SSN Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "Background Check Provider", "Provider", "The Background Check provider to use", 0, @"", "6E2366B4-9F0E-454A-9DB1-E06263749C12" ); // Rock.Workflow.Action.BackgroundCheckRequest:Background Check Provider
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3936E931-CC27-4C38-9AA5-AAA502057333" ); // Rock.Workflow.Action.BackgroundCheckRequest:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "DB2D8C44-6E57-4B45-8973-5DE327D61554", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "C0D75D1A-16C5-4786-A1E0-25669BEE8FE9" ); // Rock.Workflow.Action.AssignActivityToGroup:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "DB2D8C44-6E57-4B45-8973-5DE327D61554", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "041B7B51-A694-4AF5-B455-64D0DE7160A2" ); // Rock.Workflow.Action.AssignActivityToGroup:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "DB2D8C44-6E57-4B45-8973-5DE327D61554", "CC34CE2C-0B0E-4BB3-9549-454B2A7DF218", "Group", "Group", "Select group type, then group, to set the group to assign this activity to.", 0, @"", "BBFAD050-5968-4D11-8887-2FF877D8C8AB" ); // Rock.Workflow.Action.AssignActivityToGroup:Group
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Status Attribute", "Status", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", 0, @"Completed", "3327286F-C1A9-4624-949D-33E9F9049356" ); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF" ); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The person or group attribute value to assign this activity to.", 0, @"", "FBADD25F-D309-4512-8430-3CC8615DD60E" ); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA" ); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Safety & Security", "fa fa-medkit", "", "6F8A431C-BEBD-4D33-AAD6-1D70870329C2", 0 ); // Safety & Security

            #endregion

            #region Background Check (MinistrySafe)

            RockMigrationHelper.UpdateWorkflowType( false, true, "Background Check (MinistrySafe)", "Used to request a background check be performed on a person.", "6F8A431C-BEBD-4D33-AAD6-1D70870329C2", "Request", "fa fa-check-square-o", 0, true, 0, com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, 0 ); // Background Check (MinistrySafe)
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Checked Attribute", "CheckedAttribute", "The person attribute that indicates if person has a valid background check (passed)", 0, @"daf87b87-3d1e-463d-a197-52227fe4ea28", "BE394A58-9833-4EF2-9EDB-5675E1C1C2AF", false ); // Background Check (MinistrySafe):Checked Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "9C204CD0-1233-41C5-818A-C5DA439445AA", "Request Message", "RequestMessage", "", 1, @"", "02980C6E-1F67-458E-B636-85759B3C1061", false ); // Background Check (MinistrySafe):Request Message
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "9C204CD0-1233-41C5-818A-C5DA439445AA", "Request Status", "RequestStatus", "", 2, @"", "CF7CAB33-922B-482E-A58B-6EAF009FF3EC", false ); // Background Check (MinistrySafe):Request Status
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Date Attribute", "DateAttribute", "The person attribute the stores the date background check was completed", 3, @"3daff000-7f74-47d7-8cb0-e4a4e6c81f5f", "97934B18-0845-4333-B686-0668DAF9CA70", false ); // Background Check (MinistrySafe):Date Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Status Attribute", "StatusAttribute", "The person attribute that stores the background check status", 4, @"44490089-e02c-4e54-a456-454845abbc9d", "78EA926E-FB4A-489E-9CD6-4DAE0538867D", false ); // Background Check (MinistrySafe):Status Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Result Attribute", "ResultAttribute", "The person attribute that stores the background check document", 5, @"f3931952-460d-43e0-a6e0-eb6b5b1f9167", "B9C7DDE6-4BC8-4406-B382-2F2AC65F5E41", false ); // Background Check (MinistrySafe):Result Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requester", "Requester", "The person initiating the request", 6, @"", "6A1412F4-569A-4DFD-B477-BDB99644C024", false ); // Background Check (MinistrySafe):Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person who request should be initiated for", 7, @"", "120910C9-516D-48B5-8CE6-0E665BA1138A", false ); // Background Check (MinistrySafe):Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Warn Of Recent", "WarnOfRecent", "Flag indicating if user should be warned that person has a recent background check already.", 8, @"False", "391E0DB2-BE6D-46A3-AAC0-1F96B147864B", false ); // Background Check (MinistrySafe):Warn Of Recent
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "If included, the campus name will be used as the Billing Reference Code for the request (optional)", 9, @"", "4CC138F7-735F-4BC7-81AF-E0A551446AC9", false ); // Background Check (MinistrySafe):Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Type", "PackageType", "Value should be the type of background check to request from the vendor.", 10, @"", "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", false ); // Background Check (MinistrySafe):Type
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Reason", "Reason", "A brief description of the reason that a background check is being requested", 11, @"", "C7F035C9-8206-4179-9539-CABD0865B32A", false ); // Background Check (MinistrySafe):Reason
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Report Status", "ReportStatus", "The result status of the background check", 12, @"", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", false ); // Background Check (MinistrySafe):Report Status
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "9C204CD0-1233-41C5-818A-C5DA439445AA", "Report Recommendation", "ReportRecommendation", "Providers recommendation ( if any )", 13, @"", "B3979F05-54A2-444E-A720-63500610A01A", false ); // Background Check (MinistrySafe):Report Recommendation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, "D05B3808-803A-4531-9680-DD4AAB8ADF1A", "Report", "Report", "The downloaded background check report", 14, @"", "31C4EC7C-8DEC-4305-A363-428D7B07C300", false ); // Background Check (MinistrySafe):Report
            RockMigrationHelper.AddAttributeQualifier( "BE394A58-9833-4EF2-9EDB-5675E1C1C2AF", "allowmultiple", @"False", "D0EF2625-2B76-432C-AA71-E7059A85CA32" ); // Background Check (MinistrySafe):Checked Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier( "BE394A58-9833-4EF2-9EDB-5675E1C1C2AF", "entitytype", @"72657ed8-d16e-492e-ac12-144c5e7567e7", "6CD80570-182F-4C1E-8E33-511B56F54E35" ); // Background Check (MinistrySafe):Checked Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier( "97934B18-0845-4333-B686-0668DAF9CA70", "allowmultiple", @"False", "44B91E41-D606-4DEA-94CC-A2EA4814F2F6" ); // Background Check (MinistrySafe):Date Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier( "97934B18-0845-4333-B686-0668DAF9CA70", "entitytype", @"72657ed8-d16e-492e-ac12-144c5e7567e7", "114A6D46-76BD-479A-964F-67ECB8235917" ); // Background Check (MinistrySafe):Date Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier( "78EA926E-FB4A-489E-9CD6-4DAE0538867D", "allowmultiple", @"False", "1584E146-008E-49D2-8110-78D51E8A430B" ); // Background Check (MinistrySafe):Status Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier( "78EA926E-FB4A-489E-9CD6-4DAE0538867D", "entitytype", @"72657ed8-d16e-492e-ac12-144c5e7567e7", "85E117E0-9C86-4E38-B1DE-8A9C4C1B7B04" ); // Background Check (MinistrySafe):Status Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier( "B9C7DDE6-4BC8-4406-B382-2F2AC65F5E41", "allowmultiple", @"False", "2B26B276-9593-4ADD-AEF5-D52F63F0F8FF" ); // Background Check (MinistrySafe):Result Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier( "B9C7DDE6-4BC8-4406-B382-2F2AC65F5E41", "entitytype", @"72657ed8-d16e-492e-ac12-144c5e7567e7", "2CE815E7-3BBD-44FC-BDF3-7545ECC8E4ED" ); // Background Check (MinistrySafe):Result Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier( "391E0DB2-BE6D-46A3-AAC0-1F96B147864B", "falsetext", @"No", "36622B86-5412-4E63-9481-B08000A02B53" ); // Background Check (MinistrySafe):Warn Of Recent:falsetext
            RockMigrationHelper.AddAttributeQualifier( "391E0DB2-BE6D-46A3-AAC0-1F96B147864B", "truetext", @"Yes", "58BCDD30-CDDB-4920-A927-B29815CB908D" ); // Background Check (MinistrySafe):Warn Of Recent:truetext
            RockMigrationHelper.AddAttributeQualifier( "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", "allowmultiple", @"False", "6CDDED26-E0F6-40CF-AABB-E7257D9BE52C" ); // Background Check (MinistrySafe):Type:allowmultiple
            RockMigrationHelper.AddAttributeQualifier( "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", "definedtypeguid", @"BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "40F85EC5-D42C-4ED2-89D8-3A3C014B93A8" ); // Background Check (MinistrySafe):Type:definedtype
            RockMigrationHelper.AddAttributeQualifier( "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", "displaydescription", @"False", "C2F9EA13-6C4A-43CA-B0B9-2B56300C07ED" ); // Background Check (MinistrySafe):Type:displaydescription
            RockMigrationHelper.AddAttributeQualifier( "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", "enhancedselection", @"False", "E713EF3B-6960-4E2A-AA39-70A8AFB106EC" ); // Background Check (MinistrySafe):Type:enhancedselection
            RockMigrationHelper.AddAttributeQualifier( "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", "includeInactive", @"False", "FFA77FD0-6B98-4D16-B418-C6C1C5F6DBF0" ); // Background Check (MinistrySafe):Type:includeInactive
            RockMigrationHelper.AddAttributeQualifier( "C7F035C9-8206-4179-9539-CABD0865B32A", "numberofrows", @"4", "36C3AA98-CD59-4031-85DB-15A8767A0938" ); // Background Check (MinistrySafe):Reason:numberofrows
            RockMigrationHelper.AddAttributeQualifier( "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", "fieldtype", @"ddl", "C0904CF8-0094-4A51-A566-928C0C2C9A48" ); // Background Check (MinistrySafe):Report Status:fieldtype
            RockMigrationHelper.AddAttributeQualifier( "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", "values", @"Pass,Fail,Review", "920002FC-D562-4B52-B5A7-E3D7710D9A4B" ); // Background Check (MinistrySafe):Report Status:values
            RockMigrationHelper.AddAttributeQualifier( "B3979F05-54A2-444E-A720-63500610A01A", "ispassword", @"False", "999BB7CB-41B4-4350-855C-DA5EFF88AACD" ); // Background Check (MinistrySafe):Report Recommendation:ispassword
            RockMigrationHelper.AddAttributeQualifier( "31C4EC7C-8DEC-4305-A363-428D7B07C300", "binaryFileType", @"5C701472-8A6B-4BBE-AEC6-EC833C859F2D", "E20C9B01-6FB9-47E4-948A-5FA3992888DD" ); // Background Check (MinistrySafe):Report:binaryFileType
            RockMigrationHelper.UpdateWorkflowActivityType( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true, "Initial Request", "Saves the person and requester and prompts for additional information needed to perform the request ( Campus, Type, etc).", true, 0, "C68D3D90-AB1A-4732-AE89-B49656859173" ); // Background Check (MinistrySafe):Initial Request
            RockMigrationHelper.UpdateWorkflowActivityType( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true, "Approve Request", "Assigns the activity to security team and waits for their approval before submitting the request.", false, 1, "47DDEFDA-D5C2-42D3-A872-E271F9D71D89" ); // Background Check (MinistrySafe):Approve Request
            RockMigrationHelper.UpdateWorkflowActivityType( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true, "Review Denial", "Provides the requester a way to add additional information for the security team to approve request.", false, 2, "18281C0D-07ED-4FF4-BE23-DD7B8264D9F4" ); // Background Check (MinistrySafe):Review Denial
            RockMigrationHelper.UpdateWorkflowActivityType( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true, "Submit Request", "Submits the background request to the selected provider for processing.", false, 3, "13548270-EF72-4B02-A980-AB6A74E3D56E" ); // Background Check (MinistrySafe):Submit Request
            RockMigrationHelper.UpdateWorkflowActivityType( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true, "Request Error", "Displays any error from the request that is submitted to the background check provider and allows request to be resubmitted", false, 4, "60644286-687A-4AB4-9C4F-A930DA3C155F" ); // Background Check (MinistrySafe):Request Error
            RockMigrationHelper.UpdateWorkflowActivityType( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true, "Process Result", "Evaluates the result of the background check received from the provider", false, 5, "CA4F5173-4F0B-4A61-8AA7-E1C05F1098E6" ); // Background Check (MinistrySafe):Process Result
            RockMigrationHelper.UpdateWorkflowActivityType( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true, "Review Result", "Allows for review of the results from provider.", false, 6, "5603DF2D-C429-4230-AEA1-3B6B1A19EF71" ); // Background Check (MinistrySafe):Review Result
            RockMigrationHelper.UpdateWorkflowActivityType( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true, "Complete Request", "Notifies requester of result and updates person's record with result", false, 7, "F4EC7AF1-4478-46DC-9C4B-D2B4924C9D3A" ); // Background Check (MinistrySafe):Complete Request
            RockMigrationHelper.UpdateWorkflowActivityType( com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true, "Cancel Request", "Cancels the request prior to submitting to provider and deletes the workflow.", false, 8, "AB8ED98A-20F9-4E74-912B-430DB21E069C" ); // Background Check (MinistrySafe):Cancel Request
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "47DDEFDA-D5C2-42D3-A872-E271F9D71D89", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Note", "Note", "Any notes that approver wants to provide to submitter for review", 0, @"", "DD138A6C-D328-4A3C-94DC-E663D829165D" ); // Background Check (MinistrySafe):Approve Request:Note
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "47DDEFDA-D5C2-42D3-A872-E271F9D71D89", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Approver", "Approver", "Person who approved or denied this request", 1, @"", "11A06BD5-A023-4CF2-AE6B-767FE01FE594" ); // Background Check (MinistrySafe):Approve Request:Approver
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "47DDEFDA-D5C2-42D3-A872-E271F9D71D89", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Approval Status", "ApprovalStatus", "The status of the appoval (Approve,Deny)", 2, @"", "38BEEEE1-76FA-4E53-8978-685C9B472DF4" ); // Background Check (MinistrySafe):Approve Request:Approval Status
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "5603DF2D-C429-4230-AEA1-3B6B1A19EF71", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Review Result", "ReviewResult", "The result of the review (Pass,Fail)", 0, @"", "2CC2084E-D717-42B6-B7F6-40DE33EC92F5" ); // Background Check (MinistrySafe):Review Result:Review Result
            RockMigrationHelper.AddAttributeQualifier( "DD138A6C-D328-4A3C-94DC-E663D829165D", "numberofrows", @"5", "44219193-2B42-4D68-8803-9D1C4FDBE848" ); // Background Check (MinistrySafe):Note:numberofrows
            RockMigrationHelper.AddAttributeQualifier( "38BEEEE1-76FA-4E53-8978-685C9B472DF4", "ispassword", @"False", "4D9F1EFB-58FB-4ECF-A001-0DD24D361520" ); // Background Check (MinistrySafe):Approval Status:ispassword
            RockMigrationHelper.AddAttributeQualifier( "2CC2084E-D717-42B6-B7F6-40DE33EC92F5", "ispassword", @"False", "0EE69DC4-2A94-4536-8DAF-95A20AD67888" ); // Background Check (MinistrySafe):Review Result:ispassword
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>Background Request Details</h1>
<p> {{CurrentPerson.NickName}}, please complete the form below to start the background request process. </p>
{% if Workflow | Attribute:'WarnOfRecent' == 'Yes' %}
    <div class='alert alert-warning'>
        Notice: It's been less than a year since this person's last background check was processed.
        Please make sure you want to continue with this request!
    </div>
{% endif %}
<hr />", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^47ddefda-d5c2-42d3-a872-e271f9d71d89^Your request has been submitted successfully.|Cancel^5683E775-B9F3-408C-80AC-94DE0E51CF3A^ab8ed98a-20f9-4e74-912b-430db21e069c^The request has been canceled.", "", false, "", "33264364-ACC6-48F9-B57B-3E4ADEDE84C8" ); // Background Check (MinistrySafe):Initial Request:Get Details
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>Background Request Details</h1>
<div class='alert alert-info'>
    {{CurrentPerson.NickName}}, the following background request has been submitted for your review.
    If you approve the request it will be sent to the background check provider for processing. If you
    deny the request, it will be sent back to the requester. If you deny the request, please add notes
    explaining why the request was denied.
</div>", @"", "Approve^c88fef94-95b9-444a-bc93-58e983f3c047^^The request has been submitted to provider for processing.|Deny^d6b809a9-c1cc-4ebb-816e-33d8c1e53ea4^^The requester will be notified that this request has been denied (along with the reason why).", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "38BEEEE1-76FA-4E53-8978-685C9B472DF4", "5A1D12E1-64CD-4963-879C-DC99285B8118" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>Background Request Details</h1>
<p> {{CurrentPerson.NickName}}, this request has come back from the approval process with the following results. </p>
<div class=""well"">
    <strong>Summary of Security Notes:</strong>
    <br />
    <table class=""table table-condensed table-light margin-b-md"">
        {% for activity in Workflow.Activities %}
            {% if activity.ActivityType.Name == 'Approve Request' %}
                <tr>
                    <td width=""220"">{{activity.CompletedDateTime}}</td>
                    <td width=""220"">{{activity | Attribute:'Approver'}}</td>
                    <td>{{activity | Attribute:'Note'}}</td>
                </tr>
            {% endif %}
        {% endfor %}
    </table>
</div>
<hr />", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^47ddefda-d5c2-42d3-a872-e271f9d71d89^The request has been submitted again to the security team for approval.|Cancel Request^5683E775-B9F3-408C-80AC-94DE0E51CF3A^ab8ed98a-20f9-4e74-912b-430db21e069c^The request has been canceled.", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C" ); // Background Check (MinistrySafe):Review Denial:Review
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>Background Request Details</h1>
<div class='alert alert-danger'>
    An error occurred when submitting the background check. See details below.
</div>", @"", "Re-Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^13548270-ef72-4b02-a980-ab6a74e3d56e^Your information has been submitted successfully.", "", true, "", "60B06A7A-9187-4DCB-A998-AFE533112955" ); // Background Check (MinistrySafe):Request Error:Display Error Message
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>Background Request Details</h1>
<div class='alert alert-info'>
    {{CurrentPerson.NickName}}, the following background request was submitted and completed, but requires
    your review. Please pass or fail this request. The requester will be notified and the person's record
    will be updated to indicate the result you select.
</div>
<hr />", @"", "Pass^c88fef94-95b9-444a-bc93-58e983f3c047^^The request has been marked as passed. Requester will be notified.|Fail^d6b809a9-c1cc-4ebb-816e-33d8c1e53ea4^^The request has been marked as failed. Requester will be notified.", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "2CC2084E-D717-42B6-B7F6-40DE33EC92F5", "167C9224-873F-4F7E-B3CB-2FD58AA0E492" ); // Background Check (MinistrySafe):Review Result:Review Results
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "BE394A58-9833-4EF2-9EDB-5675E1C1C2AF", 12, false, true, false, false, @"", @"", "1D237946-2E23-46D0-BC2D-95D1C8197DA3" ); // Background Check (MinistrySafe):Initial Request:Get Details:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "02980C6E-1F67-458E-B636-85759B3C1061", 13, false, true, false, false, @"", @"", "D9BB2C29-F5E8-4696-B72A-39B8A01B7F52" ); // Background Check (MinistrySafe):Initial Request:Get Details:Request Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "CF7CAB33-922B-482E-A58B-6EAF009FF3EC", 14, false, true, false, false, @"", @"", "4A7FC5EC-8E22-48F9-AF29-D3D117503C39" ); // Background Check (MinistrySafe):Initial Request:Get Details:Request Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "97934B18-0845-4333-B686-0668DAF9CA70", 0, false, true, false, false, @"", @"", "FD46FFDF-4BE5-42C7-9C20-0E74C7C2CA09" ); // Background Check (MinistrySafe):Initial Request:Get Details:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "78EA926E-FB4A-489E-9CD6-4DAE0538867D", 1, false, true, false, false, @"", @"", "9F86BC55-9A0C-4EF5-BC9B-6239D232A143" ); // Background Check (MinistrySafe):Initial Request:Get Details:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "B9C7DDE6-4BC8-4406-B382-2F2AC65F5E41", 2, false, true, false, false, @"", @"", "86FF95FC-2E05-4C0A-AFE7-CFA6EDF27E88" ); // Background Check (MinistrySafe):Initial Request:Get Details:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "6A1412F4-569A-4DFD-B477-BDB99644C024", 3, false, true, false, false, @"", @"", "568A38DF-CF9D-41B5-AEE9-9513957BB9FE" ); // Background Check (MinistrySafe):Initial Request:Get Details:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "120910C9-516D-48B5-8CE6-0E665BA1138A", 4, true, true, false, false, @"", @"", "E948A86A-F7A9-4EBF-BF24-E19CF207104D" ); // Background Check (MinistrySafe):Initial Request:Get Details:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "391E0DB2-BE6D-46A3-AAC0-1F96B147864B", 5, false, true, false, false, @"", @"", "54B5D688-B377-4640-9AA8-FD2AAEE3848E" ); // Background Check (MinistrySafe):Initial Request:Get Details:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "4CC138F7-735F-4BC7-81AF-E0A551446AC9", 6, true, false, false, false, @"", @"", "229EA701-EE37-4D78-83B6-53D031D661BC" ); // Background Check (MinistrySafe):Initial Request:Get Details:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", 7, false, true, false, false, @"", @"", "7E26BD6A-D7A6-42A5-B975-94B623A6A374" ); // Background Check (MinistrySafe):Initial Request:Get Details:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "C7F035C9-8206-4179-9539-CABD0865B32A", 8, true, false, false, false, @"", @"", "EA06A840-C182-4A9D-83A8-4E310E004D05" ); // Background Check (MinistrySafe):Initial Request:Get Details:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 9, false, true, false, false, @"", @"", "D06488EC-AB3C-4F6F-969B-FC667AD2FB21" ); // Background Check (MinistrySafe):Initial Request:Get Details:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "B3979F05-54A2-444E-A720-63500610A01A", 11, false, true, false, false, @"", @"", "22435C2D-2C71-488F-BF41-5B0C855E95B5" ); // Background Check (MinistrySafe):Initial Request:Get Details:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "31C4EC7C-8DEC-4305-A363-428D7B07C300", 10, false, true, false, false, @"", @"", "D2993E57-306A-44AD-A6AB-C00527DA9E5A" ); // Background Check (MinistrySafe):Initial Request:Get Details:Report
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "DD138A6C-D328-4A3C-94DC-E663D829165D", 9, true, false, false, false, @"", @"", "1A146D90-3E28-485C-8037-D8DED6B5AFC5" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Note
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "BE394A58-9833-4EF2-9EDB-5675E1C1C2AF", 15, false, true, false, false, @"", @"", "DE676765-1877-4C66-9545-2EBACAB2180D" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "02980C6E-1F67-458E-B636-85759B3C1061", 16, false, true, false, false, @"", @"", "1175977E-8845-4F6E-B0CC-55EDFD69C314" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Request Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "11A06BD5-A023-4CF2-AE6B-767FE01FE594", 12, false, true, false, false, @"", @"", "0F6FA348-69B7-4770-921F-A289ACD52020" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "38BEEEE1-76FA-4E53-8978-685C9B472DF4", 13, false, true, false, false, @"", @"", "21A84ECC-816E-4CE4-9767-42BB27A67D50" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Approval Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "CF7CAB33-922B-482E-A58B-6EAF009FF3EC", 17, false, true, false, false, @"", @"", "EAB94E77-C6EB-4E4B-A420-64828E553A93" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Request Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "97934B18-0845-4333-B686-0668DAF9CA70", 0, false, true, false, false, @"", @"", "57F009F8-992A-4902-AB5C-4B34CD0D6591" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "78EA926E-FB4A-489E-9CD6-4DAE0538867D", 1, false, true, false, false, @"", @"", "09B8FCEE-0135-439C-B1BA-269834996F82" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "B9C7DDE6-4BC8-4406-B382-2F2AC65F5E41", 2, false, true, false, false, @"", @"", "84467113-69A8-4AE8-96AF-A155D4252294" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "6A1412F4-569A-4DFD-B477-BDB99644C024", 3, true, true, false, false, @"", @"", "96E1DE04-9367-4048-92A7-B58A0E606653" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "120910C9-516D-48B5-8CE6-0E665BA1138A", 4, true, true, false, false, @"", @"", "34334411-602B-40D6-A3A5-416FC927EA94" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "391E0DB2-BE6D-46A3-AAC0-1F96B147864B", 5, false, true, false, false, @"", @"", "EA7E62EB-90C6-425B-9F26-3EFDCAC5270C" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "4CC138F7-735F-4BC7-81AF-E0A551446AC9", 6, true, true, false, false, @"", @"", "2BCC1CF9-ECC7-43C5-89D5-B87EC63D3E26" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", 8, true, false, true, false, @"", @"", "DE16DE69-B011-4079-8F3E-A8AC0AB37452" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "C7F035C9-8206-4179-9539-CABD0865B32A", 7, true, true, false, false, @"", @"", "D9AC2E29-CB58-4526-A46D-B10BF4679003" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 10, false, true, false, false, @"", @"", "E72AFC35-D70F-47F9-B107-1840C4B21D81" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "B3979F05-54A2-444E-A720-63500610A01A", 14, false, true, false, false, @"", @"", "914E6B1D-D8C3-4F07-BDE0-4064530B565C" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "5A1D12E1-64CD-4963-879C-DC99285B8118", "31C4EC7C-8DEC-4305-A363-428D7B07C300", 11, false, true, false, false, @"", @"", "91D5C441-D0E6-43AD-AB16-103ACF838775" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Report
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "BE394A58-9833-4EF2-9EDB-5675E1C1C2AF", 12, false, true, false, false, @"", @"", "3C3F24B9-4727-4D42-9F8B-BF94B447DE95" ); // Background Check (MinistrySafe):Review Denial:Review:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "02980C6E-1F67-458E-B636-85759B3C1061", 13, false, true, false, false, @"", @"", "9E45FD90-6BD6-4180-B74E-E77BA5AC9C50" ); // Background Check (MinistrySafe):Review Denial:Review:Request Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "CF7CAB33-922B-482E-A58B-6EAF009FF3EC", 14, false, true, false, false, @"", @"", "9F204559-F2EB-40C4-A2FA-6CDB9991D97E" ); // Background Check (MinistrySafe):Review Denial:Review:Request Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "97934B18-0845-4333-B686-0668DAF9CA70", 0, false, true, false, false, @"", @"", "E7BD6504-49D2-47C9-80AA-07C6B3BDA4CD" ); // Background Check (MinistrySafe):Review Denial:Review:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "78EA926E-FB4A-489E-9CD6-4DAE0538867D", 1, false, true, false, false, @"", @"", "73BCFE7B-C601-4709-821A-FFF5BE2988EC" ); // Background Check (MinistrySafe):Review Denial:Review:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "B9C7DDE6-4BC8-4406-B382-2F2AC65F5E41", 2, false, true, false, false, @"", @"", "F4C8D2CA-2008-4A3B-BFE7-511ADC98A287" ); // Background Check (MinistrySafe):Review Denial:Review:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "6A1412F4-569A-4DFD-B477-BDB99644C024", 3, false, true, false, false, @"", @"", "D5BEB0E9-4709-4487-8CED-FA021A21F3C2" ); // Background Check (MinistrySafe):Review Denial:Review:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "120910C9-516D-48B5-8CE6-0E665BA1138A", 5, true, true, false, false, @"", @"", "2A1D5AD0-C270-4E86-B841-E370BEF4155D" ); // Background Check (MinistrySafe):Review Denial:Review:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "391E0DB2-BE6D-46A3-AAC0-1F96B147864B", 4, false, true, false, false, @"", @"", "17CA66EF-FF3E-403D-962C-E3D5A33598D0" ); // Background Check (MinistrySafe):Review Denial:Review:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "4CC138F7-735F-4BC7-81AF-E0A551446AC9", 6, true, false, true, false, @"", @"", "C4FEDA8E-BE57-4CB7-9977-28843B4BB8EB" ); // Background Check (MinistrySafe):Review Denial:Review:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", 7, true, false, true, false, @"", @"", "B49548AB-6CA9-473C-A9A8-8F0BB9678B33" ); // Background Check (MinistrySafe):Review Denial:Review:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "C7F035C9-8206-4179-9539-CABD0865B32A", 8, true, false, false, false, @"", @"", "A95B35B6-03FF-43A2-9E92-1D03FBF006BB" ); // Background Check (MinistrySafe):Review Denial:Review:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 9, false, true, false, false, @"", @"", "587ACB10-3A51-4316-8F8E-3CCD7C3FDEE0" ); // Background Check (MinistrySafe):Review Denial:Review:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "B3979F05-54A2-444E-A720-63500610A01A", 11, false, true, false, false, @"", @"", "05923251-AD42-475F-B405-7B51E388D841" ); // Background Check (MinistrySafe):Review Denial:Review:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "31C4EC7C-8DEC-4305-A363-428D7B07C300", 10, false, true, false, false, @"", @"", "60E9E991-B073-4251-91E8-66114DBBADBE" ); // Background Check (MinistrySafe):Review Denial:Review:Report
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "BE394A58-9833-4EF2-9EDB-5675E1C1C2AF", 0, false, true, false, false, @"", @"", "3D15AD42-E5D1-4521-801A-91340738D8E7" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "02980C6E-1F67-458E-B636-85759B3C1061", 2, true, true, false, false, @"", @"", "A9E2421D-E652-436A-AE09-F8070F569FA5" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Request Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "CF7CAB33-922B-482E-A58B-6EAF009FF3EC", 1, true, true, false, false, @"", @"", "9DEA6FEA-41D9-4956-9C1A-3E0BB0673118" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Request Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "97934B18-0845-4333-B686-0668DAF9CA70", 3, false, true, false, false, @"", @"", "A6C20EA9-9B24-4F6F-A101-ED15148B5925" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "78EA926E-FB4A-489E-9CD6-4DAE0538867D", 4, false, true, false, false, @"", @"", "7CA0FAEA-E5FF-4577-BC5E-58E0726F84EC" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "B9C7DDE6-4BC8-4406-B382-2F2AC65F5E41", 5, false, true, false, false, @"", @"", "C1C1A3EE-87D1-413D-9BC3-B723D221B16F" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "6A1412F4-569A-4DFD-B477-BDB99644C024", 6, true, true, false, false, @"", @"", "37F957EC-1CB6-46D6-B6B6-8BDEDCF150C4" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "120910C9-516D-48B5-8CE6-0E665BA1138A", 7, true, true, false, false, @"", @"", "B0B04FB7-EF74-4F92-8442-333900897887" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "391E0DB2-BE6D-46A3-AAC0-1F96B147864B", 8, false, true, false, false, @"", @"", "C0B1C5DF-B4A3-43BF-9EAB-953CEBF527AB" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "4CC138F7-735F-4BC7-81AF-E0A551446AC9", 9, true, true, false, false, @"", @"", "88AE26CC-52E6-4CC8-B5DC-12DCA3F7AC3F" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", 10, true, false, true, false, @"", @"", "9EF495BA-1BB2-472F-BAB8-298B6E25FB74" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "C7F035C9-8206-4179-9539-CABD0865B32A", 11, false, true, false, false, @"", @"", "FA42EC59-97A3-4234-963C-2A441CF72362" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 12, false, true, false, false, @"", @"", "FFBD917D-CEFC-40DD-BC6F-ECDF5B157BF1" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "B3979F05-54A2-444E-A720-63500610A01A", 13, false, true, false, false, @"", @"", "ABDAAC6E-EAEC-4554-8D30-DD9EB5E3B03F" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "60B06A7A-9187-4DCB-A998-AFE533112955", "31C4EC7C-8DEC-4305-A363-428D7B07C300", 14, false, true, false, false, @"", @"", "044D7453-E265-41E0-81DE-A38BA7B68420" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Report
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "2CC2084E-D717-42B6-B7F6-40DE33EC92F5", 12, false, true, false, false, @"", @"", "CEFBAD4A-E9B1-4BC2-B288-352D305B3E39" ); // Background Check (MinistrySafe):Review Result:Review Results:Review Result
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "BE394A58-9833-4EF2-9EDB-5675E1C1C2AF", 13, false, true, false, false, @"", @"", "67344C95-F740-4F5D-9FC2-C059B5AA74CA" ); // Background Check (MinistrySafe):Review Result:Review Results:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "02980C6E-1F67-458E-B636-85759B3C1061", 14, false, true, false, false, @"", @"", "2E29CDB4-AC5E-4E40-9F62-BE234FE7C5FB" ); // Background Check (MinistrySafe):Review Result:Review Results:Request Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "CF7CAB33-922B-482E-A58B-6EAF009FF3EC", 15, false, true, false, false, @"", @"", "0664AB5F-A4A1-4C11-92A2-E4C6C6F754F8" ); // Background Check (MinistrySafe):Review Result:Review Results:Request Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "97934B18-0845-4333-B686-0668DAF9CA70", 0, false, true, false, false, @"", @"", "F246D1B6-8C6B-43BB-95C5-ABFFE2C6E410" ); // Background Check (MinistrySafe):Review Result:Review Results:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "78EA926E-FB4A-489E-9CD6-4DAE0538867D", 1, false, true, false, false, @"", @"", "90649E24-F576-4D85-A149-E3F8B741E22D" ); // Background Check (MinistrySafe):Review Result:Review Results:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "B9C7DDE6-4BC8-4406-B382-2F2AC65F5E41", 2, false, true, false, false, @"", @"", "3E32A230-8618-4D63-B02E-8B9A9491A53C" ); // Background Check (MinistrySafe):Review Result:Review Results:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "6A1412F4-569A-4DFD-B477-BDB99644C024", 3, true, true, false, false, @"", @"", "90E332FC-AFE0-47DA-A6B7-E28784896447" ); // Background Check (MinistrySafe):Review Result:Review Results:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "120910C9-516D-48B5-8CE6-0E665BA1138A", 4, true, true, false, false, @"", @"", "F2E35241-5FAA-476E-BC2B-6380B07AB34E" ); // Background Check (MinistrySafe):Review Result:Review Results:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "391E0DB2-BE6D-46A3-AAC0-1F96B147864B", 5, false, true, false, false, @"", @"", "EE8A26C5-182E-4F48-A495-12F9A72832B3" ); // Background Check (MinistrySafe):Review Result:Review Results:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "4CC138F7-735F-4BC7-81AF-E0A551446AC9", 6, true, true, false, false, @"", @"", "DFAD105A-0ECD-4D57-B243-C768F75433E2" ); // Background Check (MinistrySafe):Review Result:Review Results:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "63DA7BF3-B290-4832-B7DD-E1F0D89F373F", 7, false, true, false, false, @"", @"", "08191082-F92F-4C0A-B244-EB1B6F84AEFD" ); // Background Check (MinistrySafe):Review Result:Review Results:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "C7F035C9-8206-4179-9539-CABD0865B32A", 8, true, true, false, false, @"", @"", "4C47748E-6FE6-4C91-B296-FC11DF1EAD9F" ); // Background Check (MinistrySafe):Review Result:Review Results:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 9, false, true, false, false, @"", @"", "12522286-257F-40B3-95B4-229EAA4DCC48" ); // Background Check (MinistrySafe):Review Result:Review Results:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "B3979F05-54A2-444E-A720-63500610A01A", 10, true, true, false, false, @"", @"", "62E67B2F-8B7C-4FD0-A248-92B1C2179CB0" ); // Background Check (MinistrySafe):Review Result:Review Results:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "31C4EC7C-8DEC-4305-A363-428D7B07C300", 11, true, true, false, false, @"", @"", "421D6852-42BF-4663-B2C0-DB9433B1698E" ); // Background Check (MinistrySafe):Review Result:Review Results:Report
            RockMigrationHelper.UpdateWorkflowActionType( "C68D3D90-AB1A-4732-AE89-B49656859173", "Set Status", 0, "96D371A7-A291-4F8F-8B38-B8F72CE5407E", true, false, "", "", 1, "", "40BC73DD-5384-41D2-89C9-5D35FF1F6FFC" ); // Background Check (MinistrySafe):Initial Request:Set Status
            RockMigrationHelper.UpdateWorkflowActionType( "C68D3D90-AB1A-4732-AE89-B49656859173", "Set Person", 1, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "056FC534-70A6-4D5C-ADE4-E362A2B638DB" ); // Background Check (MinistrySafe):Initial Request:Set Person
            RockMigrationHelper.UpdateWorkflowActionType( "C68D3D90-AB1A-4732-AE89-B49656859173", "Set Name", 2, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "AC4DE5F1-6F9A-48AB-B931-33D02E0BBD4D" ); // Background Check (MinistrySafe):Initial Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType( "C68D3D90-AB1A-4732-AE89-B49656859173", "Set Requester", 3, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "71FABDB9-6026-4746-9783-C07C2E11DC05" ); // Background Check (MinistrySafe):Initial Request:Set Requester
            RockMigrationHelper.UpdateWorkflowActionType( "C68D3D90-AB1A-4732-AE89-B49656859173", "Set Warning", 4, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "25F89999-13E5-4458-B634-0A43CFF5FD74" ); // Background Check (MinistrySafe):Initial Request:Set Warning
            RockMigrationHelper.UpdateWorkflowActionType( "C68D3D90-AB1A-4732-AE89-B49656859173", "Get Details", 5, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "33264364-ACC6-48F9-B57B-3E4ADEDE84C8", "", 1, "", "F14042CB-7887-4449-9541-DE2E21FE5CE2" ); // Background Check (MinistrySafe):Initial Request:Get Details
            RockMigrationHelper.UpdateWorkflowActionType( "47DDEFDA-D5C2-42D3-A872-E271F9D71D89", "Set Status", 0, "96D371A7-A291-4F8F-8B38-B8F72CE5407E", true, false, "", "", 1, "", "F35ACC51-7A1B-49A0-ACF0-EF20A34257EB" ); // Background Check (MinistrySafe):Approve Request:Set Status
            RockMigrationHelper.UpdateWorkflowActionType( "47DDEFDA-D5C2-42D3-A872-E271F9D71D89", "Assign to Security", 1, "08189B3F-B506-45E8-AA68-99EC51085CF3", true, false, "", "", 1, "", "FDB827B1-1969-410C-8C98-CA14E2A85E6C" ); // Background Check (MinistrySafe):Approve Request:Assign to Security
            RockMigrationHelper.UpdateWorkflowActionType( "47DDEFDA-D5C2-42D3-A872-E271F9D71D89", "Approve or Deny", 2, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "5A1D12E1-64CD-4963-879C-DC99285B8118", "", 1, "", "C0318396-A457-4213-85F3-14F15CBEE37A" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny
            RockMigrationHelper.UpdateWorkflowActionType( "47DDEFDA-D5C2-42D3-A872-E271F9D71D89", "Set Approver", 3, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "8142AE90-CDDB-4B66-9D75-63EE43FB2306" ); // Background Check (MinistrySafe):Approve Request:Set Approver
            RockMigrationHelper.UpdateWorkflowActionType( "47DDEFDA-D5C2-42D3-A872-E271F9D71D89", "Submit Request", 4, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "38BEEEE1-76FA-4E53-8978-685C9B472DF4", 1, "Approve", "37F8C070-3C7B-41B6-9C6D-53019AA957B1" ); // Background Check (MinistrySafe):Approve Request:Submit Request
            RockMigrationHelper.UpdateWorkflowActionType( "47DDEFDA-D5C2-42D3-A872-E271F9D71D89", "Deny Request", 5, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "38BEEEE1-76FA-4E53-8978-685C9B472DF4", 1, "Deny", "03BDE50C-B5E7-456D-B281-2C8D777863A9" ); // Background Check (MinistrySafe):Approve Request:Deny Request
            RockMigrationHelper.UpdateWorkflowActionType( "18281C0D-07ED-4FF4-BE23-DD7B8264D9F4", "Set Status", 0, "96D371A7-A291-4F8F-8B38-B8F72CE5407E", true, false, "", "", 1, "", "53C9BE6C-0B25-4B78-B115-B93592D5FA6D" ); // Background Check (MinistrySafe):Review Denial:Set Status
            RockMigrationHelper.UpdateWorkflowActionType( "18281C0D-07ED-4FF4-BE23-DD7B8264D9F4", "Assign to Requester", 1, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "3A44285B-7DDE-47D9-902E-B7D10D13DC6C" ); // Background Check (MinistrySafe):Review Denial:Assign to Requester
            RockMigrationHelper.UpdateWorkflowActionType( "18281C0D-07ED-4FF4-BE23-DD7B8264D9F4", "Review", 2, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "B5352A0F-D1B6-497A-BC24-A54AF8E3ED2C", "", 1, "", "412BFD14-C2DE-4BFD-BE37-719541E71CD8" ); // Background Check (MinistrySafe):Review Denial:Review
            RockMigrationHelper.UpdateWorkflowActionType( "13548270-EF72-4B02-A980-AB6A74E3D56E", "Set Status", 0, "96D371A7-A291-4F8F-8B38-B8F72CE5407E", true, false, "", "", 1, "", "6E939331-805F-4216-A147-002B7165D3C1" ); // Background Check (MinistrySafe):Submit Request:Set Status
            RockMigrationHelper.UpdateWorkflowActionType( "13548270-EF72-4B02-A980-AB6A74E3D56E", "Submit Request", 1, "C4DAE3D6-931F-497F-AC00-60BAFA87B758", true, false, "", "", 1, "", "CD5C304C-70F6-4DC9-9606-72A156A36918" ); // Background Check (MinistrySafe):Submit Request:Submit Request
            RockMigrationHelper.UpdateWorkflowActionType( "13548270-EF72-4B02-A980-AB6A74E3D56E", "Process Request Error", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "CF7CAB33-922B-482E-A58B-6EAF009FF3EC", 2, "SUCCESS", "D4DBDC65-1FB9-4629-99FA-7129D253FB6A" ); // Background Check (MinistrySafe):Submit Request:Process Request Error
            RockMigrationHelper.UpdateWorkflowActionType( "13548270-EF72-4B02-A980-AB6A74E3D56E", "Process Result", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 64, "", "62F06CCA-6091-41DD-9208-C6B82C176687" ); // Background Check (MinistrySafe):Submit Request:Process Result
            RockMigrationHelper.UpdateWorkflowActionType( "60644286-687A-4AB4-9C4F-A930DA3C155F", "Set Status", 0, "96D371A7-A291-4F8F-8B38-B8F72CE5407E", true, false, "", "", 1, "", "3E3FDA77-9980-4EFA-AEEE-889A183D3990" ); // Background Check (MinistrySafe):Request Error:Set Status
            RockMigrationHelper.UpdateWorkflowActionType( "60644286-687A-4AB4-9C4F-A930DA3C155F", "Assign to Security", 1, "DB2D8C44-6E57-4B45-8973-5DE327D61554", true, false, "", "", 1, "", "A0A4BE4C-B585-4A85-B9A5-B091438DEF31" ); // Background Check (MinistrySafe):Request Error:Assign to Security
            RockMigrationHelper.UpdateWorkflowActionType( "60644286-687A-4AB4-9C4F-A930DA3C155F", "Display Error Message", 2, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "60B06A7A-9187-4DCB-A998-AFE533112955", "", 1, "", "A23E0089-DD5E-4310-9523-F45485CB69ED" ); // Background Check (MinistrySafe):Request Error:Display Error Message
            RockMigrationHelper.UpdateWorkflowActionType( "CA4F5173-4F0B-4A61-8AA7-E1C05F1098E6", "Save Date", 0, "320622DA-52E0-41AE-AF90-2BF78B488552", true, false, "", "", 1, "", "CB94B493-0B95-4328-AB3C-874180FF1FEA" ); // Background Check (MinistrySafe):Process Result:Save Date
            RockMigrationHelper.UpdateWorkflowActionType( "CA4F5173-4F0B-4A61-8AA7-E1C05F1098E6", "Save Report", 1, "320622DA-52E0-41AE-AF90-2BF78B488552", true, false, "", "", 1, "", "304E76E2-BA98-4B72-8E37-B91917179E5B" ); // Background Check (MinistrySafe):Process Result:Save Report
            RockMigrationHelper.UpdateWorkflowActionType( "CA4F5173-4F0B-4A61-8AA7-E1C05F1098E6", "Activate Review", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 2, "Pass", "F7F8C93A-3C3B-4C5F-8B4D-8BF2560205FC" ); // Background Check (MinistrySafe):Process Result:Activate Review
            RockMigrationHelper.UpdateWorkflowActionType( "CA4F5173-4F0B-4A61-8AA7-E1C05F1098E6", "Activate Complete", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 1, "Pass", "4C0F1458-9C90-4191-B807-DBA2A9328E62" ); // Background Check (MinistrySafe):Process Result:Activate Complete
            RockMigrationHelper.UpdateWorkflowActionType( "5603DF2D-C429-4230-AEA1-3B6B1A19EF71", "Set Status", 0, "96D371A7-A291-4F8F-8B38-B8F72CE5407E", true, false, "", "", 1, "", "6BA1B8F7-7EF2-4817-8E15-E98916084B94" ); // Background Check (MinistrySafe):Review Result:Set Status
            RockMigrationHelper.UpdateWorkflowActionType( "5603DF2D-C429-4230-AEA1-3B6B1A19EF71", "Assign Activity", 1, "08189B3F-B506-45E8-AA68-99EC51085CF3", true, false, "", "", 1, "", "E5014BE9-2DB5-4907-A575-E74E4C56E484" ); // Background Check (MinistrySafe):Review Result:Assign Activity
            RockMigrationHelper.UpdateWorkflowActionType( "5603DF2D-C429-4230-AEA1-3B6B1A19EF71", "Review Results", 2, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "167C9224-873F-4F7E-B3CB-2FD58AA0E492", "", 1, "", "F4BB8142-B7F3-4246-A033-C6EE54BBF437" ); // Background Check (MinistrySafe):Review Result:Review Results
            RockMigrationHelper.UpdateWorkflowActionType( "5603DF2D-C429-4230-AEA1-3B6B1A19EF71", "Update Result", 3, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "67ACE49E-0AA7-4883-852D-8BD91262C997" ); // Background Check (MinistrySafe):Review Result:Update Result
            RockMigrationHelper.UpdateWorkflowActionType( "5603DF2D-C429-4230-AEA1-3B6B1A19EF71", "Activate Complete", 4, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "", 1, "", "BD069F33-F6B2-4169-92F4-BA8F9885E4DB" ); // Background Check (MinistrySafe):Review Result:Activate Complete
            RockMigrationHelper.UpdateWorkflowActionType( "F4EC7AF1-4478-46DC-9C4B-D2B4924C9D3A", "Update Attribute Status", 0, "320622DA-52E0-41AE-AF90-2BF78B488552", true, false, "", "", 1, "", "40A3FF6C-D093-4F78-A245-8219D1A8CABE" ); // Background Check (MinistrySafe):Complete Request:Update Attribute Status
            RockMigrationHelper.UpdateWorkflowActionType( "F4EC7AF1-4478-46DC-9C4B-D2B4924C9D3A", "Background Check Passed", 1, "320622DA-52E0-41AE-AF90-2BF78B488552", true, false, "", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 1, "Pass", "CB472BFF-058D-4544-938C-13CA981CEE9F" ); // Background Check (MinistrySafe):Complete Request:Background Check Passed
            RockMigrationHelper.UpdateWorkflowActionType( "F4EC7AF1-4478-46DC-9C4B-D2B4924C9D3A", "Background Check Failed", 2, "320622DA-52E0-41AE-AF90-2BF78B488552", true, false, "", "7D033FD0-1232-43A9-98FD-1A2F1C6C453B", 1, "Fail", "0D38493F-A0E3-4777-9180-A4D81D7805AD" ); // Background Check (MinistrySafe):Complete Request:Background Check Failed
            RockMigrationHelper.UpdateWorkflowActionType( "F4EC7AF1-4478-46DC-9C4B-D2B4924C9D3A", "Notify Requester", 3, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "C1197EEF-0422-47BD-A68F-646C5CD09AC5" ); // Background Check (MinistrySafe):Complete Request:Notify Requester
            RockMigrationHelper.UpdateWorkflowActionType( "F4EC7AF1-4478-46DC-9C4B-D2B4924C9D3A", "Complete Workflow", 4, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "8B5CA90C-AFB5-4AB7-93D1-8C9BE3DFCCF9" ); // Background Check (MinistrySafe):Complete Request:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "AB8ED98A-20F9-4E74-912B-430DB21E069C", "Delete Workflow", 0, "0E79AF40-4FB0-49D7-AB0E-E95BD828C62D", true, false, "", "", 1, "", "B57F3271-B88F-4FA8-9DE5-69B03E11315C" ); // Background Check (MinistrySafe):Cancel Request:Delete Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "40BC73DD-5384-41D2-89C9-5D35FF1F6FFC", "36CE41F4-4C87-4096-B0C6-8269163BCC0A", @"False" ); // Background Check (MinistrySafe):Initial Request:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "40BC73DD-5384-41D2-89C9-5D35FF1F6FFC", "91A9F4BE-4A8E-430A-B466-A88DB2D33B34", @"Initial Entry" ); // Background Check (MinistrySafe):Initial Request:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue( "056FC534-70A6-4D5C-ADE4-E362A2B638DB", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Background Check (MinistrySafe):Initial Request:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "056FC534-70A6-4D5C-ADE4-E362A2B638DB", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"120910c9-516d-48b5-8ce6-0e665ba1138a" ); // Background Check (MinistrySafe):Initial Request:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "056FC534-70A6-4D5C-ADE4-E362A2B638DB", "DAE99CA3-E7B6-42A0-9E65-7A4C5029EEFC", @"True" ); // Background Check (MinistrySafe):Initial Request:Set Person:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "056FC534-70A6-4D5C-ADE4-E362A2B638DB", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"True" ); // Background Check (MinistrySafe):Initial Request:Set Person:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "AC4DE5F1-6F9A-48AB-B931-33D02E0BBD4D", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // Background Check (MinistrySafe):Initial Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AC4DE5F1-6F9A-48AB-B931-33D02E0BBD4D", "93852244-A667-4749-961A-D47F88675BE4", @"120910c9-516d-48b5-8ce6-0e665ba1138a" ); // Background Check (MinistrySafe):Initial Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "71FABDB9-6026-4746-9783-C07C2E11DC05", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Background Check (MinistrySafe):Initial Request:Set Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "71FABDB9-6026-4746-9783-C07C2E11DC05", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"6a1412f4-569a-4dfd-b477-bdb99644c024" ); // Background Check (MinistrySafe):Initial Request:Set Requester:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "25F89999-13E5-4458-B634-0A43CFF5FD74", "F3B9908B-096F-460B-8320-122CF046D1F9", @"SELECT ISNULL( (     SELECT          CASE WHEN DATEADD(year, 1, AV.[ValueAsDateTime]) > GETDATE() THEN 'True' ELSE 'False' END     FROM [AttributeValue] AV         INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]         INNER JOIN [PersonAlias] P ON P.[PersonId] = AV.[EntityId]     WHERE AV.[ValueAsDateTime] IS NOT NULL         AND A.[Guid] = '{{ Workflow | Attribute:'DateAttribute','RawValue' }}'         AND P.[Guid] = '{{ Workflow | Attribute:'Person','RawValue' }}' ), 'False')" ); // Background Check (MinistrySafe):Initial Request:Set Warning:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue( "25F89999-13E5-4458-B634-0A43CFF5FD74", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // Background Check (MinistrySafe):Initial Request:Set Warning:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "25F89999-13E5-4458-B634-0A43CFF5FD74", "56997192-2545-4EA1-B5B2-313B04588984", @"391e0db2-be6d-46a3-aac0-1f96b147864b" ); // Background Check (MinistrySafe):Initial Request:Set Warning:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "25F89999-13E5-4458-B634-0A43CFF5FD74", "D992DB0A-B528-4833-ADCE-61C5BD9BD156", @"False" ); // Background Check (MinistrySafe):Initial Request:Set Warning:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue( "F14042CB-7887-4449-9541-DE2E21FE5CE2", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Background Check (MinistrySafe):Initial Request:Get Details:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F35ACC51-7A1B-49A0-ACF0-EF20A34257EB", "36CE41F4-4C87-4096-B0C6-8269163BCC0A", @"False" ); // Background Check (MinistrySafe):Approve Request:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F35ACC51-7A1B-49A0-ACF0-EF20A34257EB", "91A9F4BE-4A8E-430A-B466-A88DB2D33B34", @"Waiting for Submit Approval" ); // Background Check (MinistrySafe):Approve Request:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue( "FDB827B1-1969-410C-8C98-CA14E2A85E6C", "27BAC9C8-2BF7-405A-AA01-845A3D374295", @"False" ); // Background Check (MinistrySafe):Approve Request:Assign to Security:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FDB827B1-1969-410C-8C98-CA14E2A85E6C", "D53823A1-28CB-4BA0-A24C-873ECF4079C5", @"a6bcc49e-103f-46b0-8bac-84ea03ff04d5" ); // Background Check (MinistrySafe):Approve Request:Assign to Security:Security Role
            RockMigrationHelper.AddActionTypeAttributeValue( "C0318396-A457-4213-85F3-14F15CBEE37A", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Background Check (MinistrySafe):Approve Request:Approve or Deny:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8142AE90-CDDB-4B66-9D75-63EE43FB2306", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Background Check (MinistrySafe):Approve Request:Set Approver:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8142AE90-CDDB-4B66-9D75-63EE43FB2306", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"11a06bd5-a023-4cf2-ae6b-767fe01fe594" ); // Background Check (MinistrySafe):Approve Request:Set Approver:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "37F8C070-3C7B-41B6-9C6D-53019AA957B1", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check (MinistrySafe):Approve Request:Submit Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "37F8C070-3C7B-41B6-9C6D-53019AA957B1", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"13548270-EF72-4B02-A980-AB6A74E3D56E" ); // Background Check (MinistrySafe):Approve Request:Submit Request:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "03BDE50C-B5E7-456D-B281-2C8D777863A9", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check (MinistrySafe):Approve Request:Deny Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "03BDE50C-B5E7-456D-B281-2C8D777863A9", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"18281C0D-07ED-4FF4-BE23-DD7B8264D9F4" ); // Background Check (MinistrySafe):Approve Request:Deny Request:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "53C9BE6C-0B25-4B78-B115-B93592D5FA6D", "36CE41F4-4C87-4096-B0C6-8269163BCC0A", @"False" ); // Background Check (MinistrySafe):Review Denial:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "53C9BE6C-0B25-4B78-B115-B93592D5FA6D", "91A9F4BE-4A8E-430A-B466-A88DB2D33B34", @"Waiting for More Details" ); // Background Check (MinistrySafe):Review Denial:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue( "3A44285B-7DDE-47D9-902E-B7D10D13DC6C", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // Background Check (MinistrySafe):Review Denial:Assign to Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3A44285B-7DDE-47D9-902E-B7D10D13DC6C", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"6a1412f4-569a-4dfd-b477-bdb99644c024" ); // Background Check (MinistrySafe):Review Denial:Assign to Requester:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "412BFD14-C2DE-4BFD-BE37-719541E71CD8", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Background Check (MinistrySafe):Review Denial:Review:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6E939331-805F-4216-A147-002B7165D3C1", "36CE41F4-4C87-4096-B0C6-8269163BCC0A", @"False" ); // Background Check (MinistrySafe):Submit Request:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6E939331-805F-4216-A147-002B7165D3C1", "91A9F4BE-4A8E-430A-B466-A88DB2D33B34", @"Waiting for Result" ); // Background Check (MinistrySafe):Submit Request:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue( "CD5C304C-70F6-4DC9-9606-72A156A36918", "6BEBD4BE-EDC7-4757-B597-445FC60DB6ED", @"False" ); // Background Check (MinistrySafe):Submit Request:Submit Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CD5C304C-70F6-4DC9-9606-72A156A36918", "6E2366B4-9F0E-454A-9DB1-E06263749C12", @"09d99031-93ea-442a-8c9e-e8e3ba664240" ); // Background Check (MinistrySafe):Submit Request:Submit Request:Background Check Provider
            RockMigrationHelper.AddActionTypeAttributeValue( "CD5C304C-70F6-4DC9-9606-72A156A36918", "077A9C4E-86E7-42F6-BEC3-DBC8F57E6A13", @"120910c9-516d-48b5-8ce6-0e665ba1138a" ); // Background Check (MinistrySafe):Submit Request:Submit Request:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "CD5C304C-70F6-4DC9-9606-72A156A36918", "EC759165-949E-4966-BAFD-68A656A4EBF7", @"63da7bf3-b290-4832-b7dd-e1f0d89f373f" ); // Background Check (MinistrySafe):Submit Request:Submit Request:Request Type Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "CD5C304C-70F6-4DC9-9606-72A156A36918", "232B2F98-3B2F-4C53-81FC-061A92675C41", @"4cc138f7-735f-4bc7-81af-e0a551446ac9" ); // Background Check (MinistrySafe):Submit Request:Submit Request:Billing Code Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D4DBDC65-1FB9-4629-99FA-7129D253FB6A", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check (MinistrySafe):Submit Request:Process Request Error:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D4DBDC65-1FB9-4629-99FA-7129D253FB6A", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"60644286-687A-4AB4-9C4F-A930DA3C155F" ); // Background Check (MinistrySafe):Submit Request:Process Request Error:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "62F06CCA-6091-41DD-9208-C6B82C176687", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check (MinistrySafe):Submit Request:Process Result:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "62F06CCA-6091-41DD-9208-C6B82C176687", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"CA4F5173-4F0B-4A61-8AA7-E1C05F1098E6" ); // Background Check (MinistrySafe):Submit Request:Process Result:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "3E3FDA77-9980-4EFA-AEEE-889A183D3990", "36CE41F4-4C87-4096-B0C6-8269163BCC0A", @"False" ); // Background Check (MinistrySafe):Request Error:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3E3FDA77-9980-4EFA-AEEE-889A183D3990", "91A9F4BE-4A8E-430A-B466-A88DB2D33B34", @"Request Error" ); // Background Check (MinistrySafe):Request Error:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue( "A0A4BE4C-B585-4A85-B9A5-B091438DEF31", "C0D75D1A-16C5-4786-A1E0-25669BEE8FE9", @"False" ); // Background Check (MinistrySafe):Request Error:Assign to Security:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A0A4BE4C-B585-4A85-B9A5-B091438DEF31", "BBFAD050-5968-4D11-8887-2FF877D8C8AB", @"aece949f-704c-483e-a4fb-93d5e4720c4c|a6bcc49e-103f-46b0-8bac-84ea03ff04d5" ); // Background Check (MinistrySafe):Request Error:Assign to Security:Group
            RockMigrationHelper.AddActionTypeAttributeValue( "A23E0089-DD5E-4310-9523-F45485CB69ED", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Background Check (MinistrySafe):Request Error:Display Error Message:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CB94B493-0B95-4328-AB3C-874180FF1FEA", "E5BAC4A6-FF7F-4016-BA9C-72D16CB60184", @"False" ); // Background Check (MinistrySafe):Process Result:Save Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CB94B493-0B95-4328-AB3C-874180FF1FEA", "E456FB6F-05DB-4826-A612-5B704BC4EA13", @"120910c9-516d-48b5-8ce6-0e665ba1138a" ); // Background Check (MinistrySafe):Process Result:Save Date:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "CB94B493-0B95-4328-AB3C-874180FF1FEA", "8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762", @"3daff000-7f74-47d7-8cb0-e4a4e6c81f5f" ); // Background Check (MinistrySafe):Process Result:Save Date:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "CB94B493-0B95-4328-AB3C-874180FF1FEA", "94689BDE-493E-4869-A614-2D54822D747C", @"{{ 'Now' | Date:'yyyy-MM-dd' }}T00:00:00" ); // Background Check (MinistrySafe):Process Result:Save Date:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "304E76E2-BA98-4B72-8E37-B91917179E5B", "E5BAC4A6-FF7F-4016-BA9C-72D16CB60184", @"False" ); // Background Check (MinistrySafe):Process Result:Save Report:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "304E76E2-BA98-4B72-8E37-B91917179E5B", "E456FB6F-05DB-4826-A612-5B704BC4EA13", @"120910c9-516d-48b5-8ce6-0e665ba1138a" ); // Background Check (MinistrySafe):Process Result:Save Report:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "304E76E2-BA98-4B72-8E37-B91917179E5B", "8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762", @"f3931952-460d-43e0-a6e0-eb6b5b1f9167" ); // Background Check (MinistrySafe):Process Result:Save Report:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "304E76E2-BA98-4B72-8E37-B91917179E5B", "94689BDE-493E-4869-A614-2D54822D747C", @"31c4ec7c-8dec-4305-a363-428d7b07c300" ); // Background Check (MinistrySafe):Process Result:Save Report:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F7F8C93A-3C3B-4C5F-8B4D-8BF2560205FC", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check (MinistrySafe):Process Result:Activate Review:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F7F8C93A-3C3B-4C5F-8B4D-8BF2560205FC", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"5603DF2D-C429-4230-AEA1-3B6B1A19EF71" ); // Background Check (MinistrySafe):Process Result:Activate Review:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "4C0F1458-9C90-4191-B807-DBA2A9328E62", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check (MinistrySafe):Process Result:Activate Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4C0F1458-9C90-4191-B807-DBA2A9328E62", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"F4EC7AF1-4478-46DC-9C4B-D2B4924C9D3A" ); // Background Check (MinistrySafe):Process Result:Activate Complete:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "6BA1B8F7-7EF2-4817-8E15-E98916084B94", "36CE41F4-4C87-4096-B0C6-8269163BCC0A", @"False" ); // Background Check (MinistrySafe):Review Result:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6BA1B8F7-7EF2-4817-8E15-E98916084B94", "91A9F4BE-4A8E-430A-B466-A88DB2D33B34", @"Waiting for Review" ); // Background Check (MinistrySafe):Review Result:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue( "E5014BE9-2DB5-4907-A575-E74E4C56E484", "27BAC9C8-2BF7-405A-AA01-845A3D374295", @"False" ); // Background Check (MinistrySafe):Review Result:Assign Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E5014BE9-2DB5-4907-A575-E74E4C56E484", "D53823A1-28CB-4BA0-A24C-873ECF4079C5", @"a6bcc49e-103f-46b0-8bac-84ea03ff04d5" ); // Background Check (MinistrySafe):Review Result:Assign Activity:Security Role
            RockMigrationHelper.AddActionTypeAttributeValue( "F4BB8142-B7F3-4246-A033-C6EE54BBF437", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Background Check (MinistrySafe):Review Result:Review Results:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "67ACE49E-0AA7-4883-852D-8BD91262C997", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Background Check (MinistrySafe):Review Result:Update Result:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "67ACE49E-0AA7-4883-852D-8BD91262C997", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"7d033fd0-1232-43a9-98fd-1a2f1c6c453b" ); // Background Check (MinistrySafe):Review Result:Update Result:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "67ACE49E-0AA7-4883-852D-8BD91262C997", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"2cc2084e-d717-42b6-b7f6-40de33ec92f5" ); // Background Check (MinistrySafe):Review Result:Update Result:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "BD069F33-F6B2-4169-92F4-BA8F9885E4DB", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check (MinistrySafe):Review Result:Activate Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BD069F33-F6B2-4169-92F4-BA8F9885E4DB", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"F4EC7AF1-4478-46DC-9C4B-D2B4924C9D3A" ); // Background Check (MinistrySafe):Review Result:Activate Complete:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "40A3FF6C-D093-4F78-A245-8219D1A8CABE", "E5BAC4A6-FF7F-4016-BA9C-72D16CB60184", @"False" ); // Background Check (MinistrySafe):Complete Request:Update Attribute Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "40A3FF6C-D093-4F78-A245-8219D1A8CABE", "E456FB6F-05DB-4826-A612-5B704BC4EA13", @"120910c9-516d-48b5-8ce6-0e665ba1138a" ); // Background Check (MinistrySafe):Complete Request:Update Attribute Status:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "40A3FF6C-D093-4F78-A245-8219D1A8CABE", "8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762", @"44490089-e02c-4e54-a456-454845abbc9d" ); // Background Check (MinistrySafe):Complete Request:Update Attribute Status:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "40A3FF6C-D093-4F78-A245-8219D1A8CABE", "94689BDE-493E-4869-A614-2D54822D747C", @"7d033fd0-1232-43a9-98fd-1a2f1c6c453b" ); // Background Check (MinistrySafe):Complete Request:Update Attribute Status:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "CB472BFF-058D-4544-938C-13CA981CEE9F", "E5BAC4A6-FF7F-4016-BA9C-72D16CB60184", @"False" ); // Background Check (MinistrySafe):Complete Request:Background Check Passed:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CB472BFF-058D-4544-938C-13CA981CEE9F", "E456FB6F-05DB-4826-A612-5B704BC4EA13", @"120910c9-516d-48b5-8ce6-0e665ba1138a" ); // Background Check (MinistrySafe):Complete Request:Background Check Passed:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "CB472BFF-058D-4544-938C-13CA981CEE9F", "8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762", @"daf87b87-3d1e-463d-a197-52227fe4ea28" ); // Background Check (MinistrySafe):Complete Request:Background Check Passed:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "CB472BFF-058D-4544-938C-13CA981CEE9F", "94689BDE-493E-4869-A614-2D54822D747C", @"True" ); // Background Check (MinistrySafe):Complete Request:Background Check Passed:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "0D38493F-A0E3-4777-9180-A4D81D7805AD", "E5BAC4A6-FF7F-4016-BA9C-72D16CB60184", @"False" ); // Background Check (MinistrySafe):Complete Request:Background Check Failed:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0D38493F-A0E3-4777-9180-A4D81D7805AD", "E456FB6F-05DB-4826-A612-5B704BC4EA13", @"120910c9-516d-48b5-8ce6-0e665ba1138a" ); // Background Check (MinistrySafe):Complete Request:Background Check Failed:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "0D38493F-A0E3-4777-9180-A4D81D7805AD", "8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762", @"daf87b87-3d1e-463d-a197-52227fe4ea28" ); // Background Check (MinistrySafe):Complete Request:Background Check Failed:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0D38493F-A0E3-4777-9180-A4D81D7805AD", "94689BDE-493E-4869-A614-2D54822D747C", @"False" ); // Background Check (MinistrySafe):Complete Request:Background Check Failed:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "C1197EEF-0422-47BD-A68F-646C5CD09AC5", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Background Check (MinistrySafe):Complete Request:Notify Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C1197EEF-0422-47BD-A68F-646C5CD09AC5", "0C4C13B8-7076-4872-925A-F950886B5E16", @"6a1412f4-569a-4dfd-b477-bdb99644c024" ); // Background Check (MinistrySafe):Complete Request:Notify Requester:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "C1197EEF-0422-47BD-A68F-646C5CD09AC5", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Background Check for {{ Workflow | Attribute:'Person' }}" ); // Background Check (MinistrySafe):Complete Request:Notify Requester:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "C1197EEF-0422-47BD-A68F-646C5CD09AC5", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}  <p>{{ Person.FirstName }},</p> <p>The background check for {{ Workflow | Attribute:'Person' }} has been completed.</p> <p>Result: {{ Workflow | Attribute:'ReportStatus' | Upcase }}<p/>  {{ 'Global' | Attribute:'EmailFooter' }}" ); // Background Check (MinistrySafe):Complete Request:Notify Requester:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "C1197EEF-0422-47BD-A68F-646C5CD09AC5", "51C1E5C2-2422-46DD-BA47-5D9E1308DC32", @"False" ); // Background Check (MinistrySafe):Complete Request:Notify Requester:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "8B5CA90C-AFB5-4AB7-93D1-8C9BE3DFCCF9", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Background Check (MinistrySafe):Complete Request:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8B5CA90C-AFB5-4AB7-93D1-8C9BE3DFCCF9", "3327286F-C1A9-4624-949D-33E9F9049356", @"Completed" ); // Background Check (MinistrySafe):Complete Request:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B57F3271-B88F-4FA8-9DE5-69B03E11315C", "361A1EC8-FFD0-4880-AF68-91DC0E0D7CDC", @"False" ); // Background Check (MinistrySafe):Cancel Request:Delete Workflow:Active

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'
		" );

            #endregion

            // Add Workflow to Action Dropdown
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "7197A0FB-B330-43C4-8E62-F3C14F649813", com.bemaservices.MinistrySafe.Constants.MinistrySafeSystemGuid.MINISTRYSAFE_BACKGROUNDCHECK_WORKFLOW_TYPE, true );

        }

        private void AddBackgroundCheckPage()
        {
            // Page: MinistrySafe Background Checks
            RockMigrationHelper.AddPage( "5C7EA1BE-FC79-4821-8FA3-759F8C65C87B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "MinistrySafe Background Checks", "", "05ECBD26-FE1E-4070-B6AA-B37850D4A65E", "fa fa-shield" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "MinistrySafe Request List", "Lists all the MinistrySafe background check requests.", "~/Plugins/com_bemaservices/MinistrySafe/MinistrySafeRequestList.ascx", "BEMA Services > MinistrySafe", "AFD471D9-E8F7-487F-AB1F-800D5087A055" );
            RockMigrationHelper.UpdateBlockType( "MinistrySafe Settings", "Block for updating the settings used by the MinistrySafe integration.", "~/Plugins/com_bemaservices/MinistrySafe/MinistrySafeSettings.ascx", "BEMA Services > MinistrySafe", "A095E83E-F280-4ADB-B11D-C30CCB295FF3" );
            // Add Block to Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "05ECBD26-FE1E-4070-B6AA-B37850D4A65E", "", "A095E83E-F280-4ADB-B11D-C30CCB295FF3", "MinistrySafe Settings", "Main", "", "", 0, "A73FA4B5-8D1E-46DF-A4B3-4E57CFEC4088" );
            // Add Block to Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "05ECBD26-FE1E-4070-B6AA-B37850D4A65E", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Main", "", "", 1, "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C" );
            // Add Block to Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "05ECBD26-FE1E-4070-B6AA-B37850D4A65E", "", "AFD471D9-E8F7-487F-AB1F-800D5087A055", "MinistrySafe Request List", "Main", "", "", 2, "C74060F9-042C-447C-B034-FC5B8C17E825" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: MinistrySafe Request List:Workflow Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "AFD471D9-E8F7-487F-AB1F-800D5087A055", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "", "The page to view details about the background check workflow", 0, @"", "766A628E-9377-4BA6-BA47-6CB8E633405C" );
            // Attrib for BlockType: MinistrySafe Request List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "AFD471D9-E8F7-487F-AB1F-800D5087A055", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "1A770398-BC1F-4A9F-A10C-C0678596415D" );
            // Attrib for BlockType: MinistrySafe Request List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "AFD471D9-E8F7-487F-AB1F-800D5087A055", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "81C00726-FD6D-4A5C-AD4A-21630C8693E7" );
            // Attrib for BlockType: MinistrySafe Request List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "AFD471D9-E8F7-487F-AB1F-800D5087A055", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "D262B482-9FBE-4BA1-9827-D2AE0BDECCC5" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", "Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", "The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", "The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", "Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", "Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", "Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", "Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", "If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", "Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", "Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:HTML Content, Attribute:Cache Duration Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:HTML Content, Attribute:Require Approval Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Enable Versioning Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Start in Code Editor mode Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:HTML Content, Attribute:Image Root Folder Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:User Specific Folders Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Document Root Folder Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:Enabled Lava Commands Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:HTML Content, Attribute:Is Secondary Block Page: MinistrySafe Background Checks, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            RockMigrationHelper.UpdateHtmlContentBlock( "C35C1CC6-CEEA-4854-B4D3-A1E0B30C727C", "{% include '~/Plugins/com_bemaservices/MinistrySafe/Assets/Lava/MinistrySafeTabs.lava' %}", Guid.NewGuid().ToString() );
        }

        private void UpdateMinistrySafePage()
        {
            RockMigrationHelper.DeletePage( "5C7EA1BE-FC79-4821-8FA3-759F8C65C87B" );
            // Page: MinistrySafe
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "MinistrySafe", "", "5C7EA1BE-FC79-4821-8FA3-759F8C65C87B", "fa fa-shield" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "MinistrySafe Training List", "Lists all the MinistrySafe background check requests.", "~/Plugins/com_bemaservices/MinistrySafe/MinistrySafeTrainingList.ascx", "BEMA Services > MinistrySafe", "D8BF3D63-C063-4A13-A22C-4AD7C3B331B3" );
            RockMigrationHelper.UpdateBlockType( "MinistrySafe Settings", "Block for updating the settings used by the MinistrySafe integration.", "~/Plugins/com_bemaservices/MinistrySafe/MinistrySafeSettings.ascx", "BEMA Services > MinistrySafe", "A095E83E-F280-4ADB-B11D-C30CCB295FF3" );
            // Add Block to Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "5C7EA1BE-FC79-4821-8FA3-759F8C65C87B", "", "A095E83E-F280-4ADB-B11D-C30CCB295FF3", "MinistrySafe Settings", "Main", "", "", 0, "393F4A1E-ADE1-4663-BA56-09886470AEEF" );
            // Add Block to Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "5C7EA1BE-FC79-4821-8FA3-759F8C65C87B", "", "D8BF3D63-C063-4A13-A22C-4AD7C3B331B3", "User List", "Main", "", "", 2, "8DFAB407-2F76-41E7-B19C-63F4E5DF9BD3" );
            // Add Block to Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "5C7EA1BE-FC79-4821-8FA3-759F8C65C87B", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Main", "", "", 1, "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: MinistrySafe Training List:Workflow Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "D8BF3D63-C063-4A13-A22C-4AD7C3B331B3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "", "The page to view details about the MinistrySafe workflow", 0, @"", "1AED9A04-2926-49CF-A986-C3EA98C4357C" );
            // Attrib for BlockType: MinistrySafe Training List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "D8BF3D63-C063-4A13-A22C-4AD7C3B331B3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "350C7846-2103-47D8-A902-564E6FB86E04" );
            // Attrib for BlockType: MinistrySafe Training List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "D8BF3D63-C063-4A13-A22C-4AD7C3B331B3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "EEC0A72A-0334-4240-BFA4-A6472AC1B85B" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", "Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", "The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", "The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", "Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", "Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", "Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", "Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", "If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", "Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", "Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:HTML Content, Attribute:Cache Duration Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:HTML Content, Attribute:Require Approval Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Enable Versioning Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Start in Code Editor mode Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:HTML Content, Attribute:Image Root Folder Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:User Specific Folders Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Document Root Folder Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:Enabled Lava Commands Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:HTML Content, Attribute:Is Secondary Block Page: MinistrySafe, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            RockMigrationHelper.UpdateHtmlContentBlock( "E7759BE5-B196-41D2-B2FA-D5C062BE6F9D", "{% include '~/Plugins/com_bemaservices/MinistrySafe/Assets/Lava/MinistrySafeTabs.lava' %}", Guid.NewGuid().ToString() );
        }

        private void UpdateTerminology()
        {
            // Page: MinistrySafe
            Sql( @"
                Update BlockType
                Set Path = '~/Plugins/com_bemaservices/MinistrySafe/MinistrySafeTrainingList.ascx',
                    Name = 'MinistrySafe Training List',
                    Description = 'Lists all the MinistrySafe background check requests.'
                Where Path = '~/Plugins/com_bemaservices/MinistrySafe/UserList.ascx'
                " );
            Sql( @"
                    Update Attribute
                    Set Name = 'MinistrySafe API Token'
                    Where [Guid] = 'C3F952C7-F515-4950-B0EB-8737A013CD85'
                " );

            RockMigrationHelper.UpdatePersonBadge( "MinistrySafe Awareness Training Badge", "Shows whether someone has taken a MinistrySafe Awareness Training, as well as their score.", "Rock.PersonProfile.Badge.Liquid", 0, "9E9B9FAF-C7B8-40AA-B0C9-24177058943B" );
            RockMigrationHelper.AddDefinedType( "Global", "MinistrySafe Training Types", "", "95EF81D2-C192-4B9E-A7A3-5E1E90BDA3CE" );
            RockMigrationHelper.AddDefinedType( "Global", "MinistrySafe User Types", "", "559E79C6-2EAB-4A0D-A16F-59D9B63F002F" );
            RockMigrationHelper.UpdatePersonAttributeCategory( "MinistrySafe Awareness Training", "fa fa-lock", "", "CB481AB7-E0F9-4A3E-B846-0F5E5C94C038" );

            Sql( @"Update [Group]
                    Set Name = 'RSR - MinistrySafe Awareness Training Administration',
                        Description = 'The group of people responsible for approving MinistrySafe Awareness Trainings.'
                    Where [Guid] = '22AD73FD-B267-49C3-ABF3-DF8805898E9C'
                    " );

            RockMigrationHelper.UpdateWorkflowType( false, true, "Awareness Training (MinistrySafe)", "Used to request a MinistrySafe Awareness Training be assigned to a person.", "6F8A431C-BEBD-4D33-AAD6-1D70870329C2", "Request", "fa fa-lock", 0, true, 0, "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", 0 );

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Checked Attribute", "CheckedAttribute", "The person attribute that indicates if person has a valid MinistrySafe Training (passed)", 0, @"c19f3842-7cee-4772-b2ed-86b7968e2879", "29786696-A0D2-4930-B0F0-62D287E3B188", false );
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Date Attribute", "DateAttribute", "The person attribute the stores the date MinistrySafe Training was completed", 3, @"0b1607af-6900-406c-8f7f-8dc03fc253f3", "84194FA3-B825-4552-9BBD-B19EDCBD4C6B", false ); // MinistrySafe Training Request:Date Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Result Attribute", "ResultAttribute", "The person attribute that stores the MinistrySafe Training Result", 4, @"c19f3842-7cee-4772-b2ed-86b7968e2879", "5E7EB6E8-8C37-4BBC-8504-BB06144EEDB6", false ); // MinistrySafe Training Request:Result Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Score Attribute", "ScoreAttribute", "The person attribute that stores the MinistrySafe Training Score", 5, @"937c7d10-74dd-4512-9e0d-79b5b989deb7", "2B4133F0-1D2C-468E-9D74-E1F655D3310B", false ); // MinistrySafe Training Request:Score Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Survey Type", "SurveyType", "Value should be the type of MinistrySafe training to request from the vendor.", 9, @"c8e8e22a-1d27-4179-af30-313d2ee896ea", "FC5D6AD1-5003-4E75-B297-59675444113A", false ); // MinistrySafe Training Request:Survey Type
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Reason", "Reason", "A brief description of the reason that a MinistrySafe training is being requested", 10, @"", "7A92E2A0-8445-4D6D-8DC5-EBF4A1D37D26", false ); // MinistrySafe Training Request:Reason

            RockMigrationHelper.UpdateWorkflowActivityType( "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", true, "Request Error", "Displays any error from the request that is submitted to MinistrySafe and allows request to be resubmitted", false, 4, "0416F60D-3A97-4FD6-9A94-472935BDE9D5" ); // MinistrySafe Training Request:Request Error
            RockMigrationHelper.UpdateWorkflowActivityType( "5876314A-FC4F-4A07-8CA0-A02DE26E55BE", true, "Process Result", "Evaluates the result of the MinistrySafe Training received from the provider", false, 5, "4DB58714-21A9-44D5-8A4C-521AC11D4688" ); // MinistrySafe Training Request:Process Result
            RockMigrationHelper.UpdateWorkflowActionForm( @"{% assign WarnOfRecent = Workflow | Attribute:'WarnOfRecent' %}<h1>MinistrySafe Training Request Details</h1> <p>     {{CurrentPerson.NickName}}, please complete the form below to start the MinistrySafe Training process. </p> {% if WarnOfRecent == 'Yes' %}     <div class='alert alert-warning'>         Notice: It's been less than two years since this person's last MinistrySafe Training was completed.         Please make sure you want to continue with this request!     </div> {% endif %} <hr />", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^51383f95-d9b7-4d61-87c1-97da66015baa^Your request has been submitted successfully.|Cancel^5683E775-B9F3-408C-80AC-94DE0E51CF3A^748423b3-508b-4fdf-a200-3f3e86bf9182^The request has been canceled.", "", false, "", "4F8E538D-4E57-4C5A-A58C-B39C8AB7E357" ); // MinistrySafe Training Request:Initial Request:Get Details
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>MinistrySafe Training Request Details</h1> <div class='alert alert-info'>     {{CurrentPerson.NickName}}, the following MinistrySafe Training request has been submitted for your review.     If you approve the request it will be sent to the MinistrySafe for processing. If you     deny the request, it will be sent back to the requester. If you deny the request, please add notes     explaining why the request was denied. </div>", @"", "Approve^c88fef94-95b9-444a-bc93-58e983f3c047^^The request has been submitted to provider for processing.|Deny^d6b809a9-c1cc-4ebb-816e-33d8c1e53ea4^^The requester will be notified that this request has been denied (along with the reason why).", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "7C5DD393-8926-43E9-A75E-8AFB3D9B110A", "9D559BAA-EB64-49C9-A2C7-E197AB961373" ); // MinistrySafe Training Request:Approve Request:Approve or Deny
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>MinistrySafe Training Request Details</h1> <p> {{CurrentPerson.NickName}}, this request has come back from the approval process with the following results. </p> <div class=""well"">     <strong>Summary of Security Notes:</strong>     <br />     <table class=""table table-condensed table-light margin-b-md"">         {% for activity in Workflow.Activities %}             {% if activity.ActivityType.Name == 'Approve Request' %}                 <tr>                     <td width=""220"">{{activity.CompletedDateTime}}</td>                     <td width=""220"">{{activity | Attribute:'Approver'}}</td>                     <td>{{activity | Attribute:'Note'}}</td>                 </tr>             {% endif %}         {% endfor %}     </table> </div> <hr />", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^51383f95-d9b7-4d61-87c1-97da66015baa^The request has been submitted again to the security team for approval.|Cancel Request^5683E775-B9F3-408C-80AC-94DE0E51CF3A^748423b3-508b-4fdf-a200-3f3e86bf9182^The request has been canceled.", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "CDA94CFF-7142-4176-81B1-D2054A6A9EA3" ); // MinistrySafe Training Request:Review Denial:Review
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>MinistrySafe Training Request Details</h1> <div class='alert alert-danger'>     An error occurred when submitting the MinistrySafe Training request. See details below. </div>", @"", "Re-Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^a44f8ad7-24b2-41ad-a627-f3fa46cbc9b5^Your information has been submitted successfully.", "", true, "", "6016538C-A8EE-420B-B6D6-27E7CCFA1498" ); // MinistrySafe Training Request:Request Error:Display Error Message
            RockMigrationHelper.UpdateWorkflowActionType( "A44F8AD7-24B2-41AD-A627-F3FA46CBC9B5", "Set MinistrySafe Result", 3, "320622DA-52E0-41AE-AF90-2BF78B488552", true, false, "", "", 1, "", "C619B763-FB10-4D3B-9A31-A1CB5CCC3D37" );

            Sql( @"
                    Update Block
                    Set Name = 'MinistrySafe Awareness Training'
                    Where [Guid] = '980731DB-3271-420E-A258-CECC9E7DFE77'
                " );

        }

        public override void Down()
        {
        }
    }
}

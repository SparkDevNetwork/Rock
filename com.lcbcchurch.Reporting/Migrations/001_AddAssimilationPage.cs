// <copyright>
// Copyright by Central Christian Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    public class CreateDb : Migration
    {
        public override void Up()
        {
            // Page: Assimilation Report
            RockMigrationHelper.AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Assimilation Report", "", "82A4A145-4E72-41DA-AA05-E4B31A3290FF", "fa fa-file-invoice" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Page Parameter Filter", "A collection of filters that will pass its values as page parameters.", "~/Plugins/com_bemadev/Cms/PageParameterFilter.ascx", "com_bemadev > Cms", "FE667841-D12D-4F12-A171-A3855A7CE254" );
            RockMigrationHelper.UpdateBlockType( "Custom Dynamic Data", "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "~/Plugins/com_bemadev/Reporting/CustomDynamicData.ascx.ascx", "com_bemadev > Reporting", "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563" );
            // Add Block to Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "82A4A145-4E72-41DA-AA05-E4B31A3290FF", "", "FE667841-D12D-4F12-A171-A3855A7CE254", "Page Parameter Filter", "Main", "", "", 0, "DFB36917-0D7D-49B5-BF62-670A7F80E281" );
            // Add Block to Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "82A4A145-4E72-41DA-AA05-E4B31A3290FF", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Dynamic Data", "Feature", "", "", 0, "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3" );
            // Add Block to Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "82A4A145-4E72-41DA-AA05-E4B31A3290FF", "", "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "First Time Guests", "Main", "", "", 2, "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248" );
            // Attrib for BlockType: Custom Dynamic Data:Communication Recipient Person Id Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Communication Recipient Person Id Columns", "CommunicationRecipientPersonIdColumns", "", "Columns that contain a communication recipient person id.", 0, @"", "9EA298E8-D2E1-4111-BE77-2B6BD96EB6F8" );
            // Attrib for BlockType: Custom Dynamic Data:Stored Procedure
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Stored Procedure", "StoredProcedure", "", "Is the query a stored procedure?", 0, @"False", "01DD1794-354A-458F-AFAE-E6A5C77C044A" );
            // Attrib for BlockType: Custom Dynamic Data:Url Mask
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Url Mask", "UrlMask", "", "The Url to redirect to when a row is clicked", 0, @"", "4F942092-6859-4C44-BFDB-161A17C0D91F" );
            // Attrib for BlockType: Custom Dynamic Data:Show Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Columns", "ShowColumns", "", "Should the 'Columns' specified below be the only ones shown (vs. the only ones hidden)", 0, @"False", "302282EB-D2E2-4CC3-AA5E-47679BC7EA16" );
            // Attrib for BlockType: Custom Dynamic Data:Query Params
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Query Params", "QueryParams", "", "Parameters to pass to query", 0, @"", "4C2D28DF-C935-4250-80DC-320E741D23C8" );
            // Attrib for BlockType: Custom Dynamic Data:Formatted Output
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Formatted Output", "FormattedOutput", "", "Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}", 0, @"", "F55EB0F9-E38C-496F-878B-E25C7706D575" );
            // Attrib for BlockType: Custom Dynamic Data:Person Report
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Person Report", "PersonReport", "", "Is this report a list of people?", 0, @"False", "A7A0F7F0-E51F-446C-86A2-37E6FA974EBF" );
            // Attrib for BlockType: Custom Dynamic Data:Show Communicate
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Communicate", "ShowCommunicate", "", "Show Communicate button in grid footer?", 0, @"True", "5E2A78AE-DC9C-4A2C-8926-61E657E0BD2A" );
            // Attrib for BlockType: Custom Dynamic Data:Show Merge Person
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Merge Person", "ShowMergePerson", "", "Show Merge Person button in grid footer?", 0, @"True", "E416578C-E0D8-4C32-9E08-9998B7773052" );
            // Attrib for BlockType: Custom Dynamic Data:Show Bulk Update
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Bulk Update", "ShowBulkUpdate", "", "Show Bulk Update button in grid footer?", 0, @"True", "3D47F2DA-B48B-4F98-85BA-9504AC59A1F4" );
            // Attrib for BlockType: Custom Dynamic Data:Show Excel Export
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Excel Export", "ShowExcelExport", "", "Show Export to Excel button in grid footer?", 0, @"True", "3F8B96ED-8D77-4263-A20A-69BBA936BA60" );
            // Attrib for BlockType: Custom Dynamic Data:Show Merge Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Merge Template", "ShowMergeTemplate", "", "Show Export to Merge Template button in grid footer?", 0, @"True", "7FEC7FCC-766A-4743-8E22-CB31A723E32F" );
            // Attrib for BlockType: Custom Dynamic Data:Timeout
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Timeout", "Timeout", "", "The amount of time in xxx to allow the query to run before timing out.", 0, @"30", "63CB323E-2DC6-4D78-BEFC-4CBE2B560C58" );
            // Attrib for BlockType: Custom Dynamic Data:Merge Fields
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Merge Fields", "MergeFields", "", "Any fields to make available as merge fields for any new communications", 0, @"", "54F008E3-A974-46FC-9A00-B64843056D3A" );
            // Attrib for BlockType: Custom Dynamic Data:Encrypted Fields
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Encrypted Fields", "EncryptedFields", "", "Any fields that need to be decrypted before displaying their value", 0, @"", "70E610B9-141A-4684-BA47-B85E1284408B" );
            // Attrib for BlockType: Custom Dynamic Data:Page Title Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Page Title Lava", "PageTitleLava", "", "Optional Lava for setting the page title. If nothing is provided then the page's title will be used.", 0, @"", "95523AD5-5591-4503-AB04-7E7B73FB6184" );
            // Attrib for BlockType: Custom Dynamic Data:Paneled Grid
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Paneled Grid", "PaneledGrid", "", "Add the 'grid-panel' class to the grid to allow it to fit nicely in a block.", 0, @"False", "33075F20-0C75-4749-9A58-EEF34B6CBCB8" );
            // Attrib for BlockType: Custom Dynamic Data:Show Grid Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grid Filter", "ShowGridFilter", "", "Show filtering controls that are dynamically generated to match the columns of the dynamic data.", 0, @"True", "19F773D5-508D-40E2-9D27-C58A1755EA33" );
            // Attrib for BlockType: Custom Dynamic Data:Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Columns", "Columns", "", "The columns to hide or show", 0, @"", "740987A0-3BD1-43A9-862F-FE353697E1F8" );
            // Attrib for BlockType: Custom Dynamic Data:Update Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Update Page", "UpdatePage", "", "If True, provides fields for updating the parent page's Name and Description", 0, @"True", "09FB9660-3B33-44B2-8CB5-FED37D241B6F" );
            // Attrib for BlockType: Custom Dynamic Data:Query
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Query", "Query", "", "The query to execute. Note that if you are providing SQL you can add items from the query string using Lava like {{ QueryParmName }}.", 0, @"", "CBE194E9-4F10-4505-93DE-31EABA918E85" );
            // Attrib for BlockType: Page Parameter Filter:Boolean Select
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Boolean Select", "BooleanSelect", "", "", 1, @"", "B29D445D-4447-4137-A33F-FF0C63A6E813" );
            // Attrib for BlockType: Page Parameter Filter:Multi Select Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Multi Select Label", "MultiSelectLabel", "", "", 1, @"", "32CA03D1-7DB7-4922-B426-3131D91F96AC" );
            // Attrib for BlockType: Page Parameter Filter:Date Range Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Date Range Label", "DateRangeLabel", "", "", 1, @"", "F4D72961-F212-4D49-956F-CE42C3B86B8F" );
            // Attrib for BlockType: Page Parameter Filter:Number Range Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Number Range Label", "NumberRangeLabel", "", "", 1, @"", "AACC30CA-06F8-426B-8DBB-43301A5E5122" );
            // Attrib for BlockType: Page Parameter Filter:Enable Campuses Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campuses Filter", "EnableCampusesFilter", "", "Enables the campus filter.", 1, @"False", "204FB5CA-4650-4D82-8E55-035AA7C73304" );
            // Attrib for BlockType: Page Parameter Filter:Page Redirect
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Page Redirect", "PageRedirect", "", "If set, the filter button will redirect to the selected page.", 1, @"", "41B4D32D-ED66-402D-8B3C-FC5DCE53DBA8" );
            // Attrib for BlockType: Page Parameter Filter:Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Button Text", "ButtonText", "", "Text that will be displayed on \"GO\" button", 2, @"Filter", "9AE98186-E466-4196-A272-31852322A4BA" );
            // Attrib for BlockType: Page Parameter Filter:Boolean Select 2
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Boolean Select 2", "BooleanSelect2", "", "", 2, @"", "B3BEA2E6-31F9-4615-B452-681F3887023B" );
            // Attrib for BlockType: Page Parameter Filter:Enable Account Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Account Filter", "EnableAccountFilter", "", "Enables the account filter.", 2, @"False", "E7DF5865-7973-45AF-A090-9D28E9747B7F" );
            // Attrib for BlockType: Page Parameter Filter:Number Range Label 2
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Number Range Label 2", "NumberRangeLabel2", "", "", 2, @"", "20102155-99D0-484F-86A1-39262994EBCF" );
            // Attrib for BlockType: Page Parameter Filter:Date Range Label 2
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Date Range Label 2", "DateRangeLabel2", "", "", 2, @"", "BEAB0FC4-AB78-4E65-9157-D31390F531A1" );
            // Attrib for BlockType: Page Parameter Filter:Multi Select List
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Multi Select List", "MultiSelectList", "", "", 2, @"", "A5EAFA44-1844-41A6-A359-5E19BEF3249F" );
            // Attrib for BlockType: Page Parameter Filter:Time Range Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Range Label", "TimeRangeLabel", "", "", 2, @"", "D4570AD0-97D6-40D2-A511-F44F0B34F7F6" );
            // Attrib for BlockType: Page Parameter Filter:Multi Select Label 2
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Multi Select Label 2", "MultiSelectLabel2", "", "", 3, @"", "9172F1C2-932B-4F06-91D8-F1A254862A3E" );
            // Attrib for BlockType: Page Parameter Filter:Date Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Date Label", "DateLabel", "", "", 3, @"", "9E3699DA-DABC-4D8F-BB03-30F7E336BC22" );
            // Attrib for BlockType: Page Parameter Filter:Block Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Text", "BlockText", "", "Text that will be displayed as the Block name", 3, @"Filter", "86EFBF55-2E2F-42BD-AEEF-34E9769AE44F" );
            // Attrib for BlockType: Page Parameter Filter:Text Entry
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Text Entry", "TextEntry", "", "", 3, @"", "552A47D7-A6BE-4634-A1AC-C947147AE6CA" );
            // Attrib for BlockType: Page Parameter Filter:Enable Person Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Filter", "EnablePersonFilter", "", "Enables the person filter.", 3, @"False", "60F58D52-1C66-45C6-B3B9-85A213A53EEF" );
            // Attrib for BlockType: Page Parameter Filter:Number Range Label 3
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Number Range Label 3", "NumberRangeLabel3", "", "", 3, @"", "3D33B8AC-BFBD-4498-B8A1-882BBEB64BE2" );
            // Attrib for BlockType: Page Parameter Filter:Enable Groups Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Groups Filter", "EnableGroupsFilter", "", "Enables the groups filter.", 4, @"False", "AA948207-54F5-4CCC-AB4B-4B3582C7C4D1" );
            // Attrib for BlockType: Page Parameter Filter:Multi Select List 2
            RockMigrationHelper.UpdateBlockTypeAttribute( "FE667841-D12D-4F12-A171-A3855A7CE254", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Multi Select List 2", "MultiSelectList2", "", "", 4, @"", "1512BB91-F120-409A-A5D9-3F912911AD60" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Date Range Label Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "F4D72961-F212-4D49-956F-CE42C3B86B8F", @"Date Range" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Multi Select Label Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "32CA03D1-7DB7-4922-B426-3131D91F96AC", @"Visitors" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Number Range Label Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "AACC30CA-06F8-426B-8DBB-43301A5E5122", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Enable Campuses Filter Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "204FB5CA-4650-4D82-8E55-035AA7C73304", @"True" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Page Redirect Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "41B4D32D-ED66-402D-8B3C-FC5DCE53DBA8", @"0" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Boolean Select Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "B29D445D-4447-4137-A33F-FF0C63A6E813", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Date Range Label 2 Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "BEAB0FC4-AB78-4E65-9157-D31390F531A1", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Multi Select List Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "A5EAFA44-1844-41A6-A359-5E19BEF3249F", @"First Time Visitors^1|Second Time Visitors^2|Third Time Visitors^3" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Number Range Label 2 Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "20102155-99D0-484F-86A1-39262994EBCF", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Time Range Label Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "D4570AD0-97D6-40D2-A511-F44F0B34F7F6", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Button Text Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "9AE98186-E466-4196-A272-31852322A4BA", @"Filter" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Boolean Select 2 Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "B3BEA2E6-31F9-4615-B452-681F3887023B", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Enable Account Filter Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "E7DF5865-7973-45AF-A090-9D28E9747B7F", @"False" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Number Range Label 3 Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "3D33B8AC-BFBD-4498-B8A1-882BBEB64BE2", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Date Label Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "9E3699DA-DABC-4D8F-BB03-30F7E336BC22", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Multi Select Label 2 Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "9172F1C2-932B-4F06-91D8-F1A254862A3E", @"Age" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Text Entry Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "552A47D7-A6BE-4634-A1AC-C947147AE6CA", @"" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Enable Person Filter Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "60F58D52-1C66-45C6-B3B9-85A213A53EEF", @"False" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Block Text Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "86EFBF55-2E2F-42BD-AEEF-34E9769AE44F", @"Filter" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Multi Select List 2 Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "1512BB91-F120-409A-A5D9-3F912911AD60", @"Child^Child|Youth^Youth|Adult^Adult" );
            // Attrib Value for Block:Page Parameter Filter, Attribute:Enable Groups Filter Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFB36917-0D7D-49B5-BF62-670A7F80E281", "AA948207-54F5-4CCC-AB4B-4B3582C7C4D1", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Merge Fields Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "8EB882CE-5BB1-4844-9C28-10190903EECD", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Formatted Output Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "6A233402-446C-47E9-94A5-6A247C29BC21", @"{% for row in rows %}     {% assign numberArray = row.VisitorNumber | Split:',' %}     {% assign firstNumber = numberArray | First %}     {% assign visitorNumber = '' %}     {% for number in numberArray %}         {% if number != firstNumber %}             {% assign visitorNumber = visitorNumber | Append:', ' %}         {% endif %}                  {% assign numberString = number | NumberToOrdinal %}         {% assign visitorNumber = visitorNumber | Append:numberString %}     {% endfor %}          {% assign visitorNumber = visitorNumber | ReplaceLast:',',' and' %}     <h1>{{visitorNumber}} Time Vistors for {{row.StartDate | Date:'MMM d'}} - {{row.EndDate | DateAdd:-1 | Date:'MMM d'}}</h1> {% endfor %}" );
            // Attrib Value for Block:Dynamic Data, Attribute:Person Report Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Communication Recipient Person Id Columns Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "75DDB977-9E71-44E8-924B-27134659D3A4", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Excel Export Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "E11B57E5-EC7D-4C42-9ADA-37594D71F145", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Communicate Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "5B2C115A-C187-4AB3-93AE-7010644B39DA", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Person Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Bulk Update Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Stored Procedure Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "A4439703-5432-489A-9C14-155903D6A43E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Template Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "6697B0A2-C8FE-497A-B5B4-A9D459474338", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Paneled Grid Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "5449CB61-2DFC-4B55-A697-38F1C2AF128B", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Grid Filter Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Timeout Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "BEEE38DD-2791-4242-84B6-0495904143CC", @"30" );
            // Attrib Value for Block:Dynamic Data, Attribute:Page Title Lava Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "3F4BA170-F5C5-405E-976F-0AFBB8855FE8", @"{% for row in rows %}     {% assign numberArray = row.VisitorNumber | Split:',' %}     {% assign firstNumber = numberArray | First %}     {% assign visitorNumber = '' %}     {% for number in numberArray %}         {% if number != firstNumber %}             {% assign visitorNumber = visitorNumber | Append:', ' %}         {% endif %}                  {% assign numberString = number | NumberToOrdinal %}         {% assign visitorNumber = visitorNumber | Append:numberString %}     {% endfor %}          {% assign visitorNumber = visitorNumber | ReplaceLast:',',' and' %}     {{visitorNumber}} Time Vistors for {{row.StartDate | Date:'MMM d'}} - {{row.EndDate | DateAdd:-1 | Date:'MMM d'}} {% endfor %}" );
            // Attrib Value for Block:Dynamic Data, Attribute:Update Page Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "230EDFE8-33CA-478D-8C9A-572323AF3466", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Params Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Columns Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "90B0E6AF-B2F4-4397-953B-737A40D4023B", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"
Declare @CampusIds nvarchar(max) = 'null'
Declare @AttendanceNumbers nvarchar(max) = '1'

{% assign queryParms = 'Global' | Page:'QueryString' %}

{% for item in queryParms %}
    {% assign kvItem = item | PropertyToKeyValue %}  

	{% if kvItem.Key == 'DateRange' %}
        {% if kvItem.Value != '' %}
            {% assign DateRange = kvItem.Value | Split:',' %}
        {% endif %}
    {% endif %} 

    {% if kvItem.Key == 'CampusIds' %}
        {% if kvItem.Value != ',' %}
            {% assign CampusIds = kvItem.Value %}
        {% endif %}
    {% endif %}
    
    {% if kvItem.Key == 'Visitors' %}
        {% if kvItem.Value != ',' %}
            {% assign VisitorString = kvItem.Value %}
        {% endif %}
    {% endif %}
    
{% endfor %}

{% if CampusIds != null %}
Set @CampusIds = '{{CampusIds}}'
{% endif %}

{% if VisitorString != null %}
Set @AttendanceNumbers = '{{VisitorString}}'
{% endif %}

Declare @EndDateTime datetime =DateAdd(day, 1, [dbo].ufnUtility_GetPreviousSundayDate())
Declare @StartDateTime datetime = DateAdd(day, -7, @EndDateTime)

{% if DateRange[0] != null %}
Set @StartDateTime = '{{ DateRange[0] }} 12:00 AM'
{% endif %}

{% if DateRange[1] != null %}
Set @EndDateTime = DateAdd(day, 1,'{{ DateRange[1] }} 12:00 AM')
{% endif %}
Select @StartDateTime as 'StartDate', @EndDateTime as 'EndDate', @AttendanceNumbers as 'VisitorNumber'" );
            // Attrib Value for Block:Dynamic Data, Attribute:Url Mask Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "B9163A35-E09C-466D-8A2D-4ED81DF0114C", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Columns Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3", "202A82BF-7772-481C-8419-600012607972", @"False" );
            // Attrib Value for Block:First Time Guests, Attribute:Communication Recipient Person Id Columns Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "9EA298E8-D2E1-4111-BE77-2B6BD96EB6F8", @"" );
            // Attrib Value for Block:First Time Guests, Attribute:Stored Procedure Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "01DD1794-354A-458F-AFAE-E6A5C77C044A", @"False" );
            // Attrib Value for Block:First Time Guests, Attribute:Url Mask Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "4F942092-6859-4C44-BFDB-161A17C0D91F", @"~/Person/{Id}" );
            // Attrib Value for Block:First Time Guests, Attribute:Show Columns Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "302282EB-D2E2-4CC3-AA5E-47679BC7EA16", @"False" );
            // Attrib Value for Block:First Time Guests, Attribute:Query Params Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "4C2D28DF-C935-4250-80DC-320E741D23C8", @"" );
            // Attrib Value for Block:First Time Guests, Attribute:Formatted Output Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "F55EB0F9-E38C-496F-878B-E25C7706D575", @"" );
            // Attrib Value for Block:First Time Guests, Attribute:Person Report Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "A7A0F7F0-E51F-446C-86A2-37E6FA974EBF", @"True" );
            // Attrib Value for Block:First Time Guests, Attribute:Show Communicate Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "5E2A78AE-DC9C-4A2C-8926-61E657E0BD2A", @"True" );
            // Attrib Value for Block:First Time Guests, Attribute:Show Merge Person Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "E416578C-E0D8-4C32-9E08-9998B7773052", @"False" );
            // Attrib Value for Block:First Time Guests, Attribute:Show Bulk Update Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "3D47F2DA-B48B-4F98-85BA-9504AC59A1F4", @"False" );
            // Attrib Value for Block:First Time Guests, Attribute:Show Excel Export Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "3F8B96ED-8D77-4263-A20A-69BBA936BA60", @"True" );
            // Attrib Value for Block:First Time Guests, Attribute:Show Merge Template Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "7FEC7FCC-766A-4743-8E22-CB31A723E32F", @"True" );
            // Attrib Value for Block:First Time Guests, Attribute:Timeout Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "63CB323E-2DC6-4D78-BEFC-4CBE2B560C58", @"900" );
            // Attrib Value for Block:First Time Guests, Attribute:Merge Fields Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "54F008E3-A974-46FC-9A00-B64843056D3A", @"" );
            // Attrib Value for Block:First Time Guests, Attribute:Encrypted Fields Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "70E610B9-141A-4684-BA47-B85E1284408B", @"" );
            // Attrib Value for Block:First Time Guests, Attribute:Page Title Lava Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "95523AD5-5591-4503-AB04-7E7B73FB6184", @"" );
            // Attrib Value for Block:First Time Guests, Attribute:Paneled Grid Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "33075F20-0C75-4749-9A58-EEF34B6CBCB8", @"False" );
            // Attrib Value for Block:First Time Guests, Attribute:Show Grid Filter Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "19F773D5-508D-40E2-9D27-C58A1755EA33", @"False" );
            // Attrib Value for Block:First Time Guests, Attribute:Columns Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "740987A0-3BD1-43A9-862F-FE353697E1F8", @"Id, Test1" );
            // Attrib Value for Block:First Time Guests, Attribute:Update Page Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "09FB9660-3B33-44B2-8CB5-FED37D241B6F", @"True" );
            // Attrib Value for Block:First Time Guests, Attribute:Query Page: Assimilation Report, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "CBE194E9-4F10-4505-93DE-31EABA918E85", @"--- First grab everyone for this campus and time period Declare @CampusIds nvarchar(max) = 'null' Declare @AttendanceNumbers nvarchar(max) = '1' Declare @Ages nvarchar(max) = ''  {% assign queryParms = 'Global' | Page:'QueryString' %}  {% for item in queryParms %}     {% assign kvItem = item | PropertyToKeyValue %}     {% if kvItem.Key == 'DateRange' %}         {% if kvItem.Value != '' %}             {% assign DateRange = kvItem.Value | Split:',' %}         {% endif %}     {% endif %}       {% if kvItem.Key == 'CampusIds' %}         {% if kvItem.Value != ',' %}             {% assign CampusIds = kvItem.Value %}         {% endif %}     {% endif %}          {% if kvItem.Key == 'Visitors' %}         {% if kvItem.Value != ',' %}             {% assign AttendanceNumbers = kvItem.Value %}         {% endif %}     {% endif %}          {% if kvItem.Key == 'Age' %}         {% if kvItem.Value != ',' %}             {% assign Age = kvItem.Value %}         {% endif %}     {% endif %}      {% endfor %}  {% if CampusIds != null %} Set @CampusIds = '{{CampusIds}}' {% endif %}  {% if AttendanceNumbers != null %} Set @AttendanceNumbers = '{{AttendanceNumbers}}' {% endif %}  {% if Age != null %} Set @Ages = '{{Age}}' {% endif %}  Declare @EndDateTime datetime =DateAdd(day, 1, [dbo].ufnUtility_GetPreviousSundayDate()) Declare @StartDateTime datetime = DateAdd(day, -7, @EndDateTime)  {% if DateRange[0] != null %} Set @StartDateTime = '{{ DateRange[0] }} 12:00 AM' {% endif %}  {% if DateRange[1] != null %} Set @EndDateTime = DateAdd(day, 1,'{{ DateRange[1] }} 12:00 AM') {% endif %}  Declare @AttendedNowTable table(  PersonId int,  Age int,  AttendanceDateTime datetime,  CampusName nvarchar(max) );  Insert Into @AttendedNowTable Select * From (  Select distinct p.Id,  Case When p.BirthDate is null Then null      When p.BirthDate > DateAdd(year,DatePart(Year,p.BirthDate) - DatePart(year, getDate()),GetDate()) Then (DatePart(year, getDate()) - DatePart(Year,p.BirthDate))-1      Else  DatePart(year, getDate()) - DatePart(Year,p.BirthDate) End as 'Age',   a.StartDateTime,    c.Name  From Attendance a  Join AttendanceOccurrence ao on a.OccurrenceId = ao.Id  Join [Group] g on ao.GroupId = g.Id  Join GroupType gt on g.GroupTypeId = gt.Id  Join PersonAlias pa on a.PersonAliasId = pa.Id  Join Person p on pa.PersonId = p.Id  Join Campus c on g.CampusId = c.Id  Where gt.AttendanceCountsAsWeekendService = 1  and a.StartDateTime >= @StartDateTime  and a.StartDateTime < @EndDateTime  and( @CampusIds = 'null' or c.Id in (Select * From ufnUtility_CsvToTable(@CampusIds))) ) innerTable Where (   @Ages = '' or   (@Ages like '%Child%' and Age >= 0 and Age <= 9) or   (@Ages like '%Youth%' and Age >= 10 and Age <= 17) or   (@Ages like '%Adult%' and ( Age >= 18 or Age is null))   )  Declare @TotalAttendanceCountTable table(  PersonId int,  AttendanceDateTime datetime,  AttendanceCount int,  CampusName nvarchar(max) );  Insert Into @TotalAttendanceCountTable Select p.Id, attendedTable.AttendanceDateTime, count(0), attendedTable.CampusName From Attendance a Join AttendanceOccurrence ao on a.OccurrenceId = ao.Id Join [Group] g on ao.GroupId = g.Id Join GroupType gt on g.GroupTypeId = gt.Id Join PersonAlias pa on a.PersonAliasId = pa.Id Join Person p on pa.PersonId = p.Id Join @AttendedNowTable attendedTable on p.Id = attendedTable.PersonId Where gt.AttendanceCountsAsWeekendService = 1 and attendedTable.AttendanceDateTime >= a.StartDateTime Group By p.Id, attendedTable.AttendanceDateTime, CampusName  Select p.Id,   p.NickName as 'First Name',   p.LastName,   g.[Name] as 'Household Name',   (SELECT ISNULL([Street1], '') + ' ' + ISNULL([Street2], '')     + ' ' + ISNULL([City], '') + ', ' + ISNULL([State], '')     + ' ' + ISNULL([PostalCode], '')     FROM [Location]      WHERE [Id] = (SELECT TOP 1 [LocationId]         FROM [GroupLocation]         WHERE  [GroupLocationTypeValueId] = 19         AND  [GroupId] = g.Id)) as 'Address',   STUFF(     (   SELECT ',' + pn.NumberFormatted       From PhoneNumber pn      Where PersonId = p.Id      FOR xml path('')     )     , 1     , 1     , '') as 'Phone #''s',   p.Email,   attendedTable.Age,   countTable.CampusName as 'Campus',   countTable.AttendanceDateTime as 'Visit Date',   countTable.AttendanceCount as 'Visit #' From @TotalAttendanceCountTable countTable Join Person p on countTable.PersonId = p.Id Join @AttendedNowTable attendedTable on attendedTable.PersonId = p.Id Join GroupMember gm on p.Id = gm.PersonId Join [Group] g on gm.GroupId = g.Id and g.Id = (SELECT TOP 1 [GroupId]           FROM [GroupMember] gm           INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]           WHERE [PersonId] = p.Id AND g.[GroupTypeId] = 10) Where countTable.AttendanceCount in (Select * From ufnUtility_CsvToTable(@AttendanceNumbers))  Order By LastName, [First Name], [Visit #]" );
            RockMigrationHelper.AddBlockAttributeValue( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248", "CBE194E9-4F10-4505-93DE-31EABA918E85", @"--- First grab everyone for this campus and time period Declare @CampusIds nvarchar(max) = 'null' Declare @AttendanceNumbers nvarchar(max) = '1' Declare @Ages nvarchar(max) = ''  {% assign queryParms = 'Global' | Page:'QueryString' %}  {% for item in queryParms %}     {% assign kvItem = item | PropertyToKeyValue %}     {% if kvItem.Key == 'DateRange' %}         {% if kvItem.Value != '' %}             {% assign DateRange = kvItem.Value | Split:',' %}         {% endif %}     {% endif %}       {% if kvItem.Key == 'CampusIds' %}         {% if kvItem.Value != ',' %}             {% assign CampusIds = kvItem.Value %}         {% endif %}     {% endif %}          {% if kvItem.Key == 'Visitors' %}         {% if kvItem.Value != ',' %}             {% assign AttendanceNumbers = kvItem.Value %}         {% endif %}     {% endif %}          {% if kvItem.Key == 'Age' %}         {% if kvItem.Value != ',' %}             {% assign Age = kvItem.Value %}         {% endif %}     {% endif %}      {% endfor %}  {% if CampusIds != null %} Set @CampusIds = '{{CampusIds}}' {% endif %}  {% if AttendanceNumbers != null %} Set @AttendanceNumbers = '{{AttendanceNumbers}}' {% endif %}  {% if Age != null %} Set @Ages = '{{Age}}' {% endif %}  Declare @EndDateTime datetime =DateAdd(day, 1, [dbo].ufnUtility_GetPreviousSundayDate()) Declare @StartDateTime datetime = DateAdd(day, -7, @EndDateTime)  {% if DateRange[0] != null %} Set @StartDateTime = '{{ DateRange[0] }} 12:00 AM' {% endif %}  {% if DateRange[1] != null %} Set @EndDateTime = DateAdd(day, 1,'{{ DateRange[1] }} 12:00 AM') {% endif %}  Declare @AttendedNowTable table(  PersonId int,  Age int,  AttendanceDateTime datetime,  CampusName nvarchar(max) );  Insert Into @AttendedNowTable Select * From (  Select distinct p.Id,  Case When p.BirthDate is null Then null      When p.BirthDate > DateAdd(year,DatePart(Year,p.BirthDate) - DatePart(year, getDate()),GetDate()) Then (DatePart(year, getDate()) - DatePart(Year,p.BirthDate))-1      Else  DatePart(year, getDate()) - DatePart(Year,p.BirthDate) End as 'Age',   a.StartDateTime,    c.Name  From Attendance a  Join AttendanceOccurrence ao on a.OccurrenceId = ao.Id  Join [Group] g on ao.GroupId = g.Id  Join GroupType gt on g.GroupTypeId = gt.Id  Join PersonAlias pa on a.PersonAliasId = pa.Id  Join Person p on pa.PersonId = p.Id  Join Campus c on g.CampusId = c.Id  Where gt.AttendanceCountsAsWeekendService = 1  and a.StartDateTime >= @StartDateTime  and a.StartDateTime < @EndDateTime  and( @CampusIds = 'null' or c.Id in (Select * From ufnUtility_CsvToTable(@CampusIds))) ) innerTable Where (   @Ages = '' or   (@Ages like '%Child%' and Age >= 0 and Age <= 9) or   (@Ages like '%Youth%' and Age >= 10 and Age <= 17) or   (@Ages like '%Adult%' and ( Age >= 18 or Age is null))   )  Declare @TotalAttendanceCountTable table(  PersonId int,  AttendanceDateTime datetime,  AttendanceCount int,  CampusName nvarchar(max) );  Insert Into @TotalAttendanceCountTable Select p.Id, attendedTable.AttendanceDateTime, count(0), attendedTable.CampusName From Attendance a Join AttendanceOccurrence ao on a.OccurrenceId = ao.Id Join [Group] g on ao.GroupId = g.Id Join GroupType gt on g.GroupTypeId = gt.Id Join PersonAlias pa on a.PersonAliasId = pa.Id Join Person p on pa.PersonId = p.Id Join @AttendedNowTable attendedTable on p.Id = attendedTable.PersonId Where gt.AttendanceCountsAsWeekendService = 1 and attendedTable.AttendanceDateTime >= a.StartDateTime Group By p.Id, attendedTable.AttendanceDateTime, CampusName  Select p.Id,   p.NickName as 'First Name',   p.LastName,   g.[Name] as 'Household Name',   (SELECT ISNULL([Street1], '') + ' ' + ISNULL([Street2], '')     + ' ' + ISNULL([City], '') + ', ' + ISNULL([State], '')     + ' ' + ISNULL([PostalCode], '')     FROM [Location]      WHERE [Id] = (SELECT TOP 1 [LocationId]         FROM [GroupLocation]         WHERE  [GroupLocationTypeValueId] = 19         AND  [GroupId] = g.Id)) as 'Address',   STUFF(     (   SELECT ',' + pn.NumberFormatted       From PhoneNumber pn      Where PersonId = p.Id      FOR xml path('')     )     , 1     , 1     , '') as 'Phone #''s',   p.Email,   attendedTable.Age,   countTable.CampusName as 'Campus',   countTable.AttendanceDateTime as 'Visit Date',   countTable.AttendanceCount as 'Visit #' From @TotalAttendanceCountTable countTable Join Person p on countTable.PersonId = p.Id Join @AttendedNowTable attendedTable on attendedTable.PersonId = p.Id Join GroupMember gm on p.Id = gm.PersonId Join [Group] g on gm.GroupId = g.Id and g.Id = (SELECT TOP 1 [GroupId]           FROM [GroupMember] gm           INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]           WHERE [PersonId] = p.Id AND g.[GroupTypeId] = 10) Where countTable.AttendanceCount in (Select * From ufnUtility_CsvToTable(@AttendanceNumbers))  Order By LastName, [First Name], [Visit #]" );
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "CBE194E9-4F10-4505-93DE-31EABA918E85" );
            RockMigrationHelper.DeleteAttribute( "09FB9660-3B33-44B2-8CB5-FED37D241B6F" );
            RockMigrationHelper.DeleteAttribute( "740987A0-3BD1-43A9-862F-FE353697E1F8" );
            RockMigrationHelper.DeleteAttribute( "19F773D5-508D-40E2-9D27-C58A1755EA33" );
            RockMigrationHelper.DeleteAttribute( "33075F20-0C75-4749-9A58-EEF34B6CBCB8" );
            RockMigrationHelper.DeleteAttribute( "95523AD5-5591-4503-AB04-7E7B73FB6184" );
            RockMigrationHelper.DeleteAttribute( "70E610B9-141A-4684-BA47-B85E1284408B" );
            RockMigrationHelper.DeleteAttribute( "54F008E3-A974-46FC-9A00-B64843056D3A" );
            RockMigrationHelper.DeleteAttribute( "63CB323E-2DC6-4D78-BEFC-4CBE2B560C58" );
            RockMigrationHelper.DeleteAttribute( "7FEC7FCC-766A-4743-8E22-CB31A723E32F" );
            RockMigrationHelper.DeleteAttribute( "3F8B96ED-8D77-4263-A20A-69BBA936BA60" );
            RockMigrationHelper.DeleteAttribute( "3D47F2DA-B48B-4F98-85BA-9504AC59A1F4" );
            RockMigrationHelper.DeleteAttribute( "E416578C-E0D8-4C32-9E08-9998B7773052" );
            RockMigrationHelper.DeleteAttribute( "5E2A78AE-DC9C-4A2C-8926-61E657E0BD2A" );
            RockMigrationHelper.DeleteAttribute( "A7A0F7F0-E51F-446C-86A2-37E6FA974EBF" );
            RockMigrationHelper.DeleteAttribute( "F55EB0F9-E38C-496F-878B-E25C7706D575" );
            RockMigrationHelper.DeleteAttribute( "4C2D28DF-C935-4250-80DC-320E741D23C8" );
            RockMigrationHelper.DeleteAttribute( "302282EB-D2E2-4CC3-AA5E-47679BC7EA16" );
            RockMigrationHelper.DeleteAttribute( "4F942092-6859-4C44-BFDB-161A17C0D91F" );
            RockMigrationHelper.DeleteAttribute( "01DD1794-354A-458F-AFAE-E6A5C77C044A" );
            RockMigrationHelper.DeleteAttribute( "9EA298E8-D2E1-4111-BE77-2B6BD96EB6F8" );
            RockMigrationHelper.DeleteAttribute( "B29D445D-4447-4137-A33F-FF0C63A6E813" );
            RockMigrationHelper.DeleteAttribute( "86EFBF55-2E2F-42BD-AEEF-34E9769AE44F" );
            RockMigrationHelper.DeleteAttribute( "E7DF5865-7973-45AF-A090-9D28E9747B7F" );
            RockMigrationHelper.DeleteAttribute( "B3BEA2E6-31F9-4615-B452-681F3887023B" );
            RockMigrationHelper.DeleteAttribute( "9AE98186-E466-4196-A272-31852322A4BA" );
            RockMigrationHelper.DeleteAttribute( "41B4D32D-ED66-402D-8B3C-FC5DCE53DBA8" );
            RockMigrationHelper.DeleteAttribute( "D4570AD0-97D6-40D2-A511-F44F0B34F7F6" );
            RockMigrationHelper.DeleteAttribute( "AA948207-54F5-4CCC-AB4B-4B3582C7C4D1" );
            RockMigrationHelper.DeleteAttribute( "60F58D52-1C66-45C6-B3B9-85A213A53EEF" );
            RockMigrationHelper.DeleteAttribute( "552A47D7-A6BE-4634-A1AC-C947147AE6CA" );
            RockMigrationHelper.DeleteAttribute( "204FB5CA-4650-4D82-8E55-035AA7C73304" );
            RockMigrationHelper.DeleteAttribute( "20102155-99D0-484F-86A1-39262994EBCF" );
            RockMigrationHelper.DeleteAttribute( "AACC30CA-06F8-426B-8DBB-43301A5E5122" );
            RockMigrationHelper.DeleteAttribute( "1512BB91-F120-409A-A5D9-3F912911AD60" );
            RockMigrationHelper.DeleteAttribute( "9172F1C2-932B-4F06-91D8-F1A254862A3E" );
            RockMigrationHelper.DeleteAttribute( "A5EAFA44-1844-41A6-A359-5E19BEF3249F" );
            RockMigrationHelper.DeleteAttribute( "32CA03D1-7DB7-4922-B426-3131D91F96AC" );
            RockMigrationHelper.DeleteAttribute( "9E3699DA-DABC-4D8F-BB03-30F7E336BC22" );
            RockMigrationHelper.DeleteAttribute( "BEAB0FC4-AB78-4E65-9157-D31390F531A1" );
            RockMigrationHelper.DeleteAttribute( "F4D72961-F212-4D49-956F-CE42C3B86B8F" );
            RockMigrationHelper.DeleteAttribute( "3D33B8AC-BFBD-4498-B8A1-882BBEB64BE2" );
            RockMigrationHelper.DeleteBlock( "37A9B4BC-FED2-4E09-BECA-2CAB9DE1D248" );
            RockMigrationHelper.DeleteBlock( "1D9DAD3B-2276-48A8-AD25-1F04F13C76C3" );
            RockMigrationHelper.DeleteBlock( "DFB36917-0D7D-49B5-BF62-670A7F80E281" );
            RockMigrationHelper.DeleteBlockType( "AF9354EF-7DAA-42A7-93E8-EDBA8CDFD563" );
            RockMigrationHelper.DeleteBlockType( "FE667841-D12D-4F12-A171-A3855A7CE254" );
            RockMigrationHelper.DeletePage( "82A4A145-4E72-41DA-AA05-E4B31A3290FF" ); //  Page: Assimilation Report
        }
    }
}

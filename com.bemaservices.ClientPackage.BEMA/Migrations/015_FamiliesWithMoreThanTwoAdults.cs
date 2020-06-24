// <copyright>
// Copyright by BEMA Information Technologies
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

using Rock.Plugin;

namespace com.bemaservices.ClientPackage.BEMA
{
    [MigrationNumber( 15, "1.9.4" )]
    public class FamiliesWithMoreThanTwoAdults : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Families with More Than Two Adults
            RockMigrationHelper.AddPage("2571CBBD-7CCA-4B24-AAAB-107FD136298B","D65F783D-87A9-4CC9-8110-E83466A0EADB","Families with More Than Two Adults","Shows list of families with more than two adult people as group members","8032F679-68C5-4168-AE4F-7376FCD87E07","fa fa-users"); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Dynamic Data", "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "~/Blocks/Reporting/DynamicData.ascx", "Reporting", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" );
            // Add Block to Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8032F679-68C5-4168-AE4F-7376FCD87E07","","E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","Dynamic Data","Main","","",0,"4EEEA483-439A-47E5-B9CC-2E00CFF2C659");
            // Attrib for BlockType: Dynamic Data:Update Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Update Page", "UpdatePage", "", @"If True, provides fields for updating the parent page's Name and Description", 0, @"True", "230EDFE8-33CA-478D-8C9A-572323AF3466" );
            // Attrib for BlockType: Dynamic Data:Query Params
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Query Params", "QueryParams", "", @"Parameters to pass to query", 0, @"", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012" );
            // Attrib for BlockType: Dynamic Data:Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Columns", "Columns", "", @"The columns to hide or show", 0, @"", "90B0E6AF-B2F4-4397-953B-737A40D4023B" );
            // Attrib for BlockType: Dynamic Data:Query
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Query", "Query", "", @"The query to execute. Note that if you are providing SQL you can add items from the query string using Lava like {{ QueryParmName }}.", 0, @"", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356" );
            // Attrib for BlockType: Dynamic Data:Url Mask
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Url Mask", "UrlMask", "", @"The Url to redirect to when a row is clicked", 0, @"", "B9163A35-E09C-466D-8A2D-4ED81DF0114C" );
            // Attrib for BlockType: Dynamic Data:Show Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Columns", "ShowColumns", "", @"Should the 'Columns' specified below be the only ones shown (vs. the only ones hidden)", 0, @"False", "202A82BF-7772-481C-8419-600012607972" );
            // Attrib for BlockType: Dynamic Data:Merge Fields
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Merge Fields", "MergeFields", "", @"Any fields to make available as merge fields for any new communications", 0, @"", "8EB882CE-5BB1-4844-9C28-10190903EECD" );
            // Attrib for BlockType: Dynamic Data:Formatted Output
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Formatted Output", "FormattedOutput", "", @"Optional formatting to apply to the returned results. If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}", 0, @"", "6A233402-446C-47E9-94A5-6A247C29BC21" );
            // Attrib for BlockType: Dynamic Data:Person Report
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Person Report", "PersonReport", "", @"Is this report a list of people?", 0, @"False", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C" );
            // Attrib for BlockType: Dynamic Data:Communication Recipient Person Id Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Communication Recipient Person Id Columns", "CommunicationRecipientPersonIdColumns", "", @"Columns that contain a communication recipient person id.", 0, @"", "75DDB977-9E71-44E8-924B-27134659D3A4" );
            // Attrib for BlockType: Dynamic Data:Show Grid Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grid Filter", "ShowGridFilter", "", @"Show filtering controls that are dynamically generated to match the columns of the dynamic data.", 0, @"True", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C" );
            // Attrib for BlockType: Dynamic Data:Paneled Grid
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Paneled Grid", "PaneledGrid", "", @"Add the 'grid-panel' class to the grid to allow it to fit nicely in a block.", 0, @"False", "5449CB61-2DFC-4B55-A697-38F1C2AF128B" );
            // Attrib for BlockType: Dynamic Data:Stored Procedure
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Stored Procedure", "StoredProcedure", "", @"Is the query a stored procedure?", 0, @"False", "A4439703-5432-489A-9C14-155903D6A43E" );
            // Attrib for BlockType: Dynamic Data:Show Communicate
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Communicate", "ShowCommunicate", "", @"Show Communicate button in grid footer?", 0, @"True", "5B2C115A-C187-4AB3-93AE-7010644B39DA" );
            // Attrib for BlockType: Dynamic Data:Show Merge Person
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Merge Person", "ShowMergePerson", "", @"Show Merge Person button in grid footer?", 0, @"True", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E" );
            // Attrib for BlockType: Dynamic Data:Show Bulk Update
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Bulk Update", "ShowBulkUpdate", "", @"Show Bulk Update button in grid footer?", 0, @"True", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78" );
            // Attrib for BlockType: Dynamic Data:Show Excel Export
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Excel Export", "ShowExcelExport", "", @"Show Export to Excel button in grid footer?", 0, @"True", "E11B57E5-EC7D-4C42-9ADA-37594D71F145" );
            // Attrib for BlockType: Dynamic Data:Show Merge Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Merge Template", "ShowMergeTemplate", "", @"Show Export to Merge Template button in grid footer?", 0, @"True", "6697B0A2-C8FE-497A-B5B4-A9D459474338" );
            // Attrib for BlockType: Dynamic Data:Timeout
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Timeout", "Timeout", "", @"The amount of time in xxx to allow the query to run before timing out.", 0, @"30", "BEEE38DD-2791-4242-84B6-0495904143CC" );
            // Attrib for BlockType: Dynamic Data:Page Title Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Page Title Lava", "PageTitleLava", "", @"Optional Lava for setting the page title. If nothing is provided then the page's title will be used.", 0, @"", "3F4BA170-F5C5-405E-976F-0AFBB8855FE8" );
            // Attrib for BlockType: Dynamic Data:Encrypted Fields
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Encrypted Fields", "EncryptedFields", "", @"Any fields that need to be decrypted before displaying their value", 0, @"", "AF7714D4-D825-419A-B136-FF8293396635" );
            // Attrib for BlockType: Dynamic Data:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this dynamic data block.", 1, @"", "824634D6-7F75-465B-A2D2-BA3CE1662CAC" );
            // Attrib Value for Block:Dynamic Data, Attribute:Update Page Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","230EDFE8-33CA-478D-8C9A-572323AF3466",@"True");
            // Attrib Value for Block:Dynamic Data, Attribute:Query Params Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","B0EC41B9-37C0-48FD-8E4E-37A8CA305012",@"");
            // Attrib Value for Block:Dynamic Data, Attribute:Columns Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","90B0E6AF-B2F4-4397-953B-737A40D4023B",@"Id,GroupId");
            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356",@" {% include '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Assets/Sql/FamiliesWithMoreThanTwoAdults.sql' %} ");
            // Attrib Value for Block:Dynamic Data, Attribute:Url Mask Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","B9163A35-E09C-466D-8A2D-4ED81DF0114C",@"/EditFamily/{id}/{GroupId}");
            // Attrib Value for Block:Dynamic Data, Attribute:Show Columns Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","202A82BF-7772-481C-8419-600012607972",@"False");
            // Attrib Value for Block:Dynamic Data, Attribute:Merge Fields Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","8EB882CE-5BB1-4844-9C28-10190903EECD",@"");
            // Attrib Value for Block:Dynamic Data, Attribute:Formatted Output Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","6A233402-446C-47E9-94A5-6A247C29BC21",@"");
            // Attrib Value for Block:Dynamic Data, Attribute:Person Report Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C",@"True");
            // Attrib Value for Block:Dynamic Data, Attribute:Show Grid Filter Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","E582FD3C-9990-47D1-A57F-A3DB753B1D0C",@"True");
            // Attrib Value for Block:Dynamic Data, Attribute:Paneled Grid Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","5449CB61-2DFC-4B55-A697-38F1C2AF128B",@"False");
            // Attrib Value for Block:Dynamic Data, Attribute:Stored Procedure Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","A4439703-5432-489A-9C14-155903D6A43E",@"False");
            // Attrib Value for Block:Dynamic Data, Attribute:Show Communicate Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","5B2C115A-C187-4AB3-93AE-7010644B39DA",@"True");
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Person Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","8762ABE3-726E-4629-BD4D-3E42E1FBCC9E",@"True");
            // Attrib Value for Block:Dynamic Data, Attribute:Show Bulk Update Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","D01510AA-1B8D-467C-AFC6-F7554CB7CF78",@"True");
            // Attrib Value for Block:Dynamic Data, Attribute:Show Excel Export Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","E11B57E5-EC7D-4C42-9ADA-37594D71F145",@"True");
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Template Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","6697B0A2-C8FE-497A-B5B4-A9D459474338",@"True");
            // Attrib Value for Block:Dynamic Data, Attribute:Timeout Page: Families with More Than Two Adults, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("4EEEA483-439A-47E5-B9CC-2E00CFF2C659","BEEE38DD-2791-4242-84B6-0495904143CC",@"30");

            RockMigrationHelper.AddSecurityAuthForPage( "8032F679-68C5-4168-AE4F-7376FCD87E07", 0, "View", false, "", 1, "DDADA8F2-387A-4E77-AE61-F3F5F483E975" );
        
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "824634D6-7F75-465B-A2D2-BA3CE1662CAC" );
            RockMigrationHelper.DeleteAttribute( "AF7714D4-D825-419A-B136-FF8293396635" );
            RockMigrationHelper.DeleteAttribute( "75DDB977-9E71-44E8-924B-27134659D3A4" );
            RockMigrationHelper.DeleteAttribute( "3F4BA170-F5C5-405E-976F-0AFBB8855FE8" );
            RockMigrationHelper.DeleteAttribute( "BEEE38DD-2791-4242-84B6-0495904143CC" );
            RockMigrationHelper.DeleteAttribute( "6697B0A2-C8FE-497A-B5B4-A9D459474338" );
            RockMigrationHelper.DeleteAttribute( "E11B57E5-EC7D-4C42-9ADA-37594D71F145" );
            RockMigrationHelper.DeleteAttribute( "D01510AA-1B8D-467C-AFC6-F7554CB7CF78" );
            RockMigrationHelper.DeleteAttribute( "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E" );
            RockMigrationHelper.DeleteAttribute( "5B2C115A-C187-4AB3-93AE-7010644B39DA" );
            RockMigrationHelper.DeleteAttribute( "A4439703-5432-489A-9C14-155903D6A43E" );
            RockMigrationHelper.DeleteAttribute( "5449CB61-2DFC-4B55-A697-38F1C2AF128B" );
            RockMigrationHelper.DeleteAttribute( "E582FD3C-9990-47D1-A57F-A3DB753B1D0C" );
            RockMigrationHelper.DeleteAttribute( "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C" );
            RockMigrationHelper.DeleteAttribute( "6A233402-446C-47E9-94A5-6A247C29BC21" );
            RockMigrationHelper.DeleteAttribute( "8EB882CE-5BB1-4844-9C28-10190903EECD" );
            RockMigrationHelper.DeleteAttribute( "202A82BF-7772-481C-8419-600012607972" );
            RockMigrationHelper.DeleteAttribute( "B9163A35-E09C-466D-8A2D-4ED81DF0114C" );
            RockMigrationHelper.DeleteAttribute( "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356" );
            RockMigrationHelper.DeleteAttribute( "90B0E6AF-B2F4-4397-953B-737A40D4023B" );
            RockMigrationHelper.DeleteAttribute( "B0EC41B9-37C0-48FD-8E4E-37A8CA305012" );
            RockMigrationHelper.DeleteAttribute( "230EDFE8-33CA-478D-8C9A-572323AF3466" );
            RockMigrationHelper.DeleteBlock( "4EEEA483-439A-47E5-B9CC-2E00CFF2C659" );
            RockMigrationHelper.DeleteBlockType( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" );
            RockMigrationHelper.DeletePage( "8032F679-68C5-4168-AE4F-7376FCD87E07" ); // Page: Families with More Than Two Adults
        }
    }
}


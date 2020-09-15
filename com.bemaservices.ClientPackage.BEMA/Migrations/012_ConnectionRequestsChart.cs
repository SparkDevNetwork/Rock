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
    [MigrationNumber( 12, "1.9.4" )]
    public class ConnectionRequestsChart : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Connections Chart
            RockMigrationHelper.AddPage( "2571CBBD-7CCA-4B24-AAAB-107FD136298B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connections Chart", "Shows a Pie Chart of open connection requests and how many in each opportunity.", "0403E0FE-64C1-4671-BD90-01E5AF9D0EEC", "fa fa-plug" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Dynamic Data", "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "~/Blocks/Reporting/DynamicData.ascx", "Reporting", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" );
            RockMigrationHelper.UpdateBlockType( "Dynamic Chart", "Block to display a chart using SQL as the chart datasource", "~/Blocks/Reporting/DynamicChart.ascx", "Reporting", "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723" );
            // Add Block to Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0403E0FE-64C1-4671-BD90-01E5AF9D0EEC", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Dynamic Data", "Main", "    <div class='col-md-6'>", "</div></div>", 1, "1C818DB8-F627-498F-A06B-351A06C9900E" );
            // Add Block to Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0403E0FE-64C1-4671-BD90-01E5AF9D0EEC", "", "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "Dynamic Chart", "Main", "<div class='row'>     <div class='col-md-6'>", "</div>", 0, "5BE9807D-6081-4E45-9445-B15FEF8507EF" );
            // Add Block to Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0403E0FE-64C1-4671-BD90-01E5AF9D0EEC", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Feature", "", "", 0, "2FEC4C30-137C-4F7F-A4B7-E5EF9F6240ED" );
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
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: Dynamic Chart:Query Params
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Query Params", "QueryParams", "", "The parameters that the stored procedure expects in the format of 'param1=value;param2=value'. Any parameter with the same name as a page parameter (i.e. querystring, form, or page route) will have its value replaced with the page's current value. A parameter with the name of 'CurrentPersonId' will have its value replaced with the currently logged in person's id.", 0, @"", "0D7A45A6-C885-44CD-9FA9-B8F431D943B5" );
            // Attrib for BlockType: Dynamic Chart:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "The title of the widget", 0, @"", "94FCBF87-B633-4F02-887F-A99970129319" );
            // Attrib for BlockType: Dynamic Chart:Chart Height
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Chart Height", "ChartHeight", "", "", 0, @"200", "BFAF6EB4-5181-4473-A222-3C2F12982956" );
            // Attrib for BlockType: Dynamic Chart:SQL
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQL", "SQL", "", "The SQL for the datasource. Output columns must be as follows:  <ul>      <li>Bar or Line Chart          <ul>             <li>[SeriesName] : string or numeric </li>             <li>[DateTime] : DateTime </li>             <li>[YValue] : numeric </li>          </ul>      </li>      <li>Pie Chart          <ul>             <li>[MetricTitle] : string </li>             <li>[YValueTotal] : numeric </li>          </ul>      </li>  </ul>    Example:   <code><pre>  -- get top 25 viewed pages from the last 30 days (excluding Home)  select top 25  * from (      select           distinct          pv.PageTitle [SeriesName],           convert(date, pv.DateTimeViewed) [DateTime],           count(*) [YValue]       from           PageView pv      where PageTitle is not null          group by pv.PageTitle, convert(date, pv.DateTimeViewed)      ) x where SeriesID != 'Home'   and DateTime > DateAdd(day, -30, SysDateTime())  order by YValue desc  </pre>  </code>", 0, @"", "C95DFED0-D082-4D47-A4C0-ADD8664A9CA8" );
            // Attrib for BlockType: Dynamic Chart:Subtitle
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "", "The subtitle of the widget", 1, @"", "3006CAEC-2DBD-4C87-BCA7-5AB23657497E" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", "Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: Dynamic Data:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this dynamic data block.", 1, @"", "824634D6-7F75-465B-A2D2-BA3CE1662CAC" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", "The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: Dynamic Chart:Column Width
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Column Width", "ColumnWidth", "", "The width of the widget.", 2, @"4", "9B4B5CBF-1E80-4CCA-8B6D-806806038188" );
            // Attrib for BlockType: Dynamic Chart:Chart Style
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 3, @"", "BAAE0B29-7D8C-4369-93F2-E1B624882170" );
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
            // Attrib for BlockType: Dynamic Chart:Show Legend
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legend", "ShowLegend", "", "", 7, @"True", "55D7DEB8-1D23-430B-81EF-D7F42FFAD518" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", "If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: Dynamic Chart:Legend Position
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Legend Position", "LegendPosition", "", "Select the position of the Legend (corner)", 8, @"ne", "398336FF-64CD-4EB8-A78C-9C9DAEBF68FA" );
            // Attrib for BlockType: Dynamic Chart:Chart Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Chart Type", "ChartType", "", "", 9, @"Line", "F5626814-8713-420D-A93C-CFE6E89321DF" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", "Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", "Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: Dynamic Chart:Pie Inner Radius
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Pie Inner Radius", "PieInnerRadius", "", "If this is a pie chart, specific the inner radius to have a donut hole. For example, specify: 0.75 to have the inner radius as 75% of the outer radius.", 10, @"0", "4CAF4C44-586D-490E-918D-52C0BD81BB3C" );
            // Attrib for BlockType: Dynamic Chart:Pie Show Labels
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Pie Show Labels", "PieShowLabels", "", "If this is a pie chart, specify if labels show be shown", 11, @"True", "3B54049D-60A4-4473-BA10-49101E06F150" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:Dynamic Data, Attribute:Update Page Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "230EDFE8-33CA-478D-8C9A-572323AF3466", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Params Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Columns Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "90B0E6AF-B2F4-4397-953B-737A40D4023B", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @" {% include '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Assets/Sql/ConnectionRequestsChart.sql' %}" );
            // Attrib Value for Block:Dynamic Data, Attribute:Url Mask Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "B9163A35-E09C-466D-8A2D-4ED81DF0114C", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Columns Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "202A82BF-7772-481C-8419-600012607972", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Merge Fields Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "8EB882CE-5BB1-4844-9C28-10190903EECD", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Formatted Output Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "6A233402-446C-47E9-94A5-6A247C29BC21", @" {% include '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Assets/Lava/ConnectionRequestsChart.lava' %}" );
            // Attrib Value for Block:Dynamic Data, Attribute:Person Report Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Excel Export Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "E11B57E5-EC7D-4C42-9ADA-37594D71F145", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Communicate Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "5B2C115A-C187-4AB3-93AE-7010644B39DA", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Person Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Bulk Update Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Stored Procedure Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "A4439703-5432-489A-9C14-155903D6A43E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Template Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "6697B0A2-C8FE-497A-B5B4-A9D459474338", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Paneled Grid Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "5449CB61-2DFC-4B55-A697-38F1C2AF128B", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Grid Filter Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Timeout Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1C818DB8-F627-498F-A06B-351A06C9900E", "BEEE38DD-2791-4242-84B6-0495904143CC", @"30" );
            // Attrib Value for Block:Dynamic Chart, Attribute:Legend Position Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5BE9807D-6081-4E45-9445-B15FEF8507EF", "398336FF-64CD-4EB8-A78C-9C9DAEBF68FA", @"ne" );
            // Attrib Value for Block:Dynamic Chart, Attribute:Chart Type Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5BE9807D-6081-4E45-9445-B15FEF8507EF", "F5626814-8713-420D-A93C-CFE6E89321DF", @"Pie" );
            // Attrib Value for Block:Dynamic Chart, Attribute:Column Width Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5BE9807D-6081-4E45-9445-B15FEF8507EF", "9B4B5CBF-1E80-4CCA-8B6D-806806038188", @"6" );
            // Attrib Value for Block:Dynamic Chart, Attribute:Chart Height Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5BE9807D-6081-4E45-9445-B15FEF8507EF", "BFAF6EB4-5181-4473-A222-3C2F12982956", @"300" );
            // Attrib Value for Block:Dynamic Chart, Attribute:Show Legend Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5BE9807D-6081-4E45-9445-B15FEF8507EF", "55D7DEB8-1D23-430B-81EF-D7F42FFAD518", @"True" );
            // Attrib Value for Block:Dynamic Chart, Attribute:Pie Show Labels Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5BE9807D-6081-4E45-9445-B15FEF8507EF", "3B54049D-60A4-4473-BA10-49101E06F150", @"True" );
            // Attrib Value for Block:Dynamic Chart, Attribute:SQL Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5BE9807D-6081-4E45-9445-B15FEF8507EF", "C95DFED0-D082-4D47-A4C0-ADD8664A9CA8", @"Select CO.Name As [MetricTitle], Count(*) As [YValueTotal] From ConnectionRequest CR Inner Join ConnectionOpportunity CO On CR.ConnectionOpportunityId = CO.Id Left Join Campus C On Cr.CampusId = C.Id Where 1=1 {% if PageParameter.Campus != null %} AND C.Guid in ('{{PageParameter.Campus | SanitizeSql }}') {% endif %} Group By CO.Name " );
            // Attrib Value for Block:Dynamic Chart, Attribute:Chart Style Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5BE9807D-6081-4E45-9445-B15FEF8507EF", "BAAE0B29-7D8C-4369-93F2-E1B624882170", @"b45da8e1-b9a6-46fd-9a2b-e8440d7d6aac" );
            // Attrib Value for Block:Dynamic Chart, Attribute:Pie Inner Radius Page: Connections Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5BE9807D-6081-4E45-9445-B15FEF8507EF", "4CAF4C44-586D-490E-918D-52C0BD81BB3C", @"0" );
            RockMigrationHelper.AddSecurityAuthForPage( "0403E0FE-64C1-4671-BD90-01E5AF9D0EEC", 0, "View", false, "", 1, "5E0D683F-A4AC-4939-9CEB-93718FBFA30A" );
            RockMigrationHelper.UpdateHtmlContentBlock( "2FEC4C30-137C-4F7F-A4B7-E5EF9F6240ED", @"<div class='alert alert-info'>
    This block shows a list of open connection requests. </br>
    <b>Total Requests</b> includes all requests created in the last year.</br>
    <b>Currently Open</b> includes all requests set to <i>Active</i> or <i>Future Follow-Up</i> created in the last year.</br>
    <b>Now Serving</b> includes all requests set to <i>In Progress</i> created in the last year.</br>
    <b>Average Days until Completion</b> displays the average time between a request creation date and the time it was closed out ( The Modified Date Time) for all requests set to <i>Inactive</i> or <i>Connected</i> created in the last year.</br>
   
</div>"
           , "27d3abc2-db45-4512-a6fa-31ba52f1b01d" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "824634D6-7F75-465B-A2D2-BA3CE1662CAC" );
            RockMigrationHelper.DeleteAttribute( "AF7714D4-D825-419A-B136-FF8293396635" );
            RockMigrationHelper.DeleteAttribute( "4CAF4C44-586D-490E-918D-52C0BD81BB3C" );
            RockMigrationHelper.DeleteAttribute( "BAAE0B29-7D8C-4369-93F2-E1B624882170" );
            RockMigrationHelper.DeleteAttribute( "C95DFED0-D082-4D47-A4C0-ADD8664A9CA8" );
            RockMigrationHelper.DeleteAttribute( "3B54049D-60A4-4473-BA10-49101E06F150" );
            RockMigrationHelper.DeleteAttribute( "55D7DEB8-1D23-430B-81EF-D7F42FFAD518" );
            RockMigrationHelper.DeleteAttribute( "BFAF6EB4-5181-4473-A222-3C2F12982956" );
            RockMigrationHelper.DeleteAttribute( "9B4B5CBF-1E80-4CCA-8B6D-806806038188" );
            RockMigrationHelper.DeleteAttribute( "F5626814-8713-420D-A93C-CFE6E89321DF" );
            RockMigrationHelper.DeleteAttribute( "398336FF-64CD-4EB8-A78C-9C9DAEBF68FA" );
            RockMigrationHelper.DeleteAttribute( "3006CAEC-2DBD-4C87-BCA7-5AB23657497E" );
            RockMigrationHelper.DeleteAttribute( "94FCBF87-B633-4F02-887F-A99970129319" );
            RockMigrationHelper.DeleteAttribute( "0D7A45A6-C885-44CD-9FA9-B8F431D943B5" );
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
            RockMigrationHelper.DeleteBlock( "5BE9807D-6081-4E45-9445-B15FEF8507EF" );
            RockMigrationHelper.DeleteBlock( "1C818DB8-F627-498F-A06B-351A06C9900E" );
            RockMigrationHelper.DeleteBlockType( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723" );
            RockMigrationHelper.DeleteBlockType( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" );
            RockMigrationHelper.DeletePage( "0403E0FE-64C1-4671-BD90-01E5AF9D0EEC" ); // Page: Connections Chart
        }
    }
}


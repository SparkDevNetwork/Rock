using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.lcbcchurch.Utility.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    class DatabaseSize : Migration
    {
        public override void Up()
        {
            // Page: Database Size
            RockMigrationHelper.AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Database Size", "", "03F7B803-F5E1-4129-B29F-3B7E65C443A0", "fa fa-database" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Dynamic Data", "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "~/Blocks/Reporting/DynamicData.ascx", "Reporting", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" );
            RockMigrationHelper.UpdateBlockType( "Dynamic Chart", "Block to display a chart using SQL as the chart datasource", "~/Blocks/Reporting/DynamicChart.ascx", "Reporting", "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723" );
            // Add Block to Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03F7B803-F5E1-4129-B29F-3B7E65C443A0", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Top Ten Tables", "Main", "<h2>Top Ten Largest Tables</h2>", "", 0, "03738EAB-BE06-4DEA-AE73-2EFEC404F586" );
            // Add Block to Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03F7B803-F5E1-4129-B29F-3B7E65C443A0", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "File Type Breakdown", "Main", "<h2>Binary File Type Size Breakdown</h2>", "", 1, "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA" );
            // Add Block to Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03F7B803-F5E1-4129-B29F-3B7E65C443A0", "", "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "File Type Size Chart", "Main", "<h2>Binary File Type Charts</h2>", "", 2, "676412EB-16FC-4028-AC2D-F12A56AA422A" );
            // Add Block to Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03F7B803-F5E1-4129-B29F-3B7E65C443A0", "", "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "File Type Entry Number Chart", "Main", "", "", 3, "196E3F08-B09B-400B-AFA4-EB0F0C044F49" );
            // Add Block to Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03F7B803-F5E1-4129-B29F-3B7E65C443A0", "", "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "Average Entry Size Chart", "Main", "", "", 4, "17277CE9-BD20-41B5-9DC3-1B14295816EE" );
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
            // Attrib for BlockType: Dynamic Chart:Query Params
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Query Params", "QueryParams", "", "The parameters that the stored procedure expects in the format of 'param1=value;param2=value'. Any parameter with the same name as a page parameter (i.e. querystring, form, or page route) will have its value replaced with the page's current value. A parameter with the name of 'CurrentPersonId' will have its value replaced with the currently logged in person's id.", 0, @"", "0D7A45A6-C885-44CD-9FA9-B8F431D943B5" );
            // Attrib for BlockType: Dynamic Chart:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "The title of the widget", 0, @"", "94FCBF87-B633-4F02-887F-A99970129319" );
            // Attrib for BlockType: Dynamic Chart:Chart Height
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Chart Height", "ChartHeight", "", "", 0, @"200", "BFAF6EB4-5181-4473-A222-3C2F12982956" );
            // Attrib for BlockType: Dynamic Chart:SQL
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQL", "SQL", "", @"The SQL for the datasource. Output columns must be as follows:
< ul >
    < li > Bar or Line Chart
        <ul>
           < li >[SeriesName] : string or numeric </ li >
           < li >[DateTime] : DateTime </ li >
           < li >[YValue] : numeric </ li >
        </ ul >
    </ li >
    < li > Pie Chart
        <ul>
           < li >[MetricTitle] : string </ li >
           < li >[YValueTotal] : numeric </ li >
        </ ul >
    </ li >
</ ul >

Example: 
< code >< pre >
--get top 25 viewed pages from the last 30 days( excluding Home )
select top 25 * from(
    select
        distinct
        pv.PageTitle[SeriesName],
        convert( date, pv.DateTimeViewed )[DateTime],
        count( *)[YValue]
    from
        PageView pv
    where PageTitle is not null
    group by pv.PageTitle, convert( date, pv.DateTimeViewed )
    ) x where SeriesID != 'Home'
and DateTime > DateAdd( day, -30, SysDateTime() )
order by YValue desc
</ pre >
</ code > ",0,@"","C95DFED0-D082-4D47-A4C0-ADD8664A9CA8");
            // Attrib for BlockType: Dynamic Chart:Subtitle
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "", "The subtitle of the widget", 1, @"", "3006CAEC-2DBD-4C87-BCA7-5AB23657497E" );
            // Attrib for BlockType: Dynamic Chart:Column Width
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Column Width", "ColumnWidth", "", "The width of the widget.", 2, @"4", "9B4B5CBF-1E80-4CCA-8B6D-806806038188" );
            // Attrib for BlockType: Dynamic Chart:Chart Style
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 3, @"", "BAAE0B29-7D8C-4369-93F2-E1B624882170" );
            // Attrib for BlockType: Dynamic Chart:Show Legend
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legend", "ShowLegend", "", "", 7, @"True", "55D7DEB8-1D23-430B-81EF-D7F42FFAD518" );
            // Attrib for BlockType: Dynamic Chart:Legend Position
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Legend Position", "LegendPosition", "", "Select the position of the Legend (corner)", 8, @"ne", "398336FF-64CD-4EB8-A78C-9C9DAEBF68FA" );
            // Attrib for BlockType: Dynamic Chart:Chart Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Chart Type", "ChartType", "", "", 9, @"Line", "F5626814-8713-420D-A93C-CFE6E89321DF" );
            // Attrib for BlockType: Dynamic Chart:Pie Inner Radius
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Pie Inner Radius", "PieInnerRadius", "", "If this is a pie chart, specific the inner radius to have a donut hole. For example, specify: 0.75 to have the inner radius as 75% of the outer radius.", 10, @"0", "4CAF4C44-586D-490E-918D-52C0BD81BB3C" );
            // Attrib for BlockType: Dynamic Chart:Pie Show Labels
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Pie Show Labels", "PieShowLabels", "", "If this is a pie chart, specify if labels show be shown", 11, @"True", "3B54049D-60A4-4473-BA10-49101E06F150" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Query Params Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Update Page Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "230EDFE8-33CA-478D-8C9A-572323AF3466", @"True" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Show Columns Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "202A82BF-7772-481C-8419-600012607972", @"False" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Url Mask Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "B9163A35-E09C-466D-8A2D-4ED81DF0114C", @"" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Query Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"SELECT Top 10
    t.NAME AS TableName,
    s.Name AS SchemaName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB, 
    SUM(a.used_pages) * 8 AS UsedSpaceKB, 
    (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
FROM 
    sys.tables t
INNER JOIN      
    sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN 
    sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN 
    sys.allocation_units a ON p.partition_id = a.container_id
LEFT OUTER JOIN 
    sys.schemas s ON t.schema_id = s.schema_id
WHERE 
    t.NAME NOT LIKE 'dt%' 
    AND t.is_ms_shipped = 0
    AND i.OBJECT_ID > 255 
GROUP BY 
    t.Name, s.Name, p.Rows
ORDER BY 
    UsedSpaceKB desc" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Columns Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "90B0E6AF-B2F4-4397-953B-737A40D4023B", @"" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Merge Fields Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "8EB882CE-5BB1-4844-9C28-10190903EECD", @"" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Formatted Output Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "6A233402-446C-47E9-94A5-6A247C29BC21", @"" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Person Report Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C", @"False" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Communication Recipient Person Id Columns Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "75DDB977-9E71-44E8-924B-27134659D3A4", @"" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Show Excel Export Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "E11B57E5-EC7D-4C42-9ADA-37594D71F145", @"True" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Show Communicate Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "5B2C115A-C187-4AB3-93AE-7010644B39DA", @"False" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Show Merge Person Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E", @"False" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Show Bulk Update Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78", @"False" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Stored Procedure Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "A4439703-5432-489A-9C14-155903D6A43E", @"False" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Show Merge Template Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "6697B0A2-C8FE-497A-B5B4-A9D459474338", @"True" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Paneled Grid Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "5449CB61-2DFC-4B55-A697-38F1C2AF128B", @"False" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Show Grid Filter Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C", @"True" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Timeout Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "BEEE38DD-2791-4242-84B6-0495904143CC", @"30" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Page Title Lava Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "3F4BA170-F5C5-405E-976F-0AFBB8855FE8", @"" );
            // Attrib Value for Block:Top Ten Tables, Attribute:Encrypted Fields Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03738EAB-BE06-4DEA-AE73-2EFEC404F586", "AF7714D4-D825-419A-B136-FF8293396635", @"" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Communication Recipient Person Id Columns Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "75DDB977-9E71-44E8-924B-27134659D3A4", @"" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Show Excel Export Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "E11B57E5-EC7D-4C42-9ADA-37594D71F145", @"True" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Show Communicate Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "5B2C115A-C187-4AB3-93AE-7010644B39DA", @"False" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Show Merge Person Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E", @"False" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Show Bulk Update Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78", @"False" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Stored Procedure Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "A4439703-5432-489A-9C14-155903D6A43E", @"False" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Show Merge Template Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "6697B0A2-C8FE-497A-B5B4-A9D459474338", @"True" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Paneled Grid Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "5449CB61-2DFC-4B55-A697-38F1C2AF128B", @"False" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Show Grid Filter Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C", @"True" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Timeout Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "BEEE38DD-2791-4242-84B6-0495904143CC", @"30" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Page Title Lava Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "3F4BA170-F5C5-405E-976F-0AFBB8855FE8", @"" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Encrypted Fields Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "AF7714D4-D825-419A-B136-FF8293396635", @"" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Merge Fields Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "8EB882CE-5BB1-4844-9C28-10190903EECD", @"" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Formatted Output Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "6A233402-446C-47E9-94A5-6A247C29BC21", @"" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Person Report Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C", @"False" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Columns Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "90B0E6AF-B2F4-4397-953B-737A40D4023B", @"" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Query Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"SELECT bft.Name, SUM(DATALENGTH(bfd.Content))/1024 AS 'Size KB', SUM(DATALENGTH(bfd.Content))/1048576 AS 'Size MB', COUNT(0) AS '# of Rows', (SUM(DATALENGTH(bfd.Content))/1024)/COUNT(0) AS 'Size per row KB'
FROM [BinaryFile] bf
JOIN [BinaryFileData] bfd ON bfd.Id = bf.Id
JOIN [BinaryFileType] bft ON bf.BinaryFileTypeId = bft.Id
GROUP BY bft.Name
ORDER BY [Size per row KB] DESC" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Url Mask Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "B9163A35-E09C-466D-8A2D-4ED81DF0114C", @"" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Show Columns Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "202A82BF-7772-481C-8419-600012607972", @"False" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Update Page Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "230EDFE8-33CA-478D-8C9A-572323AF3466", @"True" );
            // Attrib Value for Block:File Type Breakdown, Attribute:Query Params Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Query Params Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "0D7A45A6-C885-44CD-9FA9-B8F431D943B5", @"" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Title Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "94FCBF87-B633-4F02-887F-A99970129319", @"File Type Size (KB)" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Chart Height Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "BFAF6EB4-5181-4473-A222-3C2F12982956", @"200" );
            // Attrib Value for Block:File Type Size Chart, Attribute:SQL Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "C95DFED0-D082-4D47-A4C0-ADD8664A9CA8", @"SELECT bft.Name AS 'SeriesId', Sum(DATALENGTH(bfd.Content))/1024 AS 'YValue'
FROM [BinaryFile] bf
JOIN [BinaryFileData] bfd ON bfd.Id = bf.Id
JOIN [BinaryFileType] bft ON bf.BinaryFileTypeId = bft.Id
GROUP BY bft.Name
ORDER BY [YValue] DESC" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Subtitle Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "3006CAEC-2DBD-4C87-BCA7-5AB23657497E", @"" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Column Width Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "9B4B5CBF-1E80-4CCA-8B6D-806806038188", @"4" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Chart Style Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "BAAE0B29-7D8C-4369-93F2-E1B624882170", @"b45da8e1-b9a6-46fd-9a2b-e8440d7d6aac" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Show Legend Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "55D7DEB8-1D23-430B-81EF-D7F42FFAD518", @"True" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Legend Position Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "398336FF-64CD-4EB8-A78C-9C9DAEBF68FA", @"ne" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Chart Type Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "F5626814-8713-420D-A93C-CFE6E89321DF", @"Pie" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Pie Inner Radius Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "4CAF4C44-586D-490E-918D-52C0BD81BB3C", @"0" );
            // Attrib Value for Block:File Type Size Chart, Attribute:Pie Show Labels Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "676412EB-16FC-4028-AC2D-F12A56AA422A", "3B54049D-60A4-4473-BA10-49101E06F150", @"False" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Query Params Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "0D7A45A6-C885-44CD-9FA9-B8F431D943B5", @"" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Title Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "94FCBF87-B633-4F02-887F-A99970129319", @"Number of Entries" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Chart Height Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "BFAF6EB4-5181-4473-A222-3C2F12982956", @"200" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:SQL Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "C95DFED0-D082-4D47-A4C0-ADD8664A9CA8", @"SELECT bft.Name AS 'SeriesId', COUNT(0) AS 'YValue'
FROM [BinaryFile] bf
JOIN [BinaryFileData] bfd ON bfd.Id = bf.Id
JOIN [BinaryFileType] bft ON bf.BinaryFileTypeId = bft.Id
GROUP BY bft.Name
ORDER BY [YValue] DESC" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Subtitle Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "3006CAEC-2DBD-4C87-BCA7-5AB23657497E", @"" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Column Width Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "9B4B5CBF-1E80-4CCA-8B6D-806806038188", @"4" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Chart Style Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "BAAE0B29-7D8C-4369-93F2-E1B624882170", @"b45da8e1-b9a6-46fd-9a2b-e8440d7d6aac" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Show Legend Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "55D7DEB8-1D23-430B-81EF-D7F42FFAD518", @"True" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Legend Position Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "398336FF-64CD-4EB8-A78C-9C9DAEBF68FA", @"ne" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Chart Type Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "F5626814-8713-420D-A93C-CFE6E89321DF", @"Pie" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Pie Inner Radius Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "4CAF4C44-586D-490E-918D-52C0BD81BB3C", @"0" );
            // Attrib Value for Block:File Type Entry Number Chart, Attribute:Pie Show Labels Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "196E3F08-B09B-400B-AFA4-EB0F0C044F49", "3B54049D-60A4-4473-BA10-49101E06F150", @"False" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Query Params Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "0D7A45A6-C885-44CD-9FA9-B8F431D943B5", @"" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Title Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "94FCBF87-B633-4F02-887F-A99970129319", @"Average Entry Size (KB)" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Chart Height Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "BFAF6EB4-5181-4473-A222-3C2F12982956", @"200" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:SQL Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "C95DFED0-D082-4D47-A4C0-ADD8664A9CA8", @"SELECT bft.Name AS 'SeriesId', (SUM(DATALENGTH(bfd.Content))/1024)/COUNT(0) AS 'YValue'
FROM [BinaryFile] bf
JOIN [BinaryFileData] bfd ON bfd.Id = bf.Id
JOIN [BinaryFileType] bft ON bf.BinaryFileTypeId = bft.Id
GROUP BY bft.Name
ORDER BY [YValue] DESC" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Subtitle Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "3006CAEC-2DBD-4C87-BCA7-5AB23657497E", @"" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Column Width Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "9B4B5CBF-1E80-4CCA-8B6D-806806038188", @"4" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Chart Style Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "BAAE0B29-7D8C-4369-93F2-E1B624882170", @"b45da8e1-b9a6-46fd-9a2b-e8440d7d6aac" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Show Legend Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "55D7DEB8-1D23-430B-81EF-D7F42FFAD518", @"True" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Legend Position Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "398336FF-64CD-4EB8-A78C-9C9DAEBF68FA", @"ne" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Chart Type Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "F5626814-8713-420D-A93C-CFE6E89321DF", @"Pie" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Pie Inner Radius Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "4CAF4C44-586D-490E-918D-52C0BD81BB3C", @"0" );
            // Attrib Value for Block:Average Entry Size Chart, Attribute:Pie Show Labels Page: Database Size, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "17277CE9-BD20-41B5-9DC3-1B14295816EE", "3B54049D-60A4-4473-BA10-49101E06F150", @"False" );

        }

        public override void Down()
        {
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
            RockMigrationHelper.DeleteAttribute( "3F4BA170-F5C5-405E-976F-0AFBB8855FE8" );
            RockMigrationHelper.DeleteAttribute( "BEEE38DD-2791-4242-84B6-0495904143CC" );
            RockMigrationHelper.DeleteAttribute( "E582FD3C-9990-47D1-A57F-A3DB753B1D0C" );
            RockMigrationHelper.DeleteAttribute( "5449CB61-2DFC-4B55-A697-38F1C2AF128B" );
            RockMigrationHelper.DeleteAttribute( "6697B0A2-C8FE-497A-B5B4-A9D459474338" );
            RockMigrationHelper.DeleteAttribute( "A4439703-5432-489A-9C14-155903D6A43E" );
            RockMigrationHelper.DeleteAttribute( "D01510AA-1B8D-467C-AFC6-F7554CB7CF78" );
            RockMigrationHelper.DeleteAttribute( "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E" );
            RockMigrationHelper.DeleteAttribute( "5B2C115A-C187-4AB3-93AE-7010644B39DA" );
            RockMigrationHelper.DeleteAttribute( "E11B57E5-EC7D-4C42-9ADA-37594D71F145" );
            RockMigrationHelper.DeleteAttribute( "75DDB977-9E71-44E8-924B-27134659D3A4" );
            RockMigrationHelper.DeleteAttribute( "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C" );
            RockMigrationHelper.DeleteAttribute( "6A233402-446C-47E9-94A5-6A247C29BC21" );
            RockMigrationHelper.DeleteAttribute( "8EB882CE-5BB1-4844-9C28-10190903EECD" );
            RockMigrationHelper.DeleteAttribute( "202A82BF-7772-481C-8419-600012607972" );
            RockMigrationHelper.DeleteAttribute( "B9163A35-E09C-466D-8A2D-4ED81DF0114C" );
            RockMigrationHelper.DeleteAttribute( "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356" );
            RockMigrationHelper.DeleteAttribute( "90B0E6AF-B2F4-4397-953B-737A40D4023B" );
            RockMigrationHelper.DeleteAttribute( "B0EC41B9-37C0-48FD-8E4E-37A8CA305012" );
            RockMigrationHelper.DeleteAttribute( "230EDFE8-33CA-478D-8C9A-572323AF3466" );
            RockMigrationHelper.DeleteBlock( "17277CE9-BD20-41B5-9DC3-1B14295816EE" );
            RockMigrationHelper.DeleteBlock( "196E3F08-B09B-400B-AFA4-EB0F0C044F49" );
            RockMigrationHelper.DeleteBlock( "676412EB-16FC-4028-AC2D-F12A56AA422A" );
            RockMigrationHelper.DeleteBlock( "25D63EA5-A51A-4FC9-BA85-2AB5DE2120BA" );
            RockMigrationHelper.DeleteBlock( "03738EAB-BE06-4DEA-AE73-2EFEC404F586" );
            RockMigrationHelper.DeleteBlockType( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723" );
            RockMigrationHelper.DeleteBlockType( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" );
            RockMigrationHelper.DeletePage( "03F7B803-F5E1-4129-B29F-3B7E65C443A0" ); //  Page: Database Size
        }
    }
}

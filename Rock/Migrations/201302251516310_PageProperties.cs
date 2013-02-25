//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class PageProperties : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update checkin pages
            Sql( @"
DECLARE @PageId int
DECLARE @SiteId int

-- Create Checkin Site
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D47858C0-0E6E-46DC-AE99-8EC84BA5F45F')
INSERT INTO [Site] (IsSystem, Name, Description, Theme, DefaultPageId, Guid)
    VALUES (1, 'Checkin', 'The Rock default checkin site', 'CheckinPark', @PageId, '15AEFC01-ACB3-4F5D-B83E-AB3AB7F2A54A')
SET @SiteId = SCOPE_IDENTITY()

-- Update Checkin pages to use new site and different layout
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CDF2C599-D341-42FD-B7DC-CD402EA96050')
UPDATE [Page] SET 
    [SiteId] = @SiteId,
    [Layout] = 'Checkin'
WHERE [ParentPageId] = @PageId
" );

            // Create new reporting page
            AddPage( "98163C8B-5C91-4A68-BB79-6AD948A604CE", "Reporting", "Reporting Pages", "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B" );

            AddPage( "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B", "Data Views", "Edit the available Data Views that are used for filtering data", "TwoColumnLeft", "4011CB37-28AA-46C4-99D5-826F4A9CADF5" );
            AddPage( "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B", "Report Sample", "Sample Report", "Default", "B49F076A-D8A5-42C2-8CB8-8B80C4DF992D" );
            AddPage( "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B", "SQL Command", "", "Default", "03C49950-9C4C-4668-9C65-9A0DF43D1B33" );
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Data Filters", "", "Default", "5537F375-B652-4603-8E04-119C74414CD7" );

            AddBlockType( "Reporting - Dynamic Data", "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "~/Blocks/Reporting/DynamicData.ascx", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" );
            AddBlockType( "Utility - Sql Command", "Block to execute dynamic display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "~/Blocks/Utility/SqlCommand.ascx", "89EAFE90-7082-4FF2-BC87-F50BFDB53298" );
            
            AddBlock( "B49F076A-D8A5-42C2-8CB8-8B80C4DF992D", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Dynamic Report", "", "Content", 0, "50419243-3DDC-4E04-8C0C-A86A24E328B7" );
            AddBlock( "03C49950-9C4C-4668-9C65-9A0DF43D1B33", "89EAFE90-7082-4FF2-BC87-F50BFDB53298", "SQL Command", "", "Content", 0, "9812642C-1B64-4F86-8C80-701973A49518" );
            AddBlock( "5537F375-B652-4603-8E04-119C74414CD7", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Filters", "", "Content", 0, "B6F6DBF7-96CA-4A6A-AFB3-ED2278EEB70E" );

            AddBlock( "4011CB37-28AA-46C4-99D5-826F4A9CADF5", "ADE003C7-649B-466A-872B-B8AC952E7841", "Category Tree", "", "LeftContent", 0, "6A9111AC-34E7-4103-A12A-9A89C2A14B57" );
            AddBlock( "4011CB37-28AA-46C4-99D5-826F4A9CADF5", "EB279DF9-D817-4905-B6AC-D9883F0DA2E4", "Data Views", "", "RightContent", 0, "7868AF5C-6512-4F33-B127-93B159E08A56" );
            AddBlock( "4011CB37-28AA-46C4-99D5-826F4A9CADF5", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Category Detail", "", "RightContent", 1, "5BAD3495-6434-4663-A940-1DAC3AC0B643" );

            AddBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Xslt File Path", "XsltFilePath", "", "The Xslt file path relative to the current theme's Assets/Xslt folder (if query returns xml that should be transformed)", 6, "", "F2E425FC-3C7E-4D3D-8FEB-19D860B62CD8" );
            AddBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Update Page", "UpdatePage", "", "If True, provides fields for updating the parent page's Name and Description", 0, "True", "230EDFE8-33CA-478D-8C9A-572323AF3466" );
            AddBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Query Params", "QueryParams", "", "Parameters to pass to query", 2, "", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012" );
            AddBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Columns", "Columns", "", "The columns to hide or show", 5, "", "90B0E6AF-B2F4-4397-953B-737A40D4023B" );
            AddBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Query", "Query", "", "The query to execute", 1, "", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356" );
            AddBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Url Mask", "UrlMask", "", "The Url to redirect to when a row is clicked", 3, "", "B9163A35-E09C-466D-8A2D-4ED81DF0114C" );
            AddBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Columns", "ShowColumns", "", "Should the 'Columns' specified below be the only ones shown (vs. the only ones hidden)", 4, "False", "202A82BF-7772-481C-8419-600012607972" );

            AddBlockAttributeValue( "B6F6DBF7-96CA-4A6A-AFB3-ED2278EEB70E", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.DataFilters.DataFilterContainer, Rock" );

            AddBlockAttributeValue( "6A9111AC-34E7-4103-A12A-9A89C2A14B57", "AA057D3E-00CC-42BD-9998-600873356EDB", "DataViewId" );
            AddBlockAttributeValue( "6A9111AC-34E7-4103-A12A-9A89C2A14B57", "06D414F0-AA20-4D3C-B297-1530CCD64395", "D550E301-4D3F-4E72-AD19-A0E694304AF3" );
            AddBlockAttributeValue( "6A9111AC-34E7-4103-A12A-9A89C2A14B57", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", "4011CB37-28AA-46C4-99D5-826F4A9CADF5" );

            Sql( @"
IF NOT EXISTS(SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DataView')
BEGIN
    INSERT INTO [EntityType] ([Name],[Guid],[IsEntity],[IsSecured])
    VALUES ('Rock.Model.DataView', newid(), 1, 1)
END

DECLARE @EntityTypeId int
SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DataView')
UPDATE [AttributeValue] SET [Value] = CAST(@EntityTypeId as varchar) WHERE [Value] = 'D550E301-4D3F-4E72-AD19-A0E694304AF3'

UPDATE [dbo].[Page] SET
    MenuDisplayDescription = 0,
    IconCssClass = 'icon-filter'
WHERE [Guid] = '5537F375-B652-4603-8E04-119C74414CD7'
" );
            DeleteAttribute( "20E83814-FBB9-47A1-9B99-702EB6750937" );
            DeleteBlock( "49B797DC-8D45-48CB-8AF6-849BDFBC6ABE" );
            DeleteBlock( "5FAAD78F-2681-4B4B-83D2-02B7C712D6EA" );
            DeleteBlockType( "A1F764A2-B076-4AE7-96A1-D5AEBFD1EDE9" );
            DeletePage( "18FAD918-5B42-4523-BBE1-FF2A08C647BF" );
            DeletePage( "FDA8A444-9132-4905-857B-41B3A38C6D22" );
        }
        

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "F2E425FC-3C7E-4D3D-8FEB-19D860B62CD8" ); // Xslt File Path
            DeleteAttribute( "230EDFE8-33CA-478D-8C9A-572323AF3466" ); // Update Page
            DeleteAttribute( "B0EC41B9-37C0-48FD-8E4E-37A8CA305012" ); // Query Params
            DeleteAttribute( "90B0E6AF-B2F4-4397-953B-737A40D4023B" ); // Columns
            DeleteAttribute( "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356" ); // Query
            DeleteAttribute( "B9163A35-E09C-466D-8A2D-4ED81DF0114C" ); // Url Mask
            DeleteAttribute( "202A82BF-7772-481C-8419-600012607972" ); // Show Columns

            DeleteBlock( "7868AF5C-6512-4F33-B127-93B159E08A56" ); // Data Views
            DeleteBlock( "6A9111AC-34E7-4103-A12A-9A89C2A14B57" ); // Category Tree
            DeleteBlock( "5BAD3495-6434-4663-A940-1DAC3AC0B643" ); // Category Detail

            DeleteBlock( "B6F6DBF7-96CA-4A6A-AFB3-ED2278EEB70E" ); // Filters
            DeleteBlock( "50419243-3DDC-4E04-8C0C-A86A24E328B7" ); // Dynamic Report
            DeleteBlock( "9812642C-1B64-4F86-8C80-701973A49518" ); // SQL Command

            DeleteBlockType( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126" ); // Reporting - Dynamic Data
            DeleteBlockType( "89EAFE90-7082-4FF2-BC87-F50BFDB53298" ); // Utility - Sql Command

            DeletePage( "5537F375-B652-4603-8E04-119C74414CD7" ); // Data Filters
            DeletePage( "B49F076A-D8A5-42C2-8CB8-8B80C4DF992D" ); // Report Sample
            DeletePage( "03C49950-9C4C-4668-9C65-9A0DF43D1B33" ); // SQL Command
            DeletePage( "4011CB37-28AA-46C4-99D5-826F4A9CADF5" ); // Data Views

            Sql( @"
DECLARE @PageId int
DECLARE @SiteId int

-- Update checkin pages to use old site and layout
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CDF2C599-D341-42FD-B7DC-CD402EA96050')
SET @SiteId = (SELECT [Id] FROM [Site] WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4')
UPDATE [Page] SET 
    [SiteId] = @SiteId,
    [Layout] = 'Default'
WHERE [ParentPageId] = @PageId

-- Delete checkin site
DELETE [Site] WHERE [Guid] = '15AEFC01-ACB3-4F5D-B83E-AB3AB7F2A54A'
" );
            // Delete Reporting Page
            DeletePage( "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B" );

            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Data Views", "Edit the available Data Views that are used for filtering data", "Default", "FDA8A444-9132-4905-857B-41B3A38C6D22" );
            AddPage( "FDA8A444-9132-4905-857B-41B3A38C6D22", "Data View Detail", "", "Default", "18FAD918-5B42-4523-BBE1-FF2A08C647BF" );
            AddBlockType( "Reporting - Data View List", "", "~/Blocks/Reporting/DataViewList.ascx", "A1F764A2-B076-4AE7-96A1-D5AEBFD1EDE9" );
            AddBlock( "18FAD918-5B42-4523-BBE1-FF2A08C647BF", "EB279DF9-D817-4905-B6AC-D9883F0DA2E4", "Data View Detail", "", "Content", 0, "49B797DC-8D45-48CB-8AF6-849BDFBC6ABE" );
            AddBlock( "FDA8A444-9132-4905-857B-41B3A38C6D22", "A1F764A2-B076-4AE7-96A1-D5AEBFD1EDE9", "Data Views", "", "Content", 0, "5FAAD78F-2681-4B4B-83D2-02B7C712D6EA" );
            AddBlockTypeAttribute( "A1F764A2-B076-4AE7-96A1-D5AEBFD1EDE9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "20E83814-FBB9-47A1-9B99-702EB6750937" );

            // Attrib Value for Data Views:Detail Page Guid
            AddBlockAttributeValue( "5FAAD78F-2681-4B4B-83D2-02B7C712D6EA", "20E83814-FBB9-47A1-9B99-702EB6750937", "18fad918-5b42-4523-bbe1-ff2a08c647bf" );

            Sql( @"
UPDATE [dbo].[Page] SET
    MenuDisplayDescription = 0,
    IconCssClass = 'icon-filter'
WHERE [Guid] = 'FDA8A444-9132-4905-857B-41B3A38C6D22'
" );
        }
    }
}

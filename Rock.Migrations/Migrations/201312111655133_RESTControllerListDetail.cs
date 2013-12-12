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
    public partial class RESTControllerListDetail : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "195BCD57-1C10-4969-886F-7324B6287B75", "REST Controllers", "", "0D51F443-1C0D-4C71-8BAE-E5F5A35E8B79", "fa fa-exchange" ); // Site:Rock Internal
            AddPage( "0D51F443-1C0D-4C71-8BAE-E5F5A35E8B79", "195BCD57-1C10-4969-886F-7324B6287B75", "REST Controller Detail", "", "7F5EF1AA-0E27-4AA1-A5E1-1CD6DDDCDDC5", "" ); // Site:Rock Internal
            AddBlockType( "Core - Rest Controller Detail", "", "~/Blocks/Core/RestControllerDetail.ascx", "20AD75DD-0DF3-49E9-9DB1-8537C12B1664" );
            AddBlockType( "Core - Rest Controller List", "", "~/Blocks/Core/RestControllerList.ascx", "7BF616C1-CE1D-4EF0-B56F-B9810B811192" );
            // Add Block to Page: REST Controllers, Site: Rock Internal
            AddBlock( "0D51F443-1C0D-4C71-8BAE-E5F5A35E8B79", "", "7BF616C1-CE1D-4EF0-B56F-B9810B811192", "Rest Controller List", "Main", 0, "9319A11D-1691-436D-86D1-06C5307B468C" );

            // Add Block to Page: REST Controller Detail, Site: Rock Internal
            AddBlock( "7F5EF1AA-0E27-4AA1-A5E1-1CD6DDDCDDC5", "", "20AD75DD-0DF3-49E9-9DB1-8537C12B1664", "Rest Controller Detail", "Main", 0, "E0AF31B6-8BEC-437B-B14F-BFD4F89ED2C5" );

            // Attrib for BlockType: Core - Rest Controller List:Detail Page
            AddBlockTypeAttribute( "7BF616C1-CE1D-4EF0-B56F-B9810B811192", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "C1CBC008-59F7-4DB3-9FA2-7646860FDDE4" );

            // Attrib for BlockType: Core - Rest Controller Detail:Detail Page
            AddBlockTypeAttribute( "20AD75DD-0DF3-49E9-9DB1-8537C12B1664", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "BE5E3C03-C10D-48AF-8EC2-3CF1E1C36940" );

            // Attrib Value for Block:Rest Controller List, Attribute:Detail Page Page: REST Controllers, Site: Rock Internal
            AddBlockAttributeValue( "9319A11D-1691-436D-86D1-06C5307B468C", "C1CBC008-59F7-4DB3-9FA2-7646860FDDE4", @"7f5ef1aa-0e27-4aa1-a5e1-1cd6dddcddc5" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Core - Rest Controller Detail:Detail Page
            DeleteAttribute( "BE5E3C03-C10D-48AF-8EC2-3CF1E1C36940" );
            // Attrib for BlockType: Core - Rest Controller List:Detail Page
            DeleteAttribute( "C1CBC008-59F7-4DB3-9FA2-7646860FDDE4" );
            // Remove Block: Rest Controller Detail, from Page: REST Controller Detail, Site: Rock Internal
            DeleteBlock( "E0AF31B6-8BEC-437B-B14F-BFD4F89ED2C5" );
            // Remove Block: Rest Controller List, from Page: REST Controllers, Site: Rock Internal
            DeleteBlock( "9319A11D-1691-436D-86D1-06C5307B468C" );
            DeleteBlockType( "7BF616C1-CE1D-4EF0-B56F-B9810B811192" ); // Core - Rest Controller List
            DeleteBlockType( "20AD75DD-0DF3-49E9-9DB1-8537C12B1664" ); // Core - Rest Controller Detail
            DeletePage( "7F5EF1AA-0E27-4AA1-A5E1-1CD6DDDCDDC5" ); // Page: REST Controller DetailLayout: Full Width, Site: Rock Internal
            DeletePage( "0D51F443-1C0D-4C71-8BAE-E5F5A35E8B79" ); // Page: REST ControllersLayout: Full Width, Site: Rock Internal
        }
    }
}

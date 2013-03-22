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
    public partial class CategoryDetail : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Administration - Category Detail", "", "~/Blocks/Administration/CategoryDetail.ascx", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C" );
            AddBlockType( "Cms - Marketing Campaign Ads Xslt", "", "~/Blocks/Cms/MarketingCampaignAdsXslt.ascx", "5A880084-7237-449A-9855-3FA02B6BD79F" );
            AddBlock( "DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Category Detail", "", "RightContent", 1, "DE94148B-7DF8-418D-A905-066740765AD4" );
            AddBlockTypeAttribute( "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "EntityTypeName", "EntityTypeName", "", "Category Entity Type", 0, "", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297" );

            // Attrib Value for Category Detail:EntityTypeName
            AddBlockAttributeValue( "DE94148B-7DF8-418D-A905-066740765AD4", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "Rock.Model.WorkflowType" );        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "FF3A33CF-8897-4FC6-9C16-64FA25E6C297" );
            DeleteBlock( "DE94148B-7DF8-418D-A905-066740765AD4" );
            DeleteBlockType( "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C" );
            DeleteBlockType( "5A880084-7237-449A-9855-3FA02B6BD79F" );
        }
    }
}

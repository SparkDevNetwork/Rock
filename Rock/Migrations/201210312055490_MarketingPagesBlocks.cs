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
    public partial class MarketingPagesBlocks : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Marketing Campaigns", "Allows for configuration of Marketing Campaigns", "~/Blocks/Administration/MarketingCampaigns.ascx", "D99313E1-EFF0-4339-8296-4FA4922B48B7" );
            AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "Marketing Campaigns", "Manage Marketing Campaigns", "74345663-5BCA-493C-A2FB-80DC9CC8E70C" );
            AddBlock( "74345663-5BCA-493C-A2FB-80DC9CC8E70C", "D99313E1-EFF0-4339-8296-4FA4922B48B7", "Marketing Campaigns", "Content", "C26C8EB3-0BF3-4D5E-A685-BC6C9B9246D8" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "C26C8EB3-0BF3-4D5E-A685-BC6C9B9246D8" );
            DeletePage( "74345663-5BCA-493C-A2FB-80DC9CC8E70C" );
            DeleteBlockType( "D99313E1-EFF0-4339-8296-4FA4922B48B7" );
        }
    }
}

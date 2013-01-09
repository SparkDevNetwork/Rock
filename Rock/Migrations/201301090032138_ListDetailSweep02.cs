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
    public partial class ListDetailSweep02 : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Delete old MarketingCampaignAdTypes Block
            DeleteBlock( "EC20DBD8-9C33-45FF-B4AE-64D460350FE0" );
            DeleteBlockType( "D780A160-621F-4531-8369-B5DEC6E7174A" );

            AddPage( "E6F5F06B-65EE-4949-AA56-1FE4E2933C63", "Marketing Campaign Ad Types Detail", "", "Default", "36826974-C613-48F2-877E-460C4EC90CCE" );

            AddBlockType( "Administration - Marketing Campaign Ad Type Detail", "", "~/Blocks/Administration/MarketingCampaignAdTypeDetail.ascx", "9373F86E-88C8-40E8-9D70-AE6CFC5DD980" );
            AddBlockType( "Administration - Marketing Campaign Ad Type List", "", "~/Blocks/Administration/MarketingCampaignAdTypeList.ascx", "7C70D1F6-78BB-4196-8A35-276DD06F8AFE" );

            AddBlock( "E6F5F06B-65EE-4949-AA56-1FE4E2933C63", "7C70D1F6-78BB-4196-8A35-276DD06F8AFE", "Marketing Campaign Ad Types List", "Content", "9F878185-9DAB-4866-B233-DFF0DA988AAC", 0 );
            AddBlock( "36826974-C613-48F2-877E-460C4EC90CCE", "9373F86E-88C8-40E8-9D70-AE6CFC5DD980", "Marketing Campaign Ad Types Detail", "Content", "16011084-CB78-4E9E-9A78-40FB13BA2C40", 0 );

            AddBlockTypeAttribute( "7C70D1F6-78BB-4196-8A35-276DD06F8AFE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "302905C1-BD31-46D9-9082-663EF468C371" );

            // Attrib Value for Marketing Campaign Ad Types List:Detail Page Guid
            AddBlockAttributeValue( "9F878185-9DAB-4866-B233-DFF0DA988AAC", "302905C1-BD31-46D9-9082-663EF468C371", "36826974-c613-48f2-877e-460c4ec90cce" );

            // Set CMS Page to only show 1st level of pages
            Sql( "update [AttributeValue] set [Value] = '1' where [Guid] = '0A38DD79-F27B-40A6-A7BC-6909C0714D29'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Set CMS Page to back to showing 2 levels of pages
            Sql( "update [AttributeValue] set [Value] = '2' where [Guid] = '0A38DD79-F27B-40A6-A7BC-6909C0714D29'" );
            
            DeleteAttribute( "302905C1-BD31-46D9-9082-663EF468C371" );
            DeleteBlock( "9F878185-9DAB-4866-B233-DFF0DA988AAC" );
            DeleteBlock( "16011084-CB78-4E9E-9A78-40FB13BA2C40" );
            DeleteBlockType( "9373F86E-88C8-40E8-9D70-AE6CFC5DD980" );
            DeleteBlockType( "7C70D1F6-78BB-4196-8A35-276DD06F8AFE" );
            DeletePage( "36826974-C613-48F2-877E-460C4EC90CCE" );        }
    }
}

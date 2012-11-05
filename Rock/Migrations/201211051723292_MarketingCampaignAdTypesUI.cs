//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MarketingCampaignAdTypesUI : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Marketing Campaign Ad Types", "Manage Marketing Campaign Ad Types", "~/Blocks/Administration/MarketingCampaignAdTypes.ascx", "D780A160-621F-4531-8369-B5DEC6E7174A" );
            AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "Marketing Campaign Ad Types", "Manage Marketing Campaign Ad Types", "E6F5F06B-65EE-4949-AA56-1FE4E2933C63" );
            AddBlock( "E6F5F06B-65EE-4949-AA56-1FE4E2933C63", "D780A160-621F-4531-8369-B5DEC6E7174A", "Marketing Campaign Ad Type", "Content", "EC20DBD8-9C33-45FF-B4AE-64D460350FE0" );

            Sql( @"
INSERT INTO [cmsMarketingCampaignAdType]
           ([IsSystem]
           ,[Name]
           ,[DateRangeType]
           ,[Guid])
     VALUES
           (0
           ,'Bulletin'
           ,1
           ,'3ED7FECC-BCA3-4014-A55E-93FF0D2263C9')" );

            Sql( @"
INSERT INTO [cmsMarketingCampaignAdType]
           ([IsSystem]
           ,[Name]
           ,[DateRangeType]
           ,[Guid])
     VALUES
           (0
           ,'Website'
           ,2
           ,'851745BF-CA38-46DC-B2E4-84F4C5990F54')" );

            Sql( @"
INSERT INTO [cmsMarketingCampaignAdType]
           ([IsSystem]
           ,[Name]
           ,[DateRangeType]
           ,[Guid])
     VALUES
           (0
           ,'Facebook'
           ,2
           ,'4D23C6C8-7457-483C-B749-330D63F3B171')" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [cmsMarketingCampaignAdType] WHERE [Guid] = '4D23C6C8-7457-483C-B749-330D63F3B171'" );
            Sql( @"DELETE FROM [cmsMarketingCampaignAdType] WHERE [Guid] = '851745BF-CA38-46DC-B2E4-84F4C5990F54'" );
            Sql( @"DELETE FROM [cmsMarketingCampaignAdType] WHERE [Guid] = '3ED7FECC-BCA3-4014-A55E-93FF0D2263C9'" );

            DeleteBlock( "EC20DBD8-9C33-45FF-B4AE-64D460350FE0" );
            DeletePage( "E6F5F06B-65EE-4949-AA56-1FE4E2933C63" );
            DeleteBlockType( "D780A160-621F-4531-8369-B5DEC6E7174A" );
        }
    }
}

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
    public partial class MarketingCampaignAds : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Fix up column
            AlterColumn("dbo.cmsMarketingCampaignAdType", "Name", c => c.String(nullable: false, maxLength: 100));

            // Event Attendees Group Type
            Sql( @"INSERT INTO [dbo].[crmGroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[DefaultGroupRoleId]
           ,[Guid])
     VALUES
           (1
           ,'Event Attendees'
           ,'Event Attendees'
           ,null
           ,'3311132B-268D-44E9-811A-A56A0835E50A')" );

            // Starter Audience Types
            AddDefinedType( "Marketing Campaign", "Audience Type", "Audience Type", "799301A3-2026-4977-994E-45DC68502559" );
            AddDefinedValue( "799301A3-2026-4977-994E-45DC68502559", "Kids", "Kids", "F2BFF319-A109-4B42-BEC2-76590E11627B", false );
            AddDefinedValue( "799301A3-2026-4977-994E-45DC68502559", "Adults", "Adults", "95E49778-AE72-454F-91CC-2FC864557DEC", false );
            AddDefinedValue( "799301A3-2026-4977-994E-45DC68502559", "Staff", "Staff", "833EE2C7-F83A-4744-AD14-6907554DF8AE", false );

            // Ad Types Block/Page
            AddBlockType( "Marketing Campaign Ad Types", "Manage Marketing Campaign Ad Types", "~/Blocks/Administration/MarketingCampaignAdTypes.ascx", "D780A160-621F-4531-8369-B5DEC6E7174A" );
            AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "Marketing Campaign Ad Types", "Manage Marketing Campaign Ad Types", "E6F5F06B-65EE-4949-AA56-1FE4E2933C63" );
            AddBlock( "E6F5F06B-65EE-4949-AA56-1FE4E2933C63", "D780A160-621F-4531-8369-B5DEC6E7174A", "Marketing Campaign Ad Type", "Content", "EC20DBD8-9C33-45FF-B4AE-64D460350FE0" );

            // Starter Ad Types
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
            // Delete Ad Types
            Sql( @"DELETE FROM [cmsMarketingCampaignAdType] WHERE [Guid] = '4D23C6C8-7457-483C-B749-330D63F3B171'" );
            Sql( @"DELETE FROM [cmsMarketingCampaignAdType] WHERE [Guid] = '851745BF-CA38-46DC-B2E4-84F4C5990F54'" );
            Sql( @"DELETE FROM [cmsMarketingCampaignAdType] WHERE [Guid] = '3ED7FECC-BCA3-4014-A55E-93FF0D2263C9'" );

            // Ad Types Block/Page
            DeleteBlock( "EC20DBD8-9C33-45FF-B4AE-64D460350FE0" );
            DeletePage( "E6F5F06B-65EE-4949-AA56-1FE4E2933C63" );
            DeleteBlockType( "D780A160-621F-4531-8369-B5DEC6E7174A" );
            
            // Delete Audience Types
            DeleteDefinedValue( "833EE2C7-F83A-4744-AD14-6907554DF8AE" );
            DeleteDefinedValue( "95E49778-AE72-454F-91CC-2FC864557DEC" );
            DeleteDefinedValue( "F2BFF319-A109-4B42-BEC2-76590E11627B" );
            DeleteDefinedType( "799301A3-2026-4977-994E-45DC68502559" );

            // Event Attendees Group Type
            Sql( @"DELETE FROM [dbo].[crmGroupType] where [Guid] = '3311132B-268D-44E9-811A-A56A0835E50A'" );

            // Fix up column
            AlterColumn("dbo.cmsMarketingCampaignAdType", "Name", c => c.String(maxLength: 100));
        }
    }
}

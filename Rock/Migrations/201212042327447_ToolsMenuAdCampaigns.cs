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
    public partial class ToolsMenuAdCampaigns : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "Tools", "", "98163C8B-5C91-4A68-BB79-6AD948A604CE" );
            AddPage( "98163C8B-5C91-4A68-BB79-6AD948A604CE", "Website & Communications", "Tools to maintain and manage external websites.", "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F" );
            AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "Podcasts", "", "9C23D9F1-0734-4CD6-A35A-B8F28C3C674C" );
            AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "HTML Content Approval", "", "9DF95EFF-88B4-401A-8F5F-E3B8DB02A308" );
            AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "Page Administration", "", "0172B025-3CD9-4DA4-9215-E965D5A7EA63" );

            // Move Ad/Marketing Campaigns to Tools menu
            MovePage( "74345663-5BCA-493C-A2FB-80DC9CC8E70C", "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F" );
            Sql( "Update [Page] set [Name] = 'Ad Campaigns', [Title] = 'Ad Campaigns', [Order]='4' where [Guid] = '74345663-5BCA-493C-A2FB-80DC9CC8E70C'" );

            // Move PageRoutes to Tools Menu
            MovePage( "4A833BE3-7D5E-4C38-AF60-5706260015EA", "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F" );
            Sql( "Update [Page] set [Name] = 'Routes', [Title] = 'Routes', [Order]='4' where [Guid] = '4A833BE3-7D5E-4C38-AF60-5706260015EA'" );

            AddSecurityRoleGroup( "Communication Administration", "Group of individuals who can administrate the various parts of the communication functionality.", "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B" );
            AddSecurityAuth( "Rock.Model.MarketingCampaignAd", "Approve", "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", "89F449C0-4FDD-488F-AF9B-088175777FB2" );

            // Grant Edit MarketingCampaignAd to Staff
            AddSecurityAuth( "Rock.Model.MarketingCampaignAd", "Edit", "2C112948-FF4C-46E7-981A-0257681EADF4", "2C112948-FF4C-46E7-981A-0257681EADF4" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteSecurityAuth( "2C112948-FF4C-46E7-981A-0257681EADF4" );
            DeleteSecurityAuth( "89F449C0-4FDD-488F-AF9B-088175777FB2" );
            DeleteSecurityRoleGroup( "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B" );

            // UnMove PageRoutes to Tools Menu
            MovePage( "4A833BE3-7D5E-4C38-AF60-5706260015EA", "B4A24AB7-9369-4055-883F-4F4892C39AE3" );
            Sql( "Update [Page] set [Name] = 'Page Routes', [Title] = 'Page Routes', [Order]='99' where [Guid] = '4A833BE3-7D5E-4C38-AF60-5706260015EA'" );

            // UnMove Ad/Marketing Campaigns to Tools menu
            MovePage( "74345663-5BCA-493C-A2FB-80DC9CC8E70C", "B4A24AB7-9369-4055-883F-4F4892C39AE3" );
            Sql( "Update [Page] set [Name] = 'Marketing Campaigns', [Title] = 'Marketing Campaigns', [Order]='99' where [Guid] = '74345663-5BCA-493C-A2FB-80DC9CC8E70C'" );

            DeletePage( "0172B025-3CD9-4DA4-9215-E965D5A7EA63" );
            DeletePage( "9DF95EFF-88B4-401A-8F5F-E3B8DB02A308" );
            DeletePage( "9C23D9F1-0734-4CD6-A35A-B8F28C3C674C" );
            DeletePage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F" );
            DeletePage( "98163C8B-5C91-4A68-BB79-6AD948A604CE" );
        }
    }
}

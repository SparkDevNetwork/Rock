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
    public partial class MarketingCampaignAdAttributes : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
INSERT INTO [coreFieldType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[Assembly]
           ,[Class]
           ,[Guid])
     VALUES
           (1
           ,'Html'
           ,'An Html Field'
           ,'Rock'
           ,'Rock.Field.Types.HtmlField'
           ,'DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF')
" );
    
            AddEntityAttribute( new Rock.Cms.MarketingCampaignAd().TypeName, "9C204CD0-1233-41C5-818A-C5DA439445AA", "Summary Text", "", "", 0, "", "763175ED-A67F-415E-8843-D2C50EF12B33" );
            AddEntityAttribute( new Rock.Cms.MarketingCampaignAd().TypeName, "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "Detail Html", "", "", 0, "", "58C196E9-A731-40BB-A015-8A63C98B6733" );
            AddEntityAttribute( new Rock.Cms.MarketingCampaignAd().TypeName, "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Summary Image", "", "", 0, "", "F090D30A-5BCA-4A73-A42F-54744E78A8F2" );
            AddEntityAttribute( new Rock.Cms.MarketingCampaignAd().TypeName, "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Detail Image", "", "", 0, "", "C5C19024-25EE-4A9A-9DCC-BFB148C84B8E" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "763175ED-A67F-415E-8843-D2C50EF12B33" );
            DeleteAttribute( "58C196E9-A731-40BB-A015-8A63C98B6733" );
            DeleteAttribute( "F090D30A-5BCA-4A73-A42F-54744E78A8F2" );
            DeleteAttribute( "C5C19024-25EE-4A9A-9DCC-BFB148C84B8E" );

            Sql( "DELETE FROM [coreFieldType] WHERE [Guid] = 'DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF'" );
        }
    }
}

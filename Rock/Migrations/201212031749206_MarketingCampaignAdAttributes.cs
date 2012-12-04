//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Rock.Model;

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
            // MarketingAdType nonspecific attributes
            DeleteAttribute( "763175ED-A67F-415E-8843-D2C50EF12B33" );
            DeleteAttribute( "58C196E9-A731-40BB-A015-8A63C98B6733" );
            DeleteAttribute( "F090D30A-5BCA-4A73-A42F-54744E78A8F2" );
            DeleteAttribute( "C5C19024-25EE-4A9A-9DCC-BFB148C84B8E" );

            // add memofield (for Summary Text, etc)
            Sql( @"
INSERT INTO [FieldType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[Assembly]
           ,[Class]
           ,[Guid])
     VALUES
           (1
           ,'Memo'
           ,'A Memo Field'
           ,'Rock'
           ,'Rock.Field.Types.MemoField'
           ,'C28C7BF3-A552-4D77-9408-DEDCF760CED0')
" );

            // create adtype specific attributes for bulletin and website adtypes
            MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
            int bulletinAdTypeId = marketingCampaignAdTypeService.Get( new Guid( "3ED7FECC-BCA3-4014-A55E-93FF0D2263C9" ) ).Id;

            AddEntityAttribute( new Rock.Model.MarketingCampaignAd().TypeName, "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "MarketingCampaignAdTypeId", 
                bulletinAdTypeId.ToString(), "Summary Text", "", "", 0, "", "D06D03C1-0FB8-488B-A087-56C204B15B3D" );

            int websiteAdTypeId = marketingCampaignAdTypeService.Get( new Guid( "851745BF-CA38-46DC-B2E4-84F4C5990F54" ) ).Id;
            AddEntityAttribute( new Rock.Model.MarketingCampaignAd().TypeName, "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "MarketingCampaignAdTypeId", 
                websiteAdTypeId.ToString(), "Summary Text", "", "", 0, "", "2D594CB6-F438-4038-8A64-8530525ABDB5" );
            
            AddEntityAttribute( new Rock.Model.MarketingCampaignAd().TypeName, "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "MarketingCampaignAdTypeId", 
                websiteAdTypeId.ToString(), "Detail Html", "", "", 0, "", "D469687D-AA31-4D67-AD49-6F027B3DC7D7" );
            
            AddEntityAttribute( new Rock.Model.MarketingCampaignAd().TypeName, "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "MarketingCampaignAdTypeId", 
                websiteAdTypeId.ToString(), "Summary Image", "", "", 0, "", "5CD67DD2-71BA-495E-B3DB-45F39FEC0173" );
            
            AddEntityAttribute( new Rock.Model.MarketingCampaignAd().TypeName, "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "MarketingCampaignAdTypeId", 
                websiteAdTypeId.ToString(), "Detail Image", "", "", 0, "", "0F0784F5-35B1-457C-94D8-9CA1EA9A5948" );

            // Delete FacebookAdType example
            Sql( "DELETE FROM [MarketingCampaignAdType] WHERE [Guid] = '4D23C6C8-7457-483C-B749-330D63F3B171'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Add FacebookAdType example back
            Sql( @"
INSERT INTO [MarketingCampaignAdType]
           ([IsSystem]
           ,[Name]
           ,[DateRangeType]
           ,[Guid])
     VALUES
           (0
           ,'Facebook'
           ,2
           ,'4D23C6C8-7457-483C-B749-330D63F3B171')" );

            // delete bulletinAdType attributes
            DeleteAttribute( "D06D03C1-0FB8-488B-A087-56C204B15B3D" );

            // delete websiteAdType attributes
            DeleteAttribute( "2D594CB6-F438-4038-8A64-8530525ABDB5" );
            DeleteAttribute( "D469687D-AA31-4D67-AD49-6F027B3DC7D7" );
            DeleteAttribute( "5CD67DD2-71BA-495E-B3DB-45F39FEC0173" );
            DeleteAttribute( "0F0784F5-35B1-457C-94D8-9CA1EA9A5948" );

            // delete MemoFieldType
            Sql( "DELETE FROM [FieldType] WHERE [Guid] = 'C28C7BF3-A552-4D77-9408-DEDCF760CED0'" );

            // Add MarketingAdType nonspecific attributes back
            AddEntityAttribute( new Rock.Model.MarketingCampaignAd().TypeName, "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Summary Text", "", "", 0, "", "763175ED-A67F-415E-8843-D2C50EF12B33" );
            AddEntityAttribute( new Rock.Model.MarketingCampaignAd().TypeName, "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "", "", "Detail Html", "", "", 0, "", "58C196E9-A731-40BB-A015-8A63C98B6733" );
            AddEntityAttribute( new Rock.Model.MarketingCampaignAd().TypeName, "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "", "", "Summary Image", "", "", 0, "", "F090D30A-5BCA-4A73-A42F-54744E78A8F2" );
            AddEntityAttribute( new Rock.Model.MarketingCampaignAd().TypeName, "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "", "", "Detail Image", "", "", 0, "", "C5C19024-25EE-4A9A-9DCC-BFB148C84B8E" );
        }
    }
}

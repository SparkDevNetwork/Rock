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
    public partial class MarketingCampaignChildBlocks : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "64521E8E-3BA7-409C-A18F-4ACAAC6758CE", "Marketing Campaign Ad Detail", "", "Default", "738006C2-692C-4402-AD32-4F729A98F8BF" );
            
            AddBlockType( "Administration - Marketing Campaign Ad Detail", "", "~/Blocks/Administration/MarketingCampaignAdDetail.ascx", "3025D1FF-8022-4E0F-8918-515D07D50335" );
            AddBlockType( "Administration - Marketing Campaign Ad List", "", "~/Blocks/Administration/MarketingCampaignAdList.ascx", "0A690902-A0A1-4AB1-AFEC-001BA5FD124B" );
            AddBlock( "64521E8E-3BA7-409C-A18F-4ACAAC6758CE", "0A690902-A0A1-4AB1-AFEC-001BA5FD124B", "Marketing Campaign Ad List", "", "Content", 1, "E12B0B16-7D2F-4E54-851B-5D9EB7C7D1A3" );
            AddBlock( "738006C2-692C-4402-AD32-4F729A98F8BF", "3025D1FF-8022-4E0F-8918-515D07D50335", "Marketing Campaign Ad Detail", "", "Content", 0, "EEA597FB-BE71-4A8E-A671-9C80856FB761" );
            AddBlockTypeAttribute( "0A690902-A0A1-4AB1-AFEC-001BA5FD124B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "0FDE1CB9-0E7D-4275-A6FF-670ABB6BE8CA" );
            // Attrib Value for Marketing Campaign Ad List:Detail Page Guid
            AddBlockAttributeValue( "E12B0B16-7D2F-4E54-851B-5D9EB7C7D1A3", "0FDE1CB9-0E7D-4275-A6FF-670ABB6BE8CA", "738006c2-692c-4402-ad32-4f729a98f8bf" );

            Sql( @"
INSERT INTO [dbo].[PageContext]([IsSystem],[PageId],[Entity],[IdParameter],[CreatedDateTime],[Guid])
     VALUES(1
           ,(select [Id] from [Page] where [Guid] = '64521E8E-3BA7-409C-A18F-4ACAAC6758CE')
           ,'Rock.Model.MarketingCampaign'
           ,'marketingCampaignId'
           ,SYSDATETIME()
           ,'0C5B5C93-BB67-43F1-9F44-D553670B1C8A')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
DELETE FROM [dbo].[PageContext] where [Guid] = 'CCC716FB-6265-43D3-8DB2-2004198DD0A3'
" );
            
            DeleteAttribute( "0FDE1CB9-0E7D-4275-A6FF-670ABB6BE8CA" );
            DeleteBlock( "E12B0B16-7D2F-4E54-851B-5D9EB7C7D1A3" );
            DeleteBlock( "EEA597FB-BE71-4A8E-A671-9C80856FB761" );
            DeleteBlockType( "3025D1FF-8022-4E0F-8918-515D07D50335" );
            DeleteBlockType( "0A690902-A0A1-4AB1-AFEC-001BA5FD124B" );
            DeletePage( "738006C2-692C-4402-AD32-4F729A98F8BF" );
        }
    }
}

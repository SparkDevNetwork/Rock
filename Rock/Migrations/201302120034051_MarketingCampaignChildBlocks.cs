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
    public partial class MarketingCampaignChildBlocks : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "64521E8E-3BA7-409C-A18F-4ACAAC6758CE", "Marketing Campaign Ad Detail", "", "Default", "738006C2-692C-4402-AD32-4F729A98F8BF" );
            
            //AddBlockType( "View Editor", "", "~/Blocks/ViewEditor.ascx", "00E31A86-17D9-4F0B-89F6-764B703A7D86" );
            //AddBlockType( "Workflow Editor", "", "~/Blocks/WorkflowEditor.ascx", "7C0E624B-9174-48A1-BB98-DB2B917B94FD" );
            AddBlockType( "Administration - Marketing Campaign Ad Detail", "", "~/Blocks/Administration/MarketingCampaignAdDetail.ascx", "3025D1FF-8022-4E0F-8918-515D07D50335" );
            AddBlockType( "Administration - Marketing Campaign Ad List", "", "~/Blocks/Administration/MarketingCampaignAdList.ascx", "0A690902-A0A1-4AB1-AFEC-001BA5FD124B" );
            /*
            AddBlockType( "Crm - Person Detail - Notes", "", "~/Blocks/Crm/PersonDetail/Notes.ascx", "1A1D2CF6-6E9C-4402-8321-00C2FA42BEC8" );
            AddBlockType( "Reporting - Dynamic Data", "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "~/Blocks/Reporting/DynamicData.ascx", "46878734-559C-4010-996C-90E729F83C37" );
            AddBlockType( "Reporting - Report Detail", "", "~/Blocks/Reporting/ReportDetail.ascx", "ACA8BAE5-06F3-4361-96FC-D35918771BA5" );
            AddBlockType( "Reporting - Report List", "", "~/Blocks/Reporting/ReportList.ascx", "F5DB3DCB-8D57-4583-B8AC-7008A9213748" );
            AddBlockType( "Utility - Sql Command", "Block to execute a sql command and display the result (if any).", "~/Blocks/Utility/SqlCommand.ascx", "D3C7505B-2DCC-4A32-9DD7-2A0F4D579191" );
             */ 
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
            
            //DeleteBlockType( "00E31A86-17D9-4F0B-89F6-764B703A7D86" );
            //DeleteBlockType( "7C0E624B-9174-48A1-BB98-DB2B917B94FD" );
            DeleteBlockType( "3025D1FF-8022-4E0F-8918-515D07D50335" );
            DeleteBlockType( "0A690902-A0A1-4AB1-AFEC-001BA5FD124B" );
            /*DeleteBlockType( "1A1D2CF6-6E9C-4402-8321-00C2FA42BEC8" );
            DeleteBlockType( "46878734-559C-4010-996C-90E729F83C37" );
            DeleteBlockType( "ACA8BAE5-06F3-4361-96FC-D35918771BA5" );
            DeleteBlockType( "F5DB3DCB-8D57-4583-B8AC-7008A9213748" );
            DeleteBlockType( "D3C7505B-2DCC-4A32-9DD7-2A0F4D579191" );
             */ 
            
            DeletePage( "738006C2-692C-4402-AD32-4F729A98F8BF" );
        }
    }
}

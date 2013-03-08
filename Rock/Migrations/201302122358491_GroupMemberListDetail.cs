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
    public partial class GroupMemberListDetail : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "4E237286-B715-4109-A578-C1445EC02707", "Group Member Detail", "", "Default", "3905C63F-4D57-40F0-9721-C60A2F681911" );
            
            AddBlockType( "Crm - Group Member Detail", "", "~/Blocks/Crm/GroupMemberDetail.ascx", "AAE2E5C3-9279-4AB0-9682-F4D19519D678" );
            AddBlockType( "Crm - Group Member List", "", "~/Blocks/Crm/GroupMemberList.ascx", "88B7EFA9-7419-4D05-9F88-38B936E61EDD" );
            AddBlock( "4E237286-B715-4109-A578-C1445EC02707", "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "Group Member List", "", "RightContent", 1, "BA0F3E7D-1C3A-47CB-9058-893DBAA35B89" );
            AddBlock( "3905C63F-4D57-40F0-9721-C60A2F681911", "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "Group Member Detail", "", "Content", 0, "C66D11C8-DA55-40EA-925C-C9D7AC71F879" );
            AddBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "E4CCB79C-479F-4BEE-8156-969B2CE05973" );
            // Attrib Value for Group Member List:Detail Page Guid
            AddBlockAttributeValue( "BA0F3E7D-1C3A-47CB-9058-893DBAA35B89", "E4CCB79C-479F-4BEE-8156-969B2CE05973", "3905c63f-4d57-40f0-9721-c60a2f681911" );
            
            Sql( @"
INSERT INTO [dbo].[PageContext]([IsSystem],[PageId],[Entity],[IdParameter],[CreatedDateTime],[Guid])
     VALUES(1
           ,(select [Id] from [Page] where [Guid] = '4E237286-B715-4109-A578-C1445EC02707')
           ,'Rock.Model.Group'
           ,'groupId'
           ,SYSDATETIME()
           ,'ACEE06A6-ECA9-4342-8606-122C69DBADBE')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [dbo].[PageContext] where [Guid] = '093C802C-593B-4E86-9684-B4668F7E8D2F'" );

            DeleteAttribute( "E4CCB79C-479F-4BEE-8156-969B2CE05973" );
            DeleteBlock( "BA0F3E7D-1C3A-47CB-9058-893DBAA35B89" );
            DeleteBlock( "C66D11C8-DA55-40EA-925C-C9D7AC71F879" );
            
            DeleteBlockType( "AAE2E5C3-9279-4AB0-9682-F4D19519D678" );
            DeleteBlockType( "88B7EFA9-7419-4D05-9F88-38B936E61EDD" );
            DeletePage( "3905C63F-4D57-40F0-9721-C60A2F681911" );
        }
    }
}

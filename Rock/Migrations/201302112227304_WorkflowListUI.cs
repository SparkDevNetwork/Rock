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
    public partial class WorkflowListUI : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Administration - Workflow List", "", "~/Blocks/Administration/WorkflowList.ascx", "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B" );
            AddBlock( "DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A", "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B", "Workflow List", "", "RightContent", 2, "B8DE3F9A-5C84-4D13-8B90-F1E768740269" );
            AddBlockTypeAttribute( "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "C0BA339B-10C5-4609-B806-D192C733FFF1" );

            Sql( @"
INSERT INTO [dbo].[PageContext]([IsSystem],[PageId],[Entity],[IdParameter],[CreatedDateTime],[Guid])
     VALUES(1
           ,(select [Id] from [Page] where [Guid] = 'DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A')
           ,'Rock.Model.WorkflowType'
           ,'WorkflowTypeId'
           ,SYSDATETIME()
           ,'468E7694-3D0E-415C-ADB9-7A02ECE52E1C')
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
DELETE FROM [dbo].[PageContext] where [Guid] = '110D2D06-29A8-4B95-90D4-B370B2CBA7D7'
" );
            
            DeleteAttribute( "C0BA339B-10C5-4609-B806-D192C733FFF1" );
            DeleteBlock( "B8DE3F9A-5C84-4D13-8B90-F1E768740269" );
            DeleteBlockType( "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B" );
        }
    }
}

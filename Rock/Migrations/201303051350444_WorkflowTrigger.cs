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
    public partial class WorkflowTrigger : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Workflow Triggers", "Entity updates that will trigger a workflow", "Default", "1A233978-5BF4-4A09-9B86-6CC4C081F48B" );
            AddPage( "1A233978-5BF4-4A09-9B86-6CC4C081F48B", "Workflow Trigger", "", "Default", "04D844EA-7780-427B-8912-FA5EB7C74439" );
            AddBlockType( "Administration - Workflow Trigger Detail", "", "~/Blocks/Administration/WorkflowTriggerDetail.ascx", "5D58BF6A-3914-420C-9013-53CE8A15E390" );
            AddBlockType( "Administration - Workflow Trigger List", "", "~/Blocks/Administration/WorkflowTriggerList.ascx", "72F48121-2CE2-4696-840C-CF404EAF7EEE" );
            AddBlock( "1A233978-5BF4-4A09-9B86-6CC4C081F48B", "72F48121-2CE2-4696-840C-CF404EAF7EEE", "Trigger List", "", "Content", 0, "0A985BCA-613D-4A58-AA75-880DBB971104" );
            AddBlock( "04D844EA-7780-427B-8912-FA5EB7C74439", "5D58BF6A-3914-420C-9013-53CE8A15E390", "Workflow Trigger", "", "Content", 0, "B7D7CF7F-93BA-4463-A816-4C22DEE151B3" );
            AddBlockTypeAttribute( "72F48121-2CE2-4696-840C-CF404EAF7EEE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "9CFFB8D6-F441-47FE-AED4-7BEBE084D134" );
            // Attrib Value for Trigger List:Detail Page Guid
            AddBlockAttributeValue( "0A985BCA-613D-4A58-AA75-880DBB971104", "9CFFB8D6-F441-47FE-AED4-7BEBE084D134", "04d844ea-7780-427b-8912-fa5eb7c74439" );

            Sql( @"
UPDATE [dbo].[Page] SET
    MenuDisplayDescription = 0,
    IconCssClass = 'icon-magic'
WHERE [Guid] = '1A233978-5BF4-4A09-9B86-6CC4C081F48B'
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "9CFFB8D6-F441-47FE-AED4-7BEBE084D134" ); // Detail Page Guid
            DeleteBlock( "0A985BCA-613D-4A58-AA75-880DBB971104" ); // Trigger List
            DeleteBlock( "B7D7CF7F-93BA-4463-A816-4C22DEE151B3" ); // Workflow Trigger
            DeleteBlockType( "5D58BF6A-3914-420C-9013-53CE8A15E390" ); // Administration - Workflow Trigger Detail
            DeleteBlockType( "72F48121-2CE2-4696-840C-CF404EAF7EEE" ); // Administration - Workflow Trigger List
            DeletePage( "1A233978-5BF4-4A09-9B86-6CC4C081F48B" ); // Workflow Triggers
            DeletePage( "04D844EA-7780-427B-8912-FA5EB7C74439" ); // Workflow Trigger

        }
    }
}

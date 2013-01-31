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
    public partial class WorkflowUI01 : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "Workflows", "", "TwoColumnLeft", "DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A" );

            AddBlockType( "Administration - Workflow Type Tree View", "", "~/Blocks/Administration/WorkflowTypeTreeView.ascx", "BBC6E8B3-3CBD-4990-8C7F-C53D8A06794C" );
            AddBlockType( "Administration - Workflow Type Detail", "", "~/Blocks/Administration/WorkflowTypeDetail.ascx", "E1FF677D-5E52-4259-90C7-5560ECBBD82B" );

            AddBlock( "DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A", "BBC6E8B3-3CBD-4990-8C7F-C53D8A06794C", "Workflow Tree", "", "LeftContent", 0, "3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA" );
            AddBlock( "DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A", "E1FF677D-5E52-4259-90C7-5560ECBBD82B", "Workflow Type Detail", "", "RightContent", 0, "2C330A26-1A1C-4B36-80FA-4CB96198F985" );

            AddBlockTypeAttribute( "BBC6E8B3-3CBD-4990-8C7F-C53D8A06794C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "C67C785F-4B19-410E-80FA-E5B320D07DD5" );

            // Attrib Value for Workflow Tree:Detail Page Guid
            AddBlockAttributeValue( "3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA", "C67C785F-4B19-410E-80FA-E5B320D07DD5", "dcb18a76-6dff-48a5-a66e-2caa10d2ca1a" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "63FA25AA-7796-4302-BF05-D96A1C390BD7" );
            DeleteAttribute( "D05368C9-5069-49CD-B7E8-9CE8C46BB75D" );
            DeleteAttribute( "9D2BFE8A-41F3-4A02-B3CF-9193F0C8419E" );
            DeleteAttribute( "C67C785F-4B19-410E-80FA-E5B320D07DD5" );
            DeleteBlock( "3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA" );
            DeleteBlock( "2C330A26-1A1C-4B36-80FA-4CB96198F985" );
            DeleteBlockType( "BBC6E8B3-3CBD-4990-8C7F-C53D8A06794C" );
            DeleteBlockType( "E1FF677D-5E52-4259-90C7-5560ECBBD82B" );
            DeletePage( "DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A" );
        }
    }
}
